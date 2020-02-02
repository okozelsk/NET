using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.MathTools;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Implements the feed forward network supporting multiple hidden layers
    /// </summary>
    [Serializable]
    public class FeedForwardNetwork : INonRecurrentNetwork
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

        //Attribute properties
        /// <summary>
        /// Number of network's input values
        /// </summary>
        public int NumOfInputValues { get; }
        /// <summary>
        /// Number of network's output values
        /// </summary>
        public int NumOfOutputValues { get; }
        /// <summary>
        /// Total number of network's neurons
        /// </summary>
        public int NumOfNeurons { get; private set; }
        /// <summary>
        /// Collection of network's layers
        /// </summary>
        public List<Layer> LayerCollection { get; }

        //Attributes
        private double[] _flatWeights;
        private bool _isAllowedNguyenWidrowRandomization;


        //Constructor
        /// <summary>
        /// Instantiates an unitialized feed forward network
        /// </summary>
        /// <param name="numOfInputValues">Number of network's input values</param>
        /// <param name="numOfOutputValues">Number of network's output values</param>
        public FeedForwardNetwork(int numOfInputValues, int numOfOutputValues)
        {
            //Input/Output counts
            NumOfInputValues = numOfInputValues;
            NumOfOutputValues = numOfOutputValues;
            NumOfNeurons = -1;
            //Network layers
            LayerCollection = new List<Layer>();
            //Flat weights
            _flatWeights = null;
            //Nguyen Widrow initial randomization
            _isAllowedNguyenWidrowRandomization = false;
            return;
        }

        /// <summary>
        /// Instantiates an initialized feed forward network
        /// </summary>
        /// <param name="numOfInputValues">Number of network's input values</param>
        /// <param name="numOfOutputValues">Number of network's output values</param>
        /// <param name="settings">Configuration parameters</param>
        public FeedForwardNetwork(int numOfInputValues, int numOfOutputValues, FeedForwardNetworkSettings settings)
            :this(numOfInputValues, numOfOutputValues)
        {
            Random rand = new Random(1);
            //Initialize FF network
            for (int i = 0; i < settings.HiddenLayerCollection.Count; i++)
            {
                AddLayer(settings.HiddenLayerCollection[i].NumOfNeurons,
                         ActivationFactory.Create(settings.HiddenLayerCollection[i].Activation, rand)
                         );
            }
            FinalizeStructure(ActivationFactory.Create(settings.OutputLayerActivation, rand));
            return;
        }

        //Properties
        /// <summary>
        /// Indicates whether the network structure is finalized
        /// </summary>
        public bool Finalized { get { return NumOfNeurons > 0; } }
        /// <summary>
        /// Total number of network's weights
        /// </summary>
        public int NumOfWeights { get { return _flatWeights.Length; } }
        /// <summary>
        /// Output range of the output layer
        /// </summary>
        public Interval OutputRange { get { return LayerCollection[LayerCollection.Count - 1].Activation.OutputRange; } }

        //Methods
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
                LayerCollection.Add(new Layer(numOfNeurons, activation));
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
            LayerCollection.Add(new Layer(NumOfOutputValues, outputActivation));
            //Finalize layers
            int numOfInputNodes = NumOfInputValues;
            int neuronsFlatStartIdx = 0;
            int weightsFlatStartIdx = 0;
            _isAllowedNguyenWidrowRandomization = true;
            foreach (Layer layer in LayerCollection)
            {
                layer.FinalizeStructure(numOfInputNodes, neuronsFlatStartIdx, weightsFlatStartIdx);
                neuronsFlatStartIdx += layer.NumOfLayerNeurons;
                weightsFlatStartIdx += layer.NumOfLayerNeurons * layer.NumOfInputNodes + layer.NumOfLayerNeurons;
                numOfInputNodes = layer.NumOfLayerNeurons;
                if(layer.Activation.GetType() != typeof(Elliot) &&
                   layer.Activation.GetType() != typeof(TanH)
                   )
                {
                    _isAllowedNguyenWidrowRandomization = false;
                }
            }
            if(LayerCollection.Count < 2)
            {
                _isAllowedNguyenWidrowRandomization = false;
            }
            NumOfNeurons = neuronsFlatStartIdx;
            _flatWeights = new double[weightsFlatStartIdx];
            return;
        }

        /// <summary>
        /// Applies the Nguyen Widrow randomization method.
        /// </summary>
        /// <param name="rand">The random generator to be used</param>
        private void RandomizeWeightsByNguyenWidrowMethod(Random rand)
        {
            foreach (Layer layer in LayerCollection)
            {
                int weightFlatIndex = layer.WeightsStartFlatIdx;
                int biasFlatIndex = layer.BiasesStartFlatIdx;
                double b = 0.35d * Math.Pow(layer.NumOfLayerNeurons, (1d / layer.NumOfInputNodes));
                for(int layerNeuronIdx = 0; layerNeuronIdx < layer.NumOfLayerNeurons; layerNeuronIdx++, biasFlatIndex++)
                {
                    for (int inputNodeIdx = 0; inputNodeIdx < layer.NumOfInputNodes; inputNodeIdx++, weightFlatIndex++)
                    {
                        _flatWeights[weightFlatIndex] = rand.NextRangedUniformDouble(0, b);
                    }
                    _flatWeights[biasFlatIndex] = rand.NextRangedUniformDouble(-b, b);
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
                rand.FillUniform(_flatWeights, WeightDefaultIniMin, WeightDefaultIniMax, true);
            }
            return;
        }

        /// <summary>
        /// Creates a deep copy of this network
        /// </summary>
        public INonRecurrentNetwork DeepClone()
        {
            FeedForwardNetwork clone = new FeedForwardNetwork(NumOfInputValues, NumOfOutputValues)
            {
                NumOfNeurons = NumOfNeurons
            };
            foreach (Layer layer in LayerCollection)
            {
                clone.LayerCollection.Add(layer.DeepClone());
            }
            clone._flatWeights = (double[])_flatWeights.Clone();
            clone._isAllowedNguyenWidrowRandomization = _isAllowedNguyenWidrowRandomization;
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
        /// <param name="flatDerivatives">
        /// It must be an allocated array of length = NumOfNeurons (flat structure).
        /// Function will set the activation derivatives into the array.
        /// </param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] input, List<double[]> layerInputCollection, double[] flatDerivatives)
        {
            double[] result = input;
            foreach (Layer layer in LayerCollection)
            {
                layerInputCollection.Add(result);
                result = layer.Compute(result, _flatWeights, flatDerivatives);
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
        /// <param name="computedOutputCollection">Collection of the computed outputs (batch)</param>
        /// <returns>Error statistics</returns>
        public BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection, out List<double[]> computedOutputCollection)
        {
            double[][] computedOutputs = new double[idealOutputCollection.Count][];
            double[] flatErrors = new double[inputCollection.Count * NumOfOutputValues];
            Parallel.For(0, inputCollection.Count, row =>
            {
                double[] computedOutputVector = Compute(inputCollection[row]);
                computedOutputs[row] = computedOutputVector;
                for (int i = 0; i < NumOfOutputValues; i++)
                {
                    flatErrors[row * NumOfOutputValues + i] = Math.Abs(idealOutputCollection[row][i] - computedOutputVector[i]);
                }
            });
            computedOutputCollection = new List<double[]>(computedOutputs);
            return new BasicStat(flatErrors);
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
            double[] flatErrors = new double[inputCollection.Count * NumOfOutputValues];
            Parallel.For(0, inputCollection.Count, row =>
            {
                double[] computedOutputVector = Compute(inputCollection[row]);
                for (int i = 0; i < NumOfOutputValues; i++)
                {
                    flatErrors[row * NumOfOutputValues + i] = Math.Abs(idealOutputCollection[row][i] - computedOutputVector[i]);
                }
            });
            return new BasicStat(flatErrors);
        }

        /// <summary>
        /// Returns copy of all network internal weights (in a flat format)
        /// </summary>
        public double[] GetWeightsCopy()
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
            /// <summary>
            /// Number of layer input nodes
            /// </summary>
            public int NumOfInputNodes { get; private set; }
            /// <summary>
            /// Number of layer neurons
            /// </summary>
            public int NumOfLayerNeurons { get; }
            /// <summary>
            /// Starting index of this layer weights in a flat structure
            /// </summary>
            public int WeightsStartFlatIdx { get; private set; }
            /// <summary>
            /// Starting index of this layer biases in a flat structure
            /// </summary>
            public int BiasesStartFlatIdx { get; private set; }
            /// <summary>
            /// Starting index of this layer neurons in a flat structure
            /// </summary>
            public int NeuronsStartFlatIdx { get; private set; }


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
                //Setup
                Activation = activation ?? throw new ArgumentException("activation", "Activation can't be null");
                NumOfLayerNeurons = numOfNeurons;
                NumOfInputNodes = -1;
                WeightsStartFlatIdx = 0;
                BiasesStartFlatIdx = 0;
                NeuronsStartFlatIdx = 0;
                return;
            }

            //Properties
            /// <summary>
            /// Indicates whether the layer structure is finalized
            /// </summary>
            public bool Finalized { get { return NumOfInputNodes > 0; } }

            //Methods
            /// <summary>
            /// Finalizes the layer structure
            /// </summary>
            /// <param name="numOfInputNodes">Number of input nodes</param>
            /// <param name="neuronsFlatStartIdx">Starting index of this layer neurons in a flat structure</param>
            /// <param name="weightsFlatStartIdx">Starting index of this layer weights in a flat structure</param>
            internal void FinalizeStructure(int numOfInputNodes, int neuronsFlatStartIdx, int weightsFlatStartIdx)
            {
                NumOfInputNodes = numOfInputNodes;
                WeightsStartFlatIdx = weightsFlatStartIdx;
                BiasesStartFlatIdx = weightsFlatStartIdx + NumOfLayerNeurons * NumOfInputNodes;
                NeuronsStartFlatIdx = neuronsFlatStartIdx;
                return;
            }

            /// <summary>
            /// Creates a deep copy of this layer
            /// </summary>
            internal Layer DeepClone()
            {
                Layer clone = new Layer(NumOfLayerNeurons, Activation)
                {
                    NumOfInputNodes = NumOfInputNodes,
                    WeightsStartFlatIdx = WeightsStartFlatIdx,
                    BiasesStartFlatIdx = BiasesStartFlatIdx,
                    NeuronsStartFlatIdx = NeuronsStartFlatIdx
                };
                return clone;
            }

            /// <summary>
            /// Computes the states of the layer's neurons
            /// </summary>
            /// <param name="inputs">The inputs for this layer</param>
            /// <param name="flatWeights">Network's weights in a flat structure</param>
            /// <param name="flatDerivatives">Network's neuron state derivatives in a flat structure</param>
            /// <returns>The layer's neurons states</returns>
            internal double[] Compute(double[] inputs, double[] flatWeights, double[] flatDerivatives = null)
            {
                double[] result = new double[NumOfLayerNeurons];
                int weightFlatIdx = WeightsStartFlatIdx;
                int biasFlatIdx = BiasesStartFlatIdx;
                for (int neuronIdx = 0; neuronIdx < NumOfLayerNeurons; neuronIdx++, biasFlatIdx++)
                {
                    double sum = flatWeights[biasFlatIdx] * BiasValue;
                    for (int inputIdx = 0; inputIdx < NumOfInputNodes; inputIdx++, weightFlatIdx++)
                    {
                        sum += flatWeights[weightFlatIdx] * inputs[inputIdx];
                    }
                    result[neuronIdx] = Activation.Compute(sum);
                    if (flatDerivatives != null)
                    {
                        flatDerivatives[NeuronsStartFlatIdx + neuronIdx] = Activation.ComputeDerivative(result[neuronIdx], sum);
                    }
                }
                return result;
            }

        }//Layer
    }//FeedForwardNetwork

}//Namespace
