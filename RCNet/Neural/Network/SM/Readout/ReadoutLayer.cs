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
        /// Data range
        /// </summary>
        private readonly Interval _dataRange;
        /// <summary>
        /// Mapping of specific predictors to readout units
        /// </summary>
        private PredictorsMapper _predictorsMapper;
        /// <summary>
        /// Maximum number of the folds
        /// </summary>
        public const int MaxNumOfFolds = 100;
        /// <summary>
        /// Maximum part of available samples useable for test purposes
        /// </summary>
        public const double MaxRatioOfTestData = 1d/3d;
        /// <summary>
        /// Minimum length of the test dataset
        /// </summary>
        public const int MinLengthOfTestDataset = 2;
        //Attributes
        /// <summary>
        /// Readout layer configuration
        /// </summary>
        private ReadoutLayerSettings _settings;
        /// <summary>
        /// Collection of clusters of trained ReadoutUnits. One cluster per output field.
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
        /// <param name="dataRange">Range of input/output data</param>
        public ReadoutLayer(ReadoutLayerSettings settings, Interval dataRange)
        {
            _settings = settings.DeepClone();
            _dataRange = dataRange.DeepClone();
            _predictorsMapper = null;
            foreach (ReadoutLayerSettings.ReadoutUnitSettings rus in _settings.ReadoutUnitCfgCollection)
            {
                if (!rus.OutputRange.BelongsTo(_dataRange.Min) || !rus.OutputRange.BelongsTo(_dataRange.Max))
                {
                    throw new Exception($"Readout unit {rus.Name} does not support data range <{_dataRange.Min}; {_dataRange.Max}>.");
                }
            }
            //Clusters
            _clusterCollection = new ReadoutUnit[_settings.ReadoutUnitCfgCollection.Count][];
            _clusterErrStatisticsCollection = new List<ClusterErrStatistics>();
            return;
        }

        /// <summary>
        /// Builds readout layer.
        /// Prepares prediction clusters containing trained readout units.
        /// </summary>
        /// <param name="predictorsCollection">Collection of predictors</param>
        /// <param name="idealOutputsCollection">Collection of desired outputs related to predictors</param>
        /// <param name="regressionController">Regression controller delegate</param>
        /// <param name="regressionControllerData">An user object</param>
        /// <param name="predictorsMapper">Optional specific mapping of predictors to readout units</param>
        /// <returns>Returned ResultComparativeBundle is something like a protocol.
        /// There is recorded fold by fold (unit by unit) predicted and corresponding ideal values.
        /// This is the pesimistic approach. Real results on unseen data could be better due to the clustering synergy.
        /// </returns>
        public ResultComparativeBundle Build(List<double[]> predictorsCollection,
                                             List<double[]> idealOutputsCollection,
                                             ReadoutUnit.RegressionCallbackDelegate regressionController,
                                             Object regressionControllerData,
                                             PredictorsMapper predictorsMapper = null
                                             )
        {
            //Random object
            Random rand = new Random(0);
            //Predictors mapper (specified or default)
            _predictorsMapper = predictorsMapper ?? new PredictorsMapper(predictorsCollection[0].Length);
            //Allocation of computed and ideal vectors for result comparative bundle
            List<double[]> validationComputedVectorCollection = new List<double[]>(idealOutputsCollection.Count);
            List<double[]> validationIdealVectorCollection = new List<double[]>(idealOutputsCollection.Count);
            for (int i = 0; i < idealOutputsCollection.Count; i++)
            {
                validationComputedVectorCollection.Add(new double[idealOutputsCollection[0].Length]);
                validationIdealVectorCollection.Add(new double[idealOutputsCollection[0].Length]);
            }
            //Test dataset size
            if (_settings.TestDataRatio > MaxRatioOfTestData)
            {
                throw new ArgumentException($"Test dataset size is greater than {MaxRatioOfTestData.ToString(CultureInfo.InvariantCulture)}", "TestDataSetSize");
            }
            int testDataSetLength = (int)Math.Round(idealOutputsCollection.Count * _settings.TestDataRatio, 0);
            if (testDataSetLength < MinLengthOfTestDataset)
            {
                throw new ArgumentException($"Num of test samples is less than {MinLengthOfTestDataset.ToString(CultureInfo.InvariantCulture)}", "TestDataSetSize");
            }
            //Number of folds
            int numOfFolds = _settings.NumOfFolds;
            if (numOfFolds <= 0)
            {
                //Auto setup
                numOfFolds = idealOutputsCollection.Count / testDataSetLength;
                if (numOfFolds > MaxNumOfFolds)
                {
                    numOfFolds = MaxNumOfFolds;
                }
            }
            //Create shuffled copy of the data
            VectorBundle shuffledData = new VectorBundle(predictorsCollection, idealOutputsCollection);
            shuffledData.Shuffle(rand);
            //Data inspection, preparation of datasets and training of ReadoutUnits
            //Clusters of readout units (one cluster for each output field)
            for (int clusterIdx = 0; clusterIdx < _settings.ReadoutUnitCfgCollection.Count; clusterIdx++)
            {
                _clusterCollection[clusterIdx] = new ReadoutUnit[numOfFolds];
                List<double[]> idealValueCollection = new List<double[]>(idealOutputsCollection.Count);
                BinDistribution refBinDistr = null;
                if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == CommonEnums.TaskType.Classification)
                {
                    //Reference binary distribution is relevant only for classification task
                    refBinDistr = new BinDistribution(_dataRange.Mid);
                }
                //Transformation to a single value vectors and data analysis
                foreach (double[] idealVector in shuffledData.OutputVectorCollection)
                {
                    double[] value = new double[1];
                    value[0] = idealVector[clusterIdx];
                    idealValueCollection.Add(value);
                    if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == CommonEnums.TaskType.Classification)
                    {
                        //Reference binary distribution is relevant only for classification task
                        refBinDistr.Update(value);
                    }
                }
                List<VectorBundle> subBundleCollection = null;
                List<double[]> readoutUnitInputVectorCollection = _predictorsMapper.CreateVectorCollection(_settings.ReadoutUnitCfgCollection[clusterIdx].Name, shuffledData.InputVectorCollection);
                //Datasets preparation is depending on the task type
                if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == CommonEnums.TaskType.Classification)
                {
                    //Classification task
                    subBundleCollection = DivideSamplesForClassificationTask(readoutUnitInputVectorCollection,
                                                                             idealValueCollection,
                                                                             refBinDistr,
                                                                             testDataSetLength
                                                                             );
                }
                else
                {
                    //Forecast task
                    subBundleCollection = DivideSamplesForForecastTask(readoutUnitInputVectorCollection,
                                                                       idealValueCollection,
                                                                       testDataSetLength
                                                                       );
                }
                //Best predicting unit per each fold in the cluster.
                ClusterErrStatistics ces = new ClusterErrStatistics(_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType, numOfFolds, refBinDistr);
                int arrayPos = 0;
                for (int foldIdx = 0; foldIdx < numOfFolds; foldIdx++)
                {
                    //Build training samples
                    List<double[]> trainingPredictorsCollection = new List<double[]>();
                    List<double[]> trainingIdealValueCollection = new List<double[]>();
                    for (int bundleIdx = 0; bundleIdx < subBundleCollection.Count; bundleIdx++)
                    {
                        if (bundleIdx != foldIdx)
                        {
                            trainingPredictorsCollection.AddRange(subBundleCollection[bundleIdx].InputVectorCollection);
                            trainingIdealValueCollection.AddRange(subBundleCollection[bundleIdx].OutputVectorCollection);
                        }
                    }
                    //Call training regression to get the best fold's readout unit.
                    //The best unit becomes to be the predicting cluster member.
                    _clusterCollection[clusterIdx][foldIdx] = ReadoutUnit.CreateTrained(_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType,
                                                                                        clusterIdx,
                                                                                        foldIdx + 1,
                                                                                        numOfFolds,
                                                                                        refBinDistr,
                                                                                        trainingPredictorsCollection,
                                                                                        trainingIdealValueCollection,
                                                                                        subBundleCollection[foldIdx].InputVectorCollection,
                                                                                        subBundleCollection[foldIdx].OutputVectorCollection,
                                                                                        rand,
                                                                                        _settings.ReadoutUnitCfgCollection[clusterIdx],
                                                                                        regressionController,
                                                                                        regressionControllerData
                                                                                        );
                    //Cluster error statistics & data for validation bundle (pesimistic approach)
                    for (int sampleIdx = 0; sampleIdx < subBundleCollection[foldIdx].OutputVectorCollection.Count; sampleIdx++)
                    {
                        
                        double value = _clusterCollection[clusterIdx][foldIdx].Network.Compute(subBundleCollection[foldIdx].InputVectorCollection[sampleIdx])[0];
                        ces.Update(value, subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][0]);
                        validationIdealVectorCollection[arrayPos][clusterIdx] = subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][0];
                        validationComputedVectorCollection[arrayPos][clusterIdx] = value;
                        ++arrayPos;
                    }

                }//foldIdx
                _clusterErrStatisticsCollection.Add(ces);

            }//clusterIdx
            //Validation bundle is returned. 
            return new ResultComparativeBundle(validationComputedVectorCollection, validationIdealVectorCollection);
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
                List<ClusterErrStatistics> clone = new List<ClusterErrStatistics>(_clusterErrStatisticsCollection.Count);
                foreach(ClusterErrStatistics ces in _clusterErrStatisticsCollection)
                {
                    clone.Add(ces.DeepClone());
                }
                return clone;
            }
        }

        //Methods
        private double Compute(double[] predictors, int clusterIdx)
        {
            WeightedAvg wAvg = new WeightedAvg();
            string readoutUnitName = _settings.ReadoutUnitCfgCollection[clusterIdx].Name;
            for (int readoutUnitIdx = 0; readoutUnitIdx < _clusterCollection[clusterIdx].Length; readoutUnitIdx++)
            {
                double[] outputValue = _clusterCollection[clusterIdx][readoutUnitIdx].Network.Compute(_predictorsMapper.CreateVector(readoutUnitName, predictors));
                double weight = _clusterCollection[clusterIdx][readoutUnitIdx].TrainingErrorStat.NumOfSamples;
                if(_clusterCollection[clusterIdx][readoutUnitIdx].TestingErrorStat != null)
                {
                    weight += _clusterCollection[clusterIdx][readoutUnitIdx].TestingErrorStat.NumOfSamples;
                }
                wAvg.AddSampleValue(outputValue[0], weight);
                // Or flat weight
                //wAvg.AddSampleValue(outputValue[0], 1);
            }
            return wAvg.Avg;
        }

        /// <summary>
        /// Computes output fields
        /// </summary>
        /// <param name="predictors">The predictors</param>
        public double[] Compute(double[] predictors)
        {
            double[] outputVector = new double[_clusterCollection.Length];
            for(int clusterIdx = 0; clusterIdx < _clusterCollection.Length; clusterIdx++)
            {
                outputVector[clusterIdx] = Compute(predictors, clusterIdx);
            }
            return outputVector;
        }
        
        private List<VectorBundle> DivideSamplesForClassificationTask(List<double[]> predictorsCollection,
                                                                      List<double[]> idealValueCollection,
                                                                      BinDistribution refBinDistr,
                                                                      int bundleSize
                                                                      )
        {
            int numOfBundles = idealValueCollection.Count / bundleSize;
            List<VectorBundle> bundleCollection = new List<VectorBundle>(numOfBundles);
            //Scan
            int[] bin0SampleIdxs = new int[refBinDistr.NumOf[0]];
            int bin0SamplesPos = 0;
            int[] bin1SampleIdxs = new int[refBinDistr.NumOf[1]];
            int bin1SamplesPos = 0;
            for (int i = 0; i < idealValueCollection.Count; i++)
            {
                if(idealValueCollection[i][0] >= refBinDistr.BinBorder)
                {
                    bin1SampleIdxs[bin1SamplesPos++] = i;
                }
                else
                {
                    bin0SampleIdxs[bin0SamplesPos++] = i;
                }
            }
            //Division
            int bundleBin0Count = Math.Max(1, refBinDistr.NumOf[0] / numOfBundles);
            int bundleBin1Count = Math.Max(1, refBinDistr.NumOf[1] / numOfBundles);
            if(bundleBin0Count * numOfBundles > bin0SampleIdxs.Length)
            {
                throw new Exception("Insufficient bin 0 samples");
            }
            if (bundleBin1Count * numOfBundles > bin1SampleIdxs.Length)
            {
                throw new Exception("Insufficient bin 1 samples");
            }
            //Bundles creation
            bin0SamplesPos = 0;
            bin1SamplesPos = 0;
            for(int bundleNum = 0; bundleNum < numOfBundles; bundleNum++)
            {
                VectorBundle bundle = new VectorBundle();
                //Bin 0
                for (int i = 0; i < bundleBin0Count; i++)
                {
                    bundle.InputVectorCollection.Add(predictorsCollection[bin0SampleIdxs[bin0SamplesPos]]);
                    bundle.OutputVectorCollection.Add(idealValueCollection[bin0SampleIdxs[bin0SamplesPos]]);
                    ++bin0SamplesPos;
                }
                //Bin 1
                for (int i = 0; i < bundleBin1Count; i++)
                {
                    bundle.InputVectorCollection.Add(predictorsCollection[bin1SampleIdxs[bin1SamplesPos]]);
                    bundle.OutputVectorCollection.Add(idealValueCollection[bin1SampleIdxs[bin1SamplesPos]]);
                    ++bin1SamplesPos;
                }
                bundleCollection.Add(bundle);
            }
            //Remaining samples
            for(int i = 0; i < bin0SampleIdxs.Length - bin0SamplesPos; i++)
            {
                int bundleIdx = i % bundleCollection.Count;
                bundleCollection[bundleIdx].InputVectorCollection.Add(predictorsCollection[bin0SampleIdxs[bin0SamplesPos + i]]);
                bundleCollection[bundleIdx].OutputVectorCollection.Add(idealValueCollection[bin0SampleIdxs[bin0SamplesPos + i]]);
            }
            for (int i = 0; i < bin1SampleIdxs.Length - bin1SamplesPos; i++)
            {
                int bundleIdx = i % bundleCollection.Count;
                bundleCollection[bundleIdx].InputVectorCollection.Add(predictorsCollection[bin1SampleIdxs[bin1SamplesPos + i]]);
                bundleCollection[bundleIdx].OutputVectorCollection.Add(idealValueCollection[bin1SampleIdxs[bin1SamplesPos + i]]);
            }
            return bundleCollection;
        }

        private List<VectorBundle> DivideSamplesForForecastTask(List<double[]> predictorsCollection,
                                                                List<double[]> idealValueCollection,
                                                                int bundleSize
                                                                )
        {
            int numOfBundles = idealValueCollection.Count / bundleSize;
            List<VectorBundle> bundleCollection = new List<VectorBundle>(numOfBundles);
            //Bundles creation
            int samplesPos = 0;
            for (int bundleNum = 0; bundleNum < numOfBundles; bundleNum++)
            {
                VectorBundle bundle = new VectorBundle();
                for (int i = 0; i < bundleSize && samplesPos < idealValueCollection.Count; i++)
                {
                    bundle.InputVectorCollection.Add(predictorsCollection[samplesPos]);
                    bundle.OutputVectorCollection.Add(idealValueCollection[samplesPos]);
                    ++samplesPos;
                }
                bundleCollection.Add(bundle);
            }
            //Remaining samples
            for (int i = 0; i < idealValueCollection.Count - samplesPos; i++)
            {
                int bundleIdx = i % bundleCollection.Count;
                bundleCollection[bundleIdx].InputVectorCollection.Add(predictorsCollection[samplesPos + i]);
                bundleCollection[bundleIdx].OutputVectorCollection.Add(idealValueCollection[samplesPos + i]);
            }
            return bundleCollection;
        }

        //Inner classes
        /// <summary>
        /// Maps specific predictors to readout units
        /// </summary>
        [Serializable]
        public class PredictorsMapper
        {
            /// <summary>
            /// Number of all available predictors
            /// </summary>
            private readonly int _numOfPredictors;
            /// <summary>
            /// Mapping of readout unit to switches determining what predictors are assigned to.
            /// </summary>
            private readonly Dictionary<string, ReadoutUnitMap> _mapCollection;

            /// <summary>
            /// Creates uninitialized instance
            /// </summary>
            /// <param name="numOfPredictors">Total number of available predictors</param>
            public PredictorsMapper(int numOfPredictors)
            {
                _numOfPredictors = numOfPredictors;
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
                if(map.Length != _numOfPredictors)
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
                ReadoutUnitMap rum = new ReadoutUnitMap(map);
                if(rum.VectorLength == 0)
                {
                    throw new ArgumentException("Map does not contain mapped predictors", "map");
                }
                _mapCollection.Add(readoutUnitName, rum);
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
                try
                {
                    ReadoutUnitMap rum = _mapCollection[readoutUnitName];
                    return CreateVector(predictors, rum.Map, rum.VectorLength);
                }
                catch
                {
                    if (predictors.Length != _numOfPredictors)
                    {
                        throw new ArgumentException("Incorrect number of predictors", "predictors");
                    }
                    return (double[])predictors.Clone();
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
                        vectorCollection.Add((double[])predictors.Clone());
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
                    Map = (bool[])map.Clone();
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
            /// Type of the solved neural task
            /// </summary>
            public CommonEnums.TaskType TaskType { get; }
            /// <summary>
            /// Number of readout units within the cluster
            /// </summary>
            public int NumOfReadoutUnits { get; }
            /// <summary>
            /// Error statistics of the distance between computed and ideal value
            /// </summary>
            public BasicStat PrecissionErrStat { get; }
            /// <summary>
            /// Statistics of the binary errors.
            /// Relevant only for the classification task type.
            /// </summary>
            public BinErrStat BinaryErrStat { get; }

            /// <summary>
            /// Constructs an instance prepared for initialization (updates)
            /// </summary>
            /// <param name="taskType"></param>
            /// <param name="numOfReadoutUnits"></param>
            /// <param name="refBinDistr"></param>
            public ClusterErrStatistics(CommonEnums.TaskType taskType, int numOfReadoutUnits, BinDistribution refBinDistr)
            {
                TaskType = taskType;
                NumOfReadoutUnits = numOfReadoutUnits;
                PrecissionErrStat = new BasicStat();
                BinaryErrStat = null;
                if (TaskType == CommonEnums.TaskType.Classification)
                {
                    BinaryErrStat = new BinErrStat(refBinDistr);
                }
                return;
            }

            /// <summary>
            /// A deep copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public ClusterErrStatistics(ClusterErrStatistics source)
            {
                TaskType = source.TaskType;
                NumOfReadoutUnits = source.NumOfReadoutUnits;
                PrecissionErrStat = new BasicStat(source.PrecissionErrStat);
                BinaryErrStat = null;
                if (TaskType == CommonEnums.TaskType.Classification)
                {
                    BinaryErrStat = new BinErrStat(source.BinaryErrStat);
                }
                return;
            }

            /// <summary>
            /// Updates cluster statistics
            /// </summary>
            /// <param name="computedValue">Value computed by the cluster</param>
            /// <param name="idealValue">Ideal value</param>
            public void Update(double computedValue, double idealValue)
            {
                PrecissionErrStat.AddSampleValue(Math.Abs(computedValue - idealValue));
                if (TaskType == CommonEnums.TaskType.Classification)
                {
                    BinaryErrStat.Update(computedValue, idealValue);
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

    }//ReadoutLayer

}//Namespace
