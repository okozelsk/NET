using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "ActivationDiffStatFeature" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value is a statistical figure computed from the differences of the activation function results (A[T] - A[T-1]).
    /// </remarks>
    [Serializable]
    public class PredictorActivationDiffStatFigure : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public PredictorActivationDiffStatFigure(PredictorActivationDiffStatFigureSettings cfg)
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
            PredictorActivationDiffStatFigureSettings cfg = (PredictorActivationDiffStatFigureSettings)Cfg;
            if (cfg.Window == PredictorActivationDiffStatFigureSettings.NAWindowNum)
            {
                return continuousActivationDiffStat.Get(cfg.Figure);
            }
            else
            {
                if (activationMDW.UsedCapacity >= cfg.Window)
                {
                    return activationMDW.GetDataDiffStat(cfg.Window).Get(cfg.Figure);
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorActivationDiffStatFigure
}//Namespace
