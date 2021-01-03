using RCNet.RandomValue;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the ISRU (Inverse Square Root Unit) activation function.
    /// </summary>
    [Serializable]
    public class AFAnalogISRUSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ActivationISRUType";

        //Typical values
        /// <summary>
        /// The typical alpha value.
        /// </summary>
        public const double TypicalAlpha = 1;

        //Attribute properties
        /// <summary>
        /// The Alpha.
        /// </summary>
        public URandomValueSettings Alpha { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="alpha">The Alpha.</param>
        public AFAnalogISRUSettings(URandomValueSettings alpha = null)
        {
            Alpha = URandomValueSettings.CloneOrCreate(alpha, TypicalAlpha);
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AFAnalogISRUSettings(AFAnalogISRUSettings source)
        {
            Alpha = (URandomValueSettings)source.Alpha.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// <param name="elem">A xml element containing the configuration data.</param>
        /// </summary>
        public AFAnalogISRUSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Alpha = URandomValueSettings.LoadOrCreate(activationSettingsElem, "alpha", TypicalAlpha);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultAlpha { get { return (Alpha.Min == TypicalAlpha && Alpha.Max == TypicalAlpha && Alpha.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return IsDefaultAlpha; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new AFAnalogISRUSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultAlpha)
            {
                rootElem.Add(Alpha.GetXml("alpha", suppressDefaults));
            }

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationISRU", suppressDefaults);
        }

    }//AFAnalogISRUSettings

}//Namespace
