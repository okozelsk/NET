using RCNet.RandomValue;
using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse connecting analog presynaptic neuron.
    /// </summary>
    [Serializable]
    public class AnalogSourceSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseAnalogSourceType";

        //Default values
        /// <summary>
        /// The default minimum weight.
        /// </summary>
        public const double DefaultMinWeight = 0d;
        /// <summary>
        /// The default maximum weight.
        /// </summary>
        public const double DefaultMaxWeight = 1d;

        //Attribute properties
        /// <summary>
        /// The configuration of the synapse's weight.
        /// </summary>
        public URandomValueSettings WeightCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="weightCfg">The configuration of the synapse's weight.</param>
        public AnalogSourceSettings(URandomValueSettings weightCfg = null)
        {
            WeightCfg = weightCfg == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : (URandomValueSettings)weightCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public AnalogSourceSettings(AnalogSourceSettings source)
            : this(source.WeightCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public AnalogSourceSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement weightSettingsElem = settingsElem.Elements("weight").FirstOrDefault();
            WeightCfg = weightSettingsElem == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : new URandomValueSettings(weightSettingsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultWeightCfg { get { return (WeightCfg.Min == DefaultMinWeight && WeightCfg.Max == DefaultMaxWeight && WeightCfg.IsDefaultDistrType); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return IsDefaultWeightCfg; } }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new AnalogSourceSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultWeightCfg)
            {
                rootElem.Add(WeightCfg.GetXml("weight", suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("analogSource", suppressDefaults);
        }

    }//AnalogSourceSettings

}//Namespace

