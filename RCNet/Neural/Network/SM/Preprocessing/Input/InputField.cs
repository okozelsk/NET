using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Provides input field data to be processed in the reservoirs. Supports analog and spike codings.
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
        /// Specifies if to route values as the additional predictors to readout
        /// </summary>
        public bool RouteToReadout { get; }

        /// <summary>
        /// Input neuron providing analog value
        /// </summary>
        public AnalogInputNeuron AnalogNeuron { get; }

        /// <summary>
        /// Collection of input neurons representing spike train of analog value
        /// </summary>
        public SpikingInputNeuron[] SpikingNeuronCollection { get; }

        //Attributes
        private readonly BaseFeatureFilter _featureFilter;
        private readonly SpikeCode _realSpikeCode;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="name">Name of the input field</param>
        /// <param name="idx">Index of the input field</param>
        /// <param name="coordinates">Input coordinates (entry point)</param>
        /// <param name="dataWorkingRange">Input data range</param>
        /// <param name="featureFilterCfg">Feature filter configuration</param>
        /// <param name="spikeCodeCfg">Configuration of the input spike code</param>
        /// <param name="routeToReadout">Specifies if to route values as the additional predictors to readout</param>
        /// <param name="inputNeuronsStartIdx">Index of the first input neuron of this unit among all input neurons</param>
        /// <param name="predictorsCfg">Configuration of predictors</param>
        public InputField(string name,
                          int idx,
                          int[] coordinates,
                          Interval dataWorkingRange,
                          IFeatureFilterSettings featureFilterCfg,
                          SpikeCodeSettings spikeCodeCfg,
                          bool routeToReadout,
                          int inputNeuronsStartIdx,
                          PredictorsSettings predictorsCfg
                          )
        {
            Name = name;
            Idx = idx;
            RouteToReadout = routeToReadout;
            _featureFilter = FeatureFilterFactory.Create(dataWorkingRange, featureFilterCfg);
            //Analog neuron
            AnalogNeuron = new AnalogInputNeuron(new NeuronLocation(InputEncoder.ReservoirID, inputNeuronsStartIdx, InputEncoder.PoolID, inputNeuronsStartIdx, idx, coordinates[0], coordinates[1], coordinates[2]),
                                                 _featureFilter.OutputRange,
                                                 null
                                                 );
            ++inputNeuronsStartIdx;
            //Spiking neurons
            _realSpikeCode = null;
            int populationSize = -1;
            switch(_featureFilter.Type)
            {
                case BaseFeatureFilter.FeatureType.Real:
                    _realSpikeCode = new SpikeCode(spikeCodeCfg);
                    populationSize = _realSpikeCode.Code.Length;
                    break;
                case BaseFeatureFilter.FeatureType.Binary:
                    populationSize = 1;
                    break;
                case BaseFeatureFilter.FeatureType.Enum:
                    populationSize = ((EnumFeatureFilterSettings)featureFilterCfg).NumOfElements;
                    break;
            }
            SpikingNeuronCollection = new SpikingInputNeuron[populationSize];
            for (int i = 0; i < SpikingNeuronCollection.Length; i++)
            {
                SpikingNeuronCollection[i] = new SpikingInputNeuron(new NeuronLocation(InputEncoder.ReservoirID, inputNeuronsStartIdx, InputEncoder.PoolID, inputNeuronsStartIdx, idx, coordinates[0], coordinates[1], coordinates[2]),
                                                                    predictorsCfg
                                                                    );
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
            _realSpikeCode?.Reset();
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
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public void SetNewData(double value, bool collectStatistics)
        {
            double iStimuli = _featureFilter.ApplyFilter(value);
            //Analog neuron
            AnalogNeuron.NewStimulation(iStimuli, 0d);
            AnalogNeuron.Recompute(collectStatistics);
            //Spiking neurons
            switch (_featureFilter.Type)
            {
                case BaseFeatureFilter.FeatureType.Real:
                    {
                        _realSpikeCode.Encode(iStimuli);
                        for(int i = 0; i < SpikingNeuronCollection.Length; i++)
                        {
                            SpikingNeuronCollection[i].NewStimulation(_realSpikeCode.Code[i], 0d);
                            SpikingNeuronCollection[i].Recompute(collectStatistics);
                        }
                    }
                    break;

                case BaseFeatureFilter.FeatureType.Binary:
                    {
                        SpikingNeuronCollection[0].NewStimulation(value, 0d);
                        SpikingNeuronCollection[0].Recompute(collectStatistics);
                    }
                    break;

                case BaseFeatureFilter.FeatureType.Enum:
                    {
                        int neuronIdx = ((int)Math.Round(value, 0)) - 1;
                        for (int i = 0; i < SpikingNeuronCollection.Length; i++)
                        {
                            double spikeVal = i == neuronIdx ? 1d : 0d;
                            SpikingNeuronCollection[i].NewStimulation(spikeVal, 0d);
                            SpikingNeuronCollection[i].Recompute(collectStatistics);
                        }
                    }
                    break;
            }
            return;
        }

        /// <summary>
        /// Prepares set of meaningful combinations of indexes of input spiking neurons to be connected together.
        /// </summary>
        /// <param name="numOfCombinations">Desired number of connections</param>
        public List<int[]> GetSpikingInputCombinations(int numOfCombinations)
        {
            List<int[]> result = new List<int[]>(numOfCombinations);
            //Alone neurons
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
            //Maximum number of combinations is reached
            return result;
        }

        /// <summary>
        /// Returns collection of input neurons having enabled at least one predictor
        /// </summary>
        /// <param name="numOfPredictors">Returned number of predictors</param>
        public List<INeuron> GetPredictingNeurons(out int numOfPredictors)
        {
            numOfPredictors = 0;
            List<INeuron> predictingNeurons = new List<INeuron>();
            if (AnalogNeuron.NumOfEnabledPredictors > 0)
            {
                numOfPredictors += AnalogNeuron.NumOfEnabledPredictors;
                predictingNeurons.Add(AnalogNeuron);
            }
            foreach (SpikingInputNeuron neuron in SpikingNeuronCollection)
            {
                if (neuron.NumOfEnabledPredictors > 0)
                {
                    numOfPredictors += neuron.NumOfEnabledPredictors;
                    predictingNeurons.Add(neuron);
                }
            }
            return predictingNeurons;
        }

    }//InputField

}//Namespace
