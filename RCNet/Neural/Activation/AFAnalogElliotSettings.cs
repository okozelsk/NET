using RCNet.RandomValue;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFAnalogElliot activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogElliotSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ActivationElliotType";

        //Typical values
        /// <summary>
        /// The typical slope of the curve.
        /// </summary>
        public const double TypicalSlope = 1;

        //Attribute properties
        /// <summary>
        /// The slope of the curve.
        /// </summary>
        public URandomValueSettings Slope { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="slope">The slope of the curve.</param>
        public AFAnalogElliotSettings(URandomValueSettings slope = null)
        {
            Slope = URandomValueSettings.CloneOrCreate(slope, TypicalSlope);
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AFAnalogElliotSettings(AFAnalogElliotSettings source)
        {
            Slope = (URandomValueSettings)source.Slope.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AFAnalogElliotSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Slope = URandomValueSettings.LoadOrCreate(activationSettingsElem, "slope", TypicalSlope);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultSlope { get { return (Slope.Min == TypicalSlope && Slope.Max == TypicalSlope && Slope.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return IsDefaultSlope; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new AFAnalogElliotSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultSlope)
            {
                rootElem.Add(Slope.GetXml("slope", suppressDefaults));
            }

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationElliot", suppressDefaults);
        }

    }//AFAnalogElliotSettings

}//Namespace
