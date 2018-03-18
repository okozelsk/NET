using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using OKOSW.CSVTools;
using OKOSW.Extensions;
using OKOSW.MathTools;
using OKOSW.Neural.Activation;
using OKOSW.Neural.Reservoir.Analog;

namespace OKOSW.Neural.Networks.EchoState
{
    /// <summary>
    /// Implements Echo State Network with multiple reservoirs optional feature.
    /// </summary>
    [Serializable]
    public class ESN
    {
        //Delegates
        public delegate void TestSamplesSelector(List<double[]> all_predictors, List<double[]> all_outputs, int outputIdx, int[] testSamplesIdxs);
        //Constants
        //Attributes
        private ESNSettings m_settings;
        private Random m_rand;
        private InputMemory m_inputMemory;
        private ReservoirInstanceData[] m_reservoirInstances;
        private List<ReservoirStat> m_reservoirsStatistics;
        private double[] m_unitsStates;
        private RegressionData[] m_regression;

        //Constructor
        public ESN(ESNSettings settings, int outputValuesCount)
        {
            m_settings = settings;
            m_inputMemory = new InputMemory(m_settings.InputFieldsCount, settings.InputFieldExtendedMemorySize);
            //Random object
            if (m_settings.RandomizerSeek < 0) m_rand = new Random();
            else m_rand = new Random(m_settings.RandomizerSeek);
            //Build structure
            //Reservoir instance(s)
            int reservoirUnitsOutputTotalLength = 0;
            m_reservoirInstances = new ReservoirInstanceData[m_settings.InputsToResCfgsMapping.Count];
            m_reservoirsStatistics = new List<ReservoirStat>(m_settings.InputsToResCfgsMapping.Count);
            for (int i = 0; i < m_settings.InputsToResCfgsMapping.Count; i++)
            {
                m_reservoirInstances[i] = new ReservoirInstanceData(m_settings.InputsToResCfgsMapping[i].InputFields.ToArray(),
                                                                      new AnalogReservoir(i, m_settings.InputsToResCfgsMapping[i].InputFields.Count, outputValuesCount, m_settings.InputsToResCfgsMapping[i].ReservoirCfg, m_settings.RandomizerSeek)
                                                                      );
                m_reservoirsStatistics.Add(m_reservoirInstances[i].Statistics);
                reservoirUnitsOutputTotalLength += m_reservoirInstances[i].ReservoirObj.OutputPredictorsCount;
            }
            //Reservoir(s) output states holder
            m_unitsStates = new double[reservoirUnitsOutputTotalLength];
            m_unitsStates.Populate(0);
            //Output FFNets
            m_regression = new RegressionData[outputValuesCount];
            m_regression.Populate(null);
            return;
        }

        //Properties
        public ESNSettings Settings { get { return m_settings; } }
        public List<ReservoirStat> ReservoirsStatistics { get { return m_reservoirsStatistics; } }
        public RegressionData[] Regression { get { return m_regression; } }

        //Methods

        /// <summary>
        /// Sets network internal state to initial state
        /// </summary>
        private void Reset()
        {
            m_inputMemory.Reset();
            foreach(ReservoirInstanceData reservoirInstanceData in m_reservoirInstances)
            {
                reservoirInstanceData.Reset();
            }
            m_unitsStates.Populate(0);
            return;
        }

        /// <summary>
        /// Pushes input values into the reservoirs and stores reservoirs output (predictors)
        /// </summary>
        /// <param name="inputValues">Input values</param>
        private void PushInput(double[] inputValues, bool afterBoot)
        {
            int reservoirsUnitsOutputIdx = 0;
            m_inputMemory.Update(inputValues);
            //Compute reservoir(s)
            foreach (ReservoirInstanceData resData in m_reservoirInstances)
            {
                double[] resInput = new double[resData.InputFieldIdxs.Length];
                for(int i = 0; i < resData.InputFieldIdxs.Length; i++)
                {
                    resInput[i] = m_inputMemory.GetInputValue(resData.InputFieldIdxs[i]);
                    //resInput[i] = inputValues[resData.InputFieldIdxs[i]];
                }
                double[] reservoirOutput = new double[resData.ReservoirObj.OutputPredictorsCount];
                //Compute reservoir output
                resData.ReservoirObj.Compute(resInput, reservoirOutput, afterBoot);
                //Add output to common all units output
                reservoirOutput.CopyTo(m_unitsStates, reservoirsUnitsOutputIdx);
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
            int predictorsCount = m_unitsStates.Length;
            if (m_settings.RouteInputToRegression)
            {
                predictorsCount += m_inputMemory.Size;
            }
            //FFNet input values (predictors)
            double[] predictors = new double[predictorsCount];
            m_unitsStates.CopyTo(predictors, 0);
            if (m_settings.RouteInputToRegression)
            {
                m_inputMemory.CopyTo(predictors, m_unitsStates.Length);
            }
            double[] esnOutput = new double[m_regression.Length];
            for (int i = 0; i < m_regression.Length; i++)
            {
                double[] outputValue = new double[1];
                m_regression[i].FFNet.Compute(predictors, outputValue);
                esnOutput[i] = outputValue[0];
            }
            return esnOutput;
        }

        /// <summary>
        /// Could be called before next prediction to tell the network right previous outputs
        /// </summary>
        /// <param name="rightOutputs">Last known right output</param>
        public void PushFeedback(double[] rightOutputs)
        {
            foreach (ReservoirInstanceData resData in m_reservoirInstances)
            {
                resData.ReservoirObj.SetFeedback(rightOutputs);
            }
            return;
        }

        public void SelectTestSamples_Seq(List<double[]> all_predictors, List<double[]> all_outputs, int outputIdx, int[] testSamplesIdxs)
        {
            //Sequential selection
            for (int srcIdx = all_predictors.Count - testSamplesIdxs.Length, i = 0; srcIdx < all_predictors.Count; srcIdx++, i++)
            {
                testSamplesIdxs[i] = srcIdx;
            }
            return;
        }

        public void SelectTestSamples_Rnd(List<double[]> all_predictors, List<double[]> all_outputs, int outputIdx, int[] testSamplesIdxs)
        {
            //Random selection
            int[] randIndexes = new int[all_predictors.Count];
            randIndexes.ShuffledIndices(m_rand);
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
        /// <param name="bootSamplesCount">Specifies, how many of starting items of dataSet to skip in regression phase</param>
        /// <param name="testSamplesCount">Specifies, how many samples from dataSet to use as the network test samples</param>
        /// <param name="testSamplesSelector">Function is called to select testing samples</param>
        /// <param name="RegressionController">Function is continuously called during the regression phase to give to caller control over regression progress</param>
        /// <param name="regressionControllerData">Custom object to be passed to RegressionController together with other standard arguments</param>
        /// <returns>Array of regression outputs</returns>
        public RegressionData[] Train(DataBundle dataSet,
                                 int bootSamplesCount,
                                 int testSamplesCount,
                                 TestSamplesSelector testSamplesSelector,
                                 RegressionControllerFn RegressionController = null,
                                 Object regressionControllerData = null
                                 )
        {
            RegressionData[] results = new RegressionData[m_regression.Length];
            int predictorsCount = m_unitsStates.Length;
            if(m_settings.RouteInputToRegression)
            {
                predictorsCount += m_inputMemory.Size;
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
                    m_unitsStates.CopyTo(predictors, 0);
                    if (m_settings.RouteInputToRegression)
                    {
                        //Add ESN input memory content to predictors
                        m_inputMemory.CopyTo(predictors, m_unitsStates.Length);
                    }
                    all_predictors.Add(predictors);
                    //Desired outputs
                    all_outputs.Add(dataSet.Outputs[dataSetIdx]);
                }
                PushFeedback(dataSet.Outputs[dataSetIdx]);
            }
            //Statistics
            CollectReservoirsStats();
            
            //Regressions
            for (int outputIdx = 0; outputIdx < m_regression.Length; outputIdx++)
            {
                //Testing data indexes
                int[] testIndexes = new int[testSamplesCount];
                testSamplesSelector(all_predictors, all_outputs, outputIdx, testIndexes);
                //Dividing to training and testing sets
                int trainingSeqLength = all_predictors.Count - testSamplesCount;
                double[][] trainingPredictors = new double[trainingSeqLength][];
                double[][] trainingOutputs = new double[trainingSeqLength][];
                double[][] testingPredictors = new double[testSamplesCount][];
                double[][] testingOutputs = new double[testSamplesCount][];
                int trainingSetPos = 0, testingSetPos = 0;
                for (int i = 0; i < all_predictors.Count; i++)
                {
                    if (!testIndexes.Contains(i))
                    {
                        trainingPredictors[trainingSetPos] = all_predictors[i];
                        trainingOutputs[trainingSetPos] = new double[1];
                        trainingOutputs[trainingSetPos][0] = all_outputs[i][outputIdx];
                        ++trainingSetPos;
                    }
                    else
                    {
                        testingPredictors[testingSetPos] = all_predictors[i];
                        testingOutputs[testingSetPos] = new double[1];
                        testingOutputs[testingSetPos][0] = all_outputs[i][outputIdx];
                        ++testingSetPos;
                    }
                }

                results[outputIdx] = RGS.BuildOutputFFNet(outputIdx,
                                                          m_reservoirsStatistics,
                                                          trainingPredictors,
                                                          trainingOutputs,
                                                          testingPredictors,
                                                          testingOutputs,
                                                          m_settings.ReadOutHiddenLayers,
                                                          m_settings.ReadOutHiddenLayersActivation,
                                                          m_settings.OutputNeuronActivation,
                                                          m_settings.RegressionMethod,
                                                          m_settings.RegressionMaxAttempts,
                                                          m_settings.RegressionMaxIterations,
                                                          m_settings.RegressionStopMSEValue,
                                                          m_rand,
                                                          RegressionController,
                                                          regressionControllerData
                                                          );
                //Store regression results and BasicNetwork
                m_regression[outputIdx] = results[outputIdx];
            }

            return results;
        }

        private void CollectReservoirsStats()
        {
            foreach (ReservoirInstanceData rid in m_reservoirInstances)
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
            public BasicStat NeuronsGeoAvgStatesStat { get; }
            public BasicStat NeuronsStateSpansStat { get; }

            //Constructor
            public ReservoirStat(string resID)
            {
                ResID = resID;
                NeuronsMaxAbsStatesStat = new BasicStat();
                NeuronsGeoAvgStatesStat = new BasicStat();
                NeuronsStateSpansStat = new BasicStat();
                return;
            }

            //Methods
            public void Reset()
            {
                NeuronsMaxAbsStatesStat.Reset();
                NeuronsGeoAvgStatesStat.Reset();
                NeuronsStateSpansStat.Reset();
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
                Statistics = new ReservoirStat(ReservoirObj.ID);
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
                    Statistics.NeuronsGeoAvgStatesStat.AddSampleValue(neuron.StatesStat.RootMeanSquare);
                    Statistics.NeuronsStateSpansStat.AddSampleValue(neuron.StatesStat.Span);
                }
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
            }
        }

        /// <summary>
        /// Implements input memory of ESN
        /// </summary>
        [Serializable]
        private class InputMemory
        {
            //Constants
            //Attributes
            private int m_inputValueMemorySize;
            private double[][] m_inputValuesStorage;

            //Constructor
            /// <summary>
            /// Constructs InputMemory instance
            /// </summary>
            /// <param name="inputValuesCount">ESN input values count</param>
            /// <param name="inputValueExtendedMemorySize">History length of each ESN input value</param>
            public InputMemory(int inputValuesCount, int inputValueExtendedMemorySize)
            {
                //Input memory
                m_inputValueMemorySize = 1 + inputValueExtendedMemorySize;
                m_inputValuesStorage = new double[inputValuesCount][];
                for (int i = 0; i < inputValuesCount; i++)
                {
                    double[] fieldValuesBuffer = new double[m_inputValueMemorySize];
                    fieldValuesBuffer.Populate(0);
                    m_inputValuesStorage[i] = fieldValuesBuffer;
                }
                return;
            }

            //Properties
            /// <summary>
            /// Count of ESN input values
            /// </summary>
            public int InputValuesCount { get { return m_inputValuesStorage.Length; } }
            /// <summary>
            /// Total size of memory (stored values)
            /// </summary>
            public int Size { get { return m_inputValuesStorage.Length * m_inputValueMemorySize; } }

            //Methods
            /// <summary>
            /// Resets memory
            /// </summary>
            public void Reset()
            {
                foreach(double[] values in m_inputValuesStorage)
                {
                    values.Populate(0);
                }
                return;
            }

            /// <summary>
            /// Shifts values history and adds new input values
            /// </summary>
            /// <param name="newInputValues">New input values</param>
            public void Update(double[] newInputValues)
            {
                for (int i = 0; i < newInputValues.Length; i++)
                {
                    m_inputValuesStorage[i].ShiftRight(newInputValues[i]);
                }
                return;
            }

            /// <summary>
            /// Returns recent ESN input value
            /// </summary>
            /// <param name="idx">Index of value</param>
            /// <returns></returns>
            public double GetInputValue(int idx)
            {
                return m_inputValuesStorage[idx][0];
            }

            /// <summary>
            /// Copies all stored input values including stored historical values to buffer
            /// </summary>
            /// <param name="buffer">Here will be values copied. Size of the buffer has to be GE than Size property</param>
            /// <param name="fromIdx">Start position in buffer</param>
            public void CopyTo(double[] buffer, int fromIdx = 0)
            {
                int outIdx = 0;
                for (int fieldIdx = 0; fieldIdx < m_inputValuesStorage.Length; fieldIdx++)
                {
                    for (int fieldMemoryIdx = 0; fieldMemoryIdx < m_inputValueMemorySize; fieldMemoryIdx++)
                    {
                        buffer[fromIdx + outIdx] = m_inputValuesStorage[fieldIdx][fieldMemoryIdx];
                        ++outIdx;
                    }
                }
                return;
            }

        }//InputMemory


        /// <summary>Echo State Network general settings</summary>
        [Serializable]
        public class ESNSettings
        {
            //Properties
            /// <summary>
            /// RandomizerSeek greater or equal to 0 causes the same "Random" initialization (important for parameters tuning and results comparableness).
            /// Specify randomizerSeek less than 0 to get different initialization of Random class every time (and also different results).
            /// </summary>
            public int RandomizerSeek { get; set; }
            /// <summary>Count of input fields.</summary>
            public List<string> InputFieldsNames { get; set; }
            public int InputFieldExtendedMemorySize { get; set; }
            /// <summary>List of all mappings of input field(s) to appropriate reservoir configuration</summary>
            public List<InputResCfgMap> InputsToResCfgsMapping { get; set; }
            /// <summary>If true, unmodified input values will be added as a part of the regression predictors</summary>
            public bool RouteInputToRegression { get; set; }
            /// <summary>Number of neurons in hidden layers of the read out FF network</summary>
            public List<int> ReadOutHiddenLayers { get; set; }
            /// <summary>Read out FF network hidden neurons activation function.</summary>
            public ActivationFactory.EnumActivationType ReadOutHiddenLayersActivation { get; set; }
            /// <summary>Output neuron activation function.</summary>
            public ActivationFactory.EnumActivationType OutputNeuronActivation { get; set; }
            /// <summary>Regression method (LM or RESILIENT).</summary>
            public string RegressionMethod { get; set; }
            /// <summary>Maximum number of regression attempts.</summary>
            public int RegressionMaxAttempts { get; set; }
            /// <summary>Maximum number of iterations to find output ESN weights.</summary>
            public int RegressionMaxIterations { get; set; }
            /// <summary>Regression will be stopped after the specified MSE on training dataset will be reached.</summary>
            public double RegressionStopMSEValue { get; set; }

            //Constructors
            /// <summary>Creates ESN setup parameters initialized by default values</summary>
            public ESNSettings()
            {
                //Default settings
                RandomizerSeek = 0; //Default is setting for debug
                InputFieldsNames = new List<string>();
                InputFieldExtendedMemorySize = 0;
                InputsToResCfgsMapping = new List<InputResCfgMap>();
                RouteInputToRegression = true;
                ReadOutHiddenLayers = new List<int>();
                ReadOutHiddenLayersActivation = ActivationFactory.EnumActivationType.Identity;
                OutputNeuronActivation = ActivationFactory.EnumActivationType.Identity; //Standard
                RegressionMethod = "RESILIENT";
                RegressionMaxAttempts = 1; //Standard
                RegressionMaxIterations = 100; //Usually enough value is 100
                RegressionStopMSEValue = 1E-15; //Usually does not make sense to continue regression after reaching MSE 1E-15
                return;
            }

            /// <summary>Creates ESN setup parameters initialized as a values copy of specified source ESN settings</summary>
            public ESNSettings(ESNSettings source)
            {
                //Copy
                RandomizerSeek = source.RandomizerSeek;
                InputFieldsNames = source.InputFieldsNames;
                InputFieldExtendedMemorySize = source.InputFieldExtendedMemorySize;
                InputsToResCfgsMapping = new List<InputResCfgMap>(source.InputsToResCfgsMapping.Count);
                foreach (InputResCfgMap mapping in source.InputsToResCfgsMapping)
                {
                    InputsToResCfgsMapping.Add(mapping.Clone());
                }
                RouteInputToRegression = source.RouteInputToRegression;
                ReadOutHiddenLayers = new List<int>(source.ReadOutHiddenLayers);
                ReadOutHiddenLayersActivation = source.ReadOutHiddenLayersActivation;
                OutputNeuronActivation = source.OutputNeuronActivation;
                RegressionMethod = source.RegressionMethod;
                RegressionMaxAttempts = source.RegressionMaxAttempts;
                RegressionMaxIterations = source.RegressionMaxIterations;
                RegressionStopMSEValue = source.RegressionStopMSEValue;
                return;
            }

            /// <summary>Creates ESN setup parameters initialized from XML</summary>
            public ESNSettings(XmlNode xmlNode)
            {
                RandomizerSeek = int.Parse(xmlNode.Attributes["RandomizerSeek"].Value);
                RouteInputToRegression = bool.Parse(xmlNode.Attributes["RouteInputToRegression"].Value);
                ReadOutHiddenLayers = new List<int>();
                DelimitedStringValues hls = new DelimitedStringValues(DelimitedStringValues.CSV_DELIMITER);
                hls.LoadFromString(xmlNode.Attributes["ReadOutHiddenLayersStructure"].Value);
                foreach (string neuronsCount in hls.Values)
                {
                    ReadOutHiddenLayers.Add(int.Parse(neuronsCount));
                }
                ReadOutHiddenLayersActivation = ActivationFactory.ParseActivation(xmlNode.Attributes["ReadOutHiddenLayersActivation"].Value);
                OutputNeuronActivation = ActivationFactory.ParseActivation(xmlNode.Attributes["OutputNeuronActivation"].Value);
                RegressionMethod = xmlNode.Attributes["RegressionMethod"].Value.ToUpper();
                RegressionMaxAttempts = int.Parse(xmlNode.Attributes["RegressionMaxAttempts"].Value);
                RegressionMaxIterations = int.Parse(xmlNode.Attributes["RegressionMaxIterations"].Value);
                RegressionStopMSEValue = double.Parse(xmlNode.Attributes["RegressionStopMSEValue"].Value);

                //Reservoirs x Inputs setup
                InputFieldExtendedMemorySize = int.Parse(xmlNode.Attributes["InputFieldExtendedMemorySize"].Value);
                DelimitedStringValues allInputFields = new DelimitedStringValues(DelimitedStringValues.CSV_DELIMITER);
                allInputFields.LoadFromString(xmlNode.Attributes["InputFields"].Value);
                InputFieldsNames = allInputFields.Values;
                InputsToResCfgsMapping = new List<InputResCfgMap>();
                foreach (XmlNode resCfgXml in xmlNode.SelectNodes("ReservoirConfig"))
                {
                    AnalogReservoir.ReservoirConfig resCfg = new AnalogReservoir.ReservoirConfig(resCfgXml);
                    DelimitedStringValues resFields = new DelimitedStringValues(DelimitedStringValues.CSV_DELIMITER);
                    resFields.LoadFromString(resCfgXml.Attributes["ApplyToInputFields"].Value);
                    List<int> resFieldsIdxs = new List<int>();
                    foreach (string fieldName in resFields.Values)
                    {
                        int fieldIdx = allInputFields.FindValueIndex(fieldName);
                        if (fieldIdx == -1)
                        {
                            throw new ApplicationException("Reservoir configuration " + resCfg.CfgName + ": unknown input field name " + fieldName);
                        }
                        resFieldsIdxs.Add(fieldIdx);
                    }
                    bool separateInputs = bool.Parse(resCfgXml.Attributes["SeparateInputs"].Value);
                    if (!separateInputs)
                    {
                        //All specified input fields will be mixed into the one reservoir
                        InputResCfgMap mapping = new InputResCfgMap(resFieldsIdxs, resCfg);
                        InputsToResCfgsMapping.Add(mapping);
                    }
                    else
                    {
                        //Each specified input field will have its own reservoir instance
                        foreach (int inpFieldIdx in resFieldsIdxs)
                        {
                            List<int> singleInpIdx = new List<int>();
                            singleInpIdx.Add(inpFieldIdx);
                            InputResCfgMap mapping = new InputResCfgMap(singleInpIdx, resCfg);
                            InputsToResCfgsMapping.Add(mapping);
                        }
                    }
                }
                return;
            }

            //Properties
            /// <summary>Count of input fields.</summary>
            public int InputFieldsCount { get { return InputFieldsNames.Count; } }


            //Methods
            /// <summary>Checkes if this settings are equivalent to specified settings</summary>
            /// <param name="cmpSettings">Settings to be compared with this settings</param>
            public bool IsEquivalent(ESNSettings cmpSettings)
            {
                if (RandomizerSeek != cmpSettings.RandomizerSeek ||
                   !InputFieldsNames.ToArray().EqualValues(cmpSettings.InputFieldsNames.ToArray()) ||
                   InputFieldExtendedMemorySize != cmpSettings.InputFieldExtendedMemorySize ||
                   InputsToResCfgsMapping.Count != cmpSettings.InputsToResCfgsMapping.Count ||
                   RouteInputToRegression != cmpSettings.RouteInputToRegression ||
                   OutputNeuronActivation != cmpSettings.OutputNeuronActivation ||
                   RegressionMethod != cmpSettings.RegressionMethod ||
                   RegressionMaxAttempts != cmpSettings.RegressionMaxAttempts ||
                   RegressionMaxIterations != cmpSettings.RegressionMaxIterations ||
                   RegressionStopMSEValue != cmpSettings.RegressionStopMSEValue ||
                   ReadOutHiddenLayers.Count != cmpSettings.ReadOutHiddenLayers.Count ||
                   ReadOutHiddenLayersActivation != cmpSettings.ReadOutHiddenLayersActivation
                   )
                {
                    return false;
                }
                for (int i = 0; i < InputsToResCfgsMapping.Count; i++)
                {
                    if (!InputsToResCfgsMapping[i].IsEquivalent(cmpSettings.InputsToResCfgsMapping[i]))
                    {
                        return false;
                    }
                }
                for (int i = 0; i < ReadOutHiddenLayers.Count; i++)
                {
                    if (ReadOutHiddenLayers[i] != cmpSettings.ReadOutHiddenLayers[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            /// <summary>
            /// Method returns the new instance of this instance as a copy.
            /// </summary>
            /// <returns></returns>
            public ESNSettings Clone()
            {
                ESNSettings clone = new ESNSettings(this);
                return clone;
            }


            //Inner classes
            /// <summary>
            /// Mapping of ESN input field(s) to reservoir configuration
            /// </summary>
            [Serializable]
            public class InputResCfgMap
            {
                //Attributes
                public List<int> InputFields;
                public AnalogReservoir.ReservoirConfig ReservoirCfg;
                //Constructor
                public InputResCfgMap(List<int> inputFields = null, AnalogReservoir.ReservoirConfig reservoirCfg = null)
                {
                    if (inputFields == null)
                    {
                        InputFields = new List<int>();
                    }
                    else
                    {
                        InputFields = new List<int>(inputFields);
                    }
                    ReservoirCfg = reservoirCfg;
                    return;
                }
                public InputResCfgMap(InputResCfgMap source)
                {
                    InputFields = new List<int>(source.InputFields);
                    ReservoirCfg = new AnalogReservoir.ReservoirConfig(source.ReservoirCfg);
                    return;
                }
                //Methods
                public InputResCfgMap Clone()
                {
                    return new InputResCfgMap(this);
                }
                public bool IsEquivalent(InputResCfgMap cmpSettings)
                {
                    if (InputFields.Count != cmpSettings.InputFields.Count ||
                       !ReservoirCfg.IsEquivalent(cmpSettings.ReservoirCfg)
                        )
                    {
                        return false;
                    }
                    for (int i = 0; i < InputFields.Count; i++)
                    {
                        if (InputFields[i] != cmpSettings.InputFields[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }



        }//ESNSettings

    }//ESN
}
