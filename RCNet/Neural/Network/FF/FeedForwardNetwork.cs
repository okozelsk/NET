using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
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
        //Dummy BIAS input value
        public const double BiasValue = 1d;
        //Default min/max weights for random initialization
        public const double WeightDefaultIniMin = 0.005;
        public const double WeightDefaultIniMax = 0.05;
        //Attributes
        private int _inputValuesCount;
        private int _outputValuesCount;
        private int _nodesCount;
        private List<Layer> _layers;
        private double[] _flatWeights;
        private bool _isAllowedNguyenWidrowRandomization;

        //Constructor
        public FeedForwardNetwork(int inputValuesCount, int outputValuesCount)
        {
            //Input/Output counts
            _inputValuesCount = inputValuesCount;
            _outputValuesCount = outputValuesCount;
            _nodesCount = -1;
            //Network layers
            _layers = new List<Layer>();
            //Flat weights
            _flatWeights = null;
            //Nguyen Widrow initial randomization
            _isAllowedNguyenWidrowRandomization = false;
            return;
        }

        //Properties
        public bool Finalized { get { return _nodesCount > 0; } }
        public int InputValuesCount { get { return _inputValuesCount; } }
        public int OutputValuesCount { get { return _outputValuesCount; } }
        public int NodesCount { get { return _nodesCount; } }
        public List<Layer> Layers { get { return _layers; } }
        public double[] FlatWeights { get { return _flatWeights; } }

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
        /// Adds new hidden layer into this network
        /// </summary>
        /// <param name="numOfNodes">Number of layer's neurons</param>
        /// <param name="activation">Each of layer's neuron will be activated by this activation function</param>
        public void AddLayer(int numOfNodes, IActivationFunction activation)
        {
            if (!Finalized)
            {
                if(numOfNodes < 1)
                {
                    throw new ArgumentException($"Invalid numOfNodes parameter value {numOfNodes.ToString()}");
                }
                //Add new layer
                Layers.Add(new Layer(numOfNodes, activation));
            }
            else
            {
                throw new Exception("Can´t add new layer. Network structure is finalized.");
            }
            return;
        }

        /// <summary>
        /// Finalizes network internal structure and locks structure against further changes.
        /// </summary>
        /// <param name="outputActivation">Activation function of the output layer's neurons</param>
        public void FinalizeStructure(IActivationFunction outputActivation)
        {
            if (Finalized)
            {
                throw new Exception("Network structure has been already finalized.");
            }
            //Add output layer
            _layers.Add(new Layer(OutputValuesCount, outputActivation));
            //Finalize layers
            int inputNodesCount = InputValuesCount;
            int nodesFlatStartIdx = 0;
            int weightsFlatStartIdx = 0;
            _isAllowedNguyenWidrowRandomization = true;
            foreach (Layer layer in _layers)
            {
                layer.FinalizeStructure(inputNodesCount, nodesFlatStartIdx, weightsFlatStartIdx);
                nodesFlatStartIdx += layer.NumOfLayerNodes;
                weightsFlatStartIdx += (layer.NumOfLayerNodes * layer.NumOfInputNodes + layer.NumOfLayerNodes);
                inputNodesCount = layer.NumOfLayerNodes;
                if(layer.Activation.GetType() != typeof(ElliotAF) &&
                   layer.Activation.GetType() != typeof(TanhAF)
                   )
                {
                    _isAllowedNguyenWidrowRandomization = false;
                }
            }
            if(_layers.Count < 2)
            {
                _isAllowedNguyenWidrowRandomization = false;
            }
            _nodesCount = nodesFlatStartIdx;
            _flatWeights = new double[weightsFlatStartIdx];
            return;
        }

        /// <summary>
        /// Applies Nguyen Widrow weights randomization method.
        /// </summary>
        /// <param name="rand">Random generator to be used</param>
        private void RandomizeWeightsByNguyenWidrowMethod(Random rand)
        {
            foreach (Layer layer in _layers)
            {
                int weightFlatIndex = layer.WeightsStartFlatIdx;
                int biasFlatIndex = layer.BiasesStartFlatIdx;
                double b = 0.35d * Math.Pow(layer.NumOfLayerNodes, (1d / layer.NumOfInputNodes));
                for(int layerNodeIdx = 0; layerNodeIdx < layer.NumOfLayerNodes; layerNodeIdx++, biasFlatIndex++)
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
        /// Randomizes network's weights (must be called before training)
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
            FeedForwardNetwork clone = new FeedForwardNetwork(InputValuesCount, OutputValuesCount);
            clone._nodesCount = _nodesCount;
            foreach (Layer layer in Layers)
            {
                clone.Layers.Add(layer.Clone());
            }
            clone._flatWeights = (double[])_flatWeights.Clone();
            return clone;
        }

        /// <summary>
        /// Computes network output values
        /// </summary>
        /// <param name="input">Input values to be passed into the network</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] input)
        {
            double[] result = input;
            foreach (Layer layer in Layers)
            {
                result = layer.Compute(result, _flatWeights);
            }
            return result;
        }

        /// <summary>
        /// Function goes through collection (batch) of inputs and for each of them computes the output.
        /// Computed output is then compared to corresponding ideal output.
        /// The error Abs(ideal - computed) is passed to overall error statistics.
        /// </summary>
        /// <param name="inputs">Batch inputs</param>
        /// <param name="idealOutputs">Batch ideal outputs</param>
        /// <returns>Error statistics</returns>
        public BasicStat ComputeBatchErrorStat(List<double[]> inputs, List<double[]> idealOutputs)
        {
            BasicStat errStat = new BasicStat();
            Parallel.For(0, inputs.Count, row =>
            {
                double[] computedOutputs = Compute(inputs[row]);
                for (int i = 0; i < _outputValuesCount; i++)
                {
                    double error = idealOutputs[row][i] - computedOutputs[i];
                    errStat.AddSampleValue(Math.Abs(error));
                }
            });
            return errStat;
        }

        /// <summary>
        /// Adopts the given weights.
        /// </summary>
        /// <param name="newFlatWeights">Weights to be adopted (in flat format)</param>
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
        /// Feed forward network's layer
        /// </summary>
        [Serializable]
        public class Layer
        {
            //Attribute properties
            public IActivationFunction Activation { get; }
            //Attributes
            private int _numOfInputNodes;
            private int _numOfLayerNodes;
            private int _weightsStartFlatIdx;
            private int _biasesStartFlatIdx;
            private int _nodesStartFlatIdx;

            //Constructor
            /// <summary>
            /// Instantiates the layer
            /// </summary>
            /// <param name="numOfNodes">Number of layer's neurons</param>
            /// <param name="activation">Each of layer's neuron will be activated by this activation function</param>
            public Layer(int numOfNodes, IActivationFunction activation)
            {
                Activation = activation;
                _numOfLayerNodes = numOfNodes;
                _numOfInputNodes = -1;
                _weightsStartFlatIdx = 0;
                _biasesStartFlatIdx = 0;
                _nodesStartFlatIdx = 0;
                return;
            }

            //Properties
            public bool Finalized { get { return _numOfInputNodes > 0; } }
            public int NumOfInputNodes { get { return _numOfInputNodes; } }
            public int NumOfLayerNodes { get { return _numOfLayerNodes; } }
            public int WeightsStartFlatIdx { get { return _weightsStartFlatIdx; } }
            public int BiasesStartFlatIdx { get { return _biasesStartFlatIdx; } }
            public int NodesStartFlatIdx { get { return _nodesStartFlatIdx; } }


            //Methods
            /// <summary>
            /// Finalizes the layer structure
            /// </summary>
            /// <param name="numOfInputNodes">Number of input nodes</param>
            /// <param name="nodesFlatStartIdx">Where starts this layer nodes in a flat structure</param>
            /// <param name="weightsFlatStartIdx">Where starts this layer weights in a flat structure</param>
            public void FinalizeStructure(int numOfInputNodes, int nodesFlatStartIdx, int weightsFlatStartIdx)
            {
                _numOfInputNodes = numOfInputNodes;
                _weightsStartFlatIdx = weightsFlatStartIdx;
                _biasesStartFlatIdx = weightsFlatStartIdx + _numOfLayerNodes * _numOfInputNodes;
                _nodesStartFlatIdx = nodesFlatStartIdx;
                return;
            }

            /// <summary>
            /// Creates a copy of this layer
            /// </summary>
            public Layer Clone()
            {
                Layer clone = new Layer(_numOfLayerNodes, Activation);
                clone._numOfInputNodes = _numOfInputNodes;
                clone._weightsStartFlatIdx = _weightsStartFlatIdx;
                clone._biasesStartFlatIdx = _biasesStartFlatIdx;
                clone._nodesStartFlatIdx = _nodesStartFlatIdx;
                return clone;
            }

            /// <summary>
            /// Computes state of each layer's neuron
            /// </summary>
            /// <param name="inputs">Input values for this layer</param>
            /// <param name="flatWeights">Network's flat weights structure</param>
            /// <returns>Layer's neurons values (states)</returns>
            public double[] Compute(double[] inputs, double[] flatWeights)
            {
                double[] result = new double[NumOfLayerNodes];
                int weightFlatIdx = _weightsStartFlatIdx;
                int biasFlatIdx = _biasesStartFlatIdx;
                for (int nodeIdx = 0; nodeIdx < NumOfLayerNodes; nodeIdx++, biasFlatIdx++)
                {
                    double sum = flatWeights[biasFlatIdx] * FeedForwardNetwork.BiasValue;
                    for (int inputIdx = 0; inputIdx < NumOfInputNodes; inputIdx++, weightFlatIdx++)
                    {
                        sum += flatWeights[weightFlatIdx] * inputs[inputIdx];
                    }
                    result[nodeIdx] = Activation.Compute(sum);
                }
                return result;
            }

            /// <summary>
            /// Computes state of each layer's neuron and its derivative
            /// </summary>
            /// <param name="inputs">Input values for this layer</param>
            /// <param name="flatWeights">Network's flat weights structure</param>
            /// <param name="flatDerivatives">Derivatives of computed states</param>
            /// <returns>Layer's neurons values (states)</returns>
            public double[] Compute(double[] inputs, double[] flatWeights, double[] flatDerivatives)
            {
                double[] result = Compute(inputs, flatWeights);
                for (int nodeIdx = 0; nodeIdx < NumOfLayerNodes; nodeIdx++)
                {
                    flatDerivatives[_nodesStartFlatIdx + nodeIdx] = Activation.ComputeDerivative(result[nodeIdx], inputs[nodeIdx]);
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
    public sealed class HiddenLayerSettings
    {
        //Attributes
        public int NeuronsCount { get; set; }
        public ActivationFactory.ActivationType ActivationType { get; set; }

        //Constructors
        public HiddenLayerSettings(int neuronsCount, ActivationFactory.ActivationType activationType)
        {
            NeuronsCount = neuronsCount;
            ActivationType = activationType;
            return;
        }

        public HiddenLayerSettings(HiddenLayerSettings source)
        {
            NeuronsCount = source.NeuronsCount;
            ActivationType = source.ActivationType;
            return;
        }

        public HiddenLayerSettings(XElement hiddenLayerElem)
        {
            NeuronsCount = int.Parse(hiddenLayerElem.Attribute("Neurons").Value);
            ActivationType = ActivationFactory.ParseActivation(hiddenLayerElem.Attribute("Activation").Value);
            return;
        }

        //Methods
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            HiddenLayerSettings cmpSettings = obj as HiddenLayerSettings;
            if (NeuronsCount != cmpSettings.NeuronsCount || ActivationType != cmpSettings.ActivationType)
            {
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return NeuronsCount.GetHashCode();
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
