using RCNet.RandomValue;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Contains settings of analog Retainment property (leaky integrator neuron)
    /// </summary>
    [Serializable]
    public class RetainmentSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "RetainmentType";

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
        public RetainmentSettings(double density, URandomValueSettings strengthCfg)
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
        public RetainmentSettings(RetainmentSettings source)
            : this(source.Density, source.StrengthCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings.</param>
        public RetainmentSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Density
            Density = double.Parse(settingsElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            //Strength
            StrengthCfg = new URandomValueSettings(settingsElem.Elements("strength").First());
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
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Density < 0)
            {
                throw new ArgumentException($"Invalid Density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0.", "Density");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new RetainmentSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
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
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("retainment", suppressDefaults);
        }

    }//RetanmentSettings

}//Namespace
