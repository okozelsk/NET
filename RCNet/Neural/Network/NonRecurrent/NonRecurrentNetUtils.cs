using RCNet.Neural.Activation;
using RCNet.Neural.Data;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Implements a set of the helper methods related to non-recurrent networks.
    /// </summary>
    public static class NonRecurrentNetUtils
    {
        /// <summary>
        /// Tests whether the specified network configuration is the feed forward network configuration.
        /// </summary>
        /// <param name="cfg">The network configuration.</param>
        public static bool IsFF(INonRecurrentNetworkSettings cfg)
        {
            return (cfg.GetType() == typeof(FeedForwardNetworkSettings));
        }

        /// <summary>
        /// Tests whether the specified xml configuration element is the feed forward network xml configuration element.
        /// </summary>
        /// <param name="elem">The xml configuration element.</param>
        public static bool IsFFElem(XElement elem)
        {
            return (elem.Name.LocalName == "ff");
        }

        /// <summary>
        /// Tests whether the specified network configuration is the parallel perceptron configuration.
        /// </summary>
        /// <param name="cfg">The network configuration.</param>
        public static bool IsPP(INonRecurrentNetworkSettings cfg)
        {
            return (cfg.GetType() == typeof(ParallelPerceptronSettings));
        }

        /// <summary>
        /// Tests whether the specified xml configuration element is the parallel perceptron xml configuration element.
        /// </summary>
        /// <param name="elem">The xml configuration element.</param>
        public static bool IsPPElem(XElement elem)
        {
            return (elem.Name.LocalName == "pp");
        }


        /// <summary>
        /// Loads the appropriate network configuration from a xml element.
        /// </summary>
        /// <param name="cfgElem">A XML element containing the configuration.</param>
        public static INonRecurrentNetworkSettings LoadNetworkConfiguration(XElement cfgElem)
        {
            if (IsFFElem(cfgElem))
            {
                return new FeedForwardNetworkSettings(cfgElem);
            }
            else if (IsPPElem(cfgElem))
            {
                return new ParallelPerceptronSettings(cfgElem);
            }
            else
            {
                throw new ArgumentException($"Unsupported cfgElem name: {cfgElem.Name}", "cfgElem");
            }
        }

        /// <summary>
        /// Loads the collection of the network configurations under the specified root xml element.
        /// When the rootElem is null then it returns an empty collection.
        /// </summary>
        /// <param name="rootElem">The root xml element.</param>
        public static List<INonRecurrentNetworkSettings> LoadNonRecurrentNetworkSettingsCollection(XElement rootElem)
        {
            List<INonRecurrentNetworkSettings> cfgCollection = new List<INonRecurrentNetworkSettings>();
            if (rootElem != null)
            {
                foreach (XElement cfgElem in rootElem.Elements())
                {
                    if (IsFFElem(cfgElem) || IsPPElem(cfgElem))
                    {
                        cfgCollection.Add(LoadNetworkConfiguration(cfgElem));
                    }
                }
            }
            return cfgCollection;
        }

        /// <summary>
        /// Creates the instance of the network and its associated trainer.
        /// </summary>
        /// <param name="cfg">The configuration of the network.</param>
        /// <param name="trainingInputVectors">The collection of training input vectors (input).</param>
        /// <param name="trainingOutputVectors">The collection of training output vectors (ideal).</param>
        /// <param name="rand">The random object to be used.</param>
        /// <param name="net">The created network.</param>
        /// <param name="trainer">The created trainer.</param>
        public static void CreateNetworkAndTrainer(INonRecurrentNetworkSettings cfg,
                                                   List<double[]> trainingInputVectors,
                                                   List<double[]> trainingOutputVectors,
                                                   Random rand,
                                                   out INonRecurrentNetwork net,
                                                   out INonRecurrentNetworkTrainer trainer
                                                   )
        {
            if (IsFF(cfg))
            {
                //Feed forward network
                FeedForwardNetworkSettings netCfg = (FeedForwardNetworkSettings)cfg;
                FeedForwardNetwork ffn = new FeedForwardNetwork(trainingInputVectors[0].Length, trainingOutputVectors[0].Length, netCfg);
                net = ffn;
                if (netCfg.TrainerCfg.GetType() == typeof(QRDRegrTrainerSettings))
                {
                    trainer = new QRDRegrTrainer(ffn, trainingInputVectors, trainingOutputVectors, (QRDRegrTrainerSettings)netCfg.TrainerCfg, rand);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(RidgeRegrTrainerSettings))
                {
                    trainer = new RidgeRegrTrainer(ffn, trainingInputVectors, trainingOutputVectors, (RidgeRegrTrainerSettings)netCfg.TrainerCfg);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(ElasticRegrTrainerSettings))
                {
                    trainer = new ElasticRegrTrainer(ffn, trainingInputVectors, trainingOutputVectors, (ElasticRegrTrainerSettings)netCfg.TrainerCfg);
                }
                else if (netCfg.TrainerCfg.GetType() == typeof(RPropTrainerSettings))
                {
                    trainer = new RPropTrainer(ffn, trainingInputVectors, trainingOutputVectors, (RPropTrainerSettings)netCfg.TrainerCfg, rand);
                }
                else
                {
                    throw new ArgumentException($"Unknown trainer {netCfg.TrainerCfg}", "netCfg");
                }
            }
            else if (IsPP(cfg))
            {
                //Parallel perceptron network
                //Check output
                if (trainingOutputVectors[0].Length != 1)
                {
                    throw new InvalidOperationException($"Can't create ParallelPerceptron. Only single output value is allowed.");
                }
                ParallelPerceptronSettings netCfg = (ParallelPerceptronSettings)cfg;
                ParallelPerceptron ppn = new ParallelPerceptron(trainingInputVectors[0].Length, netCfg);
                net = ppn;
                trainer = new PDeltaRuleTrainer(ppn, trainingInputVectors, trainingOutputVectors, netCfg.PDeltaRuleTrainerCfg, rand);
            }
            else
            {
                throw new ArgumentException($"Unknown network configuration.", "cfg");
            }
            net.RandomizeWeights(rand);
            return;
        }

        /// <summary>
        /// Checks whether the network configuration is suitable for the specified type of the network output.
        /// </summary>
        /// <param name="outputType">The type of the network output.</param>
        /// <param name="netCfg">The network configuration.</param>
        public static void CheckNetCfg(TNRNet.OutputType outputType, INonRecurrentNetworkSettings netCfg)
        {
            switch (outputType)
            {
                case TNRNet.OutputType.Probabilistic:
                    {
                        if (netCfg.GetType() != typeof(FeedForwardNetworkSettings))
                        {
                            throw new ArgumentException($"Incorrect network configuration. It must be the Feed forward network configuration.", "netCfg");
                        }
                        if (((FeedForwardNetworkSettings)netCfg).OutputActivationCfg.GetType() != typeof(AFAnalogSoftMaxSettings))
                        {
                            throw new ArgumentException($"Feed forward network must have the SoftMax output activation.", "netCfg");
                        }
                        if (((FeedForwardNetworkSettings)netCfg).TrainerCfg.GetType() != typeof(RPropTrainerSettings))
                        {
                            throw new ArgumentException($"Feed forward network must have associated the RProp trainer.", "netCfg");
                        }
                    }
                    break;
                case TNRNet.OutputType.Real:
                    {
                        if (netCfg.GetType() != typeof(FeedForwardNetworkSettings))
                        {
                            throw new ArgumentException($"Incorrect network configuration. It must be the Feed forward network configuration.", "netCfg");
                        }
                    }
                    break;
                default:
                    break;
            }
            return;
        }

        /// <summary>
        /// Checks whether the data is in accordance with the specified type of the network output.
        /// </summary>
        /// <param name="outputType">The type of the network output.</param>
        /// <param name="data">The data to be checked.</param>
        public static void CheckData(TNRNet.OutputType outputType, VectorBundle data)
        {
            //General checks
            if (data.InputVectorCollection.Count != data.OutputVectorCollection.Count)
            {
                throw new ArgumentException($"Incorrect data. Different number of input and output vectors.", "data");
            }
            if (data.OutputVectorCollection.Count < 2)
            {
                throw new ArgumentException($"Too few samples.", "data");
            }
            int outputVectorLength = data.OutputVectorCollection[0].Length;
            //Output vector length checks
            if (outputType == TNRNet.OutputType.Probabilistic)
            {
                if (outputVectorLength <= 1)
                {
                    throw new ArgumentException($"Number of output vector values must be GT 1.", "data");
                }
            }
            else if (outputType == TNRNet.OutputType.SingleBool)
            {
                if (outputVectorLength != 1)
                {
                    throw new ArgumentException($"Number of output vector values must be ET 1.", "data");
                }
            }
            //Data scan
            foreach (double[] outputVector in data.OutputVectorCollection)
            {
                if (outputVectorLength != outputVector.Length)
                {
                    throw new ArgumentException($"Inconsistent length of output vectors.", "data");
                }
                switch (outputType)
                {
                    case TNRNet.OutputType.Probabilistic:
                        {
                            int bin1Counter = 0;
                            int bin0Counter = 0;
                            for (int i = 0; i < outputVector.Length; i++)
                            {
                                if (outputVector[i] == 0d)
                                {
                                    ++bin0Counter;
                                }
                                else if (outputVector[i] == 1d)
                                {
                                    ++bin1Counter;
                                }
                                else
                                {
                                    throw new ArgumentException($"Output data vectors contain different values than 0 or 1.", "data");
                                }
                            }
                            if (bin1Counter != 1)
                            {
                                throw new ArgumentException($"Output data vector contains more than one 1.", "data");
                            }
                        }
                        break;
                    case TNRNet.OutputType.SingleBool:
                        {

                        }
                        break;
                    default:
                        break;
                }

            }
            return;
        }

        /// <summary>
        /// Concates the alone vectors into the single flat vector.
        /// </summary>
        /// <param name="vectorCollection">The collection of the vectors to be flattened.</param>
        public static double[] Flattenize(List<double[]> vectorCollection)
        {
            if (vectorCollection == null)
            {
                return null;
            }
            int length = 0;
            foreach (double[] vector in vectorCollection)
            {
                length += vector.Length;
            }
            double[] resultVector = new double[length];
            int idx = 0;
            foreach (double[] vector in vectorCollection)
            {
                vector.CopyTo(resultVector, idx);
                idx += vector.Length;
            }
            return resultVector;
        }

        /// <summary>
        /// Extracts the vectors from the int-double[] tuples.
        /// </summary>
        /// <param name="tuples">The int-double[] pairs.</param>
        public static List<double[]> ExtractVectors(List<Tuple<int, double[]>> tuples)
        {
            if (tuples == null)
            {
                return null;
            }
            return new List<double[]>(from tuple in tuples select tuple.Item2);
        }


    }//NonRecurrentNetUtils

}//Namespace
