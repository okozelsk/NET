using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;

namespace RCNet.Neural.Network.NonRecurrent.PP
{
    /// <summary>
    /// Startup parameters for the parallel perceptron p-delta rule trainer
    /// </summary>
    [Serializable]
    public class PDeltaRuleTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PPNetPDeltaRuleTrainerCfgType";

        /// <summary>
        /// Default initial learning rate
        /// </summary>
        public const double DefaultIniLR = 0.01d;
        /// <summary>
        /// Default learning rate increase
        /// </summary>
        public const double DefaultIncLR = 1.1d;
        /// <summary>
        /// Default learning rate decrease
        /// </summary>
        public const double DefaultDecLR = 0.5d;
        /// <summary>
        /// Default learning min rate
        /// </summary>
        public const double DefaultMinLR = 1E-4;
        /// <summary>
        /// Default learning max rate
        /// </summary>
        public const double DefaultMaxLR = 0.1d;

        //Attribute properties
        /// <summary>
        /// Number of attempts
        /// </summary>
        public int NumOfAttempts { get; }
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// Initial learning rate
        /// </summary>
        public double IniLR { get; }
        /// <summary>
        /// Learning rate increase
        /// </summary>
        public double IncLR { get; }
        /// <summary>
        /// Learning rate decrease
        /// </summary>
        public double DecLR { get; }
        /// <summary>
        /// Learning rate minimum
        /// </summary>
        public double MinLR { get; }
        /// <summary>
        /// Learning rate maximum
        /// </summary>
        public double MaxLR { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttempts">Number of attempts</param>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="iniLR">Initial learning rate</param>
        /// <param name="incLR">Learning rate increase</param>
        /// <param name="decLR">Learning rate decrease</param>
        /// <param name="minLR">Learning rate minimum</param>
        /// <param name="maxLR">Learning rate maximum</param>
        public PDeltaRuleTrainerSettings(int numOfAttempts,
                                         int numOfAttemptEpochs,
                                         double iniLR = DefaultIniLR,
                                         double incLR = DefaultIncLR,
                                         double decLR = DefaultDecLR,
                                         double minLR = DefaultMinLR,
                                         double maxLR = DefaultMaxLR
                                         )
        {
            NumOfAttempts = numOfAttempts;
            NumOfAttemptEpochs = numOfAttemptEpochs;
            IniLR = iniLR;
            IncLR = incLR;
            DecLR = decLR;
            MinLR = minLR;
            MaxLR = maxLR;
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PDeltaRuleTrainerSettings(PDeltaRuleTrainerSettings source)
        {
            NumOfAttempts = source.NumOfAttempts;
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            IniLR = source.IniLR;
            IncLR = source.IncLR;
            DecLR = source.DecLR;
            MinLR = source.MinLR;
            MaxLR = source.MaxLR;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
        public PDeltaRuleTrainerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfAttempts = int.Parse(settingsElem.Attribute("attempts").Value, CultureInfo.InvariantCulture);
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            IniLR = double.Parse(settingsElem.Attribute("iniLR").Value, CultureInfo.InvariantCulture);
            IncLR = double.Parse(settingsElem.Attribute("incLR").Value, CultureInfo.InvariantCulture);
            DecLR = double.Parse(settingsElem.Attribute("decLR").Value, CultureInfo.InvariantCulture);
            MinLR = double.Parse(settingsElem.Attribute("minLR").Value, CultureInfo.InvariantCulture);
            MaxLR = double.Parse(settingsElem.Attribute("maxLR").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultIniLR { get { return (IniLR == DefaultIniLR); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultIncLR { get { return (IncLR == DefaultIncLR); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultDecLR { get { return (DecLR == DefaultDecLR); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultMinLR { get { return (MinLR == DefaultMinLR); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultMaxLR { get { return (MaxLR == DefaultMaxLR); } }

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
            if (NumOfAttempts < 1)
            {
                throw new Exception($"Invalid NumOfAttempts {NumOfAttempts.ToString(CultureInfo.InvariantCulture)}. NumOfAttempts must be GE to 1.");
            }
            if (NumOfAttemptEpochs < 1)
            {
                throw new Exception($"Invalid NumOfAttemptEpochs {NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)}. NumOfAttemptEpochs must be GE to 1.");
            }
            if (IniLR < 0 || IniLR >= 1)
            {
                throw new Exception($"Invalid IniLR {IniLR.ToString(CultureInfo.InvariantCulture)}. IniLR must be GE to 0 and LT 1.");
            }
            if (IncLR <= 1)
            {
                throw new Exception($"Invalid IncLR {IncLR.ToString(CultureInfo.InvariantCulture)}. IncLR must be GT 1.");
            }
            if (DecLR < 0 || DecLR >= 1)
            {
                throw new Exception($"Invalid DecLR {DecLR.ToString(CultureInfo.InvariantCulture)}. DecLR must be GE to 0 and LT 1.");
            }
            if (MinLR < 0 || MinLR >= 1)
            {
                throw new Exception($"Invalid MinLR {MinLR.ToString(CultureInfo.InvariantCulture)}. MinLR must be GE to 0 and LT 1.");
            }
            if (MaxLR < 0 || MaxLR >= 1)
            {
                throw new Exception($"Invalid MaxLR {MaxLR.ToString(CultureInfo.InvariantCulture)}. MaxLR must be GE to 0 and LT 1.");
            }
            if (MaxLR <= MinLR)
            {
                throw new Exception($"Invalid MinLR or MaxLR. MaxLR must be GT MinLR.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new PDeltaRuleTrainerSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("attempts", NumOfAttempts.ToString(CultureInfo.InvariantCulture)),
                                                           new XAttribute("attemptEpochs", NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || !IsDefaultIniLR)
            {
                rootElem.Add(new XAttribute("iniLR", IniLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultIncLR)
            {
                rootElem.Add(new XAttribute("incLR", IncLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultDecLR)
            {
                rootElem.Add(new XAttribute("decLR", DecLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMinLR)
            {
                rootElem.Add(new XAttribute("minLR", MinLR.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMaxLR)
            {
                rootElem.Add(new XAttribute("maxLR", MaxLR.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("pDeltaRuleTrainer", suppressDefaults);
        }


    }//PDeltaRuleTrainerSettings

}//Namespace
