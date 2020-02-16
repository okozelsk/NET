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
using RCNet.Neural.Network.SM.Preprocessing;
using System.Xml.XPath;

namespace RCNet.Neural.Network.SM.Synapse
{
    /// <summary>
    /// Configuration of internal synapse dynamics
    /// </summary>
    [Serializable]
    public abstract class DynamicsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Default minimum weight
        /// </summary>
        public const double DefaultMinWeight = 0d;
        /// <summary>
        /// Default maximum weight
        /// </summary>
        public const double DefaultMaxWeight = 1d;

        //Attribute properties
        /// <summary>
        /// Synapse's resting efficacy (average probability of neurotransmitter release)
        /// </summary>
        public double RestingEfficacy { get; }
        /// <summary>
        /// Synapse's efficacy depression model time constant (ms)
        /// </summary>
        public double TauDepression { get; }
        /// <summary>
        /// Synapse's efficacy facilitation model time constant (ms)
        /// </summary>
        public double TauFacilitation { get; }
        /// <summary>
        /// Specifies whether to apply short-term plasticity
        /// </summary>
        public bool ApplyShortTermPlasticity { get; }
        /// <summary>
        /// Synapse's random weight settings
        /// </summary>
        public URandomValueSettings WeightCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="restingEfficacy">Synapse's resting efficacy (average probability of neurotransmitter release)</param>
        /// <param name="tauDepression">Synapse's efficacy depression model time constant (ms)</param>
        /// <param name="tauFacilitation">Synapse's efficacy facilitation model time constant (ms)</param>
        /// <param name="applyShortTermPlasticity">Specifies whether to apply short-term plasticity</param>
        /// <param name="weightCfg">Synapse's random weight settings</param>
        public DynamicsSettings(double restingEfficacy,
                                double tauDepression,
                                double tauFacilitation,
                                bool applyShortTermPlasticity,
                                URandomValueSettings weightCfg
                                )
        {
            RestingEfficacy = restingEfficacy;
            TauDepression = tauDepression;
            TauFacilitation = tauFacilitation;
            ApplyShortTermPlasticity = applyShortTermPlasticity;
            WeightCfg = (weightCfg == null ? new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight) : (URandomValueSettings)weightCfg.DeepClone());
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public DynamicsSettings(DynamicsSettings source)
        {
            RestingEfficacy = source.RestingEfficacy;
            TauFacilitation = source.TauFacilitation;
            TauDepression = source.TauDepression;
            ApplyShortTermPlasticity = source.ApplyShortTermPlasticity;
            WeightCfg = (URandomValueSettings)source.WeightCfg.DeepClone();
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="settingsElem">
        /// Xml data containing settings.
        /// Content of xml element is not validated against the xml schema.
        /// </param>
        public DynamicsSettings(XElement elem, string XsdTypeName)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Resting efficacy
            RestingEfficacy = double.Parse(settingsElem.Attribute("restingEfficacy").Value, CultureInfo.InvariantCulture);
            //Efficacy depression
            TauDepression = double.Parse(settingsElem.Attribute("tauDepression").Value, CultureInfo.InvariantCulture);
            //Efficacy facilitation
            TauFacilitation = double.Parse(settingsElem.Attribute("tauFacilitation").Value, CultureInfo.InvariantCulture);
            //Apply short-term plasticity ?
            ApplyShortTermPlasticity = bool.Parse(settingsElem.Attribute("applyShortTermPlasticity").Value);
            //Weight
            XElement weightCfgElem = settingsElem.Descendants("weight").FirstOrDefault();
            if (weightCfgElem != null)
            {
                WeightCfg = new URandomValueSettings(weightCfgElem);
            }
            else
            {
                WeightCfg = new URandomValueSettings(DefaultMinWeight, DefaultMaxWeight);
            }
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultWeightCfg { get { return (WeightCfg.Min == DefaultMinWeight && WeightCfg.Max == DefaultMaxWeight && WeightCfg.IsDefaultDistrType); } }


        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (RestingEfficacy < 0 || RestingEfficacy > 1)
            {
                throw new Exception($"Invalid RestingEfficacy {RestingEfficacy.ToString(CultureInfo.InvariantCulture)}. RestingEfficacy must be GE to 0 and LE to 1.");
            }
            if (TauDepression < 0)
            {
                throw new Exception($"Invalid TauDepression {TauDepression.ToString(CultureInfo.InvariantCulture)}. TauDepression must be GE to 0.");
            }
            if (TauFacilitation < 0)
            {
                throw new Exception($"Invalid TauFacilitation {TauFacilitation.ToString(CultureInfo.InvariantCulture)}. TauFacilitation must be GE to 0.");
            }
            return;
        }

    }//DynamicsSettings

}//Namespace

