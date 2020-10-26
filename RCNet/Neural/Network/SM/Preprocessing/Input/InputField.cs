using RCNet.MathTools;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Data.Coders.AnalogToSpiking;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Provides input data to be processed in the reservoirs. Supports analog and spike codings.
    /// Used spike coding method is from the "population temporal coding" family.
    /// </summary>
    [Serializable]
    public class InputField
    {
        //Constants

        //Attribute properties
        /// <summary>
        /// Input field name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Input field index among all input fields
        /// </summary>
        public int Idx { get; }

        /// <summary>
        /// Specifies whether to route values as the additional predictors to readout
        /// </summary>
        public bool RouteToReadout { get; }

        /// <summary>
        /// Input neuron providing analog value
        /// </summary>
        public AnalogInputNeuron AnalogNeuron { get; }

        /// <summary>
        /// Collection of input neurons representing an analog value as a spike code 
        /// </summary>
        public SpikingInputNeuron[] SpikingNeuronCollection { get; }

        //Attributes
        private readonly FeatureFilterBase _featureFilter;
        private readonly A2SCoder _spikingCoder;
        private double _iAnalogStimuli;
        private byte[] _iSpikingStimuli;
        private int _currentDataIdx;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Name of the input field</param>
        /// <param name="idx">Index of the input field</param>
        /// <param name="coordinates">Input coordinates (entry point)</param>
        /// <param name="dataWorkingRange">Input data range</param>
        /// <param name="featureFilterCfg">Feature filter configuration</param>
        /// <param name="spikingCoderCfg">Configuration of the analog value to spikes coder</param>
        /// <param name="routeToReadout">Specifies whether to route values as the additional predictors to readout</param>
        /// <param name="inputNeuronsStartIdx">Index of the first input neuron of this field among all input neurons</param>
        public InputField(string name,
                          int idx,
                          int[] coordinates,
                          Interval dataWorkingRange,
                          IFeatureFilterSettings featureFilterCfg,
                          A2SCoderSettings spikingCoderCfg,
                          bool routeToReadout,
                          int inputNeuronsStartIdx
                          )
        {
            Name = name;
            Idx = idx;
            RouteToReadout = routeToReadout;
            _featureFilter = FeatureFilterFactory.Create(dataWorkingRange, featureFilterCfg);
            _iAnalogStimuli = 0;
            _currentDataIdx = 0;
            //Analog neuron
            AnalogNeuron = new AnalogInputNeuron(new NeuronLocation(InputEncoder.ReservoirID, inputNeuronsStartIdx, InputEncoder.PoolID, inputNeuronsStartIdx, idx, coordinates[0], coordinates[1], coordinates[2]));
            ++inputNeuronsStartIdx;
            //Spiking neurons
            _spikingCoder = new A2SCoder(spikingCoderCfg);
            int spikingPopulationSize = -1;
            if (_spikingCoder.Method == A2SCoder.CodingMethod.Horizontal)
            {
                //Horizontal coding
                switch (_featureFilter.Type)
                {
                    case FeatureFilterBase.FeatureType.Real:
                        spikingPopulationSize = _spikingCoder.SpikeCode.Length;
                        break;
                    case FeatureFilterBase.FeatureType.Binary:
                        spikingPopulationSize = 1;
                        break;
                    case FeatureFilterBase.FeatureType.Enum:
                        spikingPopulationSize = ((EnumFeatureFilterSettings)featureFilterCfg).NumOfElements;
                        break;
                }
                _iSpikingStimuli = new byte[spikingPopulationSize];
            }
            else if (_spikingCoder.Method == A2SCoder.CodingMethod.Vertical)
            {
                //Vertical coding
                spikingPopulationSize = 1;
                _iSpikingStimuli = new byte[_spikingCoder.SpikeCode.Length];
            }
            else
            {
                //None coding
                spikingPopulationSize = 0;
                _iSpikingStimuli = new byte[_spikingCoder.SpikeCode.Length];
            }
            SpikingNeuronCollection = new SpikingInputNeuron[spikingPopulationSize];
            for (int i = 0; i < SpikingNeuronCollection.Length; i++)
            {
                SpikingNeuronCollection[i] = new SpikingInputNeuron(new NeuronLocation(InputEncoder.ReservoirID, inputNeuronsStartIdx, InputEncoder.PoolID, inputNeuronsStartIdx, idx, coordinates[0], coordinates[1], coordinates[2]));
                ++inputNeuronsStartIdx;
            }
            return;
        }

        //Properties
        /// <summary>
        /// Total number of input neurons
        /// </summary>
        public int NumOfInputNeurons { get { return (1 + SpikingNeuronCollection.Length); } }

        //Methods
        /// <summary>
        /// Resets feature associated filter
        /// </summary>
        public void ResetFilter()
        {
            _featureFilter.Reset();
            return;
        }
        /// <summary>
        /// Updates feature filter
        /// </summary>
        /// <param name="value">Sample value</param>
        public void UpdateFilter(double value)
        {
            _featureFilter.Update(value);
            return;
        }

        /// <summary>
        /// Resets all associated input neurons to initial state
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics of the associated neurons</param>
        public void ResetNeurons(bool statistics)
        {
            _spikingCoder?.Reset();
            AnalogNeuron.Reset(statistics);
            for (int i = 0; i < SpikingNeuronCollection.Length; i++)
            {
                SpikingNeuronCollection[i].Reset(statistics);
            }
            return;
        }

        /// <summary>
        /// Prepares input neurons to provide new incoming data.
        /// </summary>
        /// <param name="value">External natural input data</param>
        public void SetNewData(double value)
        {
            _iAnalogStimuli = _featureFilter.ApplyFilter(value);
            _currentDataIdx = 0;
            switch (_spikingCoder.Method)
            {
                case A2SCoder.CodingMethod.Horizontal:
                    {
                        switch (_featureFilter.Type)
                        {
                            case FeatureFilterBase.FeatureType.Real:
                                _spikingCoder.Encode(_iAnalogStimuli);
                                _spikingCoder.SpikeCode.CopyTo(_iSpikingStimuli, 0);
                                break;
                            case FeatureFilterBase.FeatureType.Binary:
                                _iSpikingStimuli[0] = (byte)value;
                                break;
                            case FeatureFilterBase.FeatureType.Enum:
                                {
                                    int spikeIdx = ((int)Math.Round(value, 0)) - 1;
                                    for (int i = 0; i < _iSpikingStimuli.Length; i++)
                                    {
                                        _iSpikingStimuli[i] = i == spikeIdx ? (byte)1 : (byte)0;
                                    }
                                }
                                break;
                        }
                    }
                    break;

                case A2SCoder.CodingMethod.Vertical:
                    _spikingCoder.Encode(_iAnalogStimuli);
                    _spikingCoder.SpikeCode.CopyTo(_iSpikingStimuli, 0);
                    break;
                
                default:
                    break;
            }
            return;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public bool Fetch(bool collectStatistics)
        {
            if (_currentDataIdx == 0 || _currentDataIdx < _iSpikingStimuli.Length)
            {
                switch (_spikingCoder.Method)
                {
                    case A2SCoder.CodingMethod.Horizontal:
                        {
                            //Analog neuron
                            AnalogNeuron.NewStimulation(_iAnalogStimuli, 0d);
                            AnalogNeuron.Recompute(collectStatistics);
                            //Spiking neurons
                            for (int i = 0; i < SpikingNeuronCollection.Length; i++)
                            {
                                SpikingNeuronCollection[i].NewStimulation(_iSpikingStimuli[i], 0d);
                                SpikingNeuronCollection[i].Recompute(collectStatistics);
                            }
                        }
                        break;

                    case A2SCoder.CodingMethod.Vertical:
                        {
                            //Analog neuron
                            AnalogNeuron.NewStimulation(_iSpikingStimuli[_currentDataIdx] == 0 ? -1d : 0d, 0d);
                            AnalogNeuron.Recompute(collectStatistics);
                            //Spiking neuron
                            SpikingNeuronCollection[0].NewStimulation(_iSpikingStimuli[_currentDataIdx], 0d);
                            SpikingNeuronCollection[0].Recompute(collectStatistics);
                        }
                        break;

                    default:
                        //Only analog neuron
                        AnalogNeuron.NewStimulation(_iAnalogStimuli, 0d);
                        AnalogNeuron.Recompute(collectStatistics);
                        break;
                }
                ++_currentDataIdx;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Prepares set of meaningful combinations of indexes of input spiking neurons to be connected to a target hidden neuron.
        /// </summary>
        /// <param name="numOfCombinations">Desired number of connections</param>
        public List<int[]> GetSpikingInputCombinations(int numOfCombinations)
        {
            List<int[]> result = new List<int[]>(numOfCombinations);
            //Alone coding spikes
            for (int i = 0; i < SpikingNeuronCollection.Length; i++)
            {
                int[] cmbIdxs = new int[1];
                cmbIdxs[0] = i;
                result.Add(cmbIdxs);
                if (result.Count == numOfCombinations)
                {
                    //Desired number of combinations is reached
                    return result;
                }
            }
            return result;
        }

    }//InputField

}//Namespace
