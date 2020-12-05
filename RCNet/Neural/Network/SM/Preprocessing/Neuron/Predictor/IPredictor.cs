using System;
using System.Collections.Generic;
using System.Text;
using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Common interface of predictor computers
    /// </summary>
    public interface IPredictor
    {
        /// <summary>
        /// Configuration of the predictor
        /// </summary>
        IPredictorSettings Cfg { get; }

        /// <summary>
        /// Resets the predictor computer to its initial state
        /// </summary>
        void Reset();

        /// <summary>
        /// Updates the predictor computer
        /// </summary>
        /// <param name="activation">Current value of the activation</param>
        /// <param name="normalizedActivation">Current value of the activation normalized between 0 and 1</param>
        /// <param name="spike">Indicates whether the neuron is firing</param>
        void Update(double activation, double normalizedActivation, bool spike);

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
        double Compute(BasicStat continuousActivationStat,
                       BasicStat continuousActivationDiffStat,
                       MovingDataWindow activationMDW,
                       SimpleQueue<byte> firingMDW,
                       double activation,
                       double normalizedActivation,
                       bool spike
                       );

    }//IPredictor

}//Namespace
