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
    /// Class encaptulates arguments of the Elliot activation function
    /// </summary>
    [Serializable]
    public class ElliotSettings
    {
        //Constants
        //Typical values
        public const double TypicalSlope = 1;

        //Attribute properties
        /// <summary>
        /// The curve slope
        /// </summary>
        public RandomValueSettings Slope { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="slope">The curve slope</param>
        public ElliotSettings(RandomValueSettings slope)
        {
            Slope = slope.DeepClone();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ElliotSettings(ElliotSettings source)
        {
            Slope = source.Slope.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing Elliot activation settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ElliotSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.ElliotSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement activationSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Slope = new RandomValueSettings(activationSettingsElem.Descendants("slope").FirstOrDefault());
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ElliotSettings cmpSettings = obj as ElliotSettings;
            if (!Equals(Slope, cmpSettings.Slope))
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
        public ElliotSettings DeepClone()
        {
            ElliotSettings clone = new ElliotSettings(this);
            return clone;
        }

    }//ElliotSettings

}//Namespace
