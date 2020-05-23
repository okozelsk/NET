using RCNet.Extensions;
using RCNet.Neural.Activation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Implements the elastic linear regression trainer.
    /// FF network has to have only output layer with the Identity activation.
    /// 
    /// Based on Regularization Paths for Generalized Linear Models via Coordinate Descent
    /// (Jerome Friedman, Trevor Hastie, Rob Tibshirani)
    /// Publication: Journal of Statistical Software (January 2010, Volume 33, Issue 1.)
    /// 
    /// </summary>
    [Serializable]
    public class ElasticRegrTrainer : INonRecurrentNetworkTrainer
    {
        //Constants
        //Attribute properties
        /// <summary>
        /// Epoch error (MSE).
        /// </summary>
        public double MSE { get; private set; }
        /// <summary>
        /// Max attempt
        /// </summary>
        public int MaxAttempt { get; private set; }
        /// <summary>
        /// Current attempt
        /// </summary>
        public int Attempt { get; private set; }
        /// <summary>
        /// Max epoch
        /// </summary>
        public int MaxAttemptEpoch { get; private set; }
        /// <summary>
        /// Current epoch (incremented each call of Iteration)
        /// </summary>
        public int AttemptEpoch { get; private set; }
        /// <summary>
        /// Informative message from the trainer
        /// </summary>
        public string InfoMessage { get; private set; }

        //Attributes
        private readonly ElasticRegrTrainerSettings _settings;
        private readonly FeedForwardNetwork _net;
        private readonly List<double[]> _inputVectorCollection;
        private readonly List<double[]> _outputVectorCollection;
        private readonly double _gamma;
        private readonly List<Tuple<int, int>> _parallelRanges;

        //Constructor
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="net">FF network to be trained</param>
        /// <param name="inputVectorCollection">Predictors (input)</param>
        /// <param name="outputVectorCollection">Ideal outputs (the same number of rows as number of inputs)</param>
        /// <param name="settings">Startup parameters of the trainer</param>
        public ElasticRegrTrainer(FeedForwardNetwork net,
                                     List<double[]> inputVectorCollection,
                                     List<double[]> outputVectorCollection,
                                     ElasticRegrTrainerSettings settings
                                     )
        {
            //Check network readyness
            if (!net.Finalized)
            {
                throw new InvalidOperationException($"Can´t create trainer. Network structure was not finalized.");
            }
            //Check network conditions
            if (net.LayerCollection.Count != 1 || !(net.LayerCollection[0].Activation is Identity))
            {
                throw new InvalidOperationException($"Can´t create trainer. Network structure is not complient (single layer having Identity activation).");
            }
            //Check samples conditions
            if (inputVectorCollection.Count == 0)
            {
                throw new InvalidOperationException($"Can´t create trainer. Missing training samples.");
            }
            //Collections
            _inputVectorCollection = new List<double[]>(inputVectorCollection);
            _outputVectorCollection = new List<double[]>(outputVectorCollection);
            var rangePartitioner = Partitioner.Create(0, _inputVectorCollection.Count);
            _parallelRanges = new List<Tuple<int, int>>(rangePartitioner.GetDynamicPartitions());
            //Parameters
            _settings = settings;
            MaxAttempt = _settings.NumOfAttempts;
            MaxAttemptEpoch = _settings.NumOfAttemptEpochs;
            Attempt = 1;
            AttemptEpoch = 0;
            _net = net;
            _gamma = _settings.Lambda * _settings.Alpha;
            return;
        }

        //Properties
        /// <summary>
        /// FF network beeing trained
        /// </summary>
        public INonRecurrentNetwork Net { get { return _net; } }

        //Methods
        /// <summary>
        /// Starts next training attempt
        /// </summary>
        public bool NextAttempt()
        {
            //Only one attempt makes the sense -> do nothhing and return false
            return false;
        }

        private double ComputeLinOutput(int dataRowIdx, double[] weights)
        {
            double output = weights[0];
            for (int i = 0; i < _net.NumOfInputValues; i++)
            {
                output += weights[i + 1] * _inputVectorCollection[dataRowIdx][i];
            }
            return output;
        }

        private double SoftThreshold(double x)
        {
            if (_gamma < Math.Abs(x))
            {
                if (x > 0)
                {
                    return x - _gamma;
                }
                else if (x < 0)
                {
                    return x + _gamma;
                }
            }
            return 0;
        }

        /// <summary>
        /// Performs training iteration.
        /// </summary>
        public bool Iteration()
        {
            //Primary stop condition
            if (AttemptEpoch == MaxAttemptEpoch)
            {
                return false;
            }
            //Next epoch allowed
            ++AttemptEpoch;
            InfoMessage = $"Lambda={_settings.Lambda.ToString(CultureInfo.InvariantCulture)} Alpha={_settings.Alpha.ToString(CultureInfo.InvariantCulture)}";
            //Whole network new weights buffer
            double[] newWeights = _net.GetWeightsCopy();
            //Optimization of the weights for each output separeatelly
            for (int outputIdx = 0; outputIdx < _net.NumOfOutputValues; outputIdx++)
            {
                //New weights for specific output neuron
                double[] weights = new double[_net.NumOfInputValues + 1];
                //Copy weights and compute the sum
                //Bias first
                weights[0] = newWeights[_net.NumOfOutputValues * _net.NumOfInputValues + outputIdx];
                //Inputs next
                Parallel.For(0, _net.NumOfInputValues, i =>
                {
                    weights[i + 1] = newWeights[outputIdx * _net.NumOfInputValues + i];
                });
                //Elastic iteration
                //Compute and store current output values and new bias
                double oldBias = weights[0];
                double newBias = 0;
                double[] parallelSubResults1 = new double[_parallelRanges.Count];
                double[] parallelSubResults2 = new double[_parallelRanges.Count];
                double[] computedOutputs = new double[_outputVectorCollection.Count];
                parallelSubResults1.Populate(0);
                Parallel.For(0, _parallelRanges.Count, rangeIdx =>
                {
                    for (int i = _parallelRanges[rangeIdx].Item1; i < _parallelRanges[rangeIdx].Item2; i++)
                    {
                        computedOutputs[i] = ComputeLinOutput(i, weights);
                        parallelSubResults1[rangeIdx] += (_outputVectorCollection[i][outputIdx] - computedOutputs[i] + oldBias);
                    }
                });
                //New bias finalization
                for (int i = 0; i < _parallelRanges.Count; i++)
                {
                    newBias += parallelSubResults1[i];
                }
                newBias /= _outputVectorCollection.Count;
                weights[0] = newBias;
                //Update computed outputs if bias has changed
                double biasDifference = newBias - oldBias;
                if (biasDifference != 0)
                {
                    Parallel.For(0, _parallelRanges.Count, rangeIdx =>
                    {
                        for (int i = _parallelRanges[rangeIdx].Item1; i < _parallelRanges[rangeIdx].Item2; i++)
                        {
                            computedOutputs[i] += biasDifference;
                        }
                    });
                }
                //Optimization
                for (int inputValueIdx = 0; inputValueIdx < _net.NumOfInputValues; inputValueIdx++)
                {
                    //Fit and denominator computation
                    double oldWeight = weights[1 + inputValueIdx];
                    double fit = 0, denominator = 0;
                    parallelSubResults1.Populate(0);
                    parallelSubResults2.Populate(0);
                    Parallel.For(0, _parallelRanges.Count, rangeIdx =>
                    {
                        for (int i = _parallelRanges[rangeIdx].Item1; i < _parallelRanges[rangeIdx].Item2; i++)
                        {
                            double x = _inputVectorCollection[i][inputValueIdx];
                            if (x != 0)
                            {
                                parallelSubResults1[rangeIdx] += x * (_outputVectorCollection[i][outputIdx] - computedOutputs[i] + x * oldWeight);
                                parallelSubResults2[rangeIdx] += x * x;
                            }
                        }
                    });
                    //Fit and denominator finalization
                    for (int i = 0; i < _parallelRanges.Count; i++)
                    {
                        fit += parallelSubResults1[i];
                        denominator += parallelSubResults2[i];
                    }
                    fit /= _outputVectorCollection.Count;
                    denominator /= _outputVectorCollection.Count;
                    denominator += _settings.Lambda * (1d - _settings.Alpha);
                    double newWeight = 0;
                    if (denominator != 0)
                    {
                        newWeight = SoftThreshold(fit) / denominator;
                    }
                    //Set new weight
                    weights[1 + inputValueIdx] = newWeight;
                    //Update computed values
                    double weightsDiff = newWeight - oldWeight;
                    if (weightsDiff != 0)
                    {
                        Parallel.For(0, _parallelRanges.Count, rangeIdx =>
                        {
                            for (int i = _parallelRanges[rangeIdx].Item1; i < _parallelRanges[rangeIdx].Item2; i++)
                            {
                                double x = _inputVectorCollection[i][inputValueIdx];
                                if (x != 0)
                                {
                                    computedOutputs[i] += weightsDiff * x;
                                }
                            }
                        });
                    }
                }
                //Put optimized weights back to newWaights buffer
                //Bias
                newWeights[_net.NumOfOutputValues * _net.NumOfInputValues + outputIdx] = weights[0];
                //Inputs
                for (int i = 0; i < _net.NumOfInputValues; i++)
                {
                    newWeights[outputIdx * _net.NumOfInputValues + i] = weights[i + 1];
                }
            }
            //Set new weights and compute final error
            _net.SetWeights(newWeights);
            MSE = _net.ComputeBatchErrorStat(_inputVectorCollection, _outputVectorCollection).MeanSquare;
            return true;
        }

    }//ElasticRegrTrainer

}//Namespace

