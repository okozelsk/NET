using RCNet.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.NonRecurrent.PP
{
    /// <summary>
    /// Implements the p-delta rule trainer of the parallel perceptron network
    /// </summary>
    [Serializable]
    public class PDeltaRuleTrainer : INonRecurrentNetworkTrainer
    {
        //Attribute properties
        /// <inheritdoc/>
        public double MSE { get; private set; }
        /// <inheritdoc/>
        public int MaxAttempt { get; private set; }
        /// <inheritdoc/>
        public int Attempt { get; private set; }
        /// <inheritdoc/>
        public int MaxAttemptEpoch { get; private set; }
        /// <inheritdoc/>
        public int AttemptEpoch { get; private set; }
        /// <inheritdoc/>
        public string InfoMessage { get; private set; }

        //Attributes
        private readonly PDeltaRuleTrainerSettings _settings;
        private readonly ParallelPerceptron _net;
        private readonly List<double[]> _inputVectorCollection;
        private readonly List<double[]> _outputVectorCollection;
        private readonly Random _rand;
        private readonly double _acceptableError;
        private readonly double _resSquashCoeff;
        private readonly double _marginSignificance;
        private double _clearMargin;
        private readonly double _minM;
        private readonly double _maxM;
        private double _learningRate;
        private double[] _prevWeights;
        private double _prevMSE;
        private readonly List<WorkerRange> _workerRangeCollection;

        //Constructor
        /// <summary>
        /// Constructs a parallel perceptron P-Delta rule trainer
        /// </summary>
        /// <param name="net">PP to be trained</param>
        /// <param name="inputVectorCollection">Predictors (input)</param>
        /// <param name="outputVectorCollection">Ideal outputs (the same number of rows as predictors rows)</param>
        /// <param name="settings">Configuration of the trainer</param>
        /// <param name="rand">Random object to be used</param>
        public PDeltaRuleTrainer(ParallelPerceptron net,
                                 List<double[]> inputVectorCollection,
                                 List<double[]> outputVectorCollection,
                                 PDeltaRuleTrainerSettings settings,
                                 Random rand
                                 )
        {
            //Parameters
            _settings = (PDeltaRuleTrainerSettings)settings.DeepClone();
            MaxAttempt = _settings.NumOfAttempts;
            MaxAttemptEpoch = _settings.NumOfAttemptEpochs;
            _net = net;
            _rand = rand;
            _inputVectorCollection = inputVectorCollection;
            _outputVectorCollection = outputVectorCollection;
            _resSquashCoeff = _net.ResSquashCoeff;
            _acceptableError = 1d / (2d * _resSquashCoeff);
            _marginSignificance = 1;
            _clearMargin = 0.05;
            _minM = _acceptableError * _resSquashCoeff;
            _maxM = 4d * _minM;
            //Parallel workers / batch ranges preparation
            _workerRangeCollection = new List<WorkerRange>();
            int numOfWorkers = Math.Min(Environment.ProcessorCount, _inputVectorCollection.Count);
            numOfWorkers = Math.Max(1, numOfWorkers);
            int workerBatchSize = _inputVectorCollection.Count / numOfWorkers;
            for (int workerIdx = 0, fromRow = 0; workerIdx < numOfWorkers; workerIdx++, fromRow += workerBatchSize)
            {
                int toRow = 0;
                if (workerIdx == numOfWorkers - 1)
                {
                    toRow = _inputVectorCollection.Count - 1;
                }
                else
                {
                    toRow = (fromRow + workerBatchSize) - 1;
                }
                WorkerRange workerRange = new WorkerRange(fromRow, toRow, _net.NumOfWeights);
                _workerRangeCollection.Add(workerRange);
            }
            InfoMessage = string.Empty;
            //Start training attempt
            Attempt = 0;
            NextAttempt();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public INonRecurrentNetwork Net { get { return _net; } }

        //Methods
        private void AdjustLearning(double M)
        {
            bool applyWeights = true;
            double clearMarginLR = _learningRate;
            if (AttemptEpoch >= 2)
            {
                if (_prevMSE > MSE)
                {
                    //Increase learning rate
                    _learningRate *= _settings.IncLR;
                    _learningRate = Math.Min(_settings.MaxLR, _learningRate);
                }
                else if (_prevMSE < MSE)
                {
                    if (_learningRate > _settings.MinLR)
                    {
                        applyWeights = false;
                    }
                    //Decrease learning rate
                    _learningRate *= _settings.DecLR;
                    _learningRate = Math.Max(_settings.MinLR, _learningRate);
                }
            }
            if (applyWeights)
            {
                //Store MSE
                _prevMSE = MSE;
                //Adjust clear margin
                for (int i = 0; i < _net.Gates; i++)
                {
                    _clearMargin += clearMarginLR * (_minM - Math.Min(_maxM, M));
                }
            }
            else
            {
                //Do not apply iteration updates
                MSE = _prevMSE;
                _net.SetWeights(_prevWeights);
            }
            return;
        }

        /// <inheritdoc/>
        public bool NextAttempt()
        {
            if (Attempt < MaxAttempt)
            {
                //Next attempt is allowed
                ++Attempt;
                //Reset
                _net.RandomizeWeights(_rand);
                _clearMargin = 0.05;
                _learningRate = _settings.IniLR;
                _prevWeights = _net.GetWeights();
                _prevMSE = 0;
                MSE = 0;
                AttemptEpoch = 0;
                return true;
            }
            else
            {
                //Max attempt reached -> do nothhing and return false
                return false;
            }
        }

        /// <inheritdoc/>
        public bool Iteration()
        {
            if (AttemptEpoch == MaxAttemptEpoch)
            {
                //Max epoch reached, try new attempt
                if (!NextAttempt())
                {
                    //Next attempt is not available
                    return false;
                }
            }
            //Epoch increment
            ++AttemptEpoch;
            //Store network weights
            _prevWeights = _net.GetWeights();
            //Weights update
            double[] adjustedNetworkWeights = _net.GetWeights();
            Parallel.ForEach(_workerRangeCollection, worker =>
            {
                double[] gateSums = new double[_net.Gates];
                for (int row = worker.FromRow; row <= worker.ToRow; row++)
                {
                    double computedOutput = _net.Compute(_inputVectorCollection[row], gateSums)[0];
                    double idealOutput = _outputVectorCollection[row][0];
                    for (int gateIdx = 0; gateIdx < _net.Gates; gateIdx++)
                    {
                        double x = 0;
                        if (computedOutput > (idealOutput + _acceptableError) && gateSums[gateIdx] >= 0)
                        {
                            x = -1;
                        }
                        else if (computedOutput < (idealOutput - _acceptableError) && gateSums[gateIdx] < 0)
                        {
                            x = 1;
                        }
                        else if (computedOutput <= (idealOutput + _acceptableError) && gateSums[gateIdx] >= 0 && gateSums[gateIdx] < _clearMargin)
                        {
                            ++worker.M;
                            x = _marginSignificance;
                        }
                        else if (computedOutput >= (idealOutput - _acceptableError) && gateSums[gateIdx] >= -_clearMargin && gateSums[gateIdx] < 0)
                        {
                            ++worker.M;
                            x = -_marginSignificance;
                        }
                        else
                        {
                            //No change
                            x = 0;
                        }
                        //Weights update
                        if (x != 0)
                        {
                            int weightFlatIdx = gateIdx * (_net.NumOfInputValues + 1);
                            for (int i = 0; i < _net.NumOfInputValues + 1; i++, weightFlatIdx++)
                            {
                                double inputValue = i < _net.NumOfInputValues ? _inputVectorCollection[row][i] : ParallelPerceptron.BiasValue;
                                worker.WeightChangeAcc[weightFlatIdx] += _learningRate * inputValue * x;
                            }
                        }
                    }
                }
            });
            //Update original weights, affect workers accumulators.
            double glM = 0;
            foreach (WorkerRange worker in _workerRangeCollection)
            {
                glM += worker.M;
                worker.UpdateWeigths(adjustedNetworkWeights);
            }
            //How it looks after weight changes?
            _net.SetWeights(adjustedNetworkWeights);
            _net.NormalizeWeights();
            MSE = _net.ComputeBatchErrorStat(_inputVectorCollection, _outputVectorCollection).MeanSquare;
            //Adjust learning parameters and weights according to results
            AdjustLearning(glM / (double)_inputVectorCollection.Count);
            return true;
        }

        //Inner classes
        [Serializable]
        internal class WorkerRange
        {
            public int FromRow { get; set; }
            public int ToRow { get; set; }
            public double[] WeightChangeAcc { get; set; }
            public double M { get; set; }

            //Constructor
            internal WorkerRange(int fromRow, int toRow, int numOfWeights)
            {
                FromRow = fromRow;
                ToRow = toRow;
                WeightChangeAcc = new double[numOfWeights];
                WeightChangeAcc.Populate(0);
                M = 0;
                return;
            }

            //Methods
            internal void UpdateWeigths(double[] weights)
            {
                Parallel.For(0, WeightChangeAcc.Length, i =>
                {
                    weights[i] += WeightChangeAcc[i];
                    //Reset back to zero
                    WeightChangeAcc[i] = 0;
                });
                M = 0;
                return;
            }
        }//WorkerRange

    }//ParallelPerceptronTrainer

}//Namespace
