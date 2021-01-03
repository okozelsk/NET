using RCNet.Extensions;
using RCNet.MathTools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.NonRecurrent.PP
{
    /// <summary>
    /// Implements the parallel perceptron network.
    /// </summary>
    /// <remarks>
    /// The parallel perceptron network has only one output value, always between -1 and 1.
    /// </remarks>
    [Serializable]
    public class ParallelPerceptron : INonRecurrentNetwork
    {
        //Constants
        /// <summary>
        /// The dummy bias input.
        /// </summary>
        public const double BiasValue = 1d;

        //Attribute properties
        /// <summary>
        /// The number of network's input values.
        /// </summary>
        public int NumOfInputValues { get; }

        /// <summary>
        /// The number of network's threshold gates.
        /// </summary>
        public int Gates { get; }

        /// <summary>
        /// The network's output resolution.
        /// </summary>
        public int Resolution { get; }

        /// <summary>
        /// The output squash coefficient (= Resolution / 2).
        /// </summary>
        public double ResSquashCoeff { get; }

        //Attributes
        private readonly int _numOfGateWeights;
        private readonly double[] _flatWeights;


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfInputs">The number of input values.</param>
        /// <param name="gates">The number of network's threshold gates.</param>
        /// <param name="resolution">The network's output resolution (the number of distinct values).</param>
        public ParallelPerceptron(int numOfInputs, int gates, int resolution)
        {
            NumOfInputValues = numOfInputs;
            if (NumOfInputValues < 1)
            {
                throw new ArgumentException($"Invalid number of input values {numOfInputs}", "numOfInputs");
            }
            Gates = gates;
            if (Gates < 1)
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfInputs">The number of input values.</param>
        /// <param name="cfg">The configuration.</param>
        public ParallelPerceptron(int numOfInputs, ParallelPerceptronSettings cfg)
            : this(numOfInputs, cfg.Gates, cfg.Resolution)
        {
            return;
        }

        //Properties
        /// <inheritdoc/>
        public int NumOfOutputValues { get { return 1; } }

        /// <inheritdoc/>
        public int NumOfWeights { get { return _flatWeights.Length; } }

        /// <inheritdoc/>
        public Interval OutputRange { get { return Interval.IntN1P1; } }

        //Methods
        private double ComputeGate(int gateIdx, double[] input, out double sum)
        {
            int weightsStartFlatIdx = gateIdx * _numOfGateWeights;
            //Bias
            sum = _flatWeights[weightsStartFlatIdx + NumOfInputValues] * BiasValue;
            //Inputs
            for (int i = 0, wIdx = weightsStartFlatIdx; i < input.Length; i++, wIdx++)
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

        /// <inheritdoc/>
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
        /// Computes the network output values.
        /// </summary>
        /// <param name="input">The input values to be passed into the network.</param>
        /// <param name="gateSums">The sums computed by the threshold gates. Array must be allocated to size = number of gates.</param>
        /// <returns>The computed values.</returns>
        public double[] Compute(double[] input, double[] gateSums)
        {
            double sum = 0;
            for (int i = 0; i < Gates; i++)
            {
                sum += ComputeGate(i, input, out gateSums[i]);
            }
            return ComputeResult(sum);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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
        /// Returns the copy of all the network internal weights (in a flat format).
        /// </summary>
        public double[] GetWeights()
        {
            return (double[])_flatWeights.Clone();
        }

        /// <summary>
        /// Sets the internal weights.
        /// </summary>
        /// <param name="newFlatWeights">The new weights (in a flat format).</param>
        public void SetWeights(double[] newFlatWeights)
        {
            newFlatWeights.CopyTo(_flatWeights, 0);
            return;
        }

        /// <inheritdoc/>
        public BasicStat ComputeWeightsStat()
        {
            return (new BasicStat(_flatWeights));
        }

        /// <inheritdoc/>
        public void RandomizeWeights(Random rand)
        {
            //Random values
            rand.FillUniform(_flatWeights, -1, 1, false);
            NormalizeWeights();
            return;
        }

        /// <summary>
        /// Normalizes the internal weights.
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

        /// <inheritdoc/>
        public INonRecurrentNetwork DeepClone()
        {
            ParallelPerceptron clone = new ParallelPerceptron(NumOfInputValues, Gates, Resolution);
            clone.SetWeights(_flatWeights);
            return clone;
        }

    }//ParallelPerceptron

}//Namespace
