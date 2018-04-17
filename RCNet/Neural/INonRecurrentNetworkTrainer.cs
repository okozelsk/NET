using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.Neural
{
    interface INonRecurrentNetworkTrainer
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
        /// Network beeing trained
        /// </summary>
        INonRecurrentNetwork Net { get; }

        /// <summary>
        /// Performs training iteration.
        /// </summary>
        void Iteration();

    }//INonRecurrentNetworkTrainer

}//Namespace
