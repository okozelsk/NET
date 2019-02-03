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
    public class ParamSeekerSettings
    {
        //Constants
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
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing the settings</param>
        public ParamSeekerSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.MathTools.PS.ParamSeekerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement linRegrTrainerSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            Min = double.Parse(linRegrTrainerSettingsElem.Attribute("min").Value, CultureInfo.InvariantCulture);
            Max = double.Parse(linRegrTrainerSettingsElem.Attribute("max").Value, CultureInfo.InvariantCulture);
            NumOfSteps = int.Parse(linRegrTrainerSettingsElem.Attribute("steps").Value, CultureInfo.InvariantCulture);
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ParamSeekerSettings cmpSettings = obj as ParamSeekerSettings;
            if (Max != cmpSettings.Max ||
                Min != cmpSettings.Min ||
                NumOfSteps != cmpSettings.NumOfSteps
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
        public ParamSeekerSettings DeepClone()
        {
            return new ParamSeekerSettings(this);
        }

    }//ParamSeekerSettings

}//Namespace
