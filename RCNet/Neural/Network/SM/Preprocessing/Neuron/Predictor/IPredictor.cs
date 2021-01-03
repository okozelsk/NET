using RCNet.MathTools;
using RCNet.Queue;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// The common interface of the predictor computers.
    /// </summary>
    public interface IPredictor
    {
        /// <summary>
        /// The configuration of the predictor.
        /// </summary>
        IPredictorSettings Cfg { get; }

        /// <summary>
        /// Resets the predictor computer to its initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Updates the predictor computer.
        /// </summary>
        /// <param name="activation">The current value of the activation.</param>
        /// <param name="normalizedActivation">The current value of the activation normalized between 0 and 1.</param>
        /// <param name="spike">Indicates whether the neuron is currently firing.</param>
        void Update(double activation, double normalizedActivation, bool spike);

        /// <summary>
        /// Computes the predictor value.
        /// </summary>
        /// <param name="continuousActivationStat">The continuous statistics of the activations.</param>
        /// <param name="continuousActivationDiffStat">The continuous statistics of the activation differences.</param>
        /// <param name="activationMDW">The moving data window of the activations.</param>
        /// <param name="firingMDW">The moving data window of the firings.</param>
        /// <param name="activation">The current value of the activation.</param>
        /// <param name="normalizedActivation">The current value of the activation normalized between 0 and 1.</param>
        /// <param name="spike">Indicates whether the neuron is currently firing.</param>
        /// <returns>The computed predictor value.</returns>
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
