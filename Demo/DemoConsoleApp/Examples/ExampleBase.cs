using Demo.DemoConsoleApp.Log;
using RCNet.CsvTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Network.SM.Readout;


namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Implements the base class of the examples.
    /// </summary>
    public class ExampleBase
    {
        //Attributes
        protected readonly IOutputLog _log;

        //Constructor
        protected ExampleBase()
        {
            _log = new ConsoleLog();
            return;
        }

        //Methods
        //Event handlers
        /// <summary>
        /// Displays an information about the verification progress.
        /// </summary>
        /// <param name="totalNumOfInputs">The total number of inputs to be processed.</param>
        /// <param name="numOfProcessedInputs">The number of already processed inputs.</param>
        protected void OnVerificationProgressChanged(int totalNumOfInputs, int numOfProcessedInputs)
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
        protected void OnPreprocessingProgressChanged(int totalNumOfInputs,
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
        protected void OnEpochDone(TNRNetBuilder.BuildProgress buildProgress, bool foundBetter)
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

        /// <summary>
        /// Loads the specified file and executes the StateMachine training.
        /// </summary>
        /// <param name="stateMachine">An instance of StateMachine to be trained.</param>
        /// <param name="trainingDataFileName">The name of the csv file containing the training data.</param>
        /// <param name="predictionInputVector">The vector to be used for next prediction (relevant only in case of continuous feeding of the input).</param>
        protected void TrainStateMachine(StateMachine stateMachine, string trainingDataFileName, out double[] predictionInputVector)
        {
            //Register to EpochDone event
            stateMachine.RL.EpochDone += OnEpochDone;
            //Load csv data
            CsvDataHolder trainingCsvData = new CsvDataHolder(trainingDataFileName);
            //Convert csv data to VectorBundle useable for StateMachine training
            VectorBundle trainingData;
            if (stateMachine.Config.NeuralPreprocessorCfg != null)
            {
                //Neural preprocessing is enabled
                if (stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Continuous)
                {
                    //Continuous feeding data format
                    trainingData = VectorBundle.Load(trainingCsvData,
                                                     stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.GetFieldNames(),
                                                     stateMachine.Config.ReadoutLayerCfg.OutputFieldNameCollection,
                                                     out predictionInputVector
                                                     );
                }
                else
                {
                    //Patterned feeding data format
                    predictionInputVector = null;
                    trainingData = VectorBundle.Load(trainingCsvData,
                                                     stateMachine.Config.ReadoutLayerCfg.OutputFieldNameCollection.Count
                                                     );
                }
                //Register to PreprocessingProgressChanged event
                stateMachine.NP.PreprocessingProgressChanged += OnPreprocessingProgressChanged;
            }
            else
            {
                //Neural preprocessing is bypassed
                predictionInputVector = null;
                trainingData = VectorBundle.Load(trainingCsvData,
                                                 stateMachine.Config.ReadoutLayerCfg.OutputFieldNameCollection.Count
                                                 );
            }
            //StateMachine training
            StateMachine.TrainingResults trainingResults = stateMachine.Train(trainingData);
            _log.Write(string.Empty);
            //Report training results
            _log.Write("    Training results", false);
            string trainingReport = trainingResults.RegressionResults.GetTrainingResultsReport(6);
            _log.Write(trainingReport);
            _log.Write(string.Empty);
            //Finished
            return;
        }

        /// <summary>
        /// Loads the specified file and executes the StateMachine verification.
        /// </summary>
        /// <param name="stateMachine">An instance of StateMachine to be verified.</param>
        /// <param name="verificationDataFileName">The name of the csv file containing the verification data.</param>
        /// <param name="omittedInputVector">Remaining input vector from training phase (relevant only in case of continuous feeding of the input).</param>
        /// <param name="predictionInputVector">The vector to be used for next prediction (relevant only in case of continuous feeding of the input).</param>
        protected void VerifyStateMachine(StateMachine stateMachine, string verificationDataFileName, double[] omittedInputVector, out double[] predictionInputVector)
        {
            //Load csv data
            CsvDataHolder verificationCsvData = new CsvDataHolder(verificationDataFileName);
            //Convert csv data to VectorBundle useable for StateMachine verification
            VectorBundle verificationData;
            //Check NeuralPreprocessor is configured
            if (stateMachine.Config.NeuralPreprocessorCfg != null)
            {
                //Neural preprocessing is enabled
                if (stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Continuous)
                {
                    //Continuous input feeding
                    //Last known input values from training (predictionInputVector) must be pushed into the reservoirs to keep time series continuity
                    //(first input data in verification.csv is output of the last data in training.csv)
                    double[] tmp = stateMachine.Compute(omittedInputVector, out ReadoutLayer.ReadoutData readoutData);
                    //Load verification data and get new predictionInputVector for final prediction
                    verificationData = VectorBundle.Load(verificationCsvData,
                                                         stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.GetFieldNames(),
                                                         stateMachine.Config.ReadoutLayerCfg.OutputFieldNameCollection,
                                                         out predictionInputVector
                                                         );
                }
                else
                {
                    predictionInputVector = null;
                    //Patterned feeding data format
                    verificationData = VectorBundle.Load(verificationCsvData, stateMachine.Config.ReadoutLayerCfg.OutputFieldNameCollection.Count);
                }
            }
            else
            {
                //Neural preprocessing is bypassed
                predictionInputVector = null;
                verificationData = VectorBundle.Load(verificationCsvData, stateMachine.Config.ReadoutLayerCfg.OutputFieldNameCollection.Count);
            }
            //StateMachine verification
            //Register to VerificationProgressChanged event
            stateMachine.VerificationProgressChanged += OnVerificationProgressChanged;
            StateMachine.VerificationResults verificationResults = stateMachine.Verify(verificationData);
            _log.Write(string.Empty);
            //Report verification results
            _log.Write("    Verification results", false);
            _log.Write(verificationResults.GetReport(6));
            _log.Write(string.Empty);

            //Finished
            return;
        }


    }//ExampleBase

}//Namespace
