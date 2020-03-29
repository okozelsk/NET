using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron.Predictor;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Provides input to be processed in the reservoir. Supports analog and spike-train codings.
    /// Used spike-train coding method is from the "Sparse coding" family.
    /// </summary>
    [Serializable]
    public class InputUnit
    {
        //Constants
        /// <summary>
        /// Maximum length of the spike-train (number of bits) representing input analog data
        /// </summary>
        public const int SpikeTrainMaxLength = 32;

        //Attribute properties
        /// <summary>
        /// Index of the associated input field
        /// </summary>
        public int InputFieldIdx { get; }

        /// <summary>
        /// Input neuron providing analog value
        /// </summary>
        public AnalogInputNeuron AnalogInputNeuron { get; }

        /// <summary>
        /// Collection of input neurons representing spike train of analog value
        /// </summary>
        public SpikingInputNeuron[] SpikeTrainInputNeuronCollection { get; }

        //Attributes
        private readonly Interval _inputRange;
        private readonly InputUnitSettings _inputUnitCfg;
        private readonly double _precisionPiece;
        private readonly ulong _maxPrecisionBitMask;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="reservoirID">Reservoir ID</param>
        /// <param name="inputRange">Input data range</param>
        /// <param name="inputFieldIdx">Index of the associated input field</param>
        /// <param name="inputNeuronsStartIdx">Index of the first input neuron of this unit among all input neurons</param>
        /// <param name="inputUnitCfg">Configuration parameters.</param>
        /// <param name="reservoirPredictorsCfg">Reservoir level configuration of predictors</param>
        public InputUnit(int reservoirID,
                         Interval inputRange,
                         int inputFieldIdx,
                         int inputNeuronsStartIdx,
                         InputUnitSettings inputUnitCfg,
                         PredictorsSettings reservoirPredictorsCfg
                         )
        {
            _inputRange = inputRange.DeepClone();
            _inputUnitCfg = (InputUnitSettings)inputUnitCfg.DeepClone();
            _precisionPiece = _inputRange.Span / Math.Pow(2d, _inputUnitCfg.SpikeTrainLength);
            _maxPrecisionBitMask = (uint)Math.Round(Math.Pow(2d, _inputUnitCfg.SpikeTrainLength) - 1d);
            InputFieldIdx = inputFieldIdx;
            PredictorsSettings combinedPredictorsCfg = new PredictorsSettings(inputUnitCfg.PredictorsCfg, null, reservoirPredictorsCfg);
            PredictorsSettings analogPredictorsCfg = (inputUnitCfg.AnalogNeuronPredictors && combinedPredictorsCfg.NumOfEnabledPredictors > 0) ? combinedPredictorsCfg : null;
            PredictorsSettings spikingPredictorsCfg = (PredictorsSettings)combinedPredictorsCfg.DeepClone();
            spikingPredictorsCfg.DisableActivationPredictors();
            if(!inputUnitCfg.SpikingNeuronPredictors || spikingPredictorsCfg.NumOfEnabledPredictors == 0)
            {
                spikingPredictorsCfg = null;
            }
            AnalogInputNeuron = new AnalogInputNeuron(reservoirID,
                                                      _inputUnitCfg.CoordinatesCfg.GetCoordinates(),
                                                      inputNeuronsStartIdx++,
                                                      _inputRange,
                                                      analogPredictorsCfg,
                                                      _inputUnitCfg.AnalogFiringThreshold
                                                      );
            SpikeTrainInputNeuronCollection = new SpikingInputNeuron[_inputUnitCfg.SpikeTrainLength];
            for (int i = 0; i < _inputUnitCfg.SpikeTrainLength; i++)
            {
                SpikeTrainInputNeuronCollection[i] = new SpikingInputNeuron(reservoirID,
                                                                            _inputUnitCfg.CoordinatesCfg.GetCoordinates(),
                                                                            inputNeuronsStartIdx++,
                                                                            spikingPredictorsCfg
                                                                            );
            }
            return;
        }

        //Properties
        /// <summary>
        /// Total number of input neurons
        /// </summary>
        public int NumOfInputNeurons { get { return (1 + _inputUnitCfg.SpikeTrainLength); } }

        //Methods
        /// <summary>
        /// Transforms analog input to a binary sequence (spike-train)
        /// </summary>
        /// <param name="analogInput">Analog input value</param>
        private uint GetSpikeTrain(double analogInput)
        {
            double movedDataValue = Math.Max(0d, analogInput - _inputRange.Min);
            uint pieces = (uint)Math.Min(Math.Floor(movedDataValue / _precisionPiece), (double)_maxPrecisionBitMask);
            return pieces;
        }

        /// <summary>
        /// Resets all associated input neurons to initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics of the associated neurons</param>
        public void Reset(bool statistics)
        {
            AnalogInputNeuron.Reset(statistics);
            for (int i = 0; i < SpikeTrainInputNeuronCollection.Length; i++)
            {
                SpikeTrainInputNeuronCollection[i].Reset(statistics);
            }
            return;
        }

        /// <summary>
        /// Forces input neurons to accept new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        public void NewStimulation(double iStimuli)
        {
            AnalogInputNeuron.NewStimulation(iStimuli, 0d);
            uint spikeTrainBits = GetSpikeTrain(iStimuli);
            for (int i = 0; i < SpikeTrainInputNeuronCollection.Length; i++)
            {
                double spikeVal = Bitwise.GetBit(spikeTrainBits, i);
                SpikeTrainInputNeuronCollection[i].NewStimulation(spikeVal, 0d);
            }
            return;
        }

        /// <summary>
        /// Forces input neurons to compute new output signal.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public void ComputeSignal(bool collectStatistics)
        {
            AnalogInputNeuron.Recompute(collectStatistics);
            foreach (SpikingInputNeuron neuron in SpikeTrainInputNeuronCollection)
            {
                neuron.Recompute(collectStatistics);
            }
            return;
        }

        /// <summary>
        /// Returns collection of input neurons having enabled at least one predictor
        /// </summary>
        /// <param name="numOfPredictors">Returned number of predictors</param>
        public List<INeuron> GetPredictingNeurons(out int numOfPredictors)
        {
            numOfPredictors = 0;
            List<INeuron> predictingNeurons = new List<INeuron>();
            if(AnalogInputNeuron.NumOfEnabledPredictors > 0)
            {
                numOfPredictors += AnalogInputNeuron.NumOfEnabledPredictors;
                predictingNeurons.Add(AnalogInputNeuron);
            }
            foreach (SpikingInputNeuron neuron in SpikeTrainInputNeuronCollection)
            {
                if (neuron.NumOfEnabledPredictors > 0)
                {
                    numOfPredictors += neuron.NumOfEnabledPredictors;
                    predictingNeurons.Add(neuron);
                }
            }
            return predictingNeurons;
        }

    }//InputUnit

}//Namespace
