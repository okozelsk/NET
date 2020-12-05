using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.NonRecurrent.FF
{
    /// <summary>
    /// Configuration of the ElasticRegrTrainer
    /// </summary>
    [Serializable]
    public class ElasticRegrTrainerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "FFNetElasticRegrTrainerType";
        //Default values
        /// <summary>
        /// Default value of lambda argument
        /// </summary>
        public const double DefaultLambda = 1e-6d;
        /// <summary>
        /// Default value of alpha argument
        /// </summary>
        public const double DefaultAlpha = 0.5d;

        //Attribute properties
        /// <summary>
        /// Number of attempt epochs
        /// </summary>
        public int NumOfAttemptEpochs { get; }
        /// <summary>
        /// Ridge lambda hyperparameter
        /// </summary>
        public double Lambda { get; }
        /// <summary>
        /// Trade-off ratio between Ridge (0) and Lasso (1) approach
        /// </summary>
        public double Alpha { get; }

        //Constructors
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="numOfAttemptEpochs">Number of attempt epochs</param>
        /// <param name="lambda">L1 (lasso) hyperparameter</param>
        /// <param name="alpha">L2 (ridge) hyperparameter</param>
        public ElasticRegrTrainerSettings(int numOfAttemptEpochs,
                                          double lambda = DefaultLambda,
                                          double alpha = DefaultAlpha
                                          )
        {
            NumOfAttemptEpochs = numOfAttemptEpochs;
            Lambda = lambda;
            Alpha = alpha;
            Check();
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml data containing elastic linear regression trainer settings</param>
        public ElasticRegrTrainerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            NumOfAttemptEpochs = int.Parse(settingsElem.Attribute("attemptEpochs").Value, CultureInfo.InvariantCulture);
            Lambda = double.Parse(settingsElem.Attribute("lambda").Value, CultureInfo.InvariantCulture);
            Alpha = double.Parse(settingsElem.Attribute("alpha").Value, CultureInfo.InvariantCulture);
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
        public bool IsDefaultLambda { get { return (Lambda == DefaultLambda); } }

        /// <summary>
        /// Checks the defaults
        /// </summary>
        public bool IsDefaultAlpha { get { return (Alpha == DefaultAlpha); } }

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
            if (Lambda < 0)
            {
                throw new ArgumentException($"Invalid Lambda {Lambda.ToString(CultureInfo.InvariantCulture)}. Lambda must be GE to 0.", "Lambda");
            }
            if (Alpha < 0)
            {
                throw new ArgumentException($"Invalid Alpha {Alpha.ToString(CultureInfo.InvariantCulture)}. Alpha must be GE to 0.", "Alpha");
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new ElasticRegrTrainerSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("attemptEpochs", NumOfAttemptEpochs.ToString(CultureInfo.InvariantCulture)));
            if (!suppressDefaults || !IsDefaultLambda)
            {
                rootElem.Add(new XAttribute("lambda", Lambda.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultAlpha)
            {
                rootElem.Add(new XAttribute("alpha", Alpha.ToString(CultureInfo.InvariantCulture)));
            }

            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("elasticRegrTrainer", suppressDefaults);
        }

    }//ElasticRegrTrainerSettings

}//Namespace
