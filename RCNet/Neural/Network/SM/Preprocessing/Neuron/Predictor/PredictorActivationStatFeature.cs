using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Statistical feature computed from the activation function results.
    /// </summary>
    [Serializable]
    public class PredictorActivationStatFeature : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
        public PredictorActivationStatFeature(PredictorActivationStatFeatureSettings cfg)
        {
            Cfg = cfg;
            return;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            return;
        }

        /// <inheritdoc/>
        public void Update(double activation, double normalizedActivation, bool spike)
        {
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
            PredictorActivationStatFeatureSettings cfg = (PredictorActivationStatFeatureSettings)Cfg;
            if(cfg.Window == PredictorActivationStatFeatureSettings.NAWindowNum)
            {
                return continuousActivationStat.Get(cfg.Feature);
            }
            else
            {
                if (activationMDW.NumOfSamples >= cfg.Window)
                {
                    return activationMDW.GetDataStat(cfg.Window).Get(cfg.Feature);
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorActivationStatFeature
}//Namespace
