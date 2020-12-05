using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Configuration of the A2SCoderSignalStrength coder
    /// </summary>
    [Serializable]
    public class A2SCoderSignalStrengthSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "A2SCoderSignalStrengthType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying number of time-points
        /// </summary>
        public const int DefaultNumOfTimePoints = 8;

        //Attribute properties
        /// <summary>
        /// Number of time-points
        /// </summary>
        public int NumOfTimePoints { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="numOfTimePoints">Number of time-points</param>
        public A2SCoderSignalStrengthSettings(int numOfTimePoints = DefaultNumOfTimePoints)
        {
            NumOfTimePoints = numOfTimePoints;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public A2SCoderSignalStrengthSettings(A2SCoderSignalStrengthSettings source)
            : this(source.NumOfTimePoints)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public A2SCoderSignalStrengthSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfTimePoints = int.Parse(settingsElem.Attribute("timePoints").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultNumOfTimePoints { get { return (NumOfTimePoints == DefaultNumOfTimePoints); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultNumOfTimePoints;
            }
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (NumOfTimePoints < 2 || NumOfTimePoints > 32)
            {
                throw new ArgumentException($"Invalid NumOfTimePoints {NumOfTimePoints.ToString(CultureInfo.InvariantCulture)}. NumOfTimePoints must be GE to 2 and LE to 32.", "NumOfTimePoints");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new A2SCoderSignalStrengthSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultNumOfTimePoints)
            {
                rootElem.Add(new XAttribute("timePoints", NumOfTimePoints.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("signalStrengthCoder", suppressDefaults);
        }

    }//A2SCoderSignalStrengthSettings

}//Namespace

