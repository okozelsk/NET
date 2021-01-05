namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Common interface of the non-recurrent network trainers.
    /// </summary>
    public interface INonRecurrentNetworkTrainer
    {
        //Properties
        /// <summary>
        /// The error of the last training epoch (MSE).
        /// </summary>
        double MSE { get; }
        /// <summary>
        /// The max number of the training attempts.
        /// </summary>
        int MaxAttempt { get; }
        /// <summary>
        /// The current training attempt number.
        /// </summary>
        int Attempt { get; }
        /// <summary>
        /// The max number of the training epochs within a training attempt.
        /// </summary>
        int MaxAttemptEpoch { get; }
        /// <summary>
        /// The current epoch number within the current training attempt.
        /// </summary>
        int AttemptEpoch { get; }
        /// <summary>
        /// The non-recurrent network that is being trained.
        /// </summary>
        INonRecurrentNetwork Net { get; }
        /// <summary>
        /// An informative message sent by the trainer.
        /// </summary>
        string InfoMessage { get; }

        /// <summary>
        /// Starts the next training attempt.
        /// </summary>
        bool NextAttempt();

        /// <summary>
        /// Performs the next training epoch.
        /// </summary>
        bool Iteration();


    }//INonRecurrentNetworkTrainer

}//Namespace
