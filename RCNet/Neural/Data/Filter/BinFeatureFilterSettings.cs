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
    public class BinFeatureFilterSettings : FeatureFilterSettings
    {
        //Constants

        //Attribute properties

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        public BinFeatureFilterSettings()
            :base(FeatureFilter.FeatureType.Binary)
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
            :base(FeatureFilter.FeatureType.Binary)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Data.Filter.BinFeatureFilterSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            return;
        }

        //Properties

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return base.Equals(obj);
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public BinFeatureFilterSettings DeepClone()
        {
            return new BinFeatureFilterSettings(this);
        }

    }//BinFeatureFilterSettings

}//Namespace
