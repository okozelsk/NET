﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OKOSW.Neural.Reservoir.Analog
{
    /// <summary>
    /// Specifies common interface of any analog reservoir implementation
    /// </summary>
    public interface IAnalogReservoir
    {
        /// <summary>
        /// Reservoir ID.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Reservoir configuration name (together with ID should be unique).
        /// </summary>
        string ConfigName { get; }

        /// <summary>
        /// Reservoir size. (Reservoir neurons count)
        /// </summary>
        int Size { get; }

        /// <summary>
        /// Number of reservoir's output predictors
        /// </summary>
        int OutputPredictorsCount { get; }

        /// <summary>
        /// Reservoir neurons.
        /// </summary>
        AnalogNeuron[] Neurons { get; }

        /// <summary>
        /// Resets all reservoir neurons to their initial state (before boot state).
        /// Function does not affect weights or internal structure of the resservoir.
        /// </summary>
        void Reset();

        /// <summary>
        /// Computes reservoir neurons new states and returns new set of reservoir output predictors.
        /// </summary>
        /// <param name="input">Array of new input values.</param>
        /// <param name="outputPredictors">Array to be filled with output predictors values. Array has to be sized to OutputPredictorsCount reservoir property.</param>
        /// <param name="collectStatistics">Switch dictates, if to collect statistics. Typical usage is FALSE within boot phase and TRUE after boot phase.</param>
        void Compute(double[] input, double[] outputPredictors, bool collectStatistics);

        /// <summary>
        /// Sets feedback values for next computation round
        /// </summary>
        /// <param name="feedback">Feedback values.</param>
        void SetFeedback(double[] feedback);
    }
}
