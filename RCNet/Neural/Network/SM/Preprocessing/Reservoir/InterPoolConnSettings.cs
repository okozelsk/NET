using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the inter-pool connection
    /// </summary>
    [Serializable]
    public class InterPoolConnSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ResStructInterPoolConnectionType";
        //Default values
        /// <summary>
        /// Default keep constant number of incoming synapses
        /// </summary>
        public const bool DefaultConstantNumOfConnections = false;


        //Attribute properties
        /// <summary>
        /// Name of the target pool
        /// </summary>
        public string TargetPoolName { get; }
        /// <summary>
        /// Determines how many neurons in the target pool will get connected to neurons from source pool
        /// </summary>
        public double TargetConnectionDensity { get; }
        /// <summary>
        /// Name of the source pool
        /// </summary>
        public string SourcePoolName { get; }
        /// <summary>
        /// Determines how many neurons from the source pool will be connected to one neuron from target pool
        /// </summary>
        public double SourceConnectionDensity { get; }
        /// <summary>
        /// Specifies whether to keep for each neuron from source pool constant number of synapses
        /// </summary>
        public bool ConstantNumOfConnections { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="targetPoolName">Name of the target pool</param>
        /// <param name="targetConnectionDensity">Determines how many neurons in the target pool will get connected to neurons from source pool</param>
        /// <param name="sourcePoolName">Name of the source pool</param>
        /// <param name="sourceConnectionDensity">Determines how many neurons from the source pool will be connected to one neuron from target pool</param>
        /// <param name="constantNumOfConnections">Specifies whether to keep constant number of synapses per target neuron</param>
        public InterPoolConnSettings(string targetPoolName,
                                     double targetConnectionDensity,
                                     string sourcePoolName,
                                     double sourceConnectionDensity,
                                     bool constantNumOfConnections = DefaultConstantNumOfConnections
                                     )
        {
            TargetPoolName = targetPoolName;
            TargetConnectionDensity = targetConnectionDensity;
            SourcePoolName = sourcePoolName;
            SourceConnectionDensity = sourceConnectionDensity;
            ConstantNumOfConnections = constantNumOfConnections;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public InterPoolConnSettings(InterPoolConnSettings source)
            : this(source.TargetPoolName, source.TargetConnectionDensity,
                  source.SourcePoolName, source.SourceConnectionDensity,
                  source.ConstantNumOfConnections
                 )
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public InterPoolConnSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            TargetPoolName = settingsElem.Attribute("targetPool").Value;
            TargetConnectionDensity = double.Parse(settingsElem.Attribute("targetConnDensity").Value, CultureInfo.InvariantCulture);
            SourcePoolName = settingsElem.Attribute("sourcePool").Value;
            SourceConnectionDensity = double.Parse(settingsElem.Attribute("sourceConnDensity").Value, CultureInfo.InvariantCulture);
            ConstantNumOfConnections = bool.Parse(settingsElem.Attribute("constantNumOfConnections").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultConstantNumOfConnections { get { return (ConstantNumOfConnections == DefaultConstantNumOfConnections); } }

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
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (TargetPoolName.Length == 0)
            {
                throw new ArgumentException($"TargetPoolName can not be empty.", "TargetPoolName");
            }
            if (TargetConnectionDensity < 0 || TargetConnectionDensity > 1)
            {
                throw new ArgumentException($"Invalid TargetConnectionDensity {TargetConnectionDensity.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0 and LE to 1.", "TargetConnectionDensity");
            }
            if (SourcePoolName.Length == 0)
            {
                throw new ArgumentException($"SourcePoolName can not be empty.", "SourcePoolName");
            }
            if (SourceConnectionDensity < 0 || SourceConnectionDensity > 1)
            {
                throw new ArgumentException($"Invalid SourceConnectionDensity {SourceConnectionDensity.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0 and LE to 1.", "SourceConnectionDensity");
            }
            if (SourcePoolName == TargetPoolName)
            {
                throw new ArgumentException($"SourcePoolName and TargetPoolName can not be the same.", "SourcePoolName");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InterPoolConnSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("targetPool", TargetPoolName),
                                             new XAttribute("targetConnDensity", TargetConnectionDensity.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("sourcePool", SourcePoolName),
                                             new XAttribute("sourceConnDensity", SourceConnectionDensity.ToString(CultureInfo.InvariantCulture))
                                             );
            if (!suppressDefaults || !IsDefaultConstantNumOfConnections)
            {
                rootElem.Add(new XAttribute("constantNumOfConnections", ConstantNumOfConnections.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("interPoolConnection", suppressDefaults);
        }


    }//InterPoolConnSettings

}//Namespace
