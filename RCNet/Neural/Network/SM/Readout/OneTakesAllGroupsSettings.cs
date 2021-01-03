using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the "One Takes All" groups.
    /// </summary>
    [Serializable]
    public class OneTakesAllGroupsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutOneTakesAllGroupsType";

        //Attribute properties
        /// <summary>
        /// The collection of the "One Takes All group" configurations.
        /// </summary>
        public List<OneTakesAllGroupSettings> OneTakesAllGroupCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="oneTakesAllGroupsCfgs">The collection of the "One Takes All group" configurations.</param>
        public OneTakesAllGroupsSettings(IEnumerable<OneTakesAllGroupSettings> oneTakesAllGroupsCfgs)
        {
            OneTakesAllGroupCfgCollection = new List<OneTakesAllGroupSettings>();
            foreach (OneTakesAllGroupSettings cfg in oneTakesAllGroupsCfgs)
            {
                OneTakesAllGroupCfgCollection.Add((OneTakesAllGroupSettings)cfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="oneTakesAllGroupsCfgs">The "One Takes All group" configurations.</param>
        public OneTakesAllGroupsSettings(params OneTakesAllGroupSettings[] oneTakesAllGroupsCfgs)
            : this(oneTakesAllGroupsCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public OneTakesAllGroupsSettings(OneTakesAllGroupsSettings source)
            : this(source.OneTakesAllGroupCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
        public OneTakesAllGroupsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            OneTakesAllGroupCfgCollection = new List<OneTakesAllGroupSettings>();
            foreach (XElement groupElem in settingsElem.Elements("group"))
            {
                OneTakesAllGroupCfgCollection.Add(new OneTakesAllGroupSettings(groupElem));
            }
            Check();
            return;
        }

        //Properties
        /// <inheritdoc />
        public override bool ContainsOnlyDefaults { get { return false; } }

        //Methods
        /// <inheritdoc />
        protected override void Check()
        {
            if (OneTakesAllGroupCfgCollection.Count == 0)
            {
                throw new ArgumentException($"Collection of the One Takes All group configurations can not be empty.", "OneTakesAllGroupCfgCollection");
            }
            //Uniqueness of the group names
            string[] names = new string[OneTakesAllGroupCfgCollection.Count];
            names[0] = OneTakesAllGroupCfgCollection[0].Name;
            for (int i = 1; i < OneTakesAllGroupCfgCollection.Count; i++)
            {
                if (names.Contains(OneTakesAllGroupCfgCollection[i].Name))
                {
                    throw new ArgumentException($"Group name {OneTakesAllGroupCfgCollection[i].Name} is not unique.", "OneTakesAllGroupCfgCollection");
                }
                names[i] = OneTakesAllGroupCfgCollection[i].Name;
            }

            return;
        }

        /// <summary>
        /// Gets an identifier (zero based index) of the "One Takes All" group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        public int GetOneTakesAllGroupID(string groupName)
        {
            for (int i = 0; i < OneTakesAllGroupCfgCollection.Count; i++)
            {
                if (OneTakesAllGroupCfgCollection[i].Name == groupName)
                {
                    return i;
                }
            }
            throw new InvalidOperationException($"Group name {groupName} not found.");
        }

        /// <summary>
        /// Gets the configuration of the "One Takes All" group.
        /// </summary>
        /// <param name="groupName">The name of the group.</param>
        public OneTakesAllGroupSettings GetOneTakesAllGroupCfg(string groupName)
        {
            return OneTakesAllGroupCfgCollection[GetOneTakesAllGroupID(groupName)];
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new OneTakesAllGroupsSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (OneTakesAllGroupSettings cfg in OneTakesAllGroupCfgCollection)
            {
                rootElem.Add(cfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("oneTakesAllGroups", suppressDefaults);
        }

    }//OneTakesAllGroupsSettings

}//Namespace
