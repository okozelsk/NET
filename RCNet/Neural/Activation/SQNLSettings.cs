using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.RandomValue;
using RCNet.XmlTools;
using RCNet.MathTools.Differential;

namespace RCNet.Neural.Activation
{
    /// <summary>
    /// Class encaptulates arguments of the SQNL (Square nonlinearity) activation function.
    /// </summary>
    [Serializable]
    public class SQNLSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationSQNLCfgType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public SQNLSettings()
        {
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public SQNLSettings(SQNLSettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing SQNL activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public SQNLSettings(XElement elem)
        {
            //Validation
            XElement activationSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public SQNLSettings DeepClone()
        {
            return new SQNLSettings(this);
        }

    }//SQNLSettings

}//Namespace
