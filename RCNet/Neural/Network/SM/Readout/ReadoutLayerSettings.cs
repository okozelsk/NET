using RCNet.Neural.Network.NonRecurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the readout layer.
    /// </summary>
    [Serializable]
    public class ReadoutLayerSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutLayerType";

        //Attribute properties
        /// <summary>
        /// The default configurations of the readout unit tasks.
        /// </summary>
        public TaskDefaultsSettings TaskDefaultsCfg { get; }

        /// <summary>
        /// The configuration of the readout units.
        /// </summary>
        public ReadoutUnitsSettings ReadoutUnitsCfg { get; }

        /// <summary>
        /// The configuration of the "One Takes All" groups.
        /// </summary>
        public OneTakesAllGroupsSettings OneTakesAllGroupsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="taskDefaultsCfg">The default configurations of the readout unit tasks.</param>
        /// <param name="readoutUnitsCfg">The configuration of the readout units.</param>
        /// <param name="oneTakesAllGroupsCfg">The configuration of the "One Takes All" groups.</param>
        public ReadoutLayerSettings(TaskDefaultsSettings taskDefaultsCfg,
                                    ReadoutUnitsSettings readoutUnitsCfg,
                                    OneTakesAllGroupsSettings oneTakesAllGroupsCfg = null
                                    )
        {
            TaskDefaultsCfg = (TaskDefaultsSettings)taskDefaultsCfg.DeepClone();
            ReadoutUnitsCfg = (ReadoutUnitsSettings)readoutUnitsCfg.DeepClone();
            OneTakesAllGroupsCfg = (OneTakesAllGroupsSettings)oneTakesAllGroupsCfg?.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReadoutLayerSettings(ReadoutLayerSettings source)
            : this(source.TaskDefaultsCfg, source.ReadoutUnitsCfg, source.OneTakesAllGroupsCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public ReadoutLayerSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            //Cluster
            TaskDefaultsCfg = new TaskDefaultsSettings(settingsElem.Element("taskDefaults"));
            //Readout units
            XElement readoutUnitsElem = settingsElem.Elements("readoutUnits").First();
            ReadoutUnitsCfg = new ReadoutUnitsSettings(readoutUnitsElem);
            //One-takes-all groups
            XElement oneTakesAllGroupsElem = settingsElem.Elements("oneTakesAllGroups").FirstOrDefault();
            OneTakesAllGroupsCfg = oneTakesAllGroupsElem == null ? null : new OneTakesAllGroupsSettings(oneTakesAllGroupsElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Gets the collection of output field names.
        /// </summary>
        public List<string> OutputFieldNameCollection
        {
            get
            {
                return (from rus in ReadoutUnitsCfg.ReadoutUnitCfgCollection select rus.Name).ToList();
            }
        }

        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <summary>
        /// Gets the readout unit's specific or default forecast cluster chain configuration.
        /// </summary>
        /// <param name="readoutUnitID">The zero-based index of the readout unit.</param>
        /// <returns>The forecast cluster chain configuration.</returns>
        public TNRNetClusterChainRealSettings GetRUnitForecastClusterChainCfg(int readoutUnitID)
        {
            TNRNetClusterChainRealSettings resultCfg = ((ForecastTaskSettings)ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitID].TaskCfg).ClusterChainCfg;
            if (resultCfg == null)
            {
                //Use task default
                resultCfg = TaskDefaultsCfg.ForecastClusterChainCfg;
                if (resultCfg == null)
                {
                    //Not defined
                    throw new ArgumentException($"For the readout unit {ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitID].Name} there is not available the forecast cluster chain configuration (specific nor default).", "readoutUnitID");
                }
            }
            return resultCfg;
        }

        /// <summary>
        /// Gets the readout unit's specific or default forecast cluster chain configuration.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
        /// <returns>The forecast cluster chain configuration.</returns>
        public TNRNetClusterChainRealSettings GetRUnitForecastClusterChainCfg(string readoutUnitName)
        {
            return GetRUnitForecastClusterChainCfg(ReadoutUnitsCfg.GetReadoutUnitID(readoutUnitName));
        }

        /// <summary>
        /// Gets the readout unit's specific or default classification cluster chain configuration.
        /// </summary>
        /// <param name="readoutUnitID">The zero-based index of the readout unit.</param>
        /// <returns>The classification cluster chain configuration.</returns>
        public TNRNetClusterChainSingleBoolSettings GetRUnitTaskClassificationClusterChainCfg(int readoutUnitID)
        {
            TNRNetClusterChainSingleBoolSettings resultCfg = ((ClassificationTaskSettings)ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitID].TaskCfg).ClusterChainCfg;
            if (resultCfg == null)
            {
                //Use task default
                resultCfg = TaskDefaultsCfg.ClassificationClusterChainCfg;
                if (resultCfg == null)
                {
                    //Not defined
                    throw new ArgumentException($"For the readout unit {ReadoutUnitsCfg.ReadoutUnitCfgCollection[readoutUnitID].Name} there is not available the classification cluster chain configuration (specific nor default).", "readoutUnitID");
                }
            }
            return resultCfg;
        }

        /// <summary>
        /// Gets the readout unit's specific or default classification cluster chain configuration.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
        /// <returns>The classification cluster chain configuration.</returns>
        public TNRNetClusterChainSingleBoolSettings GetRUnitTaskClassificationClusterChainCfg(string readoutUnitName)
        {
            return GetRUnitTaskClassificationClusterChainCfg(ReadoutUnitsCfg.GetReadoutUnitID(readoutUnitName));
        }

        /// <summary>
        /// Gets the indexes of the readout units belonging to a specified "One Takes All" group.
        /// </summary>
        /// <param name="groupName">The name of the "One Takes All" group.</param>
        public List<int> GetOneTakesAllGroupMemberRUnitIndexes(string groupName)
        {
            List<int> indexes = new List<int>();
            for (int i = 0; i < ReadoutUnitsCfg.ReadoutUnitCfgCollection.Count; i++)
            {
                if (ReadoutUnitsCfg.ReadoutUnitCfgCollection[i].TaskCfg.Type == ReadoutUnit.TaskType.Classification)
                {
                    ClassificationTaskSettings taskCfg = (ClassificationTaskSettings)ReadoutUnitsCfg.ReadoutUnitCfgCollection[i].TaskCfg;
                    if (taskCfg.OneTakesAllGroupName == groupName)
                    {
                        indexes.Add(i);
                    }
                }
            }
            return indexes;
        }

        /// <inheritdoc />
        protected override void Check()
        {
            foreach (ReadoutUnitSettings rus in ReadoutUnitsCfg.ReadoutUnitCfgCollection)
            {
                //Check that each one of readout units can get the appropriate result configuration
                if (rus.TaskCfg.Type == ReadoutUnit.TaskType.Forecast)
                {
                    //Check forecast task result can be assigned
                    GetRUnitForecastClusterChainCfg(rus.Name);
                }
                else
                {
                    //Check classification task result can be assigned
                    GetRUnitTaskClassificationClusterChainCfg(rus.Name);
                    //Check that each specified One Takes All group name exists in groups configuration
                    string oneTakesAllGroupName = ((ClassificationTaskSettings)rus.TaskCfg).OneTakesAllGroupName;
                    if (oneTakesAllGroupName != ClassificationTaskSettings.DefaultOneTakesAllGroupName)
                    {
                        if (OneTakesAllGroupsCfg == null)
                        {
                            //Not defined
                            throw new ArgumentException($"One Takes All group name {oneTakesAllGroupName} specified in readout unit {rus.Name} is not defined.", "readoutUnitID");
                        }
                        OneTakesAllGroupsCfg.GetOneTakesAllGroupID(oneTakesAllGroupName);
                    }
                }
            }
            if (OneTakesAllGroupsCfg != null)
            {
                //Check at least two readout units within the group
                foreach (string name in (from Group in OneTakesAllGroupsCfg.OneTakesAllGroupCfgCollection select Group.Name))
                {
                    if (GetOneTakesAllGroupMemberRUnitIndexes(name).Count < 2)
                    {
                        throw new ArgumentException($"One Takes All group name {name} has less than 2 member readout units.", "One Takes All group");
                    }
                }
            }
            return;
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutLayerSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, TaskDefaultsCfg.GetXml(suppressDefaults));
            rootElem.Add(ReadoutUnitsCfg.GetXml(suppressDefaults));
            if (OneTakesAllGroupsCfg != null)
            {
                rootElem.Add(OneTakesAllGroupsCfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("readoutLayer", suppressDefaults);
        }


    }//ReadoutLayerSettings

}//Namespace
