﻿using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the FiringTrace predictor computer.
    /// </summary>
    [Serializable]
    public class PredictorFiringTraceSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PredictorFiringTraceType";
        /// <summary>
        /// The numeric code indicating no data window.
        /// </summary>
        public const int NAWindowNum = 0;
        /// <summary>
        /// The string code indicating no data window.
        /// </summary>
        public const string NAWindowCode = "NA";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying the data window size.
        /// </summary>
        public const int DefaultWindow = NAWindowNum;
        /// <summary>
        /// The default value of the parameter specifying the strength of trace fading.
        /// </summary>
        public const double DefaultFading = 0.005d;

        //Attribute properties
        /// <summary>
        /// Specifies the strength of trace fading.
        /// </summary>
        public double Fading { get; }

        /// <summary>
        /// Specifies the data window size.
        /// </summary>
        public int Window { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fading">Specifies the strength of trace fading.</param>
        /// <param name="window">Specifies the data window size.</param>
        public PredictorFiringTraceSettings(double fading = DefaultFading, int window = DefaultWindow)
        {
            Fading = fading;
            Window = window;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PredictorFiringTraceSettings(PredictorFiringTraceSettings source)
            : this(source.Fading, source.Window)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PredictorFiringTraceSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            string attrValue = settingsElem.Attribute("window").Value;
            Window = attrValue == NAWindowCode ? NAWindowNum : int.Parse(attrValue, CultureInfo.InvariantCulture);
            Fading = double.Parse(settingsElem.Attribute("fading").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.FiringTrace; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfActivations { get { return 0; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfFirings { get { return Window; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationStat { get { return false; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationDiffStat { get { return false; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultFading { get { return (Fading == DefaultFading); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultWindow { get { return (Window == DefaultWindow); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultFading && IsDefaultWindow; } }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Fading < 0 || Fading > 1)
            {
                throw new ArgumentException($"Invalid Fading {Fading.ToString(CultureInfo.InvariantCulture)}. Fading must be GE0 and LE1.", "Fading");
            }
            if (Window < 0 || Window == 1 || Window > 1024)
            {
                throw new ArgumentException($"Invalid Window size {Window.ToString(CultureInfo.InvariantCulture)}. Window size must be GE0 and NE1 and LE1024.", "Window");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorFiringTraceSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultWindow)
            {
                rootElem.Add(new XAttribute("window", Window == NAWindowNum ? NAWindowCode : Window.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultFading)
            {
                rootElem.Add(new XAttribute("fading", Fading.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(PredictorFactory.GetXmlName(ID), suppressDefaults);
        }

    }//PredictorFiringTraceSettings

}//Namespace
