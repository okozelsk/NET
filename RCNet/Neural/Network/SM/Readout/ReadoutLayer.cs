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

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Class implements the common readout layer for the reservoir computing methods
    /// </summary>
    [Serializable]
    public class ReadoutLayer
    {
        //Constants
        /// <summary>
        /// Maximum part of available samples useable for test purposes
        /// </summary>
        public const double MaxRatioOfTestData = 0.5d;
        /// <summary>
        /// Minimum length of the test dataset
        /// </summary>
        public const int MinLengthOfTestDataset = 2;

        //Static attributes
        /// <summary>
        /// Input and output data will be normalized to this range before the usage
        /// </summary>
        public static readonly Interval DataRange = new Interval(-1, 1);

        //Delegates
        /// <summary>
        /// Delegate of RegressionEpochDone event handler.
        /// </summary>
        /// <param name="regrState">Current state of the regression process</param>
        /// <param name="bestUnitChanged">Indicates that the best readout unit was changed as a result of the performed epoch</param>
        public delegate void RegressionEpochDoneHandler(ReadoutUnitBuilder.RegrState regrState, bool bestUnitChanged);

        //Events
        /// <summary>
        /// This informative event occurs every time the regression epoch is done
        /// </summary>
        public event RegressionEpochDoneHandler RegressionEpochDone;

        //Attribute properties
        /// <summary>
        /// Indicates if the readout layer is trained
        /// </summary>
        public bool Trained { get; private set; }

        //Attributes
        /// <summary>
        /// Readout layer configuration
        /// </summary>
        private readonly ReadoutLayerSettings _settings;
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
        /// Collection of clusters of trained readout units. One cluster of units per output field.
        /// </summary>
        private ReadoutUnit[][] _clusterCollection;
        /// <summary>
        /// Cluster overall error statistics collection
        /// </summary>
        private List<ClusterErrStatistics> _clusterErrStatisticsCollection;



        //Constructor
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="settings">Readout layer configuration</param>
        public ReadoutLayer(ReadoutLayerSettings settings)
        {
            _settings = settings.DeepClone();
            foreach (ReadoutLayerSettings.ReadoutUnitSettings rus in _settings.ReadoutUnitCfgCollection)
            {
                if (!rus.OutputRange.BelongsTo(DataRange.Min) || !rus.OutputRange.BelongsTo(DataRange.Max))
                {
                    throw new Exception($"Readout unit {rus.Name} does not support data range <{DataRange.Min}; {DataRange.Max}>.");
                }
            }
            Reset();
            return;
        }

        //Properties
        /// <summary>
        /// Cluster overall error statistics collection
        /// </summary>
        public List<ClusterErrStatistics> ClusterErrStatisticsCollection
        {
            get
            {
                //Create and return the deep clone
                List<ClusterErrStatistics> clonedStatisticsCollection = new List<ClusterErrStatistics>(_clusterErrStatisticsCollection.Count);
                foreach (ClusterErrStatistics ces in _clusterErrStatisticsCollection)
                {
                    clonedStatisticsCollection.Add(ces.DeepClone());
                }
                return clonedStatisticsCollection;
            }
        }

        //Static methods
        /// <summary>
        /// Decides what readout unit within the One Winner group is winning
        /// </summary>
        /// <param name="oneWinnerGroupName">Name of One Winner group</param>
        /// <param name="dataVector">Vector of values corresponding to readout units</param>
        /// <param name="rls">Readout layer settings</param>
        public static int DecideOneWinner(string oneWinnerGroupName, double[] dataVector, ReadoutLayerSettings rls)
        {
            //Obtain group members
            List<ReadoutLayerSettings.ReadoutUnitSettings> rusCollection = rls.GetOneWinnerGroupMembers(oneWinnerGroupName);
            //Find the highest probability unit
            int maxPIdx = -1;
            for (int i = 0; i < rusCollection.Count; i++)
            {
                if (maxPIdx == -1 || dataVector[rusCollection[i].Index] > dataVector[maxPIdx])
                {
                    maxPIdx = rusCollection[i].Index;
                }
            }
            return maxPIdx;
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
            _clusterCollection = new ReadoutUnit[_settings.ReadoutUnitCfgCollection.Count][];
            _clusterErrStatisticsCollection = new List<ClusterErrStatistics>();
            Trained = false;
            return;
        }

        private void OnRegressionEpochDone(ReadoutUnitBuilder.RegrState regrState, bool bestUnitChanged)
        {
            //Only raise up
            RegressionEpochDone(regrState, bestUnitChanged);
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
                                        ReadoutUnitBuilder.RegressionControllerDelegate controller = null
                                        )
        {
            //Basic checks
            int numOfPredictors = dataBundle.InputVectorCollection[0].Length;
            int numOfOutputs = dataBundle.OutputVectorCollection[0].Length;
            if (numOfPredictors == 0)
            {
                throw new Exception("Number of predictors must be greater tham 0.");
            }
            if (numOfOutputs != _settings.ReadoutUnitCfgCollection.Count)
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
                _outputFeatureFilterCollection[nrmIdx] = FeatureFilterFactory.Create(DataRange, _settings.ReadoutUnitCfgCollection[nrmIdx].FeatureFilterCfg);
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

            //Data processing
            //Random object initialization
            Random rand = new Random(0);
            //Test dataset size
            if (_settings.TestDataRatio > MaxRatioOfTestData)
            {
                throw new ArgumentException($"Test data rato is greater than {MaxRatioOfTestData.ToString(CultureInfo.InvariantCulture)}", "TestDataSetSize");
            }
            int testDataSetLength = (int)Math.Round(normalizedIdealOutputsCollection.Length * _settings.TestDataRatio, 0);
            if (testDataSetLength < MinLengthOfTestDataset)
            {
                throw new ArgumentException($"Num of test samples is less than {MinLengthOfTestDataset.ToString(CultureInfo.InvariantCulture)}", "TestDataSetSize");
            }
            //Number of folds
            int numOfFolds = _settings.NumOfFolds;
            if (numOfFolds <= 0)
            {
                //Auto setup
                numOfFolds = normalizedIdealOutputsCollection.Length / testDataSetLength;
            }
            //Create shuffled copy of the data
            VectorBundle shuffledData = new VectorBundle(normalizedPredictorsCollection, normalizedIdealOutputsCollection);
            shuffledData.Shuffle(rand);
            //Data inspection, preparation of datasets and training of ReadoutUnits
            //Clusters of readout units (one cluster per each output field)
            for (int clusterIdx = 0; clusterIdx < _settings.ReadoutUnitCfgCollection.Count; clusterIdx++)
            {
                _clusterCollection[clusterIdx] = new ReadoutUnit[numOfFolds];
                List<double[]> idealValueCollection = new List<double[]>(normalizedIdealOutputsCollection.Length);
                //Transformation of ideal vectors to a single value vectors
                foreach (double[] idealVector in shuffledData.OutputVectorCollection)
                {
                    double[] value = new double[1];
                    value[0] = idealVector[clusterIdx];
                    idealValueCollection.Add(value);
                }
                List<double[]> readoutUnitInputVectorCollection = _predictorsMapper.CreateVectorCollection(_settings.ReadoutUnitCfgCollection[clusterIdx].Name, shuffledData.InputVectorCollection);
                VectorBundle readoutUnitDataBundle = new VectorBundle(readoutUnitInputVectorCollection, idealValueCollection);
                //Data split
                List<VectorBundle> subBundleCollection = readoutUnitDataBundle.Split(testDataSetLength, (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == ReadoutUnit.TaskType.Classification) ? DataRange.Mid : double.NaN);
                //Instantiate cluster error statistics
                ClusterErrStatistics ces = new ClusterErrStatistics(_settings.ReadoutUnitCfgCollection[clusterIdx].Name,
                                                                    clusterIdx,
                                                                    _settings.ReadoutUnitCfgCollection[clusterIdx].TaskType,
                                                                    numOfFolds,
                                                                    DataRange.Mid
                                                                    );
                //Train unit for each fold in the cluster.
                for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                {
                    //Prepare training data bundle
                    VectorBundle trainingData = new VectorBundle();
                    for (int bundleIdx = 0; bundleIdx < subBundleCollection.Count; bundleIdx++)
                    {
                        if (bundleIdx != foldIdx)
                        {
                            trainingData.Add(subBundleCollection[bundleIdx]);
                        }
                    }
                    ReadoutUnitBuilder readoutUnitBuilder = new ReadoutUnitBuilder(_settings.ReadoutUnitCfgCollection[clusterIdx],
                                                                                   foldIdx + 1,
                                                                                   numOfFolds,
                                                                                   trainingData,
                                                                                   subBundleCollection[foldIdx],
                                                                                   rand,
                                                                                   controller
                                                                                   );
                    //Register notification
                    readoutUnitBuilder.RegressionEpochDone += OnRegressionEpochDone;
                    //Build trained readout unit. Trained unit becomes to be the predicting cluster member
                    _clusterCollection[clusterIdx][foldIdx] = readoutUnitBuilder.Build();
                    //Update cluster error statistics (pesimistic approach)
                    for (int sampleIdx = 0; sampleIdx < subBundleCollection[foldIdx].OutputVectorCollection.Count; sampleIdx++)
                    {
                        double nrmComputedValue = _clusterCollection[clusterIdx][foldIdx].Network.Compute(subBundleCollection[foldIdx].InputVectorCollection[sampleIdx])[0];
                        double natComputedValue = _outputFeatureFilterCollection[clusterIdx].ApplyReverse(nrmComputedValue);
                        double natIdealValue = _outputFeatureFilterCollection[clusterIdx].ApplyReverse(subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][0]);
                        ces.Update(nrmComputedValue,
                                   subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][0],
                                   natComputedValue,
                                   natIdealValue
                                   );
                    }

                }//foldIdx
                _clusterErrStatisticsCollection.Add(ces);

            }//clusterIdx
            
            //Result bundle (performs full recomputation of the original data)
            ResultBundle resultBundle = new ResultBundle(dataBundle.InputVectorCollection.Count);
            for(int rowIdx = 0; rowIdx < dataBundle.InputVectorCollection.Count; rowIdx++)
            {
                double[] computedVector = Compute(dataBundle.InputVectorCollection[rowIdx]);
                resultBundle.AddVectors(dataBundle.InputVectorCollection[rowIdx], computedVector, dataBundle.OutputVectorCollection[rowIdx]);
            }
            //Readout layer is trained and ready
            Trained = true;
            return new RegressionOverview(ClusterErrStatisticsCollection, resultBundle);
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
            for (int outputIdx = 0; outputIdx < _settings.ReadoutUnitCfgCollection.Count; outputIdx++)
            {
                sb.Append(leftMargin + $"Output field [{_settings.ReadoutUnitCfgCollection[outputIdx].Name}]: {predictedValues[outputIdx].ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
            }
            return sb.ToString();
        }

        private double Compute(double[] predictors, int clusterIdx)
        {
            string readoutUnitName = _settings.ReadoutUnitCfgCollection[clusterIdx].Name;
            double[] readoutUnitPredictors = _predictorsMapper.CreateVector(readoutUnitName, predictors);
            WeightedAvg weightedResult = new WeightedAvg();
            //Loop cluster members' predictions
            foreach (ReadoutUnit clusterMember in _clusterCollection[clusterIdx])
            {
                double computedValue = clusterMember.Network.Compute(readoutUnitPredictors)[0];
                double weight = 0d;
                if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == ReadoutUnit.TaskType.Classification)
                {
                    //Classification
                    //Training accuracy as a weight base
                    weight = 1d - clusterMember.TrainingBinErrorStat.TotalErrStat.ArithAvg;
                    if (clusterMember.TestingBinErrorStat != null && clusterMember.TestingBinErrorStat.TotalErrStat.NumOfSamples > 0)
                    {
                        //Testing results are available
                        //Combined accuracy as a resulting wight
                        weight *= 1d - clusterMember.TestingBinErrorStat.TotalErrStat.ArithAvg;
                    }
                }
                else
                {
                    //Forecast
                    //Training accuracy as a weight base
                    weight = 1d - clusterMember.TrainingErrorStat.ArithAvg;
                    if (clusterMember.TestingErrorStat != null && clusterMember.TestingErrorStat.NumOfSamples > 0)
                    {
                        //Testing results are available
                        //Combined accuracy as a resulting wight
                        weight *= 1d - clusterMember.TestingErrorStat.ArithAvg;
                    }
                }
                weightedResult.AddSampleValue(computedValue, weight);
            }
            return weightedResult.Avg;
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
        /// <param name="predictors">The predictors</param>
        public double[] Compute(double[] predictors)
        {
            //Check readyness
            if(_predictorFeatureFilterCollection == null || _outputFeatureFilterCollection == null)
            {
                throw new Exception("Readout layer is not trained. Build function has to be called before Compute function can be used.");
            }
            double[] nrmPredictors = NormalizePredictors(predictors);
            double[] outputVector = new double[_clusterCollection.Length];
            for(int clusterIdx = 0; clusterIdx < _clusterCollection.Length; clusterIdx++)
            {
                outputVector[clusterIdx] = Compute(nrmPredictors, clusterIdx);
            }
            double[] natOuputVector = NaturalizeOutputs(outputVector);
            return natOuputVector;
        }

        /// <summary>
        /// Returns rich readout data
        /// </summary>
        /// <param name="readoutLayerVector">Vector of values corresponding to readout units</param>
        public ReadoutData GetReadoutData(double[] readoutLayerVector)
        {
            return new ReadoutData(readoutLayerVector, _settings);
        }

        /// <summary>
        /// Computes readout layer rich output data
        /// </summary>
        /// <param name="predictors">The predictors</param>
        public ReadoutData ComputeReadoutData(double[] predictors)
        {
            return GetReadoutData(Compute(predictors));
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
            /// <param name="rls">Readout layer settings</param>
            public ReadoutData(double[] dataVector, ReadoutLayerSettings rls)
            {
                //Alone units
                DataVector = dataVector;
                ReadoutUnitDataCollection = new Dictionary<string, ReadoutUnitData>();
                foreach(ReadoutLayerSettings.ReadoutUnitSettings rus in rls.ReadoutUnitCfgCollection)
                {
                    ReadoutUnitDataCollection.Add(rus.Name, new ReadoutUnitData() { Name = rus.Name, Index = rus.Index, Task = rus.TaskType, DataValue = DataVector[rus.Index] });
                }
                //One Winner groups
                OneWinnerDataCollection = new Dictionary<string, OneWinnerGroupData>();
                foreach (string oneWinnerGroupName in rls.OneWinnerGroupNameCollection.Values)
                {
                    //There is One Winner group
                    int winningUnitIndex = ReadoutLayer.DecideOneWinner(oneWinnerGroupName, dataVector, rls);
                    OneWinnerDataCollection.Add(oneWinnerGroupName, new OneWinnerGroupData()
                    {
                        GroupName = oneWinnerGroupName,
                        WinningReadoutUnitName = rls.ReadoutUnitCfgCollection[winningUnitIndex].Name,
                        WinningReadoutUnitIndex = winningUnitIndex
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
        /// Overall error statistics of the cluster of readout units
        /// </summary>
        [Serializable]
        public class ClusterErrStatistics
        {
            //Property attributes
            /// <summary>
            /// Name of the readout unit
            /// </summary>
            public string ReadoutUnitName { get; }
            /// <summary>
            /// Index of the readout unit
            /// </summary>
            public int ReadoutUnitIndex { get; }
            /// <summary>
            /// Type of the solved neural task
            /// </summary>
            public ReadoutUnit.TaskType TaskType { get; }
            /// <summary>
            /// Number of computing networks within the cluster
            /// </summary>
            public int NumOfMembers { get; }
            /// <summary>
            /// Error statistics of the distance between computed and ideal valus in natural form
            /// </summary>
            public BasicStat NatPrecissionErrStat { get; }
            /// <summary>
            /// Error statistics of the distance between computed and ideal valus in normalized form
            /// </summary>
            public BasicStat NrmPrecissionErrStat { get; }
            /// <summary>
            /// Statistics of the binary errors.
            /// Relevant only for the classification task type.
            /// </summary>
            public BinErrStat BinaryErrStat { get; }

            /// <summary>
            /// Constructs an instance prepared for initialization (updates)
            /// </summary>
            /// <param name="readoutUnitName">Name of the readout unit</param>
            /// <param name="readoutUnitIndex">Index of the readout unit</param>
            /// <param name="taskType">Type of neural task</param>
            /// <param name="numOfMembers">Number of computing networks within the cluster</param>
            /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered as 0 and GE as 1.
            /// (relevant only if task type is Classification)
            /// </param>
            public ClusterErrStatistics(string readoutUnitName,
                                        int readoutUnitIndex,
                                        ReadoutUnit.TaskType taskType,
                                        int numOfMembers,
                                        double binBorder
                                        )
            {
                ReadoutUnitName = readoutUnitName;
                TaskType = taskType;
                NumOfMembers = numOfMembers;
                NatPrecissionErrStat = new BasicStat();
                NrmPrecissionErrStat = new BasicStat();
                BinaryErrStat = null;
                if (TaskType == ReadoutUnit.TaskType.Classification)
                {
                    BinaryErrStat = new BinErrStat(binBorder);
                }
                return;
            }

            /// <summary>
            /// A deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public ClusterErrStatistics(ClusterErrStatistics source)
            {
                ReadoutUnitName = source.ReadoutUnitName;
                ReadoutUnitIndex = source.ReadoutUnitIndex;
                TaskType = source.TaskType;
                NumOfMembers = source.NumOfMembers;
                NatPrecissionErrStat = new BasicStat(source.NatPrecissionErrStat);
                NrmPrecissionErrStat = new BasicStat(source.NrmPrecissionErrStat);
                BinaryErrStat = null;
                if (TaskType == ReadoutUnit.TaskType.Classification)
                {
                    BinaryErrStat = new BinErrStat(source.BinaryErrStat);
                }
                return;
            }

            /// <summary>
            /// Updates cluster statistics
            /// </summary>
            /// <param name="nrmComputedValue">Normalized value computed by the cluster</param>
            /// <param name="nrmIdealValue">Normalized ideal value</param>
            /// <param name="natComputedValue">Naturalized value computed by the cluster</param>
            /// <param name="natIdealValue">Naturalized ideal value</param>
            public void Update(double nrmComputedValue, double nrmIdealValue, double natComputedValue, double natIdealValue)
            {
                NatPrecissionErrStat.AddSampleValue(Math.Abs(natComputedValue - natIdealValue));
                NrmPrecissionErrStat.AddSampleValue(Math.Abs(nrmComputedValue - nrmIdealValue));
                if (TaskType == ReadoutUnit.TaskType.Classification)
                {
                    BinaryErrStat.Update(nrmComputedValue, nrmIdealValue);
                }
                return;
            }

            /// <summary>
            /// Creates a deep copy instance of this instance
            /// </summary>
            public ClusterErrStatistics DeepClone()
            {
                return new ClusterErrStatistics(this);
            }

        }//ClusterErrStatistics

        /// <summary>
        /// Contains results of readout layer training (regression)
        /// </summary>
        [Serializable]
        public class RegressionOverview
        {
            /// <summary>
            /// Collection of error statistics related to readout units
            /// </summary>
            public List<ClusterErrStatistics> ClusterErrStatisticsCollection { get; }

            /// <summary>
            /// Original training data together with data computed by trained readout layer
            /// </summary>
            public ResultBundle TrainingDataResultBundle { get; }

            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="clusterErrStatisticsCollection">Collection of error statistics related to readout units</param>
            /// <param name="trainingDataResultBundle">Original training data together with data computed by trained readout layer</param>
            public RegressionOverview(List<ClusterErrStatistics> clusterErrStatisticsCollection,
                                      ResultBundle trainingDataResultBundle
                                      )
            {
                ClusterErrStatisticsCollection = clusterErrStatisticsCollection;
                TrainingDataResultBundle = trainingDataResultBundle;
                return;
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
                //Training results
                for (int outputIdx = 0; outputIdx < ClusterErrStatisticsCollection.Count; outputIdx++)
                {
                    ReadoutLayer.ClusterErrStatistics ces = ClusterErrStatisticsCollection[outputIdx];
                    sb.Append(leftMargin + $"Output field [{ces.ReadoutUnitName}]" + Environment.NewLine);
                    if (ces.TaskType == ReadoutUnit.TaskType.Classification)
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
                }
                return sb.ToString();
            }

        }//RegressionOverview

    }//ReadoutLayer

}//Namespace
