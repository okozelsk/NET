using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Settings of A2SCoderSignalStrength coder
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultNumOfTimePoints { get { return (NumOfTimePoints == DefaultNumOfTimePoints); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultNumOfTimePoints;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (NumOfTimePoints < 2 || NumOfTimePoints > 32)
            {
                throw new ArgumentException($"Invalid NumOfTimePoints {NumOfTimePoints.ToString(CultureInfo.InvariantCulture)}. NumOfTimePoints must be GE to 2 and LE to 32.", "NumOfTimePoints");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new A2SCoderSignalStrengthSettings(this);
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
            if (!suppressDefaults || !IsDefaultNumOfTimePoints)
            {
                rootElem.Add(new XAttribute("timePoints", NumOfTimePoints.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("signalStrengthCoder", suppressDefaults);
        }

    }//A2SCoderSignalStrengthSettings

}//Namespace

