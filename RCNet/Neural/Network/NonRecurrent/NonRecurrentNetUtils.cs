using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Static helper methods related to non-recurrent-networks
    /// </summary>
    public static class NonRecurrentNetUtils
    {
        /// <summary>
        /// Determines if given settings object is FeedForwardNetworkSettings
        /// </summary>
        /// <param name="settings">Non-recurrent-network settings</param>
        public static bool IsFF(INonRecurrentNetworkSettings settings)
        {
            return (settings.GetType() == typeof(FeedForwardNetworkSettings));
        }

        /// <summary>
        /// Determines if given configuration element is FeedForwardNetworkSettings element
        /// </summary>
        /// <param name="cfgElem">Configuration element</param>
        public static bool IsFFElem(XElement cfgElem)
        {
            return (cfgElem.Name.LocalName == "ff");
        }

        /// <summary>
        /// Determines if given settings object is ParallelPerceptronSettings
        /// </summary>
        /// <param name="settings">Non-recurrent-network settings</param>
        public static bool IsPP(INonRecurrentNetworkSettings settings)
        {
            return (settings.GetType() == typeof(ParallelPerceptronSettings));
        }

        /// <summary>
        /// Determines if given configuration element is ParallelPerceptronSettings element
        /// </summary>
        /// <param name="cfgElem">Configuration element</param>
        public static bool IsPPElem(XElement cfgElem)
        {
            return (cfgElem.Name.LocalName == "pp");
        }


        /// <summary>
        /// Instantiates appropriate non-recurrent-network settings from given xml element.
        /// Type of settings is determined using element local name.
        /// </summary>
        /// <param name="cfgElem">XML element containing settings</param>
        public static INonRecurrentNetworkSettings InstantiateSettings(XElement cfgElem)
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
                throw new InvalidOperationException($"Unsupported cfgElem name: {cfgElem.Name}");
            }
        }

        /// <summary>
        /// Loads collection of instantiated non-recurrent-network settings under given root element.
        /// If rootElem is null then empty collection is returned
        /// </summary>
        /// <param name="rootElem">Root element</param>
        public static List<INonRecurrentNetworkSettings> LoadSettingsCollection(XElement rootElem)
        {
            List<INonRecurrentNetworkSettings> settingsCollection = new List<INonRecurrentNetworkSettings>();
            if (rootElem != null)
            {
                foreach (XElement cfgElem in rootElem.Elements())
                {
                    if (IsFFElem(cfgElem) || IsPPElem(cfgElem))
                    {
                        settingsCollection.Add(InstantiateSettings(cfgElem));
                    }
                }
            }
            return settingsCollection;
        }

        /// <summary>
        /// Creates new network and associated trainer.
        /// </summary>
        /// <param name="settings">Non-recurrent-network settings</param>
        /// <param name="trainingInputVectors">Collection of training input samples</param>
        /// <param name="trainingOutputVectors">Collection of training output (desired) samples</param>
        /// <param name="rand">Random object to be used</param>
        /// <param name="net">Created network</param>
        /// <param name="trainer">Created associated trainer</param>
        public static void CreateNetworkAndTrainer(INonRecurrentNetworkSettings settings,
                                                   List<double[]> trainingInputVectors,
                                                   List<double[]> trainingOutputVectors,
                                                   Random rand,
                                                   out INonRecurrentNetwork net,
                                                   out INonRecurrentNetworkTrainer trainer
                                                   )
        {
            if (IsFF(settings))
            {
                //Feed forward network
                FeedForwardNetworkSettings netCfg = (FeedForwardNetworkSettings)settings;
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
                    throw new ArgumentException($"Unknown trainer {netCfg.TrainerCfg}");
                }
            }
            else if (IsPP(settings))
            {
                //Parallel perceptron network
                //Check output
                if (trainingOutputVectors[0].Length != 1)
                {
                    throw new InvalidOperationException($"Can't create ParallelPerceptron. Only single output value is allowed.");
                }
                ParallelPerceptronSettings netCfg = (ParallelPerceptronSettings)settings;
                ParallelPerceptron ppn = new ParallelPerceptron(trainingInputVectors[0].Length, netCfg);
                net = ppn;
                trainer = new PDeltaRuleTrainer(ppn, trainingInputVectors, trainingOutputVectors, netCfg.PDeltaRuleTrainerCfg, rand);
            }
            else
            {
                throw new InvalidOperationException($"Unknown network settings");
            }
            net.RandomizeWeights(rand);
            return;
        }

    }//NonRecurrentNetUtils

}//Namespace
