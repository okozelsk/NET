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
using RCNet.Neural.Analog.Activation;

namespace RCNet.Neural.Analog.Reservoir
{
    /// <summary>
    /// The class contains analog reservoir configuration parameters and also contains
    /// internal logic so it is not just a container of parameters. Creating an proper instance by hand is not
    /// a trivial task.
    /// The easiest and safest way to create an instance is to use the xml constructor.
    /// </summary>
    [Serializable]
    public class AnalogReservoirSettings
    {
        //Constants
        /// <summary>
        /// Supported types of reservoir internal topology
        /// </summary>
        public enum ReservoirTopologyType
        {
            /// <summary>
            /// Random topology. Reservoir's neurons are connected randomly.
            /// </summary>
            Random,
            /// <summary>
            /// Ring topology. Reservoir's neurons are connected in a ring shape.
            /// </summary>
            Ring,
            /// <summary>
            /// Doubly twisted thoroidal topology.
            /// </summary>
            DTT
        };

        //Attribute properties
        /// <summary>
        /// Name of this configuration
        /// </summary>
        public string SettingsName { get; set; }
        /// <summary>
        /// Each input field will be connected by the random weight to the number of
        /// reservoir neurons = (Size * Density).
        /// Typical InputConnectionDensity = 1 (it means the full connectivity).
        /// </summary>
        public double InputConnectionDensity { get; set; }
        /// <summary>
        /// A weight of each input field to reservoir's neuron connection.
        /// </summary>
        public RandomValueSettings InputWeight { get; set; }
        /// <summary>
        /// Number of the neurons in the reservoir.
        /// </summary>
        public int Size { get; set; }
        /// <summary>
        /// Activation settings of the neurons in the reservoir.
        /// </summary>
        public AnalogActivationSettings ReservoirActivation { get; set; }
        /// <summary>
        /// Spectral radius.
        /// </summary>
        public double SpectralRadius { get; set; }
        /// <summary>
        /// Each reservoir's neuron has its own constant input bias. Bias is always added to input signal of the neuron.
        /// A constant bias value will be for each neuron selected randomly.
        /// </summary>
        public RandomValueSettings Bias { get; set; }
        /// <summary>
        /// Neurons in the reservoir are interconnected. The weight of the connection will be randomly selected.
        /// </summary>
        public RandomValueSettings InternalWeight { get; set; }
        /// <summary>
        /// One of the supported reservoir topologies of internal neural networking.
        /// See the enumeration ReservoirTopologyType.
        /// </summary>
        public ReservoirTopologyType TopologyType { get; set; }
        /// <summary>
        /// Parameters of the topology of internal neural networking.
        /// See classes RandomTopology, RingTopology and DTTTopology.
        /// </summary>
        public Object TopologySettings { get; set; }
        /// <summary>
        /// Indicates whether the retainment (leaky integrators) neurons feature is used.
        /// </summary>
        public bool RetainmentNeuronsFeature { get; set; }
        /// <summary>
        /// The parameter says how much of the reservoir's neurons will have the Retainment property set.
        /// Specific neurons will be selected randomly.
        /// Count = Size * Density
        /// </summary>
        public double RetainmentNeuronsDensity { get; set; }
        /// <summary>
        /// If the reservoir's neuron is selected to have Retainment property then its retainment rate will be randomly selected
        /// from the closed interval (RetainmentMinRate, RetainmentMaxRate).
        /// </summary>
        public double RetainmentMinRate { get; set; }
        /// <summary>
        /// If the reservoir's neuron is selected to have Retainment property then its retainment rate will be randomly selected
        /// from the closed interval (RetainmentMinRate, RetainmentMaxRate).
        /// </summary>
        public double RetainmentMaxRate { get; set; }
        /// <summary>
        /// Indicates whether the context neuron feature is used.
        /// Context neuron is a special neuron outside the reservoir, which mixes and processes the signal from all
        /// the neurons in the reservoir. The context neuron state thus represents the state of the entire reservoir
        /// and is then used as one of the inputs to the neurons in the reservoir.
        /// </summary>
        public bool ContextNeuronFeature { get; set; }
        /// <summary>
        /// Activation settings of the context neuron.
        /// </summary>
        public AnalogActivationSettings ContextNeuronActivation { get; set; }
        /// <summary>
        /// Each weight of the connection from the reservoir neuron to the contex neuron will be randomly selected.
        /// </summary>
        public RandomValueSettings ContextNeuronInputWeight { get; set; }
        /// <summary>
        /// The parameter says how many neurons in the reservoir will receive the signal from the context neuron.
        /// Count = Size * Density
        /// </summary>
        public double ContextNeuronFeedbackDensity { get; set; }
        /// <summary>
        /// Weight of the feedback connection from the context neuron.
        /// </summary>
        public RandomValueSettings ContextNeuronFeedbackWeight { get; set; }
        /// <summary>
        /// Indicates whether the feedback feature is used.
        /// </summary>
        public bool FeedbackFeature { get; set; }
        /// <summary>
        /// Each feedback field will be connected by the random weight to the number of
        /// reservoir neurons = (Size * Density).
        /// Typical FeedbackConnectionDensity = 1 (it means the full connectivity).
        /// </summary>
        public double FeedbackConnectionDensity { get; set; }
        /// <summary>
        /// A weight of each feedback field to reservoir's neuron connection will be randomly selected.
        /// </summary>
        public RandomValueSettings FeedbackWeight { get; set; }
        /// <summary>
        /// Collection of feedback field names.
        /// </summary>
        public List<string> FeedbackFieldNameCollection { get; set; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance
        /// </summary>
        public AnalogReservoirSettings()
        {
            SettingsName = string.Empty;
            InputConnectionDensity = 0;
            InputWeight = null;
            Size = 0;
            ReservoirActivation = null;
            SpectralRadius = -1;
            Bias = null;
            InternalWeight = null;
            TopologyType = ReservoirTopologyType.Random;
            TopologySettings = null;
            RetainmentNeuronsFeature = false;
            RetainmentNeuronsDensity = 0;
            RetainmentMinRate = 0;
            RetainmentMaxRate = 0;
            ContextNeuronFeature = false;
            ContextNeuronActivation = null;
            ContextNeuronInputWeight = null;
            ContextNeuronFeedbackDensity = 0;
            ContextNeuronFeedbackWeight = null;
            FeedbackFeature = false;
            FeedbackConnectionDensity = 0;
            FeedbackWeight = null;
            FeedbackFieldNameCollection = null;
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public AnalogReservoirSettings(AnalogReservoirSettings source)
        {
            SettingsName = source.SettingsName;
            InputConnectionDensity = source.InputConnectionDensity;
            InputWeight = source.InputWeight.DeepClone();
            Size = source.Size;
            ReservoirActivation = source.ReservoirActivation.DeepClone();
            SpectralRadius = source.SpectralRadius;
            Bias = source.Bias.DeepClone();
            InternalWeight = source.InternalWeight.DeepClone();
            TopologyType = source.TopologyType;
            if (source.TopologySettings != null)
            {
                if (source.TopologySettings.GetType() == typeof(RandomTopologySettings))
                {
                    TopologySettings = new RandomTopologySettings((RandomTopologySettings)source.TopologySettings);
                }
                if (source.TopologySettings.GetType() == typeof(RingTopologySettings))
                {
                    TopologySettings = new RingTopologySettings((RingTopologySettings)source.TopologySettings);
                }
                if (source.TopologySettings.GetType() == typeof(DTTTopologySettings))
                {
                    TopologySettings = new DTTTopologySettings((DTTTopologySettings)source.TopologySettings);
                }
            }
            RetainmentNeuronsFeature = source.RetainmentNeuronsFeature;
            RetainmentNeuronsDensity = source.RetainmentNeuronsDensity;
            RetainmentMinRate = source.RetainmentMinRate;
            RetainmentMaxRate = source.RetainmentMaxRate;

            ContextNeuronActivation = null;
            ContextNeuronInputWeight = null;
            ContextNeuronFeedbackDensity = 0;
            ContextNeuronFeedbackWeight = null;
            ContextNeuronFeature = source.ContextNeuronFeature;
            if (source.ContextNeuronFeature)
            {
                ContextNeuronActivation = source.ContextNeuronActivation.DeepClone();
                ContextNeuronInputWeight = new RandomValueSettings(source.ContextNeuronInputWeight);
                ContextNeuronFeedbackDensity = source.ContextNeuronFeedbackDensity;
                ContextNeuronFeedbackWeight = new RandomValueSettings(source.ContextNeuronFeedbackWeight);
            }
            FeedbackConnectionDensity = 0;
            FeedbackWeight = null;
            FeedbackFieldNameCollection = null;
            FeedbackFeature = source.FeedbackFeature;
            if (source.FeedbackFeature)
            {
                FeedbackConnectionDensity = source.FeedbackConnectionDensity;
                FeedbackWeight = new RandomValueSettings(source.FeedbackWeight);
                FeedbackFieldNameCollection = new List<string>(source.FeedbackFieldNameCollection);
            }
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// This is the preferred way to instantiate reservoir settings.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing reservoir settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public AnalogReservoirSettings(XElement elem)
        {
            //Validation
            ElemValidator validator = new ElemValidator();
            Assembly assemblyRCNet = Assembly.GetExecutingAssembly();
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.Neural.Analog.Reservoir.AnalogReservoirSettings.xsd");
            validator.AddXsdFromResources(assemblyRCNet, "RCNet.RCNetTypes.xsd");
            XElement reservoirSettingsElem = validator.Validate(elem, "rootElem");
            //Parsing
            //Settings name
            SettingsName = reservoirSettingsElem.Attribute("name").Value;
            //Input
            XElement inputElem = reservoirSettingsElem.Descendants("input").First();
            InputConnectionDensity = double.Parse(inputElem.Attribute("connectionDensity").Value, CultureInfo.InvariantCulture);
            InputWeight = new RandomValueSettings(inputElem.Descendants("weight").First());
            //Internal
            XElement internalElem = reservoirSettingsElem.Descendants("internal").First();
            Size = int.Parse(internalElem.Attribute("size").Value);
            ReservoirActivation = new AnalogActivationSettings(internalElem.Descendants("activation").First());
            SpectralRadius = internalElem.Attribute("spectralRadius").Value == "NA" ? -1d : double.Parse(internalElem.Attribute("spectralRadius").Value, CultureInfo.InvariantCulture);
            Bias = new RandomValueSettings(internalElem.Descendants("bias").First());
            InternalWeight = new RandomValueSettings(internalElem.Descendants("weight").First());
            //Topology
            List<XElement> topologyElems = new List<XElement>();
            topologyElems.AddRange(internalElem.Descendants("topologyRandom"));
            topologyElems.AddRange(internalElem.Descendants("topologyRing"));
            topologyElems.AddRange(internalElem.Descendants("topologyDTT"));
            if(topologyElems.Count != 1)
            {
                throw new Exception("Only one reservoir topology can be specified in reservoir settings.");
            }
            if (topologyElems.Count == 0)
            {
                throw new Exception("Reservoir topology is not specified in reservoir settings.");
            }
            XElement topologyElem = topologyElems[0];
            //Random?
            if (topologyElem.Name == "topologyRandom")
            {
                TopologyType = ReservoirTopologyType.Random;
                TopologySettings = new RandomTopologySettings(topologyElem);
            }
            //Ring?
            else if (topologyElem.Name == "topologyRing")
            {
                TopologyType = ReservoirTopologyType.Ring;
                TopologySettings = new RingTopologySettings(topologyElem);
            }
            else
            {
                //DTT
                TopologyType = ReservoirTopologyType.DTT;
                TopologySettings = new DTTTopologySettings(topologyElem);
            }
            //Retainment neurons
            XElement retainmentElem = internalElem.Descendants("retainmentNeurons").FirstOrDefault();
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
            //Context neuron
            XElement ctxNeuronElem = internalElem.Descendants("contextNeuron").FirstOrDefault();
            ContextNeuronFeature = (ctxNeuronElem != null);
            if (ContextNeuronFeature)
            {
                ContextNeuronActivation = new AnalogActivationSettings(ctxNeuronElem.Descendants("activation").First());
                ContextNeuronInputWeight = new RandomValueSettings(ctxNeuronElem.Descendants("inputWeight").First());
                ContextNeuronFeedbackDensity = double.Parse(ctxNeuronElem.Attribute("feedbackDensity").Value, CultureInfo.InvariantCulture);
                ContextNeuronFeedbackWeight = new RandomValueSettings(ctxNeuronElem.Descendants("feedbackWeight").First());
                ContextNeuronFeature = (ContextNeuronFeedbackDensity > 0 &&
                                        ContextNeuronInputWeight.Active &&
                                        ContextNeuronFeedbackWeight.Active
                                        );
            }
            else
            {
                ContextNeuronActivation = null;
                ContextNeuronInputWeight = null;
                ContextNeuronFeedbackDensity = 0;
                ContextNeuronFeedbackWeight = null;
            }
            //Feedback
            XElement feedbackElem = reservoirSettingsElem.Descendants("feedback").FirstOrDefault();
            FeedbackFeature = (feedbackElem != null);
            FeedbackFieldNameCollection = new List<string>();
            if (FeedbackFeature)
            {
                FeedbackConnectionDensity = double.Parse(feedbackElem.Attribute("density").Value, CultureInfo.InvariantCulture);
                FeedbackWeight = new RandomValueSettings(feedbackElem.Descendants("weight").First());
                FeedbackFieldNameCollection = new List<string>();
                foreach (XElement feedbackFieldElem in feedbackElem.Descendants("feedbackFields").First().Descendants("field"))
                {
                    FeedbackFieldNameCollection.Add(feedbackFieldElem.Attribute("name").Value);
                }
                FeedbackFeature = (FeedbackFieldNameCollection.Count > 0);
            }
            else
            {
                FeedbackConnectionDensity = 0;
                FeedbackWeight = null;
                FeedbackFieldNameCollection = null;
            }
            return;
        }

        //Methods
        //Static methods
        /// <summary>
        /// Parses string code to ReservoirTopologyType.
        /// </summary>
        /// <param name="code">Topology code</param>
        /// <returns></returns>
        public static ReservoirTopologyType ParseReservoirTopology(string code)
        {
            switch (code.ToUpper())
            {
                case "RANDOM": return ReservoirTopologyType.Random;
                case "RING": return ReservoirTopologyType.Ring;
                case "DTT": return ReservoirTopologyType.DTT;
                default:
                    throw new ArgumentException($"Unknown reservoir's topology code {code}");
            }
        }

        //Instance methods
        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            AnalogReservoirSettings cmpSettings = obj as AnalogReservoirSettings;
            if (SettingsName != cmpSettings.SettingsName ||
                InputConnectionDensity != cmpSettings.InputConnectionDensity ||
                !Equals(InputWeight, cmpSettings.InputWeight) ||
                Size != cmpSettings.Size ||
                !Equals(ReservoirActivation, cmpSettings.ReservoirActivation) ||
                SpectralRadius != cmpSettings.SpectralRadius ||
                !Equals(InternalWeight, cmpSettings.InternalWeight) ||
                !Equals(Bias, cmpSettings.Bias) ||
                TopologyType != cmpSettings.TopologyType ||
                !Equals(TopologySettings, cmpSettings.TopologySettings) ||
                RetainmentNeuronsFeature != cmpSettings.RetainmentNeuronsFeature ||
                RetainmentNeuronsDensity != cmpSettings.RetainmentNeuronsDensity ||
                RetainmentMinRate != cmpSettings.RetainmentMinRate ||
                RetainmentMaxRate != cmpSettings.RetainmentMaxRate ||
                ContextNeuronFeature != cmpSettings.ContextNeuronFeature ||
                FeedbackFeature != cmpSettings.FeedbackFeature
                )
            {
                return false;
            }
            if (ContextNeuronFeature)
            {
                if (ContextNeuronFeedbackDensity != cmpSettings.ContextNeuronFeedbackDensity ||
                    !Equals(ContextNeuronActivation, cmpSettings.ContextNeuronActivation) ||
                    !Equals(ContextNeuronInputWeight, cmpSettings.ContextNeuronInputWeight) ||
                    !Equals(ContextNeuronFeedbackWeight, cmpSettings.ContextNeuronFeedbackWeight)
                    )
                {
                    return false;
                }
            }
            if(FeedbackFeature)
            {
                if(FeedbackConnectionDensity != cmpSettings.FeedbackConnectionDensity ||
                   !Equals(FeedbackWeight, cmpSettings.FeedbackWeight) ||
                   !FeedbackFieldNameCollection.ToArray().ContainsEqualValues(cmpSettings.FeedbackFieldNameCollection.ToArray())
                   )
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return SettingsName.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public AnalogReservoirSettings DeepClone()
        {
            AnalogReservoirSettings clone = new AnalogReservoirSettings(this);
            return clone;
        }


        //Inner classes
        /// <summary>
        /// Additional setup parameters for Random reservoir topology
        /// </summary>
        [Serializable]
        public class RandomTopologySettings
        {
            //Attributes
            /// <summary>
            /// The parameter says how many interconnections from all possible interconnections will be used.
            /// Count = Size * Size * Density
            /// </summary>
            public double ConnectionsDensity { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public RandomTopologySettings()
            {
                ConnectionsDensity = 0;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public RandomTopologySettings(RandomTopologySettings source)
            {
                ConnectionsDensity = source.ConnectionsDensity;
                return;
            }

            /// <summary>
            /// Creates the instance itialized from xml
            /// </summary>
            public RandomTopologySettings(XElement randomTopologyElem)
            {
                ConnectionsDensity = double.Parse(randomTopologyElem.Attribute("connectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                RandomTopologySettings cmpSettings = obj as RandomTopologySettings;
                if (ConnectionsDensity != cmpSettings.ConnectionsDensity)
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

        }//RandomTopologySettings

        /// <summary>
        /// Additional setup parameters for Ring reservoir topology
        /// </summary>
        [Serializable]
        public class RingTopologySettings
        {
            //Attributes
            /// <summary>
            /// The parameter specifies whether the ring interconnection will be bidirectional.
            /// </summary>
            public bool Bidirectional { get; set; }
            /// <summary>
            /// The parameter says how many neurons in the reservoir will receive the signal from itself.
            /// Count = Size * Density
            /// </summary>
            public double SelfConnectionsDensity { get; set; }
            /// <summary>
            /// The parameter says how many additional interconnections from all possible interconnections will be used.
            /// Count = Size * Size * Density
            /// </summary>
            public double InterConnectionsDensity { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public RingTopologySettings()
            {
                Bidirectional = false;
                SelfConnectionsDensity = 0;
                InterConnectionsDensity = 0;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public RingTopologySettings(RingTopologySettings source)
            {
                Bidirectional = source.Bidirectional;
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                InterConnectionsDensity = source.InterConnectionsDensity;
                return;
            }

            /// <summary>
            /// Creates the instance itialized from xml
            /// </summary>
            public RingTopologySettings(XElement ringTopologyElem)
            {
                Bidirectional = bool.Parse(ringTopologyElem.Attribute("bidirectional").Value);
                SelfConnectionsDensity = double.Parse(ringTopologyElem.Attribute("selfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                InterConnectionsDensity = double.Parse(ringTopologyElem.Attribute("interConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                RingTopologySettings cmpSettings = obj as RingTopologySettings;
                if (Bidirectional != cmpSettings.Bidirectional ||
                   SelfConnectionsDensity != cmpSettings.SelfConnectionsDensity ||
                   InterConnectionsDensity != cmpSettings.InterConnectionsDensity
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

        }//RingTopologySettings

        /// <summary>
        /// Additional setup parameters for DTT reservoir topology
        /// </summary>
        [Serializable]
        public class DTTTopologySettings
        {
            //Attributes
            /// <summary>
            /// The parameter says how many neurons in the reservoir will receive the signal from itself.
            /// Count = Size * Density
            /// </summary>
            public double SelfConnectionsDensity { get; set; }

            //Constructors
            /// <summary>
            /// Creates an unitialized instance
            /// </summary>
            public DTTTopologySettings()
            {
                SelfConnectionsDensity = 0;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public DTTTopologySettings(DTTTopologySettings source)
            {
                SelfConnectionsDensity = source.SelfConnectionsDensity;
                return;
            }

            /// <summary>
            /// Creates the instance itialized from xml
            /// </summary>
            public DTTTopologySettings(XElement dttTopologyElem)
            {
                SelfConnectionsDensity = double.Parse(dttTopologyElem.Attribute("selfConnectionsDensity").Value, CultureInfo.InvariantCulture);
                return;
            }

            //Methods
            /// <summary>
            /// See the base.
            /// </summary>
            public override bool Equals(object obj)
            {
                if (obj == null) return false;
                DTTTopologySettings cmpSettings = obj as DTTTopologySettings;
                if (SelfConnectionsDensity != cmpSettings.SelfConnectionsDensity)
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

        }//DTTTopologySettings

    }//AnalogReservoirSettings

}//Namespace

