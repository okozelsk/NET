using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Configuration of the A2SCoderDownDirArrows coder
    /// </summary>
    [Serializable]
    public class A2SCoderDownDirArrowsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "A2SCoderDirArrowsType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying number of receptors
        /// </summary>
        public const int DefaultNumOfReceptors = 8;
        /// <summary>
        /// Default value of parameter specifying number of time-points per receptor
        /// </summary>
        public const int DefaultNumOfTimePoints = 8;

        //Attribute properties
        /// <summary>
        /// Number of receptors
        /// </summary>
        public int NumOfReceptors { get; }

        /// <summary>
        /// Number of time-points per receptor
        /// </summary>
        public int NumOfTimePoints { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="numOfReceptors">Number of receptors</param>
        /// <param name="numOfTimePoints">Number of intervals per receptor</param>
        public A2SCoderDownDirArrowsSettings(int numOfReceptors = DefaultNumOfReceptors,
                                           int numOfTimePoints = DefaultNumOfTimePoints
                                           )
        {
            NumOfReceptors = numOfReceptors;
            NumOfTimePoints = numOfTimePoints;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public A2SCoderDownDirArrowsSettings(A2SCoderDownDirArrowsSettings source)
            : this(source.NumOfReceptors, source.NumOfTimePoints)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public A2SCoderDownDirArrowsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfReceptors = int.Parse(settingsElem.Attribute("receptors").Value, CultureInfo.InvariantCulture);
            NumOfTimePoints = int.Parse(settingsElem.Attribute("timePoints").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultNumOfReceptors { get { return (NumOfReceptors == DefaultNumOfReceptors); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultNumOfTimePoints { get { return (NumOfTimePoints == DefaultNumOfTimePoints); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultNumOfReceptors &&
                       IsDefaultNumOfTimePoints;
            }
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (NumOfReceptors < 1 || NumOfReceptors > 32)
            {
                throw new ArgumentException($"Invalid NumOfReceptors {NumOfReceptors.ToString(CultureInfo.InvariantCulture)}. NumOfReceptors must be GE to 1 and LE to 32.", "NumOfReceptors");
            }
            if (NumOfTimePoints < 1 || NumOfTimePoints > 32)
            {
                throw new ArgumentException($"Invalid NumOfTimePoints {NumOfTimePoints.ToString(CultureInfo.InvariantCulture)}. NumOfTimePoints must be GE to 1 and LE to 32.", "NumOfTimePoints");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new A2SCoderDownDirArrowsSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultNumOfReceptors)
            {
                rootElem.Add(new XAttribute("receptors", NumOfReceptors.ToString(CultureInfo.InvariantCulture)));
            }
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
            return GetXml("downDirArrowsCoder", suppressDefaults);
        }

    }//A2SCoderDownDirArrows

}//Namespace

