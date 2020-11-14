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
        private readonly InputSpikesCoder _spikesEncoder;
        private double _iAnalogStimuli;
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
        /// <param name="spikesEncodingCfg">Configuration of the analog value to spikes encoding</param>
        /// <param name="routeToReadout">Specifies whether to route values as the additional predictors to readout</param>
        /// <param name="inputNeuronsStartIdx">Index of the first input neuron of this field among all input neurons</param>
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
            int verticalCycles = _spikesEncoder.Regime == InputEncoder.SpikingInputEncodingRegime.Vertical ? _spikesEncoder.LargestComponentLength : 1;
            AnalogNeuron = new AnalogInputNeuron(new NeuronLocation(InputEncoder.ReservoirID, inputNeuronsStartIdx, InputEncoder.PoolID, inputNeuronsStartIdx, idx, coordinates[0], coordinates[1], coordinates[2]), verticalCycles);
            ++inputNeuronsStartIdx;
            //Spiking neurons
            int spikingPopulationSize;
            if (_spikesEncoder.Regime == InputEncoder.SpikingInputEncodingRegime.Horizontal)
            {
                //Population encoding
                spikingPopulationSize = _spikesEncoder.AllSpikesFlatCollection.Length;
            }
            else if (_spikesEncoder.Regime == InputEncoder.SpikingInputEncodingRegime.Vertical)
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
            _spikesEncoder?.Reset();
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
            _spikesEncoder.Encode(_iAnalogStimuli);
            return;
        }

        /// <summary>
        /// Fetches next piece of current data
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public bool Fetch(bool collectStatistics)
        {
            if (_currentDataIdx == 0 || (_spikesEncoder.Regime == InputEncoder.SpikingInputEncodingRegime.Vertical && _currentDataIdx < _spikesEncoder.LargestComponentLength))
            {
                switch (_spikesEncoder.Regime)
                {
                    case InputEncoder.SpikingInputEncodingRegime.Horizontal:
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

                    case InputEncoder.SpikingInputEncodingRegime.Vertical:
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
        /// Prepares set of meaningful combinations of indexes of input spiking neurons to be connected to a target hidden neuron.
        /// </summary>
        /// <param name="numOfCombinations">Desired number of connections</param>
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
