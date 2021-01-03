using RCNet.MathTools;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the MWStatTransformer.
    /// </summary>
    [Serializable]
    public class MWStatTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "MWStatTransformerType";

        //Attribute properties
        /// <summary>
        /// The name of the input field to be transformed.
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// The recent history window size.
        /// </summary>
        public int WindowSize { get; }

        /// <summary>
        /// The output statistical figure.
        /// </summary>
        public BasicStat.StatisticalFigure OutputFigure { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field to be transformed.</param>
        /// <param name="windowSize">The recent history window size.</param>
        /// <param name="outputFigure">The output statistical figure.</param>
        public MWStatTransformerSettings(string inputFieldName, int windowSize, BasicStat.StatisticalFigure outputFigure)
        {
            InputFieldName = inputFieldName;
            WindowSize = windowSize;
            OutputFigure = outputFigure;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public MWStatTransformerSettings(MWStatTransformerSettings source)
            : this(source.InputFieldName, source.WindowSize, source.OutputFigure)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public MWStatTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            WindowSize = int.Parse(settingsElem.Attribute("window").Value, CultureInfo.InvariantCulture);
            OutputFigure = (BasicStat.StatisticalFigure)Enum.Parse(typeof(BasicStat.StatisticalFigure), settingsElem.Attribute("output").Value, true);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (InputFieldName.Length == 0)
            {
                throw new ArgumentException($"Name of the input field must be specified.", "InputFieldName");
            }
            if (WindowSize < 1)
            {
                throw new ArgumentException($"Invalid window size {WindowSize.ToString(CultureInfo.InvariantCulture)}. Window size must be GE to 2.", "Window");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new MWStatTransformerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("fieldName", InputFieldName),
                                             new XAttribute("window", WindowSize.ToString(CultureInfo.InvariantCulture)),
                                             new XAttribute("output", OutputFigure.ToString())
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("stat", suppressDefaults);
        }

    }//MWStatTransformerSettings

}//Namespace
