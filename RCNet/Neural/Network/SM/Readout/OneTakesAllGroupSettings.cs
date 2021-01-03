using System;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the "One Takes All" group.
    /// </summary>
    [Serializable]
    public class OneTakesAllGroupSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutOneTakesAllGroupType";

        //Attribute properties
        /// <summary>
        /// The name of the group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The configuration of the decision.
        /// </summary>
        public IOneTakesAllDecisionSettings DecisionCfg { get; }


        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="name">The name of the group.</param>
        /// <param name="decisionCfg">The configuration of the decision.</param>
        public OneTakesAllGroupSettings(string name, IOneTakesAllDecisionSettings decisionCfg)
        {
            Name = name;
            DecisionCfg = (IOneTakesAllDecisionSettings)decisionCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public OneTakesAllGroupSettings(OneTakesAllGroupSettings source)
            : this(source.Name, source.DecisionCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public OneTakesAllGroupSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Name = settingsElem.Attribute("name").Value;
            XElement decisionElem = settingsElem.Elements().FirstOrDefault();
            if (decisionElem.Name.LocalName == "basicDecision")
            {
                DecisionCfg = new OneTakesAllBasicDecisionSettings(decisionElem);
            }
            else if (decisionElem.Name.LocalName == "clusterChainDecision")
            {
                DecisionCfg = new OneTakesAllClusterChainDecisionSettings(decisionElem);
            }
            else
            {
                throw new ArgumentException($"Unknown decision configuration element {decisionElem.Name.LocalName}.", "elem");
            }
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
            Type decisionType = DecisionCfg.GetType();
            if (decisionType != typeof(OneTakesAllBasicDecisionSettings) && decisionType != typeof(OneTakesAllClusterChainDecisionSettings))
            {
                throw new ArgumentException($"Invalid type of decision configuration {decisionType.Name}.", "DecisionCfg");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new OneTakesAllGroupSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, DecisionCfg.GetXml(suppressDefaults), new XAttribute("name", Name));
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("group", suppressDefaults);
        }

    }//OneTakesAllGroupSettings

}//Namespace
