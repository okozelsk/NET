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
        public const string XsdTypeName = "ParamSeekerType";
        /// <summary>
        /// Automatic number of sub-intervals (code)
        /// </summary>
        public const string AutoSubIntervalsCode = "Auto";
        /// <summary>
        /// Automatic number of sub-intervals (num)
        /// </summary>
        public const int AutoSubIntervalsNum = -1;
        /// <summary>
        /// Default value of parameter specifying number of sub-intervals
        /// </summary>
        public const int DefaultNumOfSubIntervals = AutoSubIntervalsNum;

        //Attribute properties
        /// <summary>
        /// Min value of the parameter to be considered
        /// </summary>
        public double Min { get; }
        /// <summary>
        /// Max value of the parameter to be considered
        /// </summary>
        public double Max { get; }
        /// <summary>
        /// Number of subintervals to use to process currently focused interval
        /// </summary>
        public int NumOfSubIntervals { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="min">Min value of the parameter to be considered</param>
        /// <param name="max">Max value of the parameter to be considered</param>
        /// <param name="numOfSubIntervals">Number of subintervals to use to process currently focused interval</param>
        public ParamSeekerSettings(double min, double max, int numOfSubIntervals = DefaultNumOfSubIntervals)
        {
            Min = min;
            Max = max;
            NumOfSubIntervals = numOfSubIntervals;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ParamSeekerSettings(ParamSeekerSettings source)
            :this(source.Min, source.Max, source.NumOfSubIntervals)
        {
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
            string subIntervals = settingsElem.Attribute("subIntervals").Value;
            NumOfSubIntervals = subIntervals == AutoSubIntervalsCode ? AutoSubIntervalsNum : int.Parse(subIntervals, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultNumOfSubIntervals { get { return (NumOfSubIntervals == DefaultNumOfSubIntervals); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (Max < Min || Min < 0 || Max < 0)
            {
                throw new Exception($"Incorrect min ({Min.ToString(CultureInfo.InvariantCulture)}) and/or max ({Max.ToString(CultureInfo.InvariantCulture)}) values. Max must be GE to min and both values must be GE 0.");
            }
            if (NumOfSubIntervals != AutoSubIntervalsNum)
            {
                if (NumOfSubIntervals < 1)
                {
                    throw new Exception($"Incorrect numOfSteps ({NumOfSubIntervals.ToString(CultureInfo.InvariantCulture)}). Value must be GE to 1.");
                }
                if (Min == Max && NumOfSubIntervals != 1)
                {
                    throw new Exception($"Incorrect numOfSteps ({NumOfSubIntervals.ToString(CultureInfo.InvariantCulture)}). Value must be 1 when Min=Max.");
                }
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ParamSeekerSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("min", Min.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("max", Max.ToString(CultureInfo.InvariantCulture))
                                             );
            if (!suppressDefaults || !IsDefaultNumOfSubIntervals)
            {
                rootElem.Add(new XAttribute("subIntervals", NumOfSubIntervals == AutoSubIntervalsNum ? AutoSubIntervalsCode : NumOfSubIntervals.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

    }//ParamSeekerSettings

}//Namespace
