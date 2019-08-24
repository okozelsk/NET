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
        private Dictionary<string, Normalizer> _fieldTypeIniNormalizerCollection;
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
        public BundleNormalizer(Interval normRange)
        {
            _fieldTypeIniNormalizerCollection = new Dictionary<string, Normalizer>();
            _fieldTypeNormalizerCollection = new Dictionary<string, Normalizer>();
            _fieldNameTypeCollection = new Dictionary<string, string>();
            _fieldNameCollection = new List<string>();
            _inputFieldNameCollection = new List<string>();
            _outputFieldNameCollection = new List<string>();
            _outputFieldAdjustmentSwitches = null;
            NormRange = normRange.DeepClone();
            InputFieldNormalizerRefCollection = new List<Normalizer>();
            OutputFieldNormalizerRefCollection = new List<Normalizer>();
            return;
        }

        //Methods
        private void ResetNormalizers()
        {
            foreach(string fieldType in _fieldTypeIniNormalizerCollection.Keys)
            {
                _fieldTypeNormalizerCollection[fieldType].Adopt(_fieldTypeIniNormalizerCollection[fieldType]);
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
        /// <param name="normReserveRatio">Reserve held by the normalizer to cover cases where future data exceeds a known range of sample data.</param>
        /// <param name="dataStandardization">Specifies whether to apply data standardization</param>
        /// <param name="fixedMin">Normalizer fixed min</param>
        /// <param name="fixedMax">Normalizer fixed max</param>
        public void DefineField(string name, string type, double normReserveRatio, bool dataStandardization, double fixedMin = double.NaN, double fixedMax = double.NaN)
        {
            if (_outputFieldAdjustmentSwitches != null)
            {
                throw new Exception($"Can't define field, structure is finalized.");
            }
            if (IsFieldDefined(name))
            {
                throw new ArgumentException($"Field {name} is already defined", "name");
            }
            if (!_fieldTypeIniNormalizerCollection.ContainsKey(type))
            {
                Normalizer iniNormalizer = new Normalizer(NormRange, normReserveRatio, dataStandardization);
                if (fixedMin.IsValid())
                {
                    iniNormalizer.Adjust(fixedMin);
                }
                if (fixedMax.IsValid())
                {
                    iniNormalizer.Adjust(fixedMax);
                }
                _fieldTypeIniNormalizerCollection.Add(type, iniNormalizer);
                _fieldTypeNormalizerCollection.Add(type, new Normalizer(iniNormalizer));
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
            InputFieldNormalizerRefCollection.Add(_fieldTypeNormalizerCollection[_fieldNameTypeCollection[name]]);
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
            OutputFieldNormalizerRefCollection.Add(_fieldTypeNormalizerCollection[_fieldNameTypeCollection[name]]);
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
            foreach (double[] outputVector in bundle.OutputVectorCollection)
            {
                AdjustOutputNormalizers(outputVector);
            }
            return;
        }

        /// <summary>
        /// Adjusts internal normalizers
        /// </summary>
        /// <param name="bundle">Sample data bundle</param>
        public void AdjustNormalizers(VectorBundle bundle)
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
                inputVector[i] = InputFieldNormalizerRefCollection[i].Denormalize(inputVector[i]);
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
                outputVector[i] = OutputFieldNormalizerRefCollection[i].Denormalize(outputVector[i]);
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
        public void Normalize(VectorBundle bundle)
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
        public void Naturalize(VectorBundle bundle)
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

    }//BundleNormalizer

}//Namespace
