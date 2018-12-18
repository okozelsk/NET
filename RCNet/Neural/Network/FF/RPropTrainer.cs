using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using RCNet.Extensions;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Implements iRPROP+ method trainer
    /// </summary>
    [Serializable]
    public class RPropTrainer : INonRecurrentNetworkTrainer
    {
        //Attributes
        private RPropTrainerSettings _settings;
        private FeedForwardNetwork _net;
        private List<double[]> _inputVectorCollection;
        private readonly List<double[]> _outputVectorCollection;
        private double[] _weigthsGradsAcc;
        private double[] _weigthsPrevGradsAcc;
        private double[] _weigthsPrevDeltas;
        private double[] _weigthsPrevChanges;
        private double _prevMSE;
        private GradientWorkerData[] _gradientWorkerDataCollection;

        //Attribute properties
        /// <summary>
        /// Epoch-1 error (MSE).
        /// </summary>
        public double MSE { get; private set; }

        /// <summary>
        /// Current epoch (incemented by call of Iteration)
        /// </summary>
        public int Epoch { get; private set; }

        //Constructor
        /// <summary>
        /// Instantiates the RPropTrainer
        /// </summary>
        /// <param name="net">
        /// The feed forward network to be trained
        /// </param>
        /// <param name="inputVectorCollection">
        /// Collection of the training input vectors
        /// </param>
        /// <param name="outputVectorCollection">
        /// Collection of the desired outputs
        /// </param>
        /// Trainer parameters
        /// <param name="settings">
        /// </param>
        public RPropTrainer(FeedForwardNetwork net,
                            List<double[]> inputVectorCollection,
                            List<double[]> outputVectorCollection,
                            RPropTrainerSettings settings = null
                            )
        {
            if (!net.Finalized)
            {
                throw new Exception("Can´t create trainer. Network structure was not finalized.");
            }
            _settings = settings;
            if (_settings == null)
            {
                //Default parameters
                _settings = new RPropTrainerSettings();
            }
            _net = net;
            _inputVectorCollection = inputVectorCollection;
            _outputVectorCollection = outputVectorCollection;
            _weigthsGradsAcc = new double[_net.NumOfWeights];
            _weigthsGradsAcc.Populate(0);
            _weigthsPrevGradsAcc = new double[_net.NumOfWeights];
            _weigthsPrevGradsAcc.Populate(0);
            _weigthsPrevDeltas = new double[_net.NumOfWeights];
            _weigthsPrevDeltas.Populate(_settings.IniDelta);
            _weigthsPrevChanges = new double[_net.NumOfWeights];
            _weigthsPrevChanges.Populate(0);
            _prevMSE = 0;
            MSE = 0;
            Epoch = 0;
            //Parallel gradient workers (batch ranges) preparation
            int numOfWorkers = Math.Max(1, Math.Min(Environment.ProcessorCount - 1, _inputVectorCollection.Count));
            //numOfWorkers = 1;
            _gradientWorkerDataCollection = new GradientWorkerData[numOfWorkers];
            int workerBatchSize = _inputVectorCollection.Count / numOfWorkers;
            for (int workerIdx = 0, fromRow = 0; workerIdx < numOfWorkers; workerIdx++, fromRow += workerBatchSize)
            {
                GradientWorkerData gwd = new GradientWorkerData
                (
                    fromRow: fromRow,
                    toRow: (workerIdx == numOfWorkers - 1 ? _inputVectorCollection.Count - 1 : (fromRow + workerBatchSize) - 1),
                    numOfWeights: _net.NumOfWeights
                );
                _gradientWorkerDataCollection[workerIdx] = gwd;
            }
            return;
        }

        //Properties
        /// <summary>
        /// FF network beeing trained
        /// </summary>
        public INonRecurrentNetwork Net { get { return _net; } }

        //Methods

        /// <summary>
        /// Decreases the zero recognition precision.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private double Sign(double value)
        {
            if (Math.Abs(value) <= _settings.ZeroTolerance)
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
        private void AdjustWeight(double[] flatWeights, int weightFlatIndex)
        {
            double weightChange = 0;
            double gradMulSign = Sign(_weigthsPrevGradsAcc[weightFlatIndex] * _weigthsGradsAcc[weightFlatIndex]);
            if (gradMulSign > 0)
            {
                //No sign change, increase delta
                _weigthsPrevDeltas[weightFlatIndex] = Math.Min(_weigthsPrevDeltas[weightFlatIndex] * _settings.PositiveEta, _settings.MaxDelta);
                weightChange = Sign(_weigthsGradsAcc[weightFlatIndex]) * _weigthsPrevDeltas[weightFlatIndex];
            }
            else if (gradMulSign < 0)
            {
                //Grad changed sign, decrease delta
                _weigthsPrevDeltas[weightFlatIndex] = Math.Max(_weigthsPrevDeltas[weightFlatIndex] * _settings.NegativeEta, _settings.MinDelta);
                //Force no change to delta in next iteration
                _weigthsGradsAcc[weightFlatIndex] = 0;
                weightChange = (MSE > _prevMSE) ? -_weigthsPrevChanges[weightFlatIndex] : 0;
            }
            else
            {
                //gradMulSign == 0 -> No change to delta
                weightChange = Sign(_weigthsGradsAcc[weightFlatIndex]) * _weigthsPrevDeltas[weightFlatIndex];
            }
            flatWeights[weightFlatIndex] += weightChange;
            _weigthsPrevChanges[weightFlatIndex] = weightChange;
            return;
        }

        //Must not be parallel to ensure the same counting order (and also results)
        private void ProcessGradientWorkersData()
        {
            MSE = 0;
            foreach (GradientWorkerData worker in _gradientWorkerDataCollection)
            {
                for (int i = 0; i < _weigthsGradsAcc.Length; i++)
                {
                    _weigthsGradsAcc[i] += worker._weigthsGradsAcc[i];
                }
                MSE += worker._sumSquaredErr;
            }
            //Finish the MSE computation
            MSE /= (double)(_inputVectorCollection.Count * _net.NumOfOutputValues);
            return;
        }

        /// <summary>
        /// Performs training iteration.
        /// </summary>
        public void Iteration()
        {
            //Next epoch
            ++Epoch;
            //Store previous iteration error
            _prevMSE = MSE;
            //Store previously accumulated weight gradients
            _weigthsGradsAcc.CopyTo(_weigthsPrevGradsAcc, 0);
            //Reset accumulated weight gradients
            _weigthsGradsAcc.Populate(0);
            //Get copy of the network weights
            double[] networkFlatWeights = _net.GetWeightsCopy();
            //Network output layer shortcut
            FeedForwardNetwork.Layer outputLayer = _net.LayerCollection[_net.LayerCollection.Count - 1];
            //Process gradient workers threads
            Parallel.ForEach(_gradientWorkerDataCollection, worker =>
            {
                //----------------------------------------------------------------------------------------------------
                //Gradient worker local variables
                List<double[]> layerInputCollection = new List<double[]>(_net.LayerCollection.Count);
                double[] gradients = new double[_net.NumOfNeurons];
                double[] derivatives = new double[_net.NumOfNeurons];
                //Reset gradient worker data
                worker.Reset();
                //Loop over the planned range of samples
                for (int row = worker._fromRow; row <= worker._toRow; row++)
                {
                    //----------------------------------------------------------------------------------------------------
                    //Reset of row dependents
                    layerInputCollection.Clear();
                    gradients.Populate(0);
                    derivatives.Populate(0);
                    //----------------------------------------------------------------------------------------------------
                    //Network computation (collect layers inputs and activation derivatives)
                    double[] computedOutputs = _net.Compute(_inputVectorCollection[row], layerInputCollection, derivatives);
                    //----------------------------------------------------------------------------------------------------
                    //Compute output layer gradients and update error
                    int outputLayerNeuronsFlatIdx = outputLayer.NeuronsStartFlatIdx;
                    for (int neuronIdx = 0; neuronIdx < outputLayer.NumOfLayerNeurons; neuronIdx++)
                    {
                        double error = _outputVectorCollection[row][neuronIdx] - computedOutputs[neuronIdx];
                        gradients[outputLayerNeuronsFlatIdx] = derivatives[outputLayerNeuronsFlatIdx] * error;
                        //Accumulate error
                        worker._sumSquaredErr += error * error;
                        ++outputLayerNeuronsFlatIdx;
                    }//neuronIdx
                    //----------------------------------------------------------------------------------------------------
                    //Hidden layers gradients
                    for (int layerIdx = _net.LayerCollection.Count - 2; layerIdx >= 0; layerIdx--)
                    {
                        FeedForwardNetwork.Layer currLayer = _net.LayerCollection[layerIdx];
                        FeedForwardNetwork.Layer nextLayer = _net.LayerCollection[layerIdx + 1];
                        int currLayerNeuronFlatIdx = currLayer.NeuronsStartFlatIdx;
                        for (int currLayerNeuronIdx = 0; currLayerNeuronIdx < currLayer.NumOfLayerNeurons; currLayerNeuronIdx++)
                        {
                            double sum = 0;
                            for (int nextLayerNeuronIdx = 0; nextLayerNeuronIdx < nextLayer.NumOfLayerNeurons; nextLayerNeuronIdx++)
                            {
                                int weightFlatIdx = nextLayer.WeightsStartFlatIdx + nextLayerNeuronIdx * nextLayer.NumOfInputNodes + currLayerNeuronIdx;
                                sum += gradients[nextLayer.NeuronsStartFlatIdx + nextLayerNeuronIdx] * networkFlatWeights[weightFlatIdx];
                            }//nextLayerNeuronIdx
                            gradients[currLayerNeuronFlatIdx] = derivatives[currLayerNeuronFlatIdx] * sum;
                            ++currLayerNeuronFlatIdx;
                        }//currLayerNeuronIdx
                    }//layerIdx
                    //----------------------------------------------------------------------------------------------------
                    //Compute increments for gradients accumulator
                    for (int layerIdx = 0; layerIdx < _net.LayerCollection.Count; layerIdx++)
                    {
                        FeedForwardNetwork.Layer layer = _net.LayerCollection[layerIdx];
                        double[] layerInputs = layerInputCollection[layerIdx];
                        int neuronFlatIdx = layer.NeuronsStartFlatIdx;
                        int weightFlatIdx = layer.WeightsStartFlatIdx;
                        int biasFlatIdx = layer.BiasesStartFlatIdx;
                        for (int neuronIdx = 0; neuronIdx < layer.NumOfLayerNeurons; neuronIdx++)
                        {
                            //Weights gradients accumulation
                            //Layer's inputs
                            for (int inputIdx = 0; inputIdx < layer.NumOfInputNodes; inputIdx++)
                            {
                                worker._weigthsGradsAcc[weightFlatIdx] += layerInputs[inputIdx] * gradients[neuronFlatIdx];
                                ++weightFlatIdx;
                            }
                            //Layer's input bias
                            worker._weigthsGradsAcc[biasFlatIdx] += FeedForwardNetwork.BiasValue * gradients[neuronFlatIdx];
                            ++neuronFlatIdx;
                            ++biasFlatIdx;
                        }//neuronIdx
                    }//layerIdx
                }//Worker main loop
            });//Worker finish
            //Update of gradient accumulator and MSE by workers
            ProcessGradientWorkersData();
            //Update all weights and biases
            Parallel.For(0, networkFlatWeights.Length, weightFlatIdx =>
            //for(int weightFlatIdx = 0; weightFlatIdx < networkFlatWeights.Length; weightFlatIdx++)
            {
                AdjustWeight(networkFlatWeights, weightFlatIdx);
            });
            //Set adjusted weights back into the network under training
            _net.SetWeights(networkFlatWeights);
            return;
        }

        //Inner classes
        [Serializable]
        private class GradientWorkerData
        {
            //Attribute properties
            public int _fromRow;
            public int _toRow;
            public double _sumSquaredErr;
            public double[] _weigthsGradsAcc;

            //Constructor
            public GradientWorkerData(int fromRow, int toRow, int numOfWeights)
            {
                _fromRow = fromRow;
                _toRow = toRow;
                _weigthsGradsAcc = new double[numOfWeights];
                Reset();
                return;
            }

            //Methods
            public void Reset()
            {
                _sumSquaredErr = 0;
                _weigthsGradsAcc.Populate(0);
                return;
            }

        }//WorkerData


    }//RPropTrainer

}//Namespace
