using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Globalization;
using System.Reflection;
using RCNet.XmlTools;
using RCNet.MathTools.PS;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Startup parameters for the ridge regression trainer
    /// </summary>
    [Serializable]
    public class RidgeRegrTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "FFNetRidgeRegrTrainerType";
        /// <summary>
        /// Seeker's default min lambda
        /// </summary>
        public const double DefaultMinLambda = 0;
        /// <summary>
        /// Seeker's default max lambda
        /// </summary>
        public const double DefaultMaxLambda = 0.5;
        /// <summary>
        /// Seeker's default number of steps within the interval
        /// </summary>
        public const int DefaultSteps = 10;

        //Attribute properties
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// Configuration of seeker of lambda hyperparameter value
        /// </summary>
        public ParamSeekerSettings LambdaSeekerCfg { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="lambdaSeekerCfg">Configuration of seeker of lambda hyperparameter value</param>
        public RidgeRegrTrainerSettings(int numOfAttemptEpochs, ParamSeekerSettings lambdaSeekerCfg = null)
        {
            NumOfAttemptEpochs = numOfAttemptEpochs;
            if (lambdaSeekerCfg == null)
            {
                LambdaSeekerCfg = new ParamSeekerSettings(DefaultMinLambda, DefaultMaxLambda, DefaultSteps);
            }
            else
            {
                LambdaSeekerCfg = (ParamSeekerSettings)lambdaSeekerCfg.DeepClone();
            }
            Check();
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RidgeRegrTrainerSettings(RidgeRegrTrainerSettings source)
        {
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            LambdaSeekerCfg = (ParamSeekerSettings)source.LambdaSeekerCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing linear regression trainer settings</param>
        public RidgeRegrTrainerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            XElement lambdaSeekerSettingsElem = settingsElem.Elements("lambda").FirstOrDefault();
            if (lambdaSeekerSettingsElem != null)
            {
                LambdaSeekerCfg = new ParamSeekerSettings(lambdaSeekerSettingsElem);
            }
            else
            {
                LambdaSeekerCfg = new ParamSeekerSettings(DefaultMinLambda, DefaultMaxLambda, DefaultSteps);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Number of attempts. Always 1.
        /// </summary>
        public int NumOfAttempts { get { return 1; } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultLambdaSeeker { get { return (LambdaSeekerCfg.Min == DefaultMinLambda && LambdaSeekerCfg.Max == DefaultMaxLambda && LambdaSeekerCfg.NumOfSubIntervals == DefaultSteps); } }

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
            if (NumOfAttemptEpochs < 1)
            {
                throw new Exception($"Invalid NumOfAttemptEpochs {NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)}. NumOfAttemptEpochs must be GE to 1.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new RidgeRegrTrainerSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("attemptEpochs", NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || !IsDefaultLambdaSeeker)
            {
                rootElem.Add(LambdaSeekerCfg.GetXml("lambda", suppressDefaults));
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
            return GetXml("ridgeRegrTrainer", suppressDefaults);
        }

    }//RidgeRegrTrainerSettings

}//Namespace
