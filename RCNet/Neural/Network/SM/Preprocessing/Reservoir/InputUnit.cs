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
    /// Used spike-train coding method is from the "population-temporal coding" family.
    /// </summary>
    [Serializable]
    public class InputUnit
    {
        //Constants
        /// <summary>
        /// Maximum length of the spike-train (number of bits) representing input analog data
        /// </summary>
        public const int SpikeTrainMaxLength = Bitwise.MaxBits * 2;

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
        private readonly int _halfTrainLength;
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
            _halfTrainLength = _inputUnitCfg.SpikeTrainLength / 2;
            _precisionPiece = (_inputRange.Span / 2d) / Math.Pow(2d, _halfTrainLength);
            _maxPrecisionBitMask = 0ul;
            for(int i = 0; i < _halfTrainLength; i++)
            {
                _maxPrecisionBitMask = Bitwise.SetBit(_maxPrecisionBitMask, i, true);
            }
            SpikeTrainInputNeuronCollection = new SpikingInputNeuron[_halfTrainLength * 2];
            for (int i = 0; i < SpikeTrainInputNeuronCollection.Length; i++)
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
        public int NumOfInputNeurons { get { return (1 + SpikeTrainInputNeuronCollection.Length); } }

        //Methods
        /// <summary>
        /// Transforms analog input to a binary sequence (spike-train)
        /// </summary>
        /// <param name="analogInput">Analog input value</param>
        /// <param name="lowHalf">Indicates what half of the train to be occupied</param>
        private ulong GetSpikeTrain(double analogInput, out bool lowHalf)
        {
            double halfValue;
            if(analogInput < _inputRange.Mid)
            {
                halfValue = Math.Abs(analogInput);
                lowHalf = true;
            }
            else
            {
                halfValue = Math.Abs(analogInput - _inputRange.Mid);
                lowHalf = false;
            }
            ulong pieces = (ulong)Math.Min(Math.Floor(halfValue / _precisionPiece), (double)_maxPrecisionBitMask);
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
        /// Prepares set of combinations of indexes of input spiking neurons to be used to connect hidden neurons.
        /// One combination per hidden neuron.
        /// </summary>
        /// <param name="numOfCombinations">Desired number of hidden neurons to be connected</param>
        public List<int[]> GetSpikingInputCombinations(int numOfCombinations)
        {
            //Indexes of spiking input neurons coding analog values LT average. In order from highest to lowest spike.
            int[] lowNeuronsIdxs = new int[_halfTrainLength];
            lowNeuronsIdxs.Indices();
            //Indexes of spiking input neurons coding analog values GE to average. In order from highest to lowest spike.
            int[] highNeuronsIdxs = new int[_halfTrainLength];
            for (int i = 0, idx = SpikeTrainInputNeuronCollection.Length - 1; idx >= _halfTrainLength; idx--, i++)
            {
                highNeuronsIdxs[i] = idx;
            }
            List<int[]> result = new List<int[]>(numOfCombinations);
            for (int cmbLength = 1; cmbLength <= _halfTrainLength; cmbLength++)
            {
                foreach (int[] neuronIdxs in new List<int[]> { lowNeuronsIdxs, highNeuronsIdxs })
                {
                    int[] cmbIdxs = new int[cmbLength];
                    for (int i = 0; i < cmbLength; i++)
                    {
                        cmbIdxs[i] = neuronIdxs[i];
                    }
                    result.Add(cmbIdxs);
                    if (result.Count == numOfCombinations)
                    {
                        //Desired number of combinations is reached
                        return result;
                    }
                }
            }
            //Maximum number of combinations is reached
            return result;
        }

        /// <summary>
        /// Prepares set of combinations of indexes of input spiking neurons to be used to connect hidden neurons.
        /// One combination per hidden neuron.
        /// </summary>
        /// <param name="numOfCombinations">Desired number of hidden neurons to be connected</param>
        public List<int[]> GetSpikingInputCombinations_ultra_short(int numOfCombinations)
        {
            //Indexes of spiking input neurons coding analog values LT average. In order from highest to lowest spike.
            int[] lowNeuronsIdxs = new int[_halfTrainLength];
            lowNeuronsIdxs.Indices();
            //Indexes of spiking input neurons coding analog values GE to average. In order from highest to lowest spike.
            int[] highNeuronsIdxs = new int[_halfTrainLength];
            for (int i = 0, idx = SpikeTrainInputNeuronCollection.Length - 1; idx >= _halfTrainLength; idx--, i++)
            {
                highNeuronsIdxs[i] = idx;
            }
            return new List<int[]> { lowNeuronsIdxs, highNeuronsIdxs };
        }

        /// <summary>
        /// Prepares set of combinations of indexes of input spiking neurons to be used to connect hidden neurons.
        /// One combination per hidden neuron.
        /// </summary>
        /// <param name="numOfCombinations">Desired number of hidden neurons to be connected</param>
        public List<int[]> GetSpikingInputCombinations_many(int numOfCombinations)
        {
            //Indexes of spiking input neurons coding analog values LT average. In order from highest to lowest spike.
            int[] lowNeuronsIdxs = new int[_halfTrainLength];
            lowNeuronsIdxs.Indices();
            //Indexes of spiking input neurons coding analog values GE to average. In order from highest to lowest spike.
            int[] highNeuronsIdxs = new int[_halfTrainLength];
            for (int i = 0, idx = SpikeTrainInputNeuronCollection.Length - 1; idx >= _halfTrainLength; idx--, i++)
            {
                highNeuronsIdxs[i] = idx;
            }
            List<int[]> result = new List<int[]>(numOfCombinations);
            for (int cmbLength = 1; cmbLength <= _halfTrainLength; cmbLength++)
            {
                for (int startIdx = 0; startIdx <= _halfTrainLength - cmbLength; startIdx++)
                {
                    foreach (int[] neuronIdxs in new List<int[]> { lowNeuronsIdxs, highNeuronsIdxs })
                    {
                        int[] cmbIdxs = new int[cmbLength];
                        for (int i = 0; i < cmbLength; i++)
                        {
                            cmbIdxs[i] = neuronIdxs[startIdx + i];
                        }
                        result.Add(cmbIdxs);
                        if (result.Count == numOfCombinations)
                        {
                            //Desired number of combinations is reached
                            return result;
                        }
                    }
                }
            }
            //Maximum number of combinations is reached
            return result;
        }

        /// <summary>
        /// Forces input neurons to accept new incoming stimulation.
        /// </summary>
        /// <param name="iStimuli">External input analog signal</param>
        public void NewStimulation(double iStimuli)
        {
            AnalogInputNeuron.NewStimulation(iStimuli, 0d);
            ulong spikeTrainBits = GetSpikeTrain(iStimuli, out bool lowHalf);
            for (int i = 0; i < _halfTrainLength; i++)
            {
                double spikeVal = Bitwise.GetBit(spikeTrainBits, i);
                if(lowHalf)
                {
                    SpikeTrainInputNeuronCollection[(_halfTrainLength - 1) - i].NewStimulation(spikeVal, 0d);
                }
                else
                {
                    SpikeTrainInputNeuronCollection[_halfTrainLength + i].NewStimulation(spikeVal, 0d);
                }
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
