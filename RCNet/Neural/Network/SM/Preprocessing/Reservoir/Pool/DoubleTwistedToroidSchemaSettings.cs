using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Configuration of the Double Twisted Toroid interconnection schema of the pool's neurons.
    /// </summary>
    [Serializable]
    public class DoubleTwistedToroidSchemaSettings : RCNetBaseSettings, IInterconnSchemaSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PoolInterconnectionDoubleTwistedToroidSchemaType";
        //Default values
        /// <summary>
        /// The default value of the ratio of involved neurons.
        /// </summary>
        public const double DefaultRatio = 1d;
        /// <summary>
        /// The default value of the parameter specifying whether the left diagonal neurons to be self connected.
        /// </summary>
        public const bool DefaultLDiagonalSelf = false;
        /// <summary>
        /// The default value of the parameter specifying whether the right diagonal neurons to be self connected.
        /// </summary>
        public const bool DefaultRDiagonalSelf = false;
        /// <summary>
        /// The default value of the parameter specifying whether the connections of this schema will replace the existing connections.
        /// </summary>
        public const bool DefaultReplaceExistingConnections = true;
        /// <summary>
        /// The default number of applications of this schema.
        /// </summary>
        public const int DefaultRepetitions = 1;


        //Attribute properties
        /// <summary>
        /// The ratio of involved neurons.
        /// </summary>
        public double Ratio { get; }
        /// <summary>
        /// Specifies whether the left diagonal neurons to be self connected.
        /// </summary>
        public bool LDiagonalSelf { get; }
        /// <summary>
        /// Specifies whether the right diagonal neurons to be self connected.
        /// </summary>
        public bool RDiagonalSelf { get; }
        /// <inheritdoc/>
        public bool ReplaceExistingConnections { get; }
        /// <inheritdoc/>
        public int Repetitions { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="ratio">The ratio of involved neurons.</param>
        /// <param name="lDiagonalSelf">Specifies whether the left diagonal neurons to be self connected.</param>
        /// <param name="rDiagonalSelf">Specifies whether the right diagonal neurons to be self connected.</param>
        /// <param name="replaceExistingConnections">Specifies whether the connections of this schema will replace the existing connections.</param>
        /// <param name="repetitions">The number of applications of this schema.</param>
        public DoubleTwistedToroidSchemaSettings(double ratio = DefaultRatio,
                                                 bool lDiagonalSelf = DefaultLDiagonalSelf,
                                                 bool rDiagonalSelf = DefaultRDiagonalSelf,
                                                 bool replaceExistingConnections = DefaultReplaceExistingConnections,
                                                 int repetitions = DefaultRepetitions
                                                 )
        {
            Ratio = ratio;
            LDiagonalSelf = lDiagonalSelf;
            RDiagonalSelf = rDiagonalSelf;
            ReplaceExistingConnections = replaceExistingConnections;
            Repetitions = repetitions;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public DoubleTwistedToroidSchemaSettings(DoubleTwistedToroidSchemaSettings source)
            : this(source.Ratio, source.LDiagonalSelf, source.RDiagonalSelf, source.ReplaceExistingConnections, source.Repetitions)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public DoubleTwistedToroidSchemaSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Ratio
            Ratio = double.Parse(settingsElem.Attribute("ratio").Value, CultureInfo.InvariantCulture);
            //Additional connections
            LDiagonalSelf = bool.Parse(settingsElem.Attribute("lDiagonalSelf").Value);
            RDiagonalSelf = bool.Parse(settingsElem.Attribute("rDiagonalSelf").Value);
            //Will be replaced existing connections?
            ReplaceExistingConnections = bool.Parse(settingsElem.Attribute("replaceExistingConnections").Value);
            //Number of schema repetitions
            Repetitions = int.Parse(settingsElem.Attribute("repetitions").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRatio { get { return (Ratio == DefaultRatio); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultLDiagonalSelf { get { return (LDiagonalSelf == DefaultLDiagonalSelf); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRDiagonalSelf { get { return (RDiagonalSelf == DefaultRDiagonalSelf); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultReplaceExistingConnections { get { return (ReplaceExistingConnections == DefaultReplaceExistingConnections); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRepetitions { get { return (Repetitions == DefaultRepetitions); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultRatio &&
                       IsDefaultLDiagonalSelf &&
                       IsDefaultRDiagonalSelf &&
                       IsDefaultReplaceExistingConnections &&
                       IsDefaultRepetitions;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Ratio <= 0 || Ratio > 1)
            {
                throw new ArgumentException($"Invalid Ratio {Ratio.ToString(CultureInfo.InvariantCulture)}. Ratio must be GT 0 and LE to 1.", "Ratio");
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
            return new DoubleTwistedToroidSchemaSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultRatio)
            {
                rootElem.Add(new XAttribute("ratio", Ratio.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultLDiagonalSelf)
            {
                rootElem.Add(new XAttribute("lDiagonalSelf", LDiagonalSelf.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultRDiagonalSelf)
            {
                rootElem.Add(new XAttribute("rDiagonalSelf", RDiagonalSelf.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
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
            return GetXml("doubleTwistedToroidSchema", suppressDefaults);
        }

    }//DoubleTwistedToroidSchemaSettings

}//Namespace
