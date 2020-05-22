using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Data.Transformers;
using RCNet.Neural.Network.SM.Preprocessing.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Neuron.Predictor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Input processing unit
    /// </summary>
    [Serializable]
    public class InputEncoder
    {
        //Constants
        /// <summary>
        /// ID of the input encoder's reservoir
        /// </summary>
        public const int ReservoirID = -1;
        
        /// <summary>
        /// ID of the input encoder's pool
        /// </summary>
        public const int PoolID = -1;

        //Enumerations
        /// <summary>
        /// Input feeding variants
        /// </summary>
        public enum InputFeedingType
        {
            /// <summary>
            /// Continuous feeding
            /// </summary>
            Continuous,
            /// <summary>
            /// Patterned feeding
            /// </summary>
            Patterned
        }

        //Static attributes
        /// <summary>
        /// Input data will be normalized by feature filters to this range before the usage in the reservoirs
        /// </summary>
        private static readonly Interval _dataRange = new Interval(-1, 1);

        //Attribute properties
        /// <summary>
        /// Collection of input fields
        /// </summary>
        public List<InputField> Fields { get; }

        /// <summary>
        /// Fields to be routed to readout
        /// </summary>
        public List<InputField> RoutedFieldCollection { get; }

        /// <summary>
        /// Number of input field values routed to readout
        /// </summary>
        public int NumOfRoutedFieldValues { get; private set; }

        //Attributes
        /// <summary>
        /// Configuration
        /// </summary>
        private readonly InputEncoderSettings _encoderCfg;

        /// <summary>
        /// Collection of the internal input transformers associated with the transformed input fields
        /// </summary>
        private readonly List<ITransformer> _internalInputTransformerCollection;

        /// <summary>
        /// Collection of the internal input generators associated with the generated input fields
        /// </summary>
        private readonly List<IGenerator> _internalInputGeneratorCollection;

        /// <summary>
        /// Required constant length of the external input vector for patterned feeding with routing input to readout.
        /// -1 (no constraint) for continuous feeding or patterned feeding without routing input to readout.
        /// </summary>
        private int _fixedExtVectorLength;
        
        /// <summary>
        /// Data to be processed
        /// </summary>
        private readonly List<double[]> _inputData;

        /// <summary>
        /// Number of already processed inputs from _inputData
        /// </summary>
        private int _numOfProcessedInputs;

        /// <summary>
        /// Indicates reverse mode of input data processing
        /// </summary>
        private bool _reverseMode;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="inputEncoderCfg">Configuration of input encoder</param>
        public InputEncoder(InputEncoderSettings inputEncoderCfg)
        {
            _encoderCfg = (InputEncoderSettings)inputEncoderCfg.DeepClone();
            //Fields
            Fields = new List<InputField>(_encoderCfg.FieldsCfg.TotalNumOfFields);
            int[] coordinates = _encoderCfg.CoordinatesCfg.GetCoordinates();
            int fieldIdx = 0;
            int inputNeuronStartIdx = 0;
            //External fields
            foreach (ExternalFieldSettings fieldCfg in _encoderCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                Fields.Add(new InputField(fieldCfg.Name,
                                          fieldIdx++,
                                          coordinates,
                                          _dataRange,
                                          fieldCfg.FeatureFilterCfg,
                                          fieldCfg.SpikeCodeCfg,
                                          (fieldCfg.RouteToReadout && _encoderCfg.FeedingCfg.RouteToReadout),
                                          inputNeuronStartIdx
                                          ));
                inputNeuronStartIdx += Fields[fieldIdx - 1].NumOfInputNeurons;
            }
            //Internal input transformers and fields
            _internalInputTransformerCollection = new List<ITransformer>();
            if (_encoderCfg.FieldsCfg.TransformedFieldsCfg != null)
            {
                List<string> names = _encoderCfg.FieldsCfg.GetNames();
                foreach (TransformedFieldSettings fieldCfg in _encoderCfg.FieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                {
                    _internalInputTransformerCollection.Add(TransformerFactory.Create(names, fieldCfg.TransformerCfg));
                    Fields.Add(new InputField(fieldCfg.Name,
                                              fieldIdx++,
                                              coordinates,
                                              _dataRange,
                                              fieldCfg.FeatureFilterCfg,
                                              fieldCfg.SpikingCodingCfg,
                                              (fieldCfg.RouteToReadout && _encoderCfg.FeedingCfg.RouteToReadout),
                                              inputNeuronStartIdx
                                              ));
                    inputNeuronStartIdx += Fields[fieldIdx - 1].NumOfInputNeurons;
                }
            }
            //Internal input generators and fields
            _internalInputGeneratorCollection = new List<IGenerator>();
            if (_encoderCfg.FieldsCfg.GeneratedFieldsCfg != null)
            {
                foreach (GeneratedFieldSettings fieldCfg in _encoderCfg.FieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                {
                    _internalInputGeneratorCollection.Add(GeneratorFactory.Create(fieldCfg.GeneratorCfg));
                    Fields.Add(new InputField(fieldCfg.Name,
                                              fieldIdx++,
                                              coordinates,
                                              _dataRange,
                                              fieldCfg.FeatureFilterCfg,
                                              fieldCfg.SpikingCodingCfg,
                                              (fieldCfg.RouteToReadout && _encoderCfg.FeedingCfg.RouteToReadout),
                                              inputNeuronStartIdx
                                              ));
                    inputNeuronStartIdx += Fields[fieldIdx - 1].NumOfInputNeurons;
                }
            }
            RoutedFieldCollection = new List<InputField>(Fields.Count);
            foreach (InputField field in Fields)
            {
                if(field.RouteToReadout)
                {
                    RoutedFieldCollection.Add(field);
                }
            }
            _fixedExtVectorLength = -1;
            NumOfRoutedFieldValues = 0;
            _inputData = new List<double[]>();
            _numOfProcessedInputs = 0;
            _reverseMode = false;
            return;
        }

        //Properties
        /// <summary>
        /// Number of remaining stored inputs to be processed
        /// </summary>
        public int NumOfRemainingInputs { get { return _inputData.Count - _numOfProcessedInputs; } }

        //Methods
        /// <summary>
        /// Returns input field having given name
        /// </summary>
        /// <param name="name">Input field name</param>
        /// <returns>Null if not found, InputField instance if found</returns>
        public InputField GetInputField(string name)
        {
            foreach(InputField field in Fields)
            {
                if(field.Name == name)
                {
                    return field;
                }
            }
            return null;
        }

        /// <summary>
        /// Resets internal input buffer
        /// </summary>
        private void ResetInputProcessing()
        {
            //Reset input data buffer
            _inputData.Clear();
            _numOfProcessedInputs = 0;
            _reverseMode = false;
            return;
        }

        /// <summary>
        /// Sets input encoder's fields internal state to initial state after feature filters initialization
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        private void ResetFields(bool resetStatistics)
        {
            //Reset transformers
            foreach (ITransformer transformer in _internalInputTransformerCollection)
            {
                transformer.Reset();
            }
            //Reset generators
            foreach (IGenerator generator in _internalInputGeneratorCollection)
            {
                generator.Reset();
            }
            //Reset fields
            foreach (InputField field in Fields)
            {
                field.ResetNeurons(resetStatistics);
            }
            return;
        }

        /// <summary>
        /// Resets all feature filters
        /// </summary>
        private void ResetFeatureFilters()
        {
            foreach(InputField field in Fields)
            {
                field.ResetFilter();
            }
            return;
        }

        /// <summary>
        /// Resets input encoder to its initial state
        /// </summary>
        public void Reset()
        {
            ResetInputProcessing();
            ResetFields(true);
            ResetFeatureFilters();
            _fixedExtVectorLength = -1;
            NumOfRoutedFieldValues = 0;
            return;
        }

        /// <summary>
        /// Adds inputs from internal transformers and generators.
        /// </summary>
        /// <param name="externalInputVector">External input vector</param>
        private double[] AddInternalInputs(double[] externalInputVector)
        {
            if (_encoderCfg.FieldsCfg.TotalNumOfFields == externalInputVector.Length)
            {
                //Defined no internal fields
                return externalInputVector;
            }
            double[] inputVector = new double[_encoderCfg.FieldsCfg.TotalNumOfFields];
            inputVector.Populate(double.NaN);
            //Original external data
            externalInputVector.CopyTo(inputVector, 0);
            int index = externalInputVector.Length;
            //Transformed fields
            foreach (ITransformer transformer in _internalInputTransformerCollection)
            {
                inputVector[index++] = transformer.Next(inputVector);
            }
            //Generated fields
            foreach (IGenerator generator in _internalInputGeneratorCollection)
            {
                inputVector[index++] = generator.Next();
            }
            return inputVector;
        }

        /// <summary>
        /// Adds inputs from internal transformers and generators.
        /// </summary>
        /// <param name="inputVectors">Collection of external input vectors</param>
        private List<double[]> AddInternalInputs(List<double[]> inputVectors)
        {
            List<double[]> outputVectors = new List<double[]>(inputVectors.Count);
            for (int i = 0; i < inputVectors.Count; i++)
            {
                outputVectors.Add(AddInternalInputs(inputVectors[i]));
            }
            return outputVectors;
        }

        /// <summary>
        /// Adds inputs from internal transformers and generators.
        /// </summary>
        /// <param name="inputPattern">External input pattern</param>
        private InputPattern AddInternalInputs(InputPattern inputPattern)
        {
            int inputPatternTimePoints = inputPattern.VariablesDataCollection[0].Length;
            //Convert to rich vectors
            List<double[]> richVectors = new List<double[]>(inputPatternTimePoints);
            for (int timePointIndex = 0; timePointIndex < inputPatternTimePoints; timePointIndex++)
            {
                double[] externalInputVector = inputPattern.GetDataAtTimePoint(timePointIndex);
                double[] richVector = AddInternalInputs(externalInputVector);
                richVectors.Add(richVector);
            }
            //Convert back to pattern
            InputPattern outputPattern = new InputPattern(_encoderCfg.FieldsCfg.TotalNumOfFields);
            for (int varIdx = 0; varIdx < _encoderCfg.FieldsCfg.TotalNumOfFields; varIdx++)
            {
                double[] varData = new double[inputPatternTimePoints];
                for (int timePointIndex = 0; timePointIndex < inputPatternTimePoints; timePointIndex++)
                {
                    varData[timePointIndex] = richVectors[timePointIndex][varIdx];
                }
                outputPattern.VariablesDataCollection.Add(varData);
            }
            return outputPattern;
        }

        /// <summary>
        /// Adds inputs from internal transformers and generators.
        /// </summary>
        /// <param name="inputPatterns">Collection of external input patterns</param>
        private List<InputPattern> AddInternalInputs(List<InputPattern> inputPatterns)
        {
            List<InputPattern> outputPatterns = new List<InputPattern>(inputPatterns.Count);
            for (int i = 0; i < inputPatterns.Count; i++)
            {
                outputPatterns.Add(AddInternalInputs(inputPatterns[i]));
            }
            return outputPatterns;
        }

        /// <summary>
        /// Updates collection of fields feature filters
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        private void UpdateFeatureFilters(List<double[]> inputVectorCollection)
        {
            Parallel.For(0, Fields.Count, i =>
            {
                //Update filter
                foreach (double[] vector in inputVectorCollection)
                {
                    Fields[i].UpdateFilter(vector[i]);
                }
            });
            return;
        }

        /// <summary>
        /// Updates collection of preprocessor's feature filters
        /// </summary>
        /// <param name="inputPatternCollection">Collection of input patterns</param>
        private void UpdateFeatureFilters(List<InputPattern> inputPatternCollection)
        {
            Parallel.For(0, _encoderCfg.FieldsCfg.TotalNumOfFields, varIdx =>
            {
                foreach (InputPattern pattern in inputPatternCollection)
                {
                    for (int i = 0; i < pattern.VariablesDataCollection[varIdx].Length; i++)
                    {
                        Fields[varIdx].UpdateFilter(pattern.VariablesDataCollection[varIdx][i]);
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Validates input vector's length
        /// </summary>
        /// <param name="extInputVectorLength">Input vector's length</param>
        private void CheckExtInputVectorLength(int extInputVectorLength)
        {
            //Check vector length
            if (_fixedExtVectorLength != -1 && extInputVectorLength != _fixedExtVectorLength)
            {
                throw new Exception("Number of the time-points in every input pattern has to be constant because input data is routed to the readout.");
            }
            return;
        }

        /// <summary>
        /// Initializes internal feature filters to operational state based on sample data
        /// </summary>
        /// <param name="inputBundle">Sample input data</param>
        public void Initialize(VectorBundle inputBundle)
        {
            //Full reset
            Reset();
            if (_encoderCfg.FeedingCfg.FeedingType == InputFeedingType.Continuous)
            {
                //Add internal inputs and initialize feature filters
                UpdateFeatureFilters(AddInternalInputs(inputBundle.InputVectorCollection));
                //Number of routed fields' values
                if (RoutedFieldCollection.Count > 0)
                {
                    NumOfRoutedFieldValues = RoutedFieldCollection.Count;
                }
            }
            else
            {
                //Input length constraint and number of routed fields' values
                if(RoutedFieldCollection.Count > 0)
                {
                    _fixedExtVectorLength = inputBundle.InputVectorCollection[0].Length;
                    NumOfRoutedFieldValues = (RoutedFieldCollection.Count * (_fixedExtVectorLength / Fields.Count));
                }
                //Convert input vectors to InputPatterns
                List<InputPattern> patterns = new List<InputPattern>(inputBundle.InputVectorCollection.Count);
                foreach (double[] vector in inputBundle.InputVectorCollection)
                {
                    //Check length of the external input vector
                    CheckExtInputVectorLength(vector.Length);
                    //Convert vector to pattern
                    FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                    InputPattern inputPattern = new InputPattern(vector,
                                                                 _encoderCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
                                                                 feedingCfg.VarSchema,
                                                                 feedingCfg.UnificationCfg.Detrend,
                                                                 feedingCfg.UnificationCfg.UnifyAmplitude,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.SignalBeginThreshold,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.SignalEndThreshold,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.UniformTimeScale,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.TargetTimePoints
                                                                 );
                    patterns.Add(inputPattern);
                }
                //Add internal inputs and initialize feature filters
                UpdateFeatureFilters(AddInternalInputs(patterns));
            }
            return;
        }

        /// <summary>
        /// Stores new input data to be processed
        /// </summary>
        /// <param name="inputVector">Input vector of external data</param>
        public void StoreNewData(double[] inputVector)
        {
            //Reset input data
            ResetInputProcessing();
            //Store new data
            if (_encoderCfg.FeedingCfg.FeedingType == InputFeedingType.Continuous)
            {
                //Add new vector
                _inputData.Add(AddInternalInputs(inputVector));
            }
            else
            {
                //Reset fields to initial state
                ResetFields(false);
                //Check length of the external input vector
                CheckExtInputVectorLength(inputVector.Length);
                //Add new pattern vectors
                FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                InputPattern inputPattern = new InputPattern(inputVector,
                                                             _encoderCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
                                                             feedingCfg.VarSchema,
                                                             feedingCfg.UnificationCfg.Detrend,
                                                             feedingCfg.UnificationCfg.UnifyAmplitude,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.SignalBeginThreshold,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.SignalEndThreshold,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.UniformTimeScale,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.TargetTimePoints
                                                             );
                InputPattern completedPattern = AddInternalInputs(inputPattern);
                int inputPatternTimePoints = completedPattern.VariablesDataCollection[0].Length;
                //Convert to vectors
                for (int timePointIndex = 0; timePointIndex < inputPatternTimePoints; timePointIndex++)
                {
                    _inputData.Add(completedPattern.GetDataAtTimePoint(timePointIndex));
                }
            }
            return;
        }

        /// <summary>
        /// Initializes input fields by next stored input data if available
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        /// <returns>True if there are were still unprocessed input data</returns>
        public bool EncodeNextInputData(bool collectStatistics)
        {
            if(_numOfProcessedInputs < _inputData.Count)
            {
                for(int i = 0; i < Fields.Count; i++)
                {
                    Fields[i].SetNewData(_inputData[_numOfProcessedInputs][i], collectStatistics);
                }
                ++_numOfProcessedInputs;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Changes order of input data and resets counter of processed inputs
        /// </summary>
        public void SetReverseMode()
        {
            //Checks
            if (_reverseMode)
            {
                throw new Exception("Can't set reverse mode. Input encoder is already in reverse mode.");
            }
            if (_encoderCfg.FeedingCfg.FeedingType != InputFeedingType.Patterned)
            {
                throw new Exception("Illegal call to set reverse mode. Reverse mode is relevant only for patterned feeding regime.");
            }
            //Reset fields to initial state
            ResetFields(false);
            //Reverse
            _inputData.Reverse();
            _numOfProcessedInputs = 0;
            _reverseMode = true;
            return;
        }

        /// <summary>
        /// Returns collection of predictor descriptor objects related to exact input values instances routed to readout
        /// </summary>
        public List<PredictorDescriptor> GetInputValuesPredictorsDescriptors()
        {
            List<PredictorDescriptor> result = new List<PredictorDescriptor>();
            int numOfFieldValInstances = _fixedExtVectorLength == -1 ? 1 : (_fixedExtVectorLength / Fields.Count);
            foreach (InputField field in RoutedFieldCollection)
            {
                for (int i = 0; i < numOfFieldValInstances; i++)
                {
                    result.Add(new PredictorDescriptor(field.Idx, ReservoirID, PoolID, PredictorDescriptor.InputFieldValue));
                }
            }
            return result;
        }

        /// <summary>
        /// Copies all routed input data to a given buffer starting from the specified position
        /// </summary>
        /// <param name="buffer">Target buffer</param>
        /// <param name="fromOffset">Starting zero based position in the target buffer</param>
        /// <returns>Number of copied values</returns>
        public int CopyRoutedInputDataTo(double[] buffer, int fromOffset)
        {
            int count = 0;
            foreach (InputField field in RoutedFieldCollection)
            {
                for (int i = 0; i < _inputData.Count; i++)
                {
                    buffer[fromOffset++] = _inputData[i][field.Idx];
                    ++count;
                }
            }
            return count;
        }

    }//InputEncoder

}//Namespace
