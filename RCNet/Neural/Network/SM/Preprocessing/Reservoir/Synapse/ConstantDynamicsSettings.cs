using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.SynapseNS
{
    /// <summary>
    /// Configuration of synapse constant (no short-term plasticity) dynamics
    /// </summary>
    [Serializable]
    public abstract class ConstantDynamicsSettings : RCNetBaseSettings, IDynamicsSettings
    {
        //Attribute properties
        /// <summary>
        /// Synapse's constant efficacy
        /// </summary>
        public double Efficacy { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="efficacy">Synapse's constant efficacy</param>
        public ConstantDynamicsSettings(double efficacy)
        {
            Efficacy = efficacy;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ConstantDynamicsSettings(ConstantDynamicsSettings source)
            :this(source.Efficacy)
        {
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings</param>
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
        /// <summary>
        /// Type of synapse's dynamics
        /// </summary>
        public PlasticityCommon.DynType Type { get { return PlasticityCommon.DynType.Constant; } }

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
            if (Efficacy < 0 || Efficacy > 1)
            {
                throw new ArgumentException($"Invalid constant efficacy {Efficacy.ToString(CultureInfo.InvariantCulture)}. Efficacy must be GE to 0 and LE to 1.", "Efficacy");
            }
            return;
        }

    }//ConstantDynamicsSettings

}//Namespace

