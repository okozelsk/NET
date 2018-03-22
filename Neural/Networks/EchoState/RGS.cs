using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.MathTools;
using OKOSW.Extensions;
using OKOSW.Neural.Activation;
using OKOSW.Neural.Networks.FF.Basic;

namespace OKOSW.Neural.Networks.EchoState
{
    public delegate RegressionControlOutArgs RGSCallbackDelegate(RegressionControlInArgs inArgs);

    /// <summary>
    /// Contains BasicNetwork and key statistics
    /// </summary>
    [Serializable]
    public class RegressionData
    {
        //Attributes
        public BasicNetwork FFNet { get; set; } = null;
        public BasicStat TrainingErrorStat { get; set; } = null;
        public BasicStat TestingErrorStat { get; set; } = null;
        public BasicStat OutputWeightsStat { get; set; } = null;
        public double CombinedError { get; set; } = -1;
        public int BestUpdatesCount { get; set; } = 0;

        //Method
        public void CopyFrom(RegressionData source)
        {
            FFNet = (BasicNetwork)source.FFNet.Clone();
            TrainingErrorStat = source.TrainingErrorStat;
            TestingErrorStat = source.TestingErrorStat;
            CombinedError = source.CombinedError;
            return;
        }
    }//RegressionData

    [Serializable]
    public class RegressionControlInArgs
    {
        public List<ESN.ReservoirStat> ReservoirsStatistics { get; set; } = null;
        public int RegrValID { get; set; } = 0;
        public int RegrAttemptNumber { get; set; } = -1;
        public int Epoch { get; set; } = -1;
        public List<double[]> TrainingPredictors { get; set; } = null;
        public List<double[]> TrainingOutputs { get; set; } = null;
        public List<double[]> TestingPredictors { get; set; } = null;
        public List<double[]> TestingOutputs { get; set; } = null;
        public RegressionData CurrRegrData { get; set; } = null;
        public RegressionData BestRegrData { get; set; } = null;
        public Object ControllerData { get; set; } = null;
    }

    [Serializable]
    public class RegressionControlOutArgs
    {
        public bool StopCurrentAttempt { get; set; } = false;
        public bool StopRegression { get; set; } = false;
        public bool Best { get; set; } = false;
    }


    /// <summary>
    /// Compute appropriate weights
    /// </summary>
    public static class RGS
    {
        /// <summary>
        /// Builds trained BasicNetwork using Levnberg Marquardt or Resilient method (ENCOG implementation)
        /// </summary>
        public static RegressionData BuildOutputFFNet(int regrValID,
                                                      List<ESN.ReservoirStat> reservoirsStatistics,
                                                      List<double[]> trainingPredictors,
                                                      List<double[]> trainingOutputs,
                                                      List<double[]> testingPredictors,
                                                      List<double[]> testingOutputs,
                                                      List<ESNSettings.ReadOutHiddenLayerCfg> readOutHiddenLayers,
                                                      ActivationFactory.ActivationType outputNeuronActivation,
                                                      string regrMethod,
                                                      int maxRegrAttempts,
                                                      int maxEpochs,
                                                      double minError,
                                                      Random rand = null,
                                                      RGSCallbackDelegate Controller = null,
                                                      Object controllerData = null
                                                      )
        {
            RegressionData bestRegrData = new RegressionData();
            //Create basic network
            BasicNetwork network = new BasicNetwork(trainingPredictors[0].Length, testingOutputs[0].Length);
            for(int i = 0; i < readOutHiddenLayers.Count; i++)
            {
                network.AddLayer(readOutHiddenLayers[i].NeuronsCount, ActivationFactory.CreateAF(readOutHiddenLayers[i].ActivationType));
            }
            network.FinalizeStructure(ActivationFactory.CreateAF(outputNeuronActivation));
            //Regression attempts
            bool stopRegression = false;
            for (int regrAttemptNumber = 1; regrAttemptNumber <= maxRegrAttempts; regrAttemptNumber++)
            {
                network.RandomizeWeights(rand);
                //Create trainer object

                IBasicTrainer trainer = null;
                switch(regrMethod.ToUpper())
                {
                    case "LINEAR":
                        trainer = new LinRegrTrainer(network, trainingPredictors, trainingOutputs, maxEpochs, rand);
                        break;
                    case "RESILIENT":
                        trainer = new RPropTrainer(network, trainingPredictors, trainingOutputs);
                        break;
                    default:
                        throw new ArgumentException("Unknown regression method " + regrMethod);
                }
                //Iterate training cycles
                for (int epoch = 1; epoch <= maxEpochs; epoch++)
                {
                    trainer.Iteration();
                    //Compute current error statistics after training iteration
                    RegressionData currRegrData = new RegressionData();
                    currRegrData.FFNet = network;
                    currRegrData.TrainingErrorStat = network.ComputeBatchErrorStat(trainingPredictors, trainingOutputs);
                    currRegrData.CombinedError = currRegrData.TrainingErrorStat.ArithAvg;
                    if (testingPredictors != null)
                    {
                        currRegrData.TestingErrorStat = network.ComputeBatchErrorStat(testingPredictors, testingOutputs);
                        currRegrData.CombinedError = Math.Max(currRegrData.CombinedError, currRegrData.TestingErrorStat.ArithAvg);
                    }
                    //Current results processing
                    bool best = false, stopTrainingCycle = false;
                    //Result first initialization
                    if (bestRegrData.CombinedError == -1)
                    {
                        //Adopt current regression results
                        bestRegrData.CopyFrom(currRegrData);
                        ++bestRegrData.BestUpdatesCount;
                    }
                    //Perform call back if it is defined
                    RegressionControlOutArgs cbOut = null;
                    if (Controller != null)
                    {
                        //Improvement evaluation is driven externaly
                        RegressionControlInArgs cbIn = new RegressionControlInArgs();
                        cbIn.ReservoirsStatistics = reservoirsStatistics;
                        cbIn.RegrValID = regrValID;
                        cbIn.RegrAttemptNumber = regrAttemptNumber;
                        cbIn.Epoch = epoch;
                        cbIn.TrainingPredictors = trainingPredictors;
                        cbIn.TrainingOutputs = trainingOutputs;
                        cbIn.TestingPredictors = testingPredictors;
                        cbIn.TestingOutputs = testingOutputs;
                        cbIn.CurrRegrData = currRegrData;
                        cbIn.BestRegrData = bestRegrData;
                        cbIn.ControllerData = controllerData;
                        cbOut = Controller(cbIn);
                        best = cbOut.Best;
                        stopTrainingCycle = cbOut.StopCurrentAttempt;
                        stopRegression = cbOut.StopRegression;
                    }
                    else
                    {
                        //Default implementation
                        if (currRegrData.CombinedError < bestRegrData.CombinedError)
                        {
                            best = true;
                        }
                    }
                    //Best?
                    if (best)
                    {
                        //Adopt current regression results
                        bestRegrData.CopyFrom(currRegrData);
                        ++bestRegrData.BestUpdatesCount;
                    }
                    //Training stop conditions
                    if (currRegrData.TrainingErrorStat.RootMeanSquare <= minError || stopTrainingCycle || stopRegression)
                    {
                        break;
                    }
                }
                //Regression stop conditions
                if (stopRegression)
                {
                    break;
                }
            }
            //Statistics of best network weights
            bestRegrData.OutputWeightsStat = bestRegrData.FFNet.ComputeWeightsStat();
            return bestRegrData;
        }

    }//RGS class
}//Namespace
