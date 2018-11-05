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
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments of the LeakyReLU activation function
    /// </summary>
    [Serializable]
    public class LeakyReLUSettings
    {
        //Attribute properties
        /// <summary>
        /// The negative slope
        /// </summary>
        public double NegSlope { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="negSlope">The negative slope</param>
        public LeakyReLUSettings(double negSlope)
        {
            NegSlope = negSlope;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public LeakyReLUSettings(LeakyReLUSettings source)
        {
            NegSlope = source.NegSlope;
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.LeakyReLUSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            NegSlope = double.Parse(activationSettingsElem.Attribute("negSlope").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            LeakyReLUSettings cmpSettings = obj as LeakyReLUSettings;
            if (NegSlope != cmpSettings.NegSlope)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public LeakyReLUSettings DeepClone()
        {
            LeakyReLUSettings clone = new LeakyReLUSettings(this);
            return clone;
        }

    }//LeakyReLUSettings

}//Namespace
