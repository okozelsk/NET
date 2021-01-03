using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the default settings of classification and forecast tasks
    /// </summary>
    [Serializable]
    public class TaskDefaultsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutTaskDefaultsType";

        //Attribute properties
        /// <summary>
        /// Default configuration of the classification cluster chain (can be null)
        /// </summary>
        public TNRNetClusterChainSingleBoolSettings ClassificationClusterChainCfg { get; }

        /// <summary>
        /// Default configuration of the forecast cluster chain (can be null)
        /// </summary>
        public TNRNetClusterChainRealSettings ForecastClusterChainCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="classificationClusterChainCfg">Default configuration of the classification cluster chain (can be null)</param>
        /// <param name="forecastClusterChainCfg">Default configuration of the forecast cluster chain (can be null)</param>
        public TaskDefaultsSettings(TNRNetClusterChainSingleBoolSettings classificationClusterChainCfg,
                                    TNRNetClusterChainRealSettings forecastClusterChainCfg
                                    )
        {
            ClassificationClusterChainCfg = (TNRNetClusterChainSingleBoolSettings)classificationClusterChainCfg?.DeepClone();
            ForecastClusterChainCfg = (TNRNetClusterChainRealSettings)forecastClusterChainCfg?.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public TaskDefaultsSettings(TaskDefaultsSettings source)
            : this(source.ClassificationClusterChainCfg, source.ForecastClusterChainCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public TaskDefaultsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement classificationElem = settingsElem.Elements("classification").FirstOrDefault();
            ClassificationClusterChainCfg = classificationElem == null ? null : new TNRNetClusterChainSingleBoolSettings(classificationElem.Element("clusterChain"));
            XElement forecastElem = settingsElem.Elements("forecast").FirstOrDefault();
            ForecastClusterChainCfg = forecastElem == null ? null : new TNRNetClusterChainRealSettings(forecastElem.Element("clusterChain"));
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return ClassificationClusterChainCfg == null && ForecastClusterChainCfg == null; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new TaskDefaultsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (ClassificationClusterChainCfg != null)
            {
                XElement classificationElem = new XElement("classification");
                classificationElem.Add(ClassificationClusterChainCfg.GetXml(suppressDefaults));
                rootElem.Add(classificationElem);
            }
            if (ForecastClusterChainCfg != null)
            {
                XElement forecastElem = new XElement("forecast");
                forecastElem.Add(ForecastClusterChainCfg.GetXml(suppressDefaults));
                rootElem.Add(forecastElem);
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("taskDefaults", suppressDefaults);
        }

    }//TaskDefaultsSettings

}//Namespace
