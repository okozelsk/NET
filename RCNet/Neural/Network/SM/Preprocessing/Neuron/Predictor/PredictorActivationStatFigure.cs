using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "ActivationStatFeature" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value is a statistical figure computed from the activation function results.
    /// </remarks>
    [Serializable]
    public class PredictorActivationStatFigure : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public PredictorActivationStatFigure(PredictorActivationStatFigureSettings cfg)
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
            PredictorActivationStatFigureSettings cfg = (PredictorActivationStatFigureSettings)Cfg;
            if (cfg.Window == PredictorActivationStatFigureSettings.NAWindowNum)
            {
                return continuousActivationStat.Get(cfg.Figure);
            }
            else
            {
                if (activationMDW.UsedCapacity >= cfg.Window)
                {
                    return activationMDW.GetDataStat(cfg.Window).Get(cfg.Figure);
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorActivationStatFigure
}//Namespace
