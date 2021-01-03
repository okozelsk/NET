using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Configuration of the readout units.
    /// </summary>
    [Serializable]
    public class ReadoutUnitsSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// The name of the associated xsd type.
        /// </summary>
        public const string XsdTypeName = "ROutUnitsType";

        //Attribute properties
        /// <summary>
        /// The collection of the readout unit configurations.
        /// </summary>
        public List<ReadoutUnitSettings> ReadoutUnitCfgCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="readoutUnitsCfgs">The collection of the readout unit configurations.</param>
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
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="readoutUnitsCfgs">The readout unit configurations.</param>
        public ReadoutUnitsSettings(params ReadoutUnitSettings[] readoutUnitsCfgs)
            : this(readoutUnitsCfgs.AsEnumerable())
        {
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
        public ReadoutUnitsSettings(ReadoutUnitsSettings source)
            : this(source.ReadoutUnitCfgCollection)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="elem">A xml element containing the configuration data.</param>
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

            return;
        }

        /// <summary>
        /// Gets an identifier (zero-based index) of the specified readout unit.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
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
        /// Gets the configuration of the specified readout unit.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
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

    }//ReadoutUnitsSettings

}//Namespace
