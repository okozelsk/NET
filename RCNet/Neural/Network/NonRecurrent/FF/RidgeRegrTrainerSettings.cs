using RCNet.MathTools.PS;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the RidgeRegrTrainer
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
        /// Creates an initialized instance.
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
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultLambdaSeeker { get { return (LambdaSeekerCfg.Min == DefaultMinLambda && LambdaSeekerCfg.Max == DefaultMaxLambda && LambdaSeekerCfg.NumOfSubIntervals == DefaultSteps); } }

        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (NumOfAttemptEpochs < 1)
            {
                throw new ArgumentException($"Invalid NumOfAttemptEpochs {NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)}. NumOfAttemptEpochs must be GE to 1.", "NumOfAttemptEpochs");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new RidgeRegrTrainerSettings(this);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("ridgeRegrTrainer", suppressDefaults);
        }

    }//RidgeRegrTrainerSettings

}//Namespace
