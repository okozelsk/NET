using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "ActivationLinWAvg" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value is a linearly weighted average computed from the activation function results.
    /// </remarks>
    [Serializable]
    public class PredictorActivationLinWAvg : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Attributes
        private readonly WeightedAvg _continuousAvg;
        private double _continuousWeight;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public PredictorActivationLinWAvg(PredictorActivationLinWAvgSettings cfg)
        {
            Cfg = cfg;
            if (cfg.Window == PredictorActivationLinWAvgSettings.NAWindowNum)
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

        /// <inheritdoc/>
        public void Reset()
        {
            _continuousAvg?.Reset();
            _continuousWeight = 0d;
            return;
        }

        /// <inheritdoc/>
        public void Update(double activation, double normalizedActivation, bool spike)
        {
            PredictorActivationLinWAvgSettings cfg = (PredictorActivationLinWAvgSettings)Cfg;
            if (cfg.Window == PredictorActivationLinWAvgSettings.NAWindowNum)
            {
                ++_continuousWeight;
                _continuousAvg.AddSample(activation, _continuousWeight);
            }
            return;
        }

        /// <inheritdoc/>
        public double Compute(BasicStat continuousActivationStat,
                              BasicStat continuousActivationDiffStat,
                              MovingDataWindow activationMDW,
                              SimpleQueue<byte> firingMDW,
                              double activation,
                              double normalizedActivation,
                              bool spike
                       )
        {
            PredictorActivationLinWAvgSettings cfg = (PredictorActivationLinWAvgSettings)Cfg;
            if (cfg.Window == PredictorActivationLinWAvgSettings.NAWindowNum)
            {
                return _continuousAvg.Result;
            }
            else
            {
                if (activationMDW.UsedCapacity >= cfg.Window)
                {
                    return activationMDW.GetDataLinWeightedAvg(cfg.Window).Result;
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorActivationLinWAvg
}//Namespace
