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
    /// Class encaptulates arguments of the SoftPlus activation function
    /// </summary>
    [Serializable]
    public class SoftPlusSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationSoftPlusCfgType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public SoftPlusSettings()
        {
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SoftPlusSettings(SoftPlusSettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing SoftPlus activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public SoftPlusSettings(XElement elem)
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
        public SoftPlusSettings DeepClone()
        {
            return new SoftPlusSettings(this);
        }

    }//SoftPlusSettings

}//Namespace
