using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Network.PP
{
    /// <summary>
    /// Class implements a parallel perceptron.
    /// </summary>
    [Serializable]
    public class ParallelPerceptron : INonRecurrentNetwork
    {
        //Constants
        /// <summary>
        /// Dummy bias input value
        /// </summary>
        public const double BiasValue = 1d;

        //Attributes
        private int _numOfInputs;
        private int _numOfGates;
        private int _numOfGateWeights;
        private double[] _flatWeights;
        private int _resolution;
        private double _resSquashCoeff;

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="numOfInputs">Number of input values</param>
        /// <param name="numOfGates">Number of parallel treshold gates</param>
        /// <param name="resolution">Requiered output resolution (number of distinct values)</param>
        public ParallelPerceptron(int numOfInputs, int numOfGates, int resolution)
        {
            _numOfInputs = numOfInputs;
            if (_numOfInputs < 1)
            {
                throw new ArgumentException($"Invalid number of input values {numOfInputs}", "numOfInputs");
            }
            _numOfGates = numOfGates;
            if(_numOfGates < 1)
            {
                throw new ArgumentException($"Invalid number of gates {numOfGates}", "numOfGates");
            }
            _resolution = resolution;
            if (_resolution < 2 || _resolution > _numOfGates * 2)
            {
                throw new ArgumentException($"Invalid resolution {resolution}. Resolution must be GE 2 and LE to (number of gates * 2).", "resolution");
            }
            _resSquashCoeff = (double)_resolution / 2d;
            _numOfGateWeights = (numOfInputs + 1);
            _flatWeights = new double[_numOfGates * _numOfGateWeights];
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="numOfInputs">Number of input values</param>
        /// <param name="settings">Configuration parameters</param>
        public ParallelPerceptron(int numOfInputs, ParallelPerceptronSettings settings)
            :this(numOfInputs, settings.NumOfGates, settings.Resolution)
        {
            return;
        }

        //Properties
        /// <summary>
        /// Number of network's input values
        /// </summary>
        public int NumOfInputValues { get { return _numOfInputs; } }

        /// <summary>
        /// Number of network's treshold gates
        /// </summary>
        public int NumOfGates { get { return _numOfGates; } }

        /// <summary>
        /// Network's output resolution
        /// </summary>
        public int Resolution { get { return _resolution; } }

        /// <summary>
        /// Output squash coeeficient (= Resolution / 2)
        /// </summary>
        public double ResSquashCoeff { get { return _resSquashCoeff; } }

        /// <summary>
        /// Number of network's output values
        /// </summary>
        public int NumOfOutputValues { get { return 1; } }

        /// <summary>
        /// Total number of network's weights
        /// </summary>
        public int NumOfWeights { get { return _flatWeights.Length; } }

        //Methods
        //Static methods

        //Instance methods
        private double ComputeGate(int gateIdx, double[] input, out double sum)
        {
            int weightsStartFlatIdx = gateIdx * _numOfGateWeights;
            //Bias
            sum = _flatWeights[weightsStartFlatIdx + _numOfInputs] * BiasValue;
            //Inputs
            for(int i = 0, wIdx = weightsStartFlatIdx; i < input.Length; i++, wIdx++)
            {
                sum += _flatWeights[wIdx] * input[i];
            }
            return sum >= 0 ? 1 : -1;
        }
        
        private double[] ComputeResult(double sum)
        {
            double[] output = new double[1];
            if (sum < -_resSquashCoeff)
            {
                output[0] = -1;
            }
            else if (sum > _resSquashCoeff)
            {
                output[0] = 1;
            }
            else
            {
                output[0] = sum / _resSquashCoeff;
            }
            return output;
        }

        /// <summary>
        /// Computes network output values
        /// </summary>
        /// <param name="input">Input values to be passed into the network</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] input)
        {
            double sum = 0;
            double gateSum;
            for (int i = 0; i < _numOfGates; i++)
            {
                sum += ComputeGate(i, input, out gateSum);
            }
            return ComputeResult(sum);
        }

        /// <summary>
        /// Computes network output values
        /// </summary>
        /// <param name="input">Input values to be passed into the network</param>
        /// <param name="gateSums">Sums computed by treshold gates. Array must be allocated to NumOfGates size</param>
        /// <returns>Computed output values</returns>
        public double[] Compute(double[] input, double[] gateSums)
        {
            double sum = 0;
            for(int i = 0; i < _numOfGates; i++)
            {
                sum += ComputeGate(i, input, out gateSums[i]);
            }
            return ComputeResult(sum);
        }

        /// <summary>
        /// Function goes through collection (batch) of the network inputs and for each of them computes the output.
        /// Computed output is then compared with a corresponding ideal output.
        /// The error Abs(ideal - computed) is passed to the result error statistics.
        /// </summary>
        /// <param name="inputCollection">Collection of the network inputs (batch)</param>
        /// <param name="idealOutputCollection">Collection of the ideal outputs (batch)</param>
        /// <param name="computedOutputCollection">Collection of the computed outputs (batch)</param>
        /// <returns>Error statistics</returns>
        public BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection, out List<double[]> computedOutputCollection)
        {
            BasicStat errStat = new BasicStat();
            double[][] computedOutputs = new double[idealOutputCollection.Count][];
            Parallel.For(0, inputCollection.Count, row =>
            {
                double[] computedOutputVector = Compute(inputCollection[row]);
                computedOutputs[row] = computedOutputVector;
                for (int i = 0; i < 1; i++)
                {
                    double error = idealOutputCollection[row][i] - computedOutputVector[i];
                    errStat.AddSampleValue(Math.Abs(error));
                }
            });
            computedOutputCollection = new List<double[]>(computedOutputs);
            return errStat;
        }

        /// <summary>
        /// Function goes through collection (batch) of the network inputs and for each of them computes the output.
        /// Computed output is then compared with a corresponding ideal output.
        /// The error Abs(ideal - computed) is passed to the result error statistics.
        /// </summary>
        /// <param name="inputCollection">Collection of the network inputs (batch)</param>
        /// <param name="idealOutputCollection">Collection of the ideal outputs (batch)</param>
        /// <returns>Error statistics</returns>
        public BasicStat ComputeBatchErrorStat(List<double[]> inputCollection, List<double[]> idealOutputCollection)
        {
            BasicStat errStat = new BasicStat();
            Parallel.For(0, inputCollection.Count, row =>
            {
                double[] computedOutputVector = Compute(inputCollection[row]);
                for (int i = 0; i < 1; i++)
                {
                    double error = idealOutputCollection[row][i] - computedOutputVector[i];
                    errStat.AddSampleValue(Math.Abs(error));
                }
            });
            return errStat;
        }

        /// <summary>
        /// Returns copy of all network internal weights (in a flat format)
        /// </summary>
        public double[] GetWeights()
        {
            return (double[])_flatWeights.Clone();
        }

        /// <summary>
        /// Adopts the given weights.
        /// </summary>
        /// <param name="newFlatWeights">New weights to be adopted (in a flat format)</param>
        public void SetWeights(double[] newFlatWeights)
        {
            newFlatWeights.CopyTo(_flatWeights, 0);
            return;
        }

        /// <summary>
        /// Function creates the statistics of the internal weights
        /// </summary>
        public BasicStat ComputeWeightsStat()
        {
            return (new BasicStat(_flatWeights));
        }

        /// <summary>
        /// Randomizes internal weights
        /// </summary>
        public void RandomizeWeights(System.Random rand)
        {
            //Random values
            rand.Fill(_flatWeights, -1, 1, false, RandomClassExtensions.DistributionType.Uniform);
            NormalizeWeights();
            return;
        }

        /// <summary>
        /// Normalizes internal weights
        /// </summary>
        public void NormalizeWeights()
        {
            Parallel.For(0, _numOfGates, gateIdx =>
            {
                int weightFlatIdx = gateIdx * (_numOfInputs + 1);
                double norm = 0;
                for (int i = 0; i < _numOfInputs + 1; i++)
                {
                    norm += _flatWeights[weightFlatIdx + i].Power(2);
                }
                norm = Math.Sqrt(norm);
                if (norm > 0)
                {
                    for (int i = 0; i < _numOfInputs + 1; i++)
                    {
                        _flatWeights[weightFlatIdx + i] /= norm;
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Creates a deep copy
        /// </summary>
        public INonRecurrentNetwork DeepClone()
        {
            ParallelPerceptron clone = new ParallelPerceptron(_numOfInputs, _numOfGates, _resolution);
            clone.SetWeights(_flatWeights);
            return clone;
        }

    }//ParallelPerceptron

}//Namespace
