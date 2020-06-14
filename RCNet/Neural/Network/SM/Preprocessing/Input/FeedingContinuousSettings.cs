using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Settings of continuous input feeding regime
    /// </summary>
    [Serializable]
    public class FeedingContinuousSettings : RCNetBaseSettings, IFeedingSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPInpFeedingContinuousType";
        /// <summary>
        /// Automatic boot cycles (code)
        /// </summary>
        public const string AutoBootCyclesCode = "Auto";
        /// <summary>
        /// Automatic boot cycles (num)
        /// </summary>
        public const int AutoBootCyclesNum = -1;
        /// <summary>
        /// Default value of parameter specifying number of boot-cycles
        /// </summary>
        public const int DefaultBootCycles = AutoBootCyclesNum;

        //Attribute properties
        /// <summary>
        /// Number of boot cycles
        /// </summary>
        public int BootCycles { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="bootCycles">Number of boot cycles</param>
        public FeedingContinuousSettings(int bootCycles = DefaultBootCycles)
        {
            BootCycles = bootCycles;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedingContinuousSettings(FeedingContinuousSettings source)
            : this(source.BootCycles)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public FeedingContinuousSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            string bootCycles = settingsElem.Attribute("bootCycles").Value;
            BootCycles = bootCycles == AutoBootCyclesCode ? AutoBootCyclesNum : int.Parse(bootCycles, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Type of input feeding
        /// </summary>
        public InputEncoder.InputFeedingType FeedingType { get { return InputEncoder.InputFeedingType.Continuous; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultBootCycles { get { return (BootCycles == DefaultBootCycles); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultBootCycles; } }

        //Methods
        /// <summary>
        /// Checks consistency
        /// </summary>
        protected override void Check()
        {
            if (BootCycles != AutoBootCyclesNum && BootCycles <= 0)
            {
                throw new ArgumentException($"Invalid BootCycles {BootCycles.ToString(CultureInfo.InvariantCulture)}. BootCycles must be equal to {AutoBootCyclesNum.ToString(CultureInfo.InvariantCulture)} for automatic boot cycles or GT 0.", "BootCycles");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new FeedingContinuousSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultBootCycles)
            {
                rootElem.Add(new XAttribute("bootCycles", BootCycles == AutoBootCyclesNum ? AutoBootCyclesCode : BootCycles.ToString(CultureInfo.InvariantCulture)));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies whether to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("feedingContinuous", suppressDefaults);
        }

    }//FeedingContinuousSettings

}//Namespace

