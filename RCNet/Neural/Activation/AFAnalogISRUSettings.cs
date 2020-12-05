using System;
using System.Xml.Linq;
using RCNet.RandomValue;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Configuration of the AFAnalogISRU activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class AFAnalogISRUSettings : RCNetBaseSettings, IActivationSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationISRUType";

        //Typical values
        /// <summary>
        /// Typical alpha value
        /// </summary>
        public const double TypicalAlpha = 1;

        //Attribute properties
        /// <summary>
        /// The Alpha
        /// </summary>
        public URandomValueSettings Alpha { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="alpha">The Alpha</param>
        public AFAnalogISRUSettings(URandomValueSettings alpha = null)
        {
            Alpha = URandomValueSettings.CloneOrDefault(alpha, TypicalAlpha);
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AFAnalogISRUSettings(AFAnalogISRUSettings source)
        {
            Alpha = (URandomValueSettings)source.Alpha.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// <param name="elem">Xml element containing the initialization settings</param>
        /// </summary>
        public AFAnalogISRUSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Alpha = URandomValueSettings.LoadOrDefault(activationSettingsElem, "alpha", TypicalAlpha);
            return;
        }

        //Properties
        /// <inheritdoc/>
        public ActivationType TypeOfActivation { get { return ActivationType.Analog; } }

        /// <summary>
        /// Checks the defaults
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
