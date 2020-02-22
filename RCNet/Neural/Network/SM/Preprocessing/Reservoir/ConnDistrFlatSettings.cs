using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Class contains configuration of the flat connection distribution
    /// </summary>
    [Serializable]
    public class ConnDistrFlatSettings : RCNetBaseSettings, IConnDistrSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ConnDistrFlatType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public ConnDistrFlatSettings()
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ConnDistrFlatSettings(ConnDistrFlatSettings source)
            :this()
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ConnDistrFlatSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            return;
        }

        //Properties
        /// <summary>
        /// EE synapses ratio
        /// </summary>
        public double RatioEE { get { return 0.25d; } }
        
        /// <summary>
        /// EI synapses ratio
        /// </summary>
        public double RatioEI { get { return 0.25d; } }
        
        /// <summary>
        /// IE synapses ratio
        /// </summary>
        public double RatioIE { get { return 0.25d; } }
        
        /// <summary>
        /// II synapses ratio
        /// </summary>
        public double RatioII { get { return 0.25d; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return true;
            }
        }


        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ConnDistrFlatSettings(this);
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
            return GetXml("distributionFlat", suppressDefaults);
        }


    }//ConnFlatProbabilitiesSettings

}//Namespace
