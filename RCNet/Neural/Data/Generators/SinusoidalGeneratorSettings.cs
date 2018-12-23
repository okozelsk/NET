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
    /// Setup parameters for the Sinusoidal signal generator
    /// </summary>
    [Serializable]
    public class SinusoidalGeneratorSettings
    {
        //Attribute properties
        /// <summary>
        /// Phase shift
        /// </summary>
        public double Phase { get; set; }

        /// <summary>
        /// Frequency coefficient
        /// </summary>
        public double Freq { get; set; }

        /// <summary>
        /// Amplitude coefficient
        /// </summary>
        public double Ampl { get; set; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="phase">Phase shift</param>
        /// <param name="freq">Frequency coefficient</param>
        /// <param name="ampl">Amplitude coefficient</param>
        public SinusoidalGeneratorSettings(double phase, double freq, double ampl)
        {
            Phase = phase;
            Freq = freq;
            Ampl = ampl;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public SinusoidalGeneratorSettings(SinusoidalGeneratorSettings source)
        {
            Phase = source.Phase;
            Freq = source.Freq;
            Ampl = source.Ampl;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public SinusoidalGeneratorSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Data.Generators.SinusoidalGeneratorSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Phase = double.Parse(settingsElem.Attribute("phase").Value, CultureInfo.InvariantCulture);
            Freq = double.Parse(settingsElem.Attribute("freq").Value, CultureInfo.InvariantCulture);
            Ampl = double.Parse(settingsElem.Attribute("ampl").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SinusoidalGeneratorSettings cmpSettings = obj as SinusoidalGeneratorSettings;
            if (Phase != cmpSettings.Phase ||
                Freq != cmpSettings.Freq ||
                Ampl != cmpSettings.Ampl
                )
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
        public SinusoidalGeneratorSettings DeepClone()
        {
            return new SinusoidalGeneratorSettings(this);
        }

    }//SinusoidalGeneratorSettings

}//Namespace
