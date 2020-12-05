using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Rescalled range computed from the differences of the activation function results (A[T] - A[T-1]).
    /// </summary>
    [Serializable]
    public class PredictorActivationDiffRescalledRange : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
        public PredictorActivationDiffRescalledRange(PredictorActivationDiffRescalledRangeSettings cfg)
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
            PredictorActivationDiffRescalledRangeSettings cfg = (PredictorActivationDiffRescalledRangeSettings)Cfg;
            if (activationMDW.NumOfSamples >= cfg.Window)
            {
                return activationMDW.GetDataDiffRescalledRange(cfg.Window);
            }
            else
            {
                return 0d;
            }
        }

    }//PredictorActivationDiffRescalledRange
}//Namespace
