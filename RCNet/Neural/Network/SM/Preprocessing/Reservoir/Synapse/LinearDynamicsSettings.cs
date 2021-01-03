using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse's linear efficacy dynamics.
    /// </summary>
    [Serializable]
    public abstract class LinearDynamicsSettings : RCNetBaseSettings, IDynamicsSettings
    {
        //Attribute properties
        /// <summary>
        /// The value of the alpha argument in the linear expression: efficacy = alpha * (spike - beta).
        /// </summary>
        public double Alpha { get; }

        /// <summary>
        /// The value of the beta argument in the linear expression: efficacy = alpha * (spike - beta).
        /// </summary>
        public double Beta { get; }

        /// <summary>
        /// The value of the synapse's initial efficacy.
        /// </summary>
        public double InitialEfficacy { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="alpha">The value of the alpha argument in the linear expression: efficacy = alpha * (spike - beta).</param>
        /// <param name="beta">The value of the beta argument in the linear expression: efficacy = alpha * (spike - beta).</param>
        /// <param name="initialEfficacy">The value of the synapse's initial efficacy.</param>
        public LinearDynamicsSettings(double alpha, double beta, double initialEfficacy)
        {
            Alpha = alpha;
            Beta = beta;
            InitialEfficacy = initialEfficacy;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public LinearDynamicsSettings(LinearDynamicsSettings source)
            : this(source.Alpha, source.Beta, source.InitialEfficacy)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        /// <param name="xsdTypeName">Name of the associated type defined in xsd</param>
        public LinearDynamicsSettings(XElement elem, string xsdTypeName)
        {
            //Validation
            XElement settingsElem = Validate(elem, xsdTypeName);
            //Parsing
            //Alpha
            Alpha = double.Parse(settingsElem.Attribute("alpha").Value, CultureInfo.InvariantCulture);
            //Beta
            Beta = double.Parse(settingsElem.Attribute("beta").Value, CultureInfo.InvariantCulture);
            //Initial efficacy
            InitialEfficacy = double.Parse(settingsElem.Attribute("initialEfficacy").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public PlasticityCommon.DynType Type { get { return PlasticityCommon.DynType.Linear; } }

        /// <inheritdoc />
        public abstract PlasticityCommon.DynApplication Application { get; }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Alpha < 0 || Alpha > 1)
            {
                throw new ArgumentException($"Invalid alpha {Alpha.ToString(CultureInfo.InvariantCulture)}. Alpha must be GE to 0 and LE to 1.", "Alpha");
            }
            if (Beta < 0 || Beta > 1)
            {
                throw new ArgumentException($"Invalid beta {Beta.ToString(CultureInfo.InvariantCulture)}. Beta must be GE to 0 and LE to 1.", "Beta");
            }
            if (InitialEfficacy < 0 || InitialEfficacy > 1)
            {
                throw new ArgumentException($"Invalid InitialEfficacy {InitialEfficacy.ToString(CultureInfo.InvariantCulture)}. InitialEfficacy must be GE to 0 and LE to 1.", "InitialEfficacy");
            }
            return;
        }

    }//LinearDynamicsSettings

}//Namespace

