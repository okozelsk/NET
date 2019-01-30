using System;
using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.MathTools.MatrixMath;
using RCNet.MathTools.VectorMath;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Implements the linear regression trainer.
    /// Principle is to add each iteration less and less piece of white-noise to predictors
    /// and then perform the standard linear regression.
    /// This technique allows to find more stable weight solution than just a linear regression
    /// of pure predictors.
    /// FF network has to have only output layer with the Identity activation.
    /// </summary>
    [Serializable]
    public class LinRegrTrainer : INonRecurrentNetworkTrainer
    {
        //Attributes
        private LinRegrTrainerSettings _settings;
        private FeedForwardNetwork _net;
        private List<double[]> _inputVectorCollection;
        private List<double[]> _outputVectorCollection;
        private List<Vector> _outputSingleColVectorCollection;
        private Random _rand;
        private readonly double[] _alphas;
        private double _mse;
        private readonly int _maxEpoch;
        private int _epoch;

        //Constructor
        /// <summary>
        /// Constructs new instance of linear regression trainer
        /// </summary>
        /// <param name="net">FF network to be trained</param>
        /// <param name="inputVectorCollection">Predictors (input)</param>
        /// <param name="outputVectorCollection">Ideal outputs (the same number of rows as number of inputs)</param>
        /// <param name="maxEpoch">Maximum allowed training epochs</param>
        /// <param name="rand">Random object to be used for adding a white-noise to predictors</param>
        /// <param name="settings">Optional startup parameters of the trainer</param>
        public LinRegrTrainer(FeedForwardNetwork net,
                              List<double[]> inputVectorCollection,
                              List<double[]> outputVectorCollection,
                              int maxEpoch,
                              Random rand,
                              LinRegrTrainerSettings settings = null
                              )
        {
            //Check network readyness
            if (!net.Finalized)
            {
                throw new Exception("Can´t create LinRegr trainer. Network structure was not finalized.");
            }
            //Check network conditions
            if (net.LayerCollection.Count != 1 || !(net.LayerCollection[0].Activation is Identity))
            {
                throw new Exception("Can´t create LinRegr trainer. Network structure is not complient (single layer having Identity activation).");
            }
            //Check samples conditions
            if(inputVectorCollection.Count < inputVectorCollection[0].Length + 1)
            {
                throw new Exception("Can´t create LinRegr trainer. Insufficient number of training samples. Minimum is " + (inputVectorCollection[0].Length + 1).ToString() + ".");
            }
            //Parameters
            _settings = settings;
            if (_settings == null)
            {
                //Default parameters
                _settings = new LinRegrTrainerSettings();
            }
            _net = net;
            _inputVectorCollection = inputVectorCollection;
            _outputVectorCollection = outputVectorCollection;
            _outputSingleColVectorCollection = new List<Vector>(_net.NumOfOutputValues);
            for (int outputIdx = 0; outputIdx < _net.NumOfOutputValues; outputIdx++)
            {
                Vector outputSingleColVector = new Vector(_outputVectorCollection.Count);
                for (int row = 0; row < _outputVectorCollection.Count; row++)
                {
                    //Output
                    outputSingleColVector.Data[row] = _outputVectorCollection[row][outputIdx];
                }
                _outputSingleColVectorCollection.Add(outputSingleColVector);
            }
            _rand = rand;
            _maxEpoch = maxEpoch;
            _epoch = 0;
            _alphas = new double[_maxEpoch];
            //Plan the iterations alphas
            double coeff = (maxEpoch > 1) ? _settings.MaxStretch / (maxEpoch - 1) : 0;
            for (int i = 0; i < _maxEpoch; i++)
            {
                _alphas[i] = _settings.HiNoiseIntensity - _settings.HiNoiseIntensity * Math.Tanh(i* coeff);
                _alphas[i] = Math.Max(0, _alphas[i]);
            }
            //Ensure the last alpha is zero
            _alphas[_maxEpoch - 1] = 0;
            return;
        }

        //Properties
        /// <summary>
        /// Epoch error (MSE).
        /// </summary>
        public double MSE { get { return _mse; } }
        /// <summary>
        /// Current epoch (incremented each call of Iteration)
        /// </summary>
        public int Epoch { get { return _epoch; } }
        /// <summary>
        /// FF network beeing trained
        /// </summary>
        public INonRecurrentNetwork Net { get { return _net; } }

        //Methods
        private Matrix PreparePredictors(double noiseIntensity)
        {
            Matrix predictors = new Matrix(_inputVectorCollection.Count, _net.NumOfInputValues + 1);
            for (int row = 0; row < _inputVectorCollection.Count; row++)
            {
                //Predictors
                for(int col = 0; col < _net.NumOfInputValues; col++)
                {
                    double predictor = _inputVectorCollection[row][col];
                    predictors.Data[row][col] = predictor * (1d + _rand.NextDouble(noiseIntensity * _settings.ZeroMargin, noiseIntensity, true, RandomClassExtensions.DistributionType.Uniform));
                }
                //Add constant bias to predictors
                predictors.Data[row][_net.NumOfInputValues] = 1;
            }
            return predictors;
        }


        /// <summary>
        /// Performs training iteration.
        /// </summary>
        public void Iteration()
        {
            //Next epoch
            ++_epoch;
            //Noise intensity
            double intensity = _alphas[Math.Min(_maxEpoch, _epoch) - 1];
            //Adjusted predictors
            Matrix predictors = PreparePredictors((double)intensity);
            //Ridge regression matrix
            Matrix regrMatrix = predictors.GetRidgeRegressionMatrix(0);
            //New weights
            double[] newWeights = new double[_net.NumOfWeights];
            //Weights for each output neuron
            for (int outputIdx = 0; outputIdx < _net.NumOfOutputValues; outputIdx++)
            {
                //Regression
                Vector weights = regrMatrix * _outputSingleColVectorCollection[outputIdx];
                //Store weights
                for (int i = 0; i < weights.Length - 1; i++)
                {
                    newWeights[outputIdx * _net.NumOfInputValues + i] = weights.Data[i];
                }
                //Bias weight
                newWeights[_net.NumOfOutputValues * _net.NumOfInputValues + outputIdx] = weights.Data[weights.Length - 1];
            }
            //Set new weights and compute error
            _net.SetWeights(newWeights);
            _mse = _net.ComputeBatchErrorStat(_inputVectorCollection, _outputVectorCollection).MeanSquare;
            return;
        }

    }//LinRegrTrainer

}//Namespace

