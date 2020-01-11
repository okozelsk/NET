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
    /// Startup parameters for the elastic linear regression trainer
    /// </summary>
    [Serializable]
    public class ElasticRegrTrainerSettings : INonRecurrentNetworkTrainerSettings
    {
        //Constants

        //Attribute properties
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; set; }
        /// <summary>
        /// Ridge lambda hyperparameter
        /// </summary>
        public double Lambda { get; set; }
        /// <summary>
        /// Trade-off ratio between Ridge (0) and Lasso (1) approach
        /// </summary>
        public double Alpha { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="lambda">L1 (lasso) hyperparameter</param>
        /// <param name="alpha">L2 (ridge) hyperparameter</param>
        public ElasticRegrTrainerSettings(int numOfAttemptEpochs, double lambda = 0, double alpha = 0)
        {
            NumOfAttemptEpochs = numOfAttemptEpochs;
            Lambda = lambda;
            Alpha = alpha;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ElasticRegrTrainerSettings(ElasticRegrTrainerSettings source)
        {
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            Lambda = source.Lambda;
            Alpha = source.Alpha;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing elastic linear regression trainer settings</param>
        public ElasticRegrTrainerSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.NonRecurrent.FF.ElasticRegrTrainerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            Lambda = double.Parse(settingsElem.Attribute("lambda").Value, CultureInfo.InvariantCulture);
            Alpha = double.Parse(settingsElem.Attribute("alpha").Value, CultureInfo.InvariantCulture);
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
            ElasticRegrTrainerSettings cmpSettings = obj as ElasticRegrTrainerSettings;
            if (NumOfAttemptEpochs != cmpSettings.NumOfAttemptEpochs ||
                Lambda != cmpSettings.Lambda ||
                Alpha != cmpSettings.Alpha
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
            return new ElasticRegrTrainerSettings(this);
        }

    }//ElasticRegrTrainerSettings

}//Namespace
