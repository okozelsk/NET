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
    public class SQNLSettings
    {
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.SQNLSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SQNLSettings cmpSettings = obj as SQNLSettings;
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
        public SQNLSettings DeepClone()
        {
            SQNLSettings clone = new SQNLSettings(this);
            return clone;
        }

    }//SQNLSettings

}//Namespace
