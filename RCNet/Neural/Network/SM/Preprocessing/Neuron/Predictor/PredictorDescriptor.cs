using RCNet.Neural.Network.SM.Preprocessing.Input;
using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Implements the descriptor of the predictor.
    /// </summary>
    [Serializable]
    public class PredictorDescriptor
    {
        //Constants
        /// <summary>
        /// Indicates the routed input field value used as the predictor.
        /// </summary>
        public const int InputFieldValue = -1;

        //Attribute properties
        /// <summary>
        /// The name of the associated input field. An empty string means no relationship with the concrete input field.
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// An origin reservoir index.
        /// </summary>
        public int ReservoirID { get; }

        /// <summary>
        /// An origin pool index.
        /// </summary>
        public int PoolID { get; }

        /// <summary>
        /// An identifier of the computed predictor or -1 when it is a routed input field value.
        /// </summary>
        public int PredictorID { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirID">An origin reservoir index.</param>
        /// <param name="poolID">An origin pool index.</param>
        /// <param name="predictorID">An identifier of the computed predictor.</param>
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputFieldName">The name of the associated input field.</param>
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
        /// Indicates the routed value of an input field.
        /// </summary>
        public bool IsInputValue { get { return PredictorID == InputFieldValue; } }

    }//PredictorDescriptor

}//Namespace
