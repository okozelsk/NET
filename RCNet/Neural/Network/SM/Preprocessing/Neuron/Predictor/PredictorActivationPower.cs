using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Powered absolute value of the result of the activation function.
    /// </summary>
    [Serializable]
    public class PredictorActivationPower : IPredictor
    {
        //Attribute properties
        /// <summary>
        /// Configuration of the predictor
        /// </summary>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
        public PredictorActivationPower(PredictorActivationPowerSettings cfg)
        {
            Cfg = cfg;
            return;
        }

        /// <summary>
        /// Resets the predictor computer
        /// </summary>
        public void Reset()
        {
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
            PredictorActivationPowerSettings cfg = (PredictorActivationPowerSettings)Cfg;
            return Math.Pow(Math.Abs(activation), cfg.Exponent) * (cfg.KeepSign ? Math.Sign(activation) : 1d);
        }

    }//PredictorActivationPower
}//Namespace
