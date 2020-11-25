﻿using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Linearly weighted average computed from the differences of the activation function results (A[T] - A[T-1]).
    /// </summary>
    [Serializable]
    public class PredictorActivationDiffLinWAvg : IPredictor
    {
        //Attribute properties
        /// <summary>
        /// Configuration of the predictor
        /// </summary>
        public IPredictorSettings Cfg { get; }

        //Attributes
        private readonly WeightedAvg _continuousAvg;
        private double _continuousWeight;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
        public PredictorActivationDiffLinWAvg(PredictorActivationDiffLinWAvgSettings cfg)
        {
            Cfg = cfg;
            if (cfg.Window == PredictorActivationDiffLinWAvgSettings.NAWindowNum)
            {
                _continuousAvg = new WeightedAvg();
                Reset();
            }
            else
            {
                _continuousAvg = null;
            }
            return;
        }

        /// <summary>
        /// Resets the predictor computer
        /// </summary>
        public void Reset()
        {
            _continuousAvg?.Reset();
            _continuousWeight = 0d;
            return;
        }

        /// <summary>
        /// Updates the predictor computer
        /// </summary>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="normalizedActivation">Current value of the activation normalized between 0 and 1</param>
        /// <param name="spike">Indicates whether the neuron is firing</param>
        public void Update(double activation, double normalizedActivation, bool spike)
        {
            PredictorActivationDiffLinWAvgSettings cfg = (PredictorActivationDiffLinWAvgSettings)Cfg;
            if (cfg.Window == PredictorActivationDiffLinWAvgSettings.NAWindowNum)
            {
                ++_continuousWeight;
                _continuousAvg.AddSampleValue(activation, _continuousWeight);
            }
            return;
        }

        /// <summary>
        /// Computes the predictor value
        /// </summary>
        /// <param name="continuousActivationStat">Continuous statistics of the activations</param>
        /// <param name="continuousActivationDiffStat">Continuous statistics of the activation differences</param>
        /// <param name="activationMDW">Moving window of the activations</param>
        /// <param name="firingMDW">Moving window of the firings</param>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="normalizedActivation">Current value of the activation normalized between 0 and 1</param>
        /// <param name="spike">Indicates whether the neuron is firing</param>
        /// <returns>Computed predictor</returns>
        public double Compute(BasicStat continuousActivationStat,
                              BasicStat continuousActivationDiffStat,
                              MovingDataWindow activationMDW,
                              SimpleQueue<byte> firingMDW,
                              double activation,
                              double normalizedActivation,
                              bool spike
                       )
        {
            PredictorActivationDiffLinWAvgSettings cfg = (PredictorActivationDiffLinWAvgSettings)Cfg;
            if(cfg.Window == PredictorActivationDiffLinWAvgSettings.NAWindowNum)
            {
                return _continuousAvg.Avg;
            }
            else
            {
                if (activationMDW.NumOfSamples >= cfg.Window)
                {
                    return activationMDW.GetDataDiffLinWeightedAvg(cfg.Window).Avg;
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorActivationDiffLinWAvg
}//Namespace
