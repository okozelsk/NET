using System;
using System.Globalization;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of the synapse's constant efficacy dynamics.
    /// </summary>
    [Serializable]
    public abstract class ConstantDynamicsSettings : RCNetBaseSettings, IDynamicsSettings
    {
        //Attribute properties
        /// <summary>
        /// The synapse's constant efficacy.
        /// </summary>
        public double Efficacy { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="efficacy">The synapse's constant efficacy.</param>
        public ConstantDynamicsSettings(double efficacy)
        {
            Efficacy = efficacy;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ConstantDynamicsSettings(ConstantDynamicsSettings source)
            : this(source.Efficacy)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        /// <param name="xsdTypeName">Name of the associated type defined in xsd</param>
        public ConstantDynamicsSettings(XElement elem, string xsdTypeName)
        {
            //Validation
            XElement settingsElem = Validate(elem, xsdTypeName);
            //Parsing
            //Constant efficacy
            Efficacy = double.Parse(settingsElem.Attribute("efficacy").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public PlasticityCommon.DynType Type { get { return PlasticityCommon.DynType.Constant; } }

        /// <inheritdoc />
        public abstract PlasticityCommon.DynApplication Application { get; }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (Efficacy < 0 || Efficacy > 1)
            {
                throw new ArgumentException($"Invalid constant efficacy {Efficacy.ToString(CultureInfo.InvariantCulture)}. Efficacy must be GE to 0 and LE to 1.", "Efficacy");
            }
            return;
        }

    }//ConstantDynamicsSettings

}//Namespace

