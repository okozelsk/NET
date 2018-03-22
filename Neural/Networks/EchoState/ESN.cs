using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;
using OKOSW.MathTools;
using OKOSW.Neural.Reservoir.Analog;

namespace OKOSW.Neural.Networks.EchoState
{
    /// <summary>
    /// Implements Echo State Network
    /// </summary>
    [Serializable]
    public class ESN
    {
        //Delegates
        /// <summary>
        /// Selects testing samples from all_ samples.
        /// </summary>
        /// <param name="all_predictors">All sample predictors</param>
        /// <param name="all_outputs">All sample desired outputs</param>
        /// <param name="outputIdx">Index of ESN output value for which will be network trained.</param>
        /// <param name="testSamplesIdxs">Array to be filled with selected test samples indexes (function output).</param>
        public delegate void ESNTestSamplesSelectorCallbackDelegate(List<double[]> all_predictors, List<double[]> all_outputs, int outputIdx, int[] testSamplesIdxs);
        //Constants
        //Attributes
        private ESNSettings _settings;
        private Random _rand;
        private ReservoirInstanceData[] _reservoirInstances;
        private List<ReservoirStat> _reservoirsStatistics;
        private double[] _unitsStates;
        private RegressionData[] _regression;

        //Constructor
        /// <summary>
        /// Constructs an instance of Echo State Network
        /// </summary>
        /// <param name="settings">Echo State Network settings</param>
        /// <param name="outputValuesCount">For how many different values will ESN predict.</param>
        public ESN(ESNSettings settings, int outputValuesCount)
        {
            _settings = settings;
            //Random object
            if (_settings.RandomizerSeek < 0) _rand = new Random();
            else _rand = new Random(_settings.RandomizerSeek);
            //Build structure
            //Reservoir instance(s)
            int reservoirUnitsOutputTotalLength = 0;
            _reservoirInstances = new ReservoirInstanceData[_settings.InputsToResCfgsMapping.Count];
            _reservoirsStatistics = new List<ReservoirStat>(_settings.InputsToResCfgsMapping.Count);
            for (int i = 0; i < _settings.InputsToResCfgsMapping.Count; i++)
            {
                _reservoirInstances[i] = new ReservoirInstanceData(_settings.InputsToResCfgsMapping[i].InputFieldsIdxs.ToArray(),
                                                                    new AnalogReservoir(i,
                                                                                        _settings.InputsToResCfgsMapping[i].InputFieldsIdxs.Count,
                                                                                        outputValuesCount,
                                                                                        _settings.InputsToResCfgsMapping[i].ReservoirSettings,
                                                                                        _settings.RandomizerSeek
                                                                                        )
                                                                    );
                _reservoirsStatistics.Add(_reservoirInstances[i].Statistics);
                reservoirUnitsOutputTotalLength += _reservoirInstances[i].ReservoirObj.OutputPredictorsCount;
            }
            //Reservoir(s) output states holder
            _unitsStates = new double[reservoirUnitsOutputTotalLength];
            _unitsStates.Populate(0);
            //Output FFNets
            _regression = new RegressionData[outputValuesCount];
            _regression.Populate(null);
            return;
        }

        //Properties
        public ESNSettings Settings { get { return _settings; } }
        public List<ReservoirStat> ReservoirsStatistics { get { return _reservoirsStatistics; } }
        public RegressionData[] Regression { get { return _regression; } }

        //Methods

        /// <summary>
        /// Sets network internal state to initial state
        /// </summary>
        private void Reset()
        {
            foreach(ReservoirInstanceData reservoirInstanceData in _reservoirInstances)
            {
                reservoirInstanceData.Reset();
            }
            _unitsStates.Populate(0);
            return;
        }

        /// <summary>
        /// Pushes input values into the reservoirs and stores reservoirs output (predictors)
        /// </summary>
        /// <param name="inputValues">Input values</param>
        private void PushInput(double[] inputValues, bool afterBoot)
        {
            int reservoirsUnitsOutputIdx = 0;
            //Compute reservoir(s)
            foreach (ReservoirInstanceData resData in _reservoirInstances)
            {
                double[] resInput = new double[resData.InputFieldIdxs.Length];
                for(int i = 0; i < resData.InputFieldIdxs.Length; i++)
                {
                    resInput[i] = inputValues[resData.InputFieldIdxs[i]];
                }
                double[] reservoirOutput = new double[resData.ReservoirObj.OutputPredictorsCount];
                //Compute reservoir output
                resData.ReservoirObj.Compute(resInput, reservoirOutput, afterBoot);
                //Add output to common all units output
                reservoirOutput.CopyTo(_unitsStates, reservoirsUnitsOutputIdx);
                reservoirsUnitsOutputIdx += reservoirOutput.Length;
            }
            return;
        }

        /// <summary>
        /// Predicts next output values
        /// </summary>
        /// <param name="inputValues">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] PredictNext(double[] inputValues)
        {
            PushInput(inputValues, true);
            int predictorsCount = _unitsStates.Length;
            if (_settings.RouteInputToReadout)
            {
                predictorsCount += inputValues.Length;
            }
            //FFNet input values (predictors)
            double[] predictors = new double[predictorsCount];
            _unitsStates.CopyTo(predictors, 0);
            if (_settings.RouteInputToReadout)
            {
                inputValues.CopyTo(predictors, _unitsStates.Length);
            }
            double[] esnOutput = new double[_regression.Length];
            for (int i = 0; i < _regression.Length; i++)
            {
                double[] outputValue;
                outputValue = _regression[i].FFNet.Compute(predictors);
                esnOutput[i] = outputValue[0];
            }
            return esnOutput;
        }

        /// <summary>
        /// Could be called before next prediction to tell the network right previous outputs (feedback)
        /// </summary>
        /// <param name="lastRealValues">Last real values</param>
        public void PushFeedback(double[] lastRealValues)
        {
            foreach (ReservoirInstanceData resData in _reservoirInstances)
            {
                resData.ReservoirObj.SetFeedback(lastRealValues);
            }
            return;
        }

        /// <summary>
        /// Selects testing samples as a lasting sequence in all_ samples.
        /// </summary>
        /// <param name="all_predictors">All sample predictors</param>
        /// <param name="all_outputs">All sample desired outputs</param>
        /// <param name="outputIdx">Index of ESN output value for which will be network trained. Not used here.</param>
        /// <param name="testSamplesIdxs">Array to be filled with selected test samples indexes</param>
        public void SelectSequenceTestSamples(List<double[]> all_predictors, List<double[]> all_outputs, int outputIdx, int[] testSamplesIdxs)
        {
            //Sequential selection
            for (int srcIdx = all_predictors.Count - testSamplesIdxs.Length, i = 0; srcIdx < all_predictors.Count; srcIdx++, i++)
            {
                testSamplesIdxs[i] = srcIdx;
            }
            return;
        }

        /// <summary>
        /// Selects testing samples randomly from all_ samples.
        /// </summary>
        /// <param name="all_predictors">All sample predictors</param>
        /// <param name="all_outputs">All sample desired outputs</param>
        /// <param name="outputIdx">Index of ESN output value for which will be network trained. Not used here.</param>
        /// <param name="testSamplesIdxs">Array to be filled with selected test samples indexes</param>
        public void SelectRandomTestSamples(List<double[]> all_predictors, List<double[]> all_outputs, int outputIdx, int[] testSamplesIdxs)
        {
            //Random selection
            int[] randIndexes = new int[all_predictors.Count];
            randIndexes.ShuffledIndices(_rand);
            for (int i = 0; i < testSamplesIdxs.Length; i++)
            {
                testSamplesIdxs[i] = randIndexes[i];
            }
            return;
        }

        /// <summary>
        /// Trains network (computes output weights).
        /// </summary>
        /// <param name="dataSet">Bundle containing all known input and desired output samples (in time order)</param>
        /// <param name="bootSamplesCount">Specifies, how many of starting items of dataSet will be used for booting of reservoirs (to ensure reservoir neurons states consistently corresponding to input data)</param>
        /// <param name="testSamplesCount">Specifies, how many samples from dataSet to use as the network test samples</param>
        /// <param name="testSamplesSelector">Function to be called to select testing samples (use SelectSequenceTestSamples, SelectTestSamples_Rnd or implement your own)</param>
        /// <param name="RegressionController">Function is continuously calling back during the regression phase to give control over regression progress</param>
        /// <param name="regressionControllerData">Custom object to be unmodified passed to RegressionController together with other standard arguments</param>
        /// <returns>Array of regression outputs</returns>
        public RegressionData[] Train(DataBundle dataSet,
                                 int bootSamplesCount,
                                 int testSamplesCount,
                                 ESNTestSamplesSelectorCallbackDelegate testSamplesSelector,
                                 RGSCallbackDelegate RegressionController = null,
                                 Object regressionControllerData = null
                                 )
        {
            RegressionData[] results = new RegressionData[_regression.Length];
            int predictorsCount = _unitsStates.Length;
            if(_settings.RouteInputToReadout)
            {
                predictorsCount += _settings.InputFieldsCount;
            }
            //All predictors and outputs collection
            int dataSetLength = dataSet.Inputs.Count;
            List<double[]> all_predictors = new List<double[]>(dataSetLength - bootSamplesCount);
            List<double[]> all_outputs = new List<double[]>(dataSetLength - bootSamplesCount);
            //ESN Reset
            Reset();
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < dataSetLength; dataSetIdx++)
            {
                PushInput(dataSet.Inputs[dataSetIdx], (dataSetIdx >= bootSamplesCount));
                //Is boot sequence passed? Collect predictors?
                if (dataSetIdx >= bootSamplesCount)
                {
                    //YES
                    //Predictors
                    double[] predictors = new double[predictorsCount];
                    //Reservoir units states to predictors
                    _unitsStates.CopyTo(predictors, 0);
                    if (_settings.RouteInputToReadout)
                    {
                        //Input to predictors
                        dataSet.Inputs[dataSetIdx].CopyTo(predictors, _unitsStates.Length);
                    }
                    all_predictors.Add(predictors);
                    //Desired outputs
                    all_outputs.Add(dataSet.Outputs[dataSetIdx]);
                }
                PushFeedback(dataSet.Outputs[dataSetIdx]);
            }
            //Statistics
            CollectReservoirsStats();
            
            //Alone regression for each ESN output field
            for (int outputIdx = 0; outputIdx < _regression.Length; outputIdx++)
            {
                //Testing data indexes
                int[] testIndexes = new int[testSamplesCount];
                testSamplesSelector(all_predictors, all_outputs, outputIdx, testIndexes);
                HashSet<int> testIndexesHashSet = new HashSet<int>(testIndexes);
                //Dividing to training and testing sets
                int trainingSeqLength = all_predictors.Count - testSamplesCount;
                List<double[]> trainingPredictors = new List<double[]>(trainingSeqLength);
                List<double[]> trainingOutputs = new List<double[]>(trainingSeqLength);
                List<double[]> testingPredictors = new List<double[]>(testSamplesCount);
                List<double[]> testingOutputs = new List<double[]>(testSamplesCount);
                for (int i = 0; i < all_predictors.Count; i++)
                {
                    if (!testIndexesHashSet.Contains(i))
                    {
                        //Training sample
                        trainingPredictors.Add(all_predictors[i]);
                        trainingOutputs.Add(new double[1]);
                        trainingOutputs[trainingOutputs.Count - 1][0] = all_outputs[i][outputIdx];
                    }
                    else
                    {
                        //Testing sample
                        testingPredictors.Add(all_predictors[i]);
                        testingOutputs.Add(new double[1]);
                        testingOutputs[testingOutputs.Count - 1][0] = all_outputs[i][outputIdx];
                    }
                }
                //Regression
                results[outputIdx] = RGS.BuildOutputFFNet(outputIdx,
                                                          _reservoirsStatistics,
                                                          trainingPredictors,
                                                          trainingOutputs,
                                                          testingPredictors,
                                                          testingOutputs,
                                                          _settings.ReadOutHiddenLayers,
                                                          _settings.OutputNeuronActivation,
                                                          _settings.RegressionMethod,
                                                          _settings.RegressionMaxAttempts,
                                                          _settings.RegressionMaxEpochs,
                                                          _settings.RegressionStopMSEValue,
                                                          _rand,
                                                          RegressionController,
                                                          regressionControllerData
                                                          );
                //Store regression results and BasicNetwork
                _regression[outputIdx] = results[outputIdx];
            }

            return results;
        }

        private void CollectReservoirsStats()
        {
            foreach (ReservoirInstanceData rid in _reservoirInstances)
            {
                rid.CollectStatistics();
            }
            return;
        }

        //Inner classes
        [Serializable]
        public class ReservoirStat
        {
            //Attributes
            public string ResID { get; }
            public BasicStat NeuronsMaxAbsStatesStat { get; }
            public BasicStat NeuronsRMSStatesStat { get; }
            public BasicStat NeuronsStateSpansStat { get; }
            public double CtxNeuronStatesRMS { get; set; }

            //Constructor
            public ReservoirStat(string resID)
            {
                ResID = resID;
                NeuronsMaxAbsStatesStat = new BasicStat();
                NeuronsRMSStatesStat = new BasicStat();
                NeuronsStateSpansStat = new BasicStat();
                CtxNeuronStatesRMS = 0;
                return;
            }

            //Methods
            public void Reset()
            {
                NeuronsMaxAbsStatesStat.Reset();
                NeuronsRMSStatesStat.Reset();
                NeuronsStateSpansStat.Reset();
                CtxNeuronStatesRMS = 0;
                return;
            }
        }

        [Serializable]
        public class ReservoirInstanceData
        {
            //Attributes
            public int[] InputFieldIdxs { get; }
            public IAnalogReservoir ReservoirObj { get; }
            public ReservoirStat Statistics { get; }
            //Constructor
            public ReservoirInstanceData(int[] inputFieldIdxs, IAnalogReservoir reservoirObj)
            {
                InputFieldIdxs = inputFieldIdxs;
                ReservoirObj = reservoirObj;
                Statistics = new ReservoirStat(ReservoirObj.SeqNum);
                return;
            }

            //Methods
            public void Reset()
            {
                ReservoirObj.Reset();
                Statistics.Reset();
                return;
            }

            public void CollectStatistics()
            {
                Statistics.Reset();
                foreach (AnalogNeuron neuron in ReservoirObj.Neurons)
                {
                    Statistics.NeuronsMaxAbsStatesStat.AddSampleValue(Math.Max(Math.Abs(neuron.StatesStat.Max), Math.Abs(neuron.StatesStat.Min)));
                    Statistics.NeuronsRMSStatesStat.AddSampleValue(neuron.StatesStat.RootMeanSquare);
                    Statistics.NeuronsStateSpansStat.AddSampleValue(neuron.StatesStat.Span);
                }
                Statistics.CtxNeuronStatesRMS = ReservoirObj.ContextNeuron.StatesStat.RootMeanSquare;
                return;
            }
        }//ReservoirInstanceData

        [Serializable]
        public class DataBundle
        {
            //Attributes
            public List<double[]> Inputs { get; }
            public List<double[]> Outputs { get; }

            //Constructor
            public DataBundle(int vectorsCount)
            {
                Inputs = new List<double[]>(vectorsCount);
                Outputs = new List<double[]>(vectorsCount);
                return;
            }//DataBundle
        }

    }//ESN
}//Namespace
