using RCNet.MathTools;
using RCNet.RandomValue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Provides proper load of settings and instantiation of data generators
    /// </summary>
    public static class GeneratorFactory
    {
        /// <summary>
        /// Based on element name loads proper type of generator settings
        /// </summary>
        /// <param name="elem">Element containing settings</param>
        public static RCNetBaseSettings LoadSettings(XElement elem)
        {
            switch(elem.Name.LocalName)
            {
                case "pulse": return new PulseGeneratorSettings(elem);
                case "random": return new RandomValueSettings(elem);
                case "sinusoidal": return new SinusoidalGeneratorSettings(elem);
                case "mackeyGlass": return new MackeyGlassGeneratorSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected generator's element name {elem.Name.LocalName}", "elem");
            }
        }

        /// <summary>
        /// Instantiates generator of proper type according to settings
        /// </summary>
        /// <param name="settings">Generator configuration</param>
        public static IGenerator Create(RCNetBaseSettings settings)
        {
            Type cfgType = settings.GetType();
            if(cfgType == typeof(PulseGeneratorSettings))
            {
                return new PulseGenerator((PulseGeneratorSettings)settings);
            }
            else if (cfgType == typeof(RandomValueSettings))
            {
                return new RandomGenerator((RandomValueSettings)settings);
            }
            else if (cfgType == typeof(SinusoidalGeneratorSettings))
            {
                return new SinusoidalGenerator((SinusoidalGeneratorSettings)settings);
            }
            else if (cfgType == typeof(MackeyGlassGeneratorSettings))
            {
                return new MackeyGlassGenerator((MackeyGlassGeneratorSettings)settings);
            }
            else
            {
                throw new ArgumentException($"Unexpected transformer configuration {cfgType.Name}", "settings");
            }
        }

    }//GeneratorFactory
}//Namespace

