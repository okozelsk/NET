using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Configuration of the continuous input feeding regime.
    /// </summary>
    [Serializable]
    public class FeedingContinuousSettings : RCNetBaseSettings, IFeedingSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "NPInpFeedingContinuousType";
        /// <summary>
        /// The string code for the automatic boot cycles.
        /// </summary>
        public const string AutoBootCyclesCode = "Auto";
        /// <summary>
        /// The numeric code for the automatic boot cycles.
        /// </summary>
        public const int AutoBootCyclesNum = -1;
        /// <summary>
        /// The default value of the parameter specifying the number of the boot cycles.
        /// </summary>
        public const int DefaultBootCycles = AutoBootCyclesNum;

        //Attribute properties
        /// <summary>
        /// Specifies the number of the boot cycles.
        /// </summary>
        public int BootCycles { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="bootCycles">Specifies the number of the boot cycles.</param>
        public FeedingContinuousSettings(int bootCycles = DefaultBootCycles)
        {
            BootCycles = bootCycles;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public FeedingContinuousSettings(FeedingContinuousSettings source)
            : this(source.BootCycles)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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
        /// <inheritdoc/>
        public InputEncoder.InputFeedingType FeedingType { get { return InputEncoder.InputFeedingType.Continuous; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultBootCycles { get { return (BootCycles == DefaultBootCycles); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return IsDefaultBootCycles; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (BootCycles != AutoBootCyclesNum && BootCycles <= 0)
            {
                throw new ArgumentException($"Invalid BootCycles {BootCycles.ToString(CultureInfo.InvariantCulture)}. BootCycles must be equal to {AutoBootCyclesNum.ToString(CultureInfo.InvariantCulture)} for automatic boot cycles or GT 0.", "BootCycles");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new FeedingContinuousSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("feedingContinuous", suppressDefaults);
        }

    }//FeedingContinuousSettings

}//Namespace

