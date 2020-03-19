﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Neural.Data.Filter;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Common interface for input data transformers.
    /// </summary>
    public interface ITransformer
    {
        /// <summary>
        /// Resets transformer to its initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Computes transformed value
        /// </summary>
        /// <param name="data">Collection of natural values of the already known input fields</param>
        double Next(double[] data);

    }//ITransformer

}//Namespace
