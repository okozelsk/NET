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

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// Class implements the common readout layer for the reservoir computing methods
    /// </summary>
    [Serializable]
    public class ReadoutLayer
    {
        //Constants
        /// <summary>
        /// Maximum number of the folds
        /// </summary>
        public const int MaxNumOfFolds = 100;
        /// <summary>
        /// Maximum part of available samples useable for test purposes
        /// </summary>
        public const double MaxRatioOfTestData = 0.3333333333333333d;
        /// <summary>
        /// Minimum length of the test dataset
        /// </summary>
        public const int MinLengthOfTestDataset = 2;
        //Attributes
        /// <summary>
        /// Type of the task
        /// </summary>
        private CommonEnums.TaskType _taskType;
        /// <summary>
        /// Readout layer configuration
        /// </summary>
        private ReadoutLayerSettings _settings;
        /// <summary>
        /// Random generator
        /// </summary>
        private System.Random _rand;
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
        /// <param name="taskType">Type of the task</param>
        /// <param name="settings">Readout layer configuration</param>
        /// <param name="rand">Random object to be used</param>
        public ReadoutLayer(CommonEnums.TaskType taskType,
                            ReadoutLayerSettings settings,
                            System.Random rand
                            )
        {
            _taskType = taskType;
            _settings = settings.DeepClone();
            _rand = rand;
            _clusterCollection = new ReadoutUnit[_settings.OutputFieldNameCollection.Count][];
            _clusterErrStatisticsCollection = new List<ClusterErrStatistics>();
            return;
        }

        /// <summary>
        /// Builds readout layer.
        /// Prepares prediction clusters containing trained readout units.
        /// </summary>
        /// <param name="predictorsCollection">Collection of all available predictors</param>
        /// <param name="idealOutputsCollection">Collection of all available desired outputs related to predictors</param>
        /// <param name="regressionController">Regression controller delegate</param>
        /// <param name="regressionControllerData">An user object</param>
        public ValidationBundle Build(List<double[]> predictorsCollection,
                                      List<double[]> idealOutputsCollection,
                                      ReadoutUnit.RegressionCallbackDelegate regressionController,
                                      Object regressionControllerData
                                      )
        {
            //Allocation of computed vectors for validation bundle
            List<double[]> computedVectorCollection = new List<double[]>(idealOutputsCollection.Count);
            for(int i = 0; i < idealOutputsCollection.Count; i++)
            {
                computedVectorCollection.Add(new double[idealOutputsCollection[0].Length]);
            }
            //Test dataset size
            if (_settings.TestDataRatio > MaxRatioOfTestData)
            {
                throw new ArgumentException($"Test dataset size is greater than {MaxRatioOfTestData.ToString(CultureInfo.InvariantCulture)}", "TestDataSetSize");
            }
            int testDataSetLength = (int)Math.Round(idealOutputsCollection.Count * _settings.TestDataRatio);
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
            TimeSeriesBundle shuffledData = new TimeSeriesBundle(predictorsCollection, idealOutputsCollection);
            shuffledData.Shuffle(_rand);
            //Data inspection, preparation of datasets and training of ReadoutUnits
            //Clusters of readout units (one cluster for each output field)
            for (int clusterIdx = 0; clusterIdx < _settings.OutputFieldNameCollection.Count; clusterIdx++)
            {
                _clusterCollection[clusterIdx] = new ReadoutUnit[numOfFolds];
                List<double[]> idealValueCollection = new List<double[]>(idealOutputsCollection.Count);
                BinDistribution refBinDistr = null;
                if (_taskType == CommonEnums.TaskType.Classification)
                {
                    //Reference binary distribution is relevant only for classification task
                    refBinDistr = new BinDistribution();
                }
                //Transformation to a single value vectors and data analysis
                foreach (double[] idealVector in shuffledData.OutputVectorCollection)
                {
                    double[] value = new double[1];
                    value[0] = idealVector[clusterIdx];
                    idealValueCollection.Add(value);
                    if (_taskType == CommonEnums.TaskType.Classification)
                    {
                        //Reference binary distribution is relevant only for classification task
                        refBinDistr.Update(value);
                    }
                }
                List<TimeSeriesBundle> subBundleCollection = null;
                //Datasets preparation is depending on the task type
                if (_taskType == CommonEnums.TaskType.Classification)
                {
                    //Classification task
                    subBundleCollection = DivideSamplesForClassification(shuffledData.InputVectorCollection,
                                                                         idealValueCollection,
                                                                         refBinDistr,
                                                                         testDataSetLength
                                                                         );
                }
                else
                {
                    //Prediction or Hybrid task
                    subBundleCollection = DivideSamplesForPredictionOrHybrid(shuffledData.InputVectorCollection,
                                                                             idealValueCollection,
                                                                             testDataSetLength
                                                                             );
                }
                //Readout units in the cluster
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
                    //Call training regression for the single readout unit
                    _clusterCollection[clusterIdx][foldIdx] = ReadoutUnit.CreateTrained(_taskType,
                                                                                        clusterIdx,
                                                                                        _settings.OutputFieldNameCollection[clusterIdx],
                                                                                        foldIdx + 1,
                                                                                        numOfFolds,
                                                                                        refBinDistr,
                                                                                        trainingPredictorsCollection,
                                                                                        trainingIdealValueCollection,
                                                                                        subBundleCollection[foldIdx].InputVectorCollection,
                                                                                        subBundleCollection[foldIdx].OutputVectorCollection,
                                                                                        _rand,
                                                                                        _settings.ReadoutUnitCfg,
                                                                                        regressionController,
                                                                                        regressionControllerData
                                                                                        );
                }//foldIdx
                //Cluster error statistics & data for validation bundle
                ClusterErrStatistics ces = new ClusterErrStatistics(_taskType, numOfFolds, refBinDistr);
                for (int sampleIdx = 0; sampleIdx < idealOutputsCollection.Count; sampleIdx++)
                {
                    double value = Compute(predictorsCollection[sampleIdx], clusterIdx);
                    ces.Update(value, idealOutputsCollection[sampleIdx][clusterIdx]);
                    computedVectorCollection[sampleIdx][clusterIdx] = value;
                }
                _clusterErrStatisticsCollection.Add(ces);

            }//clusterIdx

            return new ValidationBundle(computedVectorCollection, idealOutputsCollection);
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
            for (int readoutUnitIdx = 0; readoutUnitIdx < _clusterCollection[clusterIdx].Length; readoutUnitIdx++)
            {
                double[] outputValue = _clusterCollection[clusterIdx][readoutUnitIdx].Network.Compute(predictors);
                double weight = _clusterCollection[clusterIdx][readoutUnitIdx].TrainingErrorStat.NumOfSamples;
                if(_clusterCollection[clusterIdx][readoutUnitIdx].TestingErrorStat != null)
                {
                    weight += _clusterCollection[clusterIdx][readoutUnitIdx].TestingErrorStat.NumOfSamples;
                }
                wAvg.AddSampleValue(outputValue[0], weight);
                // Or Flat weights
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
        
        private List<TimeSeriesBundle> DivideSamplesForClassification(List<double[]> predictorsCollection,
                                                                       List<double[]> idealValueCollection,
                                                                       BinDistribution refBinDistr,
                                                                       int bundleSize
                                                                       )
        {
            int numOfBundles = idealValueCollection.Count / bundleSize;
            List<TimeSeriesBundle> bundleCollection = new List<TimeSeriesBundle>(numOfBundles);
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
                TimeSeriesBundle bundle = new TimeSeriesBundle();
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

        private List<TimeSeriesBundle> DivideSamplesForPredictionOrHybrid(List<double[]> predictorsCollection,
                                                                          List<double[]> idealValueCollection,
                                                                          int bundleSize
                                                                          )
        {
            int numOfBundles = idealValueCollection.Count / bundleSize;
            List<TimeSeriesBundle> bundleCollection = new List<TimeSeriesBundle>(numOfBundles);
            //Bundles creation
            int samplesPos = 0;
            for (int bundleNum = 0; bundleNum < numOfBundles; bundleNum++)
            {
                TimeSeriesBundle bundle = new TimeSeriesBundle();
                for (int i = 0; i < bundleSize; i++)
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
