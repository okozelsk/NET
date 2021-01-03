using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "ActivationDiffLinWAvg" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value is a linearly weighted average computed from the differences of the activation function results (A[T] - A[T-1]).
    /// </remarks>
    [Serializable]
    public class PredictorActivationDiffLinWAvg : IPredictor
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
            PredictorActivationDiffLinWAvgSettings cfg = (PredictorActivationDiffLinWAvgSettings)Cfg;
            if (cfg.Window == PredictorActivationDiffLinWAvgSettings.NAWindowNum)
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
            PredictorActivationDiffLinWAvgSettings cfg = (PredictorActivationDiffLinWAvgSettings)Cfg;
            if (cfg.Window == PredictorActivationDiffLinWAvgSettings.NAWindowNum)
            {
                return _continuousAvg.Result;
            }
            else
            {
                if (activationMDW.UsedCapacity >= cfg.Window)
                {
                    return activationMDW.GetDataDiffLinWeightedAvg(cfg.Window).Result;
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorActivationDiffLinWAvg
}//Namespace
