using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Provides a proper instantiation of the transformers and also proper loading of their configurations.
    /// </summary>
    public static class TransformerFactory
    {
        /// <summary>
        /// Based on the xml element name loads the proper type of transformer configuration.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration.</param>
        public static RCNetBaseSettings LoadSettings(XElement elem)
        {
            switch (elem.Name.LocalName)
            {
                case "diff": return new DiffTransformerSettings(elem);
                case "cdiv": return new CDivTransformerSettings(elem);
                case "log": return new LogTransformerSettings(elem);
                case "exp": return new ExpTransformerSettings(elem);
                case "power": return new PowerTransformerSettings(elem);
                case "yeoJohnson": return new YeoJohnsonTransformerSettings(elem);
                case "stat": return new MWStatTransformerSettings(elem);
                case "mul": return new MulTransformerSettings(elem);
                case "div": return new DivTransformerSettings(elem);
                case "linear": return new LinearTransformerSettings(elem);
                default:
                    throw new ArgumentException($"Unexpected element name {elem.Name.LocalName}", "elem");
            }
        }

        /// <summary>
        /// Gets the collection of names of the fields associated with the transformer configuration.
        /// </summary>
        /// <param name="cfg">The transformer configuration.</param>
        public static List<string> GetAssociatedNames(RCNetBaseSettings cfg)
        {
            List<string> names = new List<string>();
            Type cfgType = cfg.GetType();
            if (cfgType == typeof(DiffTransformerSettings))
            {
                names.Add(((DiffTransformerSettings)cfg).InputFieldName);
            }
            else if (cfgType == typeof(CDivTransformerSettings))
            {
                names.Add(((CDivTransformerSettings)cfg).InputFieldName);
            }
            else if (cfgType == typeof(LogTransformerSettings))
            {
                names.Add(((LogTransformerSettings)cfg).InputFieldName);
            }
            else if (cfgType == typeof(ExpTransformerSettings))
            {
                names.Add(((ExpTransformerSettings)cfg).InputFieldName);
            }
            else if (cfgType == typeof(PowerTransformerSettings))
            {
                names.Add(((PowerTransformerSettings)cfg).InputFieldName);
            }
            else if (cfgType == typeof(YeoJohnsonTransformerSettings))
            {
                names.Add(((YeoJohnsonTransformerSettings)cfg).InputFieldName);
            }
            else if (cfgType == typeof(MWStatTransformerSettings))
            {
                names.Add(((MWStatTransformerSettings)cfg).InputFieldName);
            }
            else if (cfgType == typeof(MulTransformerSettings))
            {
                names.Add(((MulTransformerSettings)cfg).XInputFieldName);
                names.Add(((MulTransformerSettings)cfg).YInputFieldName);
            }
            else if (cfgType == typeof(DivTransformerSettings))
            {
                names.Add(((DivTransformerSettings)cfg).XInputFieldName);
                names.Add(((DivTransformerSettings)cfg).YInputFieldName);
            }
            else if (cfgType == typeof(LinearTransformerSettings))
            {
                names.Add(((LinearTransformerSettings)cfg).XInputFieldName);
                names.Add(((LinearTransformerSettings)cfg).YInputFieldName);
            }
            else
            {
                throw new ArgumentException($"Unexpected transformer configuration {cfgType.Name}", "cfg");
            }
            return names;
        }

        /// <summary>
        /// Instantiates the appropriate transformer.
        /// </summary>
        /// <param name="fieldNames">The collection of names of all already available fields.</param>
        /// <param name="cfg">The transformer configuration.</param>
        public static ITransformer Create(List<string> fieldNames, RCNetBaseSettings cfg)
        {
            Type cfgType = cfg.GetType();
            if (cfgType == typeof(DiffTransformerSettings))
            {
                return new DiffTransformer(fieldNames, (DiffTransformerSettings)cfg);
            }
            else if (cfgType == typeof(CDivTransformerSettings))
            {
                return new CDivTransformer(fieldNames, (CDivTransformerSettings)cfg);
            }
            else if (cfgType == typeof(LogTransformerSettings))
            {
                return new LogTransformer(fieldNames, (LogTransformerSettings)cfg);
            }
            else if (cfgType == typeof(ExpTransformerSettings))
            {
                return new ExpTransformer(fieldNames, (ExpTransformerSettings)cfg);
            }
            else if (cfgType == typeof(PowerTransformerSettings))
            {
                return new PowerTransformer(fieldNames, (PowerTransformerSettings)cfg);
            }
            else if (cfgType == typeof(YeoJohnsonTransformerSettings))
            {
                return new YeoJohnsonTransformer(fieldNames, (YeoJohnsonTransformerSettings)cfg);
            }
            else if (cfgType == typeof(MWStatTransformerSettings))
            {
                return new MWStatTransformer(fieldNames, (MWStatTransformerSettings)cfg);
            }
            else if (cfgType == typeof(MulTransformerSettings))
            {
                return new MulTransformer(fieldNames, (MulTransformerSettings)cfg);
            }
            else if (cfgType == typeof(DivTransformerSettings))
            {
                return new DivTransformer(fieldNames, (DivTransformerSettings)cfg);
            }
            else if (cfgType == typeof(LinearTransformerSettings))
            {
                return new LinearTransformer(fieldNames, (LinearTransformerSettings)cfg);
            }
            else
            {
                throw new ArgumentException($"Unexpected transformer configuration {cfgType.Name}", "settings");
            }
        }

    }//TransformersFactory
}//Namespace

