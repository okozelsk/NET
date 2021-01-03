using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor
{
    /// <summary>
    /// Configuration of the Activation predictor computer.
    /// </summary>
    [Serializable]
    public class PredictorActivationSettings : RCNetBaseSettings, IPredictorSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PredictorActivationType";

        //Attribute properties

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        public PredictorActivationSettings()
        {
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public PredictorActivationSettings(PredictorActivationSettings source)
            : this()
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public PredictorActivationSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public PredictorsProvider.PredictorID ID { get { return PredictorsProvider.PredictorID.Activation; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfActivations { get { return 0; } }

        /// <inheritdoc/>
        public int RequiredWndSizeOfFirings { get { return 0; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationStat { get { return false; } }

        /// <inheritdoc/>
        public bool NeedsContinuousActivationDiffStat { get { return false; } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return true; } }


        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new PredictorActivationSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml(PredictorFactory.GetXmlName(ID), suppressDefaults);
        }

    }//PredictorActivationSettings

}//Namespace
