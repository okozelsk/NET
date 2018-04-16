using System;
using System.Collections.Generic;
using RCNet.MathTools;
using RCNet.Extensions;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// Helper class for easy standardization and normalization/naturalization of sample data
    /// </summary>
    [Serializable]
    public class BundleNormalizer
    {
        //Attributes
        private Dictionary<string, Normalizer> _fieldTypeNormalizerCollection;
        private Dictionary<string, string> _fieldNameTypeCollection;
        private List<string> _fieldNameCollection;
        private List<string> _inputFieldNameCollection;
        private List<string> _outputFieldNameCollection;
        private bool[] _outputFieldAdjustmentSwitches;

        //Attribute properties
        /// <summary>
        /// Range of normalized values
        /// </summary>
        public Interval NormRange { get; }
        /// <summary>
        /// Reserve held by the input fields normalizers to cover cases where future data exceeds a known range of sample data.
        /// </summary>
        public double InputNormReserveRatio { get; }
        /// <summary>
        /// Specifies whether to apply input data standardization
        /// </summary>
        public bool InputStandardization { get; }
        /// <summary>
        /// Reserve held by the output fields normalizers to cover cases where future data exceeds a known range of sample data.
        /// </summary>
        public double OutputNormReserveRatio { get; }
        /// <summary>
        /// Specifies whether to apply output data standardization
        /// </summary>
        public bool OutputStandardization { get; }
        /// <summary>
        /// Collection of input fields normalizers.
        /// </summary>
        public List<Normalizer> InputFieldNormalizerRefCollection { get; }
        /// <summary>
        /// Collection of output fields normalizers.
        /// </summary>
        public List<Normalizer> OutputFieldNormalizerRefCollection { get; }

        //Constructors
        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="normRange">
        /// Range of normalized values
        /// </param>
        /// <param name="inputNormReserveRatio">
        /// Reserve held by the input fields normalizers to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="inputDataStandardization">
        /// Specifies whether to apply data standardization to input data
        /// </param>
        /// <param name="outputNormReserveRatio">
        /// Reserve held by the output fields normalizers to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="outputDataStandardization">
        /// Specifies whether to apply data standardization to output data
        /// </param>
        public BundleNormalizer(Interval normRange,
                                double inputNormReserveRatio,
                                bool inputDataStandardization,
                                double outputNormReserveRatio,
                                bool outputDataStandardization
                                )
        {
            _fieldTypeNormalizerCollection = new Dictionary<string, Normalizer>();
            _fieldNameTypeCollection = new Dictionary<string, string>();
            _fieldNameCollection = new List<string>();
            _inputFieldNameCollection = new List<string>();
            _outputFieldNameCollection = new List<string>();
            _outputFieldAdjustmentSwitches = null;
            NormRange = normRange.DeepClone();
            InputNormReserveRatio = inputNormReserveRatio;
            InputStandardization = inputDataStandardization;
            OutputNormReserveRatio = outputNormReserveRatio;
            OutputStandardization = outputDataStandardization;
            InputFieldNormalizerRefCollection = new List<Normalizer>();
            OutputFieldNormalizerRefCollection = new List<Normalizer>();
            return;
        }

        //Methods
        private void ResetNormalizers()
        {
            foreach(Normalizer normalizer in _fieldTypeNormalizerCollection.Values)
            {
                normalizer.Reset();
            }
            return;
        }

        /// <summary>
        /// Determines whether the specified field is defined.
        /// </summary>
        /// <param name="name">Field name</param>
        public bool IsFieldDefined(string name)
        {
            return _fieldNameTypeCollection.ContainsKey(name);
        }

        /// <summary>
        /// Defines a data field that can then be used in input fields or output fields.
        /// The normalizer instance binds to the data field type.
        /// Specifying the same type name on different data fields causes the data fields to be grouped under
        /// one instance of the normalizer.
        /// </summary>
        /// <param name="name">Field name</param>
        /// <param name="type">Field type</param>
        public void DefineField(string name, string type)
        {
            if (_outputFieldAdjustmentSwitches != null)
            {
                throw new Exception($"Can't define field, structure is finalized.");
            }
            if (IsFieldDefined(name))
            {
                throw new ArgumentException($"Field {name} is already defined", "name");
            }
            if (!_fieldTypeNormalizerCollection.ContainsKey(type))
            {
                _fieldTypeNormalizerCollection.Add(type, null);
            }
            _fieldNameTypeCollection.Add(name, type);
            _fieldNameCollection.Add(name);
            return;
        }

        /// <summary>
        /// Once the data field is defined (DefineField), it can be designated as an input and / or output field
        /// </summary>
        /// <param name="name">Field name</param>
        public void DefineInputField(string name)
        {
            if (_outputFieldAdjustmentSwitches != null)
            {
                throw new Exception($"Can't define input field, structure is finalized.");
            }
            if (!IsFieldDefined(name))
            {
                throw new ArgumentException($"Field {name} is not defined", "name");
            }
            if(_inputFieldNameCollection.IndexOf(name) >= 0)
            {
                throw new ArgumentException($"Input field name {name} is already defined", "name");
            }
            string type = _fieldNameTypeCollection[name];
            //Instantiate a normalizer if necessary
            if(_fieldTypeNormalizerCollection[type] == null)
            {
                _fieldTypeNormalizerCollection[type] = new Normalizer(NormRange, InputNormReserveRatio, InputStandardization);
            }
            InputFieldNormalizerRefCollection.Add(_fieldTypeNormalizerCollection[type]);
            _inputFieldNameCollection.Add(name);
            return;
        }

        /// <summary>
        /// Once the data field is defined (DefineField), it can be designated as an input and / or output field
        /// </summary>
        /// <param name="name">Field name</param>
        public void DefineOutputField(string name)
        {
            if (_outputFieldAdjustmentSwitches != null)
            {
                throw new Exception($"Can't define output field, structure is finalized.");
            }
            if (!IsFieldDefined(name))
            {
                throw new ArgumentException($"Field {name} is not defined", "name");
            }
            if (_outputFieldNameCollection.IndexOf(name) >= 0)
            {
                throw new ArgumentException($"Output field name {name} is already defined", "name");
            }
            string type = _fieldNameTypeCollection[name];
            //Instantiate a normalizer if necessary
            if (_fieldTypeNormalizerCollection[type] == null)
            {
                _fieldTypeNormalizerCollection[type] = new Normalizer(NormRange, OutputNormReserveRatio, OutputStandardization);
            }
            OutputFieldNormalizerRefCollection.Add(_fieldTypeNormalizerCollection[type]);
            _outputFieldNameCollection.Add(name);
            return;
        }

        /// <summary>
        /// Finalizes the field definition.
        /// Data operations are only allowed after the internal structure has been finalized.
        /// </summary>
        public void FinalizeStructure()
        {
            if(_outputFieldAdjustmentSwitches != null)
            {
                throw new Exception($"Can't finalize structure, structure is already finalized.");
            }
            _outputFieldAdjustmentSwitches = new bool[_outputFieldNameCollection.Count];
            _outputFieldAdjustmentSwitches.Populate(true);
            for(int i = 0; i < _outputFieldNameCollection.Count; i++)
            {
                //The same field is defined in input fields so danny normalizer adjustment
                if(_inputFieldNameCollection.IndexOf(_outputFieldNameCollection[i]) >= 0)
                {
                    _outputFieldAdjustmentSwitches[i] = false;
                }
            }
            return;
        }

        /// <summary>
        /// Checkes if the structure is finalized
        /// </summary>
        private void CheckStructure()
        {
            if (_outputFieldAdjustmentSwitches == null)
            {
                throw new Exception($"Structure is not finalized.");
            }
            return;
        }

        /// <summary>
        /// Adjusts normalizer instances associated with input fields
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        private void AdjustInputNormalizers(double[] inputVector)
        {
            CheckStructure();
            for (int i = 0; i < InputFieldNormalizerRefCollection.Count; i++)
            {
                InputFieldNormalizerRefCollection[i].Adjust(inputVector[i]);
            }
            return;
        }

        /// <summary>
        /// Adjusts normalizer instances associated with output fields
        /// </summary>
        /// <param name="outputVector">Output vector</param>
        private void AdjustOutputNormalizers(double[] outputVector)
        {
            CheckStructure();
            for (int i = 0; i < OutputFieldNormalizerRefCollection.Count; i++)
            {
                if (_outputFieldAdjustmentSwitches[i])
                {
                    OutputFieldNormalizerRefCollection[i].Adjust(outputVector[i]);
                }
            }
            return;
        }

        /// <summary>
        /// Adjusts internal normalizers
        /// </summary>
        /// <param name="bundle">Sample data bundle</param>
        public void AdjustNormalizers(PatternBundle bundle)
        {
            ResetNormalizers();
            foreach (List<double[]> pattern in bundle.InputPatternCollection)
            {
                foreach(double[] inputVector in pattern)
                {
                    AdjustInputNormalizers(inputVector);
                }
            }
            foreach(double[] outputVector in bundle.OutputVectorCollection)
            {
                AdjustOutputNormalizers(outputVector);
            }
            return;
        }

        /// <summary>
        /// Adjusts internal normalizers
        /// </summary>
        /// <param name="bundle">Sample data bundle</param>
        public void AdjustNormalizers(TimeSeriesBundle bundle)
        {
            ResetNormalizers();
            foreach (double[] inputVector in bundle.InputVectorCollection)
            {
                AdjustInputNormalizers(inputVector);
            }
            foreach (double[] outputVector in bundle.OutputVectorCollection)
            {
                AdjustOutputNormalizers(outputVector);
            }
            return;
        }

        /// <summary>
        /// Normalizes values in the input vector
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        public void NormalizeInputVector(double[] inputVector)
        {
            CheckStructure();
            for (int i = 0; i < InputFieldNormalizerRefCollection.Count; i++)
            {
                inputVector[i] = InputFieldNormalizerRefCollection[i].Normalize(inputVector[i]);
            }
            return;
        }

        /// <summary>
        /// Naturalizes values in the intput vector
        /// </summary>
        /// <param name="inputVector">Input vector</param>
        public void NaturalizeInputVector(double[] inputVector)
        {
            CheckStructure();
            for (int i = 0; i < InputFieldNormalizerRefCollection.Count; i++)
            {
                inputVector[i] = InputFieldNormalizerRefCollection[i].Naturalize(inputVector[i]);
            }
            return;
        }

        /// <summary>
        /// Normalizes values in the output vector
        /// </summary>
        /// <param name="outputVector">Output vector</param>
        public void NormalizeOutputVector(double[] outputVector)
        {
            CheckStructure();
            for (int i = 0; i < OutputFieldNormalizerRefCollection.Count; i++)
            {
                outputVector[i] = OutputFieldNormalizerRefCollection[i].Normalize(outputVector[i]);
            }
            return;
        }

        /// <summary>
        /// Naturalizes values in the output vector
        /// </summary>
        /// <param name="outputVector">Output vector</param>
        public void NaturalizeOutputVector(double[] outputVector)
        {
            CheckStructure();
            for (int i = 0; i < OutputFieldNormalizerRefCollection.Count; i++)
            {
                outputVector[i] = OutputFieldNormalizerRefCollection[i].Naturalize(outputVector[i]);
            }
            return;
        }

        /// <summary>
        /// Normalizes all values in the collection of input vectors
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        public void NormalizeInputVectorCollection(List<double[]> inputVectorCollection)
        {
            foreach (double[] inputVector in inputVectorCollection)
            {
                NormalizeInputVector(inputVector);
            }
            return;
        }

        /// <summary>
        /// Naturalizes all values in the collection of input vectors
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        public void NaturalizeInputVectorCollection(List<double[]> inputVectorCollection)
        {
            foreach (double[] inputVector in inputVectorCollection)
            {
                NaturalizeInputVector(inputVector);
            }
            return;
        }

        /// <summary>
        /// Normalizes all values in the collection of output vectors
        /// </summary>
        /// <param name="outputVectorCollection">Collection of output vectors</param>
        private void NormalizeOutputVectorCollection(List<double[]> outputVectorCollection)
        {
            foreach (double[] outputVector in outputVectorCollection)
            {
                NormalizeOutputVector(outputVector);
            }
            return;
        }

        /// <summary>
        /// Naturalizes all values in the collection of output vectors
        /// </summary>
        /// <param name="outputVectorCollection">Collection of output vectors</param>
        private void NaturalizeOutputVectorCollection(List<double[]> outputVectorCollection)
        {
            foreach (double[] outputVector in outputVectorCollection)
            {
                NaturalizeOutputVector(outputVector);
            }
            return;
        }

        /// <summary>
        /// Normalizes all values in the sample data bundle
        /// </summary>
        /// <param name="bundle">Sample data bundle</param>
        public void Normalize(TimeSeriesBundle bundle)
        {
            AdjustNormalizers(bundle);
            NormalizeInputVectorCollection(bundle.InputVectorCollection);
            NormalizeOutputVectorCollection(bundle.OutputVectorCollection);
            return;
        }

        /// <summary>
        /// Naturalizes all values in the sample data bundle
        /// </summary>
        /// <param name="bundle">Sample data bundle</param>
        public void Naturalize(TimeSeriesBundle bundle)
        {
            NaturalizeInputVectorCollection(bundle.InputVectorCollection);
            NaturalizeOutputVectorCollection(bundle.OutputVectorCollection);
            return;
        }

        /// <summary>
        /// Normalizes all values in the sample data bundle
        /// </summary>
        /// <param name="bundle">Sample data bundle</param>
        public void Normalize(PatternBundle bundle)
        {
            AdjustNormalizers(bundle);
            foreach (List<double[]> pattern in bundle.InputPatternCollection)
            {
                NormalizeInputVectorCollection(pattern);
            }
            NormalizeOutputVectorCollection(bundle.OutputVectorCollection);
            return;
        }

        /// <summary>
        /// Naturalizes all values in the sample data bundle
        /// </summary>
        /// <param name="bundle">Sample data bundle</param>
        public void Naturalize(PatternBundle bundle)
        {
            foreach (List<double[]> pattern in bundle.InputPatternCollection)
            {
                NaturalizeInputVectorCollection(pattern);
            }
            NaturalizeOutputVectorCollection(bundle.OutputVectorCollection);
            return;
        }

        /// <summary>
        /// Creates PredictionBundle from the vector collection
        /// </summary>
        /// <param name="vectorCollection">Collection of vectors</param>
        /// <param name="normalize">Specifies whether to normalize data in the created bundle</param>
        /// <param name="bundle">Created bundle</param>
        /// <returns>The last unused vector</returns>
        public double[] CreateBundleFromVectorCollection(List<double[]> vectorCollection,
                                                         bool normalize,
                                                         out TimeSeriesBundle bundle
                                                         )
        {
            CheckStructure();
            if (vectorCollection[0].Length != _fieldNameTypeCollection.Count)
            {
                throw new ArgumentException($"Inconsistent number of fields ({vectorCollection[0].Length}) in vectorCollection and number of defined fields ({_fieldNameTypeCollection.Count}).", "vectorCollection");
            }
            //Input field indexes
            int[] inputFieldIdxs = new int[_inputFieldNameCollection.Count];
            for(int i = 0; i < _inputFieldNameCollection.Count; i++)
            {
                inputFieldIdxs[i] = _fieldNameCollection.IndexOf(_inputFieldNameCollection[i]);
            }
            //Output field indexes
            int[] outputFieldIdxs = new int[_outputFieldNameCollection.Count];
            for (int i = 0; i < _outputFieldNameCollection.Count; i++)
            {
                outputFieldIdxs[i] = _fieldNameCollection.IndexOf(_outputFieldNameCollection[i]);
            }
            double[] remainingInputVector = null;
            bundle = new TimeSeriesBundle();
            for(int row = 0; row < vectorCollection.Count; row++)
            {
                //Input vector
                double[] inputVector = new double[inputFieldIdxs.Length];
                for(int i = 0; i < inputFieldIdxs.Length; i++)
                {
                    inputVector[i] = vectorCollection[row][inputFieldIdxs[i]];
                }
                if(row < vectorCollection.Count - 1)
                {
                    bundle.InputVectorCollection.Add(inputVector);
                }
                else
                {
                    remainingInputVector = inputVector;
                }
                //Output vector
                if (row > 0)
                {
                    double[] outputVector = new double[outputFieldIdxs.Length];
                    for (int i = 0; i < outputFieldIdxs.Length; i++)
                    {
                        outputVector[i] = vectorCollection[row][outputFieldIdxs[i]];
                    }
                    bundle.OutputVectorCollection.Add(outputVector);
                }
            }
            //Normalization ?
            if(normalize)
            {
                Normalize(bundle);
                NormalizeInputVector(remainingInputVector);
            }
            return remainingInputVector;
        }

    }//BundleNormalizer

}//Namespace
