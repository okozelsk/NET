using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.MathTools
{
    /// <summary>
    /// Configuration of the ParamValFinder.
    /// </summary>
    [Serializable]
    public class ParamValFinderSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The The name of the associated xsd type..
        /// </summary>
        public const string XsdTypeName = "ParamValFinderType";
        /// <summary>
        /// String code for setting the automatic number of sub-intervals.
        /// </summary>
        public const string AutoSubIntervalsCode = "Auto";
        /// <summary>
        /// Number code for setting the automatic number of sub-intervals.
        /// </summary>
        public const int AutoSubIntervalsNum = -1;
        /// <summary>
        /// The default value of the parameter specifying the number of sub-intervals.
        /// </summary>
        public const int DefaultNumOfSubIntervals = AutoSubIntervalsNum;

        //Attribute properties
        /// <summary>
        /// The min value of the parameter.
        /// </summary>
        public double Min { get; }
        /// <summary>
        /// The max value of the parameter.
        /// </summary>
        public double Max { get; }
        /// <summary>
        /// The number of sub-intervals of the currently focused interval.
        /// </summary>
        public int NumOfSubIntervals { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="min">The min value of the parameter.</param>
        /// <param name="max">The max value of the parameter.</param>
        /// <param name="numOfSubIntervals">The number of sub-intervals of the currently focused interval.</param>
        public ParamValFinderSettings(double min, double max, int numOfSubIntervals = DefaultNumOfSubIntervals)
        {
            Min = min;
            Max = max;
            NumOfSubIntervals = numOfSubIntervals;
            Check();
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ParamValFinderSettings(ParamValFinderSettings source)
            : this(source.Min, source.Max, source.NumOfSubIntervals)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ParamValFinderSettings(XElement elem)
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
        /// Checks the defaults..
        /// </summary>
        public bool IsDefaultNumOfSubIntervals { get { return (NumOfSubIntervals == DefaultNumOfSubIntervals); } }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (Max < Min || Min < 0 || Max < 0)
            {
                throw new ArgumentException($"Incorrect min ({Min.ToString(CultureInfo.InvariantCulture)}) and/or max ({Max.ToString(CultureInfo.InvariantCulture)}) values. Max must be GE to min and both values must be GE 0.", "Min/Max");
            }
            if (NumOfSubIntervals != AutoSubIntervalsNum)
            {
                if (NumOfSubIntervals < 1)
                {
                    throw new ArgumentException($"Incorrect NumOfSubIntervals ({NumOfSubIntervals.ToString(CultureInfo.InvariantCulture)}). Value must be GE to 1.", "NumOfSubIntervals");
                }
                if (Min == Max && NumOfSubIntervals != 1)
                {
                    throw new ArgumentException($"Incorrect NumOfSubIntervals ({NumOfSubIntervals.ToString(CultureInfo.InvariantCulture)}). Value must be 1 when Min=Max.", "NumOfSubIntervals");
                }
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new ParamValFinderSettings(this);
        }

        /// <inheritdoc/>
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

    }//ParamValFinderSettings

}//Namespace
