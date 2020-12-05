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
        /// Crossvalidation configuration
        /// </summary>
        public CrossvalidationSettings CrossvalidationCfg { get; }

        /// <summary>
        /// Task dependent networks settings to be applied when specific networks for readout unit are not specified
        /// </summary>
        public DefaultNetworksSettings DefaultNetworksCfg { get; }

        /// <summary>
        /// Readout units configuration
        /// </summary>
        public ReadoutUnitsSettings ReadoutUnitsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="crossvalidationCfg">Crossvalidation configuration</param>
        /// <param name="readoutUnitsCfg">Readout units configuration</param>
        /// <param name="defaultNetworksCfg">Task dependent networks settings to be applied when specific networks for readout unit are not specified</param>
        public ReadoutLayerSettings(CrossvalidationSettings crossvalidationCfg,
                                    ReadoutUnitsSettings readoutUnitsCfg,
                                    DefaultNetworksSettings defaultNetworksCfg = null
                                    )
        {
            CrossvalidationCfg = (CrossvalidationSettings)crossvalidationCfg.DeepClone();
            ReadoutUnitsCfg = (ReadoutUnitsSettings)readoutUnitsCfg.DeepClone();
            DefaultNetworksCfg = defaultNetworksCfg == null ? new DefaultNetworksSettings() : (DefaultNetworksSettings)defaultNetworksCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutLayerSettings(ReadoutLayerSettings source)
            : this(source.CrossvalidationCfg, source.ReadoutUnitsCfg, source.DefaultNetworksCfg)
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
            //Crossvalidation
            CrossvalidationCfg = new CrossvalidationSettings(settingsElem.Element("crossvalidation"));
            //Default networks settings
            XElement defaultNetworksElem = settingsElem.Elements("defaultNetworks").FirstOrDefault();
            DefaultNetworksCfg = defaultNetworksElem == null ? new DefaultNetworksSettings() : new DefaultNetworksSettings(defaultNetworksElem);
            //Readout units
            XElement readoutUnitsElem = settingsElem.Elements("readoutUnits").First();
            ReadoutUnitsCfg = new ReadoutUnitsSettings(readoutUnitsElem);
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
                    if (DefaultNetworksCfg.GetTaskNetworksCfgs(rus.TaskCfg.Type).Count == 0)
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
                return DefaultNetworksCfg.GetTaskNetworksCfgs(ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitIndex].TaskCfg.Type);
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
            XElement rootElem = new XElement(rootElemName, CrossvalidationCfg.GetXml(suppressDefaults));
            if (!DefaultNetworksCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(DefaultNetworksCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(ReadoutUnitsCfg.GetXml(suppressDefaults));
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
