using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Data.Filter
{
    /// <summary>
    /// Startup parameters for the real number feature filter
    /// </summary>
    [Serializable]
    public class RealFeatureFilterSettings : BaseFeatureFilterSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "RealFeatureFilterCfgType";

        //Attribute properties
        /// <summary>
        /// Standardize?
        /// </summary>
        public bool Standardize { get; set; }
        /// <summary>
        /// Keep range reserve?
        /// </summary>
        public bool KeepReserve { get; set; }
        /// <summary>
        /// Keep sign?
        /// </summary>
        public bool KeepSign { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="standardize">Standardize?</param>
        /// <param name="keepReserve">Keep range reserve?</param>
        /// <param name="keepSign">Keep original sign?</param>
        public RealFeatureFilterSettings(bool standardize, bool keepReserve, bool keepSign)
            :base(BaseFeatureFilter.FeatureType.Real)
        {
            Standardize = standardize;
            KeepReserve = keepReserve;
            KeepSign = keepSign;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RealFeatureFilterSettings(RealFeatureFilterSettings source)
            :base(source)
        {
            Standardize = source.Standardize;
            KeepReserve = source.KeepReserve;
            KeepSign = source.KeepSign;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public RealFeatureFilterSettings(XElement elem)
            :base(BaseFeatureFilter.FeatureType.Real)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Standardize = bool.Parse(settingsElem.Attribute("standardize").Value);
            KeepReserve = bool.Parse(settingsElem.Attribute("keepReserve").Value);
            KeepSign = bool.Parse(settingsElem.Attribute("keepSign").Value);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public RealFeatureFilterSettings DeepClone()
        {
            return new RealFeatureFilterSettings(this);
        }

    }//RealFeatureFilterSettings

}//Namespace
