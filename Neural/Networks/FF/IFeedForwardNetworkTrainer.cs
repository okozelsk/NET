using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural.Networks.FF
{
    interface IFeedForwardNetworkTrainer
    {
        //Properties
        /// <summary>
        /// Epoch error (MSE).
        /// </summary>
        double MSE { get; }
        /// <summary>
        /// Current epoch (incremented each call of Iteration)
        /// </summary>
        int Epoch { get; }
        /// <summary>
        /// Trainee FF network
        /// </summary>
        FeedForwardNetwork Net { get; }

        /// <summary>
        /// Performs training iteration.
        /// </summary>
        void Iteration();

    }
}
