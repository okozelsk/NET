using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using RCNet.Extensions;
using RCNet.MathTools.Probability;
using RCNet.XmlTools;
using RCNet.RandomValue;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir.Pool.NeuronGroup
{
    /// <summary>
    /// Collection of pool's neuron groups settings
    /// </summary>
    [Serializable]
    public class NeuronGroupsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "PoolNeuronGroupsType";

        //Attribute properties
        /// <summary>
        /// Collection of neuron groups settings
        /// </summary>
        public List<INeuronGroupSettings> GroupCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        private NeuronGroupsSettings()
        {
            GroupCfgCollection = new List<INeuronGroupSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="groupCfgCollection">Schema settings</param>
        public NeuronGroupsSettings(IEnumerable<INeuronGroupSettings> groupCfgCollection)
            : this()
        {
            AddGroups(groupCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="groupCfgCollection">Group settings</param>
        public NeuronGroupsSettings(params INeuronGroupSettings[] groupCfgCollection)
            : this()
        {
            AddGroups(groupCfgCollection);
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public NeuronGroupsSettings(NeuronGroupsSettings source)
            : this()
        {
            AddGroups(source.GroupCfgCollection);
            return;
        }

        /// <summary>
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">Xml data containing settings.</param>
        public NeuronGroupsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            GroupCfgCollection = new List<INeuronGroupSettings>();
            foreach (XElement schemaElem in settingsElem.Descendants())
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
        /// <summary>
        /// Identifies settings containing only default values
        /// </summary>
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Checks validity
        /// </summary>
        private void Check()
        {
            if (GroupCfgCollection.Count == 0)
            {
                throw new Exception($"At least one group must be specified.");
            }
            return;
        }

        /// <summary>
        /// Adds cloned group settings from given collection into the internal collection
        /// </summary>
        /// <param name="groupCfgCollection"></param>
        private void AddGroups(IEnumerable<INeuronGroupSettings> groupCfgCollection)
        {
            foreach (INeuronGroupSettings schemaCfg in groupCfgCollection)
            {
                GroupCfgCollection.Add((INeuronGroupSettings)schemaCfg.DeepClone());
            }
            return;
        }

        /// <summary>
        /// Based on given target neurons total count sets appropriate neurons
        /// sub-counts for neuron groups.
        /// </summary>
        /// <param name="targetTotalCount">Target neurons total count</param>
        public void SetGrpNeuronsSubCounts(int targetTotalCount)
        {
            //Compute sum
            double sumRelShare = 0;
            foreach (INeuronGroupSettings grp in GroupCfgCollection)
            {
                sumRelShare += grp.RelShare;
            }
            //First distribution of sub-counts
            int[] subCounts = new int[GroupCfgCollection.Count];
            int distributedCount = 0;
            for(int i = 0; i < GroupCfgCollection.Count; i++)
            {
                double ratio = GroupCfgCollection[i].RelShare / sumRelShare;
                subCounts[i] = (int)Math.Round(((double)targetTotalCount) * ratio, 0);
                distributedCount += subCounts[i];
            }
            //Sub-counts finetuning
            while (distributedCount != targetTotalCount)
            {
                //Correction of sub-counts
                int sign = Math.Sign(targetTotalCount - distributedCount);
                int index = sign < 0 ? subCounts.IndexOfMax() : subCounts.IndexOfMin();
                subCounts[index] += sign;
                distributedCount += sign;
                if (subCounts[index] < 0)
                {
                    throw new Exception("Can't set proper neuron counts for the neuron groups.");
                }
            }
            //Set sub-counts
            for (int i = 0; i < GroupCfgCollection.Count; i++)
            {
                GroupCfgCollection[i].Count = subCounts[i];
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new NeuronGroupsSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
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

        /// <summary>
        /// Generates default named xml element containing the settings.
        /// </summary>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("neuronGroups", suppressDefaults);
        }

    }//PoolNeuronGroupsSettings

}//Namespace
