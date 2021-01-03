using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the inter-pool connection.
    /// </summary>
    [Serializable]
    public class InterPoolConnSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ResStructInterPoolConnectionType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying whether to keep constant number of connections from source neurons to target neuron.
        /// </summary>
        public const bool DefaultConstantNumOfConnections = false;


        //Attribute properties
        /// <summary>
        /// The name of the target pool.
        /// </summary>
        public string TargetPoolName { get; }
        /// <summary>
        /// Determines how many neurons in the target pool get connected source pool neurons.
        /// </summary>
        public double TargetConnectionDensity { get; }
        /// <summary>
        /// The name of the source pool.
        /// </summary>
        public string SourcePoolName { get; }
        /// <summary>
        /// Determines how many neurons from the source pool to be connected to one neuron from target pool.
        /// </summary>
        public double SourceConnectionDensity { get; }
        /// <summary>
        /// Specifies whether to keep constant number of connections from source neurons to target neuron.
        /// </summary>
        public bool ConstantNumOfConnections { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="targetPoolName">The name of the target pool.</param>
        /// <param name="targetConnectionDensity">Determines how many neurons in the target pool get connected source pool neurons.</param>
        /// <param name="sourcePoolName">The name of the source pool.</param>
        /// <param name="sourceConnectionDensity">Determines how many neurons from the source pool to be connected to one neuron from target pool.</param>
        /// <param name="constantNumOfConnections">Specifies whether to keep constant number of connections from source neurons to target neuron.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
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
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultConstantNumOfConnections { get { return (ConstantNumOfConnections == DefaultConstantNumOfConnections); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }


        //Methods
        /// <inheritdoc />
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

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new InterPoolConnSettings(this);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("interPoolConnection", suppressDefaults);
        }


    }//InterPoolConnSettings

}//Namespace
