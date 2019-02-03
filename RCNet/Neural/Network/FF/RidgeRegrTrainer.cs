using System;
using System.Collections.Generic;
using System.Globalization;
using RCNet.Extensions;
using RCNet.Neural.Activation;
using RCNet.MathTools.MatrixMath;
using RCNet.MathTools.VectorMath;
using RCNet.MathTools.PS;

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Implements the ridge regression trainer.
    /// FF network has to have only output layer with the Identity activation.
    /// </summary>
    [Serializable]
    public class RidgeRegrTrainer : INonRecurrentNetworkTrainer
    {
        //Constants
        private const double StopLambdaDifference = 1e-10;
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
        private RidgeRegrTrainerSettings _settings;
        private FeedForwardNetwork _net;
        private readonly List<double[]> _inputVectorCollection;
        private readonly List<double[]> _outputVectorCollection;
        private readonly Random _rand;
        private readonly Matrix _baseSquareMatrix;
        private readonly Matrix _transposedPredictorsMatrix;
        private List<Vector> _outputSingleColVectorCollection;
        private ParamSeeker _lambdaSeeker;
        private double _currLambda;

        //Constructor
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="net">FF network to be trained</param>
        /// <param name="inputVectorCollection">Predictors (input)</param>
        /// <param name="outputVectorCollection">Ideal outputs (the same number of rows as number of inputs)</param>
        /// <param name="settings">Optional startup parameters of the trainer</param>
        /// <param name="rand">Random object to be used</param>
        public RidgeRegrTrainer(FeedForwardNetwork net,
                                List<double[]> inputVectorCollection,
                                List<double[]> outputVectorCollection,
                                RidgeRegrTrainerSettings settings,
                                Random rand
                                )
        {
            //Check network readyness
            if (!net.Finalized)
            {
                throw new Exception("Can´t create trainer. Network structure was not finalized.");
            }
            //Check network conditions
            if (net.LayerCollection.Count != 1 || !(net.LayerCollection[0].Activation is Identity))
            {
                throw new Exception("Can´t create trainer. Network structure is not complient (single layer having Identity activation).");
            }
            //Check samples conditions
            if(inputVectorCollection.Count == 0)
            {
                throw new Exception("Can´t create trainer. Missing training samples.");
            }
            //Collections
            _inputVectorCollection = new List<double[]>(inputVectorCollection);
            _outputVectorCollection = new List<double[]>(outputVectorCollection);
            //Parameters
            _settings = settings;
            MaxAttempt = _settings.NumOfAttempts;
            MaxAttemptEpoch = _settings.NumOfAttemptEpochs;
            Attempt = 1;
            AttemptEpoch = 0;
            _net = net;
            _rand = rand;
            _outputSingleColVectorCollection = new List<Vector>(_net.NumOfOutputValues);
            for (int outputIdx = 0; outputIdx < _net.NumOfOutputValues; outputIdx++)
            {
                Vector outputSingleColVector = new Vector(outputVectorCollection.Count);
                for (int row = 0; row < outputVectorCollection.Count; row++)
                {
                    //Output
                    outputSingleColVector.Data[row] = outputVectorCollection[row][outputIdx];
                }
                _outputSingleColVectorCollection.Add(outputSingleColVector);
            }
            //Lambda seeker
            _lambdaSeeker = new ParamSeeker(_settings.LambdaSeekerCfg);
            _currLambda = 1e6;
            //Matrix setup
            Matrix predictorsMatrix = new Matrix(inputVectorCollection.Count, _net.NumOfInputValues + 1);
            for (int row = 0; row < inputVectorCollection.Count; row++)
            {
                //Predictors
                for (int col = 0; col < _net.NumOfInputValues; col++)
                {
                    predictorsMatrix.Data[row][col] = inputVectorCollection[row][col];
                }
                //Add constant bias to predictors
                predictorsMatrix.Data[row][_net.NumOfInputValues] = 1;
            }
            _transposedPredictorsMatrix = predictorsMatrix.Transpose();
            _baseSquareMatrix = _transposedPredictorsMatrix * predictorsMatrix;
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
            //New lambda to be tested
            double newLambda = _lambdaSeeker.Next;
            //Secondary stop condition
            if (Math.Abs(_currLambda - newLambda) < StopLambdaDifference)
            {
                return false;
            }
            //Next epoch allowed
            _currLambda = newLambda;
            ++AttemptEpoch;
            InfoMessage = $"lambda={_currLambda.ToString(CultureInfo.InvariantCulture)}";
            //Copy of base squared matrix
            Matrix tmpMatrix = new Matrix(_baseSquareMatrix);
            //Apply lambda
            tmpMatrix.AddScalarToDiagonal(_currLambda);
            //Inverse
            tmpMatrix.Inverse();
            //Ridge regression matrix
            Matrix regrMatrix = tmpMatrix * _transposedPredictorsMatrix;
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
            MSE = _net.ComputeBatchErrorStat(_inputVectorCollection, _outputVectorCollection).MeanSquare;
            //Update lambda seeker
            _lambdaSeeker.ProcessError(MSE);
            return true;
        }

    }//RidgeRegrTrainer

}//Namespace

