using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Configuration of the DiffTransformer.
    /// </summary>
    [Serializable]
    public class DiffTransformerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "DiffTransformerType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying the interval between the current and the past value.
        /// </summary>
        public const int DefaultPastInterval = 1;



        //Attribute properties
        /// <summary>
        /// The name of the input field to be transformed.
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Specifies the interval between the current and the past value.
        /// </summary>
        public int PastInterval { get; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputFieldName">The name of the input field to be transformed.</param>
        /// <param name="pastInterval">Specifies the interval between the current and the past value.</param>
        public DiffTransformerSettings(string inputFieldName, int pastInterval = DefaultPastInterval)
        {
            InputFieldName = inputFieldName;
            PastInterval = pastInterval;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public DiffTransformerSettings(DiffTransformerSettings source)
            : this(source.InputFieldName, source.PastInterval)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public DiffTransformerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("fieldName").Value;
            PastInterval = int.Parse(settingsElem.Attribute("pastInterval").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultPastInterval { get { return (PastInterval == DefaultPastInterval); } }

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
            if (PastInterval < 1)
            {
                throw new ArgumentException($"Invalid past interval {PastInterval.ToString(CultureInfo.InvariantCulture)}. Interval must be GE to 1.", "PastInterval");
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
            if (!suppressDefaults || !IsDefaultPastInterval)
            {
                rootElem.Add(new XAttribute("pastInterval", PastInterval.ToString(CultureInfo.InvariantCulture)));
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
