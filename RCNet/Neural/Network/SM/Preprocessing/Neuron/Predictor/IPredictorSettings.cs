namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Common interface of the predictor computer configurations.
    /// </summary>
    public interface IPredictorSettings
    {

        /// <inheritdoc cref="PredictorsProvider.PredictorID"/>
        PredictorsProvider.PredictorID ID { get; }

        /// <summary>
        /// Specifies the necessary size of the windowed history of the activations.
        /// </summary>
        int RequiredWndSizeOfActivations { get; }

        /// <summary>
        /// Specifies the necessary size of the windowed history of the firings.
        /// </summary>
        int RequiredWndSizeOfFirings { get; }

        /// <summary>
        /// Indicates the use of the continuous statistics of the activation.
        /// </summary>
        bool NeedsContinuousActivationStat { get; }

        /// <summary>
        /// Indicates the use of the continuous statistics of the activation difference.
        /// </summary>
        bool NeedsContinuousActivationDiffStat { get; }

    }//IPredictorSettings

}//Namespace
