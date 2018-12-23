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
    /// Setup parameters for the Mackey-Glass signal generator
    /// </summary>
    [Serializable]
    public class MackeyGlassGeneratorSettings
    {
        //Attribute properties
        /// <summary>
        /// Tau (backward deepness 2->18)
        /// </summary>
        public int Tau { get; set; }

        /// <summary>
        /// b coefficient
        /// </summary>
        public double B { get; set; }

        /// <summary>
        /// c coefficient
        /// </summary>
        public double C { get; set; }


        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="tau">Tau (backward deepness 2-18)</param>
        /// <param name="b">b coefficient</param>
        /// <param name="c">c coefficient</param>
        public MackeyGlassGeneratorSettings(int tau, double b, double c)
        {
            Tau = tau;
            B = b;
            C = c;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public MackeyGlassGeneratorSettings(MackeyGlassGeneratorSettings source)
        {
            Tau = source.Tau;
            B = source.B;
            C = source.C;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public MackeyGlassGeneratorSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Data.Generators.MackeyGlassGeneratorSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Tau = int.Parse(settingsElem.Attribute("tau").Value, CultureInfo.InvariantCulture);
            B = double.Parse(settingsElem.Attribute("b").Value, CultureInfo.InvariantCulture);
            C = double.Parse(settingsElem.Attribute("c").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            MackeyGlassGeneratorSettings cmpSettings = obj as MackeyGlassGeneratorSettings;
            if (Tau != cmpSettings.Tau ||
                B != cmpSettings.B ||
                C != cmpSettings.C
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
        public MackeyGlassGeneratorSettings DeepClone()
        {
            return new MackeyGlassGeneratorSettings(this);
        }

    }//MackeyGlassGeneratorSettings

}//Namespace
