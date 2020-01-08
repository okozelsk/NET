using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.SM.Neuron;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;


namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Implements the State Machine.
    /// </summary>
    [Serializable]
    public class StateMachine
    {

        //Delegates
        /// <summary>
        /// Delegate of VerificationProgressChanged event handler.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        public delegate void VerificationProgressChangedHandler(int totalNumOfInputs, int numOfProcessedInputs);

        //Events
        /// <summary>
        /// This informative event occurs every time the progress of verification has changed
        /// </summary>
        [field: NonSerialized]
        public event VerificationProgressChangedHandler VerificationProgressChanged;

        //Attribute properties
        /// <summary>
        /// Neural preprocessor.
        /// </summary>
        public NeuralPreprocessor NP { get; private set; }

        /// <summary>
        /// Readout layer.
        /// </summary>
        public ReadoutLayer RL { get; private set; }

        //Attributes
        /// <summary>
        /// Settings used for instance creation.
        /// </summary>
        private readonly StateMachineSettings _settings;

        //Constructor
        /// <summary>
        /// Constructs an instance of State Machine
        /// </summary>
        /// <param name="settings">State Machine settings</param>
        public StateMachine(StateMachineSettings settings)
        {
            _settings = settings.DeepClone();
            //Neural preprocessor instance
            NP = new NeuralPreprocessor(settings.NeuralPreprocessorConfig, settings.RandomizerSeek);
            //Readout layer instance
            RL = new ReadoutLayer(_settings.ReadoutLayerConfig);
            return;
        }


        //Methods
        /// <summary>
        /// Sets StateMachine internal state to its initial state
        /// </summary>
        public void Reset()
        {
            //Neural preprocessor reset
            NP.Reset(true);
            //ReadoutLayer reset
            RL.Reset();
            return;
        }

        /// <summary>
        /// Build initialized instance of predictors mapper for readout layer
        /// </summary>
        private ReadoutLayer.PredictorsMapper BuildPredictorsMapper()
        {
            //Create empty instance of the mapper
            ReadoutLayer.PredictorsMapper mapper = new ReadoutLayer.PredictorsMapper(NP.PredictorGeneralSwitchCollection);
            if (_settings.MapperCfg != null)
            {
                //Expand list of predicting neurons to array of predictor origin
                StateMachineSettings.MapperSettings.AllowedPool[] neuronPoolRefCollection = new StateMachineSettings.MapperSettings.AllowedPool[NP.TotalNumOfPredictors];
                int idx = 0;
                foreach (HiddenNeuron neuron in NP.PredictorNeuronCollection)
                {
                    StateMachineSettings.MapperSettings.AllowedPool poolRef = new StateMachineSettings.MapperSettings.AllowedPool { _reservoirInstanceIdx = neuron.Placement.ReservoirID, _poolIdx = neuron.Placement.PoolID };
                    for (int i = 0; i < neuron.PredictorsCfg.NumOfEnabledPredictors; i++)
                    {
                        neuronPoolRefCollection[idx] = poolRef;
                        ++idx;
                    }
                }
                //Iterate all readout units
                foreach (string readoutUnitName in _settings.ReadoutLayerConfig.OutputFieldNameCollection)
                {
                    bool[] switches = new bool[NP.TotalNumOfPredictors];
                    //Initially allow all valid predictors
                    NP.PredictorGeneralSwitchCollection.CopyTo(switches, 0);
                    //Exists specific mapping?
                    if (_settings.MapperCfg != null && (_settings.MapperCfg.PoolsMap.ContainsKey(readoutUnitName) || _settings.MapperCfg.RoutedInputFieldsMap.ContainsKey(readoutUnitName)))
                    {
                        //Routed input fields
                        if (_settings.MapperCfg.RoutedInputFieldsMap.ContainsKey(readoutUnitName))
                        {
                            //Initially disable all routed input fields
                            for (int i = NP.PredictorNeuronCollection.Count; i < NP.TotalNumOfPredictors; i++)
                            {
                                switches[i] = false;
                            }
                            //Enable enabled routed input fields
                            List<int> enabledRoutedFieldsIdxs = _settings.MapperCfg.RoutedInputFieldsMap[readoutUnitName];
                            for (int i = 0; i < enabledRoutedFieldsIdxs.Count; i++)
                            {
                                switches[NP.PredictorNeuronCollection.Count + enabledRoutedFieldsIdxs[i]] = NP.PredictorGeneralSwitchCollection[NP.PredictorNeuronCollection.Count + enabledRoutedFieldsIdxs[i]];
                            }
                        }
                        //Neuron predictors
                        if (_settings.MapperCfg.PoolsMap.ContainsKey(readoutUnitName))
                        {
                            //Initially disable all neuron predictors
                            for (int i = 0; i < NP.PredictorNeuronCollection.Count; i++)
                            {
                                switches[i] = false;
                            }
                            //Enable allowed neuron predictors
                            foreach (StateMachineSettings.MapperSettings.AllowedPool allowedPool in _settings.MapperCfg.PoolsMap[readoutUnitName])
                            {
                                //Enable specific predictors from allowed pool (origin)
                                for (int i = 0; i < NP.PredictorNeuronCollection.Count; i++)
                                {
                                    if (neuronPoolRefCollection[i]._reservoirInstanceIdx == allowedPool._reservoirInstanceIdx && neuronPoolRefCollection[i]._poolIdx == allowedPool._poolIdx)
                                    {
                                        //Enable predictor if it is valid
                                        switches[i] = NP.PredictorGeneralSwitchCollection[i];
                                    }
                                }
                            }
                        }
                    }
                    //Add mapping to mapper
                    mapper.Add(readoutUnitName, switches);
                }
            }
            return mapper;
        }

        /// <summary>
        /// Compute function for a patterned input feeding.
        /// Processes given input pattern and computes the output.
        /// </summary>
        /// <param name="inputPattern">Input pattern</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(List<double[]> inputPattern)
        {
            if (_settings.NeuralPreprocessorConfig.InputConfig.FeedingType == NeuralPreprocessor.InputFeedingType.Continuous)
            {
                throw new Exception("This version of Compute function is not useable for continuous input feeding.");
            }
            if (!RL.Trained)
            {
                throw new Exception("Readout layer is not trained.");
            }
            //Compute and return output
            return RL.Compute(NP.Preprocess(inputPattern));
        }

        /// <summary>
        /// Compute fuction for a continuous input feeding.
        /// Processes given input values and computes (predicts) the output.
        /// </summary>
        /// <param name="inputVector">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] inputVector)
        {
            if (_settings.NeuralPreprocessorConfig.InputConfig.FeedingType == NeuralPreprocessor.InputFeedingType.Patterned)
            {
                throw new Exception("This version of Compute function is not useable for patterned input feeding.");
            }
            if (!RL.Trained)
            {
                throw new Exception("Readout layer is not trained.");
            }
            //Compute and return output
            return RL.Compute(NP.Preprocess(inputVector));
        }


        /// <summary>
        /// Performs training of the StateMachine
        /// </summary>
        /// <param name="vectorBundle">Training data bundle (input vectors and desired output vectors)</param>
        /// <param name="regressionController">Optional regression controller.</param>
        /// <returns>Output of the regression stage</returns>
        public TrainingResults Train(VectorBundle vectorBundle, ReadoutUnitBuilder.RegressionControllerDelegate regressionController = null)
        {
            //StateMachine reset
            Reset();
            //Neural preprocessing
            VectorBundle preprocessedData = NP.InitializeAndPreprocessBundle(vectorBundle, out NeuralPreprocessor.PreprocessingOverview preprocessingOverview);
            //Training of the readout layer 
            ReadoutLayer.RegressionOverview regressionOverview = RL.Build(preprocessedData, BuildPredictorsMapper(), regressionController);
            //Return compact results
            return new TrainingResults(preprocessingOverview, regressionOverview);
        }


        /// <summary>
        /// Performs training of the StateMachine
        /// </summary>
        /// <param name="patternBundle">Training data bundle (input patterns and desired output vectors)</param>
        /// <param name="regressionController">Optional regression controller.</param>
        /// <returns>Output of the regression stage</returns>
        public TrainingResults Train(PatternBundle patternBundle, ReadoutUnitBuilder.RegressionControllerDelegate regressionController = null)
        {
            //StateMachine reset
            Reset();
            //Neural preprocessing
            VectorBundle preprocessedData = NP.InitializeAndPreprocessBundle(patternBundle, out NeuralPreprocessor.PreprocessingOverview preprocessingOverview);
            //Training of the readout layer 
            ReadoutLayer.RegressionOverview regressionOverview = RL.Build(preprocessedData, BuildPredictorsMapper(), regressionController);
            //Return compact results
            return new TrainingResults(preprocessingOverview, regressionOverview);
        }

        /// <summary>
        /// Performs given data bundle and evaluates computed results against ideal results
        /// Raises VerificationProgressChanged event.
        /// </summary>
        /// <param name="vectorBundle">Data bundle containing known input vectors and desired output vectors (in time order)</param>
        /// <returns>Verification result</returns>
        public VerificationResults Verify(VectorBundle vectorBundle)
        {
            VerificationResults verificationResults = new VerificationResults(_settings.ReadoutLayerConfig);
            for (int sampleIdx = 0; sampleIdx < vectorBundle.InputVectorCollection.Count; sampleIdx++)
            {
                double[] predictors = NP.Preprocess(vectorBundle.InputVectorCollection[sampleIdx]);
                ReadoutLayer.ReadoutData readoutData = RL.ComputeReadoutData(predictors);
                double[] result = RL.Compute(predictors);
                verificationResults.Update(predictors, readoutData, vectorBundle.OutputVectorCollection[sampleIdx]);
                VerificationProgressChanged(vectorBundle.InputVectorCollection.Count, sampleIdx + 1);
            }
            return verificationResults;
        }

        /// <summary>
        /// Performs given data bundle and evaluates computed results against ideal results.
        /// Raises VerificationProgressChanged event.
        /// </summary>
        /// <param name="patternBundle">Data bundle containing known sample input patterns and desired output vectors</param>
        /// <returns>Verification result</returns>
        public VerificationResults Verify(PatternBundle patternBundle)
        {
            VerificationResults verificationResults = new VerificationResults(_settings.ReadoutLayerConfig);
            for (int sampleIdx = 0; sampleIdx < patternBundle.InputPatternCollection.Count; sampleIdx++)
            {
                double[] predictors = NP.Preprocess(patternBundle.InputPatternCollection[sampleIdx]);
                ReadoutLayer.ReadoutData readoutData = RL.ComputeReadoutData(predictors);
                double[] result = RL.Compute(predictors);
                verificationResults.Update(predictors, readoutData, patternBundle.OutputVectorCollection[sampleIdx]);
                VerificationProgressChanged(patternBundle.InputPatternCollection.Count, sampleIdx + 1);
            }
            return verificationResults;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        //Inner classes
        /// <summary>
        /// Contains results of the StateMachine training
        /// </summary>
        [Serializable]
        public class TrainingResults
        {
            /// <summary>
            /// Reservoir(s) statistics and other important information as a result of the preprocessing phase of the StateMachine training
            /// </summary>
            public NeuralPreprocessor.PreprocessingOverview PreprocessingResults { get; }

            /// <summary>
            /// Results of readout layer training (regression) phase of the StateMachine training
            /// </summary>
            public ReadoutLayer.RegressionOverview RegressionResults { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="preprocessingResults">Reservoir(s) statistics and other important information as a result of the preprocessing phase of the StateMachine training.</param>
            /// <param name="regressionResults">Results of readout layer training (regression) phase of the StateMachine training.</param>
            public TrainingResults(NeuralPreprocessor.PreprocessingOverview preprocessingResults,
                                   ReadoutLayer.RegressionOverview regressionResults
                                   )
            {
                PreprocessingResults = preprocessingResults;
                RegressionResults = regressionResults;
                return;
            }

        }//TrainingResults

        /// <summary>
        /// Summary statistics
        /// </summary>
        [Serializable]
        public class VerificationResults
        {
            /// <summary>
            /// Configuration of the readout layer
            /// </summary>
            public ReadoutLayerSettings ReadoutLayerConfig { get; }
            /// <summary>
            /// Computation result data bundle
            /// </summary>
            public ResultBundle ComputationResultBundle { get; }
            /// <summary>
            /// Error statistics of individual readout units
            /// </summary>
            public List<ReadoutUnitStat> ReadoutUnitStatCollection { get; }
            /// <summary>
            /// Error statistics of one-winner groups of readout units
            /// </summary>
            public List<OneWinnerGroupStat> OneWinnerGroupStatCollection { get; }

            //Constructor
            /// <summary>
            /// Creates initialized instance
            /// </summary>
            /// <param name="readoutLayerConfig">Configuration of the Readout Layer</param>
            public VerificationResults(ReadoutLayerSettings readoutLayerConfig)
            {
                ReadoutLayerConfig = readoutLayerConfig.DeepClone();
                ComputationResultBundle = new ResultBundle();
                ReadoutUnitStatCollection = new List<ReadoutUnitStat>(ReadoutLayerConfig.ReadoutUnitCfgCollection.Count);
                foreach (ReadoutLayerSettings.ReadoutUnitSettings rus in ReadoutLayerConfig.ReadoutUnitCfgCollection)
                {
                    ReadoutUnitStatCollection.Add(new ReadoutUnitStat(rus));
                }
                OneWinnerGroupStatCollection = new List<OneWinnerGroupStat>();
                foreach (string groupName in ReadoutLayerConfig.OneWinnerGroupNameCollection.Values)
                {
                    OneWinnerGroupStatCollection.Add(new OneWinnerGroupStat(groupName, ReadoutLayerConfig.GetOneWinnerGroupMembers(groupName)));
                }
                return;
            }

            //Methods
            /// <summary>
            /// Updates error statistics
            /// </summary>
            /// <param name="inputValues">Input values</param>
            /// <param name="readoutData">Computed readout data</param>
            /// <param name="idealValues">Ideal values</param>
            public void Update(double[] inputValues, ReadoutLayer.ReadoutData readoutData, double[] idealValues)
            {
                //Store input, computed and ideal values
                ComputationResultBundle.InputVectorCollection.Add(inputValues);
                ComputationResultBundle.ComputedVectorCollection.Add(readoutData.DataVector);
                ComputationResultBundle.IdealVectorCollection.Add(idealValues);
                //Update statistics
                foreach (ReadoutUnitStat ruStat in ReadoutUnitStatCollection)
                {
                    ruStat.Update(readoutData.DataVector, idealValues);
                }
                foreach (OneWinnerGroupStat grStat in OneWinnerGroupStatCollection)
                {
                    grStat.Update(readoutData, idealValues);
                }
                return;
            }

            /// <summary>
            /// Returns textual summary statistics
            /// </summary>
            /// <param name="margin">Specifies how many spaces should be at the begining of each row.</param>
            /// <returns>Built text report</returns>
            public string GetReport(int margin)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                StringBuilder sb = new StringBuilder();
                //Report
                //Readout units separatelly
                foreach (ReadoutUnitStat ruStat in ReadoutUnitStatCollection)
                {
                    sb.Append(leftMargin + $"Output field [{ruStat.Name}]" + Environment.NewLine);
                    if (ruStat.Task == ReadoutUnit.TaskType.Classification)
                    {
                        //Classification task report
                        sb.Append(leftMargin + $"  Classification of negative samples" + Environment.NewLine);
                        sb.Append(leftMargin + $"    Number of samples: {ruStat.BinErrorStat.BinValErrStat[0].NumOfSamples}" + Environment.NewLine);
                        sb.Append(leftMargin + $"     Number of errors: {ruStat.BinErrorStat.BinValErrStat[0].Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"           Error rate: {ruStat.BinErrorStat.BinValErrStat[0].ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"             Accuracy: {(1 - ruStat.BinErrorStat.BinValErrStat[0].ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"  Classification of positive samples" + Environment.NewLine);
                        sb.Append(leftMargin + $"    Number of samples: {ruStat.BinErrorStat.BinValErrStat[1].NumOfSamples}" + Environment.NewLine);
                        sb.Append(leftMargin + $"     Number of errors: {ruStat.BinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"           Error rate: {ruStat.BinErrorStat.BinValErrStat[1].ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"             Accuracy: {(1 - ruStat.BinErrorStat.BinValErrStat[1].ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"  Overall classification results" + Environment.NewLine);
                        sb.Append(leftMargin + $"    Number of samples: {ruStat.BinErrorStat.TotalErrStat.NumOfSamples}" + Environment.NewLine);
                        sb.Append(leftMargin + $"     Number of errors: {ruStat.BinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"           Error rate: {ruStat.BinErrorStat.TotalErrStat.ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"             Accuracy: {(1 - ruStat.BinErrorStat.TotalErrStat.ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    }
                    else
                    {
                        //Forecast task report
                        sb.Append(leftMargin + $"  Number of samples: {ruStat.ErrorStat.NumOfSamples}" + Environment.NewLine);
                        sb.Append(leftMargin + $"      Biggest error: {ruStat.ErrorStat.Max.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"     Smallest error: {ruStat.ErrorStat.Min.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"      Average error: {ruStat.ErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    }
                    sb.Append(Environment.NewLine);
                }
                //One-winner groups
                foreach (OneWinnerGroupStat grStat in OneWinnerGroupStatCollection)
                {
                    sb.Append(leftMargin + $"One winner group [{grStat.Name}]" + Environment.NewLine);
                    foreach (string className in grStat.ClassErrorStatCollection.Keys)
                    {
                        BasicStat errorStat = grStat.ClassErrorStatCollection[className];
                        sb.Append(leftMargin + $"  Class {className}" + Environment.NewLine);
                        sb.Append(leftMargin + $"    Number of samples: {errorStat.NumOfSamples}" + Environment.NewLine);
                        sb.Append(leftMargin + $"     Number of errors: {errorStat.Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"           Error rate: {errorStat.ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"             Accuracy: {(1 - errorStat.ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    }
                    sb.Append(leftMargin + $"  Group total" + Environment.NewLine);
                    sb.Append(leftMargin + $"    Number of samples: {grStat.GroupErrorStat.NumOfSamples}" + Environment.NewLine);
                    sb.Append(leftMargin + $"     Number of errors: {grStat.GroupErrorStat.Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"           Error rate: {grStat.GroupErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"             Accuracy: {(1 - grStat.GroupErrorStat.ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);

                    sb.Append(Environment.NewLine);
                }

                return sb.ToString();
            }



            //Inner classes
            /// <summary>
            /// Readout unit statistics
            /// </summary>
            [Serializable]
            public class ReadoutUnitStat
            {
                /// <summary>
                /// Readout unit name
                /// </summary>
                public string Name { get; }
                /// <summary>
                /// Readout unit zero-based index
                /// </summary>
                public int Index { get; }
                /// <summary>
                /// Neural task
                /// </summary>
                public ReadoutUnit.TaskType Task { get; }
                /// <summary>
                /// Error statistics
                /// </summary>
                public BasicStat ErrorStat { get; }
                /// <summary>
                /// Binary error statistics. Relevant only for Classification task.
                /// </summary>
                public BinErrStat BinErrorStat { get; }

                //Constructor
                /// <summary>
                /// Creates an unitialized instance
                /// </summary>
                /// <param name="rus">Readout unit settings</param>
                public ReadoutUnitStat(ReadoutLayerSettings.ReadoutUnitSettings rus)
                {
                    Name = rus.Name;
                    Index = rus.Index;
                    Task = rus.TaskType;
                    ErrorStat = new BasicStat();
                    if (Task == ReadoutUnit.TaskType.Classification)
                    {
                        BinErrorStat = new BinErrStat(0.5d);
                    }
                    return;
                }

                //Methods
                /// <summary>
                /// Updates statistics
                /// </summary>
                /// <param name="computedValues">Computed values</param>
                /// <param name="idealValues">Ideal values</param>
                public void Update(double[] computedValues, double[] idealValues)
                {
                    ErrorStat.AddSampleValue(Math.Abs(computedValues[Index] - idealValues[Index]));
                    if (Task == ReadoutUnit.TaskType.Classification)
                    {
                        BinErrorStat.Update(computedValues[Index], idealValues[Index]);
                    }
                    return;
                }

            }//ReadoutUnitStat

            /// <summary>
            /// One-winner group statistics
            /// </summary>
            [Serializable]
            public class OneWinnerGroupStat
            {
                /// <summary>
                /// Group name
                /// </summary>
                public string Name { get; }
                /// <summary>
                /// Group binary error statistics
                /// </summary>
                public BasicStat GroupErrorStat { get; }
                /// <summary>
                /// Collection of group sub-class error statistics
                /// </summary>
                public Dictionary<string, BasicStat> ClassErrorStatCollection { get; }

                //Constructor
                /// <summary>
                /// Creates an unitialized instance
                /// </summary>
                /// <param name="groupName">One-winner group name</param>
                /// <param name="members">One-winner group members</param>
                public OneWinnerGroupStat(string groupName, List<ReadoutLayerSettings.ReadoutUnitSettings> members)
                {
                    Name = groupName;
                    GroupErrorStat = new BasicStat();
                    ClassErrorStatCollection = new Dictionary<string, BasicStat>();
                    foreach (ReadoutLayerSettings.ReadoutUnitSettings rus in members)
                    {
                        ClassErrorStatCollection.Add(rus.Name, new BasicStat());
                    }
                    return;
                }

                //Methods
                /// <summary>
                /// Updates error statistics
                /// </summary>
                /// <param name="readoutData">Computed readout data</param>
                /// <param name="idealValues">Ideal values</param>
                public void Update(ReadoutLayer.ReadoutData readoutData, double[] idealValues)
                {
                    int winningUnitIndex = readoutData.OneWinnerDataCollection[Name].WinningReadoutUnitIndex;
                    int maxIdealValueIdx = -1;
                    string maxIdealValueName = string.Empty;
                    foreach (ReadoutLayer.ReadoutData.ReadoutUnitData unitData in readoutData.ReadoutUnitDataCollection.Values)
                    {
                        if (maxIdealValueIdx == -1 || idealValues[unitData.Index] > idealValues[maxIdealValueIdx])
                        {
                            maxIdealValueIdx = unitData.Index;
                            maxIdealValueName = unitData.Name;
                        }
                    }
                    double err = winningUnitIndex == maxIdealValueIdx ? 0d : 1d;
                    GroupErrorStat.AddSampleValue(err);
                    ClassErrorStatCollection[maxIdealValueName].AddSampleValue(err);
                    return;
                }

            }//OneWinnerGroupStat

        }//VerificationResult


    }//StateMachine
}//Namespace
