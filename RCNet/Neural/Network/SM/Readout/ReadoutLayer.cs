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
            Settings = (ReadoutLayerSettings)settings.DeepClone();
            Reset();
            return;
        }

        //Static properties
        /// <summary>
        /// Binary border (for classification purposes only)
        /// </summary>
        public static double BinBorder { get { return DataRange.Mid; } }

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
            _readoutUnitCollection = new ReadoutUnit[Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count];
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
            if (numOfOutputs != Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count)
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
                _outputFeatureFilterCollection[nrmIdx] = FeatureFilterFactory.Create(DataRange, Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection[nrmIdx].TaskCfg.FeatureFilterCfg);
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
            for (int unitIdx = 0; unitIdx < Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; unitIdx++)
            {
                List<double[]> idealValueCollection = new List<double[]>(shuffledData.OutputVectorCollection.Count);
                //Transformation of ideal vectors to a single value vectors
                foreach (double[] idealVector in shuffledData.OutputVectorCollection)
                {
                    double[] value = new double[1];
                    value[0] = idealVector[unitIdx];
                    idealValueCollection.Add(value);
                }
                List<double[]> readoutUnitInputVectorCollection = _predictorsMapper.CreateVectorCollection(Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection[unitIdx].Name, shuffledData.InputVectorCollection);
                VectorBundle readoutUnitDataBundle = new VectorBundle(readoutUnitInputVectorCollection, idealValueCollection);
                TrainedNetworkClusterBuilder readoutUnitBuilder = new TrainedNetworkClusterBuilder(Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection[unitIdx].Name,
                                                                                                   Settings.GetReadoutUnitNetworksCollection(unitIdx),
                                                                                                   Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection[unitIdx].TaskCfg.Type == ReadoutUnit.TaskType.Classification ? BinBorder : double.NaN,
                                                                                                   rand,
                                                                                                   controller
                                                                                                   );
                //Register notification
                readoutUnitBuilder.RegressionEpochDone += OnRegressionEpochDone;
                //Build trained readout unit. Trained unit becomes to be the predicting cluster member
                _readoutUnitCollection[unitIdx] = new ReadoutUnit(unitIdx,
                                                                  readoutUnitBuilder.Build(readoutUnitDataBundle,
                                                                                           Settings.TestDataRatio,
                                                                                           Settings.Folds,
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
            for (int outputIdx = 0; outputIdx < Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; outputIdx++)
            {
                sb.Append(leftMargin + $"Output field [{Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection[outputIdx].Name}]: {predictedValues[outputIdx].ToString(CultureInfo.InvariantCulture)}" + Environment.NewLine);
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
            unitsAllSubResults = new List<double[]>(Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count);
            double[] outputVector = new double[_readoutUnitCollection.Length];
            for(int unitIdx = 0; unitIdx < _readoutUnitCollection.Length; unitIdx++)
            {
                double[] readoutUnitInputVector = _predictorsMapper.CreateVector(Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection[unitIdx].Name, predictors);
                outputVector[unitIdx] = _readoutUnitCollection[unitIdx].NetworkCluster.Compute(readoutUnitInputVector, out double[] memberOutputCollection);
                unitsAllSubResults.Add(memberOutputCollection);
            }
            return outputVector;
        }

        /// <summary>
        /// Computes readout layer output vector
        /// </summary>
        /// <param name="predictors">The predictors</param>
        /// <param name="unitsAllSubResults">All sub-predictions</param>
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
            membersIndexes = (from member in Settings.ReadoutUnitsCfg.OneWinnerGroupCollection[oneWinnerGroupName].Members select member.Index).ToArray();
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
        /// <param name="unitsAllSubResults">All sub-predictions</param>
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
                foreach(ReadoutUnitSettings rus in rl.Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection)
                {
                    ReadoutUnitDataCollection.Add(rus.Name, new ReadoutUnitData() { Name = rus.Name, Index = rus.Index, Task = rus.TaskCfg.Type, DataValue = DataVector[rus.Index] });
                }
                //One Winner groups
                OneWinnerDataCollection = new Dictionary<string, OneWinnerGroupData>();
                foreach (string oneWinnerGroupName in rl.Settings.ReadoutUnitsCfg.OneWinnerGroupCollection.Keys)
                {
                    //There is One Winner group
                    int winningUnitIndex = rl.DecideOneWinner(oneWinnerGroupName, dataVector, out int[] membersIndexes, out double[] membersWeightedDataVector);
                    OneWinnerDataCollection.Add(oneWinnerGroupName, new OneWinnerGroupData()
                    {
                        GroupName = oneWinnerGroupName,
                        WinningReadoutUnitName = rl.Settings.ReadoutUnitsCfg.ReadoutUnitCfgCollection[winningUnitIndex].Name,
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
