using System;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the "One Takes All" basic decision.
    /// </summary>
    [Serializable]
    public class OneTakesAllBasicDecisionSettings : RCNetBaseSettings, IOneTakesAllDecisionSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutOneTakesAllBasicDecisionType";

        //Attribute properties

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        public OneTakesAllBasicDecisionSettings()
        {
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public OneTakesAllBasicDecisionSettings(OneTakesAllBasicDecisionSettings source)
            : this()
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public OneTakesAllBasicDecisionSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public OneTakesAllGroup.OneTakesAllDecisionMethod DecisionMethod { get { return OneTakesAllGroup.OneTakesAllDecisionMethod.Basic; } }

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
            return new OneTakesAllBasicDecisionSettings(this);
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
            return GetXml("basicDecision", suppressDefaults);
        }

    }//OneTakesAllBasicDecisionSettings

}//Namespace
