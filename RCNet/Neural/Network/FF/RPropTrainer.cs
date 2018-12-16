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
        private List<GradientWorkerData> _gradientWorkerDataCollection;

        //Attribute properties
        /// <summary>
        /// !Epoch-1 error (MSE).
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
            //Parallel gradient workers / batch ranges preparation
            _gradientWorkerDataCollection = new List<GradientWorkerData>();
            int numOfWorkers = Math.Max(1, Math.Min(Environment.ProcessorCount - 1, _inputVectorCollection.Count));
            int workerBatchSize = _inputVectorCollection.Count / numOfWorkers;
            for (int workerIdx = 0, fromRow = 0; workerIdx < numOfWorkers; workerIdx++, fromRow += workerBatchSize)
            {
                GradientWorkerData gwd = new GradientWorkerData
                (
                    fromRow: fromRow,
                    toRow: (workerIdx == numOfWorkers - 1 ? _inputVectorCollection.Count - 1 : (fromRow + workerBatchSize) - 1),
                    numOfWeights: _net.NumOfWeights
                );
                _gradientWorkerDataCollection.Add(gwd);
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
        private void PerformIRPropPlusWeightChange(double[] flatWeights, int weightFlatIndex)
        {
            double weightChange = 0;
            double delta = 0;
            double gradSign = Sign(_weigthsPrevGradsAcc[weightFlatIndex] * _weigthsGradsAcc[weightFlatIndex]);
            if (gradSign > 0)
            {
                //No sign change, increase delta
                delta = _weigthsPrevDeltas[weightFlatIndex] * _settings.PositiveEta;
                if (delta > _settings.MaxDelta) delta = _settings.MaxDelta;
                _weigthsPrevDeltas[weightFlatIndex] = delta;
                weightChange = Sign(_weigthsGradsAcc[weightFlatIndex]) * delta;
            }
            else if (gradSign < 0)
            {
                //Grad changed sign, decrease delta
                delta = _weigthsPrevDeltas[weightFlatIndex] * _settings.NegativeEta;
                if (delta < _settings.MinDelta) delta = _settings.MinDelta;
                _weigthsPrevDeltas[weightFlatIndex] = delta;
                if (MSE > _prevMSE)
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
            double[] networkFlatWeights = _net.GetWeights();
            //Network output layer shortcut
            FeedForwardNetwork.Layer outputLayer = _net.LayerCollection[_net.LayerCollection.Count - 1];
            //Process gradient workers threads
            Parallel.ForEach(_gradientWorkerDataCollection, worker =>
            {
                //Gradient worker local variables
                double[] gradients = new double[_net.NumOfNeurons];
                double[] derivatives = new double[_net.NumOfNeurons];
                List<double[]> layerInputCollection = new List<double[]>(_net.LayerCollection.Count);
                //Reset gradient worker data
                worker.Reset();
                //Loop over the planned range of samples
                for (int row = worker._fromRow; row <= worker._toRow; row++)
                {
                    layerInputCollection.Clear();
                    gradients.Populate(0);
                    //Network computation (collect layers inputs and activation derivatives)
                    double[] computedOutputs = _net.Compute(_inputVectorCollection[row], layerInputCollection, derivatives);
                    //Compute output layer gradients and update error
                    for (int neuronIdx = 0, outputLayerNeuronsFlatIdx = outputLayer.NeuronsStartFlatIdx;
                         neuronIdx < outputLayer.NumOfLayerNeurons;
                         neuronIdx++, outputLayerNeuronsFlatIdx++
                         )
                    {
                        double error = _outputVectorCollection[row][neuronIdx] - computedOutputs[neuronIdx];
                        gradients[outputLayerNeuronsFlatIdx] = derivatives[outputLayerNeuronsFlatIdx] * error;
                        //Accumulate error
                        worker._sumSquaredErr += error * error;
                    }//neuronIdx
                    //Hidden layers gradients
                    for (int layerIdx = _net.LayerCollection.Count - 2; layerIdx >= 0; layerIdx--)
                    {
                        FeedForwardNetwork.Layer currLayer = _net.LayerCollection[layerIdx];
                        FeedForwardNetwork.Layer nextLayer = _net.LayerCollection[layerIdx + 1];
                        for (int currLayerNeuronIdx = 0, currLayerNeuronFlatIdx = currLayer.NeuronsStartFlatIdx;
                             currLayerNeuronIdx < currLayer.NumOfLayerNeurons;
                             currLayerNeuronIdx++, currLayerNeuronFlatIdx++
                             )
                        {
                            double sum = 0;
                            for (int nextLayerNeuronIdx = 0; nextLayerNeuronIdx < nextLayer.NumOfLayerNeurons; nextLayerNeuronIdx++)
                            {
                                int nextLayerWeightFlatIdx = nextLayer.WeightsStartFlatIdx + nextLayerNeuronIdx * nextLayer.NumOfInputNodes + currLayerNeuronIdx;
                                sum += gradients[nextLayer.NeuronsStartFlatIdx + nextLayerNeuronIdx] * networkFlatWeights[nextLayerWeightFlatIdx];
                            }//nextLayerNeuronIdx
                            gradients[currLayerNeuronFlatIdx] = derivatives[currLayerNeuronFlatIdx] * sum;
                        }//currLayerNeuronIdx
                    }//layerIdx
                    //Compute increments for gradients accumulator
                    for (int layerIdx = 0; layerIdx < _net.LayerCollection.Count; layerIdx++)
                    {
                        FeedForwardNetwork.Layer layer = _net.LayerCollection[layerIdx];
                        double[] layerInputs = layerInputCollection[layerIdx];
                        for (int neuronIdx = 0, neuronFlatIdx = layer.NeuronsStartFlatIdx, weightFlatIdx = layer.WeightsStartFlatIdx, biasFlatIdx = layer.BiasesStartFlatIdx;
                             neuronIdx < layer.NumOfLayerNeurons;
                             neuronIdx++, neuronFlatIdx++, biasFlatIdx++
                             )
                        {
                            //Weights gradients accumulation
                            //Layer's inputs
                            for (int inputIdx = 0; inputIdx < layer.NumOfInputNodes; inputIdx++, weightFlatIdx++)
                            {
                                worker._weigthsGradsAcc[weightFlatIdx] += layerInputs[inputIdx] * gradients[neuronFlatIdx];
                            }
                            //Layer's input bias
                            worker._weigthsGradsAcc[biasFlatIdx] += FeedForwardNetwork.BiasValue * gradients[neuronFlatIdx];
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
                PerformIRPropPlusWeightChange(networkFlatWeights, weightFlatIdx);
            });
            //Set adjusted weights back into the network
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
