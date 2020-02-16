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

namespace RCNet.Neural.Network.SM.Preprocessing
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
        public const string XsdTypeName = "NPResInstanceInputUnitCfgType";
        /// <summary>
        /// Default length of the spike-train representation of analog value
        /// </summary>
        public const int DefaultSpikeTrainLength = 8;
        /// <summary>
        /// Default value of parameter of the realtime transformation "Difference". Distance of the past value for the computation of the difference of the current and the past value.
        /// </summary>
        public const int DefaultDifferenceDistance = 1;
        /// <summary>
        /// Default value of parameter of the realtime transformation "LinearSteps". Number of steps dividing data interval.
        /// </summary>
        public const int DefaultNumOfLinearSteps = 16;
        /// <summary>
        /// Default value of parameter of the realtime transformation "Power". The exponent.
        /// </summary>
        public const double DefaultPowerExponent = 0.5d;
        /// <summary>
        /// Default value of parameter of the realtime transformation "FoldedPower". The exponent.
        /// </summary>
        public const double DefaultFoldedPowerExponent = 0.5d;
        /// <summary>
        /// Default value of parameter of the realtime transformation "MovingAverage". Number of the last data values involved in MovingAverage.
        /// </summary>
        public const int DefaultMovingAverageLength = 5;


        /// <summary>
        /// Name of the input field
        /// </summary>
        public string InputFieldName { get; set; }

        /// <summary>
        /// Length of the spike-train representation of analog value
        /// </summary>
        public int SpikeTrainLength { get; set; }

        /// <summary>
        /// Parameter of the realtime transformation "Difference". Distance of the past value for the computation of the difference of the current and the past value.
        /// </summary>
        public int DifferenceDistance { get; set; }

        /// <summary>
        /// Parameter of the realtime transformation "LinearSteps". Number of steps dividing data interval.
        /// </summary>
        public int NumOfLinearSteps { get; set; }

        /// <summary>
        /// Parameter of the realtime transformation "Power". The exponent.
        /// </summary>
        public double PowerExponent { get; set; }

        /// <summary>
        /// Parameter of the realtime transformation "FoldedPower". The exponent.
        /// </summary>
        public double FoldedPowerExponent { get; set; }

        /// <summary>
        /// Parameter of the realtime transformation "MovingAverage". Number of the last data values involved in MovingAverage.
        /// </summary>
        public int MovingAverageLength { get; set; }


        //Constructors
        /// <summary>
        /// Creates an itialized instance.
        /// </summary>
        /// <param name="inputFieldName">Name of the input field</param>
        /// <param name="spikeTrainLength">Length of the spike-train representation of analog value</param>
        /// <param name="differenceDistance">Parameter of the realtime transformation "Difference". Distance of the past value for the computation of the difference of the current and the past value.</param>
        /// <param name="numOfLinearSteps">Parameter of the realtime transformation "LinearSteps". Number of steps dividing data interval.</param>
        /// <param name="powerExponent">Parameter of the realtime transformation "Power". The exponent.</param>
        /// <param name="foldedPowerExponent">Parameter of the realtime transformation "FoldedPower". The exponent.</param>
        /// <param name="movingAverageLength">Parameter of the realtime transformation "MovingAverage". Number of the last data values involved in MovingAverage.</param>
        public InputUnitSettings(string inputFieldName,
                                 int spikeTrainLength = DefaultSpikeTrainLength,
                                 int differenceDistance = DefaultDifferenceDistance,
                                 int numOfLinearSteps = DefaultNumOfLinearSteps,
                                 double powerExponent = DefaultPowerExponent,
                                 double foldedPowerExponent = DefaultFoldedPowerExponent,
                                 int movingAverageLength = DefaultMovingAverageLength
                                 )
        {
            InputFieldName = inputFieldName;
            SpikeTrainLength = spikeTrainLength;
            DifferenceDistance = differenceDistance;
            NumOfLinearSteps = numOfLinearSteps;
            PowerExponent = powerExponent;
            FoldedPowerExponent = foldedPowerExponent;
            MovingAverageLength = movingAverageLength;
            Check();
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputUnitSettings(InputUnitSettings source)
        {
            InputFieldName = source.InputFieldName;
            SpikeTrainLength = source.SpikeTrainLength;
            DifferenceDistance = source.DifferenceDistance;
            NumOfLinearSteps = source.NumOfLinearSteps;
            PowerExponent = source.PowerExponent;
            FoldedPowerExponent = source.FoldedPowerExponent;
            MovingAverageLength = source.MovingAverageLength;
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
            XElement inputUnitSettingsElem = Validate(elem, XsdTypeName);
            //Parsing
            InputFieldName = inputUnitSettingsElem.Attribute("fieldName").Value;
            SpikeTrainLength = int.Parse(inputUnitSettingsElem.Attribute("spikeTrainLength").Value, CultureInfo.InvariantCulture);
            DifferenceDistance = int.Parse(inputUnitSettingsElem.Attribute("differenceDistance").Value, CultureInfo.InvariantCulture);
            NumOfLinearSteps = int.Parse(inputUnitSettingsElem.Attribute("numOfLinearSteps").Value, CultureInfo.InvariantCulture);
            PowerExponent = double.Parse(inputUnitSettingsElem.Attribute("powerExponent").Value, CultureInfo.InvariantCulture);
            FoldedPowerExponent = double.Parse(inputUnitSettingsElem.Attribute("foldedPowerExponent").Value, CultureInfo.InvariantCulture);
            MovingAverageLength = int.Parse(inputUnitSettingsElem.Attribute("movingAverageLength").Value, CultureInfo.InvariantCulture);
            Check();
            return;
        }

        //Properties
        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultSpikeTrainLength { get { return (SpikeTrainLength == DefaultSpikeTrainLength); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultDifferenceDistance { get { return (DifferenceDistance == DefaultDifferenceDistance); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultNumOfLinearSteps { get { return (NumOfLinearSteps == DefaultNumOfLinearSteps); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultPowerExponent { get { return (PowerExponent == DefaultPowerExponent); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultFoldedPowerExponent { get { return (FoldedPowerExponent == DefaultFoldedPowerExponent); } }

        /// <summary>
        /// Checks if settings are default
        /// </summary>
        public bool IsDefaultMovingAverageLength { get { return (MovingAverageLength == DefaultMovingAverageLength); } }

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
            if (DifferenceDistance < 1 || DifferenceDistance > 1024)
            {
                throw new Exception($"Invalid DifferenceDistance {DifferenceDistance.ToString(CultureInfo.InvariantCulture)}. DifferenceDistance must be GE to 1 and LE to 1024.");
            }
            if (NumOfLinearSteps < 2)
            {
                throw new Exception($"Invalid NumOfLinearSteps {NumOfLinearSteps.ToString(CultureInfo.InvariantCulture)}. NumOfLinearSteps must be GE to 2.");
            }
            if (PowerExponent <= 0)
            {
                throw new Exception($"Invalid PowerExponent {PowerExponent.ToString(CultureInfo.InvariantCulture)}. PowerExponent must be GT 0.");
            }
            if (FoldedPowerExponent <= 0)
            {
                throw new Exception($"Invalid FoldedPowerExponent {FoldedPowerExponent.ToString(CultureInfo.InvariantCulture)}. FoldedPowerExponent must be GT 0.");
            }
            if (MovingAverageLength < 1 || MovingAverageLength > 1024)
            {
                throw new Exception($"Invalid MovingAverageLength {MovingAverageLength.ToString(CultureInfo.InvariantCulture)}. MovingAverageLength must be GE to 1 and LE to 1024.");
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
            XElement rootElem = new XElement(rootElemName, new XAttribute("fieldName", InputFieldName));
            if (!suppressDefaults || !IsDefaultSpikeTrainLength)
            {
                rootElem.Add(new XAttribute("spikeTrainLength", SpikeTrainLength.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultDifferenceDistance)
            {
                rootElem.Add(new XAttribute("differenceDistance", DifferenceDistance.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultNumOfLinearSteps)
            {
                rootElem.Add(new XAttribute("numOfLinearSteps", NumOfLinearSteps.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultPowerExponent)
            {
                rootElem.Add(new XAttribute("powerExponent", PowerExponent.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultFoldedPowerExponent)
            {
                rootElem.Add(new XAttribute("foldedPowerExponent", FoldedPowerExponent.ToString(CultureInfo.InvariantCulture)));
            }
            if (!suppressDefaults || !IsDefaultMovingAverageLength)
            {
                rootElem.Add(new XAttribute("movingAverageLength", MovingAverageLength.ToString(CultureInfo.InvariantCulture)));
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
            return GetXml("inputUnit", suppressDefaults);
        }

    }//InputUnitSettings

}//Namespace

