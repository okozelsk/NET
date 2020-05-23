namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Common interface of PredictorParamsSettings classes
    /// </summary>
    public interface IPredictorParamsSettings
    {

        /// <summary>
        /// Identifier of the predictor
        /// </summary>
        PredictorsProvider.PredictorID ID { get; }

    }//IPredictorParamsSettings

}//Namespace
