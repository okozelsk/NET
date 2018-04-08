﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;

namespace RCNet.Neural.Network.PP
{
    /// <summary>
    /// Startup parameters for the parallel perceptron trainer
    /// </summary>
    [Serializable]
    public class PPTrainParameters
    {
        //Constants
        /// <summary>
        /// Default initial learning rate
        /// </summary>
        public const double DeafaultInitialLearningRate = 0.01d;
        /// <summary>
        /// Default learning rate increase
        /// </summary>
        public const double DeafaultLearningRateIncrease = 1.1d;
        /// <summary>
        /// Default learning rate decrease
        /// </summary>
        public const double DeafaultLearningRateDecrease = 0.5d;
        /// <summary>
        /// Default learning min rate
        /// </summary>
        public const double DeafaultLearningMinRate = 1E-4;
        /// <summary>
        /// Default learning max rate
        /// </summary>
        public const double DeafaultLearningMaxRate = 0.1d;

        //Attributes
        /// <summary>
        /// Initial learning rate
        /// </summary>
        public double InitialLearningRate { get; set; } = DeafaultInitialLearningRate;
        /// <summary>
        /// Learning rate increase
        /// </summary>
        public double LearningRateIncrease { get; set; } = DeafaultLearningRateIncrease;
        /// <summary>
        /// Learning rate decrease
        /// </summary>
        public double LearningRateDecrease { get; set; } = DeafaultLearningRateDecrease;
        /// <summary>
        /// Learning rate minimum
        /// </summary>
        public double LearningMinRate { get; set; } = DeafaultLearningMinRate;
        /// <summary>
        /// Learning rate maximum
        /// </summary>
        public double LearningMaxRate { get; set; } = DeafaultLearningMaxRate;

    }//PPTrainParameters

    /// <summary>
    /// Implements parallel perceptron trainer (p-delta rule)
    /// </summary>
    [Serializable]
    public class ParallelPerceptronTrainer : INonRecurrentNetworkTrainer
    {
        //Attributes
        private PPTrainParameters _parameters;
        private ParallelPerceptron _net;
        private List<double[]> _inputVectorCollection;
        private List<double[]> _outputVectorCollection;
        private double _acceptableError;
        private double _resSquashCoeff;
        private double _marginSignificance;
        private double _clearMargin;
        private double _minM;
        private double _maxM;
        private double _learningRate;
        private double[] _prevWeights;
        private double _prevMSE;
        private double _currMSE;
        private int _epoch;
        private List<WorkerRange> _workerRangeCollection;

        //Constructor
        /// <summary>
        /// Constructs a parallel perceptron trainer
        /// </summary>
        /// <param name="net">PP to be trained</param>
        /// <param name="inputVectorCollection">Predictors (input)</param>
        /// <param name="outputVectorCollection">Ideal outputs (the same number of rows as number of inputs)</param>
        /// <param name="parameters">Optional startup parameters of the trainer</param>
        public ParallelPerceptronTrainer(ParallelPerceptron net,
                                         List<double[]> inputVectorCollection,
                                         List<double[]> outputVectorCollection,
                                         PPTrainParameters parameters = null
                                         )
        {
            //Parameters
            _parameters = parameters;
            if (_parameters == null)
            {
                //Default parameters
                _parameters = new PPTrainParameters();
            }
            _net = net;
            _inputVectorCollection = inputVectorCollection;
            _outputVectorCollection = outputVectorCollection;
            _resSquashCoeff = _net.ResSquashCoeff;
            _acceptableError = 1d / (2d * _resSquashCoeff);
            _marginSignificance = 1;
            _clearMargin = 0.05;
            _minM = _acceptableError * _resSquashCoeff;
            _maxM = 4d * _minM;
            _learningRate = _parameters.InitialLearningRate;
            _prevWeights = _net.GetWeights();
            _prevMSE = 0;
            _currMSE = 0;
            _epoch = 0;
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
            return;
        }

        //Properties
        /// <summary>
        /// MSE after an epoch.
        /// </summary>
        public double MSE { get { return _currMSE; } }
        /// <summary>
        /// Current epoch (incremented each call of Iteration)
        /// </summary>
        public int Epoch { get { return _epoch; } }
        /// <summary>
        /// PP network beeing trained
        /// </summary>
        public INonRecurrentNetwork Net { get { return _net; } }

        //Methods
        private void AdjustLearning(double M)
        {
            bool applyWeights = true;
            double clearMarginLR = _learningRate;
            if (_epoch >= 2)
            {
                if (_prevMSE > _currMSE)
                {
                    //Increase learning rate
                    _learningRate *= _parameters.LearningRateIncrease;
                    _learningRate = Math.Min(_parameters.LearningMaxRate, _learningRate);
                }
                else if (_prevMSE < _currMSE)
                {
                    if(_learningRate > _parameters.LearningMinRate)
                    {
                        applyWeights = false;
                    }
                    //Decrease learning rate
                    _learningRate *= _parameters.LearningRateDecrease;
                    _learningRate = Math.Max(_parameters.LearningMinRate, _learningRate);
                }
            }
            if(applyWeights)
            {
                //Store MSE
                _prevMSE = _currMSE;
                //Adjust clear margin
                for (int i = 0; i < _net.NumOfGates; i++)
                {
                    _clearMargin += clearMarginLR * (_minM - Math.Min(_maxM, M));
                }
            }
            else
            {
                //Do not apply iteration updates
                _currMSE = _prevMSE;
                _net.SetWeights(_prevWeights);
            }
            return;
        }

        /// <summary>
        /// Performs training iteration.
        /// </summary>
        public void Iteration()
        {
            //Store network weights
            _prevWeights = _net.GetWeights();
            //Epoch increment
            ++_epoch;
            //Weights update
            double[] adjustedNetworkWeights = _net.GetWeights();
            Parallel.ForEach(_workerRangeCollection, worker =>
            {
                double[] gateSums = new double[_net.NumOfGates];
                for (int row = worker.FromRow; row <= worker.ToRow; row++)
                {
                    double computedOutput = _net.Compute(_inputVectorCollection[row], gateSums)[0];
                    double idealOutput = _outputVectorCollection[row][0];
                    for (int gateIdx = 0; gateIdx < _net.NumOfGates; gateIdx++)
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
            //Adjust local weights
            double glM = 0;
            foreach (WorkerRange worker in _workerRangeCollection)
            {
                glM += worker.M;
                worker.UpdateWeigths(adjustedNetworkWeights);
            }
            //How it looks after weight changes?
            _net.SetWeights(adjustedNetworkWeights);
            _net.NormalizeWeights();
            List<double[]> computedVectors = null;
            _currMSE = _net.ComputeBatchErrorStat(_inputVectorCollection, _outputVectorCollection, out computedVectors).MeanSquare;
            //Adjust learning parameters and weights according to results
            AdjustLearning(glM / (double)_inputVectorCollection.Count);
            return;
        }

        //Inner classes
        [Serializable]
        private class WorkerRange
        {
            public int FromRow { get; set; }
            public int ToRow { get; set; }
            public double[] WeightChangeAcc { get; set; }
            public double M { get; set; }

        //Constructor
        public WorkerRange(int fromRow, int toRow, int numOfWeights)
            {
                FromRow = fromRow;
                ToRow = toRow;
                WeightChangeAcc = new double[numOfWeights];
                WeightChangeAcc.Populate(0);
                M = 0;
                return;
            }

            //Methods
            public void UpdateWeigths(double[] weights)
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
