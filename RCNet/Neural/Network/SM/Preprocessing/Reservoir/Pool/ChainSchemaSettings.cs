using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool
{
    /// <summary>
    /// Configuration of the Chain interconnection schema of the pool's neurons.
    /// </summary>
    [Serializable]
    public class ChainSchemaSettings : RCNetBaseSettings, IInterconnSchemaSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PoolInterconnectionChainSchemaType";
        //Default values
        /// <summary>
        /// The default value of the ratio of involved neurons.
        /// </summary>
        public const double DefaultRatio = 1d;
        /// <summary>
        /// Th default value of the parameter specifying whether the chain will be closed to a circle.
        /// </summary>
        public const bool DefaultCircle = true;
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
        /// Specifies whether the chain will be closed to a circle.
        /// </summary>
        public bool Circle { get; }
        /// <inheritdoc/>
        public bool ReplaceExistingConnections { get; }
        /// <inheritdoc/>
        public int Repetitions { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="ratio">The ratio of involved neurons.</param>
        /// <param name="circle">Specifies whether the chain will be closed to a circle.</param>
        /// <param name="replaceExistingConnections">Specifies whether the connections of this schema will replace the existing connections.</param>
        /// <param name="repetitions">The number of applications of this schema.</param>
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
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ChainSchemaSettings(ChainSchemaSettings source)
            : this(source.Ratio, source.Circle, source.ReplaceExistingConnections, source.Repetitions)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRatio { get { return (Ratio == DefaultRatio); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultCircle { get { return (Circle == DefaultCircle); } }

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
                       IsDefaultCircle &&
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
            return new ChainSchemaSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("chainSchema", suppressDefaults);
        }

    }//ChainSchemaSettings

}//Namespace
