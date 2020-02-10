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
        /// Index of the field among NP input fields
        /// </summary>
        public int InputFieldIndex { get; set; }

        /// <summary>
        /// Entry point of the input
        /// </summary>
        public int[] InputEntryPoint { get; set; }

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
        /// <param name="inputFieldIndex">Index of the field among NP input fields</param>
        /// <param name="inputEntryPoint">Entry point of the input</param>
        /// <param name="spikeTrainLength">Length of the spike-train representation of analog value</param>
        /// <param name="differenceDistance">Parameter of the realtime transformation "Difference". Distance of the past value for the computation of the difference of the current and the past value.</param>
        /// <param name="numOfLinearSteps">Parameter of the realtime transformation "LinearSteps". Number of steps dividing data interval.</param>
        /// <param name="powerExponent">Parameter of the realtime transformation "Power". The exponent.</param>
        /// <param name="foldedPowerExponent">Parameter of the realtime transformation "FoldedPower". The exponent.</param>
        /// <param name="movingAverageLength">Parameter of the realtime transformation "MovingAverage". Number of the last data values involved in MovingAverage.</param>
        public InputUnitSettings(string inputFieldName,
                                 int inputFieldIndex,
                                 int[] inputEntryPoint,
                                 int spikeTrainLength = DefaultSpikeTrainLength,
                                 int differenceDistance = DefaultDifferenceDistance,
                                 int numOfLinearSteps = DefaultNumOfLinearSteps,
                                 double powerExponent = DefaultPowerExponent,
                                 double foldedPowerExponent = DefaultFoldedPowerExponent,
                                 int movingAverageLength = DefaultMovingAverageLength
                                 )
        {
            InputFieldName = inputFieldName;
            InputFieldIndex = inputFieldIndex;
            InputEntryPoint = (int[])inputEntryPoint.Clone();
            SpikeTrainLength = spikeTrainLength;
            DifferenceDistance = differenceDistance;
            NumOfLinearSteps = numOfLinearSteps;
            PowerExponent = powerExponent;
            FoldedPowerExponent = foldedPowerExponent;
            MovingAverageLength = movingAverageLength;
            return;
        }

        /// <summary>
        /// Creates an initialized instance from the given xml element.
        /// </summary>
        /// <param name="inputFieldIndex">Index of the field among NP input fields</param>
        /// <param name="inputEntryPoint">Entry point of the input</param>
        /// <param name="elem">
        /// Xml data containing the settings.
        /// Content of xml element is always validated against the xml schema.
        /// </param>
        public InputUnitSettings(int inputFieldIndex,
                                 int[] inputEntryPoint,
                                 XElement elem
                                 )
        {
            InputFieldIndex = inputFieldIndex;
            InputEntryPoint = (int[])inputEntryPoint.Clone();
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
            return;
        }

        /// <summary>
        /// The deep copy constructor.
        /// </summary>
        /// <param name="source">Source instance</param>
        public InputUnitSettings(InputUnitSettings source)
        {
            InputFieldName = source.InputFieldName;
            InputFieldIndex = source.InputFieldIndex;
            InputEntryPoint = (int[])source.InputEntryPoint.Clone();
            SpikeTrainLength = source.SpikeTrainLength;
            DifferenceDistance = source.DifferenceDistance;
            NumOfLinearSteps = source.NumOfLinearSteps;
            PowerExponent = source.PowerExponent;
            FoldedPowerExponent = source.FoldedPowerExponent;
            MovingAverageLength = source.MovingAverageLength;
            return;
        }

        //Methods
        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        /// <returns></returns>
        public InputUnitSettings DeepClone()
        {
            return new InputUnitSettings(this);
        }

    }//InputUnitSettings

}//Namespace

