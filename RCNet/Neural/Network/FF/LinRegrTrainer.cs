﻿using System;
using System.Collections.Generic;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.MathTools.MatrixMath;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Startup parameters for the linear regression trainer
    /// </summary>
    [Serializable]
    public class LinRegrParameters
    {
        //Constants
        /// <summary>
        /// Default value of the starting noise intensity
        /// </summary>
        public const double DefaultStartingNoiseIntensity = 0.05;
        /// <summary>
        /// Default maximum exponent of E (Math.Exp)
        /// </summary>
        public const double DeafaultMaxExpArgument = 30;

        //Attributes
        /// <summary>
        /// Value of the starting noise intensity
        /// </summary>
        public double StartingNoiseIntensity { get; set; } = DefaultStartingNoiseIntensity;
        /// <summary>
        /// Maximum exponent of E (Math.Exp)
        /// </summary>
        public double MaxExpArgument { get; set; } = DeafaultMaxExpArgument;

    }//LinRegrParameters

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
        private LinRegrParameters _parameters;
        private FeedForwardNetwork _net;
        private List<double[]> _inputVectorCollection;
        private List<double[]> _outputVectorCollection;
        private List<Matrix> _outputSingleColMatrixCollection;
        private Random _rand;
        private double[] _alphas;
        private double _mse;
        private int _maxEpoch;
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
        /// <param name="parameters">Optional startup parameters of the trainer</param>
        public LinRegrTrainer(FeedForwardNetwork net,
                              List<double[]> inputVectorCollection,
                              List<double[]> outputVectorCollection,
                              int maxEpoch,
                              Random rand,
                              LinRegrParameters parameters = null
                              )
        {
            //Check network readyness
            if (!net.Finalized)
            {
                throw new Exception("Can´t create LinRegr trainer. Network structure was not finalized.");
            }
            //Check network conditions
            if (net.LayerCollection.Count != 1 || !(net.LayerCollection[0].Activation is IdentityAF))
            {
                throw new Exception("Can´t create LinRegr trainer. Network structure is not complient (single layer having Identity activation).");
            }
            //Check samples conditions
            if(inputVectorCollection.Count < inputVectorCollection[0].Length + 1)
            {
                throw new Exception("Can´t create LinRegr trainer. Insufficient number of training samples. Minimum is " + (inputVectorCollection[0].Length + 1).ToString() + ".");
            }
            //Parameters
            _parameters = parameters;
            if (_parameters == null)
            {
                //Default parameters
                _parameters = new LinRegrParameters();
            }
            _net = net;
            _inputVectorCollection = inputVectorCollection;
            _outputVectorCollection = outputVectorCollection;
            _outputSingleColMatrixCollection = new List<Matrix>(_net.NumOfOutputValues);
            for (int outputIdx = 0; outputIdx < _net.NumOfOutputValues; outputIdx++)
            {
                Matrix outputSingleColMatrix = new Matrix(_outputVectorCollection.Count, 1);
                for (int row = 0; row < _outputVectorCollection.Count; row++)
                {
                    //Output
                    outputSingleColMatrix.Data[row][0] = _outputVectorCollection[row][outputIdx];
                }
                _outputSingleColMatrixCollection.Add(outputSingleColMatrix);
            }
            _rand = rand;
            _maxEpoch = maxEpoch;
            _epoch = 0;
            _alphas = new double[_maxEpoch];
            //Plan the iterations alphas
            double coeff = (maxEpoch > 1)? _parameters.MaxExpArgument / (maxEpoch - 1) : 0;
            for (int i = 0; i < _maxEpoch - 1; i++)
            {
                _alphas[i] = _parameters.StartingNoiseIntensity * (1d/(Math.Exp((i - 1) * coeff)));
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
                    predictors.Data[row][col] = predictor * (1d + _rand.NextBoundedUniformDoubleRS(0, noiseIntensity));
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
            //Decomposition
            QRD decomposition = new QRD(predictors);
            //New waights
            double[] newWaights = new double[_net.NumOfWeights];
            //Regression for each output neuron
            for (int outputIdx = 0; outputIdx < _net.NumOfOutputValues; outputIdx++)
            {
                //Regression
                Matrix solution = decomposition.Solve(_outputSingleColMatrixCollection[outputIdx]);
                //Store weights
                //Input weights
                for (int i = 0; i < solution.NumOfRows - 1; i++)
                {
                    newWaights[outputIdx * _net.NumOfInputValues + i] = solution.Data[i][0];
                }
                //Bias weight
                newWaights[_net.NumOfOutputValues * _net.NumOfInputValues + outputIdx] = solution.Data[solution.NumOfRows - 1][0];
            }
            //Set new weights and compute error
            _net.SetWeights(newWaights);
            List<double[]> computedOutputCollection = null;
            _mse = _net.ComputeBatchErrorStat(_inputVectorCollection, _outputVectorCollection, out computedOutputCollection).MeanSquare;
            return;
        }

    }//LinRegrTrainer

}//Namespace

