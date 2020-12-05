using System;
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
