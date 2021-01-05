using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Activation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Implements the Feed Forward network supporting multiple hidden layers.
    /// </summary>
    [Serializable]
    public class FeedForwardNetwork : INonRecurrentNetwork
    {
        //Constants
        /// <summary>
        /// The dummy bias input.
        /// </summary>
        public const double BiasValue = 1d;
        /// <summary>
        /// The default minimum weight for the random initialization.
        /// </summary>
        public const double WeightDefaultIniMin = 0.005;
        /// <summary>
        /// The default maximum weight for the random initialization.
        /// </summary>
        public const double WeightDefaultIniMax = 0.05;

        //Attribute properties
        /// <inheritdoc/>
        public int NumOfInputValues { get; }
        /// <inheritdoc/>
        public int NumOfOutputValues { get; }
        /// <summary>
        /// The total number of network's neurons.
        /// </summary>
        public int NumOfNeurons { get; private set; }
        /// <summary>
        /// The collection of the network's layers.
        /// </summary>
        public List<Layer> LayerCollection { get; }

        //Attributes
        private double[] _flatWeights;
        private bool _isAllowedNguyenWidrowRandomization;


        //Constructor
        /// <summary>
        /// Creates an unitialized instance.
        /// </summary>
        /// <param name="numOfInputValues">The number of the network's input values.</param>
        /// <param name="numOfOutputValues">The number of the network's output values.</param>
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfInputValues">The number of the network's input values.</param>
        /// <param name="numOfOutputValues">The number of the network's output values.</param>
        /// <param name="cfg">The configuration of the network and associated trainer.</param>
        public FeedForwardNetwork(int numOfInputValues, int numOfOutputValues, FeedForwardNetworkSettings cfg)
            : this(numOfInputValues, numOfOutputValues)
        {
            Random rand = new Random(1);
            //Initialize FF network
            for (int i = 0; i < cfg.HiddenLayersCfg.HiddenLayerCfgCollection.Count; i++)
            {
                AddLayer(cfg.HiddenLayersCfg.HiddenLayerCfgCollection[i].NumOfNeurons,
                         (AFAnalogBase)ActivationFactory.CreateAF(cfg.HiddenLayersCfg.HiddenLayerCfgCollection[i].ActivationCfg, rand)
                         );
            }
            FinalizeStructure((AFAnalogBase)ActivationFactory.CreateAF(cfg.OutputActivationCfg, rand));
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the network structure is finalized.
        /// </summary>
        public bool Finalized { get { return NumOfNeurons > 0; } }
        /// <inheritdoc/>
        public int NumOfWeights { get { return _flatWeights.Length; } }
        /// <inheritdoc/>
        public Interval OutputRange { get { return LayerCollection[LayerCollection.Count - 1].Activation.OutputRange; } }

        //Static methods
        /// <summary>
        /// Tests whether the activation function can be used as the FF network's output layer activation.
        /// </summary>
        /// <param name="activationCfg">The configuration of the activation function.</param>
        public static bool IsAllowedOutputAF(IActivationSettings activationCfg)
        {
            if (activationCfg.TypeOfActivation != ActivationType.Analog)
            {
                return false;
            }
            AFAnalogBase analogAF = (AFAnalogBase)ActivationFactory.CreateAF(activationCfg, new Random(0));
            if (!analogAF.SupportsDerivative)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Tests whether the activation function can be used as the FF network's hidden layer activation.
        /// </summary>
        /// <param name="activationCfg">The configuration of the activation function.</param>
        public static bool IsAllowedHiddenAF(IActivationSettings activationCfg)
        {
            if (activationCfg.TypeOfActivation != ActivationType.Analog)
            {
                return false;
            }
            AFAnalogBase analogAF = (AFAnalogBase)ActivationFactory.CreateAF(activationCfg, new Random(0));
            if (!analogAF.SupportsDerivative || analogAF.DependsOnSorround)
            {
                return false;
            }
            return true;
        }

        //Methods
        /// <summary>
        /// Adds the new hidden layer into the network structure.
        /// </summary>
        /// <param name="numOfNeurons">The number of layer's neurons.</param>
        /// <param name="activation">The activation function of the layer neurons.</param>
        public void AddLayer(int numOfNeurons, AFAnalogBase activation)
        {
            if (!Finalized)
            {
                if (activation.DependsOnSorround)
                {
                    throw new ArgumentException("Activation requires multiple input for the Compute method. It is not allowed for the hidden layer.", "activation");
                }
                //Add new layer
                LayerCollection.Add(new Layer(numOfNeurons, activation));
            }
            else
            {
                throw new InvalidOperationException($"Can´t add new layer. Network structure is finalized.");
            }
            return;
        }

        /// <summary>
        /// Finalizes the network internal structure and locks it against the further changes.
        /// </summary>
        /// <param name="outputActivation">The activation function of the output layer.</param>
        public void FinalizeStructure(AFAnalogBase outputActivation)
        {
            if (Finalized)
            {
                throw new InvalidOperationException($"Network structure has been already finalized.");
            }
            if (outputActivation.DependsOnSorround && NumOfOutputValues < 2)
            {
                throw new ArgumentException("Activation requires multiple input for the Compute method but number of output values is less than 2.", "outputActivation");
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
                if (layer.Activation.GetType() != typeof(AFAnalogElliot) &&
                   layer.Activation.GetType() != typeof(AFAnalogTanH)
                   )
                {
                    _isAllowedNguyenWidrowRandomization = false;
                }
            }
            if (LayerCollection.Count < 2)
            {
                _isAllowedNguyenWidrowRandomization = false;
            }
            NumOfNeurons = neuronsFlatStartIdx;
            _flatWeights = new double[weightsFlatStartIdx];
            return;
        }

        /// <summary>
        /// Randomizes internal weights using the Nguyen Widrow method.
        /// </summary>
        /// <param name="rand">The random generator to be used.</param>
        private void RandomizeWeightsByNguyenWidrowMethod(Random rand)
        {
            foreach (Layer layer in LayerCollection)
            {
                int weightFlatIndex = layer.WeightsStartFlatIdx;
                int biasFlatIndex = layer.BiasesStartFlatIdx;
                double b = 0.35d * Math.Pow(layer.NumOfLayerNeurons, (1d / layer.NumOfInputNodes));
                for (int layerNeuronIdx = 0; layerNeuronIdx < layer.NumOfLayerNeurons; layerNeuronIdx++, biasFlatIndex++)
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

        /// <inheritdoc/>
        public void RandomizeWeights(Random rand)
        {
            if (!Finalized)
            {
                throw new InvalidOperationException($"Can´t randomize weights. Network structure is not finalized.");
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        /// Computes the output values (slower version for training purposes).
        /// </summary>
        /// <param name="input">The input values to be passed into the network.</param>
        /// <param name="layerInputCollection">It must be the instantiated empty collection. Function will add inputs for each network layer into this collection.</param>
        /// <param name="flatDerivatives">It must be the allocated array of length = NumOfNeurons (flat structure). Function will set the activation derivatives into this array.</param>
        /// <returns>The computed output values.</returns>
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        /// Gets the copy of internal weights (in a flat format).
        /// </summary>
        public double[] GetWeightsCopy()
        {
            return (double[])_flatWeights.Clone();
        }

        /// <summary>
        /// Sets the internal weights.
        /// </summary>
        /// <param name="newFlatWeights">The new weights to be adopted (in a flat format).</param>
        public void SetWeights(double[] newFlatWeights)
        {
            newFlatWeights.CopyTo(_flatWeights, 0);
            return;
        }

        /// <inheritdoc/>
        public BasicStat ComputeWeightsStat()
        {
            return (new BasicStat(_flatWeights));
        }

        //Inner classes
        /// <summary>
        /// Implements the layer of the feed forward network.
        /// </summary>
        [Serializable]
        public class Layer
        {
            //Attribute properties
            /// <summary>
            /// The activation function of the layer.
            /// </summary>
            public AFAnalogBase Activation { get; }
            /// <summary>
            /// The number of layer input nodes.
            /// </summary>
            public int NumOfInputNodes { get; private set; }
            /// <summary>
            /// The number of layer neurons.
            /// </summary>
            public int NumOfLayerNeurons { get; }
            /// <summary>
            /// The starting index of this layer weights in a flat structure.
            /// </summary>
            public int WeightsStartFlatIdx { get; private set; }
            /// <summary>
            /// The starting index of this layer biases in a flat structure.
            /// </summary>
            public int BiasesStartFlatIdx { get; private set; }
            /// <summary>
            /// The starting index of this layer neurons in a flat structure.
            /// </summary>
            public int NeuronsStartFlatIdx { get; private set; }


            //Constructor
            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="numOfNeurons">The number of layer neurons.</param>
            /// <param name="activation">The activation function of the layer.</param>
            public Layer(int numOfNeurons, AFAnalogBase activation)
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
            /// Indicates the layer structure is finalized.
            /// </summary>
            public bool Finalized { get { return NumOfInputNodes > 0; } }

            //Methods
            /// <summary>
            /// Finalizes the layer structure.
            /// </summary>
            /// <param name="numOfInputNodes">The number of input nodes.</param>
            /// <param name="neuronsFlatStartIdx">The starting index of this layer neurons in a flat structure.</param>
            /// <param name="weightsFlatStartIdx">The starting index of this layer weights in a flat structure.</param>
            internal void FinalizeStructure(int numOfInputNodes, int neuronsFlatStartIdx, int weightsFlatStartIdx)
            {
                NumOfInputNodes = numOfInputNodes;
                WeightsStartFlatIdx = weightsFlatStartIdx;
                BiasesStartFlatIdx = weightsFlatStartIdx + NumOfLayerNeurons * NumOfInputNodes;
                NeuronsStartFlatIdx = neuronsFlatStartIdx;
                return;
            }

            /// <summary>
            /// Creates the deep copy instance of this layer.
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
            /// Computes the layer neurons.
            /// </summary>
            /// <param name="inputs">The inputs for this layer.</param>
            /// <param name="flatWeights">The network's weights in a flat structure.</param>
            /// <param name="flatDerivatives">The network's derivatives in a flat structure.</param>
            /// <returns>The layer activations.</returns>
            internal double[] Compute(double[] inputs, double[] flatWeights, double[] flatDerivatives = null)
            {
                int weightFlatIdx = WeightsStartFlatIdx;
                int biasFlatIdx = BiasesStartFlatIdx;
                //Compute summed weighted inputs
                double[] sums = new double[NumOfLayerNeurons];
                for (int neuronIdx = 0; neuronIdx < NumOfLayerNeurons; neuronIdx++, biasFlatIdx++)
                {
                    sums[neuronIdx] = flatWeights[biasFlatIdx] * BiasValue;
                    for (int inputIdx = 0; inputIdx < NumOfInputNodes; inputIdx++, weightFlatIdx++)
                    {
                        sums[neuronIdx] += flatWeights[weightFlatIdx] * inputs[inputIdx];
                    }
                }
                //Compute activations
                double[] activations = new double[NumOfLayerNeurons];
                Activation.Compute(sums, activations);
                if (flatDerivatives != null)
                {
                    double[] derivatives = new double[NumOfLayerNeurons];
                    //Compute derivatives
                    Activation.ComputeDerivative(activations, sums, derivatives);
                    //Copy derivatives to flat buffer
                    derivatives.CopyTo(flatDerivatives, NeuronsStartFlatIdx);
                }
                return activations;
            }

        }//Layer

    }//FeedForwardNetwork

}//Namespace
