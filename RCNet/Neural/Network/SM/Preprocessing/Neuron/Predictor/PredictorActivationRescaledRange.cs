using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "ActivationRescaledRange" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value is a rescaled range computed from the activation function results.
    /// </remarks>
    [Serializable]
    public class PredictorActivationRescaledRange : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public PredictorActivationRescaledRange(PredictorActivationRescaledRangeSettings cfg)
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
            PredictorActivationRescaledRangeSettings cfg = (PredictorActivationRescaledRangeSettings)Cfg;
            if (activationMDW.UsedCapacity >= cfg.Window)
            {
                return activationMDW.GetDataRescaledRange(cfg.Window);
            }
            else
            {
                return 0d;
            }
        }

    }//PredictorActivationRescaledRange
}//Namespace
