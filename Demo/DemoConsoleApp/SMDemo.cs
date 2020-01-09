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
    public class SMDemo
    {
        //Attributes
        private readonly IOutputLog _log;

        //Constructor
        public SMDemo(IOutputLog log)
        {
            _log = log;
            return;
        }

        //Event handlers
        /// <summary>
        /// Displays information about the verification progress.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        private void OnVerificationProgressChanged(int totalNumOfInputs, int numOfProcessedInputs)
        {
            //Display progress
            if (numOfProcessedInputs % 10 == 0 || numOfProcessedInputs == totalNumOfInputs || totalNumOfInputs == 1)
            {
                _log.Write($"    Computing verification data {totalNumOfInputs.ToString()}/{numOfProcessedInputs.ToString()}", true);
            }
            return;
        }


        //Methods

        /// <summary>
        /// Displays information about the preprocessing progress and at the end displays important NeuralPreprocessor's statistics.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="finalPreprocessingOverview">Final overview of the preprocessing phase</param>
        private void OnPreprocessingProgressChanged(int totalNumOfInputs,
                                                    int numOfProcessedInputs,
                                                    NeuralPreprocessor.PreprocessingOverview finalPreprocessingOverview
                                                    )
        {
            if (finalPreprocessingOverview == null)
            {
                //Display progress
                if (numOfProcessedInputs % 10 == 0 || numOfProcessedInputs == totalNumOfInputs || totalNumOfInputs == 1)
                {
                    _log.Write($"    Neural preprocessing and collection of State Machine predictors {totalNumOfInputs.ToString()}/{numOfProcessedInputs.ToString()}", true);
                }
            }
            else
            {
                //Display preprocessing final information
                _log.Write(string.Empty);
                _log.Write(finalPreprocessingOverview.CreateReport(4));
                _log.Write(string.Empty);
            }
            return;
        }

        /// <summary>
        /// Displays information about the readout unit regression progress.
        /// </summary>
        /// <param name="regrState">Current state of the regression process</param>
        /// <param name="bestUnitChanged">Indicates that the best readout unit was changed as a result of the performed epoch</param>
        private void OnRegressionEpochDone(ReadoutUnitBuilder.RegrState regrState, bool bestUnitChanged)
        {
            int reportEpochsInterval = 5;
            //Progress info
            if (bestUnitChanged ||
                (regrState.Epoch % reportEpochsInterval) == 0 ||
                regrState.Epoch == regrState.MaxEpochs ||
                (regrState.Epoch == 1 && regrState.RegrAttemptNumber == 1)
                )
            {
                //Build progress report message
                string progressText = regrState.GetProgressInfo(4);
                //Report the progress
                _log.Write(progressText, !(regrState.Epoch == 1 && regrState.RegrAttemptNumber == 1));
            }
            return;
        }

        /// <summary>
        /// Performs one demo case.
        /// </summary>
        /// <param name="demoCaseParams">An instance of DemoSettings.CaseSettings to be performed</param>
        public void PerformDemoCase(DemoSettings.CaseSettings demoCaseParams)
        {
            bool continuousFeedingData = false;
            //Prediction input vector (relevant only for input continuous feeding)
            double[] predictionInputVector = null;
            //Log start
            _log.Write("  Performing demo case " + demoCaseParams.Name, false);
            _log.Write(" ", false);
            //Instantiate the StateMachine
            StateMachine stateMachine = new StateMachine(demoCaseParams.StateMachineCfg);
            //////////////////////////////////////////////////////////////////////////////////////
            //Train StateMachine
            //Register to RegressionEpochDone event
            stateMachine.RL.RegressionEpochDone += OnRegressionEpochDone;
            StateMachine.TrainingResults trainingResults;
            if (stateMachine.NP != null)
            {
                //Register to PreprocessingProgressChanged event
                stateMachine.NP.PreprocessingProgressChanged += OnPreprocessingProgressChanged;
                if (demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.FeedingType == NeuralPreprocessor.InputFeedingType.Continuous)
                {
                    //Continuous input feeding
                    continuousFeedingData = true;
                    //Load data bundle from csv file
                    VectorBundle trainingData = VectorBundle.LoadFromCsv(demoCaseParams.TrainingDataFileName,
                                                                         demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.ExternalFieldNameCollection(),
                                                                         demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection,
                                                                         out predictionInputVector
                                                                         );
                    trainingResults = stateMachine.Train(trainingData);
                }
                else
                {
                    //Patterned input feeding
                    //Load data bundle from csv file
                    PatternBundle trainingData = PatternBundle.LoadFromCsv(demoCaseParams.TrainingDataFileName,
                                                                           demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.ExternalFieldNameCollection(),
                                                                           demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection
                                                                           );
                    trainingResults = stateMachine.Train(trainingData);
                }
            }
            else
            {
                //Neural preprocessor is bypassed
                bool patternedData = PatternBundle.CheckIsPatternedCsv(demoCaseParams.TrainingDataFileName);
                if (!patternedData)
                {
                    //Continuous input feeding
                    continuousFeedingData = true;
                    //Load data bundle from csv file
                    VectorBundle trainingData = VectorBundle.LoadFromCsv(demoCaseParams.TrainingDataFileName,
                                                                         null,
                                                                         demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection,
                                                                         out predictionInputVector
                                                                         );
                    trainingResults = stateMachine.Train(trainingData);
                }
                else
                {
                    //Patterned input feeding
                    //Load data bundle from csv file
                    PatternBundle trainingData = PatternBundle.LoadFromCsv(demoCaseParams.TrainingDataFileName,
                                                                           null,
                                                                           demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection
                                                                           );
                    trainingResults = stateMachine.Train(trainingData);
                }
            }

            _log.Write(string.Empty);

            //Report training (regression) results
            _log.Write("    Training results", false);
            string trainingReport = trainingResults.RegressionResults.GetTrainingResultsReport(6);
            _log.Write(trainingReport);
            _log.Write(string.Empty);

            //////////////////////////////////////////////////////////////////////////////////////
            //Verification of training quality on verification data
            if (demoCaseParams.VerificationDataFileName.Length > 0)
            {
                stateMachine.VerificationProgressChanged += OnVerificationProgressChanged;
                StateMachine.VerificationResults verificationResults;
                if (continuousFeedingData)
                {
                    //Continuous input feeding
                    //Last known values from training must be pushed into the reservoirs to keep time series continuity
                    //(first data in verification.csv is output of the last data in training.csv)
                    double[] predictionOutputVector = stateMachine.Compute(predictionInputVector);
                    //Load data bundle from csv file
                    VectorBundle verificationData = VectorBundle.LoadFromCsv(demoCaseParams.VerificationDataFileName,
                                                                                stateMachine.NP != null ? demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.ExternalFieldNameCollection() : null,
                                                                                demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection,
                                                                                out predictionInputVector
                                                                                );
                    verificationResults = stateMachine.Verify(verificationData);
                }
                else
                {
                    //Patterned input feeding
                    //Load data bundle from csv file
                    PatternBundle verificationData = PatternBundle.LoadFromCsv(demoCaseParams.VerificationDataFileName,
                                                                                stateMachine.NP != null ? demoCaseParams.StateMachineCfg.NeuralPreprocessorConfig.InputConfig.ExternalFieldNameCollection() : null,
                                                                                demoCaseParams.StateMachineCfg.ReadoutLayerConfig.OutputFieldNameCollection
                                                                                );
                    verificationResults = stateMachine.Verify(verificationData);
                }
                _log.Write(string.Empty);
                //Report verification results
                _log.Write("    Verification results", false);
                _log.Write(verificationResults.GetReport(6));
                _log.Write(string.Empty);
            }

            //Perform prediction in case the input feeding is continuous (we know the input but we don't know the ideal output)
            if (continuousFeedingData)
            {
                double[] predictionOutputVector = stateMachine.Compute(predictionInputVector);
                string predictionReport = stateMachine.RL.GetForecastReport(predictionOutputVector, 6);
                _log.Write("    Forecasts", false);
                _log.Write(predictionReport);
                _log.Write(string.Empty);
            }
            return;
        }

        /// <summary>
        /// Runs State Machine demo. This is the main function.
        /// Executes demo cases defined in xml file.
        /// </summary>
        /// <param name="demoSettingsXmlFile">Xml file containing definitions of demo cases to be performed</param>
        public void RunDemo(string demoSettingsXmlFile)
        {
            _log.Write("State Machine demo started");
            //Instantiate demo settings from the xml file
            DemoSettings demoSettings = new DemoSettings(demoSettingsXmlFile);
            //Loop through all demo cases
            Stopwatch sw = new Stopwatch();
            foreach (DemoSettings.CaseSettings demoCaseParams in demoSettings.CaseCfgCollection)
            {
                sw.Reset();
                sw.Start();
                //Execute the demo case
                PerformDemoCase(demoCaseParams);
                sw.Stop();
                TimeSpan ts = sw.Elapsed;
                _log.Write("  Run time of demo case: " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
                _log.Write(string.Empty);
                _log.Write(string.Empty);
            }
            _log.Write("State Machine demo finished");
            _log.Write(string.Empty);
            return;
        }
    }//SMDemo

}//Namespace
