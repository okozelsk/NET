using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of the input field connection.
    /// </summary>
    [Serializable]
    public class InputConnSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputConnectionType";
        //Default values
        /// <summary>
        /// The default density on target spiking neurons.
        /// </summary>
        public const double DefaultSpikingTargetDensity = 1d;
        /// <summary>
        /// The default density on target analog neurons.
        /// </summary>
        public const double DefaultAnalogTargetDensity = 1d;

        /// <summary>
        /// The name of the input field.
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// The name of the target pool.
        /// </summary>
        public string PoolName { get; }

        /// <summary>
        /// The density on the target spiking neurons.
        /// </summary>
        public double SpikingTargetDensity { get; }

        /// <summary>
        /// The density on the target analog neurons.
        /// </summary>
        public double AnalogTargetDensity { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field.</param>
        /// <param name="poolName">The name of the target pool.</param>
        /// <param name="spikingTargetDensity">The density on the target spiking neurons.</param>
        /// <param name="analogTargetDensity">The density on the target analog neurons.</param>
        public InputConnSettings(string inputFieldName,
                                 string poolName,
                                 double spikingTargetDensity = DefaultSpikingTargetDensity,
                                 double analogTargetDensity = DefaultAnalogTargetDensity
                                 )
        {
            InputFieldName = inputFieldName;
            PoolName = poolName;
            SpikingTargetDensity = spikingTargetDensity;
            AnalogTargetDensity = analogTargetDensity;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public InputConnSettings(InputConnSettings source)
            : this(source.InputFieldName, source.PoolName, source.SpikingTargetDensity, source.AnalogTargetDensity)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public InputConnSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("inputFieldName").Value;
            PoolName = settingsElem.Attribute("poolName").Value;
            SpikingTargetDensity = double.Parse(settingsElem.Attribute("spikingTargetDensity").Value, CultureInfo.InvariantCulture);
            AnalogTargetDensity = double.Parse(settingsElem.Attribute("analogTargetDensity").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSpikingTargetDensity { get { return (SpikingTargetDensity == DefaultSpikingTargetDensity); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultAnalogTargetDensity { get { return (AnalogTargetDensity == DefaultAnalogTargetDensity); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (InputFieldName.Length == 0)
            {
                throw new ArgumentException($"Input field name can not be empty.", "InputFieldName");
            }
            if (PoolName.Length == 0)
            {
                throw new ArgumentException($"Pool name can not be empty.", "PoolName");
            }
            if (SpikingTargetDensity < 0 || SpikingTargetDensity > 1)
            {
                throw new ArgumentException($"Invalid SpikingTargetDensity ({SpikingTargetDensity.ToString(CultureInfo.InvariantCulture)}). SpikingTargetDensity must be GE to 0 and LE to 1.", "SpikingTargetDensity");
            }
            if (AnalogTargetDensity < 0 || AnalogTargetDensity > 1)
            {
                throw new ArgumentException($"Invalid AnalogTargetDensity ({AnalogTargetDensity.ToString(CultureInfo.InvariantCulture)}). AnalogTargetDensity must be GE to 0 and LE to 1.", "AnalogTargetDensity");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new InputConnSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("inputFieldName", InputFieldName),
                                             new XAttribute("poolName", PoolName)
                                             );
            if (!suppressDefaults || !IsDefaultSpikingTargetDensity)
            {
                rootElem.Add(new XAttribute("spikingTargetDensity", SpikingTargetDensity.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultAnalogTargetDensity)
            {
                rootElem.Add(new XAttribute("analogTargetDensity", AnalogTargetDensity.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("connection", suppressDefaults);
        }

    }//InputConnSettings

}//Namespace

