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
    /// Setup parameters for the Pulse signal generator
    /// </summary>
    [Serializable]
    public class PulseGeneratorSettings
    {
        //Attribute properties
        /// <summary>
        /// Signal value
        /// </summary>
        public double Signal { get; set; }

        /// <summary>
        /// Pulse leak value
        /// </summary>
        public int Leak { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="signal">Pulse signal value</param>
        /// <param name="leak">Constant pulse leak</param>
        public PulseGeneratorSettings(double signal, int leak)
        {
            Signal = signal;
            Leak = Math.Abs(leak);
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PulseGeneratorSettings(PulseGeneratorSettings source)
        {
            Signal = source.Signal;
            Leak = source.Leak;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public PulseGeneratorSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Data.Generators.PulseGeneratorSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Signal = double.Parse(settingsElem.Attribute("signal").Value, CultureInfo.InvariantCulture);
            Leak = Math.Abs(int.Parse(settingsElem.Attribute("leak").Value, CultureInfo.InvariantCulture));
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PulseGeneratorSettings cmpSettings = obj as PulseGeneratorSettings;
            if (Signal != cmpSettings.Signal || Leak != cmpSettings.Leak)
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
        public PulseGeneratorSettings DeepClone()
        {
            return new PulseGeneratorSettings(this);
        }

    }//PulseGeneratorSettings

}//Namespace
