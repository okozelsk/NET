using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.MathTools;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Implements the feed forward network supporting multiple hidden layers
    /// </summary>
    [Serializable]
    public class FeedForwardNetwork
    {
        //Constants
        /// <summary>
        /// Dummy bias input value
        /// </summary>
        public const double BiasValue = 1d;
        /// <summary>
        /// Default minimum weight range limit for random initialization
        /// </summary>
        public const double WeightDefaultIniMin = 0.005;
        /// <summary>
        /// Default maximum weight range limit for random initialization
        /// </summary>
        public const double WeightDefaultIniMax = 0.05;
        
        //Attributes
        private int _numOfInputValues;
        private int _numOfOutputValues;
        private int _numOfNeurons;
        private List<Layer> _layerCollection;
        private double[] _flatWeights;
        private bool _isAllowedNguyenWidrowRandomization;

        //Constructor
        /// <summary>
        /// Instantiates a feed forward network
        /// </summary>
        /// <param name="numOfInputValues">Number of network's input values</param>
        /// <param name="numOfOutputValues">Number of network's output values</param>
        public FeedForwardNetwork(int numOfInputValues, int numOfOutputValues)
        {
            //Input/Output counts
            _numOfInputValues = numOfInputValues;
            _numOfOutputValues = numOfOutputValues;
            _numOfNeurons = -1;
            //Network layers
            _layerCollection = new List<Layer>();
            //Flat weights
            _flatWeights = null;
            //Nguyen Widrow initial randomization
            _isAllowedNguyenWidrowRandomization = false;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates whether the network structure is finalized
        /// </summary>
        public bool Finalized { get { return _numOfNeurons > 0; } }
        /// <summary>
        /// Number of network's input values
        /// </summary>
        public int NumOfInputValues { get { return _numOfInputValues; } }
        /// <summary>
        /// Number of network's output values
        /// </summary>
        public int NumOfOutputValues { get { return _numOfOutputValues; } }
        /// <summary>
        /// Total number of network's neurons
        /// </summary>
        public int NumOfNeurons { get { return _numOfNeurons; } }
        /// <summary>
        /// Total number of network's weights
        /// </summary>
        public int NumOfWeights { get { return _flatWeights.Length; } }
        /// <summary>
        /// Collection of network's layers
        /// </summary>
        public List<Layer> LayerCollection { get { return _layerCollection; } }

        //Methods
        //Static methods
        /// <summary>
        /// Parses training method type from string code
        /// </summary>
        /// <param name="code">Code of the training method type</param>
        public static TrainingMethodType ParseTrainingMethodType(string code)
        {
            switch (code.ToUpper())
            {
                case "LINEAR": return TrainingMethodType.Linear;
                case "RESILIENT": return TrainingMethodType.Resilient;
                default:
                    throw new ArgumentException($"Unknown training method code {code}");
            }
        }

        //Instance methods
        /// <summary>
        /// Adds the new hidden layer into this network
        /// </summary>
        /// <param name="numOfNeurons">Number of layer's neurons</param>
        /// <param name="activation">Each of layer's neuron will be activated by this activation function</param>
        public void AddLayer(int numOfNeurons, IActivationFunction activation)
        {
            if (!Finalized)
            {
                //Add new layer
                _layerCollection.Add(new Layer(numOfNeurons, activation));
            }
            else
            {
                throw new Exception("Can´t add new layer. Network structure is finalized.");
            }
            return;
        }

        /// <summary>
        /// Finalizes the network internal structure and locks it against further changes.
        /// </summary>
        /// <param name="outputActivation">Activation function of the output layer's neurons</param>
        public void FinalizeStructure(IActivationFunction outputActivation)
        {
            if (Finalized)
            {
                throw new Exception("Network structure has been already finalized.");
            }
            //Add output layer
            _layerCollection.Add(new Layer(NumOfOutputValues, outputActivation));
            //Finalize layers
            int numOfInputNodes = NumOfInputValues;
            int neuronsFlatStartIdx = 0;
            int weightsFlatStartIdx = 0;
            _isAllowedNguyenWidrowRandomization = true;
            foreach (Layer layer in _layerCollection)
            {
                layer.FinalizeStructure(numOfInputNodes, neuronsFlatStartIdx, weightsFlatStartIdx);
                neuronsFlatStartIdx += layer.NumOfLayerNeurons;
                weightsFlatStartIdx += (layer.NumOfLayerNeurons * layer.NumOfInputNodes + layer.NumOfLayerNeurons);
                numOfInputNodes = layer.NumOfLayerNeurons;
                if(layer.Activation.GetType() != typeof(ElliotAF) &&
                   layer.Activation.GetType() != typeof(TanhAF)
                   )
                {
                    _isAllowedNguyenWidrowRandomization = false;
                }
            }
            if(_layerCollection.Count < 2)
            {
                _isAllowedNguyenWidrowRandomization = false;
            }
            _numOfNeurons = neuronsFlatStartIdx;
            _flatWeights = new double[weightsFlatStartIdx];
            return;
        }

        /// <summary>
        /// Applies the Nguyen Widrow randomization method.
        /// </summary>
        /// <param name="rand">The random generator to be used</param>
        private void RandomizeWeightsByNguyenWidrowMethod(Random rand)
        {
            foreach (Layer layer in _layerCollection)
            {
                int weightFlatIndex = layer.WeightsStartFlatIdx;
                int biasFlatIndex = layer.BiasesStartFlatIdx;
                double b = 0.35d * Math.Pow(layer.NumOfLayerNeurons, (1d / layer.NumOfInputNodes));
                for(int layerNeuronIdx = 0; layerNeuronIdx < layer.NumOfLayerNeurons; layerNeuronIdx++, biasFlatIndex++)
                {
                    for (int inputNodeIdx = 0; inputNodeIdx < layer.NumOfInputNodes; inputNodeIdx++, weightFlatIndex++)
                    {
                        _flatWeights[weightFlatIndex] = rand.NextBoundedUniformDouble(0, b);
                    }
                    _flatWeights[biasFlatIndex] = rand.NextBoundedUniformDouble(-b, b);
                }
            }
            return;
        }

        /// <summary>
        /// Randomizes network's weights (this function must be called before the network training)
        /// </summary>
        /// <param name="rand">Random generator to be used</param>
        public void RandomizeWeights(Random rand)
        {
            if(!Finalized)
            {
                throw new Exception("Can´t randomize weights. Network structure is not finalized.");
            }
            if (_isAllowedNguyenWidrowRandomization)
            {
                RandomizeWeightsByNguyenWidrowMethod(rand);
            }
            else
            {
                rand.FillUniformRS(_flatWeights, WeightDefaultIniMin, WeightDefaultIniMax);
            }
            return;
        }

        /// <summary>
        /// Creates the copy of this network
        /// </summary>
        public FeedForwardNetwork Clone()
        {
            FeedForwardNetwork clone = new FeedForwardNetwork(NumOfInputValues, NumOfOutputValues);
            clone._numOfNeurons = _numOfNeurons;
            foreach (Layer layer in LayerCollection)
            {
                clone.LayerCollection.Add(layer.Clone());
            }
            clone._flatWeights = (double[])_flatWeights.Clone();
            return clone;
        }

        /// <summary>
        /// Computes network output values (faster version for standard usage)
        /// </summary>
        /// <param name="input">Input values to be passed into the network</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] input)
        {
            double[] result = input;
            foreach (Layer layer in LayerCollection)
            {
                result = layer.Compute(result, _flatWeights);
            }
            return result;
        }

        /// <summary>
        /// Computes network output values (slower version for training purposes)
        /// </summary>
        /// <param name="input">
        /// Input values to be passed into the network
        /// </param>
        /// <param name="layerInputCollection">
        /// It must be an instantiated empty collection.
        /// Function will add inputs for each network layer into the collection.
        /// </param>
        /// <param name="flatDerivations">
        /// It must be an allocated array of length = NumOfNeurons (flat structure).
        /// Function will set the activation derivations into the array.
        /// </param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] input, List<double[]> layerInputCollection, double[] flatDerivations)
        {
            double[] result = input;
            foreach (FeedForwardNetwork.Layer layer in _layerCollection)
            {
                layerInputCollection.Add(result);
                result = layer.Compute(result, _flatWeights, flatDerivations);
            }
            return result;
        }

        /// <summary>
        /// Function goes through collection (batch) of the network inputs and for each of them computes the output.
        /// Computed output is then compared with a corresponding ideal output.
        /// The error Abs(ideal - computed) is passed to the result error statistics.
        /// </summary>
        /// <param name="inputCollection">Collection of the network inputs (batch)</param>
        /// <param name="idealOutputCollection">Collection of the ideal outputs (batch)</param>
        /// <returns>Error statistics</returns>
        public BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection)
        {
            BasicStat errStat = new BasicStat();
            Parallel.For(0, inputCollection.Count, row =>
            {
                double[] computedOutputs = Compute(inputCollection[row]);
                for (int i = 0; i < _numOfOutputValues; i++)
                {
                    double error = idealOutputCollection[row][i] - computedOutputs[i];
                    errStat.AddSampleValue(Math.Abs(error));
                }
            });
            return errStat;
        }

        /// <summary>
        /// Returns copy of all network internal weights (in a flat format)
        /// </summary>
        public double[] GetWeights()
        {
            return (double[])_flatWeights.Clone();
        }

        /// <summary>
        /// Adopts the given weights.
        /// </summary>
        /// <param name="newFlatWeights">New weights to be adopted (in a flat format)</param>
        public void SetWeights(double[] newFlatWeights)
        {
            newFlatWeights.CopyTo(_flatWeights, 0);
            return;
        }

        /// <summary>
        /// Function creates the statistics of the internal weights
        /// </summary>
        public BasicStat ComputeWeightsStat()
        {
            return (new BasicStat(_flatWeights));
        }

        //Inner classes
        /// <summary>
        /// Class represents the layer of the feed forward network
        /// </summary>
        [Serializable]
        public class Layer
        {
            //Attribute properties
            /// <summary>
            /// The activation function of the layer's neurons
            /// </summary>
            public IActivationFunction Activation { get; }
            //Attributes
            private int _numOfInputNodes;
            private int _numOfLayerNeurons;
            private int _weightsStartFlatIdx;
            private int _biasesStartFlatIdx;
            private int _neuronsStartFlatIdx;

            //Constructor
            /// <summary>
            /// Instantiates the layer
            /// </summary>
            /// <param name="numOfNeurons">Number of layer's neurons</param>
            /// <param name="activation">Each of the layer's neuron will be activated by this activation function</param>
            public Layer(int numOfNeurons, IActivationFunction activation)
            {
                //Check correctness
                if (numOfNeurons < 1)
                {
                    throw new ArgumentOutOfRangeException("numOfNeurons", $"Invalid parameter value: {numOfNeurons}");
                }
                if (activation == null)
                {
                    throw new ArgumentException("activation", "Activation can't be null");
                }
                //Setup
                Activation = activation;
                _numOfLayerNeurons = numOfNeurons;
                _numOfInputNodes = -1;
                _weightsStartFlatIdx = 0;
                _biasesStartFlatIdx = 0;
                _neuronsStartFlatIdx = 0;
                return;
            }

            //Properties
            /// <summary>
            /// Indicates whether the layer structure is finalized
            /// </summary>
            public bool Finalized { get { return _numOfInputNodes > 0; } }
            /// <summary>
            /// Number of layer input nodes
            /// </summary>
            public int NumOfInputNodes { get { return _numOfInputNodes; } }
            /// <summary>
            /// Number of layer neurons
            /// </summary>
            public int NumOfLayerNeurons { get { return _numOfLayerNeurons; } }
            /// <summary>
            /// Starting index of this layer weights in a flat structure
            /// </summary>
            public int WeightsStartFlatIdx { get { return _weightsStartFlatIdx; } }
            /// <summary>
            /// Starting index of this layer biases in a flat structure
            /// </summary>
            public int BiasesStartFlatIdx { get { return _biasesStartFlatIdx; } }
            /// <summary>
            /// Starting index of this layer neurons in a flat structure
            /// </summary>
            public int NeuronsStartFlatIdx { get { return _neuronsStartFlatIdx; } }


            //Methods
            /// <summary>
            /// Finalizes the layer structure
            /// </summary>
            /// <param name="numOfInputNodes">Number of input nodes</param>
            /// <param name="neuronsFlatStartIdx">Starting index of this layer neurons in a flat structure</param>
            /// <param name="weightsFlatStartIdx">Starting index of this layer weights in a flat structure</param>
            internal void FinalizeStructure(int numOfInputNodes, int neuronsFlatStartIdx, int weightsFlatStartIdx)
            {
                _numOfInputNodes = numOfInputNodes;
                _weightsStartFlatIdx = weightsFlatStartIdx;
                _biasesStartFlatIdx = weightsFlatStartIdx + _numOfLayerNeurons * _numOfInputNodes;
                _neuronsStartFlatIdx = neuronsFlatStartIdx;
                return;
            }

            /// <summary>
            /// Creates a deep copy of this layer
            /// </summary>
            internal Layer Clone()
            {
                Layer clone = new Layer(_numOfLayerNeurons, Activation);
                clone._numOfInputNodes = _numOfInputNodes;
                clone._weightsStartFlatIdx = _weightsStartFlatIdx;
                clone._biasesStartFlatIdx = _biasesStartFlatIdx;
                clone._neuronsStartFlatIdx = _neuronsStartFlatIdx;
                return clone;
            }

            /// <summary>
            /// Computes the states of the layer's neurons
            /// </summary>
            /// <param name="inputs">The inputs for this layer</param>
            /// <param name="flatWeights">Network's weights in a flat structure</param>
            /// <param name="flatDerivations">Network's neuron state derivations in a flat structure</param>
            /// <returns>The layer's neurons states</returns>
            internal double[] Compute(double[] inputs, double[] flatWeights, double[] flatDerivations = null)
            {
                double[] result = new double[NumOfLayerNeurons];
                int weightFlatIdx = _weightsStartFlatIdx;
                int biasFlatIdx = _biasesStartFlatIdx;
                for (int neuronIdx = 0; neuronIdx < NumOfLayerNeurons; neuronIdx++, biasFlatIdx++)
                {
                    double sum = flatWeights[biasFlatIdx] * FeedForwardNetwork.BiasValue;
                    for (int inputIdx = 0; inputIdx < NumOfInputNodes; inputIdx++, weightFlatIdx++)
                    {
                        sum += flatWeights[weightFlatIdx] * inputs[inputIdx];
                    }
                    result[neuronIdx] = Activation.Compute(sum);
                    if(flatDerivations != null)
                    {
                        flatDerivations[_neuronsStartFlatIdx + neuronIdx] = Activation.Derive(result[neuronIdx], sum);
                    }
                }
                return result;
            }

        }//Layer
    }//FeedForwardNetwork

    /// <summary>
    /// Supported training methods
    /// </summary>
    public enum TrainingMethodType
    {
        /// <summary>
        /// Linear regression
        /// </summary>
        Linear,
        /// <summary>
        /// Resilient backpropagation
        /// </summary>
        Resilient
    }//TrainingMethodType

    /// <summary>
    /// Feed forward network hidden layer settings
    /// </summary>
    [Serializable]
    public class HiddenLayerSettings
    {
        //Attributes
        /// <summary>
        /// Number of hidden layer neurons
        /// </summary>
        public int NumOfNeurons { get; set; }
        /// <summary>
        /// Type of activation function of the hidden layer neurons
        /// </summary>
        public ActivationFactory.ActivationType ActivationType { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="numOfNeurons">Number of hidden layer neurons</param>
        /// <param name="activationType">Type of activation function of the hidden layer neurons</param>
        public HiddenLayerSettings(int numOfNeurons, ActivationFactory.ActivationType activationType)
        {
            NumOfNeurons = numOfNeurons;
            ActivationType = activationType;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public HiddenLayerSettings(HiddenLayerSettings source)
        {
            NumOfNeurons = source.NumOfNeurons;
            ActivationType = source.ActivationType;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// </summary>
        /// <param name="hiddenLayerElem">
        /// Xml data containing the settings.
        /// Content of xml element is not validated against the xml schema.
        /// </param>
        public HiddenLayerSettings(XElement hiddenLayerElem)
        {
            NumOfNeurons = int.Parse(hiddenLayerElem.Attribute("Neurons").Value);
            ActivationType = ActivationFactory.ParseActivation(hiddenLayerElem.Attribute("Activation").Value);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            HiddenLayerSettings cmpSettings = obj as HiddenLayerSettings;
            if (NumOfNeurons != cmpSettings.NumOfNeurons || ActivationType != cmpSettings.ActivationType)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return NumOfNeurons.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public HiddenLayerSettings DeepClone()
        {
            return new HiddenLayerSettings(this);
        }

    }//HiddenLayerSettings

}//Namespace
