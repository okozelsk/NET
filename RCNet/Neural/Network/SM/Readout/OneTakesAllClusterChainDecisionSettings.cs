using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the "One Takes All" cluster chain decision.
    /// </summary>
    [Serializable]
    public class OneTakesAllClusterChainDecisionSettings : RCNetBaseSettings, IOneTakesAllDecisionSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutOneTakesAllClusterChainDecisionType";
        //Default values
        /// <summary>
        /// The default value of the parameter specifying whether to use the group readout units final results as an input into the cluster.
        /// </summary>
        public const bool DefaultUseReadoutUnitsFinalResult = true;
        /// <summary>
        /// The default value of the parameter specifying whether to use the group readout units sub-results as an input into the cluster.
        /// </summary>
        public const bool DefaultUseReadoutUnitsSubResults = true;

        //Attribute properties
        /// <summary>
        /// The cluster chain configuration.
        /// </summary>
        public TNRNetClusterChainProbabilisticSettings ClusterChainCfg { get; }

        /// <summary>
        /// Specifies whether to use the group readout units final results as an input into the cluster.
        /// </summary>
        public bool UseReadoutUnitsFinalResult { get; }

        /// <summary>
        /// Specifies whether to use the group readout units sub-results as an input into the cluster.
        /// </summary>
        public bool UseReadoutUnitsSubResults { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="clusterChainCfg">The cluster chain configuration.</param>
        /// <param name="useReadoutUnitsFinalResult">Specifies whether to use the group readout units final results as an input into the cluster.</param>
        /// <param name="useReadoutUnitsSubResults">Specifies whether to use the group readout units sub-results as an input into the cluster.</param>
        public OneTakesAllClusterChainDecisionSettings(TNRNetClusterChainProbabilisticSettings clusterChainCfg,
                                                       bool useReadoutUnitsFinalResult = DefaultUseReadoutUnitsFinalResult,
                                                       bool useReadoutUnitsSubResults = DefaultUseReadoutUnitsSubResults
                                                       )
        {
            ClusterChainCfg = (TNRNetClusterChainProbabilisticSettings)clusterChainCfg.DeepClone();
            UseReadoutUnitsFinalResult = useReadoutUnitsFinalResult;
            UseReadoutUnitsSubResults = useReadoutUnitsSubResults;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public OneTakesAllClusterChainDecisionSettings(OneTakesAllClusterChainDecisionSettings source)
            : this(source.ClusterChainCfg, source.UseReadoutUnitsFinalResult, source.UseReadoutUnitsSubResults)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public OneTakesAllClusterChainDecisionSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ClusterChainCfg = new TNRNetClusterChainProbabilisticSettings(settingsElem.Element("clusterChain"));
            UseReadoutUnitsFinalResult = bool.Parse(settingsElem.Attribute("useReadoutUnitsFinalResult").Value);
            UseReadoutUnitsSubResults = bool.Parse(settingsElem.Attribute("useReadoutUnitsSubResults").Value);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public OneTakesAllGroup.OneTakesAllDecisionMethod DecisionMethod { get { return OneTakesAllGroup.OneTakesAllDecisionMethod.ClusterChain; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultUseReadoutUnitsFinalResult { get { return (UseReadoutUnitsFinalResult == DefaultUseReadoutUnitsFinalResult); } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultUseReadoutUnitsSubResults { get { return (UseReadoutUnitsSubResults == DefaultUseReadoutUnitsSubResults); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (!UseReadoutUnitsFinalResult && !UseReadoutUnitsSubResults)
            {
                throw new ArgumentException("At least one of the 'use' switches has to be switched on.");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new OneTakesAllClusterChainDecisionSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, ClusterChainCfg.GetXml(suppressDefaults));
            if (!suppressDefaults || !IsDefaultUseReadoutUnitsFinalResult)
            {
                rootElem.Add(new XAttribute("useReadoutUnitsFinalResult", UseReadoutUnitsFinalResult.ToString().ToLowerInvariant()));
            }
            if (!suppressDefaults || !IsDefaultUseReadoutUnitsSubResults)
            {
                rootElem.Add(new XAttribute("useReadoutUnitsSubResults", UseReadoutUnitsSubResults.ToString().ToLowerInvariant()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("clusterChainDecision", suppressDefaults);
        }

    }//OneTakesAllClusterChainDecisionSettings

}//Namespace
