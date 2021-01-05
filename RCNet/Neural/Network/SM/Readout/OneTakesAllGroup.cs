using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Implements the "One Takes All" group of readout units.
    /// </summary>
    /// <remarks>
    /// Supports basic decision-making based directly on the results of readout units and also more advanced decision-making based on the result of a dedicated chain of network clusters.
    /// </remarks>
    [Serializable]
    public class OneTakesAllGroup
    {
        //Enums
        /// <summary>
        /// The decision method of the "One Takes All" group.
        /// </summary>
        public enum OneTakesAllDecisionMethod
        {
            /// <summary>
            /// The basic ad-hoc decision.
            /// </summary>
            Basic,
            /// <summary>
            /// The decision makes the trained cluster chain.
            /// </summary>
            ClusterChain
        }

        /// <summary>
        /// This informative event occurs every time the regression epoch is done.
        /// </summary>
        [field: NonSerialized]
        public event TNRNetBuilder.EpochDoneHandler RegressionEpochDone;

        //Attribute properties
        /// <summary>
        /// An index of this group within "One Takes All" groups of the readout layer.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// The name of the "One Takes All" group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The indexes of the member readout units.
        /// </summary>
        public List<int> MemberReadoutUnitIndexCollection { get; }

        /// <inheritdoc cref="OneTakesAllDecisionMethod"/>
        public OneTakesAllDecisionMethod DecisionMethod;

        /// <summary>
        /// The probabilistic cluster chain.
        /// </summary>
        public TNRNetClusterChain ProbabilisticClusterChain { get; private set; }

        //Attributes
        private readonly OneTakesAllGroupSettings _groupCfg;

        //Constructors
        /// <summary>
        /// Creates an itialized instance ready for build
        /// </summary>
        /// <param name="index">An index of this group within "One Takes All" groups of the readout layer.</param>
        /// <param name="groupCfg">The configuration of the "One Takes All" group.</param>
        /// <param name="memberReadoutUnitIndexes">The indexes of the member readout units.</param>
        public OneTakesAllGroup(int index,
                                OneTakesAllGroupSettings groupCfg,
                                IEnumerable<int> memberReadoutUnitIndexes
                                )
        {
            Index = index;
            Name = groupCfg.Name;
            _groupCfg = (OneTakesAllGroupSettings)groupCfg.DeepClone();
            DecisionMethod = _groupCfg.DecisionCfg.DecisionMethod;
            MemberReadoutUnitIndexCollection = new List<int>(memberReadoutUnitIndexes);
            ProbabilisticClusterChain = null;
            return;
        }

        //Properties
        /// <summary>
        /// Gets the number of classes within the group.
        /// </summary>
        public int NumOfMemberClasses { get { return MemberReadoutUnitIndexCollection.Count; } }

        //Methods
        private void OnRegressionEpochDone(TNRNetBuilder.BuildProgress buildProgress, bool foundBetter)
        {
            //Only raise up
            RegressionEpochDone(buildProgress, foundBetter);
            return;
        }

        private double[] CreateInputVector(CompositeResult[] allReadoutUnitResults)
        {
            OneTakesAllClusterChainDecisionSettings clusterCfg = (OneTakesAllClusterChainDecisionSettings)_groupCfg.DecisionCfg;
            List<double[]> components = new List<double[]>();
            foreach (int unitIdx in MemberReadoutUnitIndexCollection)
            {
                if (clusterCfg.UseReadoutUnitsFinalResult)
                {
                    components.Add(allReadoutUnitResults[unitIdx].Result);
                }
                if (clusterCfg.UseReadoutUnitsSubResults)
                {
                    foreach (double[] subResult in allReadoutUnitResults[unitIdx].SubResults)
                    {
                        components.Add(subResult);
                    }
                }
            }
            return NonRecurrentNetUtils.Flattenize(components);
        }

        private double[] CreateOutputVector(double[] allReadoutUnitsIdealValues, BinFeatureFilter[] filters)
        {
            double[] outputVector = new double[MemberReadoutUnitIndexCollection.Count];
            for (int i = 0; i < MemberReadoutUnitIndexCollection.Count; i++)
            {
                outputVector[i] = filters[i].ApplyReverse(allReadoutUnitsIdealValues[MemberReadoutUnitIndexCollection[i]]);
            }
            return outputVector;
        }

        /// <summary>
        /// Builds the internal probabilistic cluster chain and makes the "One Takes All" group operable.
        /// </summary>
        /// <param name="readoutUnitsResultsCollection">The collection of the collections of all readout units composite results.</param>
        /// <param name="readoutUnitsIdealValuesCollection">The collection of the collections of all readout units ideal values.</param>
        /// <param name="filters">The feature filters to be used to denormalize output data.</param>
        /// <param name="rand">The random object to be used.</param>
        /// <param name="controller">The build process controller (optional).</param>
        public void Build(List<CompositeResult[]> readoutUnitsResultsCollection,
                          List<double[]> readoutUnitsIdealValuesCollection,
                          BinFeatureFilter[] filters,
                          Random rand,
                          TNRNetBuilder.BuildControllerDelegate controller = null
                          )
        {
            if (DecisionMethod != OneTakesAllDecisionMethod.ClusterChain)
            {
                throw new InvalidOperationException("Wrong call of the Build method.");
            }
            OneTakesAllClusterChainDecisionSettings decisionCfg = (OneTakesAllClusterChainDecisionSettings)_groupCfg.DecisionCfg;
            //Prepare the training data bundle for the cluster chain
            VectorBundle trainingDataBundle = new VectorBundle(readoutUnitsIdealValuesCollection.Count);
            for (int sampleIdx = 0; sampleIdx < readoutUnitsIdealValuesCollection.Count; sampleIdx++)
            {
                double[] inputVector = CreateInputVector(readoutUnitsResultsCollection[sampleIdx]);
                double[] outputVector = CreateOutputVector(readoutUnitsIdealValuesCollection[sampleIdx], filters);
                trainingDataBundle.AddPair(inputVector, outputVector);
            }


            //Cluster chain builder
            TNRNetClusterChainBuilder builder = new TNRNetClusterChainBuilder("OTAG",
                                                                              Name,
                                                                              decisionCfg.ClusterChainCfg,
                                                                              rand,
                                                                              controller
                                                                              );
            builder.EpochDone += OnRegressionEpochDone;
            ProbabilisticClusterChain = builder.Build(trainingDataBundle, filters);
            return;
        }

        /// <summary>
        /// Computes the "One Takes All" group.
        /// </summary>
        /// <param name="allReadoutUnitResults">The collection of all readout units composite results.</param>
        /// <param name="groupResult">The composite result of the group's probabilistic cluster chain.</param>
        /// <param name="outputVector">The output vector.</param>
        /// <returns>An index of the winning unit within the "One Takes All" group.</returns>
        public int Compute(CompositeResult[] allReadoutUnitResults, out CompositeResult groupResult, out double[] outputVector)
        {
            int winnerIdx;
            groupResult = new CompositeResult();
            outputVector = new double[MemberReadoutUnitIndexCollection.Count];
            if (DecisionMethod == OneTakesAllDecisionMethod.Basic)
            {
                for (int i = 0; i < MemberReadoutUnitIndexCollection.Count; i++)
                {
                    //Store rescaled member units results
                    outputVector[i] = Interval.IntZP1.Rescale(allReadoutUnitResults[MemberReadoutUnitIndexCollection[i]].Result[0], Interval.IntN1P1);
                }
                //Make the sum equal to 1
                outputVector.ScaleToNewSum(1d);
                groupResult.Result = new double[outputVector.Length];
                outputVector.CopyTo(groupResult.Result, 0);
                //Rescale output vector back to -1 and 1
                for (int i = 0; i < MemberReadoutUnitIndexCollection.Count; i++)
                {
                    outputVector[i] = Interval.IntN1P1.Rescale(outputVector[i], Interval.IntZP1);
                }
                winnerIdx = outputVector.MaxIdx();
            }
            else
            {
                double[] inputVector = CreateInputVector(allReadoutUnitResults);
                outputVector = ProbabilisticClusterChain.Compute(inputVector, out List<Tuple<int, double[]>> memberNetOuputs);
                groupResult.Result = new double[outputVector.Length];
                outputVector.CopyTo(groupResult.Result, 0);
                groupResult.SubResults = new List<double[]>();
                foreach (Tuple<int, double[]> tuple in memberNetOuputs)
                {
                    groupResult.SubResults.Add(tuple.Item2);
                }
                //Rescale output to -1 and 1
                for (int i = 0; i < MemberReadoutUnitIndexCollection.Count; i++)
                {
                    outputVector[i] = Interval.IntN1P1.Rescale(outputVector[i], Interval.IntZP1);
                }
                winnerIdx = outputVector.MaxIdx();
            }

            return winnerIdx;
        }

    }//OneTakesAllGroup

}//Namespace
