using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural
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
        /// Max attempt
        /// </summary>
        int MaxAttempt { get; }
        /// <summary>
        /// Current attempt
        /// </summary>
        int Attempt { get; }
        /// <summary>
        /// Max epoch
        /// </summary>
        int MaxAttemptEpoch { get; }
        /// <summary>
        /// Current epoch (incremented each call of Iteration)
        /// </summary>
        int AttemptEpoch { get; }
        /// <summary>
        /// Network beeing trained
        /// </summary>
        INonRecurrentNetwork Net { get; }
        /// <summary>
        /// Informative message from the trainer
        /// </summary>
        string InfoMessage { get; }

        /// <summary>
        /// Starts next training attempt
        /// </summary>
        bool NextAttempt();
        /// <summary>
        /// Performs training iteration.
        /// </summary>
        bool Iteration();


    }//INonRecurrentNetworkTrainer

}//Namespace
