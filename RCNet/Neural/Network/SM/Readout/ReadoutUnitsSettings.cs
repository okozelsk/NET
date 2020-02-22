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
using RCNet.Neural.Network.NonRecurrent;
using RCNet.Neural.Network.NonRecurrent.FF;
using RCNet.Neural.Network.NonRecurrent.PP;
using RCNet.Neural.Data.Filter;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Collection of readout unit settings
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
        private ReadoutUnitsSettings()
        {
            ReadoutUnitCfgCollection = new List<ReadoutUnitSettings>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="readoutUnitsCfgs">Collection of readout unit settings</param>
        public ReadoutUnitsSettings(IEnumerable<ReadoutUnitSettings> readoutUnitsCfgs)
            : this()
        {
            foreach (ReadoutUnitSettings rucfg in readoutUnitsCfgs)
            {
                ReadoutUnitCfgCollection.Add((ReadoutUnitSettings)rucfg.DeepClone());
            }
            CheckAndComplete();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="readoutUnitsCfgs">Readout layer settings</param>
        public ReadoutUnitsSettings(params ReadoutUnitSettings[] readoutUnitsCfgs)
            : this()
        {
            foreach (ReadoutUnitSettings rucfg in readoutUnitsCfgs)
            {
                ReadoutUnitCfgCollection.Add((ReadoutUnitSettings)rucfg.DeepClone());
            }
            CheckAndComplete();
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
        /// Creates the instance and initialize it from given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public ReadoutUnitsSettings(XElement elem)
            : this()
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            int index = 0;
            foreach (XElement unitElem in settingsElem.Descendants("readoutUnit"))
            {
                ReadoutUnitCfgCollection.Add(new ReadoutUnitSettings(index++, unitElem));
            }
            CheckAndComplete();
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
        private void CheckAndComplete()
        {
            if(ReadoutUnitCfgCollection.Count == 0)
            {
                throw new Exception($"Collection of readout units settings can not be empty.");
            }
            //Uniqueness of readout units names
            string[] names = new string[ReadoutUnitCfgCollection.Count];
            names[0] = ReadoutUnitCfgCollection[0].Name;
            for (int i = 1; i < ReadoutUnitCfgCollection.Count; i++)
            {
                if (names.Contains(ReadoutUnitCfgCollection[i].Name))
                {
                    throw new Exception($"Readout unit name {ReadoutUnitCfgCollection[i].Name} is not unique.");
                }
                names[i] = ReadoutUnitCfgCollection[i].Name;
            }
            //One winner groups
            OneWinnerGroupCollection = new Dictionary<string, OneWinnerGroup>();
            List<string> owgs = new List<string>();
            for (int index = 0; index < ReadoutUnitCfgCollection.Count; index++)
            {
                if(ReadoutUnitCfgCollection[index].Index != index)
                {
                    throw new Exception($"Inconsistent indexes of readout units.");
                }
                if(ReadoutUnitCfgCollection[index].TaskCfg.Type == ReadoutUnit.TaskType.Classification)
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
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new ReadoutUnitsSettings(this);
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
            foreach (ReadoutUnitSettings rucfg in ReadoutUnitCfgCollection)
            {
                rootElem.Add(rucfg.GetXml(suppressDefaults));
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
