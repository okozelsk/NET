using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// The collection of the readout units configurations
    /// </summary>
    [Serializable]
    public class ReadoutUnitsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "ROutLayerUnitsType";

        //Attribute properties
        /// <summary>
        /// Collection of of readout unit settings
        /// </summary>
        public List<ReadoutUnitSettings> ReadoutUnitCfgCollection { get; }

        /// <summary>
        /// Dictionary of "one winner" groups
        /// </summary>
        public Dictionary<string, OneWinnerGroup> OneWinnerGroupCollection { get; private set; }


        //Constructors
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="readoutUnitsCfgs">Collection of readout unit settings</param>
        public ReadoutUnitsSettings(IEnumerable<ReadoutUnitSettings> readoutUnitsCfgs)
        {
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            foreach (ReadoutUnitSettings rucfg in readoutUnitsCfgs)
            {
                ReadoutUnitCfgCollection.Add((ReadoutUnitSettings)rucfg.DeepClone());
            }
            Check();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="readoutUnitsCfgs">Readout layer settings</param>
        public ReadoutUnitsSettings(params ReadoutUnitSettings[] readoutUnitsCfgs)
            : this(readoutUnitsCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor
        /// </summary>
        /// <param name="source">Source instance</param>
        public ReadoutUnitsSettings(ReadoutUnitsSettings source)
            : this(source.ReadoutUnitCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">Xml element containing the initialization settings</param>
        public ReadoutUnitsSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            foreach (XElement unitElem in settingsElem.Elements("readoutUnit"))
            {
                ReadoutUnitCfgCollection.Add(new ReadoutUnitSettings(unitElem));
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
            if (ReadoutUnitCfgCollection.Count == 0)
            {
                throw new ArgumentException($"Collection of readout units settings can not be empty.", "ReadoutUnitCfgCollection");
            }
            //Uniqueness of readout units names
            string[] names = new string[ReadoutUnitCfgCollection.Count];
            names[0] = ReadoutUnitCfgCollection[0].Name;
            for (int i = 1; i < ReadoutUnitCfgCollection.Count; i++)
            {
                if (names.Contains(ReadoutUnitCfgCollection[i].Name))
                {
                    throw new ArgumentException($"Readout unit name {ReadoutUnitCfgCollection[i].Name} is not unique.", "ReadoutUnitCfgCollection");
                }
                names[i] = ReadoutUnitCfgCollection[i].Name;
            }
            //One winner groups
            OneWinnerGroupCollection = new Dictionary<string, OneWinnerGroup>();
            List<string> owgs = new List<string>();
            for (int index = 0; index < ReadoutUnitCfgCollection.Count; index++)
            {
                if (ReadoutUnitCfgCollection[index].TaskCfg.Type == ReadoutUnit.TaskType.Classification)
                {
                    ClassificationTaskSettings cts = (ClassificationTaskSettings)ReadoutUnitCfgCollection[index].TaskCfg;
                    if (cts.OneWinnerGroupName != ClassificationTaskSettings.DefaultOneWinnerGroupName)
                    {
                        if (owgs.IndexOf(cts.OneWinnerGroupName) == -1)
                        {
                            owgs.Add(cts.OneWinnerGroupName);
                        }
                    }
                }
            }
            foreach (string oneWinnerGroupName in owgs)
            {
                OneWinnerGroupCollection.Add(oneWinnerGroupName,
                                             new OneWinnerGroup(oneWinnerGroupName,
                                                                (from rus in ReadoutUnitCfgCollection where rus.TaskCfg.Type == ReadoutUnit.TaskType.Classification && ((ClassificationTaskSettings)rus.TaskCfg).OneWinnerGroupName == oneWinnerGroupName select rus).ToList()
                                                                )
                                             );
            }

            return;
        }

        /// <summary>
        /// Returns ID (zero based index) of the readout unit having the given name
        /// </summary>
        /// <param name="readoutUnitName">Readout unit name</param>
        public int GetReadoutUnitID(string readoutUnitName)
        {
            for (int i = 0; i < ReadoutUnitCfgCollection.Count; i++)
            {
                if (ReadoutUnitCfgCollection[i].Name == readoutUnitName)
                {
                    return i;
                }
            }
            throw new InvalidOperationException($"Readout unit name {readoutUnitName} not found.");
        }

        /// <summary>
        /// Returns configuration of the readout unit having the given name
        /// </summary>
        /// <param name="readoutUnitName">Readout unit name</param>
        /// <returns></returns>
        public ReadoutUnitSettings GetReadoutunitCfg(string readoutUnitName)
        {
            return ReadoutUnitCfgCollection[GetReadoutUnitID(readoutUnitName)];
        }

        /// <inheritdoc />
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutUnitsSettings(this);
        }

        /// <inheritdoc />
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName);
            foreach (ReadoutUnitSettings rucfg in ReadoutUnitCfgCollection)
            {
                rootElem.Add(rucfg.GetXml(suppressDefaults));
            }
            Validate(rootElem, XsdTypeName);
            return rootElem;
        }

        /// <inheritdoc />
        public override XElement GetXml(bool suppressDefaults)
        {
            return GetXml("readoutUnits", suppressDefaults);
        }

        //Inner classes
        /// <summary>
        /// The "one winner" group name and members
        /// </summary>
        [Serializable]
        public class OneWinnerGroup
        {
            /// <summary>
            /// Name of the group
            /// </summary>
            public string Name { get; }
            /// <summary>
            /// Indexes of member readout units
            /// </summary>
            public List<ReadoutUnitSettings> Members { get; }

            //Constructors
            /// <summary>
            /// Creates initialized instance
            /// </summary>
            /// <param name="name">One winner group name</param>
            /// <param name="members">Member readout units</param>
            public OneWinnerGroup(string name, List<ReadoutUnitSettings> members)
            {
                Name = name;
                Members = members;
                return;
            }

            /// <summary>
            /// Copy constructor
            /// </summary>
            /// <param name="source">Source instance</param>
            public OneWinnerGroup(OneWinnerGroup source)
            {
                Name = source.Name;
                Members = new List<ReadoutUnitSettings>(source.Members.Count);
                foreach (ReadoutUnitSettings rus in source.Members)
                {
                    Members.Add((ReadoutUnitSettings)rus.DeepClone());
                }
                return;
            }

            //Methods
            /// <summary>
            /// Creates the deep copy instance of this instance.
            /// </summary>
            public OneWinnerGroup DeepClone()
            {
                return new OneWinnerGroup(this);
            }


        }//OneWinnerGroup


    }//ReadoutUnitsSettings

}//Namespace
