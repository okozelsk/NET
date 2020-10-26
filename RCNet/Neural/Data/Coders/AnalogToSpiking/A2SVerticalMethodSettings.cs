using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Settings of vertical spike train
    /// </summary>
    [Serializable]
    public class A2SVerticalMethodSettings : RCNetBaseSettings, IA2SCodingMethodSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "A2SCodingMethodVerticalType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying length of the spike train
        /// </summary>
        public const int DefaultSpikeTrainLength = 16;

        //Attribute properties
        /// <summary>
        /// Length of the half of spike code
        /// </summary>
        public int SpikeTrainLength { get; }


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="spikeTrainLength">Length of the spike train</param>
        public A2SVerticalMethodSettings(int spikeTrainLength = DefaultSpikeTrainLength)
        {
            SpikeTrainLength = spikeTrainLength;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public A2SVerticalMethodSettings(A2SVerticalMethodSettings source)
            : this(source.SpikeTrainLength)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public A2SVerticalMethodSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            SpikeTrainLength = int.Parse(settingsElem.Attribute("spikeTrainLength").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Way to convert an analog value to spikes
        /// </summary>
        public A2SCoder.CodingMethod Method { get { return A2SCoder.CodingMethod.Vertical; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpikeTrainLength { get { return (SpikeTrainLength == DefaultSpikeTrainLength); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultSpikeTrainLength;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (SpikeTrainLength < 1 || SpikeTrainLength > 32)
            {
                throw new ArgumentException($"Invalid SpikeTrainLength {SpikeTrainLength.ToString(CultureInfo.InvariantCulture)}. SpikeTrainLength must be GE to 1 and LE to 32.", "SpikeTrainLength");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new A2SVerticalMethodSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultSpikeTrainLength)
            {
                rootElem.Add(new XAttribute("spikeTrainLength", SpikeTrainLength.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("vertical", suppressDefaults);
        }

    }//SpikeCodeVerticalSettings

}//Namespace

