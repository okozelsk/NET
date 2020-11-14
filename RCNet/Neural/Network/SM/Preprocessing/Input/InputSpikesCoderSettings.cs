using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Data.Coders.AnalogToSpiking;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of InputSpikesCoder
    /// </summary>
    [Serializable]
    public class InputSpikesCoderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInpSpikesCoderType";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying spiking input encoding regime
        /// </summary>
        public const InputEncoder.SpikingInputEncodingRegime DefaultRegime = InputEncoder.SpikingInputEncodingRegime.Forbidden;
        /// <summary>
        /// Default value of the parameter specifying collection of configurations of spikes coders
        /// </summary>
        public const List<RCNetBaseSettings> DefaultCoderCfgCollection = null;

        //Attribute properties
        /// <summary>
        /// Spiking input encoding regime
        /// </summary>
        public InputEncoder.SpikingInputEncodingRegime Regime { get; }
        
        /// <summary>
        /// Collection of configurations of spikes coders
        /// </summary>
        public List<RCNetBaseSettings> CoderCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="regime">Spiking input encoding regime</param>
        /// <param name="coderCfgCollection">Collection of configurations of spikes coders</param>
        public InputSpikesCoderSettings(InputEncoder.SpikingInputEncodingRegime regime = DefaultRegime,
                                      List<RCNetBaseSettings> coderCfgCollection = DefaultCoderCfgCollection
                                      )
        {
            Regime = regime;
            CoderCfgCollection = new List<RCNetBaseSettings>();
            if (Regime != InputEncoder.SpikingInputEncodingRegime.Forbidden)
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
        /// <param name="regime">Spiking input encoding regime</param>
        /// <param name="coderCfgCollection">Collection of configurations of spikes coders</param>
        public InputSpikesCoderSettings(InputEncoder.SpikingInputEncodingRegime regime,
                                      params RCNetBaseSettings[] coderCfgCollection
                                      )
            :this(regime, new List<RCNetBaseSettings>(coderCfgCollection))
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputSpikesCoderSettings(InputSpikesCoderSettings source)
            : this(source.Regime, source.CoderCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public InputSpikesCoderSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Regime
            Regime = (InputEncoder.SpikingInputEncodingRegime)Enum.Parse(typeof(InputEncoder.SpikingInputEncodingRegime), settingsElem.Attribute("regime").Value, true);
            //Coders configurations
            CoderCfgCollection = null;
            if (Regime != InputEncoder.SpikingInputEncodingRegime.Forbidden)
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRegime { get { return (Regime == DefaultRegime); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultCoderCfgCollection { get { return (CoderCfgCollection == DefaultCoderCfgCollection); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultRegime && IsDefaultCoderCfgCollection; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (Regime != InputEncoder.SpikingInputEncodingRegime.Forbidden)
            {
                if (CoderCfgCollection.Count == 0)
                {
                    throw new ArgumentException($"It must be specified at least one coder configuration.", "CoderCfgCollection");
                }
                foreach(RCNetBaseSettings coderCfg in CoderCfgCollection)
                {
                    if(!A2SCoderFactory.CheckSettings(coderCfg))
                    {
                        throw new ArgumentException($"Unknown coder configuration {coderCfg.GetType().Name}.", "CoderCfgCollection");
                    }
                }
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputSpikesCoderSettings(this);
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
            if (!suppressDefaults || !IsDefaultRegime)
            {
                rootElem.Add(new XAttribute("regime", Regime.ToString()));
            }
            foreach(RCNetBaseSettings coderCfg in CoderCfgCollection)
            {
                rootElem.Add(coderCfg.GetXml(suppressDefaults));
            }
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
            return GetXml("spikesCoder", suppressDefaults);
        }

    }//InputSpikesCoderSettings

}//Namespace

