using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural;
using RCNet.Neural.Data;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;
using RCNet.DemoConsoleApp.Log;
using System.Text;
using System.Linq;

namespace RCNet.DemoConsoleApp
{
    /// <summary>
    /// Demonstrates the State Machine usage, performing demo cases defined in xml file.
    /// </summary>
    public static class SMDemo
    {
        //Methods
        /// <summary>
        /// Informative callback function called by StateMachine to inform about predictors collection progress.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="userObject">An user object (IOutputLog)</param>
        public static void PredictorsCollectionCallback(int totalNumOfInputs,
                                                         int numOfProcessedInputs,
                                                         Object userObject
                                                         )
        {
            //SMDemo displays/updates information through IOutputLog passed as the userObject
            if (numOfProcessedInputs % 10 == 0 || numOfProcessedInputs == totalNumOfInputs || totalNumOfInputs == 1)
            {
                ((IOutputLog)userObject).Write($"    Collecting State Machine predictors {totalNumOfInputs.ToString()}/{numOfProcessedInputs.ToString()}", true);
            }
            return;
        }

        /// <summary>
        /// Function simply displays important statistics of the State Machine's reservoirs.
        /// </summary>
        /// <param name="statisticsCollection">Collection of reservoir's statistics</param>
        /// <param name="log">Output log object</param>
        private static void ReportReservoirsStatistics(List<ReservoirStat> statisticsCollection, IOutputLog log)
        {
            log.Write("    Reservoir(s) info:", false);
            foreach (ReservoirStat resStat in statisticsCollection)
            {
                log.Write($"      Statistics of reservoir instance: {resStat.ReservoirInstanceName} ({resStat.ReservoirSettingsName})", false);
                foreach (ReservoirStat.PoolStat poolStat in resStat.PoolStatCollection)
                {
                    log.Write($"        Statistics of pool: {poolStat.PoolName}", false);
                    foreach (ReservoirStat.PoolStat.NeuronGroupStat groupStat in poolStat.NeuronGroupStatCollection)
                    {
                        log.Write($"          Statistics of group: {groupStat.GroupName}", false);
                        log.Write($"            Neurons stimulation (all components)", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.AvgTStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgTStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgTStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgTStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.MaxTStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxTStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxTStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxTStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MIN Avg, Max, Min, SDdev: " + groupStat.MinTStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinTStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinTStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinTStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("             SPAN Avg, Max, Min, SDdev: " + groupStat.TStimuliSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.TStimuliSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.TStimuliSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.TStimuliSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write($"            Neurons stimulation (component coming from connected reservoir's neurons)", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.AvgRStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgRStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgRStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgRStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.MaxRStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxRStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxRStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxRStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MIN Avg, Max, Min, SDdev: " + groupStat.MinRStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinRStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinRStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinRStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("             SPAN Avg, Max, Min, SDdev: " + groupStat.RStimuliSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.RStimuliSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.RStimuliSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.RStimuliSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write($"            Synapses efficacy", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.AvgSynEfficacyStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgSynEfficacyStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgSynEfficacyStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgSynEfficacyStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.MaxSynEfficacyStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxSynEfficacyStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxSynEfficacyStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxSynEfficacyStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MIN Avg, Max, Min, SDdev: " + groupStat.MinSynEfficacyStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinSynEfficacyStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinSynEfficacyStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinSynEfficacyStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("             SPAN Avg, Max, Min, SDdev: " + groupStat.SynEfficacySpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.SynEfficacySpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.SynEfficacySpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.SynEfficacySpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write($"            Activations state", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.AvgActivationStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgActivationStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgActivationStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgActivationStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.MaxActivationStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxActivationStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxActivationStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxActivationStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("             SPAN Avg, Max, Min, SDdev: " + groupStat.ActivationStateSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.ActivationStateSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.ActivationStateSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.ActivationStateSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write($"            Neurons output signal", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.AvgOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.AvgOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.MaxOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MaxOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MIN Avg, Max, Min, SDdev: " + groupStat.MinOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.MinOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                    }
                    log.Write($"          Weights statistics", false);
                    log.Write("            Input Avg, Max, Min, SDdev: " + poolStat.InputWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                         + poolStat.InputWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                         + poolStat.InputWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                         + poolStat.InputWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                    log.Write("         Internal Avg, Max, Min, SDdev: " + poolStat.InternalWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                         + poolStat.InternalWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                         + poolStat.InternalWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                         + poolStat.InternalWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                }
            }
            log.Write(" ", false);
            //Console.ReadLine();
            return;
        }

        /// <summary>
        /// This is the callback control function of the regression process and is called by State Machine
        /// after the completion of each regression training epoch.
        /// 
        /// The goal of the regression process is for each output field to train a readout network(s)
        /// that will give good results both on the training data and the test data. An instance of the
        /// Regression.RegressionControlInArgs class passed to this function contains the best error statistics so far
        /// and the latest statistics. The primary purpose of this function is to decide whether the latest statistics
        /// are better than the best statistics so far. Here is used simply the default implementation of the decision, but
        /// the logic can be much more complicated in real-world situations.
        /// The function can also tell the regression process that it does not make any sense to continue the regression.
        /// It can terminate the current regression attempt or whole readout unit regression process.
        /// 
        /// The secondary purpose of this function is to inform about the regression process progress.
        /// </summary>
        /// <param name="inArgs">Contains all the necessary information to control the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public static ReadoutUnit.RegressionControlOutArgs RegressionControl(ReadoutUnit.RegressionControlInArgs inArgs)
        {
            //Instantiate output object.
            ReadoutUnit.RegressionControlOutArgs outArgs = new ReadoutUnit.RegressionControlOutArgs
            {
                //Call the default implementation of the judgement.
                CurrentIsBetter = ReadoutUnit.IsBetter(inArgs.TaskType, inArgs.CurrReadoutUnit, inArgs.BestReadoutUnit)
            };
            //Report the progress
            if (outArgs.CurrentIsBetter || (inArgs.Epoch % 10) == 0 || inArgs.Epoch == inArgs.MaxEpochs || (inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1))
            {
                //Mark the currently best readout unit
                ReadoutUnit bestReadoutUnit = outArgs.CurrentIsBetter ? inArgs.CurrReadoutUnit : inArgs.BestReadoutUnit;
                //Build progress text message
                StringBuilder progressText = new StringBuilder();
                progressText.Append("      OutputField: ");
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
                if (inArgs.TaskType == CommonEnums.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + bestReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + bestReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Best-Test: ");
                progressText.Append(bestReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (inArgs.TaskType == CommonEnums.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + bestReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + bestReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Train: ");
                progressText.Append(inArgs.CurrReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (inArgs.TaskType == CommonEnums.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + inArgs.CurrReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + inArgs.CurrReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                progressText.Append(", Curr-Test: ");
                progressText.Append(inArgs.CurrReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture));
                if (inArgs.TaskType == CommonEnums.TaskType.Classification)
                {
                    //Append binary errors
                    progressText.Append("/" + inArgs.CurrReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture));
                    progressText.Append("/" + inArgs.CurrReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture));
                }
                ((IOutputLog)inArgs.UserObject).Write(progressText.ToString(), !(inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1));
            }
            return outArgs;
        }

        /// <summary>
        /// Performs one demo case.
        /// Loads and prepares sample data, trains State Machine and displayes results
        /// </summary>
        /// <param name="log">Into this interface are written output messages</param>
        /// <param name="demoCaseParams">An instance of DemoSettings.CaseSettings to be performed</param>
        public static void PerformDemoCase(IOutputLog log, DemoSettings.CaseSettings demoCaseParams)
        {
            log.Write("  Performing demo case " + demoCaseParams.Name, false);
            //Bundle normalizer object
            BundleNormalizer bundleNormalizer = null;
            //Prediction input vector (relevant only for input continuous feeding)
            double[] predictionInputVector = null;
            //Instantiate the State Machine
            StateMachine stateMachine = new StateMachine(demoCaseParams.StateMachineCfg);
            //Prepare input object for regression stage
            log.Write(" ", false);
            StateMachine.RegressionInput rsi = null;
            List<string> outputFieldNameCollection = (from rus in demoCaseParams.StateMachineCfg.ReadoutLayerConfig.ReadoutUnitCfgCollection select rus.Name).ToList();
            List<CommonEnums.TaskType> outputFieldTaskCollection = (from rus in demoCaseParams.StateMachineCfg.ReadoutLayerConfig.ReadoutUnitCfgCollection select rus.TaskType).ToList();
            if (demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.FeedingType == CommonEnums.InputFeedingType.Continuous)
            {
                //Continuous input feeding
                //Load data bundle from csv file
                VectorBundle data = VectorBundle.LoadFromCsv(demoCaseParams.FileName,
                                                                     demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.ExternalFieldNameCollection(),
                                                                     outputFieldNameCollection,
                                                                     outputFieldTaskCollection,
                                                                     NeuralPreprocessor.DataRange,
                                                                     demoCaseParams.NormalizerReserveRatio,
                                                                     true,
                                                                     out bundleNormalizer,
                                                                     out predictionInputVector
                                                                     );
                rsi = stateMachine.PrepareRegressionData(data, PredictorsCollectionCallback, log);
            }
            else
            {
                //Patterned input feeding
                //Load data bundle from csv file
                PatternBundle data = PatternBundle.LoadFromCsv(demoCaseParams.FileName,
                                                               demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.ExternalFieldNameCollection(),
                                                               outputFieldNameCollection,
                                                               outputFieldTaskCollection,
                                                               NeuralPreprocessor.DataRange,
                                                               demoCaseParams.NormalizerReserveRatio,
                                                               true,
                                                               out bundleNormalizer
                                                               );
                rsi = stateMachine.PrepareRegressionData(data, PredictorsCollectionCallback, log);
            }
            //Report statistics of the State Machine's reservoirs
            ReportReservoirsStatistics(rsi.ReservoirStatCollection, log);

            //Regression stage
            log.Write("    Regression stage", false);
            //Perform the regression
            ResultComparativeBundle vb = stateMachine.BuildReadoutLayer(rsi, RegressionControl, log);

            //Perform prediction if the input feeding is continuous (we know the input but we don't know the ideal output)
            double[] predictionOutputVector = null;
            if (demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.FeedingType == CommonEnums.InputFeedingType.Continuous)
            {
                predictionOutputVector = stateMachine.Compute(predictionInputVector);
                //Values are normalized so they have to be denormalized
                bundleNormalizer.NaturalizeOutputVector(predictionOutputVector);
            }

            //Display results
            //Report training (regression) results and prediction
            log.Write("    Results", false);
            List<ReadoutLayer.ClusterErrStatistics> clusterErrStatisticsCollection = stateMachine.RL.ClusterErrStatisticsCollection;
            //Results
            for (int outputIdx = 0; outputIdx < demoCaseParams.StateMachineCfg.ReadoutLayerConfig.ReadoutUnitCfgCollection.Count; outputIdx++)
            {
                ReadoutLayer.ClusterErrStatistics ces = clusterErrStatisticsCollection[outputIdx];
                if (demoCaseParams.StateMachineCfg.ReadoutLayerConfig.ReadoutUnitCfgCollection[outputIdx].TaskType == CommonEnums.TaskType.Classification)
                {
                    //Classification task report
                    log.Write("            OutputField: " + demoCaseParams.StateMachineCfg.ReadoutLayerConfig.ReadoutUnitCfgCollection[outputIdx].Name, false);
                    log.Write("   Num of bin 0 samples: " + ces.BinaryErrStat.BinValErrStat[0].NumOfSamples.ToString(), false);
                    log.Write("     Bad bin 0 classif.: " + ces.BinaryErrStat.BinValErrStat[0].Sum.ToString(CultureInfo.InvariantCulture), false);
                    log.Write("       Bin 0 error rate: " + ces.BinaryErrStat.BinValErrStat[0].ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                    log.Write("         Bin 0 accuracy: " + (1 - ces.BinaryErrStat.BinValErrStat[0].ArithAvg).ToString(CultureInfo.InvariantCulture), false);
                    log.Write("   Num of bin 1 samples: " + ces.BinaryErrStat.BinValErrStat[1].NumOfSamples.ToString(), false);
                    log.Write("     Bad bin 1 classif.: " + ces.BinaryErrStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture), false);
                    log.Write("       Bin 1 error rate: " + ces.BinaryErrStat.BinValErrStat[1].ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                    log.Write("         Bin 1 accuracy: " + (1 - ces.BinaryErrStat.BinValErrStat[1].ArithAvg).ToString(CultureInfo.InvariantCulture), false);
                    log.Write("   Total num of samples: " + ces.BinaryErrStat.TotalErrStat.NumOfSamples.ToString(), false);
                    log.Write("     Total bad classif.: " + ces.BinaryErrStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture), false);
                    log.Write("       Total error rate: " + ces.BinaryErrStat.TotalErrStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                    log.Write("         Total accuracy: " + (1 - ces.BinaryErrStat.TotalErrStat.ArithAvg).ToString(CultureInfo.InvariantCulture), false);
                }
                else
                {
                    //Forecast task report
                    log.Write("            OutputField: " + demoCaseParams.StateMachineCfg.ReadoutLayerConfig.ReadoutUnitCfgCollection[outputIdx].Name, false);
                    log.Write("   Predicted next value: " + predictionOutputVector[outputIdx].ToString(CultureInfo.InvariantCulture), false);
                    log.Write("   Total num of samples: " + ces.PrecissionErrStat.NumOfSamples.ToString(), false);
                    log.Write("     Total Max Real Err: " + (bundleNormalizer.OutputFieldNormalizerRefCollection[outputIdx].ComputeNaturalSpan(ces.PrecissionErrStat.Max)).ToString(CultureInfo.InvariantCulture), false);
                    log.Write("     Total Avg Real Err: " + (bundleNormalizer.OutputFieldNormalizerRefCollection[outputIdx].ComputeNaturalSpan(ces.PrecissionErrStat.ArithAvg)).ToString(CultureInfo.InvariantCulture), false);
                }
                log.Write(" ", false);
            }
            log.Write(" ", false);
            return;
        }

        /// <summary>
        /// Runs State Machine demo. This is the main function.
        /// For each demo case defined in xml file function calls PerformDemoCase.
        /// </summary>
        /// <param name="log">Into this interface demo writes output to be displayed</param>
        /// <param name="demoSettingsXmlFile">Xml file containing definitions of demo cases to be prformed</param>
        public static void RunDemo(IOutputLog log, string demoSettingsXmlFile)
        {
            log.Write("State Machine demo started", false);
            //Instantiate demo settings from the xml file
            DemoSettings demoSettings = new DemoSettings(demoSettingsXmlFile);
            //Loop through all demo cases
            foreach(DemoSettings.CaseSettings demoCaseParams in demoSettings.CaseCfgCollection)
            {
                //Execute the demo case
                PerformDemoCase(log, demoCaseParams);
            }
            log.Write("State Machine demo finished", false);
            log.Write(string.Empty);
            return;
        }
    }//SMDemo

}//Namespace
