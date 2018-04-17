using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.CsvTools;
using RCNet.Neural.Data;
using RCNet.Neural.Analog.Reservoir;
using RCNet.Neural.Analog.Network.EchoState;
using RCNet.Neural.Analog.Readout;
using RCNet.Neural;
using RCNet.Demo.Log;


namespace RCNet.Demo
{
    /// <summary>
    /// Demonstrates the Esn usage.
    /// It performs demo cases defined in xml file.
    /// Input data has to be stored in a file (csv format).
    /// </summary>
    public static class EsnDemo
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
                ((IOutputLog)userObject).Write($"    Collecting Esn predictors {totalNumOfInputs.ToString()}/{numOfProcessedInputs.ToString()}", true);
            }
            return;
        }

        /// <summary>
        /// Function displays Esn reservoirs statistics.
        /// </summary>
        /// <param name="statisticsCollection">Collection of reservoir statistics</param>
        /// <param name="log">Output log object</param>
        private static void ReportEsnReservoirsStatistics(List<AnalogReservoirStat> statisticsCollection, IOutputLog log)
        {
            log.Write("    Reservoir(s) info:", false);
            for (int resIdx = 0; resIdx < statisticsCollection.Count; resIdx++)
            {
                log.Write($"      Neurons states statistics of reservoir instance {statisticsCollection[resIdx].ReservoirInstanceName} ({statisticsCollection[resIdx].ReservoirSettingsName})", false);
                log.Write("            ABS-MAX Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write("                RMS Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].NeuronsRMSStatesStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsRMSStatesStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsRMSStatesStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsRMSStatesStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write("               SPAN Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].NeuronsStateSpansStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsStateSpansStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsStateSpansStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].NeuronsStateSpansStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write("               Context neuron states RMS: " + statisticsCollection[resIdx].CtxNeuronStatesRMS.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write($"      Weights statistics of reservoir instance {statisticsCollection[resIdx].ReservoirInstanceName} ({statisticsCollection[resIdx].ReservoirSettingsName})", false);
                log.Write("              Input Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].InputWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].InputWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].InputWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].InputWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write("           Internal Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].InternalWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].InternalWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].InternalWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].InternalWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write("          Ctx input Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].CtxNeuronInputWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].CtxNeuronInputWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].CtxNeuronInputWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].CtxNeuronInputWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write("       Ctx feedback Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].CtxNeuronFeedbackWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].CtxNeuronFeedbackWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].CtxNeuronFeedbackWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].CtxNeuronFeedbackWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
                log.Write("           Feedback Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].FeedbackWeightsStat.ArithAvg.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].FeedbackWeightsStat.Max.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].FeedbackWeightsStat.Min.ToString("N4", CultureInfo.InvariantCulture) + ", "
                                                                       + statisticsCollection[resIdx].FeedbackWeightsStat.StdDev.ToString("N4", CultureInfo.InvariantCulture), false);
            }
            log.Write(" ", false);
            return;
        }

        /// <summary>
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch.
        /// The goal of the regression process is for each Esn output field to train a readout network
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
        public static ReadoutUnit.RegressionControlOutArgs EsnRegressionControl(ReadoutUnit.RegressionControlInArgs inArgs)
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
                    ", Best-Test: " + bestReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? bestReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                    ", Curr-Train: " + inArgs.CurrReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? inArgs.CurrReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture) : string.Empty) +
                    ", Curr-Test: " + inArgs.CurrReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? "/" : string.Empty) +
                                      (inArgs.TaskType == CommonEnums.TaskType.Classification ? inArgs.CurrReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum.ToString(CultureInfo.InvariantCulture) : string.Empty)
                    , !(inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1));
            }
            return outArgs;
        }

        /// <summary>
        /// Performs specified demo case.
        /// Loads and prepares sample data, trains Esn and displayes results
        /// </summary>
        /// <param name="log">Into this interface are written output messages</param>
        /// <param name="demoCaseParams">An instance of EsnDemoSettings.EsnDemoCaseSettings to be performed</param>
        public static void PerformDemoCase(IOutputLog log, EsnDemoSettings.CaseSettings demoCaseParams)
        {
            //For demo purposes is allowed only the normalization range (-1, 1)
            Interval normRange = new Interval(-1, 1);
            log.Write("  Performing demo case " + demoCaseParams.Name, false);
            
            //Bundle normalizer object
            BundleNormalizer bundleNormalizer = null;
            //Prediction input vector (relevant only for time series prediction task)
            double[] predictionInputVector = null;
            
            //Instantiate an Esn
            Esn esn = new Esn(demoCaseParams.EsnCfg);

            //Prepare regression stage input object
            log.Write(" ", false);
            Esn.RegressionStageInput rsi = null;
            if (demoCaseParams.EsnCfg.TaskType == CommonEnums.TaskType.Prediction)
            {
                //Time series prediction task
                //Load data bundle from csv file
                TimeSeriesBundle data = TimeSeriesDataLoader.Load(demoCaseParams.FileName,
                                                                  demoCaseParams.EsnCfg.InputFieldNameCollection,
                                                                  demoCaseParams.EsnCfg.ReadoutLayerConfig.OutputFieldNameCollection,
                                                                  normRange,
                                                                  demoCaseParams.NormalizerReserveRatio,
                                                                  true,
                                                                  demoCaseParams.SingleNormalizer,
                                                                  out bundleNormalizer,
                                                                  out predictionInputVector
                                                                  );
                rsi = esn.PrepareRegressionStageInput(data, demoCaseParams.NumOfBootSamples, PredictorsCollectionCallback, log);
            }
            else
            {
                //Classification or hybrid task
                //Load data bundle from csv file
                PatternBundle data = PatternDataLoader.Load(demoCaseParams.EsnCfg.TaskType == CommonEnums.TaskType.Classification,
                                                            demoCaseParams.FileName,
                                                            demoCaseParams.EsnCfg.InputFieldNameCollection,
                                                            demoCaseParams.EsnCfg.ReadoutLayerConfig.OutputFieldNameCollection,
                                                            normRange,
                                                            demoCaseParams.NormalizerReserveRatio,
                                                            true,
                                                            out bundleNormalizer
                                                            );
                rsi = esn.PrepareRegressionStageInput(data, PredictorsCollectionCallback, log);
            }
            //Report reservoirs statistics
            ReportEsnReservoirsStatistics(rsi.ReservoirStatCollection, log);

            //Regression stage
            log.Write("    Regression stage", false);
            //Training - Esn regression stage
            ValidationBundle vb = esn.RegressionStage(rsi, EsnRegressionControl, log);

            //Perform prediction if the task type is Prediction
            double[] predictionOutputVector = null;
            if(demoCaseParams.EsnCfg.TaskType == CommonEnums.TaskType.Prediction)
            {
                //Note that there is not necessary to call PushFeedback function immediately after training.
                //Feedback was already pushed during the Esn training.
                predictionOutputVector = esn.Compute(predictionInputVector);
                //Values are normalized so they have to be denormalized
                bundleNormalizer.NaturalizeOutputVector(predictionOutputVector);
            }

            //Display results
            //Report training (regression) results and prediction
            log.Write("    Results", false);
            List<ReadoutLayer.ClusterErrStatistics> clusterErrStatisticsCollection = esn.ClusterErrStatisticsCollection;
            //Classification results
            for (int outputIdx = 0; outputIdx < demoCaseParams.EsnCfg.ReadoutLayerConfig.OutputFieldNameCollection.Count; outputIdx++)
            {
                ReadoutLayer.ClusterErrStatistics ces = clusterErrStatisticsCollection[outputIdx];
                if (demoCaseParams.EsnCfg.TaskType == CommonEnums.TaskType.Classification)
                {
                    //Classification task report
                    log.Write("            OutputField: " + demoCaseParams.EsnCfg.ReadoutLayerConfig.OutputFieldNameCollection[outputIdx], false);
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
                    log.Write("            OutputField: " + demoCaseParams.EsnCfg.ReadoutLayerConfig.OutputFieldNameCollection[outputIdx], false);
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
        /// Runs ESN demo. This is the main function.
        /// For each demo case defined in demoSettingsXmlFile function calls PerformDemoCase.
        /// </summary>
        /// <param name="log">Into this interface demo writes output to be displayed</param>
        /// <param name="demoSettingsXmlFile">Xml file containing definitions of demo cases to be prformed</param>
        public static void RunDemo(IOutputLog log, string demoSettingsXmlFile)
        {
            log.Write("ESN demo started", false);
            //Instantiate demo settings from the xml file
            EsnDemoSettings demoSettings = new EsnDemoSettings(demoSettingsXmlFile);
            //Loop through all demo cases
            foreach(EsnDemoSettings.CaseSettings demoCaseParams in demoSettings.CaseCfgCollection)
            {
                //Execute the demo case
                PerformDemoCase(log, demoCaseParams);
            }
            log.Write("ESN demo finished", false);
            return;
        }
    }//ESNDemo

}//Namespace
