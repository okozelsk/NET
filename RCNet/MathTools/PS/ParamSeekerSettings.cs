using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.MathTools.PS
{
    /// <summary>
    /// Settings of the parameter seeker
    /// </summary>
    [Serializable]
    public class ParamSeekerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ParamSeekerCfgType";

        //Attribute properties
        /// <summary>
        /// Min value of the parameter to be considered
        /// </summary>
        public double Min { get; set; }
        /// <summary>
        /// Max value of the parameter to be considered
        /// </summary>
        public double Max { get; set; }
        /// <summary>
        /// Number of steps dividing searched interval
        /// </summary>
        public int NumOfSteps { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="min">Min value of the parameter to be considered</param>
        /// <param name="max">Max value of the parameter to be considered</param>
        /// <param name="numOfSteps">Number of steps dividing the searched interval</param>
        public ParamSeekerSettings(double min, double max, int numOfSteps)
        {
            Min = min;
            Max = max;
            NumOfSteps = numOfSteps;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ParamSeekerSettings(ParamSeekerSettings source)
        {
            Min = source.Min;
            Max = source.Max;
            NumOfSteps = source.NumOfSteps;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing the settings</param>
        public ParamSeekerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            Min = double.Parse(settingsElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(settingsElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            NumOfSteps = int.Parse(settingsElem.Attribute("steps").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public ParamSeekerSettings DeepClone()
        {
            return new ParamSeekerSettings(this);
        }

    }//ParamSeekerSettings

}//Namespace
