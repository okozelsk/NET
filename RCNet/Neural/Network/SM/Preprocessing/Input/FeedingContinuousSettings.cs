using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Extensions;

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
        /// <summary>
        /// Default value of parameter specifying if to route input fields to readout layer together with other predictors 
        /// </summary>
        public const bool DefaultRouteToReadout = false;

        //Attribute properties
        /// <summary>
        /// Number of boot cycles
        /// </summary>
        public int BootCycles { get; }

        /// <summary>
        /// Specifies if to route input fields to readout layer together with other predictors
        /// </summary>
        public bool RouteToReadout { get; }


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="bootCycles">Number of boot cycles</param>
        /// <param name="routeToReadout">Specifies if to route input fields to readout layer together with other predictors</param>
        public FeedingContinuousSettings(int bootCycles = DefaultBootCycles,
                                              bool routeToReadout = DefaultRouteToReadout
                                              )
        {
            BootCycles = bootCycles;
            RouteToReadout = routeToReadout;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public FeedingContinuousSettings(FeedingContinuousSettings source)
            : this(source.BootCycles, source.RouteToReadout)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public FeedingContinuousSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            string bootCycles = settingsElem.Attribute("bootCycles").Value;
            BootCycles = bootCycles == AutoBootCyclesCode ? AutoBootCyclesNum : int.Parse(bootCycles, CultureInfo.InvariantCulture);
            RouteToReadout = bool.Parse(settingsElem.Attribute("routeToReadout").Value);
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
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultRouteToReadout { get { return (RouteToReadout == DefaultRouteToReadout); } }

        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return IsDefaultBootCycles && IsDefaultRouteToReadout; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (BootCycles != AutoBootCyclesNum && BootCycles <= 0)
            {
                throw new Exception($"Invalid BootCycles {BootCycles.ToString(CultureInfo.InvariantCulture)}. BootCycles must be equal to {AutoBootCyclesNum.ToString(CultureInfo.InvariantCulture)} for automatic boot cycles or GT 0.");
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
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            if (!suppressDefaults || !IsDefaultBootCycles)
            {
                rootElem.Add(new XAttribute("bootCycles", BootCycles == AutoBootCyclesNum ? AutoBootCyclesCode : BootCycles.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultRouteToReadout)
            {
                rootElem.Add(new XAttribute("routeToReadout", RouteToReadout.ToString(CultureInfo.InvariantCulture).ToLowerInvariant()));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("feedingContinuous", suppressDefaults);
        }

    }//FeedingContinuousSettings

}//Namespace

