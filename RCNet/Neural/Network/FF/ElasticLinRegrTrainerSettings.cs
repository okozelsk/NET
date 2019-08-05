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
    /// Startup parameters for the elastic linear regression trainer
    /// </summary>
    [Serializable]
    public class ElasticLinRegrTrainerSettings : INonRecurrentNetworkTrainerSettings
    {
        //Constants

        //Attribute properties
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; set; }
        /// <summary>
        /// L1 hyperparameter
        /// </summary>
        public double L1 { get; set; }
        /// <summary>
        /// L2 hyperparameter
        /// </summary>
        public double L2 { get; set; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="lambdaSeekerCfg">Configuration of seeker of lambda hyperparameter value</param>
        public ElasticLinRegrTrainerSettings(int numOfAttemptEpochs, double l1 = 0, double l2 = 0)
        {
            NumOfAttemptEpochs = numOfAttemptEpochs;
            L1 = l1;
            L2 = l2;
            return;
        }

        /// <summary>
        /// Deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ElasticLinRegrTrainerSettings(ElasticLinRegrTrainerSettings source)
        {
            NumOfAttemptEpochs = source.NumOfAttemptEpochs;
            L1 = source.L1;
            L2 = source.L2;
            return;
        }

        /// <summary>
        /// Creates the instance and initializes it from given xml element.
        /// Content of xml element is always validated against the xml schema.
        /// </summary>
        /// <param name="elem">Xml data containing elastic linear regression trainer settings</param>
        public ElasticLinRegrTrainerSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.FF.ElasticLinRegrTrainerSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement settingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            L1 = double.Parse(settingsElem.Attribute("l1").Value, CultureInfo.InvariantCulture);
            L2 = double.Parse(settingsElem.Attribute("l2").Value, CultureInfo.InvariantCulture);
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
            ElasticLinRegrTrainerSettings cmpSettings = obj as ElasticLinRegrTrainerSettings;
            if (NumOfAttemptEpochs != cmpSettings.NumOfAttemptEpochs ||
                L1 != cmpSettings.L1 ||
                L2 != cmpSettings.L2
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
            return new ElasticLinRegrTrainerSettings(this);
        }

    }//ElasticLinRegrTrainerSettings

}//Namespace
