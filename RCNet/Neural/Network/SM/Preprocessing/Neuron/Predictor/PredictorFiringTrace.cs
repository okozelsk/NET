﻿using RCNet.MathTools;
using RCNet.Queue;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the "FiringTrace" predictor computer.
    /// </summary>
    /// <remarks>
    /// The predictor value expresses the trace of the neuron's firings over the time.
    /// </remarks>
    [Serializable]
    public class PredictorFiringTrace : IPredictor
    {
        //Attribute properties
        /// <inheritdoc/>
        public IPredictorSettings Cfg { get; }

        //Attributes
        private double _continuousTrace;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the predictor.</param>
        public PredictorFiringTrace(PredictorFiringTraceSettings cfg)
        {
            Cfg = cfg;
            if (cfg.Window == PredictorFiringTraceSettings.NAWindowNum)
            {
                Reset();
            }
            return;
        }

        /// <inheritdoc/>
        public void Reset()
        {
            _continuousTrace = 0d;
            return;
        }

        /// <inheritdoc/>
        public void Update(double activation, double normalizedActivation, bool spike)
        {
            PredictorFiringTraceSettings cfg = (PredictorFiringTraceSettings)Cfg;
            if (cfg.Window == PredictorFiringTraceSettings.NAWindowNum)
            {
                _continuousTrace *= (1d - cfg.Fading);
                _continuousTrace += spike ? 1d : 0d;
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
            PredictorFiringTraceSettings cfg = (PredictorFiringTraceSettings)Cfg;
            if (cfg.Window == PredictorFiringTraceSettings.NAWindowNum)
            {
                return _continuousTrace;
            }
            else
            {
                if (firingMDW.Count >= cfg.Window)
                {
                    double trace = 0d;
                    for (int i = cfg.Window - 1; i >= 0; i--)
                    {
                        trace *= (1d - cfg.Fading);
                        trace += firingMDW.GetElementAt(i, true);
                    }
                    return trace;
                }
                else
                {
                    return 0d;
                }
            }
        }

    }//PredictorFiringTrace
}//Namespace
