using RCNet.Neural.Data.Coders.AnalogToSpiking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Provides proper load of spikes encoding settings and related instantiation of A2S coders
    /// </summary>
    public static class SpikesEncodingFactory
    {
        /// <summary>
        /// Based on element name loads proper type of spikes encoding settings
        /// </summary>
        /// <param name="elem">Element containing settings</param>
        public static ISpikesEncodingSettings LoadSettings(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "population":
                    return new SpikesEncodingPopulationSettings(elem);
                case "spike-train":
                    return new SpikesEncodingSpiketrainSettings(elem);
                case "forbidden":
                    return new SpikesEncodingForbiddenSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}", "elem");
            }
        }

        /// <summary>
        /// Instantiates associated A2S coder or returns null if spikes encoding is forbidden
        /// </summary>
        /// <param name="settings">Settings of spikes encoding</param>
        public static A2SCoderBase CreateCoder(ISpikesEncodingSettings settings)
        {
            switch (settings.EncodingType)
            {
                case InputEncoder.SpikesEncodingType.Forbidden:
                    return null;
                case InputEncoder.SpikesEncodingType.Population:
                    return A2SCoderFactory.Create(((SpikesEncodingPopulationSettings)settings).CoderCfg);
                case InputEncoder.SpikesEncodingType.Spiketrain:
                    return A2SCoderFactory.Create(((SpikesEncodingSpiketrainSettings)settings).CoderCfg);
                default:
                    throw new ArgumentException($"Unexpected spikes encoding type {settings.EncodingType}", "settings");
            }
        }

    }//SpikesEncodingFactory

}//Namespace

