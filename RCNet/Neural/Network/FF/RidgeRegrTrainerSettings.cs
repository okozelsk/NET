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

namespace RCNet.Neural.Network.FF
{
    /// <summary>
    /// Startup parameters for the ridge regression trainer
    /// </summary>
    [Serializable]
    public class RidgeRegrTrainerSettings : INonRecurrentNetworkTrainerSettings
    {
        //Constants
        /// <summary>
        /// Seeker's default min lambda
        /// </summary>
        public const double DefaultMinLambda = 0;
        /// <summary>
        /// Seeker's default max lambda
        /// </summary>
        public const double DefaultMaxLambda = 0.05;
        /// <summary>
        /// Seeker's default number of steps within the interval
        /// </summary>
        public const int DefaultSteps = 10;

        //Attribute properties
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; set; }
        /// <summary>
        /// Configuration of seeker of lambda hyperparameter value
        /// </summary>
        public ParamSeekerSettings LambdaSeekerCfg { get; set; }

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
                LambdaSeekerCfg = lambdaSeekerCfg.DeepClone();
            }
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public RidgeRegrTrainerSettings(RidgeRegrTrainerSettings source)
        {
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            LambdaSeekerCfg = source.LambdaSeekerCfg.DeepClone();
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
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.FF.RidgeRegrTrainerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            LambdaSeekerCfg = new ParamSeekerSettings(settingsElem.Descendants("lambda").First());
            return;
        }

        //Properties
        /// <summary>
        /// Number of attempts. Always 1.
        /// </summary>
        public int NumOfAttempts { get { return 1; } }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RidgeRegrTrainerSettings cmpSettings = obj as RidgeRegrTrainerSettings;
            if (NumOfAttemptEpochs != cmpSettings.NumOfAttemptEpochs ||
                !LambdaSeekerCfg.Equals(cmpSettings.LambdaSeekerCfg)
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
        public INonRecurrentNetworkTrainerSettings DeepClone()
        {
            return new RidgeRegrTrainerSettings(this);
        }

    }//RidgeRegrTrainerSettings

}//Namespace
