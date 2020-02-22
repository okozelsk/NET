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
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Synapse;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Class contains configuration of the custom connection distribution
    /// </summary>
    [Serializable]
    public class ConnDistrCustomSettings : RCNetBaseSettings, IConnDistrSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ConnDistrCustomType";

        //Attribute properties
        /// <summary>
        /// EE synapses relative share
        /// </summary>
        public double RelShareEE { get; }
        /// <summary>
        /// EI synapses relative share
        /// </summary>
        public double RelShareEI { get; }
        /// <summary>
        /// IE synapses relative share
        /// </summary>
        public double RelShareIE { get; }
        /// <summary>
        /// II synapses relative share
        /// </summary>
        public double RelShareII { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="relShareEE">EE synapses relative share</param>
        /// <param name="relShareEI">EI synapses relative share</param>
        /// <param name="relShareIE">IE synapses relative share</param>
        /// <param name="relShareII">II synapses relative share</param>
        public ConnDistrCustomSettings(double relShareEE,
                                               double relShareEI,
                                               double relShareIE,
                                               double relShareII
                                               )
        {
            RelShareEE = relShareEE;
            RelShareEI = relShareEI;
            RelShareIE = relShareIE;
            RelShareII = relShareII;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ConnDistrCustomSettings(ConnDistrCustomSettings source)
            :this(source.RelShareEE, source.RelShareEI, source.RelShareIE, source.RelShareII)
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
        public ConnDistrCustomSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Relative shares
            RelShareEE = double.Parse(settingsElem.Attribute("relShareEE").Value, CultureInfo.InvariantCulture);
            RelShareEI = double.Parse(settingsElem.Attribute("relShareEI").Value, CultureInfo.InvariantCulture);
            RelShareIE = double.Parse(settingsElem.Attribute("relShareIE").Value, CultureInfo.InvariantCulture);
            RelShareII = double.Parse(settingsElem.Attribute("relShareII").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        private double RelShareSum { get { return RelShareEE + RelShareEI + RelShareIE + RelShareII; } }

        /// <summary>
        /// EE synapses ratio
        /// </summary>
        public double RatioEE { get { return RelShareEE / RelShareSum; } }
        
        /// <summary>
        /// EI synapses ratio
        /// </summary>
        public double RatioEI { get { return RelShareEI / RelShareSum; } }
        
        /// <summary>
        /// IE synapses ratio
        /// </summary>
        public double RatioIE { get { return RelShareIE / RelShareSum; } }
        
        /// <summary>
        /// II synapses ratio
        /// </summary>
        public double RatioII { get { return RelShareII / RelShareSum; } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (RelShareEE < 0)
            {
                throw new Exception($"Invalid RelShareEE {RelShareEE.ToString(CultureInfo.InvariantCulture)}. RelShareEE must be GE to 0.");
            }
            if (RelShareEI < 0)
            {
                throw new Exception($"Invalid RelShareEI {RelShareEI.ToString(CultureInfo.InvariantCulture)}. RelShareEI must be GE to 0.");
            }
            if (RelShareIE < 0)
            {
                throw new Exception($"Invalid RelShareEI {RelShareIE.ToString(CultureInfo.InvariantCulture)}. RelShareIE must be GE to 0.");
            }
            if (RelShareII < 0)
            {
                throw new Exception($"Invalid RelShareII {RelShareII.ToString(CultureInfo.InvariantCulture)}. RelShareII must be GE to 0.");
            }
            if (RelShareSum == 0)
            {
                throw new Exception($"Invalid sum of RelShareXX. Sum of RelShareXX must be GT 0.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ConnDistrCustomSettings(this);
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
            rootElem.Add(new XAttribute("relShareEE", RelShareEE.ToString(CultureInfo.InvariantCulture)));
            rootElem.Add(new XAttribute("relShareEI", RelShareEI.ToString(CultureInfo.InvariantCulture)));
            rootElem.Add(new XAttribute("relShareIE", RelShareIE.ToString(CultureInfo.InvariantCulture)));
            rootElem.Add(new XAttribute("relShareII", RelShareII.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("distributionCustom", suppressDefaults);
        }


    }//ConnCustomProbabilitiesSettings

}//Namespace
