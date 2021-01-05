using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Probability;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements the cluster of trained non-recurrent networks.
    /// </summary>
    /// <remarks>
    /// The cluster is based on the cross-validation approach.
    /// </remarks>
    [Serializable]
    public class TNRNetCluster
    {

        //Attribute properties
        /// <summary>
        /// The name of the cluster.
        /// </summary>
        public string ClusterName { get; }

        /// <inheritdoc cref="TNRNet.OutputType"/>
        public TNRNet.OutputType Output { get; }

        /// <inheritdoc cref="ClusterErrStatistics"/>
        public ClusterErrStatistics ErrorStats { get; }

        //Attributes
        //Macro weights
        private readonly double _trainingGroupWeight;
        private readonly double _testingGroupWeight;
        private readonly double _samplesWeight;
        private readonly double _precisionWeight;
        private readonly double _misrecognizedFalseWeight;
        private readonly double _unrecognizedTrueWeight;
        //Member networks
        private readonly List<TNRNet> _memberNetCollection;
        private readonly List<int> _memberNetScopeIDCollection;
        private double[] _memberNetWeights;

        //Constructor
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="clusterName">The name of the cluster.</param>
        /// <param name="outputType">The type of output.</param>
        /// <param name="trainingGroupWeight">The macro-weight of the group of metrics related to training.</param>
        /// <param name="testingGroupWeight">The macro-weight of the group of metrics related to testing.</param>
        /// <param name="samplesWeight">The weight of the number of samples metric.</param>
        /// <param name="precisionWeight">The weight of the numerical precision metric.</param>
        /// <param name="misrecognizedFalseWeight">The weight of the "misrecognized falses" metric.</param>
        /// <param name="unrecognizedTrueWeight">The weight of the "unrecognized trues" metric.</param>
        public TNRNetCluster(string clusterName,
                             TNRNet.OutputType outputType,
                             double trainingGroupWeight = 1d,
                             double testingGroupWeight = 1d,
                             double samplesWeight = 1d,
                             double precisionWeight = 1d,
                             double misrecognizedFalseWeight = 1d,
                             double unrecognizedTrueWeight = 0d
                             )
        {
            ClusterName = clusterName;
            Output = outputType;
            _trainingGroupWeight = trainingGroupWeight;
            _testingGroupWeight = testingGroupWeight;
            _samplesWeight = samplesWeight;
            _precisionWeight = precisionWeight;
            _misrecognizedFalseWeight = misrecognizedFalseWeight;
            _unrecognizedTrueWeight = unrecognizedTrueWeight;
            ErrorStats = new ClusterErrStatistics(ClusterName, outputType);
            _memberNetCollection = new List<TNRNet>();
            _memberNetScopeIDCollection = new List<int>();
            _memberNetWeights = null;
            return;
        }

        //Properties
        /// <summary>
        /// Indicates the cluster is finalized and ready to operate.
        /// </summary>
        public bool Finalized { get { return _memberNetWeights != null; } }

        /// <summary>
        /// Gets the number of member networks.
        /// </summary>
        public int NumOfMembers { get { return _memberNetCollection.Count; } }

        /// <summary>
        /// Gets the number of outputs.
        /// </summary>
        public int NumOfOutputs { get { return _memberNetCollection.Count > 0 ? _memberNetCollection[0].Network.NumOfOutputValues : 0; } }

        /// <summary>
        /// Gets the output data range.
        /// </summary>
        public Interval OutputDataRange { get { return TNRNet.GetOutputDataRange(Output); } }


        //Methods
        /// <summary>
        /// Adds a new member network and updates the cluster error statistics.
        /// </summary>
        /// <param name="newMemberNet">The new member network.</param>
        /// <param name="scopeID">The ID of a network's scope.</param>
        /// <param name="testData">The testing data bundle (unseen by the network to be added).</param>
        /// <param name="filters">The filters to be used to denormalize outputs.</param>
        public void AddMember(TNRNet newMemberNet, int scopeID, VectorBundle testData, FeatureFilterBase[] filters)
        {
            //Check the network output
            if (Output != newMemberNet.Output)
            {
                throw new ArgumentException("Inconsistent output type of the network to be added.", "newMemberNet");
            }
            //Check number of outputs consistency
            if (_memberNetCollection.Count > 0)
            {
                if (newMemberNet.Network.NumOfOutputValues != NumOfOutputs)
                {
                    throw new ArgumentException("Number of outputs of the network differs from already clustered networks.", "newMemberNet");
                }
            }
            //Add member to inner collection
            _memberNetCollection.Add(newMemberNet);
            _memberNetScopeIDCollection.Add(scopeID);
            //Update cluster error statistics
            for (int sampleIdx = 0; sampleIdx < testData.OutputVectorCollection.Count; sampleIdx++)
            {
                double[] nrmComputedValues = newMemberNet.Network.Compute(testData.InputVectorCollection[sampleIdx]);
                for (int outIdx = 0; outIdx < nrmComputedValues.Length; outIdx++)
                {
                    double naturalComputedValue = filters != null ? filters[outIdx].ApplyReverse(nrmComputedValues[outIdx]) : nrmComputedValues[outIdx];
                    double naturalIdealValue = filters != null ? filters[outIdx].ApplyReverse(testData.OutputVectorCollection[sampleIdx][outIdx]) : testData.OutputVectorCollection[sampleIdx][outIdx];
                    ErrorStats.Update(nrmComputedValues[outIdx],
                                      testData.OutputVectorCollection[sampleIdx][outIdx],
                                      naturalComputedValue,
                                      naturalIdealValue
                                      );
                }//outIdx
            }//sampleIdx
            return;
        }

        /// <summary>
        /// Initializes the predictive weights of the member networks.
        /// </summary>
        private void InitMemberNetworksWeights()
        {
            _memberNetWeights = new double[_memberNetCollection.Count];
            if (_memberNetWeights.Length == 1)
            {
                _memberNetWeights[0] = 1d;
            }
            else
            {
                //Holders for the metrics
                //Training
                double[] wHolderTrainSamples = new double[_memberNetCollection.Count];
                double[] wHolderTrainPrecision = new double[_memberNetCollection.Count];
                double[] wHolderTrainMisrecognized = new double[_memberNetCollection.Count];
                double[] wHolderTrainUnrecognized = new double[_memberNetCollection.Count];
                //Testing
                double[] wHolderTestSamples = new double[_memberNetCollection.Count];
                double[] wHolderTestPrecision = new double[_memberNetCollection.Count];
                double[] wHolderTestMisrecognized = new double[_memberNetCollection.Count];
                double[] wHolderTestUnrecognized = new double[_memberNetCollection.Count];
                //Collect the metrics
                for (int memberIdx = 0; memberIdx < _memberNetCollection.Count; memberIdx++)
                {
                    wHolderTrainSamples[memberIdx] = _memberNetCollection[memberIdx].TrainingErrorStat.NumOfSamples;
                    wHolderTrainPrecision[memberIdx] = _memberNetCollection[memberIdx].TrainingErrorStat.ArithAvg;
                    if (Output != TNRNet.OutputType.Real)
                    {
                        wHolderTrainMisrecognized[memberIdx] = (1d - _memberNetCollection[memberIdx].TrainingBinErrorStat.BinValErrStat[0].ArithAvg);
                        wHolderTrainUnrecognized[memberIdx] = (1d - _memberNetCollection[memberIdx].TrainingBinErrorStat.BinValErrStat[1].ArithAvg);
                    }
                    else
                    {
                        wHolderTrainMisrecognized[memberIdx] = 1d;
                        wHolderTrainUnrecognized[memberIdx] = 1d;
                    }
                    wHolderTestSamples[memberIdx] = _memberNetCollection[memberIdx].TestingErrorStat.NumOfSamples;
                    wHolderTestPrecision[memberIdx] = _memberNetCollection[memberIdx].TestingErrorStat.ArithAvg;
                    if (Output != TNRNet.OutputType.Real)
                    {
                        wHolderTestMisrecognized[memberIdx] = (1d - _memberNetCollection[memberIdx].TestingBinErrorStat.BinValErrStat[0].ArithAvg);
                        wHolderTestUnrecognized[memberIdx] = (1d - _memberNetCollection[memberIdx].TestingBinErrorStat.BinValErrStat[1].ArithAvg);
                    }
                    else
                    {
                        wHolderTestMisrecognized[memberIdx] = 1d;
                        wHolderTestUnrecognized[memberIdx] = 1d;
                    }
                }
                //Turn the metrics to have the same meaning and scale them to be useable as the sub-weights
                wHolderTrainSamples.ScaleToNewSum(1d);
                wHolderTestSamples.ScaleToNewSum(1d);
                wHolderTrainPrecision.RevertMinMax();
                wHolderTrainPrecision.ScaleToNewSum(1d);
                wHolderTestPrecision.RevertMinMax();
                wHolderTestPrecision.ScaleToNewSum(1d);
                wHolderTrainMisrecognized.ScaleToNewSum(1d);
                wHolderTestMisrecognized.ScaleToNewSum(1d);
                wHolderTrainUnrecognized.ScaleToNewSum(1d);
                wHolderTestUnrecognized.ScaleToNewSum(1d);
                //Build the final weights
                //Combine the sub-weights using defined macro weights of the metrics
                for (int i = 0; i < _memberNetCollection.Count; i++)
                {
                    _memberNetWeights[i] = _trainingGroupWeight *
                                           (_samplesWeight * wHolderTrainSamples[i] +
                                            _precisionWeight * wHolderTrainPrecision[i] +
                                            _misrecognizedFalseWeight * wHolderTrainMisrecognized[i] +
                                            _unrecognizedTrueWeight * wHolderTrainUnrecognized[i]
                                            ) +
                                          _testingGroupWeight *
                                            (_samplesWeight * wHolderTestSamples[i] +
                                             _precisionWeight * wHolderTestPrecision[i] +
                                             _misrecognizedFalseWeight * wHolderTestMisrecognized[i] +
                                             _unrecognizedTrueWeight * wHolderTestUnrecognized[i]
                                             );
                }
                //Apply the softmax transformation
                _memberNetWeights.Softmax();
            }
            return;
        }

        /// <summary>
        /// Computes the outputs of all member networks.
        /// </summary>
        private List<Tuple<int, double[]>> ComputeClusterMemberNetworks(double[] inputVector, List<Tuple<int, double[]>> precomputations = null)
        {
            List<Tuple<int, double[]>> outputVectors = new List<Tuple<int, double[]>>(NumOfMembers);
            for (int memberIdx = 0; memberIdx < NumOfMembers; memberIdx++)
            {
                double[] netInputVector = inputVector;
                if (precomputations != null)
                {
                    //Add precomputations having the same scope ID as the current network
                    for (int precompIdx = 0; precompIdx < precomputations.Count; precompIdx++)
                    {
                        if (_memberNetScopeIDCollection[memberIdx] == precomputations[precompIdx].Item1)
                        {
                            netInputVector = netInputVector.Concat(precomputations[precompIdx].Item2);
                        }
                    }
                }
                outputVectors.Add(new Tuple<int, double[]>(_memberNetScopeIDCollection[memberIdx], _memberNetCollection[memberIdx].Network.Compute(netInputVector)));
            }
            return outputVectors;
        }

        /// <summary>
        /// Computes the composite output of the cluster.
        /// </summary>
        private double[] ComputeCompositeOutput(List<Tuple<int, double[]>> memberNetOutputs)
        {
            if (Output == TNRNet.OutputType.Real)
            {
                //Real output
                double[] output = new double[NumOfOutputs];
                for (int outIdx = 0; outIdx < NumOfOutputs; outIdx++)
                {
                    //Compute weighted average of members single output
                    WeightedAvg wAvg = new WeightedAvg();
                    for (int i = 0; i < memberNetOutputs.Count; i++)
                    {
                        //Add sub-result to weighted average
                        wAvg.AddSample(memberNetOutputs[i].Item2[outIdx], _memberNetWeights[i]);
                    }
                    //Store averaged output
                    output[outIdx] = wAvg.Result;
                }
                //Return averaged outputs
                return output;
            }
            else
            {
                //Probabilistic or SingleBool output
                int numOfOutputProbabilities = memberNetOutputs[0].Item2.Length;
                double[] outProbabilities = new double[numOfOutputProbabilities];
                for (int pIdx = 0; pIdx < numOfOutputProbabilities; pIdx++)
                {
                    double[] memberPs = new double[NumOfMembers];
                    for (int i = 0; i < NumOfMembers; i++)
                    {
                        memberPs[i] = PMixer.ProbabilityRange.Rescale(memberNetOutputs[i].Item2[pIdx], OutputDataRange);
                    }
                    //Compute members mixed probability
                    outProbabilities[pIdx] = PMixer.MixP(memberPs, _memberNetWeights);
                }
                if (numOfOutputProbabilities > 1)
                {
                    outProbabilities.ScaleToNewSum(1d);
                }
                //Rescale probabilities to output range
                for (int pIdx = 0; pIdx < numOfOutputProbabilities; pIdx++)
                {
                    outProbabilities[pIdx] = OutputDataRange.Rescale(outProbabilities[pIdx], PMixer.ProbabilityRange);
                }
                return outProbabilities;
            }
        }

        /// <summary>
        /// Makes the cluster ready to operate.
        /// </summary>
        public void FinalizeCluster()
        {
            if (Finalized)
            {
                throw new InvalidOperationException("Cluster was already finalized.");
            }
            //Initialize the member networks weights
            InitMemberNetworksWeights();
            return;
        }

        /// <summary>
        /// Computes the cluster outputs.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="memberNetOutputs">The collection of member networks outputs.</param>
        public double[] Compute(double[] inputVector, out List<Tuple<int, double[]>> memberNetOutputs)
        {
            if (!Finalized)
            {
                throw new InvalidOperationException("Cluster is not finalized. Call the FinalizeCluster method first.");
            }
            //Collect member networks outputs
            memberNetOutputs = ComputeClusterMemberNetworks(inputVector);
            //Compute the result
            return ComputeCompositeOutput(memberNetOutputs);
        }

        /// <summary>
        /// Computes the cluster outputs.
        /// </summary>
        /// <param name="inputVector">The input vector.</param>
        /// <param name="precomputations">The additional precomputed inputs.</param>
        /// <param name="memberNetOutputs">The collection of member networks outputs.</param>
        public double[] Compute(double[] inputVector, List<Tuple<int, double[]>> precomputations, out List<Tuple<int, double[]>> memberNetOutputs)
        {
            if (!Finalized)
            {
                throw new InvalidOperationException("Cluster is not finalized. Call the FinalizeCluster method first.");
            }
            //Collect member networks outputs
            memberNetOutputs = ComputeClusterMemberNetworks(inputVector, precomputations);
            //Compute the result
            return ComputeCompositeOutput(memberNetOutputs);
        }

        //Inner classes
        /// <summary>
        /// Implements the holder of error statistics of the cluster.
        /// </summary>
        [Serializable]
        public class ClusterErrStatistics
        {
            //Property attributes
            /// <summary>
            /// The name of the cluster.
            /// </summary>
            public string ClusterName { get; }
            /// <summary>
            /// The error statistics of the distance between the computed and ideal values in natural (denormalized) form.
            /// </summary>
            public BasicStat NatPrecissionErrStat { get; }
            /// <summary>
            /// The error statistics of the distance between the computed and ideal values in normalized form.
            /// </summary>
            public BasicStat NrmPrecissionErrStat { get; }
            /// <summary>
            /// The binary error statistics.
            /// </summary>
            public BinErrStat BinaryErrStat { get; }

            /// <summary>
            /// Creates an uninitialized instance.
            /// </summary>
            /// <param name="clusterName">The name of the cluster.</param>
            /// <param name="outputType">The type of output.</param>
            public ClusterErrStatistics(string clusterName, TNRNet.OutputType outputType)
            {
                ClusterName = clusterName;
                NatPrecissionErrStat = new BasicStat();
                NrmPrecissionErrStat = new BasicStat();
                if (TNRNet.IsBinErrorStatsOutputType(outputType))
                {
                    BinaryErrStat = new BinErrStat(TNRNet.GetOutputDataRange(outputType).Mid);
                }
                else
                {
                    BinaryErrStat = null;
                }
                return;
            }

            /// <summary>
            /// The deep copy constructor.
            /// </summary>
            /// <param name="source">The source instance.</param>
            public ClusterErrStatistics(ClusterErrStatistics source)
            {
                ClusterName = source.ClusterName;
                NatPrecissionErrStat = new BasicStat(source.NatPrecissionErrStat);
                NrmPrecissionErrStat = new BasicStat(source.NrmPrecissionErrStat);
                BinaryErrStat = source.BinaryErrStat?.DeepClone();
                return;
            }

            //Methods
            /// <summary>
            /// Updates the cluster error statistics.
            /// </summary>
            /// <param name="nrmComputedValue">The normalized value computed by the cluster.</param>
            /// <param name="nrmIdealValue">The normalized ideal value.</param>
            /// <param name="natComputedValue">The naturalized value computed by the cluster.</param>
            /// <param name="natIdealValue">The naturalized ideal value.</param>
            public void Update(double nrmComputedValue, double nrmIdealValue, double natComputedValue, double natIdealValue)
            {
                NatPrecissionErrStat.AddSample(Math.Abs(natComputedValue - natIdealValue));
                NrmPrecissionErrStat.AddSample(Math.Abs(nrmComputedValue - nrmIdealValue));
                BinaryErrStat?.Update(nrmComputedValue, nrmIdealValue);
                return;
            }

            /// <summary>
            /// Creates a deep copy instance of this instance.
            /// </summary>
            public ClusterErrStatistics DeepClone()
            {
                return new ClusterErrStatistics(this);
            }

        }//ClusterErrStatistics

    }//TNRNetCluster

}//Namespace
