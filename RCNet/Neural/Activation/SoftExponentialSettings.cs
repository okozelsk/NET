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
    public class SoftExponentialSettings
    {
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
        public SoftExponentialSettings(RandomValueSettings alpha)
        {
            Alpha = alpha.DeepClone();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SoftExponentialSettings(SoftExponentialSettings source)
        {
            Alpha = source.Alpha.DeepClone();
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Activation.SoftExponentialSettings.xsd");
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
            SoftExponentialSettings cmpSettings = obj as SoftExponentialSettings;
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
        public SoftExponentialSettings DeepClone()
        {
            SoftExponentialSettings clone = new SoftExponentialSettings(this);
            return clone;
        }

    }//SoftExponentialSettings

}//Namespace
