using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.CsvTools;
using RCNet.Neural.Network.Data;
using RCNet.Neural.Network.EchoState;
using RCNet.Neural.Network.RCReadout;
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
                log.Write("            ABS-MAX Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.Max.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.Min.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsMaxAbsStatesStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                log.Write("                RMS Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].NeuronsRMSStatesStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsRMSStatesStat.Max.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsRMSStatesStat.Min.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsRMSStatesStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                log.Write("               SPAN Avg, Max, Min, SDdev: " + statisticsCollection[resIdx].NeuronsStateSpansStat.ArithAvg.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsStateSpansStat.Max.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsStateSpansStat.Min.ToString(CultureInfo.InvariantCulture) + " " + statisticsCollection[resIdx].NeuronsStateSpansStat.StdDev.ToString(CultureInfo.InvariantCulture), false);
                log.Write("               Context neuron states RMS: " + statisticsCollection[resIdx].CtxNeuronStatesRMS.ToString(CultureInfo.InvariantCulture), false);
            }
            return;
        }

        /// <summary>
        /// This is the control function of the regression process and is called
        /// after the completion of each regression training epoch.
        /// The goal of the regression process is for each Esn output field to train a feed forward network
        /// that will give good results both on the training data and the test data.
        /// Regression.RegressionControlInArgs object passed to the function contains the best error statistics so far
        /// and the latest statistics. The primary purpose of the function is to decide whether the latest statistics
        /// are better than the best statistics so far.
        /// Here is used simply outArgs.Best = (inArgs.CurrReadoutUnit.CombinedError LT inArgs.BestReadoutUnit.CombinedError), but
        /// the real logic could be much more complex.
        /// The function can also tell the regression process that it does not make any sense to continue the regression.
        /// It can terminate the current regression attempt or whole output field regression process.
        /// </summary>
        /// <param name="inArgs">Contains all the necessary information to control the progress of the regression.</param>
        /// <returns>Instructions for the regression process.</returns>
        public static Regression.RegressionControlOutArgs EsnRegressionControl(Regression.RegressionControlInArgs inArgs)
        {
            //Instantiate output object.
            Regression.RegressionControlOutArgs outArgs = new Regression.RegressionControlOutArgs();
            //Evaluate statistics and decide if the latest statistics are the best.
            outArgs.Best = (inArgs.CurrReadoutUnit.CombinedError < inArgs.BestReadoutUnit.CombinedError);
            //outArgs.Best = (inArgs.RegrCurrResult.TrainingErrorStat.ArithAvg < inArgs.RegrBestResult.TrainingErrorStat.ArithAvg);
            //Report the progress
            int reportInterval = Math.Max(inArgs.MaxEpochs / 100, 1);
            if (outArgs.Best || (inArgs.Epoch % reportInterval) == 0 || inArgs.Epoch == inArgs.MaxEpochs || (inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1))
            {
                ((IOutputLog)inArgs.UserObject).Write(
                    "      OutputField: " + inArgs.OutputFieldName +
                    ", Attempt/Epoch: " + inArgs.RegrAttemptNumber.ToString().PadLeft(inArgs.RegrMaxAttempts.ToString().Length, '0') + "/" + inArgs.Epoch.ToString().PadLeft(inArgs.MaxEpochs.ToString().Length, '0') +
                    ", DSet-Sizes: (" + inArgs.CurrReadoutUnit.TrainingErrorStat.NumOfSamples.ToString() + ", " + inArgs.CurrReadoutUnit.TestingErrorStat.NumOfSamples.ToString() + ")" +
                    ", Best-Train: " + (outArgs.Best ? inArgs.CurrReadoutUnit.TrainingErrorStat : inArgs.BestReadoutUnit.TrainingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) + "/" + (outArgs.Best ? inArgs.CurrReadoutUnit.TrainingBinErrorStat : inArgs.BestReadoutUnit.TrainingBinErrorStat).TotalNumOfBinErrors.ToString(CultureInfo.InvariantCulture) +
                    ", Best-Test: " + (outArgs.Best ? inArgs.CurrReadoutUnit.TestingErrorStat : inArgs.BestReadoutUnit.TestingErrorStat).ArithAvg.ToString("E3", CultureInfo.InvariantCulture) + "/" + (outArgs.Best ? inArgs.CurrReadoutUnit.TestingBinErrorStat : inArgs.BestReadoutUnit.TestingBinErrorStat).TotalNumOfBinErrors.ToString(CultureInfo.InvariantCulture) +
                    ", Curr-Train: " + inArgs.CurrReadoutUnit.TrainingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) + "/" + inArgs.CurrReadoutUnit.TrainingBinErrorStat.TotalNumOfBinErrors.ToString(CultureInfo.InvariantCulture) +
                    ", Curr-Test: " + inArgs.CurrReadoutUnit.TestingErrorStat.ArithAvg.ToString("E3", CultureInfo.InvariantCulture) + "/" + inArgs.CurrReadoutUnit.TestingBinErrorStat.TotalNumOfBinErrors.ToString(CultureInfo.InvariantCulture)
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
        public static void PerformDemoCase(IOutputLog log, EsnDemoSettings.EsnDemoCaseSettings demoCaseParams)
        {
            //For demo purposes is allowed only the normalization range (-1, 1)
            Interval normRange = new Interval(-1, 1);
            log.Write("  Performing demo case " + demoCaseParams.Name, false);
            
            //Bundle normalizer object
            BundleNormalizer bundleNormalizer = null;
            //Prediction input vector (relevant only for time series prediction task)
            double[] predictionInputVector = null;
            
            //Instantiate an Esn
            Esn esn = new Esn(demoCaseParams.EsnConfiguration);

            //Prepare regression stage input object
            log.Write(" ", false);
            Esn.RegressionStageInput rsi = null;
            if (demoCaseParams.EsnConfiguration.TaskType == CommonTypes.TaskType.Prediction)
            {
                //Time series prediction task
                //Load data bundle from csv file
                VectorsPairBundle data = TimeSeriesDataLoader.Load(demoCaseParams.CsvDataFileName,
                                                                   demoCaseParams.EsnConfiguration.InputFieldNameCollection,
                                                                   demoCaseParams.EsnConfiguration.OutputFieldNameCollection,
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
                //Classification task
                //Load data bundle from csv file
                PatternVectorPairBundle data = PatternDataLoader.Load(demoCaseParams.CsvDataFileName,
                                                                      demoCaseParams.EsnConfiguration.InputFieldNameCollection,
                                                                      demoCaseParams.EsnConfiguration.OutputFieldNameCollection,
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
            //Select appropriate method for the test samples selection
            Regression.TestSamplesSelectorDelegate samplesSelector = Regression.SelectRandomTestSamples;
            if (demoCaseParams.TestSamplesSelectionMethod == "Sequential") samplesSelector = Regression.SelectSequentialTestSamples;
            //Training - Esn regression stage
            ReadoutUnit[] readoutUnits = esn.RegressionStage(rsi, demoCaseParams.NumOfTestSamples, samplesSelector, EsnRegressionControl, log);

            //Perform prediction if the task type is Prediction
            double[] predictionOutputVector = null;
            if(demoCaseParams.EsnConfiguration.TaskType == CommonTypes.TaskType.Prediction)
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
            for (int outputIdx = 0; outputIdx < readoutUnits.Length; outputIdx++)
            {
                log.Write("            OutputField: " + readoutUnits[outputIdx].OutputFieldName, false);
                if (demoCaseParams.EsnConfiguration.TaskType == CommonTypes.TaskType.Prediction)
                {
                    log.Write("         Predicted next: " + predictionOutputVector[outputIdx].ToString(CultureInfo.InvariantCulture), false);
                }
                log.Write("    Trained weights stat", false);
                log.Write("          Min, Max, Avg: " + readoutUnits[outputIdx].OutputWeightsStat.Min.ToString(CultureInfo.InvariantCulture) + " " + readoutUnits[outputIdx].OutputWeightsStat.Max.ToString(CultureInfo.InvariantCulture) + " " + readoutUnits[outputIdx].OutputWeightsStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("               Cnt, Zrs: " + readoutUnits[outputIdx].OutputWeightsStat.NumOfSamples.ToString() + " " + (readoutUnits[outputIdx].OutputWeightsStat.NumOfSamples - readoutUnits[outputIdx].OutputWeightsStat.NumOfNonzeroSamples).ToString(), false);
                log.Write("              Error stat", false);
                log.Write("      Train set samples: " + readoutUnits[outputIdx].TrainingErrorStat.NumOfSamples.ToString(), false);
                log.Write("      Train set Avg Err: " + readoutUnits[outputIdx].TrainingErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("       Test set samples: " + readoutUnits[outputIdx].TestingErrorStat.NumOfSamples.ToString(), false);
                log.Write("       Test set Avg Err: " + readoutUnits[outputIdx].TestingErrorStat.ArithAvg.ToString(CultureInfo.InvariantCulture), false);
                log.Write("      Test Max Real Err: " + (bundleNormalizer.OutputFieldNormalizerRefCollection[outputIdx].ComputeNaturalSpan(readoutUnits[outputIdx].TestingErrorStat.Max)).ToString(CultureInfo.InvariantCulture), false);
                log.Write("      Test Avg Real Err: " + (bundleNormalizer.OutputFieldNormalizerRefCollection[outputIdx].ComputeNaturalSpan(readoutUnits[outputIdx].TestingErrorStat.ArithAvg)).ToString(CultureInfo.InvariantCulture), false);
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
            //Loop through the demo cases
            foreach(EsnDemoSettings.EsnDemoCaseSettings demoCaseParams in demoSettings.DemoCaseParamsCollection)
            {
                //Execute the demo case
                PerformDemoCase(log, demoCaseParams);
            }
            log.Write("ESN demo finished", false);
            return;
        }
    }//ESNDemo

}//Namespace
