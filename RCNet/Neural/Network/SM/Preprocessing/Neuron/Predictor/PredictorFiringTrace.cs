using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Traced neuron's firing.
    /// </summary>
    [Serializable]
    public class PredictorFiringTrace : IPredictor
    {
        //Attribute properties
        /// <summary>
        /// Configuration of the predictor
        /// </summary>
        public IPredictorSettings Cfg { get; }

        //Attributes
        private double _continuousTrace;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="cfg">Configuration of the predictor</param>
        public PredictorFiringTrace(PredictorFiringTraceSettings cfg)
        {
            Cfg = cfg;
            if (cfg.Window == PredictorFiringTraceSettings.NAWindowNum)
            {
                Reset();
            }
            return;
        }

        /// <summary>
        /// Resets the predictor computer
        /// </summary>
        public void Reset()
        {
            _continuousTrace = 0d;
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
            PredictorFiringTraceSettings cfg = (PredictorFiringTraceSettings)Cfg;
            if (cfg.Window == PredictorFiringTraceSettings.NAWindowNum)
            {
                _continuousTrace *= (1d - cfg.Fading);
                _continuousTrace += spike ? 1d : 0d;
            }
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
            PredictorFiringTraceSettings cfg = (PredictorFiringTraceSettings)Cfg;
            if(cfg.Window == PredictorFiringTraceSettings.NAWindowNum)
            {
                return _continuousTrace;
            }
            else
            {
                if (firingMDW.Count >= cfg.Window)
                {
                    double trace = 0d;
                    for(int i = cfg.Window - 1; i >= 0; i--)
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
