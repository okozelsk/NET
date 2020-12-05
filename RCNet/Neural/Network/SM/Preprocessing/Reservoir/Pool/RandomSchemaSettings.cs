using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Configuration of the Random schema of pool's neurons interconnection
    /// </summary>
    [Serializable]
    public class RandomSchemaSettings : RCNetBaseSettings, IInterconnSchemaSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolInterconnectionRandomSchemaType";
        //Default values
        /// <summary>
        /// Default interconnection density
        /// </summary>
        public const double DefaultDensity = 0.1d;
        /// <summary>
        /// Default average distance of interconnected neurons - code
        /// </summary>
        public const string DefaultAvgDistanceCode = "NA";
        /// <summary>
        /// Default average distance of interconnected neurons - numeric value
        /// </summary>
        public const double DefaultAvgDistanceNum = 0;
        /// <summary>
        /// Default allow self connection
        /// </summary>
        public const bool DefaultAllowSelfConnection = true;
        /// <summary>
        /// Default keep constant number of incoming synapses
        /// </summary>
        public const bool DefaultConstantNumOfConnections = false;
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
        /// Density of interconnected neurons.
        /// Each pool neuron will be connected as a source neuron for Pool.Size * Density neurons.
        /// </summary>
        public double Density { get; }
        /// <summary>
        /// Average distance of interconnected neurons.
        /// 0 means random distance.
        /// </summary>
        public double AvgDistance { get; }
        /// <summary>
        /// Specifies whether to allow neurons to be self connected
        /// </summary>
        public bool AllowSelfConnection { get; }
        /// <summary>
        /// Specifies whether to keep constant number of synapses per target neuron
        /// </summary>
        public bool ConstantNumOfConnections { get; }
        /// <inheritdoc/>
        public bool ReplaceExistingConnections { get; }
        /// <inheritdoc/>
        public int Repetitions { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="density">Density of interconnected neurons</param>
        /// <param name="avgDistance">Average distance of interconnected neurons</param>
        /// <param name="allowSelfConnection">Specifies whether to allow neurons to be self connected</param>
        /// <param name="constantNumOfConnections">Specifies whether to keep constant number of synapses per target neuron</param>
        /// <param name="replaceExistingConnections">Specifies whether the connections of this schema will replace existing connections</param>
        /// <param name="repetitions">Number of applications of this schema</param>
        public RandomSchemaSettings(double density = DefaultDensity,
                                    double avgDistance = DefaultAvgDistanceNum,
                                    bool allowSelfConnection = DefaultAllowSelfConnection,
                                    bool constantNumOfConnections = DefaultConstantNumOfConnections,
                                    bool replaceExistingConnections = DefaultReplaceExistingConnections,
                                    int repetitions = DefaultRepetitions
                                    )
        {
            Density = density;
            AvgDistance = avgDistance;
            AllowSelfConnection = allowSelfConnection;
            ConstantNumOfConnections = constantNumOfConnections;
            ReplaceExistingConnections = replaceExistingConnections;
            Repetitions = repetitions;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RandomSchemaSettings(RandomSchemaSettings source)
            : this(source.Density, source.AvgDistance, source.AllowSelfConnection,
                  source.ConstantNumOfConnections, source.ReplaceExistingConnections, source.Repetitions)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public RandomSchemaSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Density
            Density = double.Parse(settingsElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            //Average distance
            AvgDistance = settingsElem.Attribute("avgDistance").Value == DefaultAvgDistanceCode ? DefaultAvgDistanceNum : double.Parse(settingsElem.Attribute("avgDistance").Value, CultureInfo.InvariantCulture);
            //Allow self connections?
            AllowSelfConnection = bool.Parse(settingsElem.Attribute("allowSelfConnection").Value);
            //Will each neuron have the same number of connections?
            ConstantNumOfConnections = bool.Parse(settingsElem.Attribute("constantNumOfConnections").Value);
            //Will be replaced existing connections?
            ReplaceExistingConnections = bool.Parse(settingsElem.Attribute("replaceExistingConnections").Value);
            //Number of schema repetitions
            Repetitions = int.Parse(settingsElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultDensity { get { return (Density == DefaultDensity); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultAvgDistance { get { return (AvgDistance == DefaultAvgDistanceNum); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultAllowSelfConnection { get { return (AllowSelfConnection == DefaultAllowSelfConnection); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultConstantNumOfConnections { get { return (ConstantNumOfConnections == DefaultConstantNumOfConnections); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultReplaceExistingConnections { get { return (ReplaceExistingConnections == DefaultReplaceExistingConnections); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultRepetitions { get { return (Repetitions == DefaultRepetitions); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultDensity &&
                       IsDefaultAvgDistance &&
                       IsDefaultAllowSelfConnection &&
                       IsDefaultConstantNumOfConnections &&
                       IsDefaultReplaceExistingConnections &&
                       IsDefaultRepetitions;
            }
        }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Density <= 0 || Density > 1)
            {
                throw new ArgumentException($"Invalid Density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GT 0 and LE to 1.", "Density");
            }
            if (AvgDistance < 0)
            {
                throw new ArgumentException($"Invalid AvgDistance {AvgDistance.ToString(CultureInfo.InvariantCulture)}. AvgDistance must be GE to 0.", "AvgDistance");
            }
            if (Repetitions < 1)
            {
                throw new ArgumentException($"Invalid Repetitions {Repetitions.ToString(CultureInfo.InvariantCulture)}. Repetitions must be GT 0.", "Repetitions");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new RandomSchemaSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultDensity)
            {
                rootElem.Add(new XAttribute("density", Density.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultAvgDistance)
            {
                rootElem.Add(new XAttribute("avgDistance", IsDefaultAvgDistance ? DefaultAvgDistanceCode : AvgDistance.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultAllowSelfConnection)
            {
                rootElem.Add(new XAttribute("allowSelfConnection", AllowSelfConnection.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultConstantNumOfConnections)
            {
                rootElem.Add(new XAttribute("constantNumOfConnections", ConstantNumOfConnections.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("randomSchema", suppressDefaults);
        }


    }//RandomSchemaSettings

}//Namespace
