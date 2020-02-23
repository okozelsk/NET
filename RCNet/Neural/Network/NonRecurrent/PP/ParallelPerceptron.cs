using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;

namespace RCNet.Neural.Network.NonRecurrent.PP
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

        //Static members
        /// <summary>
        /// Output range
        /// </summary>
        public static Interval PPOutputRange = new Interval(-1d, 1d);

        //Attribute properties
        /// <summary>
        /// Number of network's input values
        /// </summary>
        public int NumOfInputValues { get; }

        /// <summary>
        /// Number of network's treshold gates
        /// </summary>
        public int Gates { get; }

        /// <summary>
        /// Network's output resolution
        /// </summary>
        public int Resolution { get; }

        /// <summary>
        /// Output squash coeeficient (= Resolution / 2)
        /// </summary>
        public double ResSquashCoeff { get; }

        //Attributes
        private readonly int _numOfGateWeights;
        private readonly double[] _flatWeights;


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="numOfInputs">Number of input values</param>
        /// <param name="gates">Number of parallel treshold gates</param>
        /// <param name="resolution">Requiered output resolution (number of distinct values)</param>
        public ParallelPerceptron(int numOfInputs, int gates, int resolution)
        {
            NumOfInputValues = numOfInputs;
            if (NumOfInputValues < 1)
            {
                throw new ArgumentException($"Invalid number of input values {numOfInputs}", "numOfInputs");
            }
            Gates = gates;
            if(Gates < 1)
            {
                throw new ArgumentException($"Invalid number of gates {gates}", "gates");
            }
            Resolution = resolution;
            if (Resolution < 2 || Resolution > Gates * 2)
            {
                throw new ArgumentException($"Invalid resolution {resolution}. Resolution must be GE 2 and LE to (gates * 2).", "resolution");
            }
            ResSquashCoeff = (double)Resolution / 2d;
            _numOfGateWeights = (numOfInputs + 1);
            _flatWeights = new double[Gates * _numOfGateWeights];
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="numOfInputs">Number of input values</param>
        /// <param name="settings">Configuration parameters</param>
        public ParallelPerceptron(int numOfInputs, ParallelPerceptronSettings settings)
            :this(numOfInputs, settings.Gates, settings.Resolution)
        {
            return;
        }

        //Properties
        /// <summary>
        /// Number of network's output values
        /// </summary>
        public int NumOfOutputValues { get { return 1; } }

        /// <summary>
        /// Total number of network's weights
        /// </summary>
        public int NumOfWeights { get { return _flatWeights.Length; } }

        /// <summary>
        /// Output range of the output layer
        /// </summary>
        public Interval OutputRange { get { return PPOutputRange; } }

        //Methods
        //Static methods

        //Instance methods
        private double ComputeGate(int gateIdx, double[] input, out double sum)
        {
            int weightsStartFlatIdx = gateIdx * _numOfGateWeights;
            //Bias
            sum = _flatWeights[weightsStartFlatIdx + NumOfInputValues] * BiasValue;
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
            if (sum < -ResSquashCoeff)
            {
                output[0] = -1;
            }
            else if (sum > ResSquashCoeff)
            {
                output[0] = 1;
            }
            else
            {
                output[0] = sum / ResSquashCoeff;
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
            for (int i = 0; i < Gates; i++)
            {
                sum += ComputeGate(i, input, out _);
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
            for(int i = 0; i < Gates; i++)
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
            double[] flatErrors = new double[inputCollection.Count * NumOfOutputValues];
            double[][] computedOutputs = new double[idealOutputCollection.Count][];
            Parallel.For(0, inputCollection.Count, row =>
            {
                double[] computedOutputVector = Compute(inputCollection[row]);
                computedOutputs[row] = computedOutputVector;
                for (int i = 0; i < NumOfOutputValues; i++)
                {
                    double error = idealOutputCollection[row][i] - computedOutputVector[i];
                    flatErrors[row * NumOfOutputValues + i] = Math.Abs(error);
                }
            });
            computedOutputCollection = new List<double[]>(computedOutputs);
            return new BasicStat(flatErrors);
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
            double[] flatErrors = new double[inputCollection.Count * NumOfOutputValues];
            Parallel.For(0, inputCollection.Count, row =>
            {
                double[] computedOutputVector = Compute(inputCollection[row]);
                for (int i = 0; i < NumOfOutputValues; i++)
                {
                    double error = idealOutputCollection[row][i] - computedOutputVector[i];
                    flatErrors[row * NumOfOutputValues + i] = Math.Abs(error);
                }
            });
            return new BasicStat(flatErrors);
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
        public void RandomizeWeights(Random rand)
        {
            //Random values
            rand.FillUniform(_flatWeights, -1, 1, false);
            NormalizeWeights();
            return;
        }

        /// <summary>
        /// Normalizes internal weights
        /// </summary>
        public void NormalizeWeights()
        {
            Parallel.For(0, Gates, gateIdx =>
            {
                int weightFlatIdx = gateIdx * (NumOfInputValues + 1);
                double norm = 0;
                for (int i = 0; i < NumOfInputValues + 1; i++)
                {
                    norm += _flatWeights[weightFlatIdx + i].Power(2);
                }
                norm = Math.Sqrt(norm);
                if (norm > 0)
                {
                    for (int i = 0; i < NumOfInputValues + 1; i++)
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
            ParallelPerceptron clone = new ParallelPerceptron(NumOfInputValues, Gates, Resolution);
            clone.SetWeights(_flatWeights);
            return clone;
        }

    }//ParallelPerceptron

}//Namespace
