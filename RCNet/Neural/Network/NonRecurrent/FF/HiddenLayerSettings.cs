using RCNet.Neural.Activation;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the feed forward network's hidden layer.
    /// </summary>
    [Serializable]
    public class HiddenLayerSettings : RCNetBaseSettings
    {
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "FFNetHiddenLayerType";

        //Attributes
        /// <summary>
        /// The number of neurons.
        /// </summary>
        public int NumOfNeurons { get; }
        /// <summary>
        /// The configuration of the activation function.
        /// </summary>
        public IActivationSettings ActivationCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfNeurons">The number of neurons.</param>
        /// <param name="activationCfg">The configuration of the activation function.</param>
        public HiddenLayerSettings(int numOfNeurons, IActivationSettings activationCfg)
        {
            NumOfNeurons = numOfNeurons;
            ActivationCfg = (IActivationSettings)activationCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public HiddenLayerSettings(HiddenLayerSettings source)
            : this(source.NumOfNeurons, source.ActivationCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public HiddenLayerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfNeurons = int.Parse(settingsElem.Attribute("neurons").Value);
            ActivationCfg = ActivationFactory.LoadSettings(settingsElem.Elements().First());
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (NumOfNeurons < 1)
            {
                throw new ArgumentException($"Invalid NumOfNeurons {NumOfNeurons.ToString(CultureInfo.InvariantCulture)}. NumOfNeurons must be GT 0.", "NumOfNeurons");
            }
            if (!FeedForwardNetwork.IsAllowedHiddenAF(ActivationCfg))
            {
                throw new ArgumentException($"Specified activation function can't be used in the hidden layer of a FF network.", "ActivationCfg");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new HiddenLayerSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            return Validate(new XElement(rootElemName, new XAttribute("neurons", NumOfNeurons.ToString(CultureInfo.InvariantCulture)),
                                                       ActivationCfg.GetXml(suppressDefaults)),
                                                       XsdTypeName);
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("layer", suppressDefaults);
        }

    }//HiddenLayerSettings

}//Namespace
