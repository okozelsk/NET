namespace RCNet.Neural.Activation
{
    /// <summary>
    /// The type of an activation function.
    /// </summary>
    public enum ActivationType
    {
        /// <summary>
        /// A spiking activation function.
        /// </summary>
        /// <remarks>
        /// Attempts to simulate the behavior of a biological neuron that accumulates (integrates) input stimulation on its membrane potential and when the critical threshold is exceeded, fires a short pulse (spike), resets membrane and the cycle starts from the beginning. In other words, the function implements one of the so-called Integrate and Fire neuron models.
        /// </remarks>
        Spiking,
        /// <summary>
        /// An analog activation function.
        /// </summary>
        /// <remarks>
        /// It has no similarity to behavior of the biological neuron. It is always stateless, which means that the output value (signal) does not depend on the previous inputs but only on current input at the time T and particular transformation equation (usually non-linear).
        /// </remarks>
        Analog
    };

}//Namespace
