using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using RCNet.Neural.Data;
using RCNet.Extensions;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Class implements the common readout layer for the reservoir computing methods
    /// </summary>
    [Serializable]
    public class ReadoutLayer
    {
        //Constants

        //Static attributes
        /// <summary>
        /// Input and output data will be normalized to this range before the usage
        /// </summary>
        public static readonly Interval DataRange = new Interval(-1, 1);

        //Delegates
        /// <summary>
        /// Delegate of RegressionEpochDone event handler.
        /// </summary>
        /// <param name="buildingState">Current state of the regression process</param>
        /// <param name="foundBetter">Indicates that the best network was found as a result of the performed epoch</param>
        public delegate void RegressionEpochDoneHandler(TrainedNetworkBuilder.BuildingState buildingState, bool foundBetter);

        //Events
        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        [field: NonSerialized]
        public event RegressionEpochDoneHandler RegressionEpochDone;

        //Attribute properties
        /// <summary>
        /// Indicates if the readout layer is trained
        /// </summary>
        public bool Trained { get; private set; }
        /// <summary>
        /// Readout layer configuration
        /// </summary>
        public ReadoutLayerSettings Settings { get; }

        //Attributes
        /// <summary>
        /// Collection of feature filters of input predictors
        /// </summary>
        private BaseFeatureFilter[] _predictorFeatureFilterCollection;
        /// <summary>
        /// Collection of feature filters of output values
        /// </summary>
        private BaseFeatureFilter[] _outputFeatureFilterCollection;
        /// <summary>
        /// Mapping of specific predictors to readout units
        /// </summary>
        private PredictorsMapper _predictorsMapper;
        /// <summary>
        /// Collection of trained readout units.
        /// </summary>
        private ReadoutUnit[] _readoutUnitCollection;


        //Constructor
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="settings">Readout layer configuration</param>
        public ReadoutLayer(ReadoutLayerSettings settings)
        {
            Settings = settings.DeepClone();
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Cluster error statistics of readout units
        /// </summary>
        public List<TrainedNetworkCluster.ClusterErrStatistics> ReadoutUnitErrStatCollection
        {
            get
            {
                //Create and return the deep clone
                List<TrainedNetworkCluster.ClusterErrStatistics> clonedStatisticsCollection = new List<TrainedNetworkCluster.ClusterErrStatistics>(_readoutUnitCollection.Length);
                foreach (ReadoutUnit ru in _readoutUnitCollection)
                {
                    clonedStatisticsCollection.Add(ru.NetworkCluster.ErrorStats.DeepClone());
                }
                return clonedStatisticsCollection;
            }
        }

        //Methods
        /// <summary>
        /// Resets readout layer to its initial untrained state
        /// </summary>
        public void Reset()
        {
            _predictorFeatureFilterCollection = null;
            _outputFeatureFilterCollection = null;
            _predictorsMapper = null;
            _readoutUnitCollection = new ReadoutUnit[Settings.ReadoutUnitCfgCollection.Count];
            _readoutUnitCollection.Populate(null);
            Trained = false;
            return;
        }

        private void OnRegressionEpochDone(TrainedNetworkBuilder.BuildingState buildingState, bool foundBetter)
        {
            //Only raise up
            RegressionEpochDone(buildingState, foundBetter);
            return;
        }

        /// <summary>
        /// Builds trained readout layer.
        /// </summary>
        /// <param name="dataBundle">Collection of input predictors and associated desired output values</param>
        /// <param name="predictorsMapper">Optional specific mapping of predictors to readout units</param>
        /// <param name="controller">Optional external regression controller</param>
        /// <returns>Results of the regression</returns>
        public RegressionOverview Build(VectorBundle dataBundle,
                                        PredictorsMapper predictorsMapper = null,
                                        TrainedNetworkBuilder.RegressionControllerDelegate controller = null
                                        )
        {
            //Basic checks
            int numOfPredictors = dataBundle.InputVectorCollection[0].Length;
            int numOfOutputs = dataBundle.OutputVectorCollection[0].Length;
            if (numOfPredictors == 0)
            {
                throw new Exception("Number of predictors must be greater tham 0.");
            }
            if (numOfOutputs != Settings.ReadoutUnitCfgCollection.Count)
            {
                throw new Exception("Incorrect length of output vectors.");
            }
            //Predictors mapper (specified or default)
            _predictorsMapper = predictorsMapper ?? new PredictorsMapper(numOfPredictors);
            //Allocation and preparation of feature filters
            //Predictors
            _predictorFeatureFilterCollection = new BaseFeatureFilter[numOfPredictors];
            Parallel.For(0, _predictorFeatureFilterCollection.Length, nrmIdx =>
            {
                _predictorFeatureFilterCollection[nrmIdx] = new RealFeatureFilter(DataRange, true, true);
                for (int pairIdx = 0; pairIdx < dataBundle.InputVectorCollection.Count; pairIdx++)
                {
                    //Adjust filter
                    _predictorFeatureFilterCollection[nrmIdx].Update(dataBundle.InputVectorCollection[pairIdx][nrmIdx]);
                }
            });
            //Output values
            _outputFeatureFilterCollection = new BaseFeatureFilter[numOfOutputs];
            Parallel.For(0, _outputFeatureFilterCollection.Length, nrmIdx =>
            {
                _outputFeatureFilterCollection[nrmIdx] = FeatureFilterFactory.Create(DataRange, Settings.ReadoutUnitCfgCollection[nrmIdx].FeatureFilterCfg);
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
            Random rand = new Random(0);
            //Create shuffled copy of the data
            VectorBundle shuffledData = new VectorBundle(normalizedPredictorsCollection, normalizedIdealOutputsCollection);
            shuffledData.Shuffle(rand);

            //Building of readout units
            for (int unitIdx = 0; unitIdx < Settings.ReadoutUnitCfgCollection.Count; unitIdx++)
            {
                List<double[]> idealValueCollection = new List<double[]>(shuffledData.OutputVectorCollection.Count);
                //Transformation of ideal vectors to a single value vectors
                foreach (double[] idealVector in shuffledData.OutputVectorCollection)
                {
                    double[] value = new double[1];
                    value[0] = idealVector[unitIdx];
                    idealValueCollection.Add(value);
                }
                List<double[]> readoutUnitInputVectorCollection = _predictorsMapper.CreateVectorCollection(Settings.ReadoutUnitCfgCollection[unitIdx].Name, shuffledData.InputVectorCollection);
                VectorBundle readoutUnitDataBundle = new VectorBundle(readoutUnitInputVectorCollection, idealValueCollection);
                TrainedNetworkClusterBuilder readoutUnitBuilder = new TrainedNetworkClusterBuilder(Settings.ReadoutUnitCfgCollection[unitIdx].Name,
                                                                                                   Settings.ReadoutUnitCfgCollection[unitIdx].NetSettings,
                                                                                                   Settings.ReadoutUnitCfgCollection[unitIdx].BinBorder,
                                                                                                   rand,
                                                                                                   controller
                                                                                                   );
                //Register notification
                readoutUnitBuilder.RegressionEpochDone += OnRegressionEpochDone;
                //Build trained readout unit. Trained unit becomes to be the predicting cluster member
                _readoutUnitCollection[unitIdx] = new ReadoutUnit(unitIdx,
                                                                  readoutUnitBuilder.Build(readoutUnitDataBundle,
                                                                                           Settings.TestDataRatio,
                                                                                           Settings.NumOfFolds,
                                                                                           Settings.Repetitions,
                                                                                           new BaseFeatureFilter[] {_outputFeatureFilterCollection [unitIdx]}
                                                                                           )
                                                                  );
            }//unitIdx
            
            //Readout layer is trained and ready
            Trained = true;
            return new RegressionOverview(ReadoutUnitErrStatCollection);
        }

        /// <summary>
        /// Creates report of predicted values
        /// </summary>
        /// <param name="predictedValues">Vector of computed values.</param>
        /// <param name="margin">Specifies how many spaces should be at the begining of each row.</param>
        /// <returns>Built text report</returns>
        public string GetForecastReport(double[] predictedValues, int margin)
        {
            string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
            StringBuilder sb = new StringBuilder();
            //Results
            for (int outputIdx = 0; outputIdx < Settings.ReadoutUnitCfgCollection.Count; outputIdx++)
            {
                sb.Append(leftMargin + $"Output field [{Settings.ReadoutUnitCfgCollection[outputIdx].Name}]: {predictedValues[outputIdx].ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Normalizes predictors vector
        /// </summary>
        /// <param name="predictors">Predictors vector</param>
        private double[] NormalizePredictors(double[] predictors)
        {
            //Check
            if (predictors.Length != _predictorFeatureFilterCollection.Length)
            {
                throw new Exception("Incorrect length of predictors vector.");
            }
            double[] nrmPredictors = new double[predictors.Length];
            for (int i = 0; i < predictors.Length; i++)
            {
                nrmPredictors[i] = _predictorFeatureFilterCollection[i].ApplyFilter(predictors[i]);
            }
            return nrmPredictors;
        }

        /// <summary>
        /// Naturalizes output values vector
        /// </summary>
        /// <param name="outputs">Output values vector</param>
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
        /// Computes readout layer output vector
        /// </summary>
        private double[] ComputeInternal(double[] predictors, out List<double[]> unitsAllSubResults)
        {
            unitsAllSubResults = new List<double[]>(Settings.ReadoutUnitCfgCollection.Count);
            double[] outputVector = new double[_readoutUnitCollection.Length];
            for(int unitIdx = 0; unitIdx < _readoutUnitCollection.Length; unitIdx++)
            {
                double[] readoutUnitInputVector = _predictorsMapper.CreateVector(Settings.ReadoutUnitCfgCollection[unitIdx].Name, predictors);
                outputVector[unitIdx] = _readoutUnitCollection[unitIdx].NetworkCluster.Compute(readoutUnitInputVector, out double[] memberOutputCollection);
                unitsAllSubResults.Add(memberOutputCollection);
            }
            return outputVector;
        }

        /// <summary>
        /// Computes readout layer output vector
        /// </summary>
        /// <param name="predictors">The predictors</param>
        public double[] Compute(double[] predictors, out List<double[]> unitsAllSubResults)
        {
            //Check readyness
            if (!Trained)
            {
                throw new Exception("Readout layer is not trained. Build function has to be called before Compute function can be used.");
            }
            double[] nrmPredictors = NormalizePredictors(predictors);
            double[] outputVector = ComputeInternal(nrmPredictors, out unitsAllSubResults);
            //Denormalization
            double[] natOuputVector = NaturalizeOutputs(outputVector);
            //Return result
            return natOuputVector;
        }

        /// <summary>
        /// Decides what readout unit within the "one winner" group is winning
        /// </summary>
        /// <param name="oneWinnerGroupName">Name of One Winner group</param>
        /// <param name="dataVector">Vector of values corresponding to layer's readout units</param>
        /// <param name="membersIndexes">Returned indexes of readout units belonging to specified "one winner" group</param>
        /// <param name="membersWeightedDataVector">Returned weighted probabilities of specified "one winner" group member units (in the same order as returned indexes)</param>
        public int DecideOneWinner(string oneWinnerGroupName, double[] dataVector, out int[] membersIndexes, out double[] membersWeightedDataVector)
        {
            //Obtain group members indexes
            membersIndexes = (from member in Settings.OneWinnerGroupCfgCollection[oneWinnerGroupName].Members select member.Index).ToArray();
            //Compute members' weighted predictions
            membersWeightedDataVector = new double[membersIndexes.Length];
            for(int i = 0; i < membersIndexes.Length; i++)
            {
                membersWeightedDataVector[i] = dataVector[membersIndexes[i]];
            }
            //Find the highest probability unit
            int maxPIdx = -1;
            for (int i = 0; i < membersWeightedDataVector.Length; i++)
            {
                if (maxPIdx == -1 || membersWeightedDataVector[i] > dataVector[maxPIdx])
                {
                    maxPIdx = i;
                }
            }
            return membersIndexes[maxPIdx];
        }

        /// <summary>
        /// Computes readout layer and returns rich output data
        /// </summary>
        /// <param name="predictors">The predictors</param>
        public ReadoutData ComputeReadoutData(double[] predictors, out List<double[]> unitsAllSubResults)
        {
            return new ReadoutData(Compute(predictors, out unitsAllSubResults), this);
        }

        //Inner classes
        /// <summary>
        /// Encapsulates readout layer data
        /// </summary>
        [Serializable]
        public class ReadoutData
        {
            /// <summary>
            /// Vector of values corresponding to readout units
            /// </summary>
            public double[] DataVector { get; }
            /// <summary>
            /// Dictionary of readout units data.
            /// </summary>
            public Dictionary<string, ReadoutUnitData> ReadoutUnitDataCollection { get; }
            /// <summary>
            /// Dictionary of one-winner groups data.
            /// </summary>
            public Dictionary<string, OneWinnerGroupData> OneWinnerDataCollection { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="dataVector">Vector of values corresponding to readout units</param>
            /// <param name="rl">Readout layer object</param>
            public ReadoutData(double[] dataVector, ReadoutLayer rl)
            {
                //Alone units
                DataVector = dataVector;
                ReadoutUnitDataCollection = new Dictionary<string, ReadoutUnitData>();
                foreach(ReadoutLayerSettings.ReadoutUnitSettings rus in rl.Settings.ReadoutUnitCfgCollection)
                {
                    ReadoutUnitDataCollection.Add(rus.Name, new ReadoutUnitData() { Name = rus.Name, Index = rus.Index, Task = rus.TaskType, DataValue = DataVector[rus.Index] });
                }
                //One Winner groups
                OneWinnerDataCollection = new Dictionary<string, OneWinnerGroupData>();
                foreach (string oneWinnerGroupName in rl.Settings.OneWinnerGroupCfgCollection.Keys)
                {
                    //There is One Winner group
                    int winningUnitIndex = rl.DecideOneWinner(oneWinnerGroupName, dataVector, out int[] membersIndexes, out double[] membersWeightedDataVector);
                    OneWinnerDataCollection.Add(oneWinnerGroupName, new OneWinnerGroupData()
                    {
                        GroupName = oneWinnerGroupName,
                        WinningReadoutUnitName = rl.Settings.ReadoutUnitCfgCollection[winningUnitIndex].Name,
                        WinningReadoutUnitIndex = winningUnitIndex,
                        MemberReadoutUnitsIndexes = membersIndexes,
                        MemberReadoutUnitsProbabilities = membersWeightedDataVector
                    });
                }
                return;
            }

            //Inner classes
            /// <summary>
            /// Encapsulates single unit data
            /// </summary>
            [Serializable]
            public class ReadoutUnitData
            {
                /// <summary>
                /// Name of the readout unit
                /// </summary>
                public string Name { get; set; }
                /// <summary>
                /// Zero-based index of the readout unit
                /// </summary>
                public int Index { get; set; }
                /// <summary>
                /// Neural task
                /// </summary>
                public ReadoutUnit.TaskType Task { get; set; }
                /// <summary>
                /// Data value
                /// </summary>
                public double DataValue { get; set; }
            }//ReadoutUnitData

            /// <summary>
            /// Encapsulates one-winner group data
            /// </summary>
            [Serializable]
            public class OneWinnerGroupData
            {
                /// <summary>
                /// Name of the one-winner group
                /// </summary>
                public string GroupName { get; set; }
                /// <summary>
                /// Name of the winning readout unit (class)
                /// </summary>
                public string WinningReadoutUnitName { get; set; }
                /// <summary>
                /// Zero-based index of the winning readout unit
                /// </summary>
                public int WinningReadoutUnitIndex { get; set; }
                /// <summary>
                /// Indexes of group member readout units
                /// </summary>
                public int[] MemberReadoutUnitsIndexes { get; set; }
                /// <summary>
                /// Computed probabilities by group member readout units (in the same order as MemberReadoutUnitsIndexes)
                /// </summary>
                public double[] MemberReadoutUnitsProbabilities { get; set; }
            }//OneWinnerGroupData

        }//ReadoutData

        /// <summary>
        /// Maps specific predictors to readout units
        /// </summary>
        [Serializable]
        public class PredictorsMapper
        {
            //Attribute properties
            /// <summary>
            /// Collection of switches generally enabling/disabling predictors
            /// </summary>
            public bool[] PredictorGeneralSwitchCollection { get; private set; }
            //Attributes
            /// <summary>
            /// Mapping of readout unit to switches determining what predictors are assigned to.
            /// </summary>
            private readonly Dictionary<string, ReadoutUnitMap> _mapCollection;
            private readonly int _numOfAllowedPredictors;
            /// <summary>
            /// Creates initialized instance
            /// </summary>
            /// <param name="numOfPredictors">Total number of available predictors</param>
            public PredictorsMapper(int numOfPredictors)
            {
                PredictorGeneralSwitchCollection = new bool[numOfPredictors];
                PredictorGeneralSwitchCollection.Populate(true);
                _numOfAllowedPredictors = numOfPredictors;
                _mapCollection = new Dictionary<string, ReadoutUnitMap>();
                return;
            }

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="predictorGeneralSwitchCollection">Collection of switches generally enabling/disabling predictors</param>
            public PredictorsMapper(bool[] predictorGeneralSwitchCollection)
            {
                PredictorGeneralSwitchCollection = (bool[])predictorGeneralSwitchCollection.Clone();
                _numOfAllowedPredictors = 0;
                for(int i = 0; i < predictorGeneralSwitchCollection.Length; i++)
                {
                    if (predictorGeneralSwitchCollection[i]) ++_numOfAllowedPredictors;
                }
                if (_numOfAllowedPredictors == 0 || predictorGeneralSwitchCollection.Length == 0)
                {
                    throw new ArgumentException("There is no available predictor", "predictorGeneralSwitchCollection");
                }
                _mapCollection = new Dictionary<string, ReadoutUnitMap>();
                return;
            }

            /// <summary>
            /// Adds new mapping for ReadoutUntit
            /// </summary>
            /// <param name="readoutUnitName"></param>
            /// <param name="map">Boolean switches indicating if to use available prdictor for the ReadoutUnit</param>
            public void Add(string readoutUnitName, bool[] map)
            {
                if(map.Length != PredictorGeneralSwitchCollection.Length)
                {
                    throw new ArgumentException("Incorrect number of switches in the map", "map");
                }
                if (readoutUnitName.Length == 0)
                {
                    throw new ArgumentException("ReadoutUnit name can not be empty", "readoutUnitName");
                }
                if (_mapCollection.ContainsKey(readoutUnitName))
                {
                    throw new ArgumentException($"Mapping already contains mapping for ReadoutUnit {readoutUnitName}", "readoutUnitName");
                }
                //Apply general switches
                bool[] localMap = (bool[])map.Clone();
                int numOfReadoutUnitAllowedPredictors = 0;
                for(int i = 0; i < localMap.Length; i++)
                {
                    if(localMap[i])
                    {
                        if (!PredictorGeneralSwitchCollection[i])
                        {
                            localMap[i] = false;
                        }
                        else
                        {
                            ++numOfReadoutUnitAllowedPredictors;
                        }
                    }
                }
                if(numOfReadoutUnitAllowedPredictors < 1)
                {
                    throw new ArgumentException("Map contains no allowed predictors", "map");
                }
                _mapCollection.Add(readoutUnitName, new ReadoutUnitMap(localMap));
                return;
            }

            private double[] CreateVector(double[] predictors, bool[] map, int vectorLength)
            {
                if (predictors.Length != map.Length)
                {
                    throw new ArgumentException("Incorrect number of predictors", "predictors");
                }
                double[] vector = new double[vectorLength];
                for(int i = 0, vIdx = 0; i < predictors.Length; i++)
                {
                    if(map[i])
                    {
                        vector[vIdx] = predictors[i];
                        ++vIdx;
                    }
                }
                return vector;
            }

            /// <summary>
            /// Creates input vector containing specific subset of predictors for the ReadoutUnit.
            /// </summary>
            /// <param name="readoutUnitName">ReadoutUnit name</param>
            /// <param name="predictors">Available predictors</param>
            public double[] CreateVector(string readoutUnitName, double[] predictors)
            {
                if (predictors.Length != PredictorGeneralSwitchCollection.Length)
                {
                    throw new ArgumentException("Incorrect number of predictors", "predictors");
                }
                if (_mapCollection.ContainsKey(readoutUnitName))
                {
                    ReadoutUnitMap rum = _mapCollection[readoutUnitName];
                    return CreateVector(predictors, rum.Map, rum.VectorLength);
                }
                else
                {
                    return CreateVector(predictors, PredictorGeneralSwitchCollection, _numOfAllowedPredictors);
                }
            }

            /// <summary>
            /// Creates input vector collection where each vector containing specific subset of predictors for the ReadoutUnit.
            /// </summary>
            /// <param name="readoutUnitName">ReadoutUnit name</param>
            /// <param name="predictorsCollection">Collection of available predictors</param>
            public List<double[]> CreateVectorCollection(string readoutUnitName, List<double[]> predictorsCollection)
            {
                List<double[]> vectorCollection = new List<double[]>(predictorsCollection.Count);
                ReadoutUnitMap rum = null;
                if (_mapCollection.ContainsKey(readoutUnitName))
                {
                    rum = _mapCollection[readoutUnitName];
                }
                foreach(double[] predictors in predictorsCollection)
                {
                    if(rum == null)
                    {
                        vectorCollection.Add(CreateVector(predictors, PredictorGeneralSwitchCollection, _numOfAllowedPredictors));
                    }
                    else
                    {
                        vectorCollection.Add(CreateVector(predictors, rum.Map, rum.VectorLength));
                    }
                }
                return vectorCollection;
            }

            //Inner classes
            /// <summary>
            /// Maps specific predictors to readout unit
            /// </summary>
            [Serializable]
            private class ReadoutUnitMap
            {
                //Attribute properties
                /// <summary>
                /// Boolean switches indicating if to use available prdictor for this ReadoutUnit
                /// </summary>
                public bool[] Map { get; set; }
                /// <summary>
                /// Resulting length of ReadoutUnit's input vector (number of true switches in the Map)
                /// </summary>
                public int VectorLength { get; private set; }

                /// <summary>
                /// Creates initialized instance
                /// </summary>
                /// <param name="map">Boolean switches indicating if to use available prdictor for this ReadoutUnit.</param>
                public ReadoutUnitMap(bool[] map)
                {
                    Map = map;
                    VectorLength = 0;
                    foreach (bool bSwitch in Map)
                    {
                        if (bSwitch) ++VectorLength;
                    }
                    return;
                }

            }//ReadoutUnitMap
        }

        /// <summary>
        /// Contains results of readout layer training (regression)
        /// </summary>
        [Serializable]
        public class RegressionOverview
        {
            /// <summary>
            /// Collection of error statistics related to readout units
            /// </summary>
            public List<TrainedNetworkCluster.ClusterErrStatistics> ReadoutUnitErrStatCollection { get; }

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="readoutUnitErrStatCollection">Collection of error statistics related to readout units</param>
            public RegressionOverview(List<TrainedNetworkCluster.ClusterErrStatistics> readoutUnitErrStatCollection)
            {
                ReadoutUnitErrStatCollection = readoutUnitErrStatCollection;
                return;
            }

            //Methods
            private string BuildErrStatReport(string leftMargin, TrainedNetworkCluster.ClusterErrStatistics ces)
            {
                StringBuilder sb = new StringBuilder();
                if (ces.BinaryOutput)
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
            /// Returns report of the readout layer training (regression)
            /// </summary>
            /// <param name="margin">Specifies how many spaces should be at the begining of each row.</param>
            /// <returns>Built report</returns>
            public string GetTrainingResultsReport(int margin)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                StringBuilder sb = new StringBuilder();
                //Training results of readout units
                foreach (TrainedNetworkCluster.ClusterErrStatistics ces in ReadoutUnitErrStatCollection)
                {
                    sb.Append(leftMargin + $"Output field [{ces.ClusterName}]" + Environment.NewLine);
                    sb.Append(BuildErrStatReport(leftMargin, ces));
                }
                return sb.ToString();
            }

        }//RegressionOverview

    }//ReadoutLayer

}//Namespace
