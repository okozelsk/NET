using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Implements the readout layer consisting of trained readout units.
    /// </summary>
    [Serializable]
    public class ReadoutLayer
    {
        //Events
        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        [field: NonSerialized]
        public event TNRNetBuilder.EpochDoneHandler EpochDone;

        //Attribute properties
        /// <summary>
        /// Indicates whether the readout layer is trained.
        /// </summary>
        public bool Trained { get; private set; }

        /// <summary>
        /// The readout layer configuration.
        /// </summary>
        public ReadoutLayerSettings ReadoutLayerCfg { get; }

        //Attributes
        private FeatureFilterBase[] _predictorFeatureFilterCollection;
        private FeatureFilterBase[] _outputFeatureFilterCollection;
        private PredictorsMapper _predictorsMapper;
        private ReadoutUnit[] _readoutUnitCollection;
        private OneTakesAllGroup[] _oneTakesAllGroupCollection;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="readoutLayerCfg">The readout layer configuration.</param>
        public ReadoutLayer(ReadoutLayerSettings readoutLayerCfg)
        {
            ReadoutLayerCfg = (ReadoutLayerSettings)readoutLayerCfg.DeepClone();
            Reset();
            return;
        }

        //Static properties
        /// <summary>
        /// Input and output data is normalized to this range.
        /// </summary>
        private static Interval InternalDataRange { get { return Interval.IntN1P1; } }

        //Properties
        /// <summary>
        /// Gets the cloned error statistics of the readout units.
        /// </summary>
        public List<TNRNetCluster.ClusterErrStatistics> ReadoutUnitErrStatCollection
        {
            get
            {
                List<TNRNetCluster.ClusterErrStatistics> clonedStatisticsCollection = new List<TNRNetCluster.ClusterErrStatistics>(_readoutUnitCollection.Length);
                foreach (ReadoutUnit ru in _readoutUnitCollection)
                {
                    clonedStatisticsCollection.Add(ru.GetErrStat());
                }
                return clonedStatisticsCollection;
            }
        }

        //Methods
        /// <summary>
        /// Resets the readout layer to its initial untrained state.
        /// </summary>
        public void Reset()
        {
            _predictorFeatureFilterCollection = null;
            _outputFeatureFilterCollection = null;
            _predictorsMapper = null;
            _readoutUnitCollection = new ReadoutUnit[ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count];
            for (int i = 0; i < ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; i++)
            {
                ReadoutUnitSettings cfg = ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection[i];
                _readoutUnitCollection[i] = new ReadoutUnit(i, cfg, ReadoutLayerCfg.TaskDefaultsCfg);
            }
            _oneTakesAllGroupCollection = null;
            if (ReadoutLayerCfg.OneTakesAllGroupsCfg != null)
            {
                _oneTakesAllGroupCollection = new OneTakesAllGroup[ReadoutLayerCfg.OneTakesAllGroupsCfg.OneTakesAllGroupCfgCollection.Count];
                for (int i = 0; i < ReadoutLayerCfg.OneTakesAllGroupsCfg.OneTakesAllGroupCfgCollection.Count; i++)
                {
                    OneTakesAllGroupSettings cfg = ReadoutLayerCfg.OneTakesAllGroupsCfg.OneTakesAllGroupCfgCollection[i];
                    _oneTakesAllGroupCollection[i] = new OneTakesAllGroup(i, cfg, ReadoutLayerCfg.GetOneTakesAllGroupMemberRUnitIndexes(cfg.Name));
                }
            }
            Trained = false;
            return;
        }

        private void OnRegressionEpochDone(TNRNetBuilder.BuildProgress buildProgress, bool foundBetter)
        {
            //Only raise up
            EpochDone(buildProgress, foundBetter);
            return;
        }

        /// <summary>
        /// Builds trained readout layer.
        /// </summary>
        /// <param name="dataBundle">The data to be used for training.</param>
        /// <param name="predictorsMapper">The mapper of specific predictors to readout units (optional).</param>
        /// <param name="controller">The build process controller (optional).</param>
        /// <param name="randomizerSeek">Specifies the random number generator initial seek (optional). A value greater than or equal to 0 will always ensure the same initialization.</param>
        /// <returns>The results of training.</returns>
        public RegressionOverview Build(VectorBundle dataBundle,
                                        PredictorsMapper predictorsMapper = null,
                                        TNRNetBuilder.BuildControllerDelegate controller = null,
                                        int randomizerSeek = 0
                                        )
        {
            if (Trained)
            {
                throw new InvalidOperationException("Readout layer is already built.");
            }
            //Basic checks
            int numOfPredictors = dataBundle.InputVectorCollection[0].Length;
            int numOfOutputs = dataBundle.OutputVectorCollection[0].Length;
            if (numOfPredictors == 0)
            {
                throw new InvalidOperationException($"Number of predictors must be greater than 0.");
            }
            if (numOfOutputs != ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count)
            {
                throw new InvalidOperationException($"Incorrect length of output vectors.");
            }
            //Predictors mapper (specified or default)
            _predictorsMapper = predictorsMapper ?? new PredictorsMapper(numOfPredictors);
            //Allocation and preparation of feature filters
            //Predictors
            _predictorFeatureFilterCollection = new FeatureFilterBase[numOfPredictors];
            Parallel.For(0, _predictorFeatureFilterCollection.Length, nrmIdx =>
            {
                _predictorFeatureFilterCollection[nrmIdx] = new RealFeatureFilter(InternalDataRange, true, true);
                for (int pairIdx = 0; pairIdx < dataBundle.InputVectorCollection.Count; pairIdx++)
                {
                    //Adjust filter
                    _predictorFeatureFilterCollection[nrmIdx].Update(dataBundle.InputVectorCollection[pairIdx][nrmIdx]);
                }
            });
            //Output values
            _outputFeatureFilterCollection = new FeatureFilterBase[numOfOutputs];
            Parallel.For(0, _outputFeatureFilterCollection.Length, nrmIdx =>
            {
                _outputFeatureFilterCollection[nrmIdx] = FeatureFilterFactory.Create(InternalDataRange, ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection[nrmIdx].TaskCfg.FeatureFilterCfg);
                for (int pairIdx = 0; pairIdx < dataBundle.OutputVectorCollection.Count; pairIdx++)
                {
                    //Adjust output normalizer
                    _outputFeatureFilterCollection[nrmIdx].Update(dataBundle.OutputVectorCollection[pairIdx][nrmIdx]);
                }
            });
            //Data normalization
            //Allocation
            double[][] normalizedPredictorsCollection = new double[dataBundle.InputVectorCollection.Count][];
            double[][] normalizedIdealOutputsCollection = new double[dataBundle.OutputVectorCollection.Count][];
            //Normalization
            Parallel.For(0, dataBundle.InputVectorCollection.Count, pairIdx =>
            {
                //Predictors
                double[] predictors = new double[numOfPredictors];
                for (int i = 0; i < numOfPredictors; i++)
                {
                    if (_predictorsMapper.PredictorGeneralSwitchCollection[i])
                    {
                        predictors[i] = _predictorFeatureFilterCollection[i].ApplyFilter(dataBundle.InputVectorCollection[pairIdx][i]);
                    }
                    else
                    {
                        predictors[i] = double.NaN;
                    }
                }
                normalizedPredictorsCollection[pairIdx] = predictors;
                //Outputs
                double[] outputs = new double[numOfOutputs];
                for (int i = 0; i < numOfOutputs; i++)
                {
                    outputs[i] = _outputFeatureFilterCollection[i].ApplyFilter(dataBundle.OutputVectorCollection[pairIdx][i]);
                }
                normalizedIdealOutputsCollection[pairIdx] = outputs;
            });

            //Random object initialization
            Random rand = (randomizerSeek < 0 ? new Random() : new Random(randomizerSeek));
            //Create shuffled copy of the data
            VectorBundle shuffledData = new VectorBundle(normalizedPredictorsCollection, normalizedIdealOutputsCollection);
            shuffledData.Shuffle(rand);

            //"One Takes All" groups input data space initialization
            List<CompositeResult[]> allReadoutUnitResults = new List<CompositeResult[]>(shuffledData.InputVectorCollection.Count);
            if (_oneTakesAllGroupCollection != null)
            {
                for (int i = 0; i < shuffledData.InputVectorCollection.Count; i++)
                {
                    allReadoutUnitResults.Add(new CompositeResult[ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count]);
                }
            }

            //Building of readout units
            for (int unitIdx = 0; unitIdx < ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; unitIdx++)
            {
                List<double[]> idealValueCollection = new List<double[]>(shuffledData.OutputVectorCollection.Count);
                //Transformation of ideal vectors to a single value vectors
                foreach (double[] idealVector in shuffledData.OutputVectorCollection)
                {
                    double[] value = new double[1];
                    value[0] = idealVector[unitIdx];
                    idealValueCollection.Add(value);
                }
                List<double[]> readoutUnitInputVectorCollection = _predictorsMapper.CreateVectorCollection(ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection[unitIdx].Name, shuffledData.InputVectorCollection);
                VectorBundle readoutUnitDataBundle = new VectorBundle(readoutUnitInputVectorCollection, idealValueCollection);
                _readoutUnitCollection[unitIdx].EpochDone += OnRegressionEpochDone;
                _readoutUnitCollection[unitIdx].Build(readoutUnitDataBundle,
                                                      _outputFeatureFilterCollection[unitIdx],
                                                      rand,
                                                      controller
                                                      );
                //Add unit's all computed results into the input data for "One Takes All" groups
                if (_oneTakesAllGroupCollection != null)
                {
                    for (int sampleIdx = 0; sampleIdx < readoutUnitDataBundle.InputVectorCollection.Count; sampleIdx++)
                    {
                        allReadoutUnitResults[sampleIdx][unitIdx] = _readoutUnitCollection[unitIdx].Compute(readoutUnitDataBundle.InputVectorCollection[sampleIdx]);
                    }
                }
            }//unitIdx

            //One Takes All groups build
            if (_oneTakesAllGroupCollection != null)
            {
                foreach (OneTakesAllGroup group in _oneTakesAllGroupCollection)
                {
                    //Only the group having inner probabilistic cluster has to be built
                    if (group.DecisionMethod == OneTakesAllGroup.OneTakesAllDecisionMethod.ClusterChain)
                    {
                        BinFeatureFilter[] groupFilters = new BinFeatureFilter[group.NumOfMemberClasses];
                        for (int i = 0; i < group.NumOfMemberClasses; i++)
                        {
                            groupFilters[i] = (BinFeatureFilter)_outputFeatureFilterCollection[group.MemberReadoutUnitIndexCollection[i]];
                        }
                        group.RegressionEpochDone += OnRegressionEpochDone;
                        group.Build(allReadoutUnitResults, shuffledData.OutputVectorCollection, groupFilters, rand, controller);
                    }
                }
            }

            //Readout layer is trained and ready
            Trained = true;
            return new RegressionOverview(ReadoutUnitErrStatCollection);
        }

        /// <summary>
        /// Creates the text report of predicted values.
        /// </summary>
        /// <param name="predictedValues">The computed vector.</param>
        /// <param name="margin">Specifies the left margin of the text.</param>
        /// <returns>The built text report.</returns>
        public string GetForecastReport(double[] predictedValues, int margin)
        {
            string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
            StringBuilder sb = new StringBuilder();
            //Results
            for (int outputIdx = 0; outputIdx < ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; outputIdx++)
            {
                sb.Append(leftMargin + $"Output field [{ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection[outputIdx].Name}]: {predictedValues[outputIdx].ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Normalizes the vector of predictors.
        /// </summary>
        /// <param name="predictors">The predictors vector.</param>
        private double[] NormalizePredictors(double[] predictors)
        {
            //Check
            if (predictors.Length != _predictorFeatureFilterCollection.Length)
            {
                throw new InvalidOperationException($"Incorrect length of predictors vector.");
            }
            double[] nrmPredictors = new double[predictors.Length];
            for (int i = 0; i < predictors.Length; i++)
            {
                nrmPredictors[i] = _predictorFeatureFilterCollection[i].ApplyFilter(predictors[i]);
            }
            return nrmPredictors;
        }

        /// <summary>
        /// Naturalizes the output values.
        /// </summary>
        /// <param name="outputs">The output values vector.</param>
        private double[] NaturalizeOutputs(double[] outputs)
        {
            double[] natOutputs = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            {
                natOutputs[i] = _outputFeatureFilterCollection[i].ApplyReverse(outputs[i]);
            }
            return natOutputs;
        }

        /// <summary>
        /// Computes the readout units.
        /// </summary>
        private CompositeResult[] ComputeReadoutUnits(double[] predictors, out double[] outputVector)
        {
            CompositeResult[] unitsResults = new CompositeResult[_readoutUnitCollection.Length];
            outputVector = new double[_readoutUnitCollection.Length];
            for (int unitIdx = 0; unitIdx < _readoutUnitCollection.Length; unitIdx++)
            {
                double[] readoutUnitInputVector = _predictorsMapper.CreateVector(ReadoutLayerCfg.ReadoutUnitsCfg.ReadoutUnitCfgCollection[unitIdx].Name, predictors);
                CompositeResult unitResult = _readoutUnitCollection[unitIdx].Compute(readoutUnitInputVector);
                outputVector[unitIdx] = unitResult.Result[0];
                unitsResults[unitIdx] = unitResult;
            }
            return unitsResults;
        }

        /// <summary>
        /// Computes the readout layer.
        /// </summary>
        /// <param name="predictors">The predictors.</param>
        /// <param name="readoutData">The detailed computed data.</param>
        /// <returns>An output vector of the computed and naturalized values.</returns>
        public double[] Compute(double[] predictors, out ReadoutData readoutData)
        {
            //Check readyness
            if (!Trained)
            {
                throw new InvalidOperationException($"Readout layer is not trained. Build function has to be called before Compute function can be used.");
            }
            //Normalize predictors
            double[] nrmPredictors = NormalizePredictors(predictors);
            //Compute all readout units
            CompositeResult[] unitsResults = ComputeReadoutUnits(nrmPredictors, out double[] nrmOutputVector);
            //Build readout units results
            ReadoutData.ReadoutUnitData[] readoutUnitsData = new ReadoutData.ReadoutUnitData[unitsResults.Length];
            for (int unitIdx = 0; unitIdx < readoutUnitsData.Length; unitIdx++)
            {
                readoutUnitsData[unitIdx] = new ReadoutData.ReadoutUnitData()
                {
                    Name = _readoutUnitCollection[unitIdx].Name,
                    Index = _readoutUnitCollection[unitIdx].Index,
                    Task = _readoutUnitCollection[unitIdx].Task,
                    CompResult = unitsResults[unitIdx],
                    RawNrmDataValue = nrmOutputVector[unitIdx],
                    RawNatDataValue = _outputFeatureFilterCollection[unitIdx].ApplyReverse(nrmOutputVector[unitIdx]),
                    FinalNatDataValue = _outputFeatureFilterCollection[unitIdx].ApplyReverse(nrmOutputVector[unitIdx])
                };
            }

            //Compute all "One Takes All" groups
            ReadoutData.OneTakesAllGroupData[] groupsData = null;
            if (_oneTakesAllGroupCollection != null)
            {
                groupsData = new ReadoutData.OneTakesAllGroupData[_oneTakesAllGroupCollection.Length];
                for (int groupIdx = 0; groupIdx < _oneTakesAllGroupCollection.Length; groupIdx++)
                {

                    int groupInnerWinnerIdx = _oneTakesAllGroupCollection[groupIdx].Compute(unitsResults, out CompositeResult groupResult, out double[] groupOutputVector);
                    int layerWinnerIdx = _oneTakesAllGroupCollection[groupIdx].MemberReadoutUnitIndexCollection[groupInnerWinnerIdx];
                    groupsData[groupIdx] = new ReadoutData.OneTakesAllGroupData()
                    {
                        GroupName = _oneTakesAllGroupCollection[groupIdx].Name,
                        WinningReadoutUnitName = _readoutUnitCollection[layerWinnerIdx].Name,
                        WinningReadoutUnitIndex = layerWinnerIdx,
                        MemberWinningGroupIndex = groupInnerWinnerIdx,
                        MemberReadoutUnitIndexes = _oneTakesAllGroupCollection[groupIdx].MemberReadoutUnitIndexCollection.ToArray(),
                        CompResult = groupResult,
                        MemberProbabilities = groupOutputVector
                    };
                    //Update nrmOuputVector
                    for (int i = 0; i < _oneTakesAllGroupCollection[groupIdx].MemberReadoutUnitIndexCollection.Count; i++)
                    {
                        if (i == groupInnerWinnerIdx)
                        {
                            nrmOutputVector[_oneTakesAllGroupCollection[groupIdx].MemberReadoutUnitIndexCollection[i]] = InternalDataRange.Max;
                        }
                        else
                        {
                            nrmOutputVector[_oneTakesAllGroupCollection[groupIdx].MemberReadoutUnitIndexCollection[i]] = InternalDataRange.Min;
                        }
                        //nrmOutputVector[_oneTakesAllGroupCollection[groupIdx].MemberReadoutUnitIndexCollection[i]] = groupOutputVector[i];
                    }
                }
            }
            //Output data finalization
            double[] natOuputVector = NaturalizeOutputs(nrmOutputVector);
            for (int unitIdx = 0; unitIdx < readoutUnitsData.Length; unitIdx++)
            {
                readoutUnitsData[unitIdx].FinalNatDataValue = natOuputVector[unitIdx];
            }
            readoutData = new ReadoutData(nrmOutputVector, natOuputVector, readoutUnitsData, groupsData);
            return natOuputVector;
        }

        //Inner classes
        /// <summary>
        /// Implements the holder of detailed computed data.
        /// </summary>
        [Serializable]
        public class ReadoutData
        {
            /// <summary>
            /// The vector of normalized output values.
            /// </summary>
            public double[] NrmDataVector { get; }
            /// <summary>
            /// The vector of naturalized output values.
            /// </summary>
            public double[] NatDataVector { get; }
            /// <summary>
            /// The collection of readout units data.
            /// </summary>
            public List<ReadoutUnitData> ReadoutUnitDataCollection { get; }
            /// <summary>
            /// The collection of one-takes-all groups data.
            /// </summary>
            public List<OneTakesAllGroupData> OneTakesAllGroupDataCollection { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="nrmDataVector">The vector of normalized output values.</param>
            /// <param name="natDataVector">The vector of naturalized output values.</param>
            /// <param name="unitsResults">The collection of readout units data.</param>
            /// <param name="oneTakesAllGroupsResults">The collection of one-takes-all groups data.</param>
            public ReadoutData(double[] nrmDataVector,
                               double[] natDataVector,
                               ReadoutUnitData[] unitsResults,
                               OneTakesAllGroupData[] oneTakesAllGroupsResults
                               )
            {
                NrmDataVector = nrmDataVector;
                NatDataVector = natDataVector;
                ReadoutUnitDataCollection = new List<ReadoutUnitData>();
                foreach (ReadoutUnitData unitResult in unitsResults)
                {
                    ReadoutUnitDataCollection.Add(unitResult);
                }
                //One Takes All groups
                OneTakesAllGroupDataCollection = new List<OneTakesAllGroupData>();
                if (oneTakesAllGroupsResults != null)
                {
                    foreach (OneTakesAllGroupData groupResult in oneTakesAllGroupsResults)
                    {
                        OneTakesAllGroupDataCollection.Add(groupResult);
                    }
                }
                return;
            }

            /// <summary>
            /// Gets the one-takes-all group data by the group name.
            /// </summary>
            /// <param name="groupName">The name of the one-takes-all group.</param>
            /// <returns>The group data or null if not found.</returns>
            public OneTakesAllGroupData GetOneTakesAllGroupData(string groupName)
            {
                foreach (OneTakesAllGroupData groupData in OneTakesAllGroupDataCollection)
                {
                    if (groupData.GroupName == groupName)
                    {
                        return groupData;
                    }
                }
                return null;
            }

            /// <summary>
            /// Gets the readout unit data by the readout unit name.
            /// </summary>
            /// <param name="readoutUnitName">The name of the readout unit.</param>
            /// <returns>The readout unit data or null if not found.</returns>
            public ReadoutUnitData GetReadoutUnitData(string readoutUnitName)
            {
                foreach (ReadoutUnitData readoutUnitData in ReadoutUnitDataCollection)
                {
                    if (readoutUnitData.Name == readoutUnitName)
                    {
                        return readoutUnitData;
                    }
                }
                return null;
            }

            //Inner classes
            /// <summary>
            /// Implements the holder of the readout unit results.
            /// </summary>
            [Serializable]
            public class ReadoutUnitData
            {
                /// <summary>
                /// The name of the readout unit.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// The zero-based index of the readout unit.
                /// </summary>
                public int Index { get; set; }

                /// <inheritdoc cref="ReadoutUnit.TaskType"/>
                public ReadoutUnit.TaskType Task { get; set; }

                /// <summary>
                /// The composite result.
                /// </summary>
                public CompositeResult CompResult { get; set; }

                /// <summary>
                /// The normalized output data value computed by the unit.
                /// </summary>
                public double RawNrmDataValue { get; set; }

                /// <summary>
                /// The naturalized output data value computed by the unit.
                /// </summary>
                public double RawNatDataValue { get; set; }

                /// <summary>
                /// The naturalized final output data value.
                /// </summary>
                public double FinalNatDataValue { get; set; }

            }//ReadoutUnitData

            /// <summary>
            /// Implements the holder of the "One Takes All" group result.
            /// </summary>
            [Serializable]
            public class OneTakesAllGroupData
            {
                /// <summary>
                /// The name of the "One Takes All" group.
                /// </summary>
                public string GroupName { get; set; }

                /// <summary>
                /// The name of the winning readout unit (class).
                /// </summary>
                public string WinningReadoutUnitName { get; set; }

                /// <summary>
                /// The zero-based index of the winning readout unit.
                /// </summary>
                public int WinningReadoutUnitIndex { get; set; }

                /// <summary>
                /// The zero-based index of the winning member within the group.
                /// </summary>
                public int MemberWinningGroupIndex { get; set; }

                /// <summary>
                /// The indexes of readout units belonging to the group.
                /// </summary>
                public int[] MemberReadoutUnitIndexes { get; set; }

                /// <summary>
                /// The composite result (always in 0...1 range).
                /// </summary>
                public CompositeResult CompResult { get; set; }

                /// <summary>
                /// The probabilities of the group members (probabilities are in common range).
                /// </summary>
                public double[] MemberProbabilities { get; set; }

            }//OneTakesAllGroupData

        }//ReadoutData

        /// <summary>
        /// Implements the holder of the readout layer training (regression) results.
        /// </summary>
        [Serializable]
        public class RegressionOverview
        {
            /// <summary>
            /// The collection of error statistics of the readout units.
            /// </summary>
            public List<TNRNetCluster.ClusterErrStatistics> ReadoutUnitErrStatCollection { get; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="readoutUnitErrStatCollection">The collection of error statistics of the readout units.</param>
            public RegressionOverview(List<TNRNetCluster.ClusterErrStatistics> readoutUnitErrStatCollection)
            {
                ReadoutUnitErrStatCollection = readoutUnitErrStatCollection;
                return;
            }

            //Methods
            private string BuildErrStatReport(string leftMargin, TNRNetCluster.ClusterErrStatistics ces)
            {
                StringBuilder sb = new StringBuilder();
                if (ces.BinaryErrStat != null)
                {
                    //Classification task report
                    sb.Append(leftMargin + $"  Classification of negative samples" + Environment.NewLine);
                    sb.Append(leftMargin + $"    Number of samples: {ces.BinaryErrStat.BinValErrStat[0].NumOfSamples}" + Environment.NewLine);
                    sb.Append(leftMargin + $"     Number of errors: {ces.BinaryErrStat.BinValErrStat[0].Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"           Error rate: {ces.BinaryErrStat.BinValErrStat[0].ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"             Accuracy: {(1 - ces.BinaryErrStat.BinValErrStat[0].ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"  Classification of positive samples" + Environment.NewLine);
                    sb.Append(leftMargin + $"    Number of samples: {ces.BinaryErrStat.BinValErrStat[1].NumOfSamples}" + Environment.NewLine);
                    sb.Append(leftMargin + $"     Number of errors: {ces.BinaryErrStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"           Error rate: {ces.BinaryErrStat.BinValErrStat[1].ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"             Accuracy: {(1 - ces.BinaryErrStat.BinValErrStat[1].ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"  Overall classification results" + Environment.NewLine);
                    sb.Append(leftMargin + $"    Number of samples: {ces.BinaryErrStat.TotalErrStat.NumOfSamples}" + Environment.NewLine);
                    sb.Append(leftMargin + $"     Number of errors: {ces.BinaryErrStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"           Error rate: {ces.BinaryErrStat.TotalErrStat.ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"             Accuracy: {(1 - ces.BinaryErrStat.TotalErrStat.ArithAvg).ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                }
                else
                {
                    //Forecast task report
                    sb.Append(leftMargin + $"  Number of samples: {ces.NatPrecissionErrStat.NumOfSamples}" + Environment.NewLine);
                    sb.Append(leftMargin + $"      Biggest error: {ces.NatPrecissionErrStat.Max.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"     Smallest error: {ces.NatPrecissionErrStat.Min.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                    sb.Append(leftMargin + $"      Average error: {ces.NatPrecissionErrStat.ArithAvg.ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
                }
                sb.Append(Environment.NewLine);
                return sb.ToString();
            }

            /// <summary>
            /// Returns the text report of the readout layer training (regression).
            /// </summary>
            /// <param name="margin">Specifies the left margin of the text.</param>
            /// <returns>The built text report.</returns>
            public string GetTrainingResultsReport(int margin)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                StringBuilder sb = new StringBuilder();
                //Training results of readout units
                foreach (TNRNetCluster.ClusterErrStatistics ces in ReadoutUnitErrStatCollection)
                {
                    sb.Append(leftMargin + $"Output field [{ces.ClusterName}]" + Environment.NewLine);
                    sb.Append(BuildErrStatReport(leftMargin, ces));
                }
                return sb.ToString();
            }

        }//RegressionOverview

    }//ReadoutLayer

}//Namespace
