using RCNet.RandomValue;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFAnalogLeakyReLU activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogLeakyReLUSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ActivationLeakyReLUType";

        //Typical values
        /// <summary>
        /// The typical negative slope.
        /// </summary>
        public const double TypicalNegSlope = 0.05;

        //Attribute properties
        /// <summary>
        /// The negative slope.
        /// </summary>
        public URandomValueSettings NegSlope { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="negSlope">The negative slope</param>
        public AFAnalogLeakyReLUSettings(URandomValueSettings negSlope = null)
        {
            NegSlope = URandomValueSettings.CloneOrCreate(negSlope, TypicalNegSlope);
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AFAnalogLeakyReLUSettings(AFAnalogLeakyReLUSettings source)
        {
            NegSlope = (URandomValueSettings)source.NegSlope.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AFAnalogLeakyReLUSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NegSlope = URandomValueSettings.LoadOrCreate(activationSettingsElem, "negSlope", TypicalNegSlope);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <summary>
        /// Checks the defaults.
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
