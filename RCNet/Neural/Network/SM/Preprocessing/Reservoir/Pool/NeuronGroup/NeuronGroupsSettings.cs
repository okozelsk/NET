using RCNet.Extensions;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Configuration of the neuron group configurations.
    /// </summary>
    [Serializable]
    public class NeuronGroupsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "PoolNeuronGroupsType";

        //Attribute properties
        /// <summary>
        /// The collection of neuron group configurations.
        /// </summary>
        public List<INeuronGroupSettings> GroupCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        private NeuronGroupsSettings()
        {
            GroupCfgCollection = new List<INeuronGroupSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="groupCfgCollection">The collection of neuron group configurations.</param>
        public NeuronGroupsSettings(IEnumerable<INeuronGroupSettings> groupCfgCollection)
            : this()
        {
            AddGroups(groupCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="groupCfgCollection">The neuron group configurations.</param>
        public NeuronGroupsSettings(params INeuronGroupSettings[] groupCfgCollection)
            : this()
        {
            AddGroups(groupCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public NeuronGroupsSettings(NeuronGroupsSettings source)
            : this()
        {
            AddGroups(source.GroupCfgCollection);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public NeuronGroupsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            GroupCfgCollection = new List<INeuronGroupSettings>();
            foreach (XElement schemaElem in settingsElem.Elements())
            {
                if (schemaElem.Name.LocalName == "analogGroup")
                {
                    GroupCfgCollection.Add(new AnalogNeuronGroupSettings(schemaElem));
                }
                else if (schemaElem.Name.LocalName == "spikingGroup")
                {
                    GroupCfgCollection.Add(new SpikingNeuronGroupSettings(schemaElem));
                }
                else
                {
                    //Ignore
                    ;
                }
            }
            Check();
            return;
        }

        //Properties
        /// <inheritdoc/>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc/>
        protected override void Check()
        {
            if (GroupCfgCollection.Count == 0)
            {
                throw new ArgumentException($"At least one group must be specified.", "GroupCfgCollection");
            }
            return;
        }

        /// <summary>
        /// Adds the group configurations from the specified collection into the internal collection.
        /// </summary>
        /// <param name="groupCfgCollection">The collection of neuron group configurations.</param>
        private void AddGroups(IEnumerable<INeuronGroupSettings> groupCfgCollection)
        {
            foreach (INeuronGroupSettings schemaCfg in groupCfgCollection)
            {
                GroupCfgCollection.Add((INeuronGroupSettings)schemaCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Distributes the specified total number of neurons over the neuron groups.
        /// </summary>
        /// <param name="totalNumOfNeurons">The total number of neurons.</param>
        public void SetGrpNeuronsSubCounts(int totalNumOfNeurons)
        {
            //Compute sum
            double sumRelShare = 0;
            foreach (INeuronGroupSettings grp in GroupCfgCollection)
            {
                sumRelShare += grp.RelShare;
            }
            //The first distribution of sub-counts
            int[] subCounts = new int[GroupCfgCollection.Count];
            int distributedCount = 0;
            for (int i = 0; i < GroupCfgCollection.Count; i++)
            {
                double ratio = GroupCfgCollection[i].RelShare / sumRelShare;
                subCounts[i] = (int)Math.Round(((double)totalNumOfNeurons) * ratio, 0);
                distributedCount += subCounts[i];
            }
            //Sub-counts finetuning
            while (distributedCount != totalNumOfNeurons)
            {
                //Correction of sub-counts
                int sign = Math.Sign(totalNumOfNeurons - distributedCount);
                int index = sign < 0 ? subCounts.IndexOfMax() : subCounts.IndexOfMin();
                subCounts[index] += sign;
                distributedCount += sign;
                if (subCounts[index] < 0)
                {
                    throw new InvalidOperationException($"Can't set proper neuron counts for the neuron groups.");
                }
            }
            //Set sub-counts
            for (int i = 0; i < GroupCfgCollection.Count; i++)
            {
                GroupCfgCollection[i].Count = subCounts[i];
            }
            return;
        }

        /// <inheritdoc/>
        public override RCNetBaseSettings DeepClone()
        {
            return new NeuronGroupsSettings(this);
        }

        /// <inheritdoc/>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (INeuronGroupSettings groupCfg in GroupCfgCollection)
            {
                rootElem.Add(groupCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc/>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("neuronGroups", suppressDefaults);
        }

    }//NeuronGroupsSettings

}//Namespace
