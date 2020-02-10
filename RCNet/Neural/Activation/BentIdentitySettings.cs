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
    /// Class encaptulates arguments of the BentIdentity activation function
    /// </summary>
    [Serializable]
    public class BentIdentitySettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ActivationBentIdentityCfgType";

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        public BentIdentitySettings()
        {
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public BentIdentitySettings(BentIdentitySettings source)
        {
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing BentIdentity activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public BentIdentitySettings(XElement elem)
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
        public BentIdentitySettings DeepClone()
        {
            return new BentIdentitySettings(this);
        }

    }//BentIdentitySettings

}//Namespace
