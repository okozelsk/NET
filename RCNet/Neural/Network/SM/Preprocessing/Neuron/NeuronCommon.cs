namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Enums and parsings of enum values for a Neuron type
    /// </summary>
    public static class NeuronCommon
    {
        //Enums
        /// <summary>
        /// Type of neuron
        /// </summary>
        public enum NeuronType
        {
            /// <summary>
            /// Input neuron
            /// </summary>
            Input,
            /// <summary>
            /// Hidden neuron
            /// </summary>
            Hidden
        }//NeuronType

        /// <summary>
        /// Output signaling restriction of the neuron
        /// </summary>
        public enum NeuronSignalingRestrictionType
        {
            /// <summary>
            /// Neuron can emitt both analog signal and spikes.
            /// </summary>
            NoRestriction,
            /// <summary>
            /// Neuron can emitt only analog signal.
            /// </summary>
            AnalogOnly,
            /// <summary>
            /// Neuron can emitt only spiking signal.
            /// </summary>
            SpikingOnly
        }


    }//NeuronCommon

} //Namespace
