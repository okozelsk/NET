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
    /// Startup parameters for the binary feature filter
    /// </summary>
    [Serializable]
    public class BinFeatureFilterSettings : BaseFeatureFilterSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "BinFeatureFilterCfgType";

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        public BinFeatureFilterSettings()
            :base(BaseFeatureFilter.FeatureType.Binary)
        {
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public BinFeatureFilterSettings(BinFeatureFilterSettings source)
            :base(source)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public BinFeatureFilterSettings(XElement elem)
            :base(BaseFeatureFilter.FeatureType.Binary)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public BinFeatureFilterSettings DeepClone()
        {
            return new BinFeatureFilterSettings(this);
        }

    }//BinFeatureFilterSettings

}//Namespace
