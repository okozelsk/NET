using Demo.DemoConsoleApp.Log;
using RCNet.CsvTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Readout;
using System;
using System.Diagnostics;

namespace Demo.DemoConsoleApp.SM
{
    /// <summary>
    /// Performs the demo cases defined in xml file, demonstrates the State Machine usage.
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
        /// Displays an information about the verification progress.
        /// </summary>
        /// <param name="totalNumOfInputs">The total number of inputs to be processed.</param>
        /// <param name="numOfProcessedInputs">The number of already processed inputs.</param>
        private void OnVerificationProgressChanged(int totalNumOfInputs, int numOfProcessedInputs)
        {
            //Display progress
            if (numOfProcessedInputs % 10 == 0 || numOfProcessedInputs == totalNumOfInputs || totalNumOfInputs == 1)
            {
                _log.Write($"    Computing verification data {totalNumOfInputs}/{numOfProcessedInputs}", true);
            }
            return;
        }

        /// <summary>
        /// Displays an information about the preprocessing progress and at the end displays important NeuralPreprocessor's statistics.
        /// </summary>
        /// <param name="totalNumOfInputs">The total number of inputs to be processed.</param>
        /// <param name="numOfProcessedInputs">The number of already processed inputs.</param>
        /// <param name="finalPreprocessingOverview">The final overview of the preprocessing.</param>
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
                    _log.Write($"    Neural preprocessing and collection of State Machine predictors {totalNumOfInputs}/{numOfProcessedInputs}", true);
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
        /// Displays information about the build process progress.
        /// </summary>
        /// <param name="buildProgress">The current state of the build process.</param>
        /// <param name="foundBetter">Indicates that the best network so far was found during the last performed epoch.</param>
        private void OnEpochDone(TNRNetBuilder.BuildProgress buildProgress, bool foundBetter)
        {
            int reportEpochsInterval = 5;
            //Progress info
            if (foundBetter ||
                (buildProgress.Epoch % reportEpochsInterval) == 0 ||
                 buildProgress.Epoch == buildProgress.MaxEpochs ||
                (buildProgress.Epoch == 1 && buildProgress.AttemptNumber == 1)
                )
            {
                //Build progress report message
                string progressText = buildProgress.GetInfo(4);
                //Report the progress
                _log.Write(progressText, !(buildProgress.Epoch == 1 && buildProgress.AttemptNumber == 1));
            }
            return;
        }

        //Methods
        /// <summary>
        /// Performs the demo case.
        /// </summary>
        /// <param name="demoCaseCfg">The configuration of the demo case to be performed.</param>
        public void PerformDemoCase(SMDemoSettings.CaseSettings demoCaseCfg)
        {
            bool continuousFeedingDataFormat = false;
            //Prediction input vector (relevant only for input continuous feeding)
            double[] predictionInputVector = null;
            //Log start
            _log.Write("  Performing demo case " + demoCaseCfg.Name, false);
            _log.Write(" ", false);
            //Instantiate the StateMachine
            StateMachine stateMachine = new StateMachine(demoCaseCfg.StateMachineCfg);
            //////////////////////////////////////////////////////////////////////////////////////
            //Train StateMachine
            //Register to EpochDone event
            stateMachine.RL.EpochDone += OnEpochDone;
            StateMachine.TrainingResults trainingResults;
            CsvDataHolder trainingCsvData = new CsvDataHolder(demoCaseCfg.TrainingDataFileName);
            VectorBundle trainingData;
            if (trainingCsvData.ColNameCollection.NumOfStringValues > 0)
            {
                //Continuous feeding data format
                continuousFeedingDataFormat = true;
                //Check NeuralPreprocessor is not bypassed
                if (stateMachine.NP == null)
                {
                    throw new InvalidOperationException($"Incorrect file format. When NeuralPreprocessor is bypassed, only patterned data are allowed.");
                }
                trainingData = VectorBundle.Load(trainingCsvData,
                                                 demoCaseCfg.StateMachineCfg.NeuralPreprocessorCfg.InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.GetFieldNames(),
                                                 demoCaseCfg.StateMachineCfg.ReadoutLayerCfg.OutputFieldNameCollection,
                                                 out predictionInputVector
                                                 );
            }
            else
            {
                //Patterned feeding data format
                trainingData = VectorBundle.Load(trainingCsvData, demoCaseCfg.StateMachineCfg.ReadoutLayerCfg.OutputFieldNameCollection.Count);
            }
            if (stateMachine.NP != null)
            {
                //Register to PreprocessingProgressChanged event
                stateMachine.NP.PreprocessingProgressChanged += OnPreprocessingProgressChanged;
            }
            //Training
            trainingResults = stateMachine.Train(trainingData);
            _log.Write(string.Empty);
            //Report training (regression) results
            _log.Write("    Training results", false);
            string trainingReport = trainingResults.RegressionResults.GetTrainingResultsReport(6);
            _log.Write(trainingReport);
            _log.Write(string.Empty);

            //////////////////////////////////////////////////////////////////////////////////////
            //Verification of training quality on verification data
            if (demoCaseCfg.VerificationDataFileName.Length > 0)
            {
                stateMachine.VerificationProgressChanged += OnVerificationProgressChanged;
                StateMachine.VerificationResults verificationResults;
                CsvDataHolder verificationCsvData = new CsvDataHolder(demoCaseCfg.VerificationDataFileName);
                VectorBundle verificationData;
                if (continuousFeedingDataFormat)
                {
                    //Continuous input feeding
                    //Last known input values from training (predictionInputVector) must be pushed into the reservoirs to keep time series continuity
                    //(first input data in verification.csv is output of the last data in training.csv)
                    double[] tmp = stateMachine.Compute(predictionInputVector, out ReadoutLayer.ReadoutData readoutData);
                    //Load verification data and get new predictionInputVector for final prediction
                    verificationData = VectorBundle.Load(verificationCsvData,
                                                         demoCaseCfg.StateMachineCfg.NeuralPreprocessorCfg.InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.GetFieldNames(),
                                                         demoCaseCfg.StateMachineCfg.ReadoutLayerCfg.OutputFieldNameCollection,
                                                         out predictionInputVector
                                                         );
                }
                else
                {
                    //Patterned feeding data format
                    verificationData = VectorBundle.Load(verificationCsvData, demoCaseCfg.StateMachineCfg.ReadoutLayerCfg.OutputFieldNameCollection.Count);
                }
                verificationResults = stateMachine.Verify(verificationData);
                _log.Write(string.Empty);
                //Report verification results
                _log.Write("    Verification results", false);
                _log.Write(verificationResults.GetReport(6));
                _log.Write(string.Empty);
            }

            //Perform prediction in case of input feeding is continuous (we know the input but we don't know the ideal output)
            if (continuousFeedingDataFormat)
            {
                double[] predictionOutputVector = stateMachine.Compute(predictionInputVector, out ReadoutLayer.ReadoutData readoutData);
                string predictionReport = stateMachine.RL.GetForecastReport(predictionOutputVector, 6);
                _log.Write("    Forecasts", false);
                _log.Write(predictionReport);
                _log.Write(string.Empty);
            }

            return;
        }

        /// <summary>
        /// Runs the State Machine demo.
        /// Executes the demo cases defined in xml file one by one.
        /// </summary>
        /// <param name="demoCasesXmlFile">The name of the xml file containing the definitions of demo cases to be performed.</param>
        public void RunDemo(string demoCasesXmlFile)
        {
            _log.Write("State Machine demo started");
            //Instantiate the demo configuration from the xml file
            SMDemoSettings demoCfg = new SMDemoSettings(demoCasesXmlFile);
            //Loop through the demo cases
            Stopwatch sw = new Stopwatch();
            foreach (SMDemoSettings.CaseSettings caseCfg in demoCfg.CaseCfgCollection)
            {
                sw.Reset();
                sw.Start();
                //Execute the demo case
                PerformDemoCase(caseCfg);
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
