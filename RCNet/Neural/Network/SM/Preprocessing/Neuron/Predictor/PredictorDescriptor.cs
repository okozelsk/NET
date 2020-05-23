using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Describes predictor type and origin
    /// </summary>
    [Serializable]
    public class PredictorDescriptor
    {
        //Constants
        /// <summary>
        /// Value indicating no association with concrete input field
        /// </summary>
        public const int NoInputFieldRelated = -1;

        /// <summary>
        /// Value indicating non standard predictor - exact value of input field
        /// </summary>
        public const int InputFieldValue = -1;

        //Attribute properties
        /// <summary>
        /// Index of the associated input field. Value -1 means no relationship with concrete input field (hidden neuron origin).
        /// </summary>
        public int InputFieldID { get; }

        /// <summary>
        /// Associated reservoir index or InputEncoder.ReservoirID when predictor is associated with an input neuron of the InputEncoder.
        /// </summary>
        public int ReservoirID { get; }

        /// <summary>
        /// Pool index within the reservoir or InputEncoder.PoolID when predictor is associated with an input neuron of the InputEncoder.
        /// </summary>
        public int PoolID { get; }

        /// <summary>
        /// PredictorsProvider.PredictorID or -1 when it is an input value to be used as a predictor.
        /// </summary>
        public int PredictorID { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputFieldID">Index of the associated input field. Value -1 means no relationship with concrete input field (hidden neuron origin).</param>
        /// <param name="reservoirID">Associated reservoir index or InputEncoder.ReservoirID when predictor is associated with an input neuron of the InputEncoder.</param>
        /// <param name="poolID">Pool index within the reservoir or InputEncoder.PoolID when predictor is associated with an input neuron of the InputEncoder.</param>
        /// <param name="predictorID">PredictorsProvider.PredictorID or -1 when it is an input value to be used as a predictor.</param>
        public PredictorDescriptor(int inputFieldID,
                                   int reservoirID,
                                   int poolID,
                                   int predictorID
                                   )
        {
            InputFieldID = inputFieldID;
            ReservoirID = reservoirID;
            PoolID = poolID;
            PredictorID = predictorID;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates predictor related to concrete input field
        /// </summary>
        public bool IsInputFieldRelated { get { return InputFieldID != NoInputFieldRelated; } }

        /// <summary>
        /// Indicates a non standard predictor - exact value of input field
        /// </summary>
        public bool IsInputFieldValue { get { return PredictorID == InputFieldValue; } }

    }//PredictorDescriptor

}//Namespace
