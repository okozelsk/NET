using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Data.Transformers
{
    /// <summary>
    /// Provides proper load of settings and instantiation of data transformers
    /// </summary>
    public static class TransformerFactory
    {
        /// <summary>
        /// Based on element name loads proper type of transformer settings
        /// </summary>
        /// <param name="elem">Element containing settings</param>
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
        /// Returns collection of names of the fields associated with the transformed field
        /// </summary>
        /// <param name="settings">Transformed field configuration</param>
        public static List<string> GetAssociatedNames(RCNetBaseSettings settings)
        {
            List<string> names = new List<string>();
            Type cfgType = settings.GetType();
            if (cfgType == typeof(DiffTransformerSettings))
            {
                names.Add(((DiffTransformerSettings)settings).InputFieldName);
            }
            else if (cfgType == typeof(CDivTransformerSettings))
            {
                names.Add(((CDivTransformerSettings)settings).InputFieldName);
            }
            else if (cfgType == typeof(LogTransformerSettings))
            {
                names.Add(((LogTransformerSettings)settings).InputFieldName);
            }
            else if (cfgType == typeof(ExpTransformerSettings))
            {
                names.Add(((ExpTransformerSettings)settings).InputFieldName);
            }
            else if (cfgType == typeof(PowerTransformerSettings))
            {
                names.Add(((PowerTransformerSettings)settings).InputFieldName);
            }
            else if (cfgType == typeof(YeoJohnsonTransformerSettings))
            {
                names.Add(((YeoJohnsonTransformerSettings)settings).InputFieldName);
            }
            else if (cfgType == typeof(MWStatTransformerSettings))
            {
                names.Add(((MWStatTransformerSettings)settings).InputFieldName);
            }
            else if (cfgType == typeof(MulTransformerSettings))
            {
                names.Add(((MulTransformerSettings)settings).XInputFieldName);
                names.Add(((MulTransformerSettings)settings).YInputFieldName);
            }
            else if (cfgType == typeof(DivTransformerSettings))
            {
                names.Add(((DivTransformerSettings)settings).XInputFieldName);
                names.Add(((DivTransformerSettings)settings).YInputFieldName);
            }
            else if (cfgType == typeof(LinearTransformerSettings))
            {
                names.Add(((LinearTransformerSettings)settings).XInputFieldName);
                names.Add(((LinearTransformerSettings)settings).YInputFieldName);
            }
            else
            {
                throw new ArgumentException($"Unexpected transformer configuration {cfgType.Name}", "settings");
            }
            return names;
        }

        /// <summary>
        /// Instantiates transformer of proper type according to settings
        /// </summary>
        /// <param name="fieldNames">Collection of names of all available input fields</param>
        /// <param name="settings">Transformer configuration</param>
        public static ITransformer Create(List<string> fieldNames, RCNetBaseSettings settings)
        {
            Type cfgType = settings.GetType();
            if (cfgType == typeof(DiffTransformerSettings))
            {
                return new DiffTransformer(fieldNames, (DiffTransformerSettings)settings);
            }
            else if (cfgType == typeof(CDivTransformerSettings))
            {
                return new CDivTransformer(fieldNames, (CDivTransformerSettings)settings);
            }
            else if (cfgType == typeof(LogTransformerSettings))
            {
                return new LogTransformer(fieldNames, (LogTransformerSettings)settings);
            }
            else if (cfgType == typeof(ExpTransformerSettings))
            {
                return new ExpTransformer(fieldNames, (ExpTransformerSettings)settings);
            }
            else if (cfgType == typeof(PowerTransformerSettings))
            {
                return new PowerTransformer(fieldNames, (PowerTransformerSettings)settings);
            }
            else if (cfgType == typeof(YeoJohnsonTransformerSettings))
            {
                return new YeoJohnsonTransformer(fieldNames, (YeoJohnsonTransformerSettings)settings);
            }
            else if (cfgType == typeof(MWStatTransformerSettings))
            {
                return new MWStatTransformer(fieldNames, (MWStatTransformerSettings)settings);
            }
            else if (cfgType == typeof(MulTransformerSettings))
            {
                return new MulTransformer(fieldNames, (MulTransformerSettings)settings);
            }
            else if (cfgType == typeof(DivTransformerSettings))
            {
                return new DivTransformer(fieldNames, (DivTransformerSettings)settings);
            }
            else if (cfgType == typeof(LinearTransformerSettings))
            {
                return new LinearTransformer(fieldNames, (LinearTransformerSettings)settings);
            }
            else
            {
                throw new ArgumentException($"Unexpected transformer configuration {cfgType.Name}", "settings");
            }
        }

    }//TransformersFactory
}//Namespace

