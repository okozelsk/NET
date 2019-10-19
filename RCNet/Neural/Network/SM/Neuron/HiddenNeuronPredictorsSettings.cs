using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;
using RCNet.Neural.Network.SM.Synapse;

namespace RCNet.Neural.Network.SM.Neuron
{
    /// <summary>
    /// Configuration of the availble predictors
    /// </summary>
    [Serializable]
    public class HiddenNeuronPredictorsSettings
    {
        //Attribute properties
        //Configuration
        /// <summary>
        /// Parameters of the predictors
        /// </summary>
        public Settings Params { get; private set; }
        //Permits
        /// <summary>
        /// Current activation state
        /// </summary>
        public bool Activation { get; private set; }
        /// <summary>
        /// Squared current activation state
        /// </summary>
        public bool SquaredActivation { get; private set; }
        /// <summary>
        /// Exponentially weighted average firing rate within the last N cycles window
        /// </summary>
        public bool FiringExpWRate { get; private set; }
        /// <summary>
        /// Fading number of firings
        /// </summary>
        public bool FiringFadingSum { get; private set; }
        /// <summary>
        /// Number of firings within the last N cycles window
        /// </summary>
        public bool FiringCount { get; private set; }
        /// <summary>
        /// Binary (0/1) firings within the last N cycles window as an unsigned integer number
        /// </summary>
        public bool FiringBinPattern { get; private set; }
        /// <summary>
        /// Number of enabled predictors
        /// </summary>
        public int NumOfEnabledPredictors { get; private set; }

        //Constructors
        /// <summary>
        /// Creates initialized instance as a result of neuron group, pool and reservoir instance predictors settings
        /// </summary>
        /// <param name="groupPredictorsSettings">Neuron group predictors settings</param>
        /// <param name="poolPredictorsSettings">Pool predictors settings</param>
        /// <param name="reservoirPredictorsSettings">Reservoir predictors settings</param>
        public HiddenNeuronPredictorsSettings(HiddenNeuronPredictorsSettings groupPredictorsSettings,
                                              HiddenNeuronPredictorsSettings poolPredictorsSettings,
                                              HiddenNeuronPredictorsSettings reservoirPredictorsSettings
                                              )
        {
            //Params
            Params = groupPredictorsSettings?.Params != null ? groupPredictorsSettings.Params.DeepClone() : (poolPredictorsSettings?.Params != null ? poolPredictorsSettings.Params.DeepClone() : (reservoirPredictorsSettings?.Params != null ? reservoirPredictorsSettings.Params.DeepClone() : new Settings()));
            //Permits
            Activation = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.Activation) &&
                          (poolPredictorsSettings == null ? true : poolPredictorsSettings.Activation) &&
                          (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.Activation)
                          );
            SquaredActivation = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.SquaredActivation) &&
                                 (poolPredictorsSettings == null ? true : poolPredictorsSettings.SquaredActivation) &&
                                 (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.SquaredActivation)
                                 );
            FiringExpWRate = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.FiringExpWRate) &&
                              (poolPredictorsSettings == null ? true : poolPredictorsSettings.FiringExpWRate) &&
                              (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.FiringExpWRate)
                              );
            FiringFadingSum = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.FiringFadingSum) &&
                               (poolPredictorsSettings == null ? true : poolPredictorsSettings.FiringFadingSum) &&
                               (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.FiringFadingSum)
                               );
            FiringCount = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.FiringCount) &&
                           (poolPredictorsSettings == null ? true : poolPredictorsSettings.FiringCount) &&
                           (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.FiringCount)
                           );
            FiringBinPattern = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.FiringBinPattern) &&
                                (poolPredictorsSettings == null ? true : poolPredictorsSettings.FiringBinPattern) &&
                                (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.FiringBinPattern)
                                );
            NumOfEnabledPredictors = GetNumOfEnabledPredictors();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public HiddenNeuronPredictorsSettings(HiddenNeuronPredictorsSettings source)
        {
            //Params
            Params = source.Params?.DeepClone();
            //Permits
            Activation = source.Activation;
            SquaredActivation = source.SquaredActivation;
            FiringExpWRate = source.FiringExpWRate;
            FiringFadingSum = source.FiringFadingSum;
            FiringCount = source.FiringCount;
            FiringBinPattern = source.FiringBinPattern;
            NumOfEnabledPredictors = source.NumOfEnabledPredictors;
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml element containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public HiddenNeuronPredictorsSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.Neuron.HiddenNeuronPredictorsSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement predictorsElem = validator.Validate(elem, "rootElem");
            //Parsing of params
            Params = null;
            XElement paramsElem = predictorsElem.Descendants("settings").FirstOrDefault();
            if(paramsElem != null)
            {
                Params = new Settings(paramsElem);
            }
            //Parsing of permits
            XElement permitElem = predictorsElem.Descendants("permission").First();
            Activation = bool.Parse(permitElem.Attribute("activation").Value);
            SquaredActivation = bool.Parse(permitElem.Attribute("squaredActivation").Value);
            FiringExpWRate = bool.Parse(permitElem.Attribute("firingExpWRate").Value);
            FiringFadingSum = bool.Parse(permitElem.Attribute("firingFadingSum").Value);
            FiringCount = bool.Parse(permitElem.Attribute("firingCount").Value);
            FiringBinPattern = bool.Parse(permitElem.Attribute("firingBinPattern").Value);
            NumOfEnabledPredictors = GetNumOfEnabledPredictors();
            return;
        }

        //Properties
        //Methods
        /// <summary>
        /// Determines number of enabled predictors
        /// </summary>
        private int GetNumOfEnabledPredictors()
        {
            int count = 0;
            count += Activation ? 1 : 0;
            count += SquaredActivation ? 1 : 0;
            count += FiringExpWRate ? 1 : 0;
            count += FiringFadingSum ? 1 : 0;
            count += FiringCount ? 1 : 0;
            count += FiringBinPattern ? 1 : 0;
            return count;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            HiddenNeuronPredictorsSettings cmpSettings = obj as HiddenNeuronPredictorsSettings;
            if (!Equals(Params, cmpSettings.Params) ||
                Activation != cmpSettings.Activation ||
                SquaredActivation != cmpSettings.SquaredActivation ||
                FiringExpWRate != cmpSettings.FiringExpWRate ||
                FiringFadingSum != cmpSettings.FiringFadingSum ||
                FiringCount != cmpSettings.FiringCount ||
                FiringBinPattern != cmpSettings.FiringBinPattern ||
                NumOfEnabledPredictors != cmpSettings.NumOfEnabledPredictors
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
        public HiddenNeuronPredictorsSettings DeepClone()
        {
            HiddenNeuronPredictorsSettings clone = new HiddenNeuronPredictorsSettings(this);
            return clone;
        }

        //Inner classes
        /// <summary>
        /// Predictors' parameters
        /// </summary>
        [Serializable]
        public class Settings
        {
            //Constants
            /// <summary>
            /// Default value of window length for FiringExpWRate predictor
            /// </summary>
            public const int DefaultFiringExpWRateWindow = 64;
            /// <summary>
            /// Default value of strength of fading for FiringFadingSum predictor
            /// </summary>
            public const double DefaultFiringFadingSumStrength = 0.005;
            /// <summary>
            /// Default value of window length for FiringCount predictor
            /// </summary>
            public const int DefaultFiringCountWindow = 64;
            /// <summary>
            /// Default value of window length for FiringBinPattern predictor
            /// </summary>
            public const int DefaultFiringBinPatternWindow = 32;

            //Attribute properties
            /// <summary>
            /// Window length for FiringExpWRate predictor
            /// </summary>
            public int FiringExpWRateWindow { get; set; }
            /// <summary>
            /// Strength of fading for FiringFadingSum predictor
            /// </summary>
            public double FiringFadingSumStrength { get; set; }
            //Attribute properties
            /// <summary>
            /// Window length for FiringCount predictor
            /// </summary>
            public int FiringCountWindow { get; set; }
            /// <summary>
            /// Window length for FiringBinPattern predictor
            /// </summary>
            public int FiringBinPatternWindow { get; set; }
            
            //Constructor
            /// <summary>
            /// Creates initialized instance using default values
            /// </summary>
            public Settings()
            {
                FiringExpWRateWindow = DefaultFiringExpWRateWindow;
                FiringFadingSumStrength = DefaultFiringFadingSumStrength;
                FiringCountWindow = DefaultFiringCountWindow;
                FiringBinPatternWindow = DefaultFiringBinPatternWindow;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public Settings(Settings source)
            {
                FiringExpWRateWindow = source.FiringExpWRateWindow;
                FiringFadingSumStrength = source.FiringFadingSumStrength;
                FiringCountWindow = source.FiringCountWindow;
                FiringBinPatternWindow = source.FiringBinPatternWindow;
                return;
            }

            /// <summary>
            /// Creates initialized instance using xml element
            /// </summary>
            /// <param name="elem">Xml element containing settings</param>
            public Settings(XElement elem)
            {
                //Parsing
                FiringExpWRateWindow = int.Parse(elem.Descendants("firingExpWRate").First().Attribute("window").Value, CultureInfo.InvariantCulture);
                FiringFadingSumStrength = double.Parse(elem.Descendants("firingFadingSum").First().Attribute("strength").Value, CultureInfo.InvariantCulture);
                FiringCountWindow = int.Parse(elem.Descendants("firingCount").First().Attribute("window").Value, CultureInfo.InvariantCulture);
                FiringBinPatternWindow = int.Parse(elem.Descendants("firingBinPattern").First().Attribute("window").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                Settings cmpSettings = obj as Settings;
                if (FiringExpWRateWindow != cmpSettings.FiringExpWRateWindow ||
                    FiringFadingSumStrength != cmpSettings.FiringFadingSumStrength ||
                    FiringCountWindow != cmpSettings.FiringCountWindow ||
                    FiringBinPatternWindow != cmpSettings.FiringBinPatternWindow
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
            public Settings DeepClone()
            {
                Settings clone = new Settings(this);
                return clone;
            }

        }//Settings

    }//HiddenNeuronPredictorsSettings

}//Namespace

