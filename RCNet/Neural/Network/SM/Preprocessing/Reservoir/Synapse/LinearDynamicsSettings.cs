using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of synapse linear short-term plasticity dynamics
    /// </summary>
    [Serializable]
    public abstract class LinearDynamicsSettings : RCNetBaseSettings, IDynamicsSettings
    {
        //Attribute properties
        /// <summary>
        /// The alpha argument in the linear expression efficacy = alpha * (spike - beta)
        /// </summary>
        public double Alpha { get; }

        /// <summary>
        /// The beta argument in the linear expression efficacy = alpha * (spike - beta)
        /// </summary>
        public double Beta { get; }

        /// <summary>
        /// Synapse initial efficacy
        /// </summary>
        public double InitialEfficacy { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="alpha">The alpha argument in the linear expression efficacy = alpha * (spike - beta)</param>
        /// <param name="beta">The beta argument in the linear expression efficacy = alpha * (spike - beta)</param>
        /// <param name="initialEfficacy">Synapse initial efficacy</param>
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
        /// <param name="source">Source instance</param>
        public LinearDynamicsSettings(LinearDynamicsSettings source)
            : this(source.Alpha, source.Beta, source.InitialEfficacy)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
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
        /// <summary>
        /// Type of synapse's dynamics
        /// </summary>
        public PlasticityCommon.DynType Type { get { return PlasticityCommon.DynType.Linear; } }

        /// <summary>
        /// Application of the synapse's dynamics
        /// </summary>
        public abstract PlasticityCommon.DynApplication Application { get; }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
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

