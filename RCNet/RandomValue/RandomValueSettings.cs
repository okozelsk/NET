using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the random value.
    /// </summary>
    [Serializable]
    public class RandomValueSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "RandomValueType";

        //Default values
        /// <summary>
        /// The default value of the parameter specifying whether to randomize the value sign.
        /// </summary>
        public const bool DefaultRandomSign = false;
        /// <summary>
        /// The default type of random distribution.
        /// </summary>
        public const RandomCommon.DistributionType DefaultDistributionType = RandomCommon.DistributionType.Uniform;

        //Attribute properties
        /// <summary>
        /// The min value (inclusive).
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// The max value (exclusive).
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Specifies whether to randomize the value sign.
        /// </summary>
        public bool RandomSign { get; }

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
        /// <param name="randomSign">Specifies whether to randomize the value sign.</param>
        /// <param name="distrCfg">The configuration of the distribution.</param>
        public RandomValueSettings(double min,
                                   double max,
                                   bool randomSign = DefaultRandomSign,
                                   IDistrSettings distrCfg = null
                                   )
        {
            Min = min;
            Max = max;
            RandomSign = randomSign;
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
        public RandomValueSettings(RandomValueSettings source)
        {
            Min = source.Min;
            Max = source.Max;
            RandomSign = source.RandomSign;
            DistrCfg = (IDistrSettings)((RCNetBaseSettings)source.DistrCfg).DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RandomValueSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Min = double.Parse(settingsElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(settingsElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            RandomSign = bool.Parse(settingsElem.Attribute("randomSign").Value);
            XElement distrParamsElem = settingsElem.Elements().FirstOrDefault();
            if (distrParamsElem == null)
            {
                DistrCfg = new UniformDistrSettings();
            }
            else
            {
                DistrCfg = RandomCommon.LoadDistrCfg(distrParamsElem);
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

        /// <summary>
        /// Type of used random distribution
        /// </summary>
        public RandomCommon.DistributionType DistrType { get { return DistrCfg.Type; } }

        //Methods
        //Static methods
        /// <summary>
        /// Loads or creates the configuration of the random value.
        /// </summary>
        /// <remarks>
        /// Checks whether exists the specified descendant element under the root element and if so, loads the configuration.
        /// If the specified descendant element does not exist, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="rootElem">The root xml element.</param>
        /// <param name="descendant">The name of descendant element containing the configuration data.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="randomSign">Specifies whether to randomize the value sign.</param>
        public static RandomValueSettings LoadOrCreate(XElement rootElem, string descendant, double min, double max, bool randomSign = false)
        {
            XElement descendantElement = rootElem.Elements(descendant).FirstOrDefault();
            if (descendantElement != null)
            {
                return new RandomValueSettings(descendantElement);
            }
            else
            {
                return new RandomValueSettings(min, max, randomSign);
            }
        }

        /// <summary>
        /// Loads or creates the configuration of the random value.
        /// </summary>
        /// <remarks>
        /// Checks whether exists the specified descendant element under the root element and if so, loads the configuration.
        /// If the specified descendant element does not exist, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="rootElem">The root xml element.</param>
        /// <param name="descendant">The name of descendant element containing the configuration data.</param>
        /// <param name="constValue">The constant value (the same min and max values).</param>
        /// <param name="randomSign">Specifies whether to randomize the value sign.</param>
        public static RandomValueSettings LoadOrCreate(XElement rootElem, string descendant, double constValue, bool randomSign = false)
        {
            return LoadOrCreate(rootElem, descendant, constValue, constValue, randomSign);
        }

        /// <summary>
        /// Clones the existing configuration or creates the new configuration of the random value.
        /// </summary>
        /// <remarks>
        /// Checks whether the specified source configuration instance is not null and if so, creates its clone.
        /// If the source configuration instance is null, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="source">The source configuration instance.</param>
        /// <param name="min">The min value.</param>
        /// <param name="max">The max value.</param>
        /// <param name="randomSign">Specifies whether to randomize the value sign.</param>
        public static RandomValueSettings CloneOrCreate(RandomValueSettings source, double min, double max, bool randomSign = false)
        {
            if (source == null)
            {
                return new RandomValueSettings(min, max, randomSign);
            }
            else
            {
                return (RandomValueSettings)source.DeepClone();
            }
        }

        /// <summary>
        /// Clones the existing configuration or creates the new configuration of the random value.
        /// </summary>
        /// <remarks>
        /// Checks whether the specified source configuration instance is not null and if so, creates its clone.
        /// If the source configuration instance is null, creates the configuration according to specified parameters.
        /// </remarks>
        /// <param name="source">The source configuration instance.</param>
        /// <param name="constValue">The constant value (the same min and max values).</param>
        /// <param name="randomSign">Specifies whether to randomize the value sign.</param>
        public static RandomValueSettings CloneOrCreate(RandomValueSettings source, double constValue, bool randomSign = false)
        {
            return CloneOrCreate(source, constValue, constValue, randomSign);
        }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Max < Min)
            {
                throw new ArgumentException($"Incorrect Min ({Min.ToString(CultureInfo.InvariantCulture)}) and Max ({Max.ToString(CultureInfo.InvariantCulture)}) values. Max must be GE to Min.", "Max/Min");
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new RandomValueSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("min", Min.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("max", Max.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || RandomSign != DefaultRandomSign)
            {
                rootElem.Add(new XAttribute("randomSign", RandomSign.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            if (!suppressDefaults || DistrType != RandomCommon.DistributionType.Uniform)
            {
                rootElem.Add(((RCNetBaseSettings)DistrCfg).GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

    }//RandomValueSettings

}//Namespace
