using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// The collection of the feed forward network's hidden layers configurations
    /// </summary>
    [Serializable]
    public class HiddenLayersSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "FFNetHiddenLayersType";

        //Attribute properties
        /// <summary>
        /// Collection of classification networks settings
        /// </summary>
        public List<HiddenLayerSettings> HiddenLayerCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public HiddenLayersSettings()
        {
            HiddenLayerCfgCollection = new List<HiddenLayerSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="hiddenLayersCfgs">Collection of hidden layers settings</param>
        public HiddenLayersSettings(IEnumerable<HiddenLayerSettings> hiddenLayersCfgs)
            : this()
        {
            foreach (HiddenLayerSettings hlcfg in hiddenLayersCfgs)
            {
                HiddenLayerCfgCollection.Add((HiddenLayerSettings)hlcfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="hiddenLayersCfgs">Hidden layer settings</param>
        public HiddenLayersSettings(params HiddenLayerSettings[] hiddenLayersCfgs)
            : this()
        {
            foreach (HiddenLayerSettings hlcfg in hiddenLayersCfgs)
            {
                HiddenLayerCfgCollection.Add((HiddenLayerSettings)hlcfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public HiddenLayersSettings(HiddenLayersSettings source)
            : this()
        {
            foreach (HiddenLayerSettings hlcfg in source.HiddenLayerCfgCollection)
            {
                HiddenLayerCfgCollection.Add((HiddenLayerSettings)hlcfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public HiddenLayersSettings(XElement elem)
            : this()
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            foreach (XElement layerElem in settingsElem.Elements("layer"))
            {
                HiddenLayerCfgCollection.Add(new HiddenLayerSettings(layerElem));
            }
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return HiddenLayerCfgCollection.Count == 0; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new HiddenLayersSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (HiddenLayerSettings hlcfg in HiddenLayerCfgCollection)
            {
                rootElem.Add(hlcfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("hiddenLayers", suppressDefaults);
        }

    }//HiddenLayersSettings

}//Namespace
