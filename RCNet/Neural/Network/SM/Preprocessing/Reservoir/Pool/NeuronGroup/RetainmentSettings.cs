using RCNet.RandomValue;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Configuration of the analog neuron Retainment property.
    /// </summary>
    [Serializable]
    public class RetainmentSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RetainmentType";

        //Attribute properties
        /// <summary>
        /// Specifies the ratio of the neurons having the Retainment property.
        /// </summary>
        public double Density { get; }
        /// <summary>
        /// The configuration of the retainment strength.
        /// </summary>
        public URandomValueSettings StrengthCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="density">Specifies the ratio of the neurons having the Retainment property.</param>
        /// <param name="strengthCfg">The configuration of the retainment strength.</param>
        public RetainmentSettings(double density, URandomValueSettings strengthCfg)
        {
            Density = density;
            StrengthCfg = (URandomValueSettings)strengthCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RetainmentSettings(RetainmentSettings source)
            : this(source.Density, source.StrengthCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return false;
            }
        }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Density < 0)
            {
                throw new ArgumentException($"Invalid Density {Density.ToString(CultureInfo.InvariantCulture)}. Density must be GE to 0.", "Density");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new RetainmentSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            rootElem.Add(new XAttribute("density", Density.ToString(CultureInfo.InvariantCulture)));
            rootElem.Add(StrengthCfg.GetXml("strength", suppressDefaults));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("retainment", suppressDefaults);
        }

    }//RetainmentSettings

}//Namespace
