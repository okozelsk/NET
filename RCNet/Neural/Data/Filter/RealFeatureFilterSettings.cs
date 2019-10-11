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

        //Attribute properties
        /// <summary>
        /// Standardize?
        /// </summary>
        public bool Standardize { get; set; }
        /// <summary>
        /// Keep range reserve?
        /// </summary>
        public bool KeepReserve { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="standardize">Standardize?</param>
        /// <param name="keepReserve">Keep range reserve?</param>
        public RealFeatureFilterSettings(bool standardize, bool keepReserve)
            :base(BaseFeatureFilter.FeatureType.Real)
        {
            Standardize = standardize;
            KeepReserve = keepReserve;
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Data.Filter.RealFeatureFilterSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Standardize = bool.Parse(settingsElem.Attribute("standardize").Value);
            KeepReserve = bool.Parse(settingsElem.Attribute("keepReserve").Value);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RealFeatureFilterSettings cmpSettings = obj as RealFeatureFilterSettings;
            if (Standardize != cmpSettings.Standardize || KeepReserve != cmpSettings.KeepReserve)
            {
                return false;
            }
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
        public RealFeatureFilterSettings DeepClone()
        {
            return new RealFeatureFilterSettings(this);
        }

    }//RealFeatureFilterSettings

}//Namespace
