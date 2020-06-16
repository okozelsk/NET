﻿using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of input pattern resampling
    /// </summary>
    [Serializable]
    public class SpikeCodeSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInputSpikeCodeType";
        //Default values
        /// <summary>
        /// Default value of parameter specifying length of the half of component code
        /// </summary>
        public const int DefaultComponentHalfCodeLength = 16;
        /// <summary>
        /// Default value of parameter specifying firing threshold of the most sensitive input neuron
        /// </summary>
        public const double DefaultLowestThreshold = 1e-5;
        /// <summary>
        /// Default value of parameter specifying if to use strength of the current analog signal as a component of the spike code
        /// </summary>
        public const bool DefaultSignalComponent = true;
        /// <summary>
        /// Default value of parameter specifying if to use difference of the current analog signal and previous analog signal as a component of the spike code
        /// </summary>
        public const bool DefaultDeltaComponent = false;


        //Attribute properties
        /// <summary>
        /// Length of the half of component code
        /// </summary>
        public int ComponentHalfCodeLength { get; }

        /// <summary>
        /// Firing threshold of the most sensitive input neuron
        /// </summary>
        public double LowestThreshold { get; }

        /// <summary>
        /// Specifies whether to use strength of the current analog signal as a component of the spike code
        /// </summary>
        public bool SignalComponent { get; }

        /// <summary>
        /// Specifies whether to use difference of the current analog signal and previous analog signal as a component of the spike code
        /// </summary>
        public bool DeltaComponent { get; }


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="componentHalfCodeLength">Length of the half of component code</param>
        /// <param name="lowestThreshold">Firing threshold of the most sensitive input neuron</param>
        /// <param name="signalComponent">Specifies whether to use strength of the current analog signal as a component of the spike code</param>
        /// <param name="deltaComponent">Specifies whether to use difference of the current analog signal and previous analog signal as a component of the spike code</param>
        public SpikeCodeSettings(int componentHalfCodeLength = DefaultComponentHalfCodeLength,
                                 double lowestThreshold = DefaultLowestThreshold,
                                 bool signalComponent = DefaultSignalComponent,
                                 bool deltaComponent = DefaultDeltaComponent
                                 )
        {
            ComponentHalfCodeLength = componentHalfCodeLength;
            LowestThreshold = lowestThreshold;
            SignalComponent = signalComponent;
            DeltaComponent = deltaComponent;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public SpikeCodeSettings(SpikeCodeSettings source)
            : this(source.ComponentHalfCodeLength, source.LowestThreshold, source.SignalComponent, source.DeltaComponent)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public SpikeCodeSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ComponentHalfCodeLength = int.Parse(settingsElem.Attribute("componentHalfCodeLength").Value, CultureInfo.InvariantCulture);
            LowestThreshold = double.Parse(settingsElem.Attribute("lowestThreshold").Value, CultureInfo.InvariantCulture);
            SignalComponent = bool.Parse(settingsElem.Attribute("signalComponent").Value);
            DeltaComponent = bool.Parse(settingsElem.Attribute("deltaComponent").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultComponentHalfCodeLength { get { return (ComponentHalfCodeLength == DefaultComponentHalfCodeLength); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultLowestThreshold { get { return (LowestThreshold == DefaultLowestThreshold); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultUseDeviation { get { return (SignalComponent == DefaultSignalComponent); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultUseDifference { get { return (DeltaComponent == DefaultDeltaComponent); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults
        {
            get
            {
                return IsDefaultComponentHalfCodeLength &&
                       IsDefaultLowestThreshold &&
                       IsDefaultUseDeviation &&
                       IsDefaultUseDifference;
            }
        }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (ComponentHalfCodeLength < 1 || ComponentHalfCodeLength > 1024)
            {
                throw new ArgumentException($"Invalid ComponentHalfCodeLength {ComponentHalfCodeLength.ToString(CultureInfo.InvariantCulture)}. ComponentHalfCodeLength must be GE to 1 and LE to 1024.", "ComponentHalfCodeLength");
            }
            if (LowestThreshold <= 0 || LowestThreshold >= 1d)
            {
                throw new ArgumentException($"Invalid LowestThreshold {LowestThreshold.ToString(CultureInfo.InvariantCulture)}. LowestThreshold must be GT 0 and LT 1.", "LowestThreshold");
            }
            if (!SignalComponent && !DeltaComponent)
            {
                throw new ArgumentException($"At least one component of the spike code has to be used.", "SignalComponent/DeltaComponent");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new SpikeCodeSettings(this);
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
            if (!suppressDefaults || !IsDefaultComponentHalfCodeLength)
            {
                rootElem.Add(new XAttribute("componentHalfCodeLength", ComponentHalfCodeLength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultLowestThreshold)
            {
                rootElem.Add(new XAttribute("lowestThreshold", LowestThreshold.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultUseDeviation)
            {
                rootElem.Add(new XAttribute("signalComponent", SignalComponent.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultUseDifference)
            {
                rootElem.Add(new XAttribute("deltaComponent", DeltaComponent.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
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
            return GetXml("spikeCode", suppressDefaults);
        }

    }//SpikeCodeSettings

}//Namespace
