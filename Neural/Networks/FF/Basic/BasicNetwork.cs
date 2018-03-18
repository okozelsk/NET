using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OKOSW.Extensions;
using OKOSW.Neural.Activation;
using OKOSW.MathTools;

namespace OKOSW.Neural.Networks.FF.Basic
{
    [Serializable]
    public class BasicNetwork
    {
        //Constants
        //Dummy BIAS input value
        public const double BIAS_VALUE = 1d;
        //Default min/max weights for random initialization
        public const double WEIGHT_INI_DEFAULT_MIN = 0.005;
        public const double WEIGHT_INI_DEFAULT_MAX = 0.05;
        //Attributes
        private int m_inputValuesCount;
        private int m_outputValuesCount;
        private int m_nodesCount;
        private List<Layer> m_layers;
        private double[] m_flatWeights;
        private bool m_NWRandomization;

        //Constructor
        public BasicNetwork(int inputValuesCount, int outputValuesCount)
        {
            //Input/Output counts
            m_inputValuesCount = inputValuesCount;
            m_outputValuesCount = outputValuesCount;
            m_nodesCount = -1;
            //Network layers
            m_layers = new List<Layer>();
            //Flat weights
            m_flatWeights = null;
            return;
        }

        //Properties
        public bool Finalized { get { return m_nodesCount > 0; } }
        public int InputValuesCount { get { return m_inputValuesCount; } }
        public int OutputValuesCount { get { return m_outputValuesCount; } }
        public int NodesCount { get { return m_nodesCount; } }
        public List<Layer> Layers { get { return m_layers; } }
        public double[] FlatWeights { get { return m_flatWeights; } }

        //Methods
        public void AddLayer(int nodesCount, IActivationFunction activation)
        {
            if (!Finalized)
            {
                //Add new layer
                Layers.Add(new Layer(nodesCount, activation));
            }
            else
            {
                throw new Exception("Can´t add new layer. Network structure is finalized.");
            }
            return;
        }

        public void FinalizeStructure(IActivationFunction outputActivation)
        {
            if (Finalized)
            {
                throw new Exception("Network structure has been already finalized.");
            }
            //Add output layer
            m_layers.Add(new Layer(OutputValuesCount, outputActivation));
            //Finalize layers
            int inputNodesCount = InputValuesCount;
            int nodesFlatStartIdx = 0;
            int weightsFlatStartIdx = 0;
            m_NWRandomization = true;
            foreach (Layer layer in m_layers)
            {
                layer.FinalizeStructure(inputNodesCount, nodesFlatStartIdx, weightsFlatStartIdx);
                nodesFlatStartIdx += layer.LayerNodesCount;
                weightsFlatStartIdx += (layer.LayerNodesCount * layer.InputNodesCount + layer.LayerNodesCount);
                inputNodesCount = layer.LayerNodesCount;
                if(layer.Activation.GetType() != typeof(ElliotAF) &&
                   layer.Activation.GetType() != typeof(TanhAF)
                   )
                {
                    m_NWRandomization = false;
                }
            }
            if(m_layers.Count < 2)
            {
                m_NWRandomization = false;
            }
            m_nodesCount = nodesFlatStartIdx;
            m_flatWeights = new double[weightsFlatStartIdx];
            return;
        }

        private void RandomizeWeightsNW(Random rand)
        {
            foreach (Layer layer in m_layers)
            {
                int weightFlatIndex = layer.WeightsStartFlatIdx;
                int biasFlatIndex = layer.BiasesStartFlatIdx;
                double b = 0.35d * Math.Pow(layer.LayerNodesCount, (1d / layer.InputNodesCount));
                for(int layerNodeIdx = 0; layerNodeIdx < layer.LayerNodesCount; layerNodeIdx++, biasFlatIndex++)
                {
                    for (int inputNodeIdx = 0; inputNodeIdx < layer.InputNodesCount; inputNodeIdx++, weightFlatIndex++)
                    {
                        m_flatWeights[weightFlatIndex] = rand.NextBoundedUniformDouble(0, b);
                    }
                    m_flatWeights[biasFlatIndex] = rand.NextBoundedUniformDouble(-b, b);
                }
            }
            return;
        }

        public void RandomizeWeights(Random rand)
        {
            if(!Finalized)
            {
                throw new Exception("Can´t randomize weights. Network structure is not finalized.");
            }
            if (m_NWRandomization)
            {
                RandomizeWeightsNW(rand);
            }
            else
            {
                rand.FillUniformRS(m_flatWeights, WEIGHT_INI_DEFAULT_MIN, WEIGHT_INI_DEFAULT_MAX);
            }
            return;
        }

        public BasicNetwork Clone()
        {
            BasicNetwork clone = new BasicNetwork(InputValuesCount, OutputValuesCount);
            clone.m_nodesCount = m_nodesCount;
            foreach (Layer layer in Layers)
            {
                clone.Layers.Add(layer.Clone());
            }
            clone.m_flatWeights = (double[])m_flatWeights.Clone();
            return clone;
        }

        public double[] Compute(double[] input)
        {
            double[] result = input;
            foreach (Layer layer in Layers)
            {
                result = layer.Compute(result, m_flatWeights);
            }
            return result;
        }

        public BasicStat ComputeBatchErrorStat(List<double[]> inputs, List<double[]> idealOutputs)
        {
            BasicStat errStat = new BasicStat();
            Parallel.For(0, inputs.Count, row =>
            {
                double[] computedOutputs = Compute(inputs[row]);
                for (int i = 0; i < m_outputValuesCount; i++)
                {
                    double error = idealOutputs[row][i] - computedOutputs[i];
                    errStat.AddSampleValue(Math.Abs(error));
                }
            });
            return errStat;
        }

        public void SetWeights(double[] newFlatWeights)
        {
            newFlatWeights.CopyTo(m_flatWeights, 0);
            return;
        }

        public BasicStat ComputeWeightsStat()
        {
            return (new BasicStat(m_flatWeights));
        }

        //Inner classes
        [Serializable]
        public class Layer
        {
            //Attributes
            public IActivationFunction Activation { get; }
            private int m_inputNodesCount;
            private int m_layerNodesCount;
            private int m_weightsStartFlatIdx;
            private int m_biasesStartFlatIdx;
            private int m_nodesStartFlatIdx;

            //Constructor
            public Layer(int layerNodesCount, IActivationFunction activation)
            {
                Activation = activation;
                m_layerNodesCount = layerNodesCount;
                m_inputNodesCount = -1;
                m_weightsStartFlatIdx = 0;
                m_biasesStartFlatIdx = 0;
                m_nodesStartFlatIdx = 0;
                return;
            }

            //Properties
            public bool Finalized { get { return m_inputNodesCount > 0; } }
            public int InputNodesCount { get { return m_inputNodesCount; } }
            public int LayerNodesCount { get { return m_layerNodesCount; } }
            public int WeightsStartFlatIdx { get { return m_weightsStartFlatIdx; } }
            public int BiasesStartFlatIdx { get { return m_biasesStartFlatIdx; } }
            public int NodesStartFlatIdx { get { return m_nodesStartFlatIdx; } }


            //Methods
            public void FinalizeStructure(int inputNodesCount, int nodesFlatStartIdx, int weightsFlatStartIdx)
            {
                m_inputNodesCount = inputNodesCount;
                m_weightsStartFlatIdx = weightsFlatStartIdx;
                m_biasesStartFlatIdx = weightsFlatStartIdx + m_layerNodesCount * m_inputNodesCount;
                m_nodesStartFlatIdx = nodesFlatStartIdx;
                return;
            }

            public Layer Clone()
            {
                Layer clone = new Layer(LayerNodesCount, Activation);
                clone.m_inputNodesCount = m_inputNodesCount;
                clone.m_weightsStartFlatIdx = m_weightsStartFlatIdx;
                clone.m_biasesStartFlatIdx = m_biasesStartFlatIdx;
                clone.m_nodesStartFlatIdx = m_nodesStartFlatIdx;
                return clone;
            }

            public double[] Compute(double[] inputs, double[] flatWeights)
            {
                double[] result = new double[LayerNodesCount];
                int weightFlatIdx = m_weightsStartFlatIdx;
                int biasFlatIdx = m_biasesStartFlatIdx;
                for (int nodeIdx = 0; nodeIdx < LayerNodesCount; nodeIdx++, biasFlatIdx++)
                {
                    double sum = flatWeights[biasFlatIdx] * BasicNetwork.BIAS_VALUE;
                    for (int inputIdx = 0; inputIdx < InputNodesCount; inputIdx++, weightFlatIdx++)
                    {
                        sum += flatWeights[weightFlatIdx] * inputs[inputIdx];
                    }
                    result[nodeIdx] = Activation.Compute(sum);
                }
                return result;
            }

            public double[] Compute(double[] inputs, double[] flatWeights, double[] flatDerivatives)
            {
                double[] result = Compute(inputs, flatWeights);
                for (int nodeIdx = 0; nodeIdx < LayerNodesCount; nodeIdx++)
                {
                    flatDerivatives[m_nodesStartFlatIdx + nodeIdx] = Activation.ComputeDerivative(result[nodeIdx]);
                }
                return result;
            }
        }//BasicNetworkLayer
    }//BasicNetwork

}//Namespace
