using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of a synapse connecting postsynaptic hidden spiking neuron.
    /// </summary>
    [Serializable]
    public class SynapseSTSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "SynapseSTType";

        //Attribute properties
        /// <summary>
        /// The configuration of an input synapse.
        /// </summary>
        public SynapseSTInputSettings InputSynCfg { get; }

        /// <summary>
        /// The configuration of an excitatory synapse.
        /// </summary>
        public SynapseSTExcitatorySettings ExcitatorySynCfg { get; }

        /// <summary>
        /// The configuration of an inhibitory synapse.
        /// </summary>
        public SynapseSTInhibitorySettings InhibitorySynCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputSynCfg">The configuration of an input synapse.</param>
        /// <param name="excitatorySynCfg">The configuration of an excitatory synapse.</param>
        /// <param name="inhibitorySynCfg">The configuration of an inhibitory synapse.</param>
        public SynapseSTSettings(SynapseSTInputSettings inputSynCfg = null,
                                 SynapseSTExcitatorySettings excitatorySynCfg = null,
                                 SynapseSTInhibitorySettings inhibitorySynCfg = null
                                 )
        {
            InputSynCfg = inputSynCfg == null ? new SynapseSTInputSettings() : (SynapseSTInputSettings)inputSynCfg.DeepClone();
            ExcitatorySynCfg = excitatorySynCfg == null ? new SynapseSTExcitatorySettings() : (SynapseSTExcitatorySettings)excitatorySynCfg.DeepClone();
            InhibitorySynCfg = inhibitorySynCfg == null ? new SynapseSTInhibitorySettings() : (SynapseSTInhibitorySettings)inhibitorySynCfg.DeepClone();
            Check();
            return;
        }


        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public SynapseSTSettings(SynapseSTSettings source)
            : this(source.InputSynCfg, source.ExcitatorySynCfg, source.InhibitorySynCfg)
        {

            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public SynapseSTSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            XElement inputSynElem = settingsElem.Elements("input").FirstOrDefault();
            InputSynCfg = inputSynElem == null ? new SynapseSTInputSettings() : new SynapseSTInputSettings(inputSynElem);
            XElement excitatorySynElem = settingsElem.Elements("excitatory").FirstOrDefault();
            ExcitatorySynCfg = excitatorySynElem == null ? new SynapseSTExcitatorySettings() : new SynapseSTExcitatorySettings(excitatorySynElem);
            XElement inhibitorySynElem = settingsElem.Elements("inhibitory").FirstOrDefault();
            InhibitorySynCfg = inhibitorySynElem == null ? new SynapseSTInhibitorySettings() : new SynapseSTInhibitorySettings(inhibitorySynElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultInputSynCfg { get { return InputSynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultExcitatorySynCfg { get { return ExcitatorySynCfg.ContainsOnlyDefaults; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultInhibitorySynCfg { get { return InhibitorySynCfg.ContainsOnlyDefaults; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultInputSynCfg &&
                       IsDefaultExcitatorySynCfg &&
                       IsDefaultInhibitorySynCfg;
            }
        }


        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new SynapseSTSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultInputSynCfg)
            {
                rootElem.Add(InputSynCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultExcitatorySynCfg)
            {
                rootElem.Add(ExcitatorySynCfg.GetXml(suppressDefaults));
            }
            if (!suppressDefaults || !IsDefaultInhibitorySynCfg)
            {
                rootElem.Add(InhibitorySynCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("spikingTarget", suppressDefaults);
        }

    }//SynapseSTSettings

}//Namespace

