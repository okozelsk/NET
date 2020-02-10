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
    /// Startup parameters for the enumeration feature filter
    /// </summary>
    [Serializable]
    public class EnumFeatureFilterSettings : BaseFeatureFilterSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "EnumFeatureFilterCfgType";

        //Attribute properties
        /// <summary>
        /// Number of enum elements
        /// </summary>
        public int NumOfElements { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfElements">Number of feature's enumerated elements</param>
        public EnumFeatureFilterSettings(int numOfElements)
            :base(BaseFeatureFilter.FeatureType.Enum)
        {
            NumOfElements = numOfElements;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public EnumFeatureFilterSettings(EnumFeatureFilterSettings source)
            :base(source)
        {
            NumOfElements = source.NumOfElements;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public EnumFeatureFilterSettings(XElement elem)
            :base(BaseFeatureFilter.FeatureType.Enum)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfElements = int.Parse(settingsElem.Attribute("numOfElements").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public EnumFeatureFilterSettings DeepClone()
        {
            return new EnumFeatureFilterSettings(this);
        }

    }//EnumFeatureFilterSettings

}//Namespace
