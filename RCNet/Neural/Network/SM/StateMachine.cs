using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM.PM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Readout;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Implements the state machine.
    /// </summary>
    [Serializable]
    public class StateMachine
    {

        //Delegates
        /// <summary>
        /// The delegate of VerificationProgressChanged event handler.
        /// </summary>
        /// <param name="totalNumOfInputs">The total number of inputs to be processed.</param>
        /// <param name="numOfProcessedInputs">The number of already processed inputs.</param>
        public delegate void VerificationProgressChangedHandler(int totalNumOfInputs, int numOfProcessedInputs);

        //Events
        /// <summary>
        /// This informative event occurs every time the progress of verification has changed.
        /// </summary>
        [field: NonSerialized]
        public event VerificationProgressChangedHandler VerificationProgressChanged;

        //Attribute properties
        /// <summary>
        /// The configuration.
        /// </summary>
        public StateMachineSettings Config { get; }

        /// <summary>
        /// The neural preprocessor.
        /// </summary>
        public NeuralPreprocessor NP { get; private set; }

        /// <summary>
        /// The readout layer.
        /// </summary>
        public ReadoutLayer RL { get; private set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The state machine configuration.</param>
        public StateMachine(StateMachineSettings cfg)
        {
            Config = (StateMachineSettings)cfg.DeepClone();
            //Neural preprocessor instance
            NP = Config.NeuralPreprocessorCfg == null ? null : new NeuralPreprocessor(Config.NeuralPreprocessorCfg, Config.RandomizerSeek);
            //Readout layer instance
            RL = new ReadoutLayer(Config.ReadoutLayerCfg);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="xmlFileName">The name of xml file where the root element matches the state machine configuration.</param>
        public StateMachine(string xmlFileName)
        {
            XDocument xmlDoc = XDocument.Load(xmlFileName);
            Config = new StateMachineSettings(xmlDoc.Root);
            //Neural preprocessor instance
            NP = Config.NeuralPreprocessorCfg == null ? null : new NeuralPreprocessor(Config.NeuralPreprocessorCfg, Config.RandomizerSeek);
            //Readout layer instance
            RL = new ReadoutLayer(Config.ReadoutLayerCfg);
            return;
        }

        //Static methods
        /// <summary>
        /// Deserializes the state machine instance from the specified stream.
        /// </summary>
        /// <param name="stream">The stream to be used.</param>
        /// <returns>The instance of the state machine.</returns>
        public static StateMachine Deserialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (StateMachine)formatter.Deserialize(stream);
        }

        /// <summary>
        /// Deserializes the state machine instance from the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The instance of the state machine.</returns>
        public static StateMachine Deserialize(string fileName)
        {
            using (Stream stream = File.Open(fileName, FileMode.Open))
            {
                return Deserialize(stream);
            }
        }

        //Methods
        /// <summary>
        /// Serializes this instance into the specified stream.
        /// </summary>
        /// <param name="stream">The stream to be used.</param>
        public void Serialize(Stream stream)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, this);
            return;
        }

        /// <summary>
        /// Serializes this instance into the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        public void Serialize(string fileName)
        {
            using (Stream stream = File.Create(fileName))
            {
                Serialize(stream);
            }
            return;
        }

        /// <summary>
        /// Resets the state machine to its initial state.
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
        /// Builds the predictors mapper.
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
                    //Routed input field names
                    string[] routedInputFieldNames = Config.NeuralPreprocessorCfg.InputEncoderCfg.GetRoutedFieldNames().ToArray();
                    //Iterate all readout units
                    foreach (string readoutUnitName in Config.ReadoutLayerCfg.OutputFieldNameCollection)
                    {
                        bool[] switches = new bool[NP.OutputFeatureGeneralSwitchCollection.Length];
                        //Exists specific mapping?
                        ReadoutUnitMapSettings unitMap = Config.MapperCfg.GetMapCfg(readoutUnitName, false);
                        if (unitMap != null)
                        {
                            //Initially disable all predictors
                            switches.Populate(false);
                            for (int i = 0; i < NP.PredictorDescriptorCollection.Count; i++)
                            {
                                if (!NP.PredictorDescriptorCollection[i].IsInputValue)
                                {
                                    string reservoirInstanceName = Config.NeuralPreprocessorCfg.ReservoirInstancesCfg.ReservoirInstanceCfgCollection[NP.PredictorDescriptorCollection[i].ReservoirID].Name;
                                    ReservoirStructureSettings rss = Config.NeuralPreprocessorCfg.ReservoirStructuresCfg.GetReservoirStructureCfg(Config.NeuralPreprocessorCfg.ReservoirInstancesCfg.ReservoirInstanceCfgCollection[NP.PredictorDescriptorCollection[i].ReservoirID].StructureCfgName);
                                    string poolName = rss.PoolsCfg.PoolCfgCollection[NP.PredictorDescriptorCollection[i].PoolID].Name;
                                    switches[i] = unitMap.IsAllowedPredictor(reservoirInstanceName, poolName, (PredictorsProvider.PredictorID)NP.PredictorDescriptorCollection[i].PredictorID);
                                }
                                else
                                {
                                    switches[i] = unitMap.IsAllowedInputField(NP.PredictorDescriptorCollection[i].InputFieldName);
                                }
                            }
                        }
                        else
                        {
                            //Allow all valid predictors
                            NP.OutputFeatureGeneralSwitchCollection.CopyTo(switches, 0);
                        }
                        //Add mapping to mapper
                        mapper.Add(readoutUnitName, switches);
                    }
                }
                return mapper;
            }
        }

        /// <summary>
        /// Preprocesses the data and computes the readout layer.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="readoutData">The detailed data computed by the readout layer.</param>
        /// <returns>The computed output values in the natural form.</returns>
        public double[] Compute(double[] inputVector, out ReadoutLayer.ReadoutData readoutData)
        {
            if (!RL.Trained)
            {
                throw new InvalidOperationException($"Readout layer is not trained.");
            }
            if (NP == null)
            {
                //Neural preprocessor is bypassed
                return RL.Compute(inputVector, out readoutData);
            }
            else
            {
                //Compute and return output
                return RL.Compute(NP.Preprocess(inputVector), out readoutData);
            }
        }

        /// <summary>
        /// Performs the training of the state machine.
        /// </summary>
        /// <param name="trainingData">The training data bundle.</param>
        /// <param name="controller">The build process controller (optional).</param>
        /// <returns>The training results.</returns>
        public TrainingResults Train(VectorBundle trainingData, TNRNetBuilder.BuildControllerDelegate controller = null)
        {
            //StateMachine reset
            Reset();
            VectorBundle readoutTrainingData;
            NeuralPreprocessor.PreprocessingOverview preprocessingOverview = null;
            if (NP == null)
            {
                //Neural preprocessor is bypassed
                readoutTrainingData = trainingData;
            }
            else
            {
                //Neural preprocessing
                readoutTrainingData = NP.InitializeAndPreprocessBundle(trainingData, out preprocessingOverview);
            }
            //Training of the readout layer 
            ReadoutLayer.RegressionOverview regressionOverview = RL.Build(readoutTrainingData, BuildPredictorsMapper(), controller, Config.RandomizerSeek);
            //Return the training results
            return new TrainingResults(preprocessingOverview, regressionOverview);
        }

        /// <summary>
        /// Verifies the state machine's accuracy.
        /// </summary>
        /// <remarks>
        /// Evaluates the computed data against the ideal data.
        /// </remarks>
        /// <param name="verificationData">The verification data bundle.</param>
        /// <returns>The verification results.</returns>
        public VerificationResults Verify(VectorBundle verificationData)
        {
            VerificationResults verificationResults = new VerificationResults(Config.ReadoutLayerCfg);
            for (int sampleIdx = 0; sampleIdx < verificationData.InputVectorCollection.Count; sampleIdx++)
            {
                double[] predictors;
                if (NP == null)
                {
                    //Neural preprocessor is bypassed
                    predictors = verificationData.InputVectorCollection[sampleIdx];
                }
                else
                {
                    //Neural preprocessing
                    predictors = NP.Preprocess(verificationData.InputVectorCollection[sampleIdx]);
                }
                double[] outputVector = RL.Compute(predictors, out ReadoutLayer.ReadoutData readoutData);
                verificationResults.Update(predictors, readoutData, verificationData.OutputVectorCollection[sampleIdx]);
                VerificationProgressChanged(verificationData.InputVectorCollection.Count, sampleIdx + 1);
            }
            return verificationResults;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////

        //Inner classes
        /// <summary>
        /// Implements the holder of the state machine training results.
        /// </summary>
        [Serializable]
        public class TrainingResults
        {
            /// <summary>
            /// The preprocessing overview.
            /// </summary>
            public NeuralPreprocessor.PreprocessingOverview PreprocessingResults { get; }

            /// <summary>
            /// The regression overview.
            /// </summary>
            public ReadoutLayer.RegressionOverview RegressionResults { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="preprocessingResults">The preprocessing overview.</param>
            /// <param name="regressionResults">The regression overview.</param>
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
        /// Implements the holder of the verification results.
        /// </summary>
        [Serializable]
        public class VerificationResults
        {
            /// <summary>
            /// The configuration of the readout layer.
            /// </summary>
            public ReadoutLayerSettings ReadoutLayerConfig { get; }
            /// <summary>
            /// The computation result data bundle.
            /// </summary>
            public ResultBundle ComputationResultBundle { get; }
            /// <summary>
            /// The collection of the readout units statistics.
            /// </summary>
            public List<ReadoutUnitErrorStat> ReadoutUnitStatCollection { get; }
            /// <summary>
            /// The collection of "One Takes All" groups statistics.
            /// </summary>
            public List<OneTakesAllGroupErrorStat> OneTakesAllGroupStatCollection { get; }

            //Constructor
            /// <summary>
            /// Creates an uninitialized instance.
            /// </summary>
            /// <param name="readoutLayerConfig">The configuration of the readout layer.</param>
            public VerificationResults(ReadoutLayerSettings readoutLayerConfig)
            {
                ReadoutLayerConfig = (ReadoutLayerSettings)readoutLayerConfig.DeepClone();
                ComputationResultBundle = new ResultBundle();
                ReadoutUnitStatCollection = new List<ReadoutUnitErrorStat>(ReadoutLayerConfig.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count);
                for (int i = 0; i < ReadoutLayerConfig.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; i++)
                {
                    ReadoutUnitStatCollection.Add(new ReadoutUnitErrorStat(i, ReadoutLayerConfig.ReadoutUnitsCfg.ReadoutUnitCfgCollection[i]));
                }
                OneTakesAllGroupStatCollection = new List<OneTakesAllGroupErrorStat>();
                if (ReadoutLayerConfig.OneTakesAllGroupsCfg != null)
                {
                    foreach (OneTakesAllGroupSettings groupCfg in ReadoutLayerConfig.OneTakesAllGroupsCfg.OneTakesAllGroupCfgCollection)
                    {
                        int[] unitIndexes = ReadoutLayerConfig.GetOneTakesAllGroupMemberRUnitIndexes(groupCfg.Name).ToArray();
                        OneTakesAllGroupStatCollection.Add(new OneTakesAllGroupErrorStat(groupCfg.Name, unitIndexes, ReadoutLayerConfig.ReadoutUnitsCfg.ReadoutUnitCfgCollection));
                    }
                }
                return;
            }

            //Methods
            /// <summary>
            /// Updates the statistics.
            /// </summary>
            /// <param name="inputValues">The input values</param>
            /// <param name="readoutData">The computed readout data.</param>
            /// <param name="idealValues">The ideal values.</param>
            public void Update(double[] inputValues, ReadoutLayer.ReadoutData readoutData, double[] idealValues)
            {
                //Store input, computed and ideal values
                ComputationResultBundle.InputVectorCollection.Add(inputValues);
                ComputationResultBundle.ComputedVectorCollection.Add(readoutData.NatDataVector);
                ComputationResultBundle.IdealVectorCollection.Add(idealValues);
                //Update statistics
                foreach (ReadoutUnitErrorStat ruStat in ReadoutUnitStatCollection)
                {
                    ruStat.Update(readoutData.NatDataVector, idealValues);
                }
                foreach (OneTakesAllGroupErrorStat grStat in OneTakesAllGroupStatCollection)
                {
                    grStat.Update(readoutData, idealValues);
                }
                return;
            }

            /// <summary>
            /// Gets the text report.
            /// </summary>
            /// <param name="margin">Specifies the left text margin.</param>
            /// <returns>The built text report.</returns>
            public string GetReport(int margin)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                StringBuilder sb = new StringBuilder();
                //Report
                //Readout units separatelly
                foreach (ReadoutUnitErrorStat ruStat in ReadoutUnitStatCollection)
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
                //One-takes-all groups
                foreach (OneTakesAllGroupErrorStat grStat in OneTakesAllGroupStatCollection)
                {
                    sb.Append(leftMargin + $"One Takes All group [{grStat.Name}]" + Environment.NewLine);
                    foreach (OneTakesAllGroupErrorStat.MemberErrorStat memberErrStat in grStat.MemberErrorStatCollection)
                    {
                        sb.Append(leftMargin + $"  Class [{memberErrStat.UnitCfg.Name}]" + Environment.NewLine);
                        sb.Append(leftMargin + $"    Analytics" + Environment.NewLine);
                        sb.Append(leftMargin + $"      {memberErrStat.NumOfCorrectButBellowBorderSelections.ToString(CultureInfo.InvariantCulture)}x correctly selected as a winner but the probability distributed within the group is below-border." + Environment.NewLine);
                        sb.Append(leftMargin + $"      {memberErrStat.NumOfOverbeatedAboveBorderRawProbabilities.ToString(CultureInfo.InvariantCulture)}x correctly computed above-border raw probability but overbeated by the raw probability of another class." + Environment.NewLine);
                        sb.Append(leftMargin + $"    Totals" + Environment.NewLine);
                        sb.Append(leftMargin + $"      Number of samples: {memberErrStat.ErrStat.NumOfSamples}" + Environment.NewLine);
                        sb.Append(leftMargin + $"       Number of errors: {memberErrStat.ErrStat.Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"             Error rate: {memberErrStat.ErrStat.ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"               Accuracy: {(1 - memberErrStat.ErrStat.ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
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
            /// Implements the holder of the readout unit error statistics.
            /// </summary>
            [Serializable]
            public class ReadoutUnitErrorStat
            {
                /// <summary>
                /// The name of the readout unit.
                /// </summary>
                public string Name { get; }
                /// <summary>
                /// The zero-based index of the readout unit.
                /// </summary>
                public int Index { get; }
                /// <inheritdoc cref="ReadoutUnit.TaskType"/>
                public ReadoutUnit.TaskType Task { get; }
                /// <summary>
                /// The precision error statistics.
                /// </summary>
                public BasicStat ErrorStat { get; }
                /// <summary>
                /// The binary error statistics.
                /// </summary>
                public BinErrStat BinErrorStat { get; }

                //Constructor
                /// <summary>
                /// Creates an unitialized instance
                /// </summary>
                /// <param name="index">The zero-based index of the readout unit.</param>
                /// <param name="readoutUnitCfg">The configuration of the readout unit.</param>
                public ReadoutUnitErrorStat(int index, ReadoutUnitSettings readoutUnitCfg)
                {
                    Name = readoutUnitCfg.Name;
                    Index = index;
                    Task = readoutUnitCfg.TaskCfg.Type;
                    ErrorStat = new BasicStat();
                    if (Task == ReadoutUnit.TaskType.Classification)
                    {
                        BinErrorStat = new BinErrStat(0.5d);
                    }
                    return;
                }

                //Methods
                /// <summary>
                /// Updates the statistics.
                /// </summary>
                /// <param name="computedValues">The computed values.</param>
                /// <param name="idealValues">The ideal values.</param>
                public void Update(double[] computedValues, double[] idealValues)
                {
                    ErrorStat.AddSample(Math.Abs(computedValues[Index] - idealValues[Index]));
                    if (Task == ReadoutUnit.TaskType.Classification)
                    {
                        BinErrorStat.Update(computedValues[Index], idealValues[Index]);
                    }
                    return;
                }

            }//ReadoutUnitErrorStat

            /// <summary>
            /// Implements the holder of the "One Takes All" group error statistics.
            /// </summary>
            [Serializable]
            public class OneTakesAllGroupErrorStat
            {
                /// <summary>
                /// The name of the group.
                /// </summary>
                public string Name { get; }
                /// <summary>
                /// The binary error statistics.
                /// </summary>
                public BasicStat GroupErrorStat { get; }
                /// <summary>
                /// The collection of the group member error statistics.
                /// </summary>
                public List<MemberErrorStat> MemberErrorStatCollection { get; }

                //Constructor
                /// <summary>
                /// Creates an unitialized instance
                /// </summary>
                /// <param name="groupName">The name of the group.</param>
                /// <param name="unitIndexes">The member unit indexes.</param>
                /// <param name="unitCfgCollection">The collection of all readout unit configurations.</param>
                public OneTakesAllGroupErrorStat(string groupName, int[] unitIndexes, List<ReadoutUnitSettings> unitCfgCollection)
                {
                    Name = groupName;
                    GroupErrorStat = new BasicStat();
                    MemberErrorStatCollection = new List<MemberErrorStat>();
                    for (int memberIdx = 0; memberIdx < unitIndexes.Length; memberIdx++)
                    {
                        MemberErrorStatCollection.Add(new MemberErrorStat(unitCfgCollection[unitIndexes[memberIdx]], unitIndexes[memberIdx], memberIdx));
                    }
                    return;
                }

                //Methods
                /// <summary>
                /// Updates the statistics.
                /// </summary>
                /// <param name="readoutData">The computed readout data.</param>
                /// <param name="idealValues">The ideal data.</param>
                public void Update(ReadoutLayer.ReadoutData readoutData, double[] idealValues)
                {
                    ReadoutLayer.ReadoutData.OneTakesAllGroupData groupData = readoutData.GetOneTakesAllGroupData(Name);
                    //Determine correct member
                    int memberCorrectIndex = 0;
                    double idealMaxP = double.MinValue;
                    double[] memberRawProbabilities = new double[groupData.MemberReadoutUnitIndexes.Length];
                    for (int i = 0; i < groupData.MemberReadoutUnitIndexes.Length; i++)
                    {
                        memberRawProbabilities[i] = readoutData.ReadoutUnitDataCollection[groupData.MemberReadoutUnitIndexes[i]].RawNrmDataValue;
                        if (idealValues[groupData.MemberReadoutUnitIndexes[i]] > idealMaxP)
                        {
                            memberCorrectIndex = i;
                            idealMaxP = idealValues[groupData.MemberReadoutUnitIndexes[i]];
                        }
                    }

                    //Group error
                    double err = (groupData.MemberWinningGroupIndex == memberCorrectIndex ? 0d : 1d);
                    GroupErrorStat.AddSample(err);
                    //Member errors
                    MemberErrorStatCollection[memberCorrectIndex].Update(memberRawProbabilities,
                                                                         groupData.MemberProbabilities,
                                                                         groupData.MemberWinningGroupIndex
                                                                         );
                    return;
                }

                //Inner classes
                /// <summary>
                /// Implements the holder of the "One Takes All" group member error statistics.
                /// </summary>
                [Serializable]
                public class MemberErrorStat
                {
                    //Attribute properties
                    /// <summary>
                    /// The configuration of the readout unit.
                    /// </summary>
                    public ReadoutUnitSettings UnitCfg { get; }

                    /// <summary>
                    /// An index of the readout unit within the readout layer.
                    /// </summary>
                    public int UnitIndex { get; }

                    /// <summary>
                    /// An index within the "One Takes All" group.
                    /// </summary>
                    public int MemberIndex { get; }

                    /// <summary>
                    /// The precision error statistics.
                    /// </summary>
                    public BasicStat ErrStat { get; }

                    /// <summary>
                    /// The number of correct bellow-border selections.
                    /// </summary>
                    public int NumOfCorrectButBellowBorderSelections;

                    /// <summary>
                    /// The number of overbeated correct classifications.
                    /// </summary>
                    public int NumOfOverbeatedAboveBorderRawProbabilities;

                    //Constructors
                    /// <summary>
                    /// Creates an uninitialized instance.
                    /// </summary>
                    /// <param name="unitCfg">The configuration of the readout unit.</param>
                    /// <param name="unitIndex">An index of the readout unit within the readout layer.</param>
                    /// <param name="memberIndex">An index within the "One Takes All" group.</param>
                    public MemberErrorStat(ReadoutUnitSettings unitCfg, int unitIndex, int memberIndex)
                    {
                        UnitCfg = (ReadoutUnitSettings)unitCfg.DeepClone();
                        UnitIndex = unitIndex;
                        MemberIndex = memberIndex;
                        ErrStat = new BasicStat();
                        NumOfCorrectButBellowBorderSelections = 0;
                        NumOfOverbeatedAboveBorderRawProbabilities = 0;
                        return;
                    }

                    //Methods
                    /// <summary>
                    /// Updates the statistics.
                    /// </summary>
                    /// <param name="memberRawProbabilities">The raw member probabilities.</param>
                    /// <param name="memberProbabilities">The member probabilities.</param>
                    /// <param name="memberWinningIndex">The index of the winning member.</param>
                    public void Update(double[] memberRawProbabilities, double[] memberProbabilities, int memberWinningIndex)
                    {
                        //Error stat
                        double err = (MemberIndex == memberWinningIndex ? 0d : 1d);
                        ErrStat.AddSample(err);
                        //Counters
                        if (MemberIndex == memberWinningIndex)
                        {
                            if (memberProbabilities[MemberIndex] < 0d)
                            {
                                ++NumOfCorrectButBellowBorderSelections;
                            }
                        }
                        else
                        {
                            if (memberRawProbabilities[MemberIndex] >= 0d)
                            {
                                ++NumOfOverbeatedAboveBorderRawProbabilities;
                            }
                        }
                        return;
                    }

                }//MemberErrorStat

            }//OneTakesAllGroupErrorStat

        }//VerificationResult

    }//StateMachine

}//Namespace
