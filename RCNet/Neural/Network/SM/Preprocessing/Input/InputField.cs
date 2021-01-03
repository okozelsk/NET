using RCNet.MathTools;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Implements the input field and associated input neurons coding the input value.
    /// </summary>
    /// <remarks>
    /// An input field always has associated one analog input neuron and depending on the spikes coding mode it can also have associated none, one or more spiking input neurons.
    /// </remarks>
    [Serializable]
    public class InputField
    {
        //Attribute properties
        /// <summary>
        /// The name of the input field.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The zero-based index of the input field among other input fields.
        /// </summary>
        public int Idx { get; }

        /// <summary>
        /// Specifies whether to route the input field values to readout layer.
        /// </summary>
        public bool RouteToReadout { get; }

        /// <summary>
        /// The input neuron providing the analog value.
        /// </summary>
        public AnalogInputNeuron AnalogNeuron { get; }

        /// <summary>
        /// The collection of input neurons providing the spike code representation of the analog value.
        /// </summary>
        public SpikingInputNeuron[] SpikingNeuronCollection { get; }

        //Attributes
        private readonly FeatureFilterBase _featureFilter;
        private readonly InputSpikesCoder _spikesEncoder;
        private double _iAnalogStimuli;
        private int _currentDataIdx;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="name">The name of the input field.</param>
        /// <param name="idx">The zero-based index of the input field among other input fields.</param>
        /// <param name="coordinates">The coordinates of input neurons in 3D space.</param>
        /// <param name="dataWorkingRange">The output range of the input data.</param>
        /// <param name="featureFilterCfg">The configuration of the feature filter.</param>
        /// <param name="spikesEncodingCfg">The configuration of the spikes coder.</param>
        /// <param name="routeToReadout">Specifies whether to route the input field values to readout layer.</param>
        /// <param name="inputNeuronsStartIdx">The zero-based index of the first input neuron of this field among all other input neurons.</param>
        public InputField(string name,
                          int idx,
                          int[] coordinates,
                          Interval dataWorkingRange,
                          IFeatureFilterSettings featureFilterCfg,
                          InputSpikesCoderSettings spikesEncodingCfg,
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
            //Spikes encoder
            _spikesEncoder = new InputSpikesCoder(spikesEncodingCfg);
            //Analog neuron
            int verticalCycles = _spikesEncoder.Regime == InputEncoder.InputSpikesCoding.Vertical ? _spikesEncoder.LargestComponentLength : 1;
            AnalogNeuron = new AnalogInputNeuron(new NeuronLocation(InputEncoder.ReservoirID, inputNeuronsStartIdx, InputEncoder.PoolID, inputNeuronsStartIdx, idx, coordinates[0], coordinates[1], coordinates[2]), verticalCycles);
            ++inputNeuronsStartIdx;
            //Spiking neurons
            int spikingPopulationSize;
            if (_spikesEncoder.Regime == InputEncoder.InputSpikesCoding.Horizontal)
            {
                //Population encoding
                spikingPopulationSize = _spikesEncoder.AllSpikesFlatCollection.Length;
            }
            else if (_spikesEncoder.Regime == InputEncoder.InputSpikesCoding.Vertical)
            {
                //Spike-train encoding
                spikingPopulationSize = _spikesEncoder.ComponentSpikesCollection.Length;
            }
            else
            {
                //Forbidden encoding
                spikingPopulationSize = 0;
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
        /// The total number of input neurons.
        /// </summary>
        public int NumOfInputNeurons { get { return (1 + SpikingNeuronCollection.Length); } }

        //Methods
        /// <summary>
        /// Resets the feature filter.
        /// </summary>
        public void ResetFilter()
        {
            _featureFilter.Reset();
            return;
        }
        /// <summary>
        /// Updates the feature filter.
        /// </summary>
        /// <param name="value">The sample value.</param>
        public void UpdateFilter(double value)
        {
            _featureFilter.Update(value);
            return;
        }

        /// <summary>
        /// Resets all input neurons to their initial state.
        /// </summary>
        /// <param name="statistics">Specifies whether to reset internal statistics of the neurons.</param>
        public void ResetNeurons(bool statistics)
        {
            _spikesEncoder?.Reset();
            AnalogNeuron.Reset(statistics);
            for (int i = 0; i < SpikingNeuronCollection.Length; i++)
            {
                SpikingNeuronCollection[i].Reset(statistics);
            }
            return;
        }

        /// <summary>
        /// Sets the input field's new data.
        /// </summary>
        /// <param name="value">The value.</param>
        public void SetNewData(double value)
        {
            _iAnalogStimuli = _featureFilter.ApplyFilter(value);
            _currentDataIdx = 0;
            _spikesEncoder.Encode(_iAnalogStimuli);
            return;
        }

        /// <summary>
        /// Fetches the next piece of current data.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of input neurons.</param>
        public bool Fetch(bool collectStatistics)
        {
            if (_currentDataIdx == 0 || (_spikesEncoder.Regime == InputEncoder.InputSpikesCoding.Vertical && _currentDataIdx < _spikesEncoder.LargestComponentLength))
            {
                switch (_spikesEncoder.Regime)
                {
                    case InputEncoder.InputSpikesCoding.Horizontal:
                        {
                            //Analog neuron
                            AnalogNeuron.NewStimulation(_iAnalogStimuli, 0d);
                            AnalogNeuron.Recompute(collectStatistics);
                            //Spiking neurons
                            for (int i = 0; i < SpikingNeuronCollection.Length; i++)
                            {
                                SpikingNeuronCollection[i].NewStimulation(_spikesEncoder.AllSpikesFlatCollection[i], 0d);
                                SpikingNeuronCollection[i].Recompute(collectStatistics);
                            }
                        }
                        break;

                    case InputEncoder.InputSpikesCoding.Vertical:
                        {
                            //Analog neuron
                            AnalogNeuron.NewStimulation(_iAnalogStimuli, 0d);
                            AnalogNeuron.Recompute(collectStatistics);
                            //Spiking neurons
                            for (int i = 0; i < _spikesEncoder.ComponentSpikesCollection.Length; i++)
                            {
                                SpikingNeuronCollection[i].NewStimulation(_spikesEncoder.ComponentSpikesCollection[i][_currentDataIdx], 0d);
                                SpikingNeuronCollection[i].Recompute(collectStatistics);
                            }
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
        /// Prepares a set of meaningful combinations of indexes of input spiking neurons to be connected together to a hidden neuron.
        /// </summary>
        /// <param name="numOfCombinations">The desired number of combinations.</param>
        public List<int[]> GetSpikingInputCombinations(int numOfCombinations)
        {
            List<int[]> result = new List<int[]>(numOfCombinations);
            //Alone spiking neurons
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
