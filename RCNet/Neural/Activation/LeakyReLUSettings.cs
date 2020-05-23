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
    /// Class encaptulates arguments of the LeakyReLU activation function.
    /// Arguments are in RandomValue form to allow their dynamic random initialization within the specified ranges.
    /// </summary>
    [Serializable]
    public class LeakyReLUSettings : RCNetBaseSettings
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
        public LeakyReLUSettings(URandomValueSettings negSlope = null)
        {
            NegSlope = URandomValueSettings.CloneOrDefault(negSlope, TypicalNegSlope);
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LeakyReLUSettings(LeakyReLUSettings source)
        {
            NegSlope = (URandomValueSettings)source.NegSlope.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing LeakyReLU activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public LeakyReLUSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NegSlope = URandomValueSettings.LoadOrDefault(activationSettingsElem, "negSlope", TypicalNegSlope);
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultNegSlope { get { return (NegSlope.Min == TypicalNegSlope && NegSlope.Max == TypicalNegSlope && NegSlope.DistrType == RandomCommon.DistributionType.Uniform); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultNegSlope; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new LeakyReLUSettings(this);
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
            if (!suppressDefaults || !IsDefaultNegSlope)
            {
                rootElem.Add(NegSlope.GetXml("negSlope", suppressDefaults));
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
            return GetXml("activationLeakyReLU", suppressDefaults);
        }

    }//LeakyReLUSettings

}//Namespace
