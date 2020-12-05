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
    /// Processes given natural external input data and provides it's representation on analog and spiking input neurons for the data processing in the reservoirs.
    /// Supports set of various realtime input chainable data transformations and data generators as additional computed input fields.
    /// Supports two main input feeding regimes: Continuous (one input is data vector at time T) and Patterned (one input is InputPattern containing data for all timepoints).
    /// Supports three ways how to represent analog value as the spikes: Horizontal (fast - simultaneous activity of the neuronal population), Vertical (slow - spike-train on single input neuron) or None (fast - spiking represetantion is then forbidden).
    /// </summary>
    [Serializable]
    public class InputEncoder
    {
        //Constants
        /// <summary>
        /// Identifies variable number of time-points per one instance of processed input vector
        /// </summary>
        public const int VariableNumOfTimePoints = -1;

        /// <summary>
        /// Identifies variable length of external varying input data vector
        /// </summary>
        private const int NonFixedVaryingDataVectorLength = -1;

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
        /// Type of the input feeding regime
        /// </summary>
        public enum InputFeedingType
        {
            /// <summary>
            /// Continuous input feeding
            /// </summary>
            Continuous,
            /// <summary>
            /// Patterned input feeding
            /// </summary>
            Patterned
        }

        /// <summary>
        /// Enumeration of spiking input encoding regimes
        /// </summary>
        public enum SpikingInputEncodingRegime
        {
            /// <summary>
            /// Spikes are encoded at once as input neurons population activity (horizontal)
            /// </summary>
            Horizontal,
            /// <summary>
            /// Spikes are encoded in several cycles as spike-train on single input neuron (vertical)
            /// </summary>
            Vertical,
            /// <summary>
            /// Spikes encoding is not allowed
            /// </summary>
            Forbidden
        }

        //Attribute properties
        /// <summary>
        /// Number of fixed time-points of one instance of processed external input vector
        /// or VariableNumOfTimePoints (-1)
        /// </summary>
        public int NumOfTimePoints { get; private set; }

        /// <summary>
        /// Number of values routed to readout
        /// </summary>
        public int NumOfRoutedValues { get; private set; }

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
        /// Collection of varying input fields
        /// </summary>
        private readonly List<InputField> _varyingFields;
        
        /// <summary>
        /// Number of steady fields (relevant only in case of patterned feeding)
        /// </summary>
        private readonly int _numOfSteadyFields;
        
        /// <summary>
        /// Indexes of steady fields to be routed to the readout
        /// </summary>
        private readonly List<int> _routedSteadyFieldIndexCollection;
        
        /// <summary>
        /// Varying fields to be routed to readout
        /// </summary>
        private readonly List<InputField> _routedVaryingFieldCollection;
        
        /// <summary>
        /// Required constant length of the external varying data vector or VariableVaryingVectorLength (no constraint)
        /// </summary>
        private int _varyingDataVectorLength;
        
        /// <summary>
        /// Steady data to be processed
        /// </summary>
        private double[] _steadyData;
        
        /// <summary>
        /// Varying data to be processed
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
            //Steady fields
            _numOfSteadyFields = 0;
            _routedSteadyFieldIndexCollection = new List<int>();
            if (_encoderCfg.FeedingCfg.FeedingType == InputFeedingType.Patterned)
            {
                FeedingPatternedSettings fps = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                if (fps.SteadyFieldsCfg != null)
                {
                    _numOfSteadyFields = fps.SteadyFieldsCfg.FieldCfgCollection.Count;
                    for(int i = 0; i < fps.SteadyFieldsCfg.FieldCfgCollection.Count; i++)
                    {
                        if(fps.SteadyFieldsCfg.FieldCfgCollection[i].RouteToReadout)
                        {
                            _routedSteadyFieldIndexCollection.Add(i);
                        }
                    }
                }
            }
            //Varying fields
            _varyingFields = new List<InputField>(_encoderCfg.VaryingFieldsCfg.TotalNumOfFields);
            int[] coordinates = _encoderCfg.CoordinatesCfg.GetCoordinates();
            int fieldIdx = _numOfSteadyFields;
            int inputNeuronStartIdx = 0;
            //External fields
            foreach (ExternalFieldSettings fieldCfg in _encoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                _varyingFields.Add(new InputField(fieldCfg.Name,
                                          fieldIdx++,
                                          coordinates,
                                          Interval.IntN1P1,
                                          fieldCfg.FeatureFilterCfg,
                                          _encoderCfg.VaryingFieldsCfg.SpikesCoderCfg,
                                          (fieldCfg.RouteToReadout && _encoderCfg.VaryingFieldsCfg.RouteToReadout),
                                          inputNeuronStartIdx
                                          ));
                inputNeuronStartIdx += _varyingFields[(fieldIdx - _numOfSteadyFields) - 1].NumOfInputNeurons;
            }
            //Internal input transformers and fields
            _internalInputTransformerCollection = new List<ITransformer>();
            if (_encoderCfg.VaryingFieldsCfg.TransformedFieldsCfg != null)
            {
                List<string> names = _encoderCfg.VaryingFieldsCfg.GetNames();
                foreach (TransformedFieldSettings fieldCfg in _encoderCfg.VaryingFieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                {
                    _internalInputTransformerCollection.Add(TransformerFactory.Create(names, fieldCfg.TransformerCfg));
                    _varyingFields.Add(new InputField(fieldCfg.Name,
                                              fieldIdx++,
                                              coordinates,
                                              Interval.IntN1P1,
                                              fieldCfg.FeatureFilterCfg,
                                              _encoderCfg.VaryingFieldsCfg.SpikesCoderCfg,
                                              (fieldCfg.RouteToReadout && _encoderCfg.VaryingFieldsCfg.RouteToReadout),
                                              inputNeuronStartIdx
                                              ));
                    inputNeuronStartIdx += _varyingFields[(fieldIdx - _numOfSteadyFields) - 1].NumOfInputNeurons;
                }
            }
            //Internal input generators and fields
            _internalInputGeneratorCollection = new List<IGenerator>();
            if (_encoderCfg.VaryingFieldsCfg.GeneratedFieldsCfg != null)
            {
                foreach (GeneratedFieldSettings fieldCfg in _encoderCfg.VaryingFieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                {
                    _internalInputGeneratorCollection.Add(GeneratorFactory.Create(fieldCfg.GeneratorCfg));
                    _varyingFields.Add(new InputField(fieldCfg.Name,
                                              fieldIdx++,
                                              coordinates,
                                              Interval.IntN1P1,
                                              fieldCfg.FeatureFilterCfg,
                                              _encoderCfg.VaryingFieldsCfg.SpikesCoderCfg,
                                              (fieldCfg.RouteToReadout && _encoderCfg.VaryingFieldsCfg.RouteToReadout),
                                              inputNeuronStartIdx
                                              ));
                    inputNeuronStartIdx += _varyingFields[(fieldIdx - _numOfSteadyFields) - 1].NumOfInputNeurons;
                }
            }
            _routedVaryingFieldCollection = new List<InputField>(_varyingFields.Count);
            foreach (InputField field in _varyingFields)
            {
                if (field.RouteToReadout)
                {
                    _routedVaryingFieldCollection.Add(field);
                }
            }
            _varyingDataVectorLength = NonFixedVaryingDataVectorLength;
            NumOfTimePoints = VariableNumOfTimePoints;
            NumOfRoutedValues = 0;
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
        /// Returns varying input field having given name
        /// </summary>
        /// <param name="name">Input field name</param>
        /// <returns>Null if not found, InputField instance if found</returns>
        public InputField GetVaryingInputField(string name)
        {
            foreach (InputField field in _varyingFields)
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
            _steadyData = null;
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
            foreach (InputField field in _varyingFields)
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
            foreach (InputField field in _varyingFields)
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
            _varyingDataVectorLength = NonFixedVaryingDataVectorLength;
            NumOfTimePoints = VariableNumOfTimePoints;
            NumOfRoutedValues = 0;
            return;
        }

        /// <summary>
        /// Adds inputs from internal transformers and generators.
        /// </summary>
        /// <param name="externalInputVector">External input vector</param>
        private double[] AddInternalInputs(double[] externalInputVector)
        {
            if (_encoderCfg.VaryingFieldsCfg.TotalNumOfFields == externalInputVector.Length)
            {
                //Defined no internal fields
                return externalInputVector;
            }
            double[] inputVector = new double[_encoderCfg.VaryingFieldsCfg.TotalNumOfFields];
            inputVector.Populate(double.NaN);
            //Original external data
            externalInputVector.CopyTo(inputVector, 0);
            int index = externalInputVector.Length;
            //Transformed fields
            foreach (ITransformer transformer in _internalInputTransformerCollection)
            {
                inputVector[index++] = transformer.Transform(inputVector);
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
            Parallel.For(0, _varyingFields.Count, i =>
            {
                //Update filter
                foreach (double[] vector in inputVectorCollection)
                {
                    _varyingFields[i].UpdateFilter(vector[i]);
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
            if (_varyingDataVectorLength != NonFixedVaryingDataVectorLength && extInputVectorLength != _varyingDataVectorLength)
            {
                throw new InvalidOperationException($"Number of the time-points in every input data has to be constant ({_varyingDataVectorLength}).");
            }
            return;
        }

        /// <summary>
        /// Splits external input vector to a steady part and varying part
        /// </summary>
        /// <param name="extInputVector">Input external data vector</param>
        /// <param name="steadyVector">Output steady data vector</param>
        /// <param name="varyingVector">Output varying data vector</param>
        private void SplitSteadyAndVaryingInputData(double[] extInputVector, out double[] steadyVector, out double[] varyingVector)
        {
            //Separate steady and varying data
            if (_numOfSteadyFields > 0)
            {
                //Steady data
                steadyVector = new double[_numOfSteadyFields];
                for (int i = 0; i < _numOfSteadyFields; i++)
                {
                    steadyVector[i] = extInputVector[i];
                }
                //Varying data
                varyingVector = new double[extInputVector.Length - _numOfSteadyFields];
                for (int i = 0; i < varyingVector.Length; i++)
                {
                    varyingVector[i] = extInputVector[_numOfSteadyFields + i];
                }
            }
            else
            {
                //Steady data
                steadyVector = null;
                //Varying data
                varyingVector = extInputVector;
            }
            return;
        }

        /// <summary>
        /// Initializes internal feature filters to operational state based on a bundle of sample data
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
                _varyingDataVectorLength = inputBundle.InputVectorCollection[0].Length;
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
                if(NumOfTimePoints == VariableNumOfTimePoints && (_routedVaryingFieldCollection.Count > 0 || feedingPatternedCfg.Slices > 1))
                {
                    //because no resampling, length of external input vector must be fixed to keep consistent data
                    _varyingDataVectorLength = inputBundle.InputVectorCollection[0].Length - _numOfSteadyFields;
                    //Number of time-points must be also fixed
                    NumOfTimePoints = _varyingDataVectorLength / _varyingFields.Count;
                }
                //Convert input vectors to InputPatterns
                foreach (double[] orgVector in inputBundle.InputVectorCollection)
                {
                    //Split steady and varying data
                    SplitSteadyAndVaryingInputData(orgVector, out double[] steadyVector, out double[] varyingVector);
                    //Check length of the external varying data vector
                    ValidateExtInputVectorLength(varyingVector.Length);
                    //Convert external vector to pattern
                    FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                    InputPattern inputPattern = new InputPattern(varyingVector,
                                                                 _encoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
                                                                 feedingCfg.VarSchema,
                                                                 feedingCfg.UnificationCfg.Detrend,
                                                                 feedingCfg.UnificationCfg.UnifyAmplitude,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.SignalBeginThreshold,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.SignalEndThreshold,
                                                                 feedingCfg.UnificationCfg.ResamplingCfg.UniformTimeScale,
                                                                 NumOfTimePoints == VariableNumOfTimePoints ? ResamplingSettings.AutoTargetTimePointsNum : NumOfTimePoints
                                                                 );
                    List<double[]> inputPatternVectors = CompleteInputPattern(inputPattern);
                    UpdateFeatureFilters(inputPatternVectors);
                }
            }
            //Number of routed input values
            NumOfRoutedValues = _routedSteadyFieldIndexCollection.Count;
            if (_routedVaryingFieldCollection.Count > 0)
            {
                NumOfRoutedValues += _routedVaryingFieldCollection.Count * NumOfTimePoints;
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
                //Split steady and varying data
                SplitSteadyAndVaryingInputData(inputVector, out _steadyData, out double[] varyingVector);
                //Check length of the external varying data vector
                ValidateExtInputVectorLength(varyingVector.Length);
                //Reset input neurons to initial state
                ResetInputNeurons(false);
                //Prepare input pattern
                FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_encoderCfg.FeedingCfg;
                InputPattern inputPattern = new InputPattern(varyingVector,
                                                             _encoderCfg.VaryingFieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
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
        public bool EncodeNextInputData()
        {
            if (_numOfProcessedInputs < _inputDataQueue.Count)
            {
                for (int i = 0; i < _varyingFields.Count; i++)
                {
                    _varyingFields[i].SetNewData(_inputDataQueue[_numOfProcessedInputs][i]);
                }
                ++_numOfProcessedInputs;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Delivers encoded data into the input neurons
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of associated input neurons</param>
        public bool Fetch(bool collectStatistics)
        {
            foreach(InputField field in _varyingFields)
            {
                if(!field.Fetch(collectStatistics))
                {
                    return false;
                }
            }
            return true;
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
            //Build descriptors
            List<PredictorDescriptor> result = new List<PredictorDescriptor>(_routedSteadyFieldIndexCollection.Count + _routedVaryingFieldCollection.Count * NumOfTimePoints);
            //Steady fields
            foreach(int idx in _routedSteadyFieldIndexCollection)
            {
                result.Add(new PredictorDescriptor(((FeedingPatternedSettings)_encoderCfg.FeedingCfg).SteadyFieldsCfg.FieldCfgCollection[idx].Name));
            }
            //Varying fields
            foreach (InputField field in _routedVaryingFieldCollection)
            {
                for (int i = 0; i < NumOfTimePoints; i++)
                {
                    result.Add(new PredictorDescriptor(field.Name));
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
            //Steady fields
            foreach(int idx in _routedSteadyFieldIndexCollection)
            {
                buffer[fromOffset++] = _steadyData[idx];
                ++count;
            }
            //Varying fields
            foreach (InputField field in _routedVaryingFieldCollection)
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
