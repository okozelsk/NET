using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using OKOSW.Extensions;

namespace OKOSW.Neural.Networks.FF.Basic
{
    [Serializable]
    public class RPropParameters
    {
        //Constants
        public const double DEFAULT_ZERO_TOLERANCE = 1E-17d;
        public const double DEFAULT_POSITIVE_ETA = 1.2d;
        public const double DEFAULT_NEGATIVE_ETA = 0.5d;
        public const double DEFAULT_DELTA_INI = 0.1d;
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

    [Serializable]
    public class RPropTrainer : IBasicTrainer
    {
        //Attributes
        private RPropParameters m_parameters;
        private BasicNetwork m_net;
        private List<double[]> m_trainInputs;
        private List<double[]> m_trainIdealOutputs;
        private double[] m_weigthsGradsAcc;
        private double[] m_weigthsPrevGradsAcc;
        private double[] m_weigthsPrevDeltas;
        private double[] m_weigthsPrevChanges;
        private double m_prevMSE;
        private double m_lastMSE;
        private int m_epoch;
        private List<WorkerRange> m_workersRanges;

        //Constructor
        public RPropTrainer(BasicNetwork net, List<double[]> inputs, List<double[]> outputs, RPropParameters parameters = null)
        {
            if (!net.Finalized)
            {
                throw new Exception("Can´t create trainer. Network structure was not finalized.");
            }
            m_parameters = parameters;
            if (m_parameters == null)
            {
                //Default parameters
                m_parameters = new RPropParameters();
            }
            m_net = net;
            m_trainInputs = inputs;
            m_trainIdealOutputs = outputs;
            m_weigthsGradsAcc = new double[m_net.FlatWeights.Length];
            m_weigthsGradsAcc.Populate(0);
            m_weigthsPrevGradsAcc = new double[m_net.FlatWeights.Length];
            m_weigthsPrevGradsAcc.Populate(0);
            m_weigthsPrevDeltas = new double[m_net.FlatWeights.Length];
            m_weigthsPrevDeltas.Populate(m_parameters.DeltaIni);
            m_weigthsPrevChanges = new double[m_net.FlatWeights.Length];
            m_weigthsPrevChanges.Populate(0);
            m_prevMSE = 0;
            m_lastMSE = 0;
            m_epoch = 0;
            //Parallel gradient workers count and batch ranges preparation
            m_workersRanges = new List<WorkerRange>();
            int workersCount = Math.Min(Environment.ProcessorCount, m_trainInputs.Count);
            workersCount = Math.Max(1, workersCount);
            int workerBatchSize = m_trainInputs.Count / workersCount;
            for (int workerIdx = 0, fromRow = 0; workerIdx < workersCount; workerIdx++, fromRow += workerBatchSize)
            {
                WorkerRange workerRange = new WorkerRange();
                workerRange.FromRow = fromRow;
                if (workerIdx == workersCount - 1)
                {
                    workerRange.ToRow = m_trainInputs.Count - 1;
                }
                else
                {
                    workerRange.ToRow = (fromRow + workerBatchSize) - 1;
                }
                m_workersRanges.Add(workerRange);
            }
            return;
        }

        //Properties
        /// <summary>
        /// !Epoch-1 error (MSE).
        /// </summary>
        public double MSE { get { return m_lastMSE; } }
        /// <summary>
        /// Current epoch (incemented by call of Iteration)
        /// </summary>
        public int Epoch { get { return m_epoch; } }
        /// <summary>
        /// Training FF BasicNetwork
        /// </summary>
        public BasicNetwork Net { get { return m_net; } }

        //Methods

        /// <summary>
        /// Decreases zero recognition precision.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        /// <summary>
        /// iRPROP+ variant of weight update.
        /// </summary>
        private void RPROPWeightChange(int weightFlatIndex)
        {
            double weightChange = 0;
            double delta = 0;
            double gradSign = Sign(m_weigthsPrevGradsAcc[weightFlatIndex] * m_weigthsGradsAcc[weightFlatIndex]);
            if (gradSign > 0)
            {
                //No sign change, increase delta
                delta = m_weigthsPrevDeltas[weightFlatIndex] * m_parameters.PositiveEta;
                if (delta > m_parameters.DeltaMax) delta = m_parameters.DeltaMax;
                m_weigthsPrevDeltas[weightFlatIndex] = delta;
                weightChange = Sign(m_weigthsGradsAcc[weightFlatIndex]) * delta;
            }
            else if (gradSign < 0)
            {
                //Grad changed sign, decrease delta
                delta = m_weigthsPrevDeltas[weightFlatIndex] * m_parameters.NegativeEta;
                if (delta < m_parameters.DeltaMin) delta = m_parameters.DeltaMin;
                m_weigthsPrevDeltas[weightFlatIndex] = delta;
                if (m_lastMSE > m_prevMSE)
                {
                    weightChange = -m_weigthsPrevChanges[weightFlatIndex];
                }
                //Force no adjustments in next iteration
                m_weigthsGradsAcc[weightFlatIndex] = 0;
            }
            else
            {
                //No change to delta
                delta = m_weigthsPrevDeltas[weightFlatIndex];
                weightChange = Sign(m_weigthsGradsAcc[weightFlatIndex]) * delta;
            }
            m_net.FlatWeights[weightFlatIndex] += weightChange;
            m_weigthsPrevChanges[weightFlatIndex] = weightChange;
            return;
        }

        /// <summary>
        /// Network computation for training purposes.
        /// </summary>
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

        private void ProcessGradientWorkerOutputs(double[] weigthsGradsAccInput, double sumOfErrorPowers)
        {
            lock (m_weigthsGradsAcc)
            {
                for (int i = 0; i < weigthsGradsAccInput.Length; i++)
                {
                    m_weigthsGradsAcc[i] += weigthsGradsAccInput[i];
                    m_lastMSE += sumOfErrorPowers;
                }
            }
            return;
        }

        /// <summary>
        /// Performs training iteration.
        /// </summary>
        public void Iteration()
        {
            //----------------------------------------------------
            //Next epoch
            ++m_epoch;
            //Store previous iteration error
            m_prevMSE = m_lastMSE;
            //Reset iteration error
            m_lastMSE = 0;
            //----------------------------------------------------
            //Accumulated weights gradients over all training data
            m_weigthsGradsAcc.Populate(0);
            //Process gradient workers threads
            Parallel.ForEach(m_workersRanges, range =>
            {
                //----------------------------------------------------
                //Gradient worker variables
                double sumOfErrPowers = 0;
                double[] weigthsGradsAccInput = new double[m_net.FlatWeights.Length];
                weigthsGradsAccInput.Populate(0);
                double[] nodesGradients = new double[m_net.NodesCount];
                double[] derivatives = new double[m_net.NodesCount];
                List<double[]> layersInputs = new List<double[]>(m_net.Layers.Count);
                //----------------------------------------------------
                //Go paralelly through all samples
                for (int row = range.FromRow; row <= range.ToRow; row++)
                {
                    //----------------------------------------------------
                    //Network computation (collect layers inputs and derivatives)
                    layersInputs.Clear();
                    double[] computedOutputs = Compute(m_trainInputs[row], layersInputs, derivatives);
                    //----------------------------------------------------
                    //Compute network nodes gradients
                    //Compute output layer gradients and update last error statistics
                    BasicNetwork.Layer outputLayer = m_net.Layers[m_net.Layers.Count - 1];
                    for (int nodeIdx = 0, outputLayerNodeFlatIdx = outputLayer.NodesStartFlatIdx; nodeIdx < outputLayer.LayerNodesCount; nodeIdx++, outputLayerNodeFlatIdx++)
                    {
                        double error = m_trainIdealOutputs[row][nodeIdx] - computedOutputs[nodeIdx];
                        nodesGradients[outputLayerNodeFlatIdx] = derivatives[outputLayerNodeFlatIdx] * error;
                        //Accumulate power of error
                        sumOfErrPowers += error * error;
                    }
                    //Hidden layers gradients
                    for (int layerIdx = m_net.Layers.Count - 2; layerIdx >= 0; layerIdx--)
                    {
                        BasicNetwork.Layer currLayer = m_net.Layers[layerIdx];
                        BasicNetwork.Layer nextLayer = m_net.Layers[layerIdx + 1];
                        int currLayerNodeFlatIdx = currLayer.NodesStartFlatIdx;
                        for (int currLayerNodeIdx = 0; currLayerNodeIdx < currLayer.LayerNodesCount; currLayerNodeIdx++, currLayerNodeFlatIdx++)
                        {
                            double sum = 0;
                            for (int nextLayerNodeIdx = 0; nextLayerNodeIdx < nextLayer.LayerNodesCount; nextLayerNodeIdx++)
                            {
                                int nextLayerWeightFlatIdx = nextLayer.WeightsStartFlatIdx + nextLayerNodeIdx * nextLayer.InputNodesCount + currLayerNodeIdx;
                                sum += nodesGradients[nextLayer.NodesStartFlatIdx + nextLayerNodeIdx] * m_net.FlatWeights[nextLayerWeightFlatIdx];
                            }
                            nodesGradients[currLayerNodeFlatIdx] = derivatives[currLayerNodeFlatIdx] * sum;
                        }
                    }
                    //----------------------------------------------------
                    //Compute increments for gradients accumulator
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
                                weigthsGradsAccInput[weightFlatIdx] += layerInputs[inputIdx] * nodesGradients[nodeFlatIdx];
                            }
                            //Bias gradients accumulation
                            weigthsGradsAccInput[biasFlatIdx] += nodesGradients[nodeFlatIdx] * BasicNetwork.BIAS_VALUE;
                        }
                    }
                }//Worker loop
                //Worker finish
                ProcessGradientWorkerOutputs(weigthsGradsAccInput, sumOfErrPowers);
            });
            //----------------------------------------------------
            //Update all network weights and biases
            Parallel.For(0, m_net.FlatWeights.Length, weightFlatIdx =>
            {
                RPROPWeightChange(weightFlatIdx);
            });
            //----------------------------------------------------
            //Store accumulated gradients for next iteration
            m_weigthsGradsAcc.CopyTo(m_weigthsPrevGradsAcc, 0);
            //----------------------------------------------------
            //Finish MSE
            m_lastMSE /= (double)(m_trainInputs.Count * m_net.OutputValuesCount);
            return;
        }

        [Serializable]
        private class WorkerRange
        {
            public int FromRow { get; set; }
            public int ToRow { get; set; }
        }//WorkerRange

    }//RPropTrainer

}
