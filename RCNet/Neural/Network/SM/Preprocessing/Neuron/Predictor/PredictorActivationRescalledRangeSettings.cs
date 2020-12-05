using RCNet.MathTools;
using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the ActivationRescalledRange predictor
    /// </summary>
    [Serializable]
    public class PredictorActivationRescalledRangeSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorActivationRescalledRangeType";

        //Attribute properties
        /// <summary>
        /// Specifies data window size
        /// </summary>
        public int Window { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="window">Specifies data window size</param>
        public PredictorActivationRescalledRangeSettings(int window)
        {
            Window = window;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PredictorActivationRescalledRangeSettings(PredictorActivationRescalledRangeSettings source)
            : this(source.Window)
        {
            return;
        }

        /// <summary>
        /// Creates initialized instance using xml element
        /// </summary>
        /// <param name="elem">Xml element containing settings</param>
        public PredictorActivationRescalledRangeSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Window = int.Parse(settingsElem.Attribute("window").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.ActivationRescalledRange; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfActivations { get { return Window; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfFirings { get { return 0; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationStat { get { return false; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationDiffStat { get { return false; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Window < 2 || Window > 1024)
            {
                throw new ArgumentException($"Invalid Window size {Window.ToString(CultureInfo.InvariantCulture)}. Window size must be GE2 and LE1024.", "Window");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorActivationRescalledRangeSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName,
                                             new XAttribute("window", Window.ToString(CultureInfo.InvariantCulture))
                                             );
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(PredictorFactory.GetXmlName(ID), suppressDefaults);
        }

    }//PredictorActivationRescalledRangeSettings

}//Namespace
