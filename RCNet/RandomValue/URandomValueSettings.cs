using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the unsigned random value.
    /// </summary>
    [Serializable]
    public class URandomValueSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "URandomValueType";

        //Default values
        /// <summary>
        /// Default type of distribution
        /// </summary>
        public const RandomCommon.DistributionType DefaultDistributionType = RandomCommon.DistributionType.Uniform;

        //Attribute properties
        /// <summary>
        /// The min value.
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// The max value.
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// The configuration of the distribution.
        /// </summary>
        public IDistrSettings DistrCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="min">The min value (inclusive).</param>
        /// <param name="max">The max value (exclusive).</param>
        /// <param name="distrCfg">The configuration of the distribution.</param>
        public URandomValueSettings(double min,
                                    double max,
                                    IDistrSettings distrCfg = null
                                    )
        {
            Min = min;
            Max = max;
            DistrCfg = distrCfg;
            if (DistrCfg == null)
            {
                DistrCfg = new UniformDistrSettings();
            }
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public URandomValueSettings(URandomValueSettings source)
        {
            Min = source.Min;
            Max = source.Max;
            DistrCfg = (IDistrSettings)((RCNetBaseSettings)source.DistrCfg).DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public URandomValueSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Min = double.Parse(settingsElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(settingsElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            XElement distrParamsElem = settingsElem.Elements().FirstOrDefault();
            if (distrParamsElem == null)
            {
                DistrCfg = new UniformDistrSettings();
            }
            else
            {
                DistrCfg = RandomCommon.LoadUDistrCfg(distrParamsElem);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultDistrType { get { return DistrType == RandomCommon.DistributionType.Uniform; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        /// <inheritdoc />
        public RandomCommon.DistributionType DistrType { get { return DistrCfg.Type; } }

        //Methods
        //Static methods
        /// <summary>
        /// Loads or creates the configuration of the unsigned random value.
        /// </summary>
        /// <remarks>
        /// Checks whether exists the specified descendant element under the root element and if so, loads the configuration.
        /// If the specified descendant element does not exist, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="rootElem">The root xml element.</param>
        /// <param name="descendant">The name of descendant element containing the configuration data.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static URandomValueSettings LoadOrCreate(XElement rootElem, string descendant, double min, double max)
        {
            XElement descendantElement = rootElem.Elements(descendant).FirstOrDefault();
            if (descendantElement != null)
            {
                return new URandomValueSettings(descendantElement);
            }
            else
            {
                return new URandomValueSettings(min, max);
            }
        }

        /// <summary>
        /// Loads or creates the configuration of the unsigned random value.
        /// </summary>
        /// <remarks>
        /// Checks whether exists the specified descendant element under the root element and if so, loads the configuration.
        /// If the specified descendant element does not exist, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="rootElem">The root xml element.</param>
        /// <param name="descendant">The name of descendant element containing the configuration data.</param>
        /// <param name="constValue">The constant value (the same min and max values).</param>
        public static URandomValueSettings LoadOrCreate(XElement rootElem, string descendant, double constValue)
        {
            return LoadOrCreate(rootElem, descendant, constValue, constValue);
        }

        /// <summary>
        /// Clones the existing configuration or creates the new configuration of the unsigned random value.
        /// </summary>
        /// <remarks>
        /// Checks whether the specified source configuration instance is not null and if so, creates its clone.
        /// If the source configuration instance is null, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="source">The source configuration instance.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        public static URandomValueSettings CloneOrCreate(URandomValueSettings source, double min, double max)
        {
            if (source == null)
            {
                return new URandomValueSettings(min, max);
            }
            else
            {
                return (URandomValueSettings)source.DeepClone();
            }
        }

        /// <summary>
        /// Clones the existing configuration or creates the new configuration of the unsigned random value.
        /// </summary>
        /// <remarks>
        /// Checks whether the specified source configuration instance is not null and if so, creates its clone.
        /// If the source configuration instance is null, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="source">The source configuration instance.</param>
        /// <param name="constValue">The constant value (the same min and max values).</param>
        public static URandomValueSettings CloneOrCreate(URandomValueSettings source, double constValue)
        {
            return CloneOrCreate(source, constValue, constValue);
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Max < Min || Min < 0 || Max < 0)
            {
                throw new ArgumentException($"Incorrect min ({Min.ToString(CultureInfo.InvariantCulture)}) and/or max ({Max.ToString(CultureInfo.InvariantCulture)}) values. Max must be GE to min and both values must be GE 0.", "Max/Min");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new URandomValueSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("min", Min.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("max", Max.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || DistrType != RandomCommon.DistributionType.Uniform)
            {
                rootElem.Add(((RCNetBaseSettings)DistrCfg).GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

    }//URandomValueSettings

}//Namespace
