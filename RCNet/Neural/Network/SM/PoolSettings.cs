using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.IO;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.RandomValue;
using RCNet.Neural.Activation;

namespace RCNet.Neural.Network.SM
{
    /// <summary>
    /// The class contains neural pool configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. To create the proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create a proper instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class PoolSettings
    {
        //Attribute properties
        /// <summary>
        /// Instance name of this pool
        /// </summary>
        public string InstanceName { get; set; }
        /// <summary>
        /// Pool dimensions. Pool is 3D.
        /// </summary>
        public PoolDimensions Dim { get; set; }
        /// <summary>
        /// Determines whether to use neurons as the readout predictors
        /// </summary>
        public bool RouteToReadout { get; set; }
        /// <summary>
        /// Each reservoir input neuron will be connected by the synapse to the number of
        /// pool neurons = (Dim.Size * Density).
        /// Typical InputConnectionDensity = 1 (it means the full connectivity).
        /// </summary>
        public double InputConnectionDensity { get; set; }
        /// <summary>
        /// A weight of input neuron to pool's neuron synapse.
        /// </summary>
        public RandomValueSettings InputSynapseWeight { get; set; }
        /// <summary>
        /// Density of inhibitory neurons in the pool.
        /// </summary>
        public double InhibitoryNeuronsDensity { get; set; }
        /// <summary>
        /// Activation settings of the inhibitory neurons in the pool.
        /// </summary>
        public ActivationSettings InhibitoryActivation { get; set; }
        /// <summary>
        /// Each pool's neuron has its own constant input bias. Bias is always added to input signal of the neuron.
        /// A constant bias value of the neuron will be selected randomly.
        /// </summary>
        public RandomValueSettings InhibitoryBias { get; set; }
        /// <summary>
        /// Activation settings of the excitatory neurons in the pool.
        /// </summary>
        public ActivationSettings ExcitatoryActivation { get; set; }
        /// <summary>
        /// Each pool's neuron has its own constant input bias. Bias is always added to input signal of the neuron.
        /// A constant bias value of the neuron will be selected randomly.
        /// </summary>
        public RandomValueSettings ExcitatoryBias { get; set; }
        /// <summary>
        /// Density of interconnected neurons.
        /// Each pool neuron will be connected as a source neuron for Dim.Size * InterconnectionDensity neurons.
        /// </summary>
        public double InterconnectionDensity { get; set; }
        /// <summary>
        /// Average distance of interconnected neurons.
        /// 0 means random distance.
        /// </summary>
        public double InterconnectionAvgDistance { get; set; }
        /// <summary>
        /// Specifies whether to allow neurons to be self connected
        /// </summary>
        public bool InterconnectionAllowSelfConn { get; set; }
        /// <summary>
        /// Neurons in the pool are interconnected. The weight of the connection synapse will be selected randomly.
        /// </summary>
        public RandomValueSettings InterconnectionSynapseWeight { get; set; }
        /// <summary>
        /// Indicates whether the retainment (leaky integrators) neurons feature is used.
        /// Relevant for neurons having time independent activation (analog)
        /// </summary>
        public bool RetainmentNeuronsFeature { get; set; }
        /// <summary>
        /// The parameter says how much of the pool neurons will have the Retainment property set.
        /// Specific analog neurons will be selected randomly.
        /// Count = NumberOfAnalogNeurons * Density
        /// </summary>
        public double RetainmentNeuronsDensity { get; set; }
        /// <summary>
        /// If the pool neuron is selected to have the Retainment property then its retainment rate will be randomly selected
        /// from the closed interval (RetainmentMinRate, RetainmentMaxRate).
        /// </summary>
        public double RetainmentMinRate { get; set; }
        /// <summary>
        /// If the pool neuron is selected to have the Retainment property then its retainment rate will be randomly selected
        /// from the closed interval (RetainmentMinRate, RetainmentMaxRate).
        /// </summary>
        public double RetainmentMaxRate { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public PoolSettings()
        {
            InstanceName = string.Empty;
            Dim = null;
            InputConnectionDensity = 0;
            InputSynapseWeight = null;
            InhibitoryNeuronsDensity = 0;
            InhibitoryActivation = null;
            InhibitoryBias = null;
            ExcitatoryActivation = null;
            ExcitatoryBias = null;
            InterconnectionDensity = 0;
            InterconnectionAvgDistance = 0;
            InterconnectionSynapseWeight = null;
            RetainmentNeuronsFeature = false;
            RetainmentNeuronsDensity = 0;
            RetainmentMinRate = 0;
            RetainmentMaxRate = 0;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public PoolSettings(PoolSettings source)
        {
            InstanceName = source.InstanceName;
            Dim = null;
            if(source.Dim != null)
            {
                Dim = new PoolDimensions(source.Dim.X, source.Dim.Y, source.Dim.Z);
            }
            RouteToReadout = source.RouteToReadout;
            InputConnectionDensity = source.InputConnectionDensity;
            InputSynapseWeight = null;
            if (source.InputSynapseWeight != null)
            {
                InputSynapseWeight = source.InputSynapseWeight.DeepClone();
            }
            InhibitoryNeuronsDensity = source.InhibitoryNeuronsDensity;
            InhibitoryActivation = null;
            if(source.InhibitoryActivation != null)
            {
                InhibitoryActivation = source.InhibitoryActivation.DeepClone();
            }
            InhibitoryBias = null;
            if (source.InhibitoryBias != null)
            {
                InhibitoryBias = source.InhibitoryBias.DeepClone();
            }
            ExcitatoryActivation = null;
            if (source.ExcitatoryActivation != null)
            {
                ExcitatoryActivation = source.ExcitatoryActivation.DeepClone();
            }
            ExcitatoryBias = null;
            if(source.ExcitatoryBias != null)
            {
                ExcitatoryBias = source.ExcitatoryBias.DeepClone();
            }
            InterconnectionDensity = source.InterconnectionDensity;
            InterconnectionAvgDistance = source.InterconnectionAvgDistance;
            InterconnectionSynapseWeight = null;
            if(source.InterconnectionSynapseWeight != null)
            {
                InterconnectionSynapseWeight = source.InterconnectionSynapseWeight.DeepClone();
            }
            RetainmentNeuronsFeature = source.RetainmentNeuronsFeature;
            RetainmentNeuronsDensity = source.RetainmentNeuronsDensity;
            RetainmentMinRate = source.RetainmentMinRate;
            RetainmentMaxRate = source.RetainmentMaxRate;
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate pool settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing pool settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public PoolSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Network.SM.PoolSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement poolSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Name
            InstanceName = poolSettingsElem.Attribute("instanceName").Value;
            //Dimensions
            Dim = new PoolDimensions(int.Parse(poolSettingsElem.Attribute("dimX").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("dimY").Value, CultureInfo.InvariantCulture),
                                     int.Parse(poolSettingsElem.Attribute("dimZ").Value, CultureInfo.InvariantCulture)
                                     );
            RouteToReadout = bool.Parse(poolSettingsElem.Attribute("routeToReadout").Value);
            //Input
            XElement inputElem = poolSettingsElem.Descendants("input").First();
            InputConnectionDensity = double.Parse(inputElem.Attribute("connectionDensity").Value, CultureInfo.InvariantCulture);
            InputSynapseWeight = new RandomValueSettings(inputElem.Descendants("weight").First());
            //Excitatory
            XElement excitatoryElem = poolSettingsElem.Descendants("excitatory").First();
            ExcitatoryActivation = new ActivationSettings(excitatoryElem.Descendants("activation").First());
            ExcitatoryBias = new RandomValueSettings(excitatoryElem.Descendants("bias").First());
            double excitatoryRelShare = double.Parse(excitatoryElem.Attribute("relShare").Value, CultureInfo.InvariantCulture);
            //Inhibitory
            XElement inhibitoryElem = poolSettingsElem.Descendants("inhibitory").First();
            InhibitoryActivation = new ActivationSettings(inhibitoryElem.Descendants("activation").First());
            InhibitoryBias = new RandomValueSettings(inhibitoryElem.Descendants("bias").First());
            double inhibitoryRelShare = double.Parse(inhibitoryElem.Attribute("relShare").Value, CultureInfo.InvariantCulture);
            InhibitoryNeuronsDensity = inhibitoryRelShare / (inhibitoryRelShare + excitatoryRelShare);
            //Interconnection
            XElement interconnectionElem = poolSettingsElem.Descendants("interconnection").First();
            InterconnectionDensity = double.Parse(interconnectionElem.Attribute("density").Value, CultureInfo.InvariantCulture);
            InterconnectionAvgDistance = interconnectionElem.Attribute("avgDistance").Value == "NA" ? 0d : double.Parse(interconnectionElem.Attribute("avgDistance").Value, CultureInfo.InvariantCulture);
            InterconnectionAllowSelfConn = bool.Parse(interconnectionElem.Attribute("allowSelfConnection").Value);
            InterconnectionSynapseWeight = new RandomValueSettings(interconnectionElem.Descendants("weight").First());
            //Retainment neurons
            XElement retainmentElem = poolSettingsElem.Descendants("retainmentNeurons").FirstOrDefault();
            RetainmentNeuronsFeature = (retainmentElem != null);
            if (RetainmentNeuronsFeature)
            {
                RetainmentNeuronsDensity = double.Parse(retainmentElem.Attribute("density").Value, CultureInfo.InvariantCulture);
                RetainmentMinRate = double.Parse(retainmentElem.Attribute("retainmentMinRate").Value, CultureInfo.InvariantCulture);
                RetainmentMaxRate = double.Parse(retainmentElem.Attribute("retainmentMaxRate").Value, CultureInfo.InvariantCulture);
                RetainmentNeuronsFeature = (RetainmentNeuronsDensity > 0 &&
                                            RetainmentMaxRate > 0
                                            );
            }
            else
            {
                RetainmentNeuronsDensity = 0;
                RetainmentMinRate = 0;
                RetainmentMaxRate = 0;
            }
            return;
        }

        //Methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            PoolSettings cmpSettings = obj as PoolSettings;
            if (InstanceName != cmpSettings.InstanceName ||
                !Equals(Dim, cmpSettings.Dim) ||
                RouteToReadout != cmpSettings.RouteToReadout ||
                InputConnectionDensity != cmpSettings.InputConnectionDensity ||
                !Equals(InputSynapseWeight, cmpSettings.InputSynapseWeight) ||
                !Equals(ExcitatoryActivation, cmpSettings.ExcitatoryActivation) ||
                !Equals(ExcitatoryBias, cmpSettings.ExcitatoryBias) ||
                !Equals(InhibitoryActivation, cmpSettings.InhibitoryActivation) ||
                !Equals(InhibitoryBias, cmpSettings.InhibitoryBias) ||
                InhibitoryNeuronsDensity != cmpSettings.InhibitoryNeuronsDensity ||
                InterconnectionDensity != cmpSettings.InterconnectionDensity ||
                InterconnectionAvgDistance != cmpSettings.InterconnectionAvgDistance ||
                !Equals(InterconnectionSynapseWeight, cmpSettings.InterconnectionSynapseWeight) ||
                RetainmentNeuronsFeature != cmpSettings.RetainmentNeuronsFeature ||
                RetainmentNeuronsDensity != cmpSettings.RetainmentNeuronsDensity ||
                RetainmentMinRate != cmpSettings.RetainmentMinRate ||
                RetainmentMaxRate != cmpSettings.RetainmentMaxRate
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
            return InstanceName.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public PoolSettings DeepClone()
        {
            PoolSettings clone = new PoolSettings(this);
            return clone;
        }

    }//PoolSettings

}//Namespace

