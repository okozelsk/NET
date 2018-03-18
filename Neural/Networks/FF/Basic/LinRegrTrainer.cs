using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.CompilerServices;
using OKOSW.Extensions;
using OKOSW.Neural.Activation;
using OKOSW.MathTools.MatrixMath;

namespace OKOSW.Neural.Networks.FF.Basic
{
    /// <summary>
    /// Startup parameters for linear regression trainer
    /// </summary>
    [Serializable]
    public class LinRegrParameters
    {
        //Constants
        public const double DEFAULT_START_NOISE_INTENSITY = 0.01;
        public const double DEFAULT_MAX_EXP_ARG = 30;

        //Attributes
        public double StartNoiseIntensity { get; set; } = DEFAULT_START_NOISE_INTENSITY;
        public double MaxExpArg { get; set; } = DEFAULT_MAX_EXP_ARG;

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
        private LinRegrParameters m_parameters;
        private BasicNetwork m_net;
        private List<double[]> m_trainInputs;
        private List<double[]> m_trainIdealOutputs;
        private List<Matrix> m_regrIdealOutputs;
        private Random m_rand;
        private double[] m_alphas;
        private double m_MSE;
        private int m_maxEpoch;
        private int m_epoch;

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
            m_parameters = parameters;
            if (m_parameters == null)
            {
                //Default parameters
                m_parameters = new LinRegrParameters();
            }
            m_net = net;
            m_trainInputs = inputs;
            m_trainIdealOutputs = outputs;
            m_regrIdealOutputs = new List<Matrix>(m_net.OutputValuesCount);
            for (int outputIdx = 0; outputIdx < m_net.OutputValuesCount; outputIdx++)
            {
                Matrix regrOutputs = new Matrix(m_trainInputs.Count, 1);
                for (int row = 0; row < m_trainInputs.Count; row++)
                {
                    //Output
                    regrOutputs.Data[row][0] = m_trainIdealOutputs[row][outputIdx];
                }
                m_regrIdealOutputs.Add(regrOutputs);
            }
            m_rand = rand;
            m_maxEpoch = maxEpoch;
            m_epoch = 0;
            m_alphas = new double[m_maxEpoch];
            //Plan iterations alphas
            double coeff = (maxEpoch > 1)? m_parameters.MaxExpArg / (maxEpoch - 1) : 0;
            for (int i = 0; i < m_maxEpoch - 1; i++)
            {
                m_alphas[i] = m_parameters.StartNoiseIntensity * (1d/(Math.Exp((i - 1) * coeff)));
                m_alphas[i] = Math.Max(0, m_alphas[i]);
            }
            //Ensure last alpha is zero
            m_alphas[m_maxEpoch - 1] = 0;
            return;
        }

        //Properties
        /// <summary>
        /// Epoch error (MSE).
        /// </summary>
        public double MSE { get { return m_MSE; } }
        /// <summary>
        /// Current epoch (incremented each call of Iteration)
        /// </summary>
        public int Epoch { get { return m_epoch; } }
        /// <summary>
        /// Trainee FF BasicNetwork
        /// </summary>
        public BasicNetwork Net { get { return m_net; } }

        //Methods
        private Matrix PreparePredictors(double noiseIntensity)
        {
            Matrix predictors = new Matrix(m_trainInputs.Count, m_net.InputValuesCount + 1);
            for (int row = 0; row < m_trainInputs.Count; row++)
            {
                //Predictors
                for(int col = 0; col < m_net.InputValuesCount; col++)
                {
                    double predictor = m_trainInputs[row][col];
                    predictors.Data[row][col] = predictor * (1d + m_rand.NextBoundedUniformDoubleRS(0, noiseIntensity));
                }
                //Add bias to predictors
                predictors.Data[row][m_net.InputValuesCount] = 1;
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
            ++m_epoch;
            //----------------------------------------------------
            //Noise intensity
            double intensity = m_alphas[Math.Min(m_maxEpoch, m_epoch) - 1];
            //Adjusted predictors
            Matrix predictors = PreparePredictors((double)intensity);
            //Decomposition
            QRD decomposition = new QRD(predictors);
            //New waights
            double[] newWaights = new double[m_net.FlatWeights.Length];
            //Regression for each output neuron
            for (int outputIdx = 0; outputIdx < m_net.OutputValuesCount; outputIdx++)
            {
                //Regression
                Matrix solution = decomposition.Solve(m_regrIdealOutputs[outputIdx]);
                //Store weights
                //Input weights
                for (int i = 0; i < solution.RowsCount - 1; i++)
                {
                    newWaights[outputIdx * m_net.InputValuesCount + i] = solution.Data[i][0];
                }
                //Bias weight
                newWaights[m_net.OutputValuesCount * m_net.InputValuesCount + outputIdx] = solution.Data[solution.RowsCount - 1][0];
            }
            //Set new weights and compute error
            m_net.SetWeights(newWaights);
            m_MSE = m_net.ComputeBatchErrorStat(m_trainInputs, m_trainIdealOutputs).MeanSquare;
            return;
        }


    }//LinRegrTrainer

}//Namespace
