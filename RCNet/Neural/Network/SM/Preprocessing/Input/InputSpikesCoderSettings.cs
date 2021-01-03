using RCNet.Neural.Data.Coders.AnalogToSpiking;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the InputSpikesCoder
    /// </summary>
    [Serializable]
    public class InputSpikesCoderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPInpSpikesCoderType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying the way of input spikes coding.
        /// </summary>
        public const InputEncoder.InputSpikesCoding DefaultRegime = InputEncoder.InputSpikesCoding.Forbidden;
        /// <summary>
        /// The default value of the parameter specifying the collection of the A2S coder configurations.
        /// </summary>
        public const List<RCNetBaseSettings> DefaultCoderCfgCollection = null;

        //Attribute properties
        /// <inheritdoc cref="InputEncoder.InputSpikesCoding"/>
        public InputEncoder.InputSpikesCoding Regime { get; }

        /// <summary>
        /// The collection of the A2S coder configurations.
        /// </summary>
        public List<RCNetBaseSettings> CoderCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="regime">The way of input spikes coding.</param>
        /// <param name="coderCfgCollection">The collection of the A2S coder configurations.</param>
        public InputSpikesCoderSettings(InputEncoder.InputSpikesCoding regime = DefaultRegime,
                                        List<RCNetBaseSettings> coderCfgCollection = DefaultCoderCfgCollection
                                        )
        {
            Regime = regime;
            CoderCfgCollection = new List<RCNetBaseSettings>();
            if (Regime != InputEncoder.InputSpikesCoding.Forbidden)
            {
                if (coderCfgCollection != null && coderCfgCollection.Count > 0)
                {
                    foreach (RCNetBaseSettings coderSettings in coderCfgCollection)
                    {
                        CoderCfgCollection.Add(coderSettings.DeepClone());
                    }
                }
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="regime">The way of input spikes coding.</param>
        /// <param name="coderCfgCollection">The A2S coder configurations.</param>
        public InputSpikesCoderSettings(InputEncoder.InputSpikesCoding regime,
                                        params RCNetBaseSettings[] coderCfgCollection
                                        )
            : this(regime, new List<RCNetBaseSettings>(coderCfgCollection))
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public InputSpikesCoderSettings(InputSpikesCoderSettings source)
            : this(source.Regime, source.CoderCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public InputSpikesCoderSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Regime
            Regime = (InputEncoder.InputSpikesCoding)Enum.Parse(typeof(InputEncoder.InputSpikesCoding), settingsElem.Attribute("regime").Value, true);
            //Coders configurations
            CoderCfgCollection = null;
            if (Regime != InputEncoder.InputSpikesCoding.Forbidden)
            {
                CoderCfgCollection = new List<RCNetBaseSettings>();
                foreach (XElement coderCfgElem in settingsElem.Elements())
                {
                    if (A2SCoderFactory.CheckSettingsElemName(coderCfgElem))
                    {
                        CoderCfgCollection.Add(A2SCoderFactory.LoadSettings(coderCfgElem));
                    }
                }
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultRegime { get { return (Regime == DefaultRegime); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultCoderCfgCollection { get { return (CoderCfgCollection == DefaultCoderCfgCollection); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultRegime && IsDefaultCoderCfgCollection; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Regime != InputEncoder.InputSpikesCoding.Forbidden)
            {
                if (CoderCfgCollection.Count == 0)
                {
                    throw new ArgumentException($"It must be specified at least one coder configuration.", "CoderCfgCollection");
                }
                foreach (RCNetBaseSettings coderCfg in CoderCfgCollection)
                {
                    if (!A2SCoderFactory.CheckSettings(coderCfg))
                    {
                        throw new ArgumentException($"Unknown coder configuration {coderCfg.GetType().Name}.", "CoderCfgCollection");
                    }
                }
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputSpikesCoderSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultRegime)
            {
                rootElem.Add(new XAttribute("regime", Regime.ToString()));
            }
            foreach (RCNetBaseSettings coderCfg in CoderCfgCollection)
            {
                rootElem.Add(coderCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("spikesCoder", suppressDefaults);
        }

    }//InputSpikesCoderSettings

}//Namespace

