using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using RCNet.Extensions;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Parameters of RPROP trainer
    /// </summary>
    [Serializable]
    public class RPropParameters
    {
        //Constants
        public const double DefaultZeroTolerance = 1E-17d;
        public const double DefaultPositiveEta = 1.2d;
        public const double DefaultNegativeEta = 0.5d;
        public const double DefaultDeltaIni = 0.1d;
        public const double DefaultDeltaMin = 1E-6d;
        public const double DefaultDeltaMax = 50d;

        //Attributes
        public double ZeroTolerance { get; set; } = DefaultZeroTolerance;
        public double PositiveEta { get; set; } = DefaultPositiveEta;
        public double NegativeEta { get; set; } = DefaultNegativeEta;
        public double DeltaIni { get; set; } = DefaultDeltaIni;
        public double DeltaMin { get; set; } = DefaultDeltaMin;
        public double DeltaMax { get; set; } = DefaultDeltaMax;
    }//RPropParameters

    /// <summary>
    /// Implements iRPROP+ method trainer
    /// </summary>
    [Serializable]
    public class RPropTrainer : IFeedForwardNetworkTrainer
    {
        //Attributes
        private RPropParameters _parameters;
        private FeedForwardNetwork _net;
        private List<double[]> _trainInputs;
        private List<double[]> _trainIdealOutputs;
        private double[] _weigthsGradsAcc;
        private double[] _weigthsPrevGradsAcc;
        private double[] _weigthsPrevDeltas;
        private double[] _weigthsPrevChanges;
        private double _prevMSE;
        private double _lastMSE;
        private int _epoch;
        private List<WorkerRange> _workerRangeCollection;

        //Constructor
        public RPropTrainer(FeedForwardNetwork net, List<double[]> inputs, List<double[]> outputs, RPropParameters parameters = null)
        {
            if (!net.Finalized)
            {
                throw new Exception("Can´t create trainer. Network structure was not finalized.");
            }
            _parameters = parameters;
            if (_parameters == null)
            {
                //Default parameters
                _parameters = new RPropParameters();
            }
            _net = net;
            _trainInputs = inputs;
            _trainIdealOutputs = outputs;
            _weigthsGradsAcc = new double[_net.FlatWeights.Length];
            _weigthsGradsAcc.Populate(0);
            _weigthsPrevGradsAcc = new double[_net.FlatWeights.Length];
            _weigthsPrevGradsAcc.Populate(0);
            _weigthsPrevDeltas = new double[_net.FlatWeights.Length];
            _weigthsPrevDeltas.Populate(_parameters.DeltaIni);
            _weigthsPrevChanges = new double[_net.FlatWeights.Length];
            _weigthsPrevChanges.Populate(0);
            _prevMSE = 0;
            _lastMSE = 0;
            _epoch = 0;
            //Parallel gradient workers count and batch ranges preparation
            _workerRangeCollection = new List<WorkerRange>();
            int workersCount = Math.Min(Environment.ProcessorCount, _trainInputs.Count);
            workersCount = Math.Max(1, workersCount);
            int workerBatchSize = _trainInputs.Count / workersCount;
            for (int workerIdx = 0, fromRow = 0; workerIdx < workersCount; workerIdx++, fromRow += workerBatchSize)
            {
                WorkerRange workerRange = new WorkerRange();
                workerRange.FromRow = fromRow;
                if (workerIdx == workersCount - 1)
                {
                    workerRange.ToRow = _trainInputs.Count - 1;
                }
                else
                {
                    workerRange.ToRow = (fromRow + workerBatchSize) - 1;
                }
                _workerRangeCollection.Add(workerRange);
            }
            return;
        }

        //Properties
        /// <summary>
        /// !Epoch-1 error (MSE).
        /// </summary>
        public double MSE { get { return _lastMSE; } }
        /// <summary>
        /// Current epoch (incemented by call of Iteration)
        /// </summary>
        public int Epoch { get { return _epoch; } }
        /// <summary>
        /// Trainee FF network
        /// </summary>
        public FeedForwardNetwork Net { get { return _net; } }

        //Methods

        /// <summary>
        /// Decreases zero recognition precision.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double Sign(double value)
        {
            if (Math.Abs(value) <= _parameters.ZeroTolerance)
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
        private void PerformIRPropPlusWeightChange(int weightFlatIndex)
        {
            double weightChange = 0;
            double delta = 0;
            double gradSign = Sign(_weigthsPrevGradsAcc[weightFlatIndex] * _weigthsGradsAcc[weightFlatIndex]);
            if (gradSign > 0)
            {
                //No sign change, increase delta
                delta = _weigthsPrevDeltas[weightFlatIndex] * _parameters.PositiveEta;
                if (delta > _parameters.DeltaMax) delta = _parameters.DeltaMax;
                _weigthsPrevDeltas[weightFlatIndex] = delta;
                weightChange = Sign(_weigthsGradsAcc[weightFlatIndex]) * delta;
            }
            else if (gradSign < 0)
            {
                //Grad changed sign, decrease delta
                delta = _weigthsPrevDeltas[weightFlatIndex] * _parameters.NegativeEta;
                if (delta < _parameters.DeltaMin) delta = _parameters.DeltaMin;
                _weigthsPrevDeltas[weightFlatIndex] = delta;
                if (_lastMSE > _prevMSE)
                {
                    weightChange = -_weigthsPrevChanges[weightFlatIndex];
                }
                //Force no adjustments in next iteration
                _weigthsGradsAcc[weightFlatIndex] = 0;
            }
            else
            {
                //No change to delta
                delta = _weigthsPrevDeltas[weightFlatIndex];
                weightChange = Sign(_weigthsGradsAcc[weightFlatIndex]) * delta;
            }
            _net.FlatWeights[weightFlatIndex] += weightChange;
            _weigthsPrevChanges[weightFlatIndex] = weightChange;
            return;
        }

        /// <summary>
        /// Network computation for training purposes.
        /// </summary>
        private double[] Compute(double[] input, List<double[]> layersInputs, double[] derivatives)
        {
            double[] result = input;
            foreach (FeedForwardNetwork.Layer layer in _net.Layers)
            {
                layersInputs.Add(result);
                result = layer.Compute(result, _net.FlatWeights, derivatives);
            }
            return result;
        }

        private void ProcessGradientWorkerOutputs(double[] weigthsGradsAccInput, double sumOfErrorPowers)
        {
            lock (_weigthsGradsAcc)
            {
                for (int i = 0; i < weigthsGradsAccInput.Length; i++)
                {
                    _weigthsGradsAcc[i] += weigthsGradsAccInput[i];
                    _lastMSE += sumOfErrorPowers;
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
            ++_epoch;
            //Store previous iteration error
            _prevMSE = _lastMSE;
            //Reset iteration error
            _lastMSE = 0;
            //----------------------------------------------------
            //Accumulated weights gradients over all training data
            _weigthsGradsAcc.Populate(0);
            //Process gradient workers threads
            Parallel.ForEach(_workerRangeCollection, range =>
            {
                //----------------------------------------------------
                //Gradient worker variables
                double sumOfErrPowers = 0;
                double[] weigthsGradsAccInput = new double[_net.FlatWeights.Length];
                weigthsGradsAccInput.Populate(0);
                double[] nodesGradients = new double[_net.NodesCount];
                double[] derivatives = new double[_net.NodesCount];
                List<double[]> layersInputs = new List<double[]>(_net.Layers.Count);
                //----------------------------------------------------
                //Go paralelly through all samples
                for (int row = range.FromRow; row <= range.ToRow; row++)
                {
                    //----------------------------------------------------
                    //Network computation (collect layers inputs and derivatives)
                    layersInputs.Clear();
                    double[] computedOutputs = Compute(_trainInputs[row], layersInputs, derivatives);
                    //----------------------------------------------------
                    //Compute network nodes gradients
                    //Compute output layer gradients and update last error statistics
                    FeedForwardNetwork.Layer outputLayer = _net.Layers[_net.Layers.Count - 1];
                    for (int nodeIdx = 0, outputLayerNodeFlatIdx = outputLayer.NodesStartFlatIdx; nodeIdx < outputLayer.LayerNodesCount; nodeIdx++, outputLayerNodeFlatIdx++)
                    {
                        double error = _trainIdealOutputs[row][nodeIdx] - computedOutputs[nodeIdx];
                        nodesGradients[outputLayerNodeFlatIdx] = derivatives[outputLayerNodeFlatIdx] * error;
                        //Accumulate power of error
                        sumOfErrPowers += error * error;
                    }
                    //Hidden layers gradients
                    for (int layerIdx = _net.Layers.Count - 2; layerIdx >= 0; layerIdx--)
                    {
                        FeedForwardNetwork.Layer currLayer = _net.Layers[layerIdx];
                        FeedForwardNetwork.Layer nextLayer = _net.Layers[layerIdx + 1];
                        int currLayerNodeFlatIdx = currLayer.NodesStartFlatIdx;
                        for (int currLayerNodeIdx = 0; currLayerNodeIdx < currLayer.LayerNodesCount; currLayerNodeIdx++, currLayerNodeFlatIdx++)
                        {
                            double sum = 0;
                            for (int nextLayerNodeIdx = 0; nextLayerNodeIdx < nextLayer.LayerNodesCount; nextLayerNodeIdx++)
                            {
                                int nextLayerWeightFlatIdx = nextLayer.WeightsStartFlatIdx + nextLayerNodeIdx * nextLayer.InputNodesCount + currLayerNodeIdx;
                                sum += nodesGradients[nextLayer.NodesStartFlatIdx + nextLayerNodeIdx] * _net.FlatWeights[nextLayerWeightFlatIdx];
                            }
                            nodesGradients[currLayerNodeFlatIdx] = derivatives[currLayerNodeFlatIdx] * sum;
                        }
                    }
                    //----------------------------------------------------
                    //Compute increments for gradients accumulator
                    for (int layerIdx = 0; layerIdx < _net.Layers.Count; layerIdx++)
                    {
                        FeedForwardNetwork.Layer layer = _net.Layers[layerIdx];
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
                            weigthsGradsAccInput[biasFlatIdx] += nodesGradients[nodeFlatIdx] * FeedForwardNetwork.BiasValue;
                        }
                    }
                }//Worker loop
                //Worker finish
                ProcessGradientWorkerOutputs(weigthsGradsAccInput, sumOfErrPowers);
            });
            //----------------------------------------------------
            //Update all network weights and biases
            Parallel.For(0, _net.FlatWeights.Length, weightFlatIdx =>
            {
                PerformIRPropPlusWeightChange(weightFlatIdx);
            });
            //----------------------------------------------------
            //Store accumulated gradients for next iteration
            _weigthsGradsAcc.CopyTo(_weigthsPrevGradsAcc, 0);
            //----------------------------------------------------
            //Finish MSE
            _lastMSE /= (double)(_trainInputs.Count * _net.OutputValuesCount);
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
