using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Contains settings of analog Retainment property (leaky integrator neuron)
    /// </summary>
    [Serializable]
    public class AnalogRetainmentSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolAnalogNeuronGroupRetainmentType";

        //Attribute properties
        /// <summary>
        /// Specifies how many neurons within the context will have the Retainment property (leaky integrator neuron)
        /// </summary>
        public double Density { get; }
        /// <summary>
        /// Retainment strength random settings
        /// </summary>
        public URandomValueSettings StrengthCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="density">Specifies how many neurons within the context will have the Retainment property (leaky integrator neuron)</param>
        /// <param name="strengthCfg">Retainment strength random settings</param>
        public AnalogRetainmentSettings(double density, URandomValueSettings strengthCfg)
        {
            Density = density;
            StrengthCfg = (URandomValueSettings)strengthCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AnalogRetainmentSettings(AnalogRetainmentSettings source)
            :this(source.Density, source.StrengthCfg)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public AnalogRetainmentSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Density
            Density = double.Parse(settingsElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            //Strength
            StrengthCfg = new URandomValueSettings(settingsElem.Descendants("strength").First());
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Density < 0)
            {
                throw new Exception($"Invalid Density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new AnalogRetainmentSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            rootElem.Add(new XAttribute("density", Density.ToString(CultureInfo.InvariantCulture)));
            rootElem.Add(StrengthCfg.GetXml("strength", suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("retainment", suppressDefaults);
        }

    }//AnalogNeuronsRetanmentSettings

}//Namespace
