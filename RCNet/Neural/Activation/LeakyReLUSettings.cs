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
        public const string XsdTypeName = "ActivationLeakyReLUCfgType";

        //Typical values
        /// <summary>
        /// Typical negative slope
        /// </summary>
        public const double TypicalNegSlope = 0.05;

        //Attribute properties
        /// <summary>
        /// The negative slope
        /// </summary>
        public RandomValueSettings NegSlope { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="negSlope">The negative slope</param>
        public LeakyReLUSettings(RandomValueSettings negSlope = null)
        {
            NegSlope = RandomValueSettings.CloneOrDefault(negSlope, TypicalNegSlope);
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LeakyReLUSettings(LeakyReLUSettings source)
        {
            NegSlope = source.NegSlope.DeepClone();
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
            NegSlope = RandomValueSettings.LoadOrDefault(activationSettingsElem, "negSlope", TypicalNegSlope);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public LeakyReLUSettings DeepClone()
        {
            return new LeakyReLUSettings(this);
        }

    }//LeakyReLUSettings

}//Namespace
