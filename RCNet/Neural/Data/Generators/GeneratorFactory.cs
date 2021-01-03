using RCNet.RandomValue;
using System;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Generators
{
    /// <summary>
    /// Provides a proper instantiation of the generators and also proper loading of their configurations.
    /// </summary>
    public static class GeneratorFactory
    {
        /// <summary>
        /// Based on the xml element name loads the proper type of generator configuration.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration.</param>
        public static RCNetBaseSettings LoadSettings(XElement elem)
        {
            switch (elem.Name.LocalName)
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
        /// Instantiates the appropriate generator.
        /// </summary>
        /// <param name="cfg">The generator configuration.</param>
        public static IGenerator Create(RCNetBaseSettings cfg)
        {
            Type cfgType = cfg.GetType();
            if (cfgType == typeof(PulseGeneratorSettings))
            {
                return new PulseGenerator((PulseGeneratorSettings)cfg);
            }
            else if (cfgType == typeof(RandomValueSettings))
            {
                return new RandomGenerator((RandomValueSettings)cfg);
            }
            else if (cfgType == typeof(SinusoidalGeneratorSettings))
            {
                return new SinusoidalGenerator((SinusoidalGeneratorSettings)cfg);
            }
            else if (cfgType == typeof(MackeyGlassGeneratorSettings))
            {
                return new MackeyGlassGenerator((MackeyGlassGeneratorSettings)cfg);
            }
            else
            {
                throw new ArgumentException($"Unexpected transformer configuration {cfgType.Name}", "settings");
            }
        }

    }//GeneratorFactory

}//Namespace

