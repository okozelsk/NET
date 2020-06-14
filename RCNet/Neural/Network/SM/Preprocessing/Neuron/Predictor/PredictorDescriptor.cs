using RCNet.Neural.Network.SM.Preprocessing.Input;
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
        /// Value indicating non standard predictor - exact value of an input field
        /// </summary>
        public const int InputFieldValue = -1;

        //Attribute properties
        /// <summary>
        /// Name of the associated input field. Empty string means no relationship with concrete input field (hidden neuron origin).
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Associated reservoir index or InputEncoder.ReservoirID when predictor is an input value.
        /// </summary>
        public int ReservoirID { get; }

        /// <summary>
        /// Pool index within the reservoir or InputEncoder.PoolID when predictor predictor is an input value.
        /// </summary>
        public int PoolID { get; }

        /// <summary>
        /// PredictorsProvider.PredictorID or -1 when it is an input value to be used as a predictor.
        /// </summary>
        public int PredictorID { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirID">Associated reservoir index.</param>
        /// <param name="poolID">Pool index within the reservoir.</param>
        /// <param name="predictorID">ID of the predictor (PredictorsProvider.PredictorID).</param>
        public PredictorDescriptor(int reservoirID,
                                   int poolID,
                                   PredictorsProvider.PredictorID predictorID
                                   )
        {
            ReservoirID = reservoirID;
            PoolID = poolID;
            PredictorID = (int)predictorID;
            InputFieldName = string.Empty;
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputFieldName">Name of the associated input field.</param>
        public PredictorDescriptor(string inputFieldName)
        {
            ReservoirID = InputEncoder.ReservoirID;
            PoolID = InputEncoder.PoolID;
            PredictorID = InputFieldValue;
            InputFieldName = inputFieldName;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates exact value of input field (not a hidden neuron's predictor)
        /// </summary>
        public bool IsInputValue { get { return PredictorID == InputFieldValue; } }

    }//PredictorDescriptor

}//Namespace
