using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the DiffTransformer
    /// </summary>
    [Serializable]
    public class DiffTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "DiffTransformerType";
        //Default values
        /// <summary>
        /// Default value of interval argument
        /// </summary>
        public const int DefaultInterval = 1;



        //Attribute properties
        /// <summary>
        /// Name of the input field to be transformed
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Difference interval
        /// </summary>
        public int Interval { get; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="inputFieldName">Name of the input field to be transformed</param>
        /// <param name="interval">Difference interval</param>
        public DiffTransformerSettings(string inputFieldName, int interval = DefaultInterval)
        {
            InputFieldName = inputFieldName;
            Interval = interval;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public DiffTransformerSettings(DiffTransformerSettings source)
            : this(source.InputFieldName, source.Interval)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public DiffTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            Interval = int.Parse(settingsElem.Attribute("interval").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultInterval { get { return (Interval == DefaultInterval); } }

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
            if (Interval < 1)
            {
                throw new ArgumentException($"Invalid difference interval {Interval.ToString(CultureInfo.InvariantCulture)}. Interval must be GE to 1.", "Interval");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new DiffTransformerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("fieldName", InputFieldName)
                                             );
            if (!suppressDefaults || !IsDefaultInterval)
            {
                rootElem.Add(new XAttribute("interval", Interval.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("diff", suppressDefaults);
        }

    }//DiffTransformerSettings

}//Namespace
