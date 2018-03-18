using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OKOSW.Extensions;

namespace OKOSW.LibrariesTest
{
    public class EDUNetwork
    {
        private int m_numInput; // number input nodes
        private int m_numHidden;
        private int m_numOutput;

        private double[] m_inputs;
        private double[][] m_ihWeights; // input-hidden
        private double[] m_hBiases;
        private double[] m_hOutputs;

        private double[][] m_hoWeights; // hidden-output
        private double[] m_oBiases;
        private double[] m_outputs;

        private Random m_rand;

        public EDUNetwork(int numInput, int numHidden, int numOutput, int randomizerSeek = -1)
        {
            m_numInput = numInput;
            m_numHidden = numHidden;
            m_numOutput = numOutput;

            m_inputs = new double[numInput];

            m_ihWeights = MakeMatrix(numInput, numHidden, 0.0);
            m_hBiases = new double[numHidden];
            m_hOutputs = new double[numHidden];

            m_hoWeights = MakeMatrix(numHidden, numOutput, 0.0);
            m_oBiases = new double[numOutput];
            m_outputs = new double[numOutput];

            if (randomizerSeek == -1)
            {
                m_rand = new Random();
            }
            else
            {
                m_rand = new Random(randomizerSeek);
            }
            InitializeWeights(); // all weights and biases
        } // ctor

        private static double[][] MakeMatrix(int rows, int cols, double v) // helper for ctor, Train
        {
            double[][] result = new double[rows][];
            for (int r = 0; r < result.Length; ++r)
                result[r] = new double[cols];
            for (int i = 0; i < rows; ++i)
                for (int j = 0; j < cols; ++j)
                    result[i][j] = v;
            return result;
        }

        private static double[] MakeVector(int len, double v) // helper for Train
        {
            double[] result = new double[len];
            for (int i = 0; i < len; ++i)
                result[i] = v;
            return result;
        }

        private void InitializeWeights()
        {
            // initialize weights and biases to random values between 0.0001 and 0.001
            int numWeights = (m_numInput * m_numHidden) + (m_numHidden * m_numOutput) + m_numHidden + m_numOutput;
            double[] initialWeights = new double[numWeights];
            for (int i = 0; i < initialWeights.Length; ++i)
                initialWeights[i] = (0.001 - 0.0001) * m_rand.NextDouble() + 0.0001;
            SetWeights(initialWeights);
            return;
        }

        public double[] TrainRPROP(double[][] trainData, int maxEpochs) // using RPROP
        {
            // there is an accumulated gradient and a previous gradient for each weight and bias
            double[] hGradTerms = new double[m_numHidden]; // intermediate val for h-o weight gradients
            double[] oGradTerms = new double[m_numOutput]; // output gradients

            double[][] hoWeightGradsAcc = MakeMatrix(m_numHidden, m_numOutput, 0.0); // accumulated over all training data
            double[][] ihWeightGradsAcc = MakeMatrix(m_numInput, m_numHidden, 0.0);
            double[] oBiasGradsAcc = new double[m_numOutput];
            double[] hBiasGradsAcc = new double[m_numHidden];

            double[][] hoPrevWeightGradsAcc = MakeMatrix(m_numHidden, m_numOutput, 0.0); // accumulated, previous iteration
            double[][] ihPrevWeightGradsAcc = MakeMatrix(m_numInput, m_numHidden, 0.0);
            double[] oPrevBiasGradsAcc = new double[m_numOutput];
            double[] hPrevBiasGradsAcc = new double[m_numHidden];

            // must save previous weight deltas
            double[][] hoPrevWeightDeltas = MakeMatrix(m_numHidden, m_numOutput, 0.01);
            double[][] ihPrevWeightDeltas = MakeMatrix(m_numInput, m_numHidden, 0.01);
            double[] oPrevBiasDeltas = MakeVector(m_numOutput, 0.01);
            double[] hPrevBiasDeltas = MakeVector(m_numHidden, 0.01);

            double etaPlus = 1.2; // values are from the paper
            double etaMinus = 0.5;
            double deltaMax = 50.0;
            double deltaMin = 1.0E-6;

            int epoch = 0;
            while (epoch < maxEpochs)
            {
                ++epoch;

                if (epoch % 100 == 0)
                {
                    double[] currWts = GetWeights();
                    double rmse = Math.Sqrt(MeanSquaredError(trainData, currWts));
                    Console.Write("Epoch: " + epoch + " RMSE: " + rmse.ToString() + "\r");
                }

                // 1. compute and accumulate all gradients
                hoWeightGradsAcc.Populate(0); // zero-out values from prev iteration
                ihWeightGradsAcc.Populate(0);
                oBiasGradsAcc.Populate(0);
                hBiasGradsAcc.Populate(0);

                double[] xValues = new double[m_numInput]; // inputs
                double[] tValues = new double[m_numOutput]; // target values
                for (int row = 0; row < trainData.Length; ++row)  // walk thru all training data
                {
                    // no need to visit in random order because all rows processed before any updates ('batch')
                    Array.Copy(trainData[row], xValues, m_numInput); // get the inputs
                    Array.Copy(trainData[row], m_numInput, tValues, 0, m_numOutput); // get the target values
                    ComputeOutputs(xValues); // copy xValues in, compute outputs using curr weights (and store outputs internally)

                    // compute the h-o gradient term/component as in regular back-prop
                    // this term usually is lower case Greek delta but there are too many other deltas below
                    for (int i = 0; i < m_numOutput; ++i)
                    {
                        double derivative = (1 - m_outputs[i]) * (1 + m_outputs[i]); // derivative of tanh = (1 - y) * (1 + y)
                        //double derivative = (1 - m_outputs[i]) * m_outputs[i]; // derivative of softmax = (1 - y) * y (same as log-sigmoid)
                        oGradTerms[i] = derivative * (m_outputs[i] - tValues[i]); // careful with O-T vs. T-O, O-T is the most usual
                    }

                    // compute the i-h gradient term/component as in regular back-prop
                    for (int i = 0; i < m_numHidden; ++i)
                    {
                        double derivative = (1 - m_hOutputs[i]) * (1 + m_hOutputs[i]); // derivative of tanh = (1 - y) * (1 + y)
                        double sum = 0.0;
                        for (int j = 0; j < m_numOutput; ++j) // each hidden delta is the sum of numOutput terms
                        {
                            double x = oGradTerms[j] * m_hoWeights[i][j];
                            sum += x;
                        }
                        hGradTerms[i] = derivative * sum;
                    }

                    // add input to h-o component to make h-o weight gradients, and accumulate
                    for (int i = 0; i < m_numHidden; ++i)
                    {
                        for (int j = 0; j < m_numOutput; ++j)
                        {
                            double grad = oGradTerms[j] * m_hOutputs[i];
                            hoWeightGradsAcc[i][j] += grad;
                        }
                    }

                    // the (hidden-to-) output bias gradients
                    for (int i = 0; i < m_numOutput; ++i)
                    {
                        double grad = oGradTerms[i] * 1.0; // dummy input
                        oBiasGradsAcc[i] += grad;
                    }

                    // add input term to i-h component to make i-h weight gradients and accumulate
                    for (int i = 0; i < m_numInput; ++i)
                    {
                        for (int j = 0; j < m_numHidden; ++j)
                        {
                            double grad = hGradTerms[j] * m_inputs[i];
                            ihWeightGradsAcc[i][j] += grad;
                        }
                    }

                    // the (input-to-) hidden bias gradient
                    for (int i = 0; i < m_numHidden; ++i)
                    {
                        double grad = hGradTerms[i] * 1.0;
                        hBiasGradsAcc[i] += grad;
                    }
                } // each row
                  // end compute all gradients





                // update all weights and biases (in any order)

                // update input-hidden weights
                double delta = 0.0;

                for (int i = 0; i < m_numInput; ++i)
                {
                    for (int j = 0; j < m_numHidden; ++j)
                    {
                        if (ihPrevWeightGradsAcc[i][j] * ihWeightGradsAcc[i][j] > 0) // no sign change, increase delta
                        {
                            delta = ihPrevWeightDeltas[i][j] * etaPlus; // compute delta
                            if (delta > deltaMax) delta = deltaMax; // keep it in range
                            double tmp = -Math.Sign(ihWeightGradsAcc[i][j]) * delta; // determine direction and magnitude
                            m_ihWeights[i][j] += tmp; // update weights
                        }
                        else if (ihPrevWeightGradsAcc[i][j] * ihWeightGradsAcc[i][j] < 0) // grad changed sign, decrease delta
                        {
                            delta = ihPrevWeightDeltas[i][j] * etaMinus; // the delta (not used, but saved for later)
                            if (delta < deltaMin) delta = deltaMin; // keep it in range
                            m_ihWeights[i][j] -= ihPrevWeightDeltas[i][j]; // revert to previous weight
                            ihWeightGradsAcc[i][j] = 0; // forces next if-then branch, next iteration
                        }
                        else // this happens next iteration after 2nd branch above (just had a change in gradient)
                        {
                            delta = ihPrevWeightDeltas[i][j]; // no change to delta
                                                              // no way should delta be 0 . . . 
                            double tmp = -Math.Sign(ihWeightGradsAcc[i][j]) * delta; // determine direction
                            m_ihWeights[i][j] += tmp; // update
                        }
                        //Console.WriteLine(ihPrevWeightGradsAcc[i][j] + " " + ihWeightGradsAcc[i][j]); Console.ReadLine();

                        ihPrevWeightDeltas[i][j] = delta; // save delta
                        ihPrevWeightGradsAcc[i][j] = ihWeightGradsAcc[i][j]; // save the (accumulated) gradient
                    } // j
                } // i

                // update (input-to-) hidden biases
                for (int i = 0; i < m_numHidden; ++i)
                {
                    if (hPrevBiasGradsAcc[i] * hBiasGradsAcc[i] > 0) // no sign change, increase delta
                    {
                        delta = hPrevBiasDeltas[i] * etaPlus; // compute delta
                        if (delta > deltaMax) delta = deltaMax;
                        double tmp = -Math.Sign(hBiasGradsAcc[i]) * delta; // determine direction
                        m_hBiases[i] += tmp; // update
                    }
                    else if (hPrevBiasGradsAcc[i] * hBiasGradsAcc[i] < 0) // grad changed sign, decrease delta
                    {
                        delta = hPrevBiasDeltas[i] * etaMinus; // the delta (not used, but saved later)
                        if (delta < deltaMin) delta = deltaMin;
                        m_hBiases[i] -= hPrevBiasDeltas[i]; // revert to previous weight
                        hBiasGradsAcc[i] = 0; // forces next branch, next iteration
                    }
                    else // this happens next iteration after 2nd branch above (just had a change in gradient)
                    {
                        delta = hPrevBiasDeltas[i]; // no change to delta

                        if (delta > deltaMax) delta = deltaMax;
                        else if (delta < deltaMin) delta = deltaMin;
                        // no way should delta be 0 . . . 
                        double tmp = -Math.Sign(hBiasGradsAcc[i]) * delta; // determine direction
                        m_hBiases[i] += tmp; // update
                    }
                    hPrevBiasDeltas[i] = delta;
                    hPrevBiasGradsAcc[i] = hBiasGradsAcc[i];
                }

                // update hidden-to-output weights
                for (int i = 0; i < m_numHidden; ++i)
                {
                    for (int j = 0; j < m_numOutput; ++j)
                    {
                        if (hoPrevWeightGradsAcc[i][j] * hoWeightGradsAcc[i][j] > 0) // no sign change, increase delta
                        {
                            delta = hoPrevWeightDeltas[i][j] * etaPlus; // compute delta
                            if (delta > deltaMax) delta = deltaMax;
                            double tmp = -Math.Sign(hoWeightGradsAcc[i][j]) * delta; // determine direction
                            m_hoWeights[i][j] += tmp; // update
                        }
                        else if (hoPrevWeightGradsAcc[i][j] * hoWeightGradsAcc[i][j] < 0) // grad changed sign, decrease delta
                        {
                            delta = hoPrevWeightDeltas[i][j] * etaMinus; // the delta (not used, but saved later)
                            if (delta < deltaMin) delta = deltaMin;
                            m_hoWeights[i][j] -= hoPrevWeightDeltas[i][j]; // revert to previous weight
                            hoWeightGradsAcc[i][j] = 0; // forces next branch, next iteration
                        }
                        else // this happens next iteration after 2nd branch above (just had a change in gradient)
                        {
                            delta = hoPrevWeightDeltas[i][j]; // no change to delta
                                                              // no way should delta be 0 . . . 
                            double tmp = -Math.Sign(hoWeightGradsAcc[i][j]) * delta; // determine direction
                            m_hoWeights[i][j] += tmp; // update
                        }
                        hoPrevWeightDeltas[i][j] = delta; // save delta
                        hoPrevWeightGradsAcc[i][j] = hoWeightGradsAcc[i][j]; // save the (accumulated) gradients
                    } // j
                } // i

                // update (hidden-to-) output biases
                for (int i = 0; i < m_numOutput; ++i)
                {
                    if (oPrevBiasGradsAcc[i] * oBiasGradsAcc[i] > 0) // no sign change, increase delta
                    {
                        delta = oPrevBiasDeltas[i] * etaPlus; // compute delta
                        if (delta > deltaMax) delta = deltaMax;
                        double tmp = -Math.Sign(oBiasGradsAcc[i]) * delta; // determine direction
                        m_oBiases[i] += tmp; // update
                    }
                    else if (oPrevBiasGradsAcc[i] * oBiasGradsAcc[i] < 0) // grad changed sign, decrease delta
                    {
                        delta = oPrevBiasDeltas[i] * etaMinus; // the delta (not used, but saved later)
                        if (delta < deltaMin) delta = deltaMin;
                        m_oBiases[i] -= oPrevBiasDeltas[i]; // revert to previous weight
                        oBiasGradsAcc[i] = 0; // forces next branch, next iteration
                    }
                    else // this happens next iteration after 2nd branch above (just had a change in gradient)
                    {
                        delta = oPrevBiasDeltas[i]; // no change to delta
                                                    // no way should delta be 0 . . . 
                        double tmp = -Math.Sign(hBiasGradsAcc[i]) * delta; // determine direction
                        m_oBiases[i] += tmp; // update
                    }
                    oPrevBiasDeltas[i] = delta;
                    oPrevBiasGradsAcc[i] = oBiasGradsAcc[i];
                }
            } // while

            double[] wts = GetWeights();
            return wts;
        } // Train

        public void SetWeights(double[] weights)
        {
            // copy weights and biases in weights[] array to i-h weights, i-h biases, h-o weights, h-o biases
            int numWeights = (m_numInput * m_numHidden) + (m_numHidden * m_numOutput) + m_numHidden + m_numOutput;
            if (weights.Length != numWeights)
                throw new Exception("Bad weights array in SetWeights");

            int k = 0; // points into weights param

            for (int i = 0; i < m_numInput; ++i)
                for (int j = 0; j < m_numHidden; ++j)
                    m_ihWeights[i][j] = weights[k++];
            for (int i = 0; i < m_numHidden; ++i)
                m_hBiases[i] = weights[k++];
            for (int i = 0; i < m_numHidden; ++i)
                for (int j = 0; j < m_numOutput; ++j)
                    m_hoWeights[i][j] = weights[k++];
            for (int i = 0; i < m_numOutput; ++i)
                m_oBiases[i] = weights[k++];
        }

        public double[] GetWeights()
        {
            int numWeights = (m_numInput * m_numHidden) + (m_numHidden * m_numOutput) + m_numHidden + m_numOutput;
            double[] result = new double[numWeights];
            int k = 0;
            for (int i = 0; i < m_ihWeights.Length; ++i)
                for (int j = 0; j < m_ihWeights[0].Length; ++j)
                    result[k++] = m_ihWeights[i][j];
            for (int i = 0; i < m_hBiases.Length; ++i)
                result[k++] = m_hBiases[i];
            for (int i = 0; i < m_hoWeights.Length; ++i)
                for (int j = 0; j < m_hoWeights[0].Length; ++j)
                    result[k++] = m_hoWeights[i][j];
            for (int i = 0; i < m_oBiases.Length; ++i)
                result[k++] = m_oBiases[i];
            return result;
        }

        public double[] ComputeOutputs(double[] xValues)
        {
            double[] hSums = new double[m_numHidden]; // hidden nodes sums scratch array
            double[] oSums = new double[m_numOutput]; // output nodes sums

            for (int i = 0; i < xValues.Length; ++i) // copy x-values to inputs
                m_inputs[i] = xValues[i];
            // note: no need to copy x-values unless you implement a ToString and want to see them.
            // more efficient is to simply use the xValues[] directly.

            for (int j = 0; j < m_numHidden; ++j)  // compute i-h sum of weights * inputs
                for (int i = 0; i < m_numInput; ++i)
                    hSums[j] += m_inputs[i] * m_ihWeights[i][j]; // note +=

            for (int i = 0; i < m_numHidden; ++i)  // add biases to input-to-hidden sums
                hSums[i] += m_hBiases[i];

            for (int i = 0; i < m_numHidden; ++i)   // apply activation
                m_hOutputs[i] = HyperTan(hSums[i]); // hard-coded

            for (int j = 0; j < m_numOutput; ++j)   // compute h-o sum of weights * hOutputs
                for (int i = 0; i < m_numHidden; ++i)
                    oSums[j] += m_hOutputs[i] * m_hoWeights[i][j];

            for (int i = 0; i < m_numOutput; ++i)  // add biases to input-to-hidden sums
                oSums[i] += m_oBiases[i];

            for (int i = 0; i < m_numOutput; ++i)
                m_outputs[i] = HyperTan(oSums[i]);
            /*
            double[] softOut = Softmax(oSums); // softmax activation does all outputs at once for efficiency
            Array.Copy(softOut, m_outputs, softOut.Length);
            */

            double[] retResult = new double[m_numOutput]; // could define a GetOutputs method instead
            Array.Copy(m_outputs, retResult, retResult.Length);
            return retResult;
        }

        private static double HyperTan(double x)
        {
            if (x < -20.0) return -1.0; // approximation is correct to 30 decimals
            else if (x > 20.0) return 1.0;
            else return Math.Tanh(x);
        }

        private static double[] Softmax(double[] oSums)
        {
            // does all output nodes at once so scale doesn't have to be re-computed each time
            // determine max output-sum
            double max = oSums[0];
            for (int i = 0; i < oSums.Length; ++i)
                if (oSums[i] > max) max = oSums[i];

            // determine scaling factor -- sum of exp(each val - max)
            double scale = 0.0;
            for (int i = 0; i < oSums.Length; ++i)
                scale += Math.Exp(oSums[i] - max);

            double[] result = new double[oSums.Length];
            for (int i = 0; i < oSums.Length; ++i)
                result[i] = Math.Exp(oSums[i] - max) / scale;

            return result; // now scaled so that xi sum to 1.0
        }

        public double MeanSquaredError(double[][] trainData, double[] weights)
        {
            SetWeights(weights); // copy the weights to evaluate in

            double[] xValues = new double[m_numInput]; // inputs
            double[] tValues = new double[m_numOutput]; // targets
            double sumSquaredError = 0.0;
            for (int i = 0; i < trainData.Length; ++i) // walk through each training data item
            {
                // following assumes data has all x-values first, followed by y-values!
                Array.Copy(trainData[i], xValues, m_numInput); // extract inputs
                Array.Copy(trainData[i], m_numInput, tValues, 0, m_numOutput); // extract targets
                double[] yValues = ComputeOutputs(xValues);
                for (int j = 0; j < yValues.Length; ++j)
                    sumSquaredError += ((yValues[j] - tValues[j]) * (yValues[j] - tValues[j]));
            }
            return sumSquaredError / (trainData.Length * m_numOutput);
        }


    }//EDUNetwork
}//Namespace
