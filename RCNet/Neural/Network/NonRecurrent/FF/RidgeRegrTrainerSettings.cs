using RCNet.MathTools;
using System;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the RidgeRegrTrainer.
    /// </summary>
    [Serializable]
    public class RidgeRegrTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "FFNetRidgeRegrTrainerType";
        //Default values
        /// <summary>
        /// The default min lambda.
        /// </summary>
        public const double DefaultMinLambda = 0;
        /// <summary>
        /// The default max lambda.
        /// </summary>
        public const double DefaultMaxLambda = 0.5;
        /// <summary>
        /// The default number of steps within the interval.
        /// </summary>
        public const int DefaultSteps = 10;

        //Attribute properties
        /// <summary>
        /// The number of attempt epochs.
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// The configuration of the lambda parameter finder.
        /// </summary>
        public ParamValFinderSettings LambdaFinderCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfAttemptEpochs">The number of attempt epochs.</param>
        /// <param name="lambdaFinderCfg">The configuration of the lambda parameter finder.</param>
        public RidgeRegrTrainerSettings(int numOfAttemptEpochs, ParamValFinderSettings lambdaFinderCfg = null)
        {
            NumOfAttemptEpochs = numOfAttemptEpochs;
            if (lambdaFinderCfg == null)
            {
                LambdaFinderCfg = new ParamValFinderSettings(DefaultMinLambda, DefaultMaxLambda, DefaultSteps);
            }
            else
            {
                LambdaFinderCfg = (ParamValFinderSettings)lambdaFinderCfg.DeepClone();
            }
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public RidgeRegrTrainerSettings(RidgeRegrTrainerSettings source)
        {
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            LambdaFinderCfg = (ParamValFinderSettings)source.LambdaFinderCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public RidgeRegrTrainerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            XElement lambdaSeekerSettingsElem = settingsElem.Elements("lambda").FirstOrDefault();
            if (lambdaSeekerSettingsElem != null)
            {
                LambdaFinderCfg = new ParamValFinderSettings(lambdaSeekerSettingsElem);
            }
            else
            {
                LambdaFinderCfg = new ParamValFinderSettings(DefaultMinLambda, DefaultMaxLambda, DefaultSteps);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// The number of attempts. Always 1.
        /// </summary>
        public int NumOfAttempts { get { return 1; } }

        /// <summary>
        /// Checks the defaults.
        /// </summary>
        public bool IsDefaultLambdaSeeker { get { return (LambdaFinderCfg.Min == DefaultMinLambda && LambdaFinderCfg.Max == DefaultMaxLambda && LambdaFinderCfg.NumOfSubIntervals == DefaultSteps); } }

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
                rootElem.Add(LambdaFinderCfg.GetXml("lambda", suppressDefaults));
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
