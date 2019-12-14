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
        /// Maximum number of the folds
        /// </summary>
        public const int MaxNumOfFolds = 100;
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
        private readonly ReadoutUnit[][] _clusterCollection;
        /// <summary>
        /// Cluster overall error statistics collection
        /// </summary>
        private readonly List<ClusterErrStatistics> _clusterErrStatisticsCollection;



        //Constructor
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        /// <param name="settings">Readout layer configuration</param>
        public ReadoutLayer(ReadoutLayerSettings settings)
        {
            _settings = settings.DeepClone();
            _predictorFeatureFilterCollection = null;
            _outputFeatureFilterCollection = null;
            _predictorsMapper = null;
            foreach (ReadoutLayerSettings.ReadoutUnitSettings rus in _settings.ReadoutUnitCfgCollection)
            {
                if (!rus.OutputRange.BelongsTo(DataRange.Min) || !rus.OutputRange.BelongsTo(DataRange.Max))
                {
                    throw new Exception($"Readout unit {rus.Name} does not support data range <{DataRange.Min}; {DataRange.Max}>.");
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
        /// <param name="dataBundle">Collection of input predictors and associated desired output values</param>
        /// <param name="regressionController">Regression controller delegate</param>
        /// <param name="regressionControllerData">An user object</param>
        /// <param name="predictorsMapper">Optional specific mapping of predictors to readout units</param>
        /// <returns>Returned ResultComparativeBundle is something like a protocol.
        /// There is recorded fold by fold (unit by unit) predicted and corresponding ideal values.
        /// This is the pesimistic approach. Real results on unseen data could be better due to the clustering.
        /// </returns>
        public ResultBundle Build(VectorBundle dataBundle,
                                  ReadoutUnit.RegressionCallbackDelegate regressionController,
                                  Object regressionControllerData,
                                  PredictorsMapper predictorsMapper = null
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
                throw new Exception("Incorrect number of ideal output values in the vector.");
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
            double[][] predictorsCollection = new double[dataBundle.InputVectorCollection.Count][];
            double[][] idealOutputsCollection = new double[dataBundle.OutputVectorCollection.Count][];
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
                predictorsCollection[pairIdx] = predictors;
                //Outputs
                double[] outputs = new double[numOfOutputs];
                for (int i = 0; i < numOfOutputs; i++)
                {
                    outputs[i] = _outputFeatureFilterCollection[i].ApplyFilter(dataBundle.OutputVectorCollection[pairIdx][i]);
                }
                idealOutputsCollection[pairIdx] = outputs;
            });

            //Data processing
            //Random object initialization
            Random rand = new Random(0);
            //Test dataset size
            if (_settings.TestDataRatio > MaxRatioOfTestData)
            {
                throw new ArgumentException($"Test dataset size is greater than {MaxRatioOfTestData.ToString(CultureInfo.InvariantCulture)}", "TestDataSetSize");
            }
            int testDataSetLength = (int)Math.Round(idealOutputsCollection.Length * _settings.TestDataRatio, 0);
            if (testDataSetLength < MinLengthOfTestDataset)
            {
                throw new ArgumentException($"Num of test samples is less than {MinLengthOfTestDataset.ToString(CultureInfo.InvariantCulture)}", "TestDataSetSize");
            }
            //Number of folds
            int numOfFolds = _settings.NumOfFolds;
            if (numOfFolds <= 0)
            {
                //Auto setup
                numOfFolds = idealOutputsCollection.Length / testDataSetLength;
                if (numOfFolds > MaxNumOfFolds)
                {
                    numOfFolds = MaxNumOfFolds;
                }
            }
            //Create shuffled copy of the data
            VectorBundle shuffledData = new VectorBundle(predictorsCollection, idealOutputsCollection);
            shuffledData.Shuffle(rand);
            //Data inspection, preparation of datasets and training of ReadoutUnits
            //Clusters of readout units (one cluster per each output field)
            for (int clusterIdx = 0; clusterIdx < _settings.ReadoutUnitCfgCollection.Count; clusterIdx++)
            {
                _clusterCollection[clusterIdx] = new ReadoutUnit[numOfFolds];
                List<double[]> idealValueCollection = new List<double[]>(idealOutputsCollection.Length);
                BinDistribution refBinDistr = null;
                if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == ReadoutUnit.TaskType.Classification)
                {
                    //Reference binary distribution is relevant only for classification task
                    refBinDistr = new BinDistribution(DataRange.Mid);
                }
                //Transformation to a single value vectors and data analysis
                foreach (double[] idealVector in shuffledData.OutputVectorCollection)
                {
                    double[] value = new double[1];
                    value[0] = idealVector[clusterIdx];
                    idealValueCollection.Add(value);
                    if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == ReadoutUnit.TaskType.Classification)
                    {
                        //Reference binary distribution is relevant only for classification task
                        refBinDistr.Update(value);
                    }
                }
                List<VectorBundle> subBundleCollection = null;
                List<double[]> readoutUnitInputVectorCollection = _predictorsMapper.CreateVectorCollection(_settings.ReadoutUnitCfgCollection[clusterIdx].Name, shuffledData.InputVectorCollection);
                //Datasets preparation is depending on the task type
                if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == ReadoutUnit.TaskType.Classification)
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
                //Find best unit per each fold in the cluster.
                ClusterErrStatistics ces = new ClusterErrStatistics(_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType, numOfFolds, DataRange.Mid);
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
                                                                                        DataRange.Mid,
                                                                                        trainingPredictorsCollection,
                                                                                        trainingIdealValueCollection,
                                                                                        subBundleCollection[foldIdx].InputVectorCollection,
                                                                                        subBundleCollection[foldIdx].OutputVectorCollection,
                                                                                        rand,
                                                                                        _settings.ReadoutUnitCfgCollection[clusterIdx],
                                                                                        regressionController,
                                                                                        regressionControllerData
                                                                                        );
                    //Cluster error statistics (pesimistic approach)
                    for (int sampleIdx = 0; sampleIdx < subBundleCollection[foldIdx].OutputVectorCollection.Count; sampleIdx++)
                    {
                        
                        double nrmComputedValue = _clusterCollection[clusterIdx][foldIdx].Network.Compute(subBundleCollection[foldIdx].InputVectorCollection[sampleIdx])[0];
                        double natComputedValue = _outputFeatureFilterCollection[clusterIdx].ApplyReverse(nrmComputedValue);
                        double natIdealValue = _outputFeatureFilterCollection[clusterIdx].ApplyReverse(subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][0]);
                        ces.Update(nrmComputedValue,
                                   subBundleCollection[foldIdx].OutputVectorCollection[sampleIdx][0],
                                   natComputedValue,
                                   natIdealValue);
                        ++arrayPos;
                    }

                }//foldIdx
                _clusterErrStatisticsCollection.Add(ces);

            }//clusterIdx
            //Result bundle to be returned (perform full recomputation - optimistic approach)
            ResultBundle resultBundle = new ResultBundle(dataBundle.InputVectorCollection.Count);
            for(int rowIdx = 0; rowIdx < dataBundle.InputVectorCollection.Count; rowIdx++)
            {
                double[] computedVector = Compute(dataBundle.InputVectorCollection[rowIdx]);
                resultBundle.AddVectors(dataBundle.InputVectorCollection[rowIdx], computedVector, dataBundle.OutputVectorCollection[rowIdx]);
            }
            return resultBundle;
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
                foreach(ClusterErrStatistics ces in _clusterErrStatisticsCollection)
                {
                    clonedStatisticsCollection.Add(ces.DeepClone());
                }
                return clonedStatisticsCollection;
            }
        }

        //Static methods
        /// <summary>
        /// Builds report string containing information about the regression progress.
        /// It is usually called from the RegressionControl user implementation.
        /// </summary>
        /// <param name="inArgs">>Contains all the necessary information to control the regression.</param>
        /// <param name="bestReadoutUnit">Currently the best readout unit.</param>
        /// <param name="margin">Specifies how many spaces to be at the begining of the row.</param>
        /// <returns>Built text report</returns>
        public static string GetProgressReport(ReadoutUnit.RegressionControlInArgs inArgs,
                                               ReadoutUnit bestReadoutUnit,
                                               int margin = 0
                                               )
        {
            //Build progress text message
            StringBuilder progressText = new StringBuilder();
            progressText.Append(new string(' ', margin));
            progressText.Append("OutputField: ");
            progressText.Append(inArgs.OutputFieldName);
            progressText.Append(", Fold/Attempt/Epoch: ");
            progressText.Append(inArgs.FoldNum.ToString().PadLeft(inArgs.NumOfFolds.ToString().Length, '0') + "/");
            progressText.Append(inArgs.RegrAttemptNumber.ToString().PadLeft(inArgs.RegrMaxAttempts.ToString().Length, '0') + "/");
            progressText.Append(inArgs.Epoch.ToString().PadLeft(inArgs.MaxEpochs.ToString().Length, '0'));
            progressText.Append(", DSet-Sizes: (");
            progressText.Append(inArgs.CurrReadoutUnit.TrainingErrorStat.NumOfSamples.ToString() + ", ");
            progressText.Append(inArgs.CurrReadoutUnit.TestingErrorStat.NumOfSamples.ToString() + ")");
            progressText.Append(", Best-Train: ");
            progressText.Append(bestReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
            if (inArgs.TaskType == ReadoutUnit.TaskType.Classification)
            {
                //Append binary errors
                progressText.Append("/" + bestReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + bestReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
            }
            progressText.Append(", Best-Test: ");
            progressText.Append(bestReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
            if (inArgs.TaskType == ReadoutUnit.TaskType.Classification)
            {
                //Append binary errors
                progressText.Append("/" + bestReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + bestReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
            }
            progressText.Append(", Curr-Train: ");
            progressText.Append(inArgs.CurrReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
            if (inArgs.TaskType == ReadoutUnit.TaskType.Classification)
            {
                //Append binary errors
                progressText.Append("/" + inArgs.CurrReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + inArgs.CurrReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
            }
            progressText.Append(", Curr-Test: ");
            progressText.Append(inArgs.CurrReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
            if (inArgs.TaskType == ReadoutUnit.TaskType.Classification)
            {
                //Append binary errors
                progressText.Append("/" + inArgs.CurrReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                progressText.Append("/" + inArgs.CurrReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
            }
            progressText.Append($" [{bestReadoutUnit.TrainerInfoMessage}]");
            return progressText.ToString();
        }

        //Methods
        /// <summary>
        /// Returns results of the readout units training
        /// </summary>
        /// <param name="margin">Specifies how many spaces should be at the begining of each row.</param>
        /// <returns>Built text report</returns>
        public string GetTrainingResultsReport(int margin)
        {
            string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
            StringBuilder sb = new StringBuilder();
            //Training results
            for (int outputIdx = 0; outputIdx < _settings.ReadoutUnitCfgCollection.Count; outputIdx++)
            {
                ReadoutLayer.ClusterErrStatistics ces = _clusterErrStatisticsCollection[outputIdx];
                sb.Append(leftMargin + $"Output field [{_settings.ReadoutUnitCfgCollection[outputIdx].Name}]" + Environment.NewLine);
                if (_settings.ReadoutUnitCfgCollection[outputIdx].TaskType == ReadoutUnit.TaskType.Classification)
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

        /// <summary>
        /// Returns results of the readout units training
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
            foreach (ReadoutUnit clusterMember in _clusterCollection[clusterIdx])
            {
                double computedValue = clusterMember.Network.Compute(readoutUnitPredictors)[0];
                if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == ReadoutUnit.TaskType.Classification)
                {
                    //Classification
                    //Training accuracy as a sub-result wight
                    weightedResult.AddSampleValue(computedValue, 1d - clusterMember.TrainingBinErrorStat.TotalErrStat.ArithAvg);
                    if (clusterMember.TestingBinErrorStat != null && clusterMember.TestingBinErrorStat.TotalErrStat.NumOfSamples > 0)
                    {
                        //Testing accuracy as a sub-result wight
                        weightedResult.AddSampleValue(computedValue, 1d - clusterMember.TestingBinErrorStat.TotalErrStat.ArithAvg);
                    }
                }
                else
                {
                    //Forecast
                    //Training accuracy as a sub-result wight
                    weightedResult.AddSampleValue(computedValue, 1d - clusterMember.TrainingErrorStat.ArithAvg);
                    if (clusterMember.TestingErrorStat != null && clusterMember.TestingErrorStat.NumOfSamples > 0)
                    {
                        //Testing accuracy as a sub-result wight
                        weightedResult.AddSampleValue(computedValue, 1d - clusterMember.TestingErrorStat.ArithAvg);
                    }

                }
            }
            return weightedResult.Avg;
        }


        private double Compute_v1(double[] predictors, int clusterIdx)
        {
            string readoutUnitName = _settings.ReadoutUnitCfgCollection[clusterIdx].Name;
            double[] readoutUnitPredictors = _predictorsMapper.CreateVector(readoutUnitName, predictors);
            //Collect outputs from cluster members
            List<double> computedValues = new List<double>(_clusterCollection[clusterIdx].Length);
            BasicStat computedValuesStat = new BasicStat();
            foreach(ReadoutUnit clusterMember in _clusterCollection[clusterIdx])
            {
                double computedValue = clusterMember.Network.Compute(readoutUnitPredictors)[0];
                //Stat update
                computedValues.Add(computedValue);
                computedValuesStat.AddSampleValue(computedValue);
            }
            //Finalize result
            if (_settings.ReadoutUnitCfgCollection[clusterIdx].TaskType == ReadoutUnit.TaskType.Classification)
            {
                return computedValuesStat.ArithAvg;
                //return computedValuesStat.Max;
            }
            else
            {
                return computedValuesStat.ArithAvg;
            }
        }

        private double Compute_org(double[] predictors, int clusterIdx)
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
        /// Returns rich readout data based on values vector
        /// </summary>
        /// <param name="readoutLayerVector">Vector of values corresponding to readout units</param>
        public ReadoutData GetReadoutData(double[] readoutLayerVector)
        {
            return new ReadoutData(readoutLayerVector, _settings);
        }

        /// <summary>
        /// Computes readout layer output data
        /// </summary>
        /// <param name="predictors">The predictors</param>
        public ReadoutData ComputeReadoutData(double[] predictors)
        {
            return GetReadoutData(Compute(predictors));
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
                DataVector = dataVector;
                ReadoutUnitDataCollection = new Dictionary<string, ReadoutUnitData>();
                foreach(ReadoutLayerSettings.ReadoutUnitSettings rus in rls.ReadoutUnitCfgCollection)
                {
                    ReadoutUnitDataCollection.Add(rus.Name, new ReadoutUnitData() { Name = rus.Name, Index = rus.Index, Task = rus.TaskType, DataValue = DataVector[rus.Index] });
                }
                OneWinnerDataCollection = new Dictionary<string, OneWinnerGroupData>();
                foreach(string oneWinnerGroupName in rls.OneWinnerGroupNameCollection.Values)
                {
                    List<ReadoutLayerSettings.ReadoutUnitSettings> rusCollection = rls.GetOneWinnerGroupMembers(oneWinnerGroupName);
                    //Probabilities
                    double[] probabilities = new double[rusCollection.Count];
                    BasicStat pStat = new BasicStat();
                    double plusMin = 0d;
                    for (int i = 0; i < rusCollection.Count; i++)
                    {
                        pStat.AddSampleValue(dataVector[rusCollection[i].Index]);
                    }
                    if(pStat.Min < 0)
                    {
                        plusMin = Math.Abs(pStat.Min);
                        BasicStat transPStat = new BasicStat();
                        for (int i = 0; i < rusCollection.Count; i++)
                        {
                            pStat.AddSampleValue(dataVector[rusCollection[i].Index] + plusMin);
                        }
                        pStat = transPStat;
                    }
                    int winningIndex = -1;
                    for (int i = 0; i < rusCollection.Count; i++)
                    {
                        probabilities[i] = (dataVector[rusCollection[i].Index] + plusMin) / pStat.Sum;
                        if(winningIndex == -1 || probabilities[i] > probabilities[winningIndex])
                        {
                            winningIndex = i;
                        }
                    }
                    string winningReadoutUnitName = rusCollection[winningIndex].Name;
                    OneWinnerDataCollection.Add(oneWinnerGroupName, new OneWinnerGroupData(){GroupName = oneWinnerGroupName,
                                                                                             WinningReadoutUnitName = rusCollection[winningIndex].Name,
                                                                                             WinningReadoutUnitIndex = winningIndex,
                                                                                             Probabilities = probabilities });
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
                /// Winning probabilities (within the group)
                /// </summary>
                public double[] Probabilities { get; set; }
            }//OneWinnerGroupData

        }//ReadoutData

        /// <summary>
        /// Summary statistics
        /// </summary>
        [Serializable]
        public class SummaryResultStat
        {
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
            /// <param name="rls">Readout layer settings</param>
            public SummaryResultStat(ReadoutLayerSettings rls)
            {
                ReadoutUnitStatCollection = new List<ReadoutUnitStat>(rls.ReadoutUnitCfgCollection.Count);
                foreach(ReadoutLayerSettings.ReadoutUnitSettings rus in rls.ReadoutUnitCfgCollection)
                {
                    ReadoutUnitStatCollection.Add(new ReadoutUnitStat(rus));
                }
                OneWinnerGroupStatCollection = new List<OneWinnerGroupStat>();
                foreach(string groupName in rls.OneWinnerGroupNameCollection.Values)
                {
                    OneWinnerGroupStatCollection.Add(new OneWinnerGroupStat(groupName, rls));
                }
                return;
            }

            //Methods
            /// <summary>
            /// Updates error statistics
            /// </summary>
            /// <param name="computedValues">Computed values</param>
            /// <param name="idealValues">Ideal values</param>
            /// <param name="rls">Readout layer settings</param>
            public void Update(double[] computedValues, double[] idealValues, ReadoutLayerSettings rls)
            {
                foreach(ReadoutUnitStat ruStat in ReadoutUnitStatCollection)
                {
                    ruStat.Update(computedValues, idealValues);
                }
                foreach(OneWinnerGroupStat grStat in OneWinnerGroupStatCollection)
                {
                    grStat.Update(computedValues, idealValues, rls);
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
                    foreach(string className in grStat.ClassErrorStatCollection.Keys)
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
                    if(Task == ReadoutUnit.TaskType.Classification)
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
                    if(Task == ReadoutUnit.TaskType.Classification)
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
                /// <param name="rls">Readout layer settings</param>
                public OneWinnerGroupStat(string groupName, ReadoutLayerSettings rls)
                {
                    Name = groupName;
                    GroupErrorStat = new BasicStat();
                    ClassErrorStatCollection = new Dictionary<string, BasicStat>();
                    foreach(ReadoutLayerSettings.ReadoutUnitSettings rus in rls.GetOneWinnerGroupMembers(Name))
                    {
                        ClassErrorStatCollection.Add(rus.Name, new BasicStat());
                    }
                    return;
                }

                //Methods
                /// <summary>
                /// Updates error statistics
                /// </summary>
                /// <param name="computedValues">Computed values</param>
                /// <param name="idealValues">Ideal values</param>
                /// <param name="rls">Readout layer settings</param>
                public void Update(double[] computedValues, double[] idealValues, ReadoutLayerSettings rls)
                {
                    List<ReadoutLayerSettings.ReadoutUnitSettings> grpMembers = rls.GetOneWinnerGroupMembers(Name);
                    int maxComputedValueIdx = -1;
                    int maxIdealValueIdx = -1;
                    foreach(ReadoutLayerSettings.ReadoutUnitSettings rus in grpMembers)
                    {
                        if(maxComputedValueIdx == -1 || computedValues[rus.Index] > computedValues[maxComputedValueIdx])
                        {
                            maxComputedValueIdx = rus.Index;
                        }
                        if (maxIdealValueIdx == -1 || idealValues[rus.Index] > idealValues[maxIdealValueIdx])
                        {
                            maxIdealValueIdx = rus.Index;
                        }
                    }
                    double err = maxComputedValueIdx == maxIdealValueIdx ? 0d : 1d;
                    GroupErrorStat.AddSampleValue(err);
                    ClassErrorStatCollection[rls.ReadoutUnitCfgCollection[maxIdealValueIdx].Name].AddSampleValue(err);
                    return;
                }

            }//OneWinnerGroupStat

        }//SummaryResultStat


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
            /// Creates initialized instance
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
            /// <param name="taskType">Type of neural task</param>
            /// <param name="numOfMembers">Number of computing networks within the cluster</param>
            /// <param name="binBorder">Binary 0/1 border. Double value LT this border is considered as 0 and GE as 1.
            /// (relevant only if task type is Classification)
            /// </param>
            public ClusterErrStatistics(ReadoutUnit.TaskType taskType, int numOfMembers, double binBorder)
            {
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

    }//ReadoutLayer

}//Namespace
