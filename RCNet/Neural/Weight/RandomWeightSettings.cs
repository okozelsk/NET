using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml.Linq;
using System.Reflection;
using RCNet.Extensions;

namespace RCNet.Neural.Weight
{
    /// <summary>
    /// Class specifies properties of randomly generated weight
    /// </summary>
    [Serializable]
    public class RandomWeightSettings
    {
        //Attribute properties
        /// <summary>
        /// Weight min value
        /// </summary>
        public double Min { get; set; }
        /// <summary>
        /// Weight max value
        /// </summary>
        public double Max { get; set; }
        /// <summary>
        /// Specifies whether to randomize weight value sign
        /// </summary>
        public bool RandomSign { get; set; }
        /// <summary>
        /// Specifies what distribution to use
        /// </summary>
        public RandomClassExtensions.DistributionType DistrType { get; set; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="min">Weight min value</param>
        /// <param name="max">Weight max value</param>
        /// <param name="randomSign">Specifies whether to randomize weight value sign</param>
        /// <param name="distrType">Specifies what distribution to use</param>
        public RandomWeightSettings(double min = -1,
                                    double max = 1,
                                    bool randomSign = false,
                                    RandomClassExtensions.DistributionType distrType = RandomClassExtensions.DistributionType.Uniform
                                    )
        {
            Min = min;
            Max = max;
            RandomSign = randomSign;
            DistrType = distrType;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RandomWeightSettings(RandomWeightSettings source)
        {
            Min = source.Min;
            Max = source.Max;
            RandomSign = source.RandomSign;
            DistrType = source.DistrType;
            return;
        }

        /// <summary>
        /// Creates an instance and initializes it from given xml element.
        /// </summary>
        /// <param name="weightSettingsElem">
        /// Xml data containing RandomWeightSettings settings.
        /// </param>
        public RandomWeightSettings(XElement weightSettingsElem)
        {
            //Parsing
            Min = double.Parse(weightSettingsElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(weightSettingsElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            RandomSign = bool.Parse(weightSettingsElem.Attribute("randomSign").Value);
            DistrType = RandomClassExtensions.ParseDistributionType(weightSettingsElem.Attribute("distribution").Value);
            return;
        }

        //Properties
        /// <summary>
        /// Determines whether this settings produce nonzero weights
        /// </summary>
        public bool Active { get { return (Min != 0 || Max != 0); } }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RandomWeightSettings cmpSettings = obj as RandomWeightSettings;
            if (Min != cmpSettings.Min ||
                Max != cmpSettings.Max ||
                RandomSign != cmpSettings.RandomSign ||
                DistrType != cmpSettings.DistrType
                )
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public RandomWeightSettings DeepClone()
        {
            RandomWeightSettings clone = new RandomWeightSettings(this);
            return clone;
        }

    }//RandomWeightSettings

}//Namespace
