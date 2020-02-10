using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;
using RCNet.XmlTools;

namespace RCNet.RandomValue
{
    /// <summary>
    /// Class specifies properties of randomly generated values
    /// </summary>
    [Serializable]
    public class RandomValueSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "RandomValueType";

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
        /// Specifies whether to randomize value sign
        /// </summary>
        public bool RandomSign { get; }
        
        /// <summary>
        /// Specifies what distribution to use
        /// </summary>
        public RandomExtensions.DistributionType DistrType { get; }

        /// <summary>
        /// Distribution parameters
        /// </summary>
        public IDistrSettings DistrCfg { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="min">Min random value</param>
        /// <param name="max">Max random value</param>
        /// <param name="randomSign">Specifies whether to randomize value sign</param>
        /// <param name="distrCfg">Specific parameters of the distribution</param>
        public RandomValueSettings(double min = -1,
                                   double max = 1,
                                   bool randomSign = false,
                                   IDistrSettings distrCfg = null
                                   )
        {
            Min = min;
            Max = max;
            RandomSign = randomSign;
            DistrCfg = distrCfg;
            if(DistrCfg == null)
            {
                DistrType = RandomExtensions.DistributionType.Uniform;
            }
            else
            {
                Type dcType = DistrCfg.GetType();
                if(dcType == typeof(GaussianDistrSettings))
                {
                    DistrType = RandomExtensions.DistributionType.Gaussian;
                }
                else if(dcType == typeof(ExponentialDistrSettings))
                {
                    DistrType = RandomExtensions.DistributionType.Exponential;
                }
                else if (dcType == typeof(GammaDistrSettings))
                {
                    DistrType = RandomExtensions.DistributionType.Gamma;
                }
                else
                {
                    throw new Exception($"Unexpected distribution configuration");
                }
            }
            Check();
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RandomValueSettings(RandomValueSettings source)
        {
            Min = source.Min;
            Max = source.Max;
            RandomSign = source.RandomSign;
            DistrType = source.DistrType;
            DistrCfg = source.DistrCfg?.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing RandomValueSettings settings.</param>
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
                DistrType = RandomExtensions.DistributionType.Uniform;
                DistrCfg = new UniformDistrSettings();
            }
            else
            {
                switch (distrParamsElem.Name.LocalName)
                {
                    case "uniformDistr":
                        DistrType = RandomExtensions.DistributionType.Uniform;
                        DistrCfg = new UniformDistrSettings(distrParamsElem);
                        break;
                    case "gaussianDistr":
                        DistrType = RandomExtensions.DistributionType.Gaussian;
                        DistrCfg = new GaussianDistrSettings(distrParamsElem);
                        break;
                    case "exponentialDistr":
                        DistrType = RandomExtensions.DistributionType.Exponential;
                        DistrCfg = new ExponentialDistrSettings(distrParamsElem);
                        break;
                    case "gammaDistr":
                        DistrType = RandomExtensions.DistributionType.Gamma;
                        DistrCfg = new GammaDistrSettings(distrParamsElem);
                        break;
                    default:
                        throw new Exception($"Unexpected element {distrParamsElem.Name.LocalName}");
                }
            }
            Check();
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// If exists descendant element within the root element then function creates instance of the RandomValueSettings using
        /// descendant's xml settings. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings LoadOrDefault(XElement rootElem, string descendant, double defaultMin, double defaultMax, bool randomSign = false)
        {
            XElement descendantElement = rootElem.Descendants(descendant).FirstOrDefault();
            if (descendantElement != null)
            {
                return new RandomValueSettings(descendantElement);
            }
            else
            {
                return new RandomValueSettings(defaultMin, defaultMax, randomSign);
            }
        }

        /// <summary>
        /// If exists descendant element within the root element then function creates instance of the RandomValueSettings using
        /// descendant's xml settings. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings LoadOrDefault(XElement rootElem, string descendant, double defaultConst, bool randomSign = false)
        {
            return LoadOrDefault(rootElem, descendant, defaultConst, defaultConst, randomSign);
        }

        /// <summary>
        /// If source is not null then function creates it's clone. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings CloneOrDefault(RandomValueSettings source, double defaultMin, double defaultMax, bool randomSign = false)
        {
            if(source == null)
            {
                return new RandomValueSettings(defaultMin, defaultMax, randomSign);
            }
            else
            {
                return source.DeepClone();
            }
        }

        /// <summary>
        /// If source is not null then function creates it's clone. If not, function creates instance of the RandomValueSettings using specified default parameters.
        /// </summary>
        public static RandomValueSettings CloneOrDefault(RandomValueSettings source, double defaultConst, bool randomSign = false)
        {
            return CloneOrDefault(source, defaultConst, defaultConst, randomSign);
        }

        //Instance methods
        private void Check()
        {
            if(Max < Min)
            {
                throw new Exception($"Incorrect min ({Min.ToString(CultureInfo.InvariantCulture)}) and max ({Max.ToString(CultureInfo.InvariantCulture)}) values. Max must be GE to min.");
            }
            return;
        }
        
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public RandomValueSettings DeepClone()
        {
            return new RandomValueSettings(this);
        }

        //Inner classes
        /// <summary>
        /// Uniform distribution parameters
        /// </summary>
        [Serializable]
        public class UniformDistrSettings : IDistrSettings
        {
            //Attributes

            //Constructors
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            public UniformDistrSettings()
            {
                Check();
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public UniformDistrSettings(UniformDistrSettings source)
            {
                return;
            }

            /// <summary>
            /// Creates an instance and initializes it from given xml element.
            /// </summary>
            /// <param name="elem"> Xml data containing settings.</param>
            public UniformDistrSettings(XElement elem)
            {
                //Parsing
                //Nothing to do
                Check();
                return;
            }

            //Methods
            private void Check()
            {
                return;
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public IDistrSettings DeepClone()
            {
                return new UniformDistrSettings(this);
            }

        }//UniformDistrSettings

        /// <summary>
        /// Gaussian distribution parameters
        /// </summary>
        [Serializable]
        public class GaussianDistrSettings : IDistrSettings
        {
            //Attributes
            /// <summary>
            /// Mean
            /// </summary>
            public double Mean { get; }
            
            /// <summary>
            /// Standard deviation
            /// </summary>
            public double StdDev { get; }

            //Constructors
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="mean">Mean</param>
            /// <param name="stdDev">Standard deviation</param>
            public GaussianDistrSettings(double mean = 0, double stdDev = 1)
            {
                Mean = mean;
                StdDev = stdDev;
                Check();
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public GaussianDistrSettings(GaussianDistrSettings source)
            {
                Mean = source.Mean;
                StdDev = source.StdDev;
                return;
            }

            /// <summary>
            /// Creates an instance and initializes it from given xml element.
            /// </summary>
            /// <param name="elem"> Xml data containing settings.</param>
            public GaussianDistrSettings(XElement elem)
            {
                //Parsing
                Mean = double.Parse(elem.Attribute("mean").Value, CultureInfo.InvariantCulture);
                StdDev = double.Parse(elem.Attribute("stdDev").Value, CultureInfo.InvariantCulture);
                Check();
                return;
            }

            //Methods
            private void Check()
            {
                if (StdDev <= 0)
                {
                    throw new Exception($"Incorrect StdDev ({StdDev.ToString(CultureInfo.InvariantCulture)}) value. StdDev must be GT 0.");
                }
                return;
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public IDistrSettings DeepClone()
            {
                return new GaussianDistrSettings(this);
            }

        }//GaussianDistrSettings

        /// <summary>
        /// Exponential distribution parameters
        /// </summary>
        [Serializable]
        public class ExponentialDistrSettings : IDistrSettings
        {
            //Attributes
            /// <summary>
            /// Mean
            /// </summary>
            public double Mean { get; }

            //Constructors
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="mean">Mean</param>
            public ExponentialDistrSettings(double mean)
            {
                Mean = mean;
                Check();
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public ExponentialDistrSettings(ExponentialDistrSettings source)
            {
                Mean = source.Mean;
                return;
            }

            /// <summary>
            /// Creates an instance and initializes it from given xml element.
            /// </summary>
            /// <param name="elem"> Xml data containing settings.</param>
            public ExponentialDistrSettings(XElement elem)
            {
                //Parsing
                Mean = double.Parse(elem.Attribute("mean").Value, CultureInfo.InvariantCulture);
                Check();
                return;
            }

            //Methods
            private void Check()
            {
                if (Mean == 0)
                {
                    throw new Exception($"Incorrect Mean ({Mean.ToString(CultureInfo.InvariantCulture)}) value. Mean must not be EQ to 0.");
                }
                return;
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public IDistrSettings DeepClone()
            {
                return new ExponentialDistrSettings(this);
            }

        }//ExponentialDistrSettings

        /// <summary>
        /// Gamma distribution parameters
        /// </summary>
        [Serializable]
        public class GammaDistrSettings : IDistrSettings
        {
            //Attributes
            /// <summary>
            /// Alpha, the shape parameter
            /// </summary>
            public double Alpha { get; }

            /// <summary>
            /// Beta, the rate parameter
            /// </summary>
            public double Beta { get; }

            //Constructors
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="alpha">Shape parameter (alpha)</param>
            /// <param name="beta">Rate parameter (beta)</param>
            public GammaDistrSettings(double alpha, double beta)
            {
                Alpha = alpha;
                Beta = beta;
                Check();
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public GammaDistrSettings(GammaDistrSettings source)
            {
                Alpha = source.Alpha;
                Beta = source.Beta;
                return;
            }

            /// <summary>
            /// Creates an instance and initializes it from given xml element.
            /// </summary>
            /// <param name="elem"> Xml data containing settings.</param>
            public GammaDistrSettings(XElement elem)
            {
                //Parsing
                Alpha = double.Parse(elem.Attribute("alpha").Value, CultureInfo.InvariantCulture);
                Beta = double.Parse(elem.Attribute("beta").Value, CultureInfo.InvariantCulture);
                Check();
                return;
            }

            //Methods
            private void Check()
            {
                if (Alpha <= 0)
                {
                    throw new Exception($"Incorrect Alpha ({Alpha.ToString(CultureInfo.InvariantCulture)}) value. Alpha must be GT 0.");
                }
                if (Beta <= 0)
                {
                    throw new Exception($"Incorrect Beta ({Beta.ToString(CultureInfo.InvariantCulture)}) value. Beta must be GT 0.");
                }
                return;
            }

            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public IDistrSettings DeepClone()
            {
                return new GammaDistrSettings(this);
            }

        }//GammaDistrSettings

    }//RandomValueSettings

}//Namespace
