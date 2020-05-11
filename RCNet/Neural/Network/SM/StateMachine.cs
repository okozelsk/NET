using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Linq;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Readout;
using RCNet.Neural.Network.SM.PM;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;

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
        /// Configuration
        /// </summary>
        public StateMachineSettings Config { get; }

        /// <summary>
        /// Neural preprocessor.
        /// </summary>
        public NeuralPreprocessor NP { get; private set; }

        /// <summary>
        /// Readout layer.
        /// </summary>
        public ReadoutLayer RL { get; private set; }

        //Constructors
        /// <summary>
        /// Creates an instance of StateMachine
        /// </summary>
        /// <param name="settings">StateMachine configuration</param>
        public StateMachine(StateMachineSettings settings)
        {
            Config = (StateMachineSettings)settings.DeepClone();
            //Neural preprocessor instance
            NP = Config.NeuralPreprocessorCfg == null ? null : new NeuralPreprocessor(Config.NeuralPreprocessorCfg, Config.RandomizerSeek);
            //Readout layer instance
            RL = new ReadoutLayer(Config.ReadoutLayerCfg);
            return;
        }

        /// <summary>
        /// Creates an instance of StateMachine
        /// </summary>
        /// <param name="settingsXmlFile">Xml file where root element matches StateMachine settings</param>
        public StateMachine(string settingsXmlFile)
        {
            XDocument xmlDoc = XDocument.Load(settingsXmlFile);
            Config = new StateMachineSettings(xmlDoc.Root);
            //Neural preprocessor instance
            NP = Config.NeuralPreprocessorCfg == null ? null : new NeuralPreprocessor(Config.NeuralPreprocessorCfg, Config.RandomizerSeek);
            //Readout layer instance
            RL = new ReadoutLayer(Config.ReadoutLayerCfg);
            return;
        }

        //Static methods
        /// <summary>
        /// Deserializes StateMachine instance from the given stream
        /// </summary>
        /// <param name="stream">Stream to be used</param>
        /// <returns>Instance of the StateMachine</returns>
        public static StateMachine Deserialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (StateMachine)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Deserializes StateMachine instance from the given file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>Instance of the StateMachine</returns>
        public static StateMachine LoadFromFile(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                return Deserialize(stream);
            }
        }

        //Methods
        /// <summary>
        /// Serializes this instance into the given stream
        /// </summary>
        /// <param name="stream">Stream to be used</param>
        public void Serialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            return;
        }

        /// <summary>
        /// Serializes this instance into the given file
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        public void SaveToFile(string fileName)
        {
            using (Stream stream = File.Create(fileName))
            {
                Serialize(stream);
            }
            return;
        }



        /// <summary>
        /// Sets StateMachine internal state to its initial state
        /// </summary>
        public void Reset()
        {
            //Neural preprocessor reset
            NP?.Reset();
            //ReadoutLayer reset
            RL.Reset();
            return;
        }

        /// <summary>
        /// Build initialized instance of predictors mapper for readout layer
        /// </summary>
        private PredictorsMapper BuildPredictorsMapper()
        {
            if (NP == null)
            {
                //Neural preprocessor is bypassed -> no mapper
                return null;
            }
            else
            {
                //Create empty instance of the mapper
                PredictorsMapper mapper = new PredictorsMapper(NP.OutputFeatureGeneralSwitchCollection);
                if (Config.MapperCfg != null)
                {
                    //Iterate all readout units
                    foreach (string readoutUnitName in Config.ReadoutLayerCfg.OutputFieldNameCollection)
                    {
                        bool[] switches = new bool[NP.OutputFeatureGeneralSwitchCollection.Length];
                        //Initially allow all valid predictors
                        NP.OutputFeatureGeneralSwitchCollection.CopyTo(switches, 0);
                        //Exists specific mapping?
                        ReadoutUnitMapSettings unitMap = Config.MapperCfg.GetMapCfg(readoutUnitName, false);
                        if (unitMap != null)
                        {
                            //Allowed predictor types filter
                            if (unitMap.AllowedPredictorsCfg != null)
                            {
                                for (int i = 0; i < NP.OutputFeatureDescriptorCollection.Count; i++)
                                {
                                    //Disable not allowed neural predictor types
                                    if (!NP.OutputFeatureDescriptorCollection[i].IsInputFieldValue)
                                    {
                                        if (switches[i] && !unitMap.AllowedPredictorsCfg.IsAllowed((PredictorsProvider.PredictorID)NP.OutputFeatureDescriptorCollection[i].PredictorID))
                                        {
                                            switches[i] = false;
                                        }
                                    }
                                }
                            }
                            //Allowed reservoirs' origin
                            if(unitMap.AllowedPoolsCfg != null)
                            {
                                for (int i = 0; i < NP.OutputFeatureDescriptorCollection.Count; i++)
                                {
                                    if (switches[i] && !NP.OutputFeatureDescriptorCollection[i].IsInputFieldRelated)
                                    {
                                        //Disable not allowed origin
                                        string reservoirInstanceName = Config.NeuralPreprocessorCfg.ReservoirInstancesCfg.ReservoirInstanceCfgCollection[NP.OutputFeatureDescriptorCollection[i].ReservoirID].Name;
                                        ReservoirStructureSettings rss = Config.NeuralPreprocessorCfg.ReservoirStructuresCfg.GetReservoirStructureCfg(Config.NeuralPreprocessorCfg.ReservoirInstancesCfg.ReservoirInstanceCfgCollection[NP.OutputFeatureDescriptorCollection[i].ReservoirID].StructureCfgName);
                                        string poolName = rss.PoolsCfg.PoolCfgCollection[NP.OutputFeatureDescriptorCollection[i].PoolID].Name;
                                        if (!unitMap.AllowedPoolsCfg.IsAllowed(reservoirInstanceName, poolName))
                                        {
                                            switches[i] = false;
                                        }
                                    }
                                }
                            }
                            //Allowed input fields. Rejection of related predictors and exact values
                            if (unitMap.AllowedInputFieldsCfg != null)
                            {
                                string[] fieldNames = Config.NeuralPreprocessorCfg.InputEncoderCfg.FieldsCfg.GetNames().ToArray();
                                //Allowed input fields related predictors and values
                                for (int i = 0; i < NP.OutputFeatureDescriptorCollection.Count; i++)
                                {
                                    if (switches[i] && NP.OutputFeatureDescriptorCollection[i].IsInputFieldRelated)
                                    {
                                        string fieldName = fieldNames[NP.OutputFeatureDescriptorCollection[i].InputFieldID];
                                        if(!unitMap.AllowedInputFieldsCfg.IsAllowed(fieldName))
                                        {
                                            switches[i] = false;
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
        }

        /// <summary>
        /// Processes given input values and computes (predicts) the output.
        /// </summary>
        /// <param name="inputVector">Input values</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] inputVector)
        {
            if (!RL.Trained)
            {
                throw new Exception("Readout layer is not trained.");
            }
            if (NP == null)
            {
                //Neural preprocessor is bypassed
                return RL.Compute(inputVector, out _);
            }
            else
            {
                //Compute and return output
                return RL.Compute(NP.Preprocess(inputVector), out _);
            }
        }

        /// <summary>
        /// Processes given input values and computes (predicts) richer form of output.
        /// </summary>
        /// <param name="inputVector">Input values</param>
        public ReadoutLayer.ReadoutData ComputeReadoutData(double[] inputVector)
        {
            if (!RL.Trained)
            {
                throw new Exception("Readout layer is not trained.");
            }
            if (NP == null)
            {
                //Neural preprocessor is bypassed
                return RL.ComputeReadoutData(inputVector, out _);
            }
            else
            {
                //Compute and return output
                return RL.ComputeReadoutData(NP.Preprocess(inputVector), out _);
            }
        }

        /// <summary>
        /// Performs training of the StateMachine
        /// </summary>
        /// <param name="vectorBundle">Training data bundle (input vectors and desired output vectors)</param>
        /// <param name="regressionController">Optional regression controller.</param>
        /// <returns>Output of the regression stage</returns>
        public TrainingResults Train(VectorBundle vectorBundle, TrainedNetworkBuilder.RegressionControllerDelegate regressionController = null)
        {
            //StateMachine reset
            Reset();
            VectorBundle readoutInput;
            NeuralPreprocessor.PreprocessingOverview preprocessingOverview = null;
            if (NP == null)
            {
                //Neural preprocessor is bypassed
                readoutInput = vectorBundle;
            }
            else
            {
                //Neural preprocessing
                readoutInput = NP.InitializeAndPreprocessBundle(vectorBundle, out preprocessingOverview);
            }
            //Training of the readout layer 
            ReadoutLayer.RegressionOverview regressionOverview = RL.Build(readoutInput, BuildPredictorsMapper(), regressionController);
            //Return compact results
            return new TrainingResults(preprocessingOverview, regressionOverview);
        }


        /// <summary>
        /// Performs given data bundle and evaluates computed results against ideal results
        /// Raises VerificationProgressChanged event.
        /// </summary>
        /// <param name="vectorBundle">Data bundle containing input vectors and desired output vectors</param>
        /// <returns>Verification result</returns>
        public VerificationResults Verify(VectorBundle vectorBundle)
        {
            VerificationResults verificationResults = new VerificationResults(Config.ReadoutLayerCfg);
            for (int sampleIdx = 0; sampleIdx < vectorBundle.InputVectorCollection.Count; sampleIdx++)
            {
                double[] predictors;
                if (NP == null)
                {
                    //Neural preprocessor is bypassed
                    predictors = vectorBundle.InputVectorCollection[sampleIdx];
                }
                else
                {
                    //Neural preprocessing
                    predictors = NP.Preprocess(vectorBundle.InputVectorCollection[sampleIdx]);
                }
                ReadoutLayer.ReadoutData readoutData = RL.ComputeReadoutData(predictors, out List<double[]> unitsAllSubResults);
                verificationResults.Update(predictors, readoutData, vectorBundle.OutputVectorCollection[sampleIdx]);
                VerificationProgressChanged(vectorBundle.InputVectorCollection.Count, sampleIdx + 1);
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
                ReadoutLayerConfig = (ReadoutLayerSettings)readoutLayerConfig.DeepClone();
                ComputationResultBundle = new ResultBundle();
                ReadoutUnitStatCollection = new List<ReadoutUnitStat>(ReadoutLayerConfig.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count);
                for(int i = 0; i < ReadoutLayerConfig.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; i++)
                {
                    ReadoutUnitStatCollection.Add(new ReadoutUnitStat(i, ReadoutLayerConfig.ReadoutUnitsCfg.ReadoutUnitCfgCollection[i]));
                }
                OneWinnerGroupStatCollection = new List<OneWinnerGroupStat>();
                foreach (string groupName in ReadoutLayerConfig.ReadoutUnitsCfg.OneWinnerGroupCollection.Keys)
                {
                    OneWinnerGroupStatCollection.Add(new OneWinnerGroupStat(groupName, ReadoutLayerConfig.ReadoutUnitsCfg.OneWinnerGroupCollection[groupName].Members));
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
                /// Readout unit's zero-based index
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
                /// <param name="index">Readout unit's zero-based index</param>
                /// <param name="rus">Readout unit settings</param>
                public ReadoutUnitStat(int index, ReadoutUnitSettings rus)
                {
                    Name = rus.Name;
                    Index = index;
                    Task = rus.TaskCfg.Type;
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
                public OneWinnerGroupStat(string groupName, List<ReadoutUnitSettings> members)
                {
                    Name = groupName;
                    GroupErrorStat = new BasicStat();
                    ClassErrorStatCollection = new Dictionary<string, BasicStat>();
                    foreach (ReadoutUnitSettings rus in members)
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
                    double err = (winningUnitIndex == maxIdealValueIdx ? 0d : 1d);
                    GroupErrorStat.AddSampleValue(err);
                    ClassErrorStatCollection[maxIdealValueName].AddSampleValue(err);
                    return;
                }

            }//OneWinnerGroupStat

        }//VerificationResult


    }//StateMachine
}//Namespace
