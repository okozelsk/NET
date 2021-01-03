using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the feed forward network's hidden layers.
    /// </summary>
    [Serializable]
    public class HiddenLayersSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "FFNetHiddenLayersType";

        //Attribute properties
        /// <summary>
        /// The collection of hidden layer configurations.
        /// </summary>
        public List<HiddenLayerSettings> HiddenLayerCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public HiddenLayersSettings()
        {
            HiddenLayerCfgCollection = new List<HiddenLayerSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="hiddenLayersCfgs">The collection of hidden layer configurations.</param>
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="hiddenLayersCfgs">The configurations of the hidden layers.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
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
        /// <param name="elem">A xml element containing the configuration data.</param>
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
