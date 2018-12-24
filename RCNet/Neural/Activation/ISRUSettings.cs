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
    /// Class encaptulates arguments of the ISRU activation function
    /// </summary>
    [Serializable]
    public class ISRUSettings
    {
        //Constants
        //Typical values
        /// <summary>
        /// Typical alpha value
        /// </summary>
        public const double TypicalAlpha = 1;

        //Attribute properties
        /// <summary>
        /// The Alpha
        /// </summary>
        public RandomValueSettings Alpha { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="alpha">The Alpha</param>
        public ISRUSettings(RandomValueSettings alpha)
        {
            Alpha = alpha.DeepClone();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ISRUSettings(ISRUSettings source)
        {
            Alpha = source.Alpha.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing ISRU activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ISRUSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.ISRUSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Alpha = new RandomValueSettings(activationSettingsElem.Descendants("alpha").FirstOrDefault());
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ISRUSettings cmpSettings = obj as ISRUSettings;
            if (!Equals(Alpha, cmpSettings.Alpha))
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
        public ISRUSettings DeepClone()
        {
            ISRUSettings clone = new ISRUSettings(this);
            return clone;
        }

    }//ISRUSettings

}//Namespace
