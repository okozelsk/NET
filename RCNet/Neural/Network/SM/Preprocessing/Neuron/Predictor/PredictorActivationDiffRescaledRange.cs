using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "ActivationDiffRescaledRange" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value is a rescaled range computed from the differences of the activation function results (A[T] - A[T-1]).
    /// </remarks>
    [Serializable]
    public class PredictorActivationDiffRescaledRange : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public PredictorActivationDiffRescaledRange(PredictorActivationDiffRescaledRangeSettings cfg)
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
            PredictorActivationDiffRescaledRangeSettings cfg = (PredictorActivationDiffRescaledRangeSettings)Cfg;
            if (activationMDW.UsedCapacity >= cfg.Window)
            {
                return activationMDW.GetDataDiffRescaledRange(cfg.Window);
            }
            else
            {
                return 0d;
            }
        }

    }//PredictorActivationDiffRescaledRange
}//Namespace
