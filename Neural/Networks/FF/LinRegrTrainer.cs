using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using OKOSW.Extensions;
using OKOSW.Neural.Activation;
using OKOSW.MathTools.MatrixMath;

namespace OKOSW.Neural.Networks.FF
{
    /// <summary>
    /// Startup parameters for linear regression trainer
    /// </summary>
    [Serializable]
    public class LinRegrParameters
    {
        //Constants
        public const double DefaultStartingNoiseIntensity = 0.01;
        public const double DeafaultMaxExpArgument = 30;

        //Attributes
        public double StartingNoiseIntensity { get; set; } = DefaultStartingNoiseIntensity;
        public double MaxExpArgument { get; set; } = DeafaultMaxExpArgument;

    }//LinRegrParameters

    /// <summary>
    /// Implements linear regression trainer.
    /// Principle is to add each iteration less and less piece of white-noise to predictors and then perform linear regression.
    /// This technique allows to find more stable weight solution (according to testing samples) than just a linear regression
    /// of pure predictors.
    /// Trainee Basic network has to have only output layer and Identity activation.
    /// </summary>
    [Serializable]
    public class LinRegrTrainer : IBasicTrainer
    {
        //Attributes
        private LinRegrParameters _parameters;
        private BasicNetwork _net;
        private List<double[]> _trainInputs;
        private List<double[]> _trainIdealOutputs;
        private List<Matrix> _regrIdealOutputs;
        private Random _rand;
        private double[] _alphas;
        private double _mse;
        private int _maxEpoch;
        private int _epoch;

        //Constructor
        /// <summary>
        /// Constructs new instance of linear regression trainer
        /// </summary>
        /// <param name="net">Basic network to be trained</param>
        /// <param name="inputs">Predictors (input)</param>
        /// <param name="outputs">Ideal outputs (the same rows count as inputs)</param>
        /// <param name="maxEpoch">Maximum allowed training epochs</param>
        /// <param name="rand">Random object to be used for adding white-noise to predictors</param>
        /// <param name="parameters">Optional startup parameters of the trainer</param>
        public LinRegrTrainer(BasicNetwork net, List<double[]> inputs, List<double[]> outputs, int maxEpoch, Random rand, LinRegrParameters parameters = null)
        {
            //Check network readyness
            if (!net.Finalized)
            {
                throw new Exception("Can´t create LinRegr trainer. Network structure was not finalized.");
            }
            //Check network conditions
            if (net.Layers.Count != 1 || !(net.Layers[0].Activation is IdentityAF))
            {
                throw new Exception("Can´t create LinRegr trainer. Network structure is not complient (single layer having Identity activation).");
            }
            //Check samples conditions
            if(inputs.Count < inputs[0].Length + 1)
            {
                throw new Exception("Can´t create LinRegr trainer. Insufficient number of training samples. Minimum is " + (inputs[0].Length + 1).ToString() + ".");
            }
            //Parameters
            _parameters = parameters;
            if (_parameters == null)
            {
                //Default parameters
                _parameters = new LinRegrParameters();
            }
            _net = net;
            _trainInputs = inputs;
            _trainIdealOutputs = outputs;
            _regrIdealOutputs = new List<Matrix>(_net.OutputValuesCount);
            for (int outputIdx = 0; outputIdx < _net.OutputValuesCount; outputIdx++)
            {
                Matrix regrOutputs = new Matrix(_trainInputs.Count, 1);
                for (int row = 0; row < _trainInputs.Count; row++)
                {
                    //Output
                    regrOutputs.Data[row][0] = _trainIdealOutputs[row][outputIdx];
                }
                _regrIdealOutputs.Add(regrOutputs);
            }
            _rand = rand;
            _maxEpoch = maxEpoch;
            _epoch = 0;
            _alphas = new double[_maxEpoch];
            //Plan iterations alphas
            double coeff = (maxEpoch > 1)? _parameters.MaxExpArgument / (maxEpoch - 1) : 0;
            for (int i = 0; i < _maxEpoch - 1; i++)
            {
                _alphas[i] = _parameters.StartingNoiseIntensity * (1d/(Math.Exp((i - 1) * coeff)));
                _alphas[i] = Math.Max(0, _alphas[i]);
            }
            //Ensure last alpha is zero
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
        /// Trainee FF BasicNetwork
        /// </summary>
        public BasicNetwork Net { get { return _net; } }

        //Methods
        private Matrix PreparePredictors(double noiseIntensity)
        {
            Matrix predictors = new Matrix(_trainInputs.Count, _net.InputValuesCount + 1);
            for (int row = 0; row < _trainInputs.Count; row++)
            {
                //Predictors
                for(int col = 0; col < _net.InputValuesCount; col++)
                {
                    double predictor = _trainInputs[row][col];
                    predictors.Data[row][col] = predictor * (1d + _rand.NextBoundedUniformDoubleRS(0, noiseIntensity));
                }
                //Add bias to predictors
                predictors.Data[row][_net.InputValuesCount] = 1;
            }
            return predictors;
        }


        /// <summary>
        /// Performs training iteration.
        /// </summary>
        public void Iteration()
        {
            //----------------------------------------------------
            //Next epoch
            ++_epoch;
            //----------------------------------------------------
            //Noise intensity
            double intensity = _alphas[Math.Min(_maxEpoch, _epoch) - 1];
            //Adjusted predictors
            Matrix predictors = PreparePredictors((double)intensity);
            //Decomposition
            QRD decomposition = new QRD(predictors);
            //New waights
            double[] newWaights = new double[_net.FlatWeights.Length];
            //Regression for each output neuron
            for (int outputIdx = 0; outputIdx < _net.OutputValuesCount; outputIdx++)
            {
                //Regression
                Matrix solution = decomposition.Solve(_regrIdealOutputs[outputIdx]);
                //Store weights
                //Input weights
                for (int i = 0; i < solution.RowsCount - 1; i++)
                {
                    newWaights[outputIdx * _net.InputValuesCount + i] = solution.Data[i][0];
                }
                //Bias weight
                newWaights[_net.OutputValuesCount * _net.InputValuesCount + outputIdx] = solution.Data[solution.RowsCount - 1][0];
            }
            //Set new weights and compute error
            _net.SetWeights(newWaights);
            _mse = _net.ComputeBatchErrorStat(_trainInputs, _trainIdealOutputs).MeanSquare;
            return;
        }


    }//LinRegrTrainer

}//Namespace
