using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments of the SoftExponential activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class SoftExponentialSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationSoftExponentialCfgType";

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
        public SoftExponentialSettings(URandomValueSettings alpha = null)
        {
            Alpha = URandomValueSettings.CloneOrDefault(alpha, TypicalAlpha);
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SoftExponentialSettings(SoftExponentialSettings source)
        {
            Alpha = (URandomValueSettings)source.Alpha.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing SoftExponential activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public SoftExponentialSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Alpha = URandomValueSettings.LoadOrDefault(activationSettingsElem, "alpha", TypicalAlpha);
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultAlpha { get { return (Alpha.Min == TypicalAlpha && Alpha.Max == TypicalAlpha && Alpha.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultAlpha; } }


        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SoftExponentialSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("activationSoftExponential", suppressDefaults);
        }

    }//SoftExponentialSettings

}//Namespace
