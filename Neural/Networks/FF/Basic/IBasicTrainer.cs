using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Neural.Networks.FF.Basic
{
    interface IBasicTrainer
    {
        //Properties
        /// <summary>
        /// Epoch error (MSE).
        /// </summary>
        double MSE { get; }
        /// <summary>
        /// Current epoch (incemented by call of Iteration)
        /// </summary>
        int Epoch { get; }
        /// <summary>
        /// Training FF BasicNetwork
        /// </summary>
        BasicNetwork Net { get; }

        /// <summary>
        /// Performs training iteration.
        /// </summary>
        void Iteration();

    }
}
