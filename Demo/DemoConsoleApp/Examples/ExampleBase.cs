using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Demo.DemoConsoleApp.Log;
using RCNet.CsvTools;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.SM;
using RCNet.Neural.Network.SM.Preprocessing;
using RCNet.Neural.Network.SM.Preprocessing.Input;

namespace Demo.DemoConsoleApp.Examples
{
    /// <summary>
    /// Base class of the implemented examples
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
        /// Displays information about the verification progress.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        protected void OnVerificationProgressChanged(int totalNumOfInputs, int numOfProcessedInputs)
        {
            //Display progress
            if (numOfProcessedInputs % 10 == 0 || numOfProcessedInputs == totalNumOfInputs || totalNumOfInputs == 1)
            {
                _log.Write($"    Computing verification data {totalNumOfInputs.ToString()}/{numOfProcessedInputs.ToString()}", true);
            }
            return;
        }

        /// <summary>
        /// Displays information about the preprocessing progress and at the end displays important NeuralPreprocessor's statistics.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="finalPreprocessingOverview">Final overview of the preprocessing phase</param>
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
        /// <param name="buildingState">Current state of the regression process</param>
        /// <param name="foundBetter">Indicates that the best readout unit was changed as a result of the performed epoch</param>
        protected void OnRegressionEpochDone(TrainedNetworkBuilder.BuildingState buildingState, bool foundBetter)
        {
            int reportEpochsInterval = 5;
            //Progress info
            if (foundBetter ||
                (buildingState.Epoch % reportEpochsInterval) == 0 ||
                buildingState.Epoch == buildingState.MaxEpochs ||
                (buildingState.Epoch == 1 && buildingState.RegrAttemptNumber == 1)
                )
            {
                //Build progress report message
                string progressText = buildingState.GetProgressInfo(4);
                //Report the progress
                _log.Write(progressText, !(buildingState.Epoch == 1 && buildingState.RegrAttemptNumber == 1));
            }
            return;
        }

        /// <summary>
        /// Loads given file and executes StateMachine training.
        /// This version of function requires configured NeuralPreprocessor.
        /// </summary>
        /// <param name="stateMachine">Instance of StateMachine to be trained</param>
        /// <param name="trainingDataFileName">Name of the csv file containing training data</param>
        /// <param name="predictionInputVector">Returned vector to be used for next prediction (relevant only in case of continuous feeding of the input)</param>
        protected void TrainStateMachine(StateMachine stateMachine, string trainingDataFileName, out double[] predictionInputVector)
        {
            //Check NeuralPreprocessor is configured
            if(stateMachine.Config.NeuralPreprocessorCfg == null)
            {
                throw new Exception("TrainStateMachine: Neural preprocessor has to be configured.");
            }
            //Register to RegressionEpochDone event
            stateMachine.RL.RegressionEpochDone += OnRegressionEpochDone;
            //Load csv data
            CsvDataHolder trainingCsvData = new CsvDataHolder(trainingDataFileName);
            //Convert csv data to VectorBundle useable for StateMachine training
            VectorBundle trainingData;
            if (stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Continuous)
            {
                //Continuous feeding data format
                trainingData = VectorBundle.Load(trainingCsvData,
                                                 stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.FieldsCfg.ExternalFieldsCfg.GetFieldNames(),
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
        /// Loads given file and executes StateMachine verification.
        /// This version of function requires configured NeuralPreprocessor.
        /// </summary>
        /// <param name="stateMachine">Instance of StateMachine to be trained</param>
        /// <param name="verificationDataFileName">Name of the csv file containing verification data</param>
        /// <param name="omittedInputVector">Remaining input vector from training phase (relevant only in case of continuous feeding of the input)</param>
        /// <param name="predictionInputVector">Returned vector to be used for next prediction (relevant only in case of continuous feeding of the input)</param>
        protected void VerifyStateMachine(StateMachine stateMachine, string verificationDataFileName, double[] omittedInputVector, out double[] predictionInputVector)
        {
            //Check NeuralPreprocessor is configured
            if (stateMachine.Config.NeuralPreprocessorCfg == null)
            {
                throw new Exception("TrainStateMachine: Neural preprocessor has to be configured.");
            }
            //Load csv data
            CsvDataHolder verificationCsvData = new CsvDataHolder(verificationDataFileName);
            //Convert csv data to VectorBundle useable for StateMachine verification
            VectorBundle verificationData;
            if (stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.FeedingCfg.FeedingType == InputEncoder.InputFeedingType.Continuous)
            {
                //Continuous input feeding
                //Last known input values from training (predictionInputVector) must be pushed into the reservoirs to keep time series continuity
                //(first input data in verification.csv is output of the last data in training.csv)
                double[] tmp = stateMachine.Compute(omittedInputVector);
                //Load verification data and get new predictionInputVector for final prediction
                verificationData = VectorBundle.Load(verificationCsvData,
                                                     stateMachine.Config.NeuralPreprocessorCfg.InputEncoderCfg.FieldsCfg.ExternalFieldsCfg.GetFieldNames(),
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
            //Register to VerificationProgressChanged event
            stateMachine.VerificationProgressChanged += OnVerificationProgressChanged;
            //StateMachine verification
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
