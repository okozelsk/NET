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
    public class HiddenNeuronPredictorsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PredictorsCfgType";

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
        public bool ActivationSquare { get; private set; }
        /// <summary>
        /// Fading sum of the activation state
        /// </summary>
        public bool ActivationFadingSum { get; private set; }
        /// <summary>
        /// Moving weighted average activation
        /// </summary>
        public bool ActivationMWAvg { get; private set; }
        /// <summary>
        /// Fading number of firings
        /// </summary>
        public bool FiringFadingSum { get; private set; }
        /// <summary>
        /// Moving weighted average firing
        /// </summary>
        public bool FiringMWAvg { get; private set; }
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
            ActivationSquare = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.ActivationSquare) &&
                                 (poolPredictorsSettings == null ? true : poolPredictorsSettings.ActivationSquare) &&
                                 (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.ActivationSquare)
                                 );
            ActivationFadingSum = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.ActivationFadingSum) &&
                                 (poolPredictorsSettings == null ? true : poolPredictorsSettings.ActivationFadingSum) &&
                                 (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.ActivationFadingSum)
                                 );
            ActivationMWAvg = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.ActivationMWAvg) &&
                              (poolPredictorsSettings == null ? true : poolPredictorsSettings.ActivationMWAvg) &&
                              (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.ActivationMWAvg)
                              );
            FiringFadingSum = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.FiringFadingSum) &&
                               (poolPredictorsSettings == null ? true : poolPredictorsSettings.FiringFadingSum) &&
                               (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.FiringFadingSum)
                               );
            FiringMWAvg = ((groupPredictorsSettings == null ? true : groupPredictorsSettings.FiringMWAvg) &&
                              (poolPredictorsSettings == null ? true : poolPredictorsSettings.FiringMWAvg) &&
                              (reservoirPredictorsSettings == null ? true : reservoirPredictorsSettings.FiringMWAvg)
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
            ActivationSquare = source.ActivationSquare;
            ActivationFadingSum = source.ActivationFadingSum;
            ActivationMWAvg = source.ActivationMWAvg;
            FiringFadingSum = source.FiringFadingSum;
            FiringMWAvg = source.FiringMWAvg;
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
            XElement predictorsElem = Validate(elem, XsdTypeName);
            //Parsing of params
            Params = null;
            XElement paramsElem = predictorsElem.Descendants("settings").FirstOrDefault();
            if(paramsElem != null)
            {
                Params = new Settings(paramsElem);
            }
            //Parsing of permission
            XElement permitElem = predictorsElem.Descendants("permission").First();
            Activation = bool.Parse(permitElem.Attribute("activation").Value);
            ActivationSquare = bool.Parse(permitElem.Attribute("activationSquare").Value);
            ActivationFadingSum = bool.Parse(permitElem.Attribute("activationFadingSum").Value);
            ActivationMWAvg = bool.Parse(permitElem.Attribute("activationMWAvg").Value);
            FiringFadingSum = bool.Parse(permitElem.Attribute("firingFadingSum").Value);
            FiringMWAvg = bool.Parse(permitElem.Attribute("firingMWAvg").Value);
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
            count += ActivationSquare ? 1 : 0;
            count += ActivationFadingSum ? 1 : 0;
            count += ActivationMWAvg ? 1 : 0;
            count += FiringFadingSum ? 1 : 0;
            count += FiringMWAvg ? 1 : 0;
            count += FiringCount ? 1 : 0;
            count += FiringBinPattern ? 1 : 0;
            return count;
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
            /// Default value of strength of fading for ActivationFadingSum predictor
            /// </summary>
            public const double DefaultActivationFadingSumStrength = 0.1;
            /// <summary>
            /// Default value of window length for ActivationMWAvg predictor
            /// </summary>
            public const int DefaultActivationMWAvgWindow = 64;
            /// <summary>
            /// Default value of leakage for ActivationMWAvg predictor
            /// </summary>
            public const int DefaultActivationMWAvgLeakage = 0;
            /// <summary>
            /// Default weights type for ActivationMWAvg predictor
            /// </summary>
            public const NeuronCommon.NeuronPredictorMWAvgWeightsType DefaultActivationMWAvgWeightsType = NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential;
            /// <summary>
            /// Default value of strength of fading for FiringFadingSum predictor
            /// </summary>
            public const double DefaultFiringFadingSumStrength = 0.005;
            /// <summary>
            /// Default value of window length for FiringMWAvg predictor
            /// </summary>
            public const int DefaultFiringMWAvgWindow = 64;
            /// <summary>
            /// Default value of leakage for FiringMWAvg predictor
            /// </summary>
            public const int DefaultFiringMWAvgLeakage = 0;
            /// <summary>
            /// Default weights type for FiringMWAvg predictor
            /// </summary>
            public const NeuronCommon.NeuronPredictorMWAvgWeightsType DefaultFiringMWAvgWeightsType = NeuronCommon.NeuronPredictorMWAvgWeightsType.Exponential;
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
            /// Strength of fading for ActivationFadingSum predictor
            /// </summary>
            public double ActivationFadingSumStrength { get; set; }
            /// <summary>
            /// Window length for ActivationMWAvg predictor
            /// </summary>
            public int ActivationMWAvgWindow { get; set; }
            /// <summary>
            /// Leakage for ActivationMWAvg predictor
            /// </summary>
            public int ActivationMWAvgLeakage { get; set; }
            /// <summary>
            /// Leakage for ActivationMWAvg predictor
            /// </summary>
            public NeuronCommon.NeuronPredictorMWAvgWeightsType ActivationMWAvgWeightsType { get; set; }
            /// <summary>
            /// Strength of fading for FiringFadingSum predictor
            /// </summary>
            public double FiringFadingSumStrength { get; set; }
            /// <summary>
            /// Window length for FiringMWAvg predictor
            /// </summary>
            public int FiringMWAvgWindow { get; set; }
            /// <summary>
            /// Leakage for FiringMWAvg predictor
            /// </summary>
            public int FiringMWAvgLeakage { get; set; }
            /// <summary>
            /// Leakage for FiringMWAvg predictor
            /// </summary>
            public NeuronCommon.NeuronPredictorMWAvgWeightsType FiringMWAvgWeightsType { get; set; }
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
                ActivationFadingSumStrength = DefaultActivationFadingSumStrength;
                ActivationMWAvgWindow = DefaultActivationMWAvgWindow;
                ActivationMWAvgLeakage = DefaultActivationMWAvgLeakage;
                ActivationMWAvgWeightsType = DefaultActivationMWAvgWeightsType;
                FiringFadingSumStrength = DefaultFiringFadingSumStrength;
                FiringMWAvgWindow = DefaultFiringMWAvgWindow;
                FiringMWAvgLeakage = DefaultFiringMWAvgLeakage;
                FiringMWAvgWeightsType = DefaultFiringMWAvgWeightsType;
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
                ActivationFadingSumStrength = source.ActivationFadingSumStrength;
                ActivationMWAvgWindow = source.ActivationMWAvgWindow;
                ActivationMWAvgLeakage = source.ActivationMWAvgLeakage;
                ActivationMWAvgWeightsType = source.ActivationMWAvgWeightsType;
                FiringFadingSumStrength = source.FiringFadingSumStrength;
                FiringMWAvgWindow = source.FiringMWAvgWindow;
                FiringMWAvgLeakage = source.FiringMWAvgLeakage;
                FiringMWAvgWeightsType = source.FiringMWAvgWeightsType;
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
                ActivationFadingSumStrength = double.Parse(elem.Descendants("activationFadingSum").First().Attribute("strength").Value, CultureInfo.InvariantCulture);
                ActivationMWAvgWindow = int.Parse(elem.Descendants("activationMWAvg").First().Attribute("window").Value, CultureInfo.InvariantCulture);
                ActivationMWAvgLeakage = int.Parse(elem.Descendants("activationMWAvg").First().Attribute("leakage").Value, CultureInfo.InvariantCulture);
                ActivationMWAvgWeightsType = NeuronCommon.ParseNeuronPredictorMWAvgWeightsType(elem.Descendants("activationMWAvg").First().Attribute("weights").Value);
                FiringFadingSumStrength = double.Parse(elem.Descendants("firingFadingSum").First().Attribute("strength").Value, CultureInfo.InvariantCulture);
                FiringMWAvgWindow = int.Parse(elem.Descendants("firingMWAvg").First().Attribute("window").Value, CultureInfo.InvariantCulture);
                FiringMWAvgLeakage = int.Parse(elem.Descendants("firingMWAvg").First().Attribute("leakage").Value, CultureInfo.InvariantCulture);
                FiringMWAvgWeightsType = NeuronCommon.ParseNeuronPredictorMWAvgWeightsType(elem.Descendants("firingMWAvg").First().Attribute("weights").Value);
                FiringCountWindow = int.Parse(elem.Descendants("firingCount").First().Attribute("window").Value, CultureInfo.InvariantCulture);
                FiringBinPatternWindow = int.Parse(elem.Descendants("firingBinPattern").First().Attribute("window").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// Creates the deep copy instance of this instance
            /// </summary>
            public Settings DeepClone()
            {
                return new Settings(this);
            }

        }//Settings

    }//HiddenNeuronPredictorsSettings

}//Namespace

