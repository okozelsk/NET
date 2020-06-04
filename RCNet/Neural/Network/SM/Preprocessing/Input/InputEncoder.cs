using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Data.Transformers;
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
        /// Identifies variable number of time-points per one instance of processed external input vector
        /// </summary>
        public const int VariableNumOfTimePoints = -1;

        /// <summary>
        /// Identifies variable length of external input vector
        /// </summary>
        private const int VariableExtVectorLength = -1;

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
        /// Number of fixed time-points of one instance of processed external input vector
        /// or VariableNumOfTimePoints (-1)
        /// </summary>
        public int NumOfTimePoints { get; private set; }

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
        /// Required constant length of the external input vector or VariableExtVectorLength (no constraint)
        /// </summary>
        private int _fixedExtVectorLength;
        /// <summary>
        /// Data to be processed
        /// </summary>
        private readonly List<double[]> _inputDataQueue;
        /// <summary>
        /// Number of already processed inputs from _inputDataQueue
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
                if (field.RouteToReadout)
                {
                    RoutedFieldCollection.Add(field);
                }
            }
            _fixedExtVectorLength = VariableExtVectorLength;
            NumOfTimePoints = VariableNumOfTimePoints;
            NumOfRoutedFieldValues = 0;
            //Input processing queue
            _inputDataQueue = new List<double[]>();
            ResetInputProcessingQueue();
            return;
        }

        //Properties
        /// <summary>
        /// Number of remaining stored inputs (remaining parts of external input vector) to be processed
        /// </summary>
        public int NumOfRemainingInputs { get { return _inputDataQueue.Count - _numOfProcessedInputs; } }

        //Methods
        /// <summary>
        /// Returns input field having given name
        /// </summary>
        /// <param name="name">Input field name</param>
        /// <returns>Null if not found, InputField instance if found</returns>
        public InputField GetInputField(string name)
        {
            foreach (InputField field in Fields)
            {
                if (field.Name == name)
                {
                    return field;
                }
            }
            return null;
        }

        /// <summary>
        /// Resets internal input buffer
        /// </summary>
        private void ResetInputProcessingQueue()
        {
            //Reset input data buffer
            _inputDataQueue.Clear();
            _numOfProcessedInputs = 0;
            _reverseMode = false;
            return;
        }

        /// <summary>
        /// Resets internal transformers and generators to initial state
        /// </summary>
        private void ResetTransformersAndGenerators()
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
            return;
        }

        /// <summary>
        /// Resets all input neurons to initial state
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        private void ResetInputNeurons(bool resetStatistics)
        {
            //Reset input neurons in all input fields
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
            foreach (InputField field in Fields)
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
            ResetInputProcessingQueue();
            ResetTransformersAndGenerators();
            ResetInputNeurons(true);
            ResetFeatureFilters();
            _fixedExtVectorLength = VariableExtVectorLength;
            NumOfTimePoints = VariableNumOfTimePoints;
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
        /// Adds internal inputs (transformers and generators) and converts pattern to series of vectors.
        /// </summary>
        /// <param name="inputPattern">External input pattern</param>
        private List<double[]> CompleteInputPattern(InputPattern inputPattern)
        {
            //Reset transformers and generators
            ResetTransformersAndGenerators();
            //Convert pattern to completed vectors having added internal inputs from transformers and generators
            int inputPatternTimePoints = inputPattern.VariablesDataCollection[0].Length;
            List<double[]> completedVectors = new List<double[]>(inputPatternTimePoints);
            for (int timePointIndex = 0; timePointIndex < inputPatternTimePoints; timePointIndex++)
            {
                double[] externalInputVector = inputPattern.GetDataAtTimePoint(timePointIndex);
                double[] completedVector = AddInternalInputs(externalInputVector);
                completedVectors.Add(completedVector);
            }
            return completedVectors;
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
        /// Validates input vector's length
        /// </summary>
        /// <param name="extInputVectorLength">Input vector's length</param>
        private void ValidateExtInputVectorLength(int extInputVectorLength)
        {
            //Check vector length
            if (_fixedExtVectorLength != VariableExtVectorLength && extInputVectorLength != _fixedExtVectorLength)
            {
                throw new InvalidOperationException($"Number of the time-points in every input data has to be constant ({_fixedExtVectorLength}).");
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
                //Continuous feeding
                //Fixed lengths
                _fixedExtVectorLength = inputBundle.InputVectorCollection[0].Length;
                NumOfTimePoints = 1;
                //Add internal inputs and initialize feature filters
                UpdateFeatureFilters(AddInternalInputs(inputBundle.InputVectorCollection));
            }
            else
            {
                //Patterned feeding
                FeedingPatternedSettings feedingPatternedCfg = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                //Input pattern length constraint and number of time-points
                NumOfTimePoints = feedingPatternedCfg.UnificationCfg.ResamplingCfg.TargetTimePoints != ResamplingSettings.AutoTargetTimePointsNum ? feedingPatternedCfg.UnificationCfg.ResamplingCfg.TargetTimePoints : VariableNumOfTimePoints;
                if(NumOfTimePoints == VariableNumOfTimePoints && (RoutedFieldCollection.Count > 0 || feedingPatternedCfg.Slices > 1))
                {
                    //because no resampling, length of external input vector must be fixed to keep consistent data
                    _fixedExtVectorLength = inputBundle.InputVectorCollection[0].Length;
                    //Number of time-points must be also fixed
                    NumOfTimePoints = _fixedExtVectorLength / Fields.Count;
                }
                //Convert input vectors to InputPatterns
                List<List<double[]>> inputPatterns = new List<List<double[]>>(inputBundle.InputVectorCollection.Count);
                foreach (double[] vector in inputBundle.InputVectorCollection)
                {
                    //Check length of the external input vector
                    ValidateExtInputVectorLength(vector.Length);
                    //Convert external vector to pattern
                    FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                    InputPattern inputPattern = new InputPattern(vector,
                                                                 _encoderCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
                                                                 feedingCfg.VarSchema,
                                                                 feedingCfg.UnificationCfg.Detrend,
                                                                 feedingCfg.UnificationCfg.UnifyAmplitude,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.SignalBeginThreshold,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.SignalEndThreshold,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.UniformTimeScale,
                                                                 NumOfTimePoints == VariableNumOfTimePoints ? ResamplingSettings.AutoTargetTimePointsNum : NumOfTimePoints
                                                                 );
                    List<double[]> inputPatternVectors = CompleteInputPattern(inputPattern);
                    inputPatterns.Add(inputPatternVectors);
                    UpdateFeatureFilters(inputPatternVectors);
                }
            }
            //Number of routed input values
            if (RoutedFieldCollection.Count > 0)
            {
                NumOfRoutedFieldValues = RoutedFieldCollection.Count * NumOfTimePoints;
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
            ResetInputProcessingQueue();
            //Store new data
            if (_encoderCfg.FeedingCfg.FeedingType == InputFeedingType.Continuous)
            {
                //Add single vector into the processing queue
                _inputDataQueue.Add(AddInternalInputs(inputVector));
            }
            else
            {
                //Check length of the external input vector
                ValidateExtInputVectorLength(inputVector.Length);
                //Reset input neurons to initial state
                ResetInputNeurons(false);
                //Prepare input pattern
                FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                InputPattern inputPattern = new InputPattern(inputVector,
                                                             _encoderCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
                                                             feedingCfg.VarSchema,
                                                             feedingCfg.UnificationCfg.Detrend,
                                                             feedingCfg.UnificationCfg.UnifyAmplitude,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.SignalBeginThreshold,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.SignalEndThreshold,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.UniformTimeScale,
                                                             NumOfTimePoints == VariableNumOfTimePoints ? ResamplingSettings.AutoTargetTimePointsNum : NumOfTimePoints
                                                             );
                _inputDataQueue.AddRange(CompleteInputPattern(inputPattern));
            }
            return;
        }

        /// <summary>
        /// Initializes input fields by next stored piece of input data if available
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public bool EncodeNextInputData(bool collectStatistics)
        {
            if (_numOfProcessedInputs < _inputDataQueue.Count)
            {
                for (int i = 0; i < Fields.Count; i++)
                {
                    Fields[i].SetNewData(_inputDataQueue[_numOfProcessedInputs][i], collectStatistics);
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
                throw new InvalidOperationException($"Can't set reverse mode. Input encoder is already in reverse mode.");
            }
            if (_encoderCfg.FeedingCfg.FeedingType != InputFeedingType.Patterned)
            {
                throw new InvalidOperationException($"Illegal call to set reverse mode. Reverse mode is relevant only for patterned feeding regime.");
            }
            //Reset input neurons to initial state
            ResetInputNeurons(false);
            //Reverse processing queue
            _inputDataQueue.Reverse();
            _numOfProcessedInputs = 0;
            _reverseMode = true;
            return;
        }

        /// <summary>
        /// Returns collection of predictor descriptor objects related to exact input values instances routed to readout
        /// </summary>
        public List<PredictorDescriptor> GetInputValuesPredictorsDescriptors()
        {
            //Check call relevancy
            if(NumOfTimePoints == VariableNumOfTimePoints)
            {
                throw new InvalidOperationException("Wrong function call. Number of time-points per input is variable.");
            }
            //Build descriptors
            List<PredictorDescriptor> result = new List<PredictorDescriptor>(RoutedFieldCollection.Count * NumOfTimePoints);
            foreach (InputField field in RoutedFieldCollection)
            {
                for (int i = 0; i < NumOfTimePoints; i++)
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
                for (int i = 0; i < _inputDataQueue.Count; i++)
                {
                    buffer[fromOffset++] = _inputDataQueue[i][field.Idx];
                    ++count;
                }
            }
            return count;
        }

    }//InputEncoder

}//Namespace
