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
    /// Implements a mediation layer between the external input data and the internal reservoirs of the neural preprocessor.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Processes the external input data in the natural form and provides it's representation on analog and spiking input neurons for the next processing in the reservoirs.
    /// </para>
    /// <para>
    /// Allows to create new computed input fields using chainable transformations of existing external input fields, as well as adding independently generated input fields using various generators.
    /// </para>
    /// <para>
    /// Supports two input feeding modes: Continuous and Patterned.
    /// The Continuous feeding mode processes an input vector as the variable(s) values at the single time-point T.
    /// The Patterned feeding mode processes an input vector as an alone input pattern consisting of a time series of the variable(s) values.
    /// </para>
    /// Supports three ways how to represent an analog value as the spikes: Horizontal, Vertical or Forbidden.
    /// The Horizontal way of coding means a simultaneous activity of the neuronal population where every input field is coded by several spiking input neurons (a horizontal spike-train). It is fast, it leads to a single computation cycle of the reservoirs per the input field value.
    /// The Vertical way of coding means that the input field value is coded as a spike-train on a single spiking input neuron. It is slower, it leads to multiple computation cycles of the reservoirs according to the spike-train length.
    /// The Forbidden way of coding means there is no coding of an analog value as the spikes. It is fast, it leads to a single computation cycle of the reservoirs per the input field value and it does not utilize any spiking input neuron(s).
    /// </remarks>
    [Serializable]
    public class InputEncoder
    {
        //Constants
        /// <summary>
        /// Identifies that there is a variable number of time-points to be processed in the reservoirs.
        /// </summary>
        public const int VariableNumOfTimePoints = -1;

        /// <summary>
        /// Identifies the variable length of an external input data vector.
        /// </summary>
        private const int NonFixedVaryingDataVectorLength = -1;

        /// <summary>
        /// The ID of the input encoder's fictive reservoir.
        /// </summary>
        public const int ReservoirID = -1;

        /// <summary>
        /// The ID of the input encoder's fictive pool.
        /// </summary>
        public const int PoolID = -1;

        //Enumerations
        /// <summary>
        /// The type of input feeding.
        /// </summary>
        public enum InputFeedingType
        {
            /// <summary>
            /// The continuous input feeding mode.
            /// </summary>
            Continuous,
            /// <summary>
            /// The patterned input feeding mode.
            /// </summary>
            Patterned
        }

        /// <summary>
        /// The way of input spikes coding.
        /// </summary>
        public enum InputSpikesCoding
        {
            /// <summary>
            /// The horizontal coding.
            /// </summary>
            Horizontal,
            /// <summary>
            /// The vertical coding.
            /// </summary>
            Vertical,
            /// <summary>
            /// The coding of input spikes is not allowed.
            /// </summary>
            Forbidden
        }

        //Attribute properties
        /// <summary>
        /// The number of fixed time-points per one instance of processed external input vector or VariableNumOfTimePoints (-1).
        /// </summary>
        public int NumOfTimePoints { get; private set; }

        /// <summary>
        /// The number of values being routed to readout layer.
        /// </summary>
        public int NumOfRoutedValues { get; private set; }

        //Attributes
        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly InputEncoderSettings _encoderCfg;

        /// <summary>
        /// The collection of the internal input transformers associated with the transformed input fields.
        /// </summary>
        private readonly List<ITransformer> _internalInputTransformerCollection;

        /// <summary>
        /// The collection of the internal generators associated with the generated input fields.
        /// </summary>
        private readonly List<IGenerator> _internalInputGeneratorCollection;

        /// <summary>
        /// The collection of varying input fields.
        /// </summary>
        private readonly List<InputField> _varyingFields;

        /// <summary>
        /// The number of steady input fields (relevant only in case of patterned feeding).
        /// </summary>
        private readonly int _numOfSteadyFields;

        /// <summary>
        /// The indexes of steady input fields being routed to the readout layer.
        /// </summary>
        private readonly List<int> _routedSteadyFieldIndexCollection;

        /// <summary>
        /// The collection of the varying input fields being routed to the readout layer.
        /// </summary>
        private readonly List<InputField> _routedVaryingFieldCollection;

        /// <summary>
        /// The constant length of the external varying data vector or NonFixedVaryingDataVectorLength (-1). (no constraint)
        /// </summary>
        private int _varyingDataVectorLength;

        /// <summary>
        /// The steady input data to be processed.
        /// </summary>
        private double[] _steadyData;

        /// <summary>
        /// The varying input data to be processed.
        /// </summary>
        private readonly List<double[]> _inputDataQueue;

        /// <summary>
        /// The number of already processed inputs from the queue.
        /// </summary>
        private int _numOfProcessedInputs;

        /// <summary>
        /// Indicates the reverse mode of input data processing.
        /// </summary>
        private bool _reverseMode;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="inputEncoderCfg">The configuration of the input encoder.</param>
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
                    for (int i = 0; i < fps.SteadyFieldsCfg.FieldCfgCollection.Count; i++)
                    {
                        if (fps.SteadyFieldsCfg.FieldCfgCollection[i].RouteToReadout)
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
                                                  _encoderCfg.VaryingFieldsCfg.InputSpikesCoderCfg,
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
                                              _encoderCfg.VaryingFieldsCfg.InputSpikesCoderCfg,
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
                                              _encoderCfg.VaryingFieldsCfg.InputSpikesCoderCfg,
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
        /// The number of remaining stored inputs to be processed.
        /// </summary>
        public int NumOfRemainingInputs { get { return _inputDataQueue.Count - _numOfProcessedInputs; } }

        //Methods
        /// <summary>
        /// Gets the varying input field object by name.
        /// </summary>
        /// <param name="name">The varying input field name.</param>
        /// <returns>An InputField instance when found or null when not found.</returns>
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
        /// Resets the internal input buffers.
        /// </summary>
        private void ResetInputProcessingQueue()
        {
            _steadyData = null;
            _inputDataQueue.Clear();
            _numOfProcessedInputs = 0;
            _reverseMode = false;
            return;
        }

        /// <summary>
        /// Resets all internal transformers and generators to their initial state.
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
        /// Resets all input neurons to their initial state.
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset also neurons' internal statistics.</param>
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
        /// Resets all feature filters to their initial state.
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
        /// Resets the input encoder to its initial state.
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
        /// <param name="externalInputVector">An external input vector.</param>
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
        /// <param name="inputVectors">The collection of external input vectors</param>
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
        /// Adds inputs from internal transformers and generators and converts an input pattern to series of vectors.
        /// </summary>
        /// <param name="inputPattern">An input pattern.</param>
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
        /// Updates the input feature filters.
        /// </summary>
        /// <param name="inputVectorCollection">The collection of input vectors.</param>
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
        /// Validates the length of the external input vector.
        /// </summary>
        /// <param name="extInputVectorLength">The length of the external input vector.</param>
        private void ValidateExtInputVectorLength(int extInputVectorLength)
        {
            //Check the length
            if (_varyingDataVectorLength != NonFixedVaryingDataVectorLength && extInputVectorLength != _varyingDataVectorLength)
            {
                throw new InvalidOperationException($"The number of the time-points must be constant ({_varyingDataVectorLength}).");
            }
            return;
        }

        /// <summary>
        /// Splits an external input vector to a steady part and a varying part.
        /// </summary>
        /// <param name="extInputVector">An external input vector.</param>
        /// <param name="steadyVector">An output steady data vector.</param>
        /// <param name="varyingVector">An output varying data vector.</param>
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
        /// Initializes the input encoder and its feature filters from the specified samples data bundle.
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
                if (NumOfTimePoints == VariableNumOfTimePoints && (_routedVaryingFieldCollection.Count > 0 || feedingPatternedCfg.Slices > 1))
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
        /// Stores new input data to be processed.
        /// </summary>
        /// <param name="inputVector">An external input vector.</param>
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
        /// Initializes input fields by the next piece of stored input data (if available).
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
        /// Fetches the encoded data from the input fields into the input neurons.
        /// </summary>
        /// <param name="collectStatistics">Specifies whether to update internal statistics of the input neurons.</param>
        public bool Fetch(bool collectStatistics)
        {
            foreach (InputField field in _varyingFields)
            {
                if (!field.Fetch(collectStatistics))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Changes an order of input data and resets the counter of processed inputs.
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
        /// Gets the collection of predictor descriptor objects of the inputs being routed to readout layer.
        /// </summary>
        public List<PredictorDescriptor> GetPredictorsDescriptorsOfRoutedInputs()
        {
            //Build descriptors
            List<PredictorDescriptor> result = new List<PredictorDescriptor>(_routedSteadyFieldIndexCollection.Count + _routedVaryingFieldCollection.Count * NumOfTimePoints);
            //Steady fields
            foreach (int idx in _routedSteadyFieldIndexCollection)
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
        /// Copies all routed inputs into a buffer.
        /// </summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="fromOffset">The zero-based position within the buffer where to start copying to.</param>
        /// <returns>The number of copied values.</returns>
        public int CopyRoutedInputsTo(double[] buffer, int fromOffset)
        {
            int count = 0;
            //Steady fields
            foreach (int idx in _routedSteadyFieldIndexCollection)
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
