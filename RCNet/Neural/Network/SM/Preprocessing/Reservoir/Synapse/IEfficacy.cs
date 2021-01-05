namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Common interface of the synapse's efficacy computers.
    /// </summary>
    public interface IEfficacy
    {
        /// <summary>
        /// Resets the efficacy computer to its initial state.
        /// </summary>
        void Reset();

        /// <summary>
        /// Computes the synapse's efficacy.
        /// </summary>
        double Compute();

    }//IEfficacy

}//Namespace
