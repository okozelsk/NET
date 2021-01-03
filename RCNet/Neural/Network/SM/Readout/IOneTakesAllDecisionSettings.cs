using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// The common interface of "One Takes All" decision configurations.
    /// </summary>
    public interface IOneTakesAllDecisionSettings
    {

        /// <inheritdoc cref="OneTakesAllGroup.OneTakesAllDecisionMethod" />
        OneTakesAllGroup.OneTakesAllDecisionMethod DecisionMethod { get; }

        /// <inheritdoc cref="RCNetBaseSettings.DeepClone" />
        RCNetBaseSettings DeepClone();

        /// <inheritdoc cref="RCNetBaseSettings.GetXml(bool)" />
        XElement GetXml(bool suppressDefaults);


    }//IOneTakesAllDecisionSettings

}//Namespace
