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
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Class contains configuration of the Chain schema of pool's neurons interconnection
    /// </summary>
    [Serializable]
    public class ChainSchemaSettings : RCNetBaseSettings, IInterconnSchemaSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolInterconnectionChainSchemaType";
        //Default values
        /// <summary>
        /// Default ratio of involved neurons
        /// </summary>
        public const double DefaultRatio = 1d;
        /// <summary>
        /// Default circle shape
        /// </summary>
        public const bool DefaultCircle = true;
        /// <summary>
        /// Default replacement of existing connections
        /// </summary>
        public const bool DefaultReplaceExistingConnections = true;
        /// <summary>
        /// Default number of schema repetitions
        /// </summary>
        public const int DefaultRepetitions = 1;


        //Attribute properties
        /// <summary>
        /// Ratio of involved neurons.
        /// </summary>
        public double Ratio { get; }
        /// <summary>
        /// Specifies whether the chain will be closed to circle
        /// </summary>
        public bool Circle { get; }
        /// <summary>
        /// Specifies whether connections of this schema will replace existing connections
        /// </summary>
        public bool ReplaceExistingConnections { get; }
        /// <summary>
        /// Number of applications of this schema
        /// </summary>
        public int Repetitions { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="ratio">Ratio of involved neurons</param>
        /// <param name="circle">Specifies whether the chain will be closed to circle</param>
        /// <param name="replaceExistingConnections">Specifies whether connections of this schema will replace existing connections</param>
        /// <param name="repetitions">Number of applications of this schema</param>
        public ChainSchemaSettings(double ratio = DefaultRatio,
                                                      bool circle = DefaultCircle,
                                                      bool replaceExistingConnections = DefaultReplaceExistingConnections,
                                                      int repetitions = DefaultRepetitions
                                                      )
        {
            Ratio = ratio;
            Circle = circle;
            ReplaceExistingConnections = replaceExistingConnections;
            Repetitions = repetitions;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ChainSchemaSettings(ChainSchemaSettings source)
            :this(source.Ratio, source.Circle, source.ReplaceExistingConnections, source.Repetitions)
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
        public ChainSchemaSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Ratio
            Ratio = double.Parse(settingsElem.Attribute("ratio").Value, CultureInfo.InvariantCulture);
            //Will be chain closed to circle?
            Circle = bool.Parse(settingsElem.Attribute("circle").Value);
            //Will be replaced existing connections?
            ReplaceExistingConnections = bool.Parse(settingsElem.Attribute("replaceExistingConnections").Value);
            //Number of schema repetitions
            Repetitions = int.Parse(settingsElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRatio { get { return (Ratio == DefaultRatio); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultCircle { get { return (Circle == DefaultCircle); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultReplaceExistingConnections { get { return (ReplaceExistingConnections == DefaultReplaceExistingConnections); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRepetitions { get { return (Repetitions == DefaultRepetitions); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultRatio &&
                       IsDefaultCircle &&
                       IsDefaultReplaceExistingConnections &&
                       IsDefaultRepetitions;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Ratio < 0 || Ratio > 1)
            {
                throw new Exception($"Invalid Ratio {Ratio.ToString(CultureInfo.InvariantCulture)}. Ratio must be GE to 0 and LE to 1.");
            }
            if (Repetitions < 1)
            {
                throw new Exception($"Invalid Repetitions {Repetitions.ToString(CultureInfo.InvariantCulture)}. Repetitions must be GT 0.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ChainSchemaSettings(this);
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
            if (!suppressDefaults || !IsDefaultRatio)
            {
                rootElem.Add(new XAttribute("ratio", Ratio.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultCircle)
            {
                rootElem.Add(new XAttribute("circle", Circle.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultReplaceExistingConnections)
            {
                rootElem.Add(new XAttribute("replaceExistingConnections", ReplaceExistingConnections.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultRepetitions)
            {
                rootElem.Add(new XAttribute("repetitions", Repetitions.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("chainSchema", suppressDefaults);
        }

    }//ChainSchemaSettings

}//Namespace
