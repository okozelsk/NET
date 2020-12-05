using System;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Configuration of the binary feature filter
    /// </summary>
    [Serializable]
    public class BinFeatureFilterSettings : RCNetBaseSettings, IFeatureFilterSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "BinFeatureFilterType";

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        public BinFeatureFilterSettings()
        {
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public BinFeatureFilterSettings(BinFeatureFilterSettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public BinFeatureFilterSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public FeatureFilterBase.FeatureType Type { get { return FeatureFilterBase.FeatureType.Binary; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return true; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new BinFeatureFilterSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName), XsdTypeName);
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("binFeature", suppressDefaults);
        }


    }//BinFeatureFilterSettings

}//Namespace
