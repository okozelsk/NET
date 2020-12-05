using RCNet.RandomValue;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFAnalogLeakyReLU activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class AFAnalogLeakyReLUSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationLeakyReLUType";

        //Typical values
        /// <summary>
        /// Typical negative slope
        /// </summary>
        public const double TypicalNegSlope = 0.05;

        //Attribute properties
        /// <summary>
        /// The negative slope
        /// </summary>
        public URandomValueSettings NegSlope { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="negSlope">The negative slope</param>
        public AFAnalogLeakyReLUSettings(URandomValueSettings negSlope = null)
        {
            NegSlope = URandomValueSettings.CloneOrDefault(negSlope, TypicalNegSlope);
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AFAnalogLeakyReLUSettings(AFAnalogLeakyReLUSettings source)
        {
            NegSlope = (URandomValueSettings)source.NegSlope.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public AFAnalogLeakyReLUSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NegSlope = URandomValueSettings.LoadOrDefault(activationSettingsElem, "negSlope", TypicalNegSlope);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultNegSlope { get { return (NegSlope.Min == TypicalNegSlope && NegSlope.Max == TypicalNegSlope && NegSlope.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return IsDefaultNegSlope; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new AFAnalogLeakyReLUSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultNegSlope)
            {
                rootElem.Add(NegSlope.GetXml("negSlope", suppressDefaults));
            }

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationLeakyReLU", suppressDefaults);
        }

    }//AFAnalogLeakyReLUSettings

}//Namespace
