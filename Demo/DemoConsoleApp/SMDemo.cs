using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
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
        /// This is the callback control function of the regression process and is called by State Machine
        /// after the completion of each training epoch.
        /// 
        /// The goal of the regression process is to train for each output field the readout network(s)
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
            const int reportEpochsInterval = 10;
            //Instantiate output object to set instructions for the regression process.
            ReadoutUnit.RegressionControlOutArgs outArgs = new ReadoutUnit.RegressionControlOutArgs
            {
                //Call the default implementation of the judgement.
                CurrentIsBetter = ReadoutUnit.IsBetter(inArgs.TaskType, inArgs.CurrReadoutUnit, inArgs.BestReadoutUnit),
                StopRegression = (inArgs.TaskType == ReadoutUnit.TaskType.Classification &&
                                  inArgs.CurrReadoutUnit.TrainingBinErrorStat.TotalErrStat.Sum == 0 &&
                                  inArgs.CurrReadoutUnit.TestingBinErrorStat.TotalErrStat.Sum == 0
                                 )
            };
            //Progress info
            if (outArgs.CurrentIsBetter ||
                (inArgs.Epoch % reportEpochsInterval) == 0 ||
                inArgs.Epoch == inArgs.MaxEpochs ||
                (inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1)
                )
            {
                //Build progress report message
                string progressText = ReadoutLayer.GetProgressReport(inArgs,
                                                                     outArgs.CurrentIsBetter ? inArgs.CurrReadoutUnit : inArgs.BestReadoutUnit,
                                                                     6);
                //Report the progress
                ((IOutputLog)inArgs.UserObject).Write(progressText, !(inArgs.Epoch == 1 && inArgs.RegrAttemptNumber == 1));
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
            log.Write(" ", false);
            //Instantiate the State Machine
            StateMachine stateMachine = new StateMachine(demoCaseParams.StateMachineCfg);
            //Prepare input object for regression stage
            StateMachine.RegressionInput rsi = null;
            //Prediction input vector (relevant only for input continuous feeding)
            double[] predictionInputVector = null;
            if (demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.FeedingType == NeuralPreprocessor.InputFeedingType.Continuous)
            {
                //Continuous input feeding
                //Load data bundle from csv file
                VectorBundle data = VectorBundle.LoadFromCsv(demoCaseParams.FileName,
                                                             demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.ExternalFieldNameCollection(),
                                                             demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection,
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
                                                               demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection
                                                               );
                rsi = stateMachine.PrepareRegressionData(data, PredictorsCollectionCallback, log);
            }
            //Report key statistics of the State Machine's reservoirs
            string statisticsReport = rsi.CreateReport(4);
            log.Write(statisticsReport);
            log.Write(string.Empty);

            //Regression stage - building of trained readout layer
            log.Write("    Regression stage (training of readout layer)", false);
            //Perform the regression
            StateMachine.RegressionOutput regressionOutput = stateMachine.BuildReadoutLayer(rsi, RegressionControl, log);
            log.Write(string.Empty);

            //Report training (regression) results
            log.Write("    Training results", false);
            string trainingReport = stateMachine.RL.GetTrainingResultsReport(6);
            log.Write(trainingReport);
            log.Write(string.Empty);

            if (regressionOutput.VerificationResultBundle.InputVectorCollection.Count > 0)
            {
                //Report verification results
                log.Write("    Verification results", false);
                string verificationReport = regressionOutput.VerificationSummaryStat.GetReport(6);
                log.Write(verificationReport);
                log.Write(string.Empty);
            }

            //Perform prediction in case the input feeding is continuous (we know the input but we don't know the ideal output)
            if (demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.FeedingType == NeuralPreprocessor.InputFeedingType.Continuous)
            {
                double[] predictionOutputVector = stateMachine.Compute(predictionInputVector);
                string predictionReport = stateMachine.RL.GetForecastReport(predictionOutputVector, 6);
                log.Write("    Forecasts", false);
                log.Write(predictionReport);
                log.Write(string.Empty);
            }
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
            log.Write("State Machine demo started");
            //Instantiate demo settings from the xml file
            DemoSettings demoSettings = new DemoSettings(demoSettingsXmlFile);
            //Loop through all demo cases
            Stopwatch sw = new Stopwatch();
            foreach (DemoSettings.CaseSettings demoCaseParams in demoSettings.CaseCfgCollection)
            {
                sw.Reset();
                sw.Start();
                //Execute the demo case
                PerformDemoCase(log, demoCaseParams);
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                log.Write("Run time of demo case: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
                log.Write(string.Empty);
                log.Write(string.Empty);
            }
            log.Write("State Machine demo finished");
            log.Write(string.Empty);
            return;
        }
    }//SMDemo

}//Namespace
