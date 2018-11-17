using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using RCNet.MathTools;
using RCNet.Neural;
using RCNet.Neural.Data;
using RCNet.Neural.Network.SM;
using RCNet.DemoConsoleApp.Log;


namespace RCNet.DemoConsoleApp
{
    /// <summary>
    /// Demonstrates the State Machine usage.
    /// It performs demo cases defined in xml file.
    /// Input data has to be stored in a file (csv format).
    /// </summary>
    public static class SMDemo
    {

        /// <summary>
        /// Informative callback function used to display predictors collection progress.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="userObject">An user object (IOutputLog)</param>
        public static void PredictorsCollectionCallback(int totalNumOfInputs,
                                                         int numOfProcessedInputs,
                                                         Object userObject
                                                         )
        {
            if (numOfProcessedInputs % 10 == 0 || numOfProcessedInputs == totalNumOfInputs || totalNumOfInputs == 1)
            {
                ((IOutputLog)userObject).Write($"    Collecting State Machine predictors {totalNumOfInputs.ToString()}/{numOfProcessedInputs.ToString()}", true);
            }
            return;
        }

        /// <summary>
        /// Function displays reservoirs statistics.
        /// </summary>
        /// <param name="statisticsCollection">Collection of reservoir statistics</param>
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
                        log.Write($"            Neurons states", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.NeuronsAvgStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.NeuronsMaxStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("             SPAN Avg, Max, Min, SDdev: " + groupStat.NeuronsStateSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsStateSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsStateSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsStateSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write($"            Neurons stimuli", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.NeuronsAvgStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.NeuronsMaxStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MIN Avg, Max, Min, SDdev: " + groupStat.NeuronsMinStimuliStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMinStimuliStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMinStimuliStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMinStimuliStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("             SPAN Avg, Max, Min, SDdev: " + groupStat.NeuronsStimuliSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsStimuliSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsStimuliSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsStimuliSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write($"            Neurons output signal", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.NeuronsAvgOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MAX Avg, Max, Min, SDdev: " + groupStat.NeuronsMaxOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMaxOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write("              MIN Avg, Max, Min, SDdev: " + groupStat.NeuronsMinOutputSignalStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMinOutputSignalStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMinOutputSignalStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsMinOutputSignalStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                        log.Write($"            Neurons output signal frequency", false);
                        log.Write("              AVG Avg, Max, Min, SDdev: " + groupStat.NeuronsAvgOutputFreqStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgOutputFreqStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgOutputFreqStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                             + groupStat.NeuronsAvgOutputFreqStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
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
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch.
        /// The goal of the regression process is for each output field to train a readout network
        /// that will give good results both on the training data and the test data.
        /// Regression.RegressionControlInArgs object passed to the function contains the best error statistics so far
        /// and the latest statistics. The primary purpose of the function is to decide whether the latest statistics
        /// are better than the best statistics so far.
        /// Here is used simply the default implementation of the decision, but
        /// the the implemented logic can be much more complicated in real-world situations.
        /// The function can also tell the regression process that it does not make any sense to continue the regression.
        /// It can terminate the current regression attempt or whole readout unit regression process.
        /// </summary>
        /// <param name="inArgs">Contains all the necessary information to control the progress of the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public static ReadoutUnit.RegressionControlOutArgs RegressionControl(ReadoutUnit.RegressionControlInArgs inArgs)
        {
            //Instantiate output object.
            ReadoutUnit.RegressionControlOutArgs outArgs = new ReadoutUnit.RegressionControlOutArgs();
            //Call the default implementation of the judgement.
            outArgs.CurrentIsBetter = ReadoutUnit.IsBetter(inArgs.TaskType, inArgs.CurrReadoutUnit, inArgs.BestReadoutUnit);
            //Report the progress
            int reportInterval = Math.Max(inArgs.MaxEpochs / 100, 1);
            ReadoutUnit bestReadoutUnit = outArgs.CurrentIsBetter ? inArgs.CurrReadoutUnit : inArgs.BestReadoutUnit;
            if (outArgs.CurrentIsBetter || (inArgs.Epoch % reportInterval) == 0 || inArgs.Epoch == inArgs.MaxEpochs || (inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1))
            {
                ((IOutputLog)inArgs.UserObject).Write(
                    "      OutputField: " + inArgs.OutputFieldName +
                    ", Fold/Attempt/Epoch: " + inArgs.FoldNum.ToString().PadLeft(inArgs.NumOfFolds.ToString().Length, '0') + "/" +
                                               inArgs.RegrAttemptNumber.ToString().PadLeft(inArgs.RegrMaxAttempts.ToString().Length, '0') + "/" +
                                               inArgs.Epoch.ToString().PadLeft(inArgs.MaxEpochs.ToString().Length, '0') +
                    ", DSet-Sizes: (" + inArgs.CurrReadoutUnit.TrainingErrorStat.NumOfSamples.ToString() + ", " +
                                        inArgs.CurrReadoutUnit.TestingErrorStat.NumOfSamples.ToString() + ")" +
                    ", Best-Train: " + bestReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                                       (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                       (inArgs.TaskType == CommonEnums.TaskType.Classification ? bestReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                                       (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                       (inArgs.TaskType == CommonEnums.TaskType.Classification ? bestReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                    ", Best-Test: " + bestReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? bestReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? bestReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                    ", Curr-Train: " + inArgs.CurrReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? inArgs.CurrReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? inArgs.CurrReadoutUnit.TrainingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                    ", Curr-Test: " + inArgs.CurrReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? inArgs.CurrReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? inArgs.CurrReadoutUnit.TestingBinErrorStat.BinValErrStat[1].Sum.ToString(CultureInfo.InvariantCulture) : string.Empty)
                    , !(inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1));
            }
            return outArgs;
        }

        /// <summary>
        /// Performs specified demo case.
        /// Loads and prepares sample data, trains State Machine and displayes results
        /// </summary>
        /// <param name="log">Into this interface are written output messages</param>
        /// <param name="demoCaseParams">An instance of DemoSettings.CaseSettings to be performed</param>
        public static void PerformDemoCase(IOutputLog log, DemoSettings.CaseSettings demoCaseParams)
        {
            log.Write("  Performing demo case " + demoCaseParams.Name, false);
            //Input/Output data for the State Machine has to be always normalized within the range -1 and 1
            Interval normalizationRange = CommonEnums.GetDataNormalizationRange(CommonEnums.DataNormalizationRange.Inclusive_Neg1_Pos1);
            //Bundle normalizer object
            BundleNormalizer bundleNormalizer = null;
            //Prediction input vector (relevant only for time series prediction task)
            double[] predictionInputVector = null;
            
            //Instantiate an State Machine
            StateMachine stateMachine = new StateMachine(demoCaseParams.stateMachineCfg);

            //Prepare regression stage input object
            log.Write(" ", false);
            StateMachine.RegressionStageInput rsi = null;
            if (demoCaseParams.stateMachineCfg.TaskType == CommonEnums.TaskType.Prediction)
            {
                //Time series prediction task
                //Load data bundle from csv file
                TimeSeriesBundle data = TimeSeriesDataLoader.Load(demoCaseParams.FileName,
                                                                  demoCaseParams.stateMachineCfg.InputFieldNameCollection,
                                                                  demoCaseParams.stateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection,
                                                                  normalizationRange,
                                                                  demoCaseParams.NormalizerReserveRatio,
                                                                  true,
                                                                  demoCaseParams.SingleNormalizer,
                                                                  out bundleNormalizer,
                                                                  out predictionInputVector
                                                                  );
                rsi = stateMachine.PrepareRegressionStageInput(data, demoCaseParams.NumOfBootSamples, PredictorsCollectionCallback, log);
            }
            else
            {
                //Classification or hybrid task
                //Load data bundle from csv file
                PatternBundle data = PatternDataLoader.Load(demoCaseParams.stateMachineCfg.TaskType == CommonEnums.TaskType.Classification,
                                                            demoCaseParams.FileName,
                                                            demoCaseParams.stateMachineCfg.InputFieldNameCollection,
                                                            demoCaseParams.stateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection,
                                                            normalizationRange,
                                                            demoCaseParams.NormalizerReserveRatio,
                                                            true,
                                                            out bundleNormalizer
                                                            );
                rsi = stateMachine.PrepareRegressionStageInput(data, PredictorsCollectionCallback, log);
            }
            //Report reservoirs statistics
            ReportReservoirsStatistics(rsi.ReservoirStatCollection, log);

            //Regression stage
            log.Write("    Regression stage", false);
            //Training - State Machine regression stage
            ValidationBundle vb = stateMachine.RegressionStage(rsi, RegressionControl, log);

            //Perform prediction if the task type is Prediction
            double[] predictionOutputVector = null;
            if(demoCaseParams.stateMachineCfg.TaskType == CommonEnums.TaskType.Prediction)
            {
                predictionOutputVector = stateMachine.Compute(predictionInputVector);
                //Values are normalized so they have to be denormalized
                bundleNormalizer.NaturalizeOutputVector(predictionOutputVector);
            }

            //Display results
            //Report training (regression) results and prediction
            log.Write("    Results", false);
            List<ReadoutLayer.ClusterErrStatistics> clusterErrStatisticsCollection = stateMachine.ClusterErrStatisticsCollection;
            //Classification results
            for (int outputIdx = 0; outputIdx < demoCaseParams.stateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection.Count; outputIdx++)
            {
                ReadoutLayer.ClusterErrStatistics ces = clusterErrStatisticsCollection[outputIdx];
                if (demoCaseParams.stateMachineCfg.TaskType == CommonEnums.TaskType.Classification)
                {
                    //Classification task report
                    log.Write("            OutputField: " + demoCaseParams.stateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection[outputIdx], false);
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
                    //Prediction task report
                    log.Write("            OutputField: " + demoCaseParams.stateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection[outputIdx], false);
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
