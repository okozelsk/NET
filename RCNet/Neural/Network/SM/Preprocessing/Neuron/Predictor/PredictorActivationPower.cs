using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "ActivationPower" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value is the powered absolute value of the current activation.
    /// </remarks>
    [Serializable]
    public class PredictorActivationPower : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public PredictorActivationPower(PredictorActivationPowerSettings cfg)
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
            PredictorActivationPowerSettings cfg = (PredictorActivationPowerSettings)Cfg;
            return Math.Pow(Math.Abs(activation), cfg.Exponent) * (cfg.KeepSign ? Math.Sign(activation) : 1d);
        }

    }//PredictorActivationPower
}//Namespace
