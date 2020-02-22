using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.XmlTools;
using RCNet.Neural.Activation;
using RCNet.MathTools;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Collection of feed forward network's hidden layers settings
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
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public HiddenLayersSettings(XElement elem)
            :this()
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            foreach (XElement layerElem in settingsElem.Descendants("layer"))
            {
                HiddenLayerCfgCollection.Add(new HiddenLayerSettings(layerElem));
            }
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return HiddenLayerCfgCollection.Count == 0; } }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new HiddenLayersSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("hiddenLayers", suppressDefaults);
        }

    }//HiddenLayersSettings

}//Namespace
