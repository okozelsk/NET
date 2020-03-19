using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RCNet.Extensions;
using RCNet.XmlTools;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Space3D;

namespace RCNet.Neural.Network.SM.Preprocessing.Reservoir
{
    /// <summary>
    /// Configuration of an input unit associated with the input field
    /// </summary>
    [Serializable]
    public class InputUnitSettings : RCNetBaseSettings
    {
        //Constants
        /// <summary>
        /// Name of the associated xsd type
        /// </summary>
        public const string XsdTypeName = "NPResInstanceInputUnitType";
        /// <summary>
        /// Default length of the spike-train representation of analog value
        /// </summary>
        public const int DefaultSpikeTrainLength = 8;


        /// <summary>
        /// Name of the input field
        /// </summary>
        public string InputFieldName { get; }

        /// <summary>
        /// Length of the spike-train representation of analog value
        /// </summary>
        public int SpikeTrainLength { get; }

        /// <summary>
        /// Input entry point coordinates within the 3D space
        /// </summary>
        public CoordinatesSettings CoordinatesCfg { get; }

        /// <summary>
        /// Input unit connections
        /// </summary>
        public InputUnitConnsSettings ConnsCfg { get; }

        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="inputFieldName">Name of the input field</param>
        /// <param name="connsCfg">Input unit connections</param>
        /// <param name="spikeTrainLength">Length of the spike-train representation of analog value</param>
        /// <param name="coordinatesCfg">Input entry point coordinates within the 3D space</param>
        public InputUnitSettings(string inputFieldName,
                                 InputUnitConnsSettings connsCfg,
                                 int spikeTrainLength = DefaultSpikeTrainLength,
                                 CoordinatesSettings coordinatesCfg = null
                                 )
        {
            InputFieldName = inputFieldName;
            ConnsCfg = (InputUnitConnsSettings)connsCfg.DeepClone();
            SpikeTrainLength = spikeTrainLength;
            CoordinatesCfg = coordinatesCfg == null ? new CoordinatesSettings() : (CoordinatesSettings)coordinatesCfg.DeepClone();
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputUnitSettings(InputUnitSettings source)
            :this(source.InputFieldName, source.ConnsCfg, source.SpikeTrainLength, source.CoordinatesCfg)
        {
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public InputUnitSettings(XElement elem)
        {
            //Validation
            XElement settingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = settingsElem.Attribute("inputFieldName").Value;
            SpikeTrainLength = int.Parse(settingsElem.Attribute("spikeTrainLength").Value, CultureInfo.InvariantCulture);
            ConnsCfg = new InputUnitConnsSettings(settingsElem.Descendants("connections").First());
            XElement coordinatesElem = settingsElem.Descendants("coordinates").FirstOrDefault();
            CoordinatesCfg = coordinatesElem == null ? new CoordinatesSettings() : new CoordinatesSettings(coordinatesElem);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpikeTrainLength { get { return (SpikeTrainLength == DefaultSpikeTrainLength); } }

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
            if(InputFieldName.Length == 0)
            {
                throw new Exception($"Empty InputFieldName.");
            }
            if (SpikeTrainLength < 1 || SpikeTrainLength > 32)
            {
                throw new Exception($"Invalid SpikeTrainLength {SpikeTrainLength.ToString(CultureInfo.InvariantCulture)}. SpikeTrainLength must be GE to 1 and LE to 32.");
            }
            return;
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public override RCNetBaseSettings DeepClone()
        {
            return new InputUnitSettings(this);
        }

        /// <summary>
        /// Generates xml element containing the settings.
        /// </summary>
        /// <param name="rootElemName">Name to be used as a name of the root element.</param>
        /// <param name="suppressDefaults">Specifies if to ommit optional nodes having set default values</param>
        /// <returns>XElement containing the settings</returns>
        public override XElement GetXml(string rootElemName, bool suppressDefaults)
        {
            XElement rootElem = new XElement(rootElemName, new XAttribute("inputFieldName", InputFieldName));
            if (!suppressDefaults || !IsDefaultSpikeTrainLength)
            {
                rootElem.Add(new XAttribute("spikeTrainLength", SpikeTrainLength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !CoordinatesCfg.ContainsOnlyDefaults)
            {
                rootElem.Add(CoordinatesCfg.GetXml(suppressDefaults));
            }
            rootElem.Add(ConnsCfg.GetXml(suppressDefaults));
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
            return GetXml("inputUnit", suppressDefaults);
        }

    }//InputUnitSettings

}//Namespace

