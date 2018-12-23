using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Setup parameters for the Const signal generator
    /// </summary>
    [Serializable]
    public class ConstGeneratorSettings
    {
        //Attribute properties
        /// <summary>
        /// Constant signal value
        /// </summary>
        public double ConstSignal { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="constSignal">Constant signal value</param>
        public ConstGeneratorSettings(double constSignal)
        {
            ConstSignal = constSignal;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ConstGeneratorSettings(ConstGeneratorSettings source)
        {
            ConstSignal = source.ConstSignal;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public ConstGeneratorSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Data.Generators.ConstGeneratorSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            ConstSignal = double.Parse(settingsElem.Attribute("constSignal").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ConstGeneratorSettings cmpSettings = obj as ConstGeneratorSettings;
            if (ConstSignal != cmpSettings.ConstSignal)
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
        public ConstGeneratorSettings DeepClone()
        {
            return new ConstGeneratorSettings(this);
        }

    }//ConstGeneratorSettings

}//Namespace
