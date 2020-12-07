using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Network.NonRecurrent;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the ReadoutLayer
    /// </summary>
    [Serializable]
    public class ReadoutLayerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerType";

        //Attribute properties
        /// <summary>
        /// Configuration of the cluster associated with the readout unit
        /// </summary>
        public ClusterSettings ClusterCfg { get; }

        /// <summary>
        /// Readout units configuration
        /// </summary>
        public ReadoutUnitsSettings ReadoutUnitsCfg { get; }

        /// <summary>
        /// Configuration of the decision maker applied on "One winner" groups
        /// </summary>
        public OneWinnerDecisionMakerSettings OneWinnerDecisionMakerCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="clusterCfg">Configuration of the cluster associated with the readout unit</param>
        /// <param name="readoutUnitsCfg">Readout units configuration</param>
        /// <param name="oneWinnerDecisionMakerCfg">Configuration of the decision maker applied on "One winner" groups</param>
        public ReadoutLayerSettings(ClusterSettings clusterCfg,
                                    ReadoutUnitsSettings readoutUnitsCfg,
                                    OneWinnerDecisionMakerSettings oneWinnerDecisionMakerCfg = null
                                    )
        {
            ClusterCfg = (ClusterSettings)clusterCfg.DeepClone();
            ReadoutUnitsCfg = (ReadoutUnitsSettings)readoutUnitsCfg.DeepClone();
            OneWinnerDecisionMakerCfg = (OneWinnerDecisionMakerSettings)oneWinnerDecisionMakerCfg?.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutLayerSettings(ReadoutLayerSettings source)
            : this(source.ClusterCfg, source.ReadoutUnitsCfg, source.OneWinnerDecisionMakerCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// This is the preferred way to instantiate ReadoutLayer settings.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ReadoutLayerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Cluster
            ClusterCfg = new ClusterSettings(settingsElem.Element("cluster"));
            //Readout units
            XElement readoutUnitsElem = settingsElem.Elements("readoutUnits").First();
            ReadoutUnitsCfg = new ReadoutUnitsSettings(readoutUnitsElem);
            //One winner decision maker
            XElement oneWinnerDecisionMakerElem = settingsElem.Elements("oneWinnerDecisionMaker").FirstOrDefault();
            OneWinnerDecisionMakerCfg = oneWinnerDecisionMakerElem == null ? null : new OneWinnerDecisionMakerSettings(oneWinnerDecisionMakerElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Collection of names of output fields
        /// </summary>
        public List<string> OutputFieldNameCollection
        {
            get
            {
                return (from rus in ReadoutUnitsCfg.ReadoutUnitCfgCollection select rus.Name).ToList();
            }
        }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            foreach (ReadoutUnitSettings rus in ReadoutUnitsCfg.ReadoutUnitCfgCollection)
            {
                if (rus.TaskCfg.NetworkCfgCollection.Count == 0)
                {
                    if (ClusterCfg.DefaultNetworksCfg.GetTaskNetworksCfgs(rus.TaskCfg.Type).Count == 0)
                    {
                        throw new ArgumentException($"Readout unit {rus.Name} has not associated network(s) settings.", "ReadoutUnitsCfg");
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Return network configurations associated with readout unit or default network configurations if no specific network configurations.
        /// </summary>
        /// <param name="readoutUnitIndex">Index of the readout unit</param>
        /// <returns></returns>
        public List<INonRecurrentNetworkSettings> GetReadoutUnitNetworksCollection(int readoutUnitIndex)
        {
            if (ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitIndex].TaskCfg.NetworkCfgCollection.Count > 0)
            {
                return ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitIndex].TaskCfg.NetworkCfgCollection;
            }
            else
            {
                return ClusterCfg.DefaultNetworksCfg.GetTaskNetworksCfgs(ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitIndex].TaskCfg.Type);
            }
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutLayerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, ClusterCfg.GetXml(suppressDefaults));
            rootElem.Add(ReadoutUnitsCfg.GetXml(suppressDefaults));
            if (OneWinnerDecisionMakerCfg != null)
            {
                rootElem.Add(OneWinnerDecisionMakerCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("readoutLayer", suppressDefaults);
        }


    }//ReadoutLayerSettings

}//Namespace
