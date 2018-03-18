using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Xml;
using OKOSW.Extensions;
using OKOSW.Neural.Activation;
using OKOSW.Neural.Reservoir.Analog;



namespace OKOSW.Neural.Networks.EchoState
{
    /// <summary>Echo State Network general settings</summary>
    [Serializable]
    public class ESNSettings
    {
        //Properties
        /// <summary>
        /// RandomizerSeek greater or equal to 0 causes the same "Random" initialization (important for parameters tuning and results comparableness).
        /// Specify randomizerSeek less than 0 to get different initialization of Random class every time (and also different results).
        /// </summary>
        public int RandomizerSeek { get; set; }
        /// <summary>Count of input fields.</summary>
        public List<string> InputFieldsNames { get; set; }
        /// <summary>List of all mappings of input field(s) to appropriate reservoir configuration</summary>
        public List<InputResCfgMap> InputsToResCfgsMapping { get; set; }
        /// <summary>If true, unmodified input values will be added as a part of the regression predictors</summary>
        public bool RouteInputToReadout { get; set; }
        /// <summary>Number of neurons in hidden layers of the read out FF network</summary>
        public List<ReadOutHiddenLayerCfg> ReadOutHiddenLayers { get; set; }
        /// <summary>Readout FF network output neuron activation function.</summary>
        public ActivationFactory.EnumActivationType OutputNeuronActivation { get; set; }
        /// <summary>Regression method (LM or RESILIENT).</summary>
        public string RegressionMethod { get; set; }
        /// <summary>Maximum number of regression attempts.</summary>
        public int RegressionMaxAttempts { get; set; }
        /// <summary>Maximum number of iterations to find output ESN weights.</summary>
        public int RegressionMaxEpochs { get; set; }
        /// <summary>Regression will be stopped after the specified MSE on training dataset will be reached.</summary>
        public double RegressionStopMSEValue { get; set; }

        //Constructors
        /// <summary>Creates ESN setup parameters initialized by default values</summary>
        public ESNSettings()
        {
            //Default settings
            RandomizerSeek = 0; //Default is setting for debug
            InputFieldsNames = new List<string>();
            InputsToResCfgsMapping = new List<InputResCfgMap>();
            RouteInputToReadout = true;
            ReadOutHiddenLayers = new List<ReadOutHiddenLayerCfg>();
            OutputNeuronActivation = ActivationFactory.EnumActivationType.Identity; //Standard
            RegressionMethod = "LINEAR";
            RegressionMaxAttempts = 1; //Standard
            RegressionMaxEpochs = 50; //Usually enough value is 100
            RegressionStopMSEValue = 1E-15; //Usually does not make sense to continue regression after reaching MSE 1E-15
            return;
        }

        /// <summary>Creates ESN setup parameters initialized as a values copy of specified source ESN settings</summary>
        public ESNSettings(ESNSettings source)
        {
            //Copy
            RandomizerSeek = source.RandomizerSeek;
            InputFieldsNames = source.InputFieldsNames;
            InputsToResCfgsMapping = new List<InputResCfgMap>(source.InputsToResCfgsMapping.Count);
            foreach (InputResCfgMap mapping in source.InputsToResCfgsMapping)
            {
                InputsToResCfgsMapping.Add(mapping.Clone());
            }
            RouteInputToReadout = source.RouteInputToReadout;
            ReadOutHiddenLayers = new List<ReadOutHiddenLayerCfg>(source.ReadOutHiddenLayers.Count);
            foreach (ReadOutHiddenLayerCfg hiddenLayerCfg in source.ReadOutHiddenLayers)
            {
                ReadOutHiddenLayers.Add(hiddenLayerCfg.Clone());
            }
            OutputNeuronActivation = source.OutputNeuronActivation;
            RegressionMethod = source.RegressionMethod;
            RegressionMaxAttempts = source.RegressionMaxAttempts;
            RegressionMaxEpochs = source.RegressionMaxEpochs;
            RegressionStopMSEValue = source.RegressionStopMSEValue;
            return;
        }

        /// <summary>Creates ESN setup parameters initialized from XML</summary>
        public ESNSettings(XmlNode xmlNode)
        {
            RandomizerSeek = int.Parse(xmlNode.Attributes["RandomizerSeek"].Value);
            //Input
            XmlNode inputNode = xmlNode.SelectSingleNode("Input");
            RouteInputToReadout = bool.Parse(inputNode.Attributes["RouteToReadout"].Value);
            InputFieldsNames = new List<string>();
            foreach(XmlNode inputFieldNode in inputNode.SelectNodes("Field"))
            {
                InputFieldsNames.Add(inputFieldNode.Attributes["Name"].Value);
            }
            //Readout
            XmlNode readoutNode = xmlNode.SelectSingleNode("Readout");
            OutputNeuronActivation = ActivationFactory.ParseActivation(readoutNode.Attributes["Activation"].Value);
            //Hidden layers
            ReadOutHiddenLayers = new List<ReadOutHiddenLayerCfg>();
            foreach (XmlNode layerNode in readoutNode.SelectNodes("Layer"))
            {
                ReadOutHiddenLayers.Add(new ReadOutHiddenLayerCfg(layerNode.Attributes["Neurons"].Value, layerNode.Attributes["Activation"].Value));
            }
            //Regression
            XmlNode regressionNode = xmlNode.SelectSingleNode("Regression");
            RegressionMethod = regressionNode.Attributes["RegressionMethod"].Value.ToUpper();
            RegressionMaxAttempts = int.Parse(regressionNode.Attributes["MaxAttempts"].Value);
            RegressionMaxEpochs = int.Parse(regressionNode.Attributes["MaxEpochs"].Value);
            RegressionStopMSEValue = double.Parse(regressionNode.Attributes["StopMSE"].Value, CultureInfo.InvariantCulture);
            //Reservoirs and mapping to input fields
            InputsToResCfgsMapping = new List<InputResCfgMap>();
            foreach (XmlNode resNode in xmlNode.SelectNodes("Reservoir"))
            {
                AnalogReservoirSettings resCfg = new AnalogReservoirSettings(resNode);
                List<string> resFields = new List<string>();
                XmlNode resInputNode = resNode.SelectSingleNode("Input");
                foreach(XmlNode resInputFieldNode in resInputNode.SelectNodes("Field"))
                {
                    resFields.Add(resInputFieldNode.Attributes["Name"].Value);
                }
                List<int> resFieldsIdxs = new List<int>();
                foreach (string fieldName in resFields)
                {
                    int fieldIdx = InputFieldsNames.IndexOf(fieldName);
                    if (fieldIdx == -1)
                    {
                        throw new Exception("Reservoir configuration " + resCfg.CfgName + ": unknown input field name " + fieldName);
                    }
                    resFieldsIdxs.Add(fieldIdx);
                }
                bool aloneReservoirPerInputField = bool.Parse(resNode.Attributes["Multiple"].Value);
                if (!aloneReservoirPerInputField)
                {
                    //All specified input fields will be mixed into the one reservoir
                    InputResCfgMap mapping = new InputResCfgMap(resFieldsIdxs, resCfg);
                    InputsToResCfgsMapping.Add(mapping);
                }
                else
                {
                    //Each specified input field will have its own reservoir instance
                    foreach (int inpFieldIdx in resFieldsIdxs)
                    {
                        List<int> singleInpIdxs = new List<int>();
                        singleInpIdxs.Add(inpFieldIdx);
                        InputResCfgMap mapping = new InputResCfgMap(singleInpIdxs, resCfg);
                        InputsToResCfgsMapping.Add(mapping);
                    }
                }
            }
            return;
        }

        //Properties
        /// <summary>Number of input fields.</summary>
        public int InputFieldsCount { get { return InputFieldsNames.Count; } }


        //Methods
        /// <summary>Checkes if this settings are equivalent to specified settings</summary>
        /// <param name="cmpSettings">Settings to be compared with this settings</param>
        public bool IsEquivalent(ESNSettings cmpSettings)
        {
            if (RandomizerSeek != cmpSettings.RandomizerSeek ||
               !InputFieldsNames.ToArray().EqualValues(cmpSettings.InputFieldsNames.ToArray()) ||
               InputsToResCfgsMapping.Count != cmpSettings.InputsToResCfgsMapping.Count ||
               RouteInputToReadout != cmpSettings.RouteInputToReadout ||
               OutputNeuronActivation != cmpSettings.OutputNeuronActivation ||
               RegressionMethod != cmpSettings.RegressionMethod ||
               RegressionMaxAttempts != cmpSettings.RegressionMaxAttempts ||
               RegressionMaxEpochs != cmpSettings.RegressionMaxEpochs ||
               RegressionStopMSEValue != cmpSettings.RegressionStopMSEValue ||
               ReadOutHiddenLayers.Count != cmpSettings.ReadOutHiddenLayers.Count
               )
            {
                return false;
            }
            for (int i = 0; i < InputsToResCfgsMapping.Count; i++)
            {
                if (!InputsToResCfgsMapping[i].IsEquivalent(cmpSettings.InputsToResCfgsMapping[i]))
                {
                    return false;
                }
            }
            for (int i = 0; i < ReadOutHiddenLayers.Count; i++)
            {
                if (!ReadOutHiddenLayers[i].IsEquivalent(cmpSettings.ReadOutHiddenLayers[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Method returns the new instance of this instance as a copy.
        /// </summary>
        /// <returns></returns>
        public ESNSettings Clone()
        {
            ESNSettings clone = new ESNSettings(this);
            return clone;
        }


        //Inner classes
        [Serializable]
        public class ReadOutHiddenLayerCfg
        {
            //Attributes
            public int NeuronsCount { get; set; }
            public ActivationFactory.EnumActivationType ActivationType { get; set; }

            //Constructor
            public ReadOutHiddenLayerCfg(string neuronsCount, string activationType)
            {
                NeuronsCount = int.Parse(neuronsCount);
                ActivationType = ActivationFactory.ParseActivation(activationType);
                return;
            }

            public ReadOutHiddenLayerCfg(ReadOutHiddenLayerCfg source)
            {
                NeuronsCount = source.NeuronsCount;
                ActivationType = source.ActivationType;
                return;
            }

            public bool IsEquivalent(ReadOutHiddenLayerCfg cmpItem)
            {
                if (NeuronsCount != cmpItem.NeuronsCount || ActivationType != cmpItem.ActivationType)
                {
                    return false;
                }
                return true;
            }

            public ReadOutHiddenLayerCfg Clone()
            {
                return new ReadOutHiddenLayerCfg(this);
            }
        }//ReadOutHiddenLayerCfg

        /// <summary>
        /// Mapping of ESN input field(s) to reservoir configuration
        /// </summary>
        [Serializable]
        public class InputResCfgMap
        {
            //Attributes
            public List<int> InputFieldsIdxs;
            public AnalogReservoirSettings ReservoirSettings;

            //Constructor
            public InputResCfgMap(List<int> inputFieldsIdxs = null, AnalogReservoirSettings reservoirSettings = null)
            {
                if (inputFieldsIdxs == null)
                {
                    InputFieldsIdxs = new List<int>();
                }
                else
                {
                    InputFieldsIdxs = new List<int>(inputFieldsIdxs);
                }
                ReservoirSettings = reservoirSettings;
                return;
            }
            public InputResCfgMap(InputResCfgMap source)
            {
                InputFieldsIdxs = new List<int>(source.InputFieldsIdxs);
                ReservoirSettings = new AnalogReservoirSettings(source.ReservoirSettings);
                return;
            }
            //Methods
            public InputResCfgMap Clone()
            {
                return new InputResCfgMap(this);
            }

            public bool IsEquivalent(InputResCfgMap cmpSettings)
            {
                if (InputFieldsIdxs.Count != cmpSettings.InputFieldsIdxs.Count ||
                    !ReservoirSettings.IsEquivalent(cmpSettings.ReservoirSettings)
                    )
                {
                    return false;
                }
                for (int i = 0; i < InputFieldsIdxs.Count; i++)
                {
                    if (InputFieldsIdxs[i] != cmpSettings.InputFieldsIdxs[i])
                    {
                        return false;
                    }
                }
                return true;
            }
        }



    }//ESNSettings
}//Namespace
