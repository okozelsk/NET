namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Common interface of non-recurrent trainers
    /// </summary>
    public interface INonRecurrentNetworkTrainer
    {
        //Properties
        /// <summary>
        /// Epoch error (MSE).
        /// </summary>
        double MSE { get; }
        /// <summary>
        /// Max number of training attempts
        /// </summary>
        int MaxAttempt { get; }
        /// <summary>
        /// Current training attempt number
        /// </summary>
        int Attempt { get; }
        /// <summary>
        /// Max number of epochs within the training attempt
        /// </summary>
        int MaxAttemptEpoch { get; }
        /// <summary>
        /// Current epoch number within the current training attempt
        /// </summary>
        int AttemptEpoch { get; }
        /// <summary>
        /// A non-recurrent network that is being trained
        /// </summary>
        INonRecurrentNetwork Net { get; }
        /// <summary>
        /// An informative message sent from the trainer
        /// </summary>
        string InfoMessage { get; }

        /// <summary>
        /// Starts next training attempt
        /// </summary>
        bool NextAttempt();

        /// <summary>
        /// Performs the training epoch.
        /// </summary>
        bool Iteration();


    }//INonRecurrentNetworkTrainer

}//Namespace
