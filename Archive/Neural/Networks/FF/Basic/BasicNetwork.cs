using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OKOSW.Extensions;
using OKOSW.Neural.Activation;
using OKOSW.MathTools;
using OKOSW.Queue;

namespace OKOSW.Neural.Networks.FF.Basic
{
    [Serializable]
    public sealed class BasicNetwork
    {
        //Constants
        //Dummy BIAS input value
        public const double BIAS_VALUE = 1d;
        //Default min/max weights during random initialization
        public const double WEIGHT_INI_DEFAULT_MIN = 0.001d;
        public const double WEIGHT_INI_DEFAULT_MAX = 0.01d;
        //Attributes
        private int m_inputValuesCount;
        private int m_outputValuesCount;
        private int m_nodesCount;
        private List<Layer> m_layers;
        private double[] m_flatWeights;

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
            if (m_nodesCount < 0)
            {
                //Add new layer
                Layers.Add(new Layer(nodesCount, activation));
            }
            else
            {
                throw new ApplicationException("Can´t add new layer. Network structure is finalized.");
            }
            return;
        }

        public void FinalizeStructure(IActivationFunction outputActivation)
        {
            //Add output layer
            Layers.Add(new Layer(OutputValuesCount, outputActivation));
            //Finalize layers
            int inputNodesCount = InputValuesCount;
            int nodesFlatStartIdx = 0;
            int weightsFlatStartIdx = 0;
            foreach (Layer layer in Layers)
            {
                layer.FinalizeStructure(inputNodesCount, nodesFlatStartIdx, weightsFlatStartIdx);
                nodesFlatStartIdx += layer.LayerNodesCount;
                weightsFlatStartIdx += (layer.LayerNodesCount * layer.InputNodesCount + layer.LayerNodesCount);
                inputNodesCount = layer.LayerNodesCount;
            }
            m_nodesCount = nodesFlatStartIdx;
            m_flatWeights = new double[weightsFlatStartIdx];
            return;
        }

        public void RandomizeWeights(Random rand, double weightMin = WEIGHT_INI_DEFAULT_MIN, double weightMax = WEIGHT_INI_DEFAULT_MAX)
        {
            rand.FillUniform(m_flatWeights, weightMin, weightMax);
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
                result = layer.Compute(result, m_flatWeights, null);
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

        public BasicStat CreateWeightsStat()
        {
            BasicStat weightsStat = new BasicStat(m_flatWeights);
            return weightsStat;
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

            public double[] Compute(double[] inputs, double[] flatWeights, double[] flatDerivatives)
            {
                double[] result = new double[LayerNodesCount];
                int weightFlatIdx = m_weightsStartFlatIdx;
                int nodeFlatIdx = m_nodesStartFlatIdx;
                int biasFlatIdx = m_biasesStartFlatIdx;
                for (int nodeIdx = 0; nodeIdx < LayerNodesCount; nodeIdx++, nodeFlatIdx++, biasFlatIdx++)
                {
                    double sum = flatWeights[biasFlatIdx] * BasicNetwork.BIAS_VALUE;
                    for (int inputIdx = 0; inputIdx < InputNodesCount; inputIdx++, weightFlatIdx++)
                    {
                        sum += flatWeights[weightFlatIdx] * inputs[inputIdx];
                    }
                    result[nodeIdx] = Activation.Compute(sum);
                    if (flatDerivatives != null)
                    {
                        flatDerivatives[nodeFlatIdx] = Activation.ComputeDerivative(result[nodeIdx]);
                    }
                }
                return result;
            }
        }//BasicNetworkLayer
    }//BasicNetwork

    public class RPropTrainer
    {
        //Attributes
        private RPropParameters m_parameters;
        private BasicNetwork m_net;
        private List<double[]> m_trainInputs;
        private List<double[]> m_trainIdealOutputs;
        private double[] m_weigthsPrevGradsAcc;
        private double[] m_weigthsPrevDeltas;
        private double[] m_weigthsPrevChanges;
        private BasicStat m_prevErrStat;
        private BasicStat m_lastErrStat;
        private int m_epoch;

        //Constructor
        public RPropTrainer(BasicNetwork net, List<double[]> inputs, List<double[]> outputs, RPropParameters parameters = null)
        {
            if (!net.Finalized)
            {
                throw new ApplicationException("Can´t create trainer. Network structure was not finalized.");
            }
            m_parameters = parameters;
            if (m_parameters == null)
            {
                m_parameters = new RPropParameters();
            }
            m_net = net;
            m_trainInputs = inputs;
            m_trainIdealOutputs = outputs;
            m_weigthsPrevGradsAcc = new double[m_net.FlatWeights.Length];
            m_weigthsPrevGradsAcc.Populate(0);
            m_weigthsPrevDeltas = new double[m_net.FlatWeights.Length];
            m_weigthsPrevDeltas.Populate(m_parameters.DeltaIni);
            m_weigthsPrevChanges = new double[m_net.FlatWeights.Length];
            m_weigthsPrevChanges.Populate(0);
            m_prevErrStat = new BasicStat();
            m_lastErrStat = new BasicStat();
            m_epoch = 0;
            return;
        }
        //Properties
        public BasicStat LastErrorStat { get { return m_lastErrStat; } }
        public int Epoch { get { return m_epoch; } }

        //Methods
        private double Sign(double value)
        {
            if (Math.Abs(value) <= m_parameters.ZeroTolerance)
            {
                return 0;
            }
            if (value > 0)
            {
                return 1;
            }
            return -1;
        }

        private double[] Compute(double[] input, List<double[]> layersInputs, double[] derivatives)
        {
            double[] result = input;
            foreach (BasicNetwork.Layer layer in m_net.Layers)
            {
                layersInputs.Add(result);
                result = layer.Compute(result, m_net.FlatWeights, derivatives);
            }
            return result;
        }

        /// <summary>
        /// iRPROP+ variant.
        /// </summary>
        private double CalculateWeightChange(int weightFlatIndex, double[] weigthsGradsAcc)
        {
            double weightChange = 0;
            double delta = 0;
            double gradSign = Sign(m_weigthsPrevGradsAcc[weightFlatIndex] * weigthsGradsAcc[weightFlatIndex]);
            if (gradSign > 0)
            {
                //No sign change, increase delta
                delta = m_weigthsPrevDeltas[weightFlatIndex] * m_parameters.PositiveEta;
                if (delta > m_parameters.DeltaMax) delta = m_parameters.DeltaMax;
                m_weigthsPrevDeltas[weightFlatIndex] = delta;
                weightChange = Sign(weigthsGradsAcc[weightFlatIndex]) * delta;
            }
            else if (gradSign < 0)
            {
                //Grad changed sign, decrease delta (not used, but saved for later)
                delta = m_weigthsPrevDeltas[weightFlatIndex] * m_parameters.NegativeEta;
                if (delta < m_parameters.DeltaMin) delta = m_parameters.DeltaMin;
                m_weigthsPrevDeltas[weightFlatIndex] = delta;
                if (m_lastErrStat.MeanSquare > m_prevErrStat.MeanSquare)
                {
                    weightChange = -m_weigthsPrevChanges[weightFlatIndex];
                }
                //Forces no adjustment in next iteration
                weigthsGradsAcc[weightFlatIndex] = 0;
            }
            else
            {
                //No change to delta
                delta = m_weigthsPrevDeltas[weightFlatIndex];
                weightChange = Sign(weigthsGradsAcc[weightFlatIndex]) * delta;
            }
            return weightChange;
        }

        public void Iteration()
        {
            //----------------------------------------------------
            //Next epoch
            ++m_epoch;
            //Store previous iteration error statistics
            m_prevErrStat.Adopt(m_lastErrStat);
            //Reset iteration error statistics
            m_lastErrStat.Reset();
            //Accumulated weights gradients over all training data
            double[] weigthsGradsAcc = new double[m_net.FlatWeights.Length];
            weigthsGradsAcc.Populate(0);
            //Gradients accumulation queue
            SimpleQueue<double[]> weightsGradsAccQueue = new SimpleQueue<double[]>(m_trainInputs.Count);
            bool accumulatorThreadFinished = false;
            //Start weights gradients accumulator thread
            (new Thread(() =>
            {
                while (!weightsGradsAccQueue.FeedingStopped)
                {
                    while (weightsGradsAccQueue.Count > 0)
                    {
                        //Parallel.For(0, weightsGradsAccQueue.Count, dummyIdx =>
                        //{
                            double[] data = weightsGradsAccQueue.Dequeue();
                            for (int i = 0; i < data.Length; i++)
                            {
                                weigthsGradsAcc[i] += data[i];
                            }
                        //});
                    }
                    Thread.Sleep(1);
                };
                accumulatorThreadFinished = true;
            })).Start();

            //Go paralelly through all samples
            Parallel.For(0, m_trainInputs.Count, row =>
            {
                //----------------------------------------------------
                //Net computation (store layers inputs and derivatives)
                double[] derivatives = new double[m_net.NodesCount];
                List<double[]> layersInputs = new List<double[]>(m_net.Layers.Count);
                double[] computedOutputs = Compute(m_trainInputs[row], layersInputs, derivatives);
                //----------------------------------------------------
                //Compute network nodes gradients
                double[] gradients = new double[m_net.NodesCount];
                //Output layer gradients and update of last error statistics
                BasicNetwork.Layer outputLayer = m_net.Layers.Last();
                int outputLayerNodeFlatIdx = outputLayer.NodesStartFlatIdx; ;
                for (int nodeIdx = 0; nodeIdx < outputLayer.LayerNodesCount; nodeIdx++, outputLayerNodeFlatIdx++)
                {
                    double error = m_trainIdealOutputs[row][nodeIdx] - computedOutputs[nodeIdx];
                    gradients[outputLayerNodeFlatIdx] = derivatives[outputLayerNodeFlatIdx] * error;
                    m_lastErrStat.AddSampleValue(Math.Abs(error));
                }
                //Hidden layers gradients
                for (int layerIdx = m_net.Layers.Count - 2; layerIdx >= 0; layerIdx--)
                {
                    BasicNetwork.Layer currLayer = m_net.Layers[layerIdx];
                    BasicNetwork.Layer nextLayer = m_net.Layers[layerIdx + 1];
                    int currentLayerNodeFlatIdx = currLayer.NodesStartFlatIdx;
                    for (int currLayerNodeIdx = 0; currLayerNodeIdx < currLayer.LayerNodesCount; currLayerNodeIdx++, currentLayerNodeFlatIdx++)
                    {
                        double sum = 0;
                        for (int nextLayerNodeIdx = 0; nextLayerNodeIdx < nextLayer.LayerNodesCount; nextLayerNodeIdx++)
                        {
                            int nextLayerWeightFlatIdx = nextLayer.WeightsStartFlatIdx + nextLayerNodeIdx * nextLayer.InputNodesCount + currLayerNodeIdx;
                            sum += gradients[nextLayer.NodesStartFlatIdx + nextLayerNodeIdx] * m_net.FlatWeights[nextLayerWeightFlatIdx];
                        }
                        gradients[currentLayerNodeFlatIdx] = derivatives[currentLayerNodeFlatIdx] * sum;
                    }
                }
                //----------------------------------------------------
                //Compute data for weights and biases gradients accumulator thread
                double[] weigthsGradsAccInput = new double[m_net.FlatWeights.Length];
                for (int layerIdx = 0; layerIdx < m_net.Layers.Count; layerIdx++)
                {
                    BasicNetwork.Layer layer = m_net.Layers[layerIdx];
                    double[] layerInputs = layersInputs[layerIdx];
                    int nodeFlatIdx = layer.NodesStartFlatIdx;
                    int biasFlatIdx = layer.BiasesStartFlatIdx;
                    int weightFlatIdx = layer.WeightsStartFlatIdx;
                    for (int nodeIdx = 0; nodeIdx < layer.LayerNodesCount; nodeIdx++, nodeFlatIdx++, biasFlatIdx++)
                    {
                        //Weights gradients accumulation
                        for (int inputIdx = 0; inputIdx < layer.InputNodesCount; inputIdx++, weightFlatIdx++)
                        {
                            weigthsGradsAccInput[weightFlatIdx] = layerInputs[inputIdx] * gradients[nodeFlatIdx];
                        }
                        //Bias gradients accumulation
                        weigthsGradsAccInput[biasFlatIdx] = gradients[nodeFlatIdx] * BasicNetwork.BIAS_VALUE;
                    }
                }
                //Engueue data for accumulator
                weightsGradsAccQueue.Enqueue(weigthsGradsAccInput);
            }); //Parallel.For
            //----------------------------------------------------
            //Signal of stopped feeding and waiting until accumulator thread is completed
            weightsGradsAccQueue.FeedingStopped = true;
            while(!accumulatorThreadFinished)
            {
                Thread.Sleep(1);
            }
            //----------------------------------------------------
            //Update all weights and biases
            foreach (BasicNetwork.Layer layer in m_net.Layers)
            {
                double weightChange = 0;
                int biasFlatIdx = layer.BiasesStartFlatIdx;
                int weightFlatIdx = layer.WeightsStartFlatIdx;
                for (int nodeIdx = 0; nodeIdx < layer.LayerNodesCount; nodeIdx++, biasFlatIdx++)
                {
                    //Weights
                    for (int inputIdx = 0; inputIdx < layer.InputNodesCount; inputIdx++, weightFlatIdx++)
                    {
                        weightChange = CalculateWeightChange(weightFlatIdx, weigthsGradsAcc);
                        m_net.FlatWeights[weightFlatIdx] += weightChange;
                        m_weigthsPrevChanges[weightFlatIdx] = weightChange;
                    }
                    //Bias
                    weightChange = CalculateWeightChange(biasFlatIdx, weigthsGradsAcc);
                    m_net.FlatWeights[biasFlatIdx] += weightChange;
                    m_weigthsPrevChanges[biasFlatIdx] = weightChange;
                }
            }
            //----------------------------------------------------
            //Store accumulated weights gradients
            weigthsGradsAcc.CopyTo(m_weigthsPrevGradsAcc, 0);
            //Finished
            return;
        }


        [Serializable]
        public class RPropParameters
        {
            //Constants
            public const double DEFAULT_ZERO_TOLERANCE = 1E-17d;
            public const double DEFAULT_POSITIVE_ETA = 1.2d;
            public const double DEFAULT_NEGATIVE_ETA = 0.5d;
            public const double DEFAULT_DELTA_INI = 0.01d;
            public const double DEFAULT_DELTA_MIN = 1E-6d;
            public const double DEFAULT_DELTA_MAX = 50d;

            //Attributes
            public double ZeroTolerance { get; set; } = DEFAULT_ZERO_TOLERANCE;
            public double PositiveEta { get; set; } = DEFAULT_POSITIVE_ETA;
            public double NegativeEta { get; set; } = DEFAULT_NEGATIVE_ETA;
            public double DeltaIni { get; set; } = DEFAULT_DELTA_INI;
            public double DeltaMin { get; set; } = DEFAULT_DELTA_MIN;
            public double DeltaMax { get; set; } = DEFAULT_DELTA_MAX;
        }//RPropParameters

    }//RPropTrainer













    public class BasicNetworkOrg
    {
        private int m_numInput; // number input nodes
        private int m_numHidden;
        private int m_numOutput;

        private double[] m_inputs;
        private double[][] m_ihWeights; // input-hidden
        private double[] m_hBiases;
        private double[] m_hOutputs;

        private double[][] m_hoWeights; // hidden-output
        private double[] m_oBiases;
        private double[] m_outputs;

        private Random m_rand;

        public BasicNetworkOrg(int numInput, int numHidden, int numOutput, int randomizerSeek = -1)
        {
            m_numInput = numInput;
            m_numHidden = numHidden;
            m_numOutput = numOutput;

            m_inputs = new double[numInput];

            m_ihWeights = MakeMatrix(numInput, numHidden, 0.0);
            m_hBiases = new double[numHidden];
            m_hOutputs = new double[numHidden];

            m_hoWeights = MakeMatrix(numHidden, numOutput, 0.0);
            m_oBiases = new double[numOutput];
            m_outputs = new double[numOutput];

            if (randomizerSeek == -1)
            {
                m_rand = new Random();
            }
            else
            {
                m_rand = new Random(randomizerSeek);
            }
            InitializeWeights(); // all weights and biases
        } // ctor

        private static double[][] MakeMatrix(int rows, int cols, double v) // helper for ctor, Train
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = v;
            return result;
        }

        private static double[] MakeVector(int len, double v) // helper for Train
        {
            double[] result = new double[len];
            for (int i = 0; i < len; ++i)
                result[i] = v;
            return result;
        }

        private void InitializeWeights()
        {
            // initialize weights and biases to random values between 0.0001 and 0.001
            int numWeights = (m_numInput * m_numHidden) + (m_numHidden * m_numOutput) + m_numHidden + m_numOutput;
            double[] initialWeights = new double[numWeights];
            for (int i = 0; i < initialWeights.Length; ++i)
                initialWeights[i] = (0.001 - 0.0001) * m_rand.NextDouble() + 0.0001;
            SetWeights(initialWeights);
            return;
        }

        public double[] TrainRPROP(double[][] trainData, int maxEpochs) // using RPROP
        {
            // there is an accumulated gradient and a previous gradient for each weight and bias
            double[] hGradTerms = new double[m_numHidden]; // intermediate val for h-o weight gradients
            double[] oGradTerms = new double[m_numOutput]; // output gradients

            double[][] hoWeightGradsAcc = MakeMatrix(m_numHidden, m_numOutput, 0.0); // accumulated over all training data
            double[][] ihWeightGradsAcc = MakeMatrix(m_numInput, m_numHidden, 0.0);
            double[] oBiasGradsAcc = new double[m_numOutput];
            double[] hBiasGradsAcc = new double[m_numHidden];

            double[][] hoPrevWeightGradsAcc = MakeMatrix(m_numHidden, m_numOutput, 0.0); // accumulated, previous iteration
            double[][] ihPrevWeightGradsAcc = MakeMatrix(m_numInput, m_numHidden, 0.0);
            double[] oPrevBiasGradsAcc = new double[m_numOutput];
            double[] hPrevBiasGradsAcc = new double[m_numHidden];

            // must save previous weight deltas
            double[][] hoPrevWeightDeltas = MakeMatrix(m_numHidden, m_numOutput, 0.01);
            double[][] ihPrevWeightDeltas = MakeMatrix(m_numInput, m_numHidden, 0.01);
            double[] oPrevBiasDeltas = MakeVector(m_numOutput, 0.01);
            double[] hPrevBiasDeltas = MakeVector(m_numHidden, 0.01);

            double etaPlus = 1.2; // values are from the paper
            double etaMinus = 0.5;
            double deltaMax = 50.0;
            double deltaMin = 1.0E-6;

            int epoch = 0;
            while (epoch < maxEpochs)
            {
                ++epoch;

                if (epoch % 100 == 0)
                {
                    double[] currWts = GetWeights();
                    double rmse = Math.Sqrt(MeanSquaredError(trainData, currWts));
                    Console.Write("Epoch: " + epoch + " RMSE: " + rmse.ToString() + "\r");
                }

                // 1. compute and accumulate all gradients
                hoWeightGradsAcc.Populate(0); // zero-out values from prev iteration
                ihWeightGradsAcc.Populate(0);
                oBiasGradsAcc.Populate(0);
                hBiasGradsAcc.Populate(0);

                double[] xValues = new double[m_numInput]; // inputs
                double[] tValues = new double[m_numOutput]; // target values
                for (int row = 0; row < trainData.Length; ++row)  // walk thru all training data
                {
                    // no need to visit in random order because all rows processed before any updates ('batch')
                    Array.Copy(trainData[row], xValues, m_numInput); // get the inputs
                    Array.Copy(trainData[row], m_numInput, tValues, 0, m_numOutput); // get the target values
                    ComputeOutputs(xValues); // copy xValues in, compute outputs using curr weights (and store outputs internally)

                    // compute the h-o gradient term/component as in regular back-prop
                    // this term usually is lower case Greek delta but there are too many other deltas below
                    for (int i = 0; i < m_numOutput; ++i)
                    {
                        double derivative = (1 - m_outputs[i]) * (1 + m_outputs[i]); // derivative of tanh = (1 - y) * (1 + y)
                        //double derivative = (1 - m_outputs[i]) * m_outputs[i]; // derivative of softmax = (1 - y) * y (same as log-sigmoid)
                        oGradTerms[i] = derivative * (m_outputs[i] - tValues[i]); // careful with O-T vs. T-O, O-T is the most usual
                    }

                    // compute the i-h gradient term/component as in regular back-prop
                    for (int i = 0; i < m_numHidden; ++i)
                    {
                        double derivative = (1 - m_hOutputs[i]) * (1 + m_hOutputs[i]); // derivative of tanh = (1 - y) * (1 + y)
                        double sum = 0.0;
                        for (int j = 0; j < m_numOutput; ++j) // each hidden delta is the sum of numOutput terms
                        {
                            double x = oGradTerms[j] * m_hoWeights[i][j];
                            sum += x;
                        }
                        hGradTerms[i] = derivative * sum;
                    }

                    // add input to h-o component to make h-o weight gradients, and accumulate
                    for (int i = 0; i < m_numHidden; ++i)
                    {
                        for (int j = 0; j < m_numOutput; ++j)
                        {
                            double grad = oGradTerms[j] * m_hOutputs[i];
                            hoWeightGradsAcc[i][j] += grad;
                        }
                    }

                    // the (hidden-to-) output bias gradients
                    for (int i = 0; i < m_numOutput; ++i)
                    {
                        double grad = oGradTerms[i] * 1.0; // dummy input
                        oBiasGradsAcc[i] += grad;
                    }

                    // add input term to i-h component to make i-h weight gradients and accumulate
                    for (int i = 0; i < m_numInput; ++i)
                    {
                        for (int j = 0; j < m_numHidden; ++j)
                        {
                            double grad = hGradTerms[j] * m_inputs[i];
                            ihWeightGradsAcc[i][j] += grad;
                        }
                    }

                    // the (input-to-) hidden bias gradient
                    for (int i = 0; i < m_numHidden; ++i)
                    {
                        double grad = hGradTerms[i] * 1.0;
                        hBiasGradsAcc[i] += grad;
                    }
                } // each row
                  // end compute all gradients





                // update all weights and biases (in any order)

                // update input-hidden weights
                double delta = 0.0;

                for (int i = 0; i < m_numInput; ++i)
                {
                    for (int j = 0; j < m_numHidden; ++j)
                    {
                        if (ihPrevWeightGradsAcc[i][j] * ihWeightGradsAcc[i][j] > 0) // no sign change, increase delta
                        {
                            delta = ihPrevWeightDeltas[i][j] * etaPlus; // compute delta
                            if (delta > deltaMax) delta = deltaMax; // keep it in range
                            double tmp = -Math.Sign(ihWeightGradsAcc[i][j]) * delta; // determine direction and magnitude
                            m_ihWeights[i][j] += tmp; // update weights
                        }
                        else if (ihPrevWeightGradsAcc[i][j] * ihWeightGradsAcc[i][j] < 0) // grad changed sign, decrease delta
                        {
                            delta = ihPrevWeightDeltas[i][j] * etaMinus; // the delta (not used, but saved for later)
                            if (delta < deltaMin) delta = deltaMin; // keep it in range
                            m_ihWeights[i][j] -= ihPrevWeightDeltas[i][j]; // revert to previous weight
                            ihWeightGradsAcc[i][j] = 0; // forces next if-then branch, next iteration
                        }
                        else // this happens next iteration after 2nd branch above (just had a change in gradient)
                        {
                            delta = ihPrevWeightDeltas[i][j]; // no change to delta
                                                              // no way should delta be 0 . . . 
                            double tmp = -Math.Sign(ihWeightGradsAcc[i][j]) * delta; // determine direction
                            m_ihWeights[i][j] += tmp; // update
                        }
                        //Console.WriteLine(ihPrevWeightGradsAcc[i][j] + " " + ihWeightGradsAcc[i][j]); Console.ReadLine();

                        ihPrevWeightDeltas[i][j] = delta; // save delta
                        ihPrevWeightGradsAcc[i][j] = ihWeightGradsAcc[i][j]; // save the (accumulated) gradient
                    } // j
                } // i

                // update (input-to-) hidden biases
                for (int i = 0; i < m_numHidden; ++i)
                {
                    if (hPrevBiasGradsAcc[i] * hBiasGradsAcc[i] > 0) // no sign change, increase delta
                    {
                        delta = hPrevBiasDeltas[i] * etaPlus; // compute delta
                        if (delta > deltaMax) delta = deltaMax;
                        double tmp = -Math.Sign(hBiasGradsAcc[i]) * delta; // determine direction
                        m_hBiases[i] += tmp; // update
                    }
                    else if (hPrevBiasGradsAcc[i] * hBiasGradsAcc[i] < 0) // grad changed sign, decrease delta
                    {
                        delta = hPrevBiasDeltas[i] * etaMinus; // the delta (not used, but saved later)
                        if (delta < deltaMin) delta = deltaMin;
                        m_hBiases[i] -= hPrevBiasDeltas[i]; // revert to previous weight
                        hBiasGradsAcc[i] = 0; // forces next branch, next iteration
                    }
                    else // this happens next iteration after 2nd branch above (just had a change in gradient)
                    {
                        delta = hPrevBiasDeltas[i]; // no change to delta

                        if (delta > deltaMax) delta = deltaMax;
                        else if (delta < deltaMin) delta = deltaMin;
                        // no way should delta be 0 . . . 
                        double tmp = -Math.Sign(hBiasGradsAcc[i]) * delta; // determine direction
                        m_hBiases[i] += tmp; // update
                    }
                    hPrevBiasDeltas[i] = delta;
                    hPrevBiasGradsAcc[i] = hBiasGradsAcc[i];
                }

                // update hidden-to-output weights
                for (int i = 0; i < m_numHidden; ++i)
                {
                    for (int j = 0; j < m_numOutput; ++j)
                    {
                        if (hoPrevWeightGradsAcc[i][j] * hoWeightGradsAcc[i][j] > 0) // no sign change, increase delta
                        {
                            delta = hoPrevWeightDeltas[i][j] * etaPlus; // compute delta
                            if (delta > deltaMax) delta = deltaMax;
                            double tmp = -Math.Sign(hoWeightGradsAcc[i][j]) * delta; // determine direction
                            m_hoWeights[i][j] += tmp; // update
                        }
                        else if (hoPrevWeightGradsAcc[i][j] * hoWeightGradsAcc[i][j] < 0) // grad changed sign, decrease delta
                        {
                            delta = hoPrevWeightDeltas[i][j] * etaMinus; // the delta (not used, but saved later)
                            if (delta < deltaMin) delta = deltaMin;
                            m_hoWeights[i][j] -= hoPrevWeightDeltas[i][j]; // revert to previous weight
                            hoWeightGradsAcc[i][j] = 0; // forces next branch, next iteration
                        }
                        else // this happens next iteration after 2nd branch above (just had a change in gradient)
                        {
                            delta = hoPrevWeightDeltas[i][j]; // no change to delta
                                                              // no way should delta be 0 . . . 
                            double tmp = -Math.Sign(hoWeightGradsAcc[i][j]) * delta; // determine direction
                            m_hoWeights[i][j] += tmp; // update
                        }
                        hoPrevWeightDeltas[i][j] = delta; // save delta
                        hoPrevWeightGradsAcc[i][j] = hoWeightGradsAcc[i][j]; // save the (accumulated) gradients
                    } // j
                } // i

                // update (hidden-to-) output biases
                for (int i = 0; i < m_numOutput; ++i)
                {
                    if (oPrevBiasGradsAcc[i] * oBiasGradsAcc[i] > 0) // no sign change, increase delta
                    {
                        delta = oPrevBiasDeltas[i] * etaPlus; // compute delta
                        if (delta > deltaMax) delta = deltaMax;
                        double tmp = -Math.Sign(oBiasGradsAcc[i]) * delta; // determine direction
                        m_oBiases[i] += tmp; // update
                    }
                    else if (oPrevBiasGradsAcc[i] * oBiasGradsAcc[i] < 0) // grad changed sign, decrease delta
                    {
                        delta = oPrevBiasDeltas[i] * etaMinus; // the delta (not used, but saved later)
                        if (delta < deltaMin) delta = deltaMin;
                        m_oBiases[i] -= oPrevBiasDeltas[i]; // revert to previous weight
                        oBiasGradsAcc[i] = 0; // forces next branch, next iteration
                    }
                    else // this happens next iteration after 2nd branch above (just had a change in gradient)
                    {
                        delta = oPrevBiasDeltas[i]; // no change to delta
                                                    // no way should delta be 0 . . . 
                        double tmp = -Math.Sign(hBiasGradsAcc[i]) * delta; // determine direction
                        m_oBiases[i] += tmp; // update
                    }
                    oPrevBiasDeltas[i] = delta;
                    oPrevBiasGradsAcc[i] = oBiasGradsAcc[i];
                }
            } // while

            double[] wts = GetWeights();
            return wts;
        } // Train

        public void SetWeights(double[] weights)
        {
            // copy weights and biases in weights[] array to i-h weights, i-h biases, h-o weights, h-o biases
            int numWeights = (m_numInput * m_numHidden) + (m_numHidden * m_numOutput) + m_numHidden + m_numOutput;
            if (weights.Length != numWeights)
                throw new Exception("Bad weights array in SetWeights");

            int k = 0; // points into weights param

            for (int i = 0; i < m_numInput; ++i)
                for (int j = 0; j < m_numHidden; ++j)
                    m_ihWeights[i][j] = weights[k++];
            for (int i = 0; i < m_numHidden; ++i)
                m_hBiases[i] = weights[k++];
            for (int i = 0; i < m_numHidden; ++i)
                for (int j = 0; j < m_numOutput; ++j)
                    m_hoWeights[i][j] = weights[k++];
            for (int i = 0; i < m_numOutput; ++i)
                m_oBiases[i] = weights[k++];
        }

        public double[] GetWeights()
        {
            int numWeights = (m_numInput * m_numHidden) + (m_numHidden * m_numOutput) + m_numHidden + m_numOutput;
            double[] result = new double[numWeights];
            int k = 0;
            for (int i = 0; i < m_ihWeights.Length; ++i)
                for (int j = 0; j < m_ihWeights[0].Length; ++j)
                    result[k++] = m_ihWeights[i][j];
            for (int i = 0; i < m_hBiases.Length; ++i)
                result[k++] = m_hBiases[i];
            for (int i = 0; i < m_hoWeights.Length; ++i)
                for (int j = 0; j < m_hoWeights[0].Length; ++j)
                    result[k++] = m_hoWeights[i][j];
            for (int i = 0; i < m_oBiases.Length; ++i)
                result[k++] = m_oBiases[i];
            return result;
        }

        public double[] ComputeOutputs(double[] xValues)
        {
            double[] hSums = new double[m_numHidden]; // hidden nodes sums scratch array
            double[] oSums = new double[m_numOutput]; // output nodes sums

            for (int i = 0; i < xValues.Length; ++i) // copy x-values to inputs
                m_inputs[i] = xValues[i];
            // note: no need to copy x-values unless you implement a ToString and want to see them.
            // more efficient is to simply use the xValues[] directly.

            for (int j = 0; j < m_numHidden; ++j)  // compute i-h sum of weights * inputs
                for (int i = 0; i < m_numInput; ++i)
                    hSums[j] += m_inputs[i] * m_ihWeights[i][j]; // note +=

            for (int i = 0; i < m_numHidden; ++i)  // add biases to input-to-hidden sums
                hSums[i] += m_hBiases[i];

            for (int i = 0; i < m_numHidden; ++i)   // apply activation
                m_hOutputs[i] = HyperTan(hSums[i]); // hard-coded

            for (int j = 0; j < m_numOutput; ++j)   // compute h-o sum of weights * hOutputs
                for (int i = 0; i < m_numHidden; ++i)
                    oSums[j] += m_hOutputs[i] * m_hoWeights[i][j];

            for (int i = 0; i < m_numOutput; ++i)  // add biases to input-to-hidden sums
                oSums[i] += m_oBiases[i];

            for (int i = 0; i < m_numOutput; ++i)
                m_outputs[i] = HyperTan(oSums[i]);
            /*
            double[] softOut = Softmax(oSums); // softmax activation does all outputs at once for efficiency
            Array.Copy(softOut, m_outputs, softOut.Length);
            */

            double[] retResult = new double[m_numOutput]; // could define a GetOutputs method instead
            Array.Copy(m_outputs, retResult, retResult.Length);
            return retResult;
        }

        private static double HyperTan(double x)
        {
            if (x < -20.0) return -1.0; // approximation is correct to 30 decimals
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        private static double[] Softmax(double[] oSums)
        {
            // does all output nodes at once so scale doesn't have to be re-computed each time
            // determine max output-sum
            double max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            // determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                scale += Math.Exp(oSums[i] - max);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i] - max) / scale;

            return result; // now scaled so that xi sum to 1.0
        }

        public double MeanSquaredError(double[][] trainData, double[] weights)
        {
            SetWeights(weights); // copy the weights to evaluate in

            double[] xValues = new double[m_numInput]; // inputs
            double[] tValues = new double[m_numOutput]; // targets
            double sumSquaredError = 0.0;
            for (int i = 0; i < trainData.Length; ++i) // walk through each training data item
            {
                // following assumes data has all x-values first, followed by y-values!
                Array.Copy(trainData[i], xValues, m_numInput); // extract inputs
                Array.Copy(trainData[i], m_numInput, tValues, 0, m_numOutput); // extract targets
                double[] yValues = ComputeOutputs(xValues);
                for (int j = 0; j < yValues.Length; ++j)
                    sumSquaredError += ((yValues[j] - tValues[j]) * (yValues[j] - tValues[j]));
            }
            return sumSquaredError / (trainData.Length * m_numOutput);
        }


    }//BasicNetwork

}//Namespace
