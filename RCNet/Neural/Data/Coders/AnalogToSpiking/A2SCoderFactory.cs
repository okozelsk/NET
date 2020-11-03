using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Coders.AnalogToSpiking
{
    /// <summary>
    /// Provides proper load of A2S coders settings and related instantiation of A2S coders
    /// </summary>
    public static class A2SCoderFactory
    {
        /// <summary>
        /// Based on element name loads proper type of A2S coder settings
        /// </summary>
        /// <param name="elem">Element containing settings</param>
        public static IA2SCoderSettings LoadSettings(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "potentiometer":
                    return new A2SCoderPotentiometerSettings(elem);
                case "bintree":
                    return new A2SCoderBintreeSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}", "elem");
            }
        }

        /// <summary>
        /// Instantiates A2S coder of proper type according to given settings
        /// </summary>
        /// <param name="settings">Settings of A2S coder</param>
        public static A2SCoderBase Create(IA2SCoderSettings settings)
        {
            Type type = settings.GetType();
            if(type == typeof(A2SCoderPotentiometerSettings))
            {
                return new A2SCoderPotentiometer((A2SCoderPotentiometerSettings)settings);
            }
            else if(type == typeof(A2SCoderBintreeSettings))
            {
                return new A2SCoderBintree((A2SCoderBintreeSettings)settings);
            }
            else
            {
                throw new ArgumentException($"Unexpected A2S coder type {type.Name}", "settings");
            }
        }

    }//A2SCoderFactory

}//Namespace

