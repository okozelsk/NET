using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Linearly weighted average computed from the activation function results.
    /// </summary>
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
        /// Creates an initialized instance
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
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
                _continuousAvg.AddSampleValue(activation, _continuousWeight);
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
            if(cfg.Window == PredictorActivationLinWAvgSettings.NAWindowNum)
            {
                return _continuousAvg.Avg;
            }
            else
            {
                if (activationMDW.NumOfSamples >= cfg.Window)
                {
                    return activationMDW.GetDataLinWeightedAvg(cfg.Window).Avg;
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorActivationLinWAvg
}//Namespace
