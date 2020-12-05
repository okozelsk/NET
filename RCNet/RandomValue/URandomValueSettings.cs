using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Configuration of the unsigned random value
    /// </summary>
    [Serializable]
    public class URandomValueSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "URandomValueType";

        //Default values
        /// <summary>
        /// Default type of distribution
        /// </summary>
        public const RandomCommon.DistributionType DefaultDistributionType = RandomCommon.DistributionType.Uniform;

        //Attribute properties
        /// <summary>
        /// Min random value
        /// </summary>
        public double Min { get; }

        /// <summary>
        /// Max random value
        /// </summary>
        public double Max { get; }

        /// <summary>
        /// Distribution parameters
        /// </summary>
        public IDistrSettings DistrCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="min">Min random value</param>
        /// <param name="max">Max random value</param>
        /// <param name="distrCfg">Specific parameters of the distribution</param>
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
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public URandomValueSettings(URandomValueSettings source)
        {
            Min = source.Min;
            Max = source.Max;
            DistrCfg = (IDistrSettings)((RCNetBaseSettings)source.DistrCfg).DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing RandomValueSettings settings.</param>
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
                DistrCfg = RandomCommon.CreateUDistrSettings(distrParamsElem);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultDistrType { get { return DistrType == RandomCommon.DistributionType.Uniform; } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        /// <inheritdoc />
        public RandomCommon.DistributionType DistrType { get { return DistrCfg.Type; } }

        //Methods
        //Static methods
        /// <summary>
        /// If exists descendant element within the root element then function creates instance of the RandomValueSettings using
        /// descendant's xml settings. If not, function creates an instance of the URandomValueSettings using specified default parameters.
        /// </summary>
        public static URandomValueSettings LoadOrDefault(XElement rootElem, string descendant, double defaultMin, double defaultMax)
        {
            XElement descendantElement = rootElem.Elements(descendant).FirstOrDefault();
            if (descendantElement != null)
            {
                return new URandomValueSettings(descendantElement);
            }
            else
            {
                return new URandomValueSettings(defaultMin, defaultMax);
            }
        }

        /// <summary>
        /// If exists descendant element within the root element then function creates instance of the URandomValueSettings using
        /// descendant's xml settings. If not, function creates an instance of the URandomValueSettings using specified default parameters.
        /// </summary>
        public static URandomValueSettings LoadOrDefault(XElement rootElem, string descendant, double defaultConst)
        {
            return LoadOrDefault(rootElem, descendant, defaultConst, defaultConst);
        }

        /// <summary>
        /// If source is not null then function creates it's clone. If not, function creates instance of the URandomValueSettings using specified default parameters.
        /// </summary>
        public static URandomValueSettings CloneOrDefault(URandomValueSettings source, double defaultMin, double defaultMax)
        {
            if (source == null)
            {
                return new URandomValueSettings(defaultMin, defaultMax);
            }
            else
            {
                return (URandomValueSettings)source.DeepClone();
            }
        }

        /// <summary>
        /// If source is not null then function creates it's clone. If not, function creates instance of the URandomValueSettings using specified default parameters.
        /// </summary>
        public static URandomValueSettings CloneOrDefault(URandomValueSettings source, double defaultConst)
        {
            return CloneOrDefault(source, defaultConst, defaultConst);
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
