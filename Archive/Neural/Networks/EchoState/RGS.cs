using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Encog.ML;
using Encog.Neural.Networks.Layers;
using Encog.ML.Data;
using Encog.ML.Data.Basic;
using Encog.ML.Train;
using Encog.ML.EA.Train;
using Encog.Engine.Network.Activation;
using Encog.Neural.Pattern;
using Encog.Neural.Networks;
using Encog.Neural.Networks.Training;
using Encog.Neural.Networks.Training.Propagation.Resilient;
using Encog.Neural.Networks.Training.Lma;
using OKOSW.MathTools;
using OKOSW.Extensions;
using OKOSW.Neural.Activation;

namespace OKOSW.Neural.Networks.EchoState
{
    public delegate RegressionCallBackOutArgs RegressionControllerFn(RegressionCallBackInArgs inArgs);

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
        public void AdoptSource(RegressionData source)
        {
            FFNet = (BasicNetwork)source.FFNet.Clone();
            TrainingErrorStat = source.TrainingErrorStat;
            TestingErrorStat = source.TestingErrorStat;
            CombinedError = source.CombinedError;
            return;
        }
    }//RegressionData

    [Serializable]
    public class RegressionCallBackInArgs
    {
        public List<ESN.ReservoirStat> ReservoirsStatistics { get; set; } = null;
        public int RegrValID { get; set; } = 0;
        public int RegrAttemptNumber { get; set; } = -1;
        public int IterationNumber { get; set; } = -1;
        public double[][] TrainingPredictors { get; set; } = null;
        public double[][] TrainingOutputs { get; set; } = null;
        public double[][] TestingPredictors { get; set; } = null;
        public double[][] TestingOutputs { get; set; } = null;
        public RegressionData CurrRegrData { get; set; } = null;
        public RegressionData BestRegrData { get; set; } = null;
        public Object ControllerData { get; set; } = null;
    }

    [Serializable]
    public class RegressionCallBackOutArgs
    {
        public bool StopCurrentTrainingCycle { get; set; } = false;
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
                                                      double[][] trainingPredictors,
                                                      double[][] trainingOutputs,
                                                      double[][] testingPredictors,
                                                      double[][] testingOutputs,
                                                      List<int> readOutHiddenLayers,
                                                      ActivationFactory.EnumActivationType readOutHiddenLayersActivation,
                                                      ActivationFactory.EnumActivationType outputNeuronActivation,
                                                      string regrMethod,
                                                      int maxRegrAttempts,
                                                      int maxIterations,
                                                      double minError,
                                                      Random rand = null,
                                                      RegressionControllerFn Controller = null,
                                                      Object controllerData = null
                                                      )
        {
            RegressionData bestRegrData = new RegressionData();
            //Create basic network
            BasicNetwork network = new BasicNetwork();
            network.AddLayer(new BasicLayer(null, false, trainingPredictors[0].Length)); //Input layer has no activation
            for(int i = 0; i < readOutHiddenLayers.Count; i++)
            {
                network.AddLayer(new BasicLayer(CreateENCOGActivation(readOutHiddenLayersActivation), true, readOutHiddenLayers[i])); //Hidden layer
            }
            network.AddLayer(new BasicLayer(CreateENCOGActivation(outputNeuronActivation), true, 1)); //Output layer neuron
            network.Structure.FinalizeStructure();
            //Regression attempts
            bool stopRegression = false;
            for (int regrAttemptNumber = 1; regrAttemptNumber <= maxRegrAttempts; regrAttemptNumber++)
            {
                network.Reset();
                if (rand != null)
                {
                    rand.FillUniform(network.Structure.Flat.Weights, -1, 1, 0.1);
                }
                //Create trainer object
                IMLTrain trainer = null;
                switch (regrMethod.ToUpper())
                {
                    case "RESILIENT":
                        trainer = new ResilientPropagation(network, new BasicMLDataSet(trainingPredictors, trainingOutputs));
                        ((ResilientPropagation)trainer).RType = RPROPType.iRPROPp;
                        break;
                    case "LM":
                        trainer = new LevenbergMarquardtTraining(network, new BasicMLDataSet(trainingPredictors, trainingOutputs));
                        break;
                    default:
                        trainer = new LevenbergMarquardtTraining(network, new BasicMLDataSet(trainingPredictors, trainingOutputs));
                        break;
                }
                //Iterate training cycles
                for (int iteration = 1; iteration <= maxIterations; iteration++)
                {
                    trainer.Iteration();
                    //Compute current error statistics after training iteration
                    RegressionData currRegrData = new RegressionData();
                    currRegrData.FFNet = network;
                    currRegrData.TrainingErrorStat = ComputeErrorStat(network, trainingPredictors, trainingOutputs);
                    currRegrData.CombinedError = currRegrData.TrainingErrorStat.ArithAvg;
                    if (testingPredictors != null)
                    {
                        currRegrData.TestingErrorStat = ComputeErrorStat(network, testingPredictors, testingOutputs);
                        currRegrData.CombinedError = Math.Max(currRegrData.CombinedError, currRegrData.TestingErrorStat.ArithAvg);
                    }
                    //Current results processing
                    bool best = false, stopTrainingCycle = false;
                    //Result first initialization
                    if (bestRegrData.CombinedError == -1)
                    {
                        //Adopt current regression results
                        bestRegrData.AdoptSource(currRegrData);
                        ++bestRegrData.BestUpdatesCount;
                    }
                    //Perform call back if it is defined
                    RegressionCallBackOutArgs cbOut = null;
                    if (Controller != null)
                    {
                        //Improvement evaluation is driven externaly
                        RegressionCallBackInArgs cbIn = new RegressionCallBackInArgs();
                        cbIn.ReservoirsStatistics = reservoirsStatistics;
                        cbIn.RegrValID = regrValID;
                        cbIn.RegrAttemptNumber = regrAttemptNumber;
                        cbIn.IterationNumber = iteration;
                        cbIn.TrainingPredictors = trainingPredictors;
                        cbIn.TrainingOutputs = trainingOutputs;
                        cbIn.TestingPredictors = testingPredictors;
                        cbIn.TestingOutputs = testingOutputs;
                        cbIn.CurrRegrData = currRegrData;
                        cbIn.BestRegrData = bestRegrData;
                        cbIn.ControllerData = controllerData;
                        cbOut = Controller(cbIn);
                        best = cbOut.Best;
                        stopTrainingCycle = cbOut.StopCurrentTrainingCycle;
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
                        bestRegrData.AdoptSource(currRegrData);
                        ++bestRegrData.BestUpdatesCount;
                    }
                    //Training stop conditions
                    if (currRegrData.TrainingErrorStat.RootMeanSquare <= minError || stopTrainingCycle || stopRegression)
                    {
                        break;
                    }
                }
                trainer.FinishTraining();
                //Regression stop conditions
                if (stopRegression)
                {
                    break;
                }
            }
            //Statistics of best network weights
            bestRegrData.OutputWeightsStat = new BasicStat();
            bestRegrData.OutputWeightsStat.AddSampleValues(bestRegrData.FFNet.Flat.Weights);
            return bestRegrData;
        }

        private static BasicStat ComputeErrorStat(BasicNetwork network, double[][] predictors, double[][] outputs)
        {
            BasicStat errStat = new BasicStat();
            for (int row = 0; row < predictors.Length; row++)
            {
                double[] output = new double[1];
                network.Compute(predictors[row], output);
                errStat.AddSampleValue(Math.Abs(outputs[row][0] - output[0]));
            }
            return errStat;
        }

        private static Encog.Engine.Network.Activation.IActivationFunction CreateENCOGActivation(ActivationFactory.EnumActivationType activation)
        {
            switch (activation)
            {
                case ActivationFactory.EnumActivationType.Identity:
                    return new ActivationLinear();
                case ActivationFactory.EnumActivationType.Tanh:
                    return new ActivationTANH();
                case ActivationFactory.EnumActivationType.Sinusoid:
                    return new ActivationSIN();
                default:
                    return null;
            }
        }

    }//RGS class
}//Namespace
