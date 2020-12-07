using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the OneWinnerDecisionMaker
    /// </summary>
    [Serializable]
    public class OneWinnerDecisionMakerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerOneWinnerDecisionMakerType";
        //Default values
        /// <summary>
        /// Default value of the parameter specifying how rich will be an input for the final probabilities network. True means to use all available sub-predictions from clusters members and False means to use only already aggregated predictions from clusters.
        /// </summary>
        public const bool DefaultConsiderAllSubPredictions = true;

        //Attribute properties
        /// <summary>
        /// Crossvalidation configuration
        /// </summary>
        public CrossvalidationSettings CrossvalidationCfg { get; }

        /// <summary>
        /// Final probabilities network configuration
        /// </summary>
        public FeedForwardNetworkSettings NetCfg { get; }

        /// <summary>
        /// Specifies how rich will be an input for the final probabilities network. True means to use all available sub-predictions from clusters members and False means to use only already aggregated predictions from clusters.
        /// </summary>
        public bool ConsiderAllSubPredictions { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="crossvalidationCfg">Crossvalidation configuration</param>
        /// <param name="netCfg">Final probabilities network configuration</param>
        /// <param name="considerAllSubPredictions">Specifies how rich will be an input for the final probabilities network. True means to use all available sub-predictions from clusters members and False means to use only already aggregated predictions from clusters.</param>
        public OneWinnerDecisionMakerSettings(CrossvalidationSettings crossvalidationCfg,
                                              FeedForwardNetworkSettings netCfg,
                                              bool considerAllSubPredictions = DefaultConsiderAllSubPredictions
                                              )
        {
            CrossvalidationCfg = (CrossvalidationSettings)crossvalidationCfg.DeepClone();
            NetCfg = (FeedForwardNetworkSettings)netCfg.DeepClone();
            ConsiderAllSubPredictions = considerAllSubPredictions;
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public OneWinnerDecisionMakerSettings(OneWinnerDecisionMakerSettings source)
            : this(source.CrossvalidationCfg, source.NetCfg, source.ConsiderAllSubPredictions)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml data containing the settings.</param>
        public OneWinnerDecisionMakerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            CrossvalidationCfg = new CrossvalidationSettings(settingsElem.Element("crossvalidation"));
            NetCfg = new FeedForwardNetworkSettings(settingsElem.Element("ff"));
            ConsiderAllSubPredictions = bool.Parse(settingsElem.Attribute("considerAllSubPredictions").Value);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultConsiderAllSubPredictions { get { return (ConsiderAllSubPredictions == DefaultConsiderAllSubPredictions); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            TrainedOneTakesAllNetworkBuilder.CheckNetCfg(NetCfg);
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new OneWinnerDecisionMakerSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, CrossvalidationCfg.GetXml(suppressDefaults), NetCfg.GetXml(suppressDefaults));
            if (!suppressDefaults || !IsDefaultConsiderAllSubPredictions)
            {
                rootElem.Add(new XAttribute("considerAllSubPredictions", ConsiderAllSubPredictions.ToString().ToLowerInvariant()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("oneWinnerDecisionMaker", suppressDefaults);
        }

    }//OneWinnerDecisionMakerSettings

}//Namespace
