using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Result of the activation function
    /// </summary>
    [Serializable]
    public class PredictorActivation : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
        public PredictorActivation(PredictorActivationSettings cfg)
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
            return activation;
        }

    }//PredictorActivation
}//Namespace
