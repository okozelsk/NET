using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Globalization;
using System.Text;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.MathTools.Hurst;
using RCNet.RandomValue;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir;
using RCNet.Neural.Network.SM.Preprocessing.Reservoir.Neuron;
using RCNet.Neural.Network.SM.Preprocessing.Input;
using RCNet.Neural.Data.Transformers;

namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Implements the neural data preprocessor, one of the main components of the State Machine.
    /// </summary>
    [Serializable]
    public class NeuralPreprocessor
    {
        //Constants
        private const double MinPredictorValueDifference = 1e-6;

        //Enums
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

        //Delegates
        /// <summary>
        /// Delegate of PreprocessingProgressChanged event handler.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="finalPreprocessingOverview">Final overview of the preprocessing phase</param>
        public delegate void PreprocessingProgressChangedDelegate(int totalNumOfInputs,
                                                                  int numOfProcessedInputs,
                                                                  PreprocessingOverview finalPreprocessingOverview
                                                                  );
        //Events
        /// <summary>
        /// This informative event occurs every time the progress of neural preprocessing has changed
        /// </summary>
        [field: NonSerialized]
        public event PreprocessingProgressChangedDelegate PreprocessingProgressChanged;


        //Attribute properties
        /// <summary>
        /// Collection of reservoir instances.
        /// </summary>
        public List<ReservoirInstance> ReservoirCollection { get; }

        /// <summary>
        /// All predicting neurons.
        /// </summary>
        public List<HiddenNeuron> PredictingNeuronCollection { get; }

        /// <summary>
        /// Number of boot cycles
        /// </summary>
        public int BootCycles { get; }

        /// <summary>
        /// Number of neurons
        /// </summary>
        public int NumOfNeurons { get; }

        /// <summary>
        /// -1 (no constraint) for continuous feeding or patterned feeding without routing input to readout.
        /// Required constant length of the input pattern for patterned feeding with routing input to readout.
        /// </summary>
        public int InputPatternFixedLengthConstraint { get; private set; }

        /// <summary>
        /// Total number of predictors
        /// </summary>
        public int TotalNumOfPredictors { get; private set; }

        /// <summary>
        /// Number of active predictors
        /// </summary>
        public int NumOfActivePredictors { get; private set; }

        /// <summary>
        /// Collection of switches generally enabling/disabling predictors
        /// </summary>
        public bool[] PredictorGeneralSwitchCollection { get; private set; }


        //Attributes
        /// <summary>
        /// Settings used for instance creation.
        /// </summary>
        private readonly NeuralPreprocessorSettings _settings;

        /// <summary>
        /// Collection of the internal input transformers associated with the transformed input fields
        /// </summary>
        private readonly List<ITransformer> _internalInputTransformerCollection;

        /// <summary>
        /// Collection of the internal input generators associated with the generated input fields
        /// </summary>
        private readonly List<IGenerator> _internalInputGeneratorCollection;

        /// <summary>
        /// Collection of input feature filters
        /// </summary>
        private readonly BaseFeatureFilter[] _featureFilterCollection;

        //Constructor
        /// <summary>
        /// Constructs an initialized instance
        /// </summary>
        /// <param name="settings">Neural Preprocessor's configuration</param>
        /// <param name="randomizerSeek">
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same network structure, which is good for tuning
        /// network parameters. A value less than 0 causes a fully random initialization when creating a network instance.
        /// </param>
        public NeuralPreprocessor(NeuralPreprocessorSettings settings, int randomizerSeek)
        {
            _settings = (NeuralPreprocessorSettings)settings.DeepClone();
            ///////////////////////////////////////////////////////////////////////////////////
            //Input
            //Internal input transformers
            _internalInputTransformerCollection = new List<ITransformer>();
            if (_settings.InputCfg.FieldsCfg.TransformedFieldsCfg != null)
            {
                List<string> names = _settings.InputCfg.FieldsCfg.GetNames();
                foreach (TransformedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                {
                    _internalInputTransformerCollection.Add(TransformerFactory.Create(names, fieldCfg.TransformerCfg));
                }
            }
            //Internal input generators
            _internalInputGeneratorCollection = new List<IGenerator>();
            if (_settings.InputCfg.FieldsCfg.GeneratedFieldsCfg != null)
            {
                foreach (GeneratedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                {
                    _internalInputGeneratorCollection.Add(GeneratorFactory.Create(fieldCfg.GeneratorCfg));
                }
            }
            ///////////////////////////////////////////////////////////////////////////////////
            //Feature filters
            _featureFilterCollection = new BaseFeatureFilter[_settings.InputCfg.FieldsCfg.TotalNumOfFields];
            int featureIdx = 0;
            //External fields filters
            foreach (ExternalFieldSettings externalFieldCfg in _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
            {
                _featureFilterCollection[featureIdx++] = FeatureFilterFactory.Create(_dataRange, externalFieldCfg.FeatureFilterCfg);
            }
            //Transformed fields filters
            if (_settings.InputCfg.FieldsCfg.TransformedFieldsCfg != null)
            {
                foreach (TransformedFieldSettings transformedFieldCfg in _settings.InputCfg.FieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                {
                    _featureFilterCollection[featureIdx++] = new RealFeatureFilter(_dataRange);
                }
            }
            //Generated fields filters
            if (_settings.InputCfg.FieldsCfg.GeneratedFieldsCfg != null)
            {
                foreach (GeneratedFieldSettings generatedFieldCfg in _settings.InputCfg.FieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                {
                    _featureFilterCollection[featureIdx++] = new RealFeatureFilter(_dataRange);
                }
            }
            ///////////////////////////////////////////////////////////////////////////////////
            //Reservoir instance(s)
            BootCycles = 0;
            //Random generator used for reservoir structure initialization
            Random rand = (randomizerSeek < 0 ? new Random() : new Random(randomizerSeek));
            PredictingNeuronCollection = new List<HiddenNeuron>();
            NumOfNeurons = 0;
            InputPatternFixedLengthConstraint = -1;
            TotalNumOfPredictors = -1;
            NumOfActivePredictors = 0;
            PredictorGeneralSwitchCollection = null;
            ReservoirCollection = new List<ReservoirInstance>(_settings.ReservoirInstancesCfg.ReservoirInstanceCfgCollection.Count);
            int reservoirInstanceID = 0;
            int biggestInterconnectedArea = 0;
            foreach(ReservoirInstanceSettings reservoirInstanceCfg in _settings.ReservoirInstancesCfg.ReservoirInstanceCfgCollection)
            {
                ReservoirStructureSettings structCfg = _settings.ReservoirStructuresCfg.GetReservoirStructureCfg(reservoirInstanceCfg.StructureCfgName);
                ReservoirInstance reservoir = new ReservoirInstance(reservoirInstanceID++,
                                                                    _settings.InputCfg,
                                                                    structCfg,
                                                                    reservoirInstanceCfg,
                                                                    _dataRange,
                                                                    rand
                                                                    );
                ReservoirCollection.Add(reservoir);
                PredictingNeuronCollection.AddRange(reservoir.PredictingNeuronCollection);
                NumOfNeurons += reservoir.Size;
                biggestInterconnectedArea = Math.Max(biggestInterconnectedArea, structCfg.LargestInterconnectedAreaSize);
            }
            if (_settings.InputCfg.FeedingCfg.FeedingType == InputFeedingType.Continuous)
            {
                FeedingContinuousSettings feedCfg = (FeedingContinuousSettings)settings.InputCfg.FeedingCfg;
                BootCycles = feedCfg.BootCycles == FeedingContinuousSettings.AutoBootCyclesNum ? biggestInterconnectedArea : feedCfg.BootCycles;
            }
            else
            {
                BootCycles = 0;
            }
            return;
        }

        //Properties
        /// <summary>
        /// Number of suppressed predictors (exhibits no meaningfully different values)
        /// </summary>
        public int NumOfSuppressedPredictors { get { return TotalNumOfPredictors - NumOfActivePredictors; } }

        //Methods
        private void InitTotalNumOfPredictors(int externalInputPatternLength = -1)
        {
            TotalNumOfPredictors = 0;
            //Input fields as the predictors
            int numOfInpFieldInstances = _settings.InputCfg.FeedingCfg.FeedingType == InputFeedingType.Patterned ? externalInputPatternLength : 1;
            if (_settings.InputCfg.FeedingCfg.RouteToReadout)
            {
                InputPatternFixedLengthConstraint = externalInputPatternLength;
                foreach (ExternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
                {
                    if (fieldCfg.RouteToReadout) TotalNumOfPredictors += numOfInpFieldInstances;
                }
                if (_settings.InputCfg.FieldsCfg.TransformedFieldsCfg != null)
                {
                    foreach (TransformedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout) TotalNumOfPredictors += numOfInpFieldInstances;
                    }
                }
                if (_settings.InputCfg.FieldsCfg.GeneratedFieldsCfg != null)
                {
                    foreach (GeneratedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout) TotalNumOfPredictors += numOfInpFieldInstances;
                    }
                }
            }
            //All reservoirs' predictors
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                bool bidir = _settings.InputCfg.FeedingCfg.FeedingType == InputFeedingType.Patterned && ((FeedingPatternedSettings)_settings.InputCfg.FeedingCfg).Bidir;
                TotalNumOfPredictors += (bidir ? 2 : 1) * reservoir.NumOfPredictors;
            }
            return;
        }

        /// <summary>
        /// Function checks given predictors and set general enabling/disabling switches of predictors
        /// </summary>
        /// <param name="predictorsCollection">Collection of regression predictors</param>
        private void InitPredictorsGeneralSwitches(List<double[]> predictorsCollection)
        {
            //Allocate general predictor switches
            PredictorGeneralSwitchCollection = new bool[TotalNumOfPredictors];
            //Init general predictor switches to false
            PredictorGeneralSwitchCollection.Populate(false);
            //Compute statistics
            List<Tuple<int, double, BasicStat>> predictorStatCollection = new List<Tuple<int, double, BasicStat>>(TotalNumOfPredictors);
            RescalledRange[] rescalledRangeCollection = new RescalledRange[TotalNumOfPredictors];
            BasicStat[] basicStatCollection = new BasicStat[TotalNumOfPredictors];
            for (int i = 0; i < TotalNumOfPredictors; i++)
            {
                RescalledRange rescalledRange = new RescalledRange(predictorsCollection.Count);
                BasicStat basicStat = new BasicStat();
                for (int row = 0; row < predictorsCollection.Count; row++)
                {
                    rescalledRange.AddValue(predictorsCollection[row][i]);
                    basicStat.AddSampleValue(predictorsCollection[row][i]);
                }
                predictorStatCollection.Add(new Tuple<int, double, BasicStat>(i, rescalledRange.Compute(), basicStat));
            }
            //Create sorted statistics
            List<Tuple<int, double, BasicStat>> sortedPredictorStatCollection = new List<Tuple<int, double, BasicStat>>(predictorStatCollection.OrderByDescending(k => k.Item2));
            int reductionCount = (int)(Math.Round(TotalNumOfPredictors * _settings.PredictorsReductionRatio));
            int firstInvalidOrderIndex = TotalNumOfPredictors - reductionCount;
            //Enable valid predictors
            NumOfActivePredictors = 0;
            for (int i = 0; i < TotalNumOfPredictors; i++)
            {
                if(sortedPredictorStatCollection[i].Item3.Span > MinPredictorValueDifference && i < firstInvalidOrderIndex)
                {
                    //Enable predictor
                    PredictorGeneralSwitchCollection[sortedPredictorStatCollection[i].Item1] = true;
                    ++NumOfActivePredictors;
                }
            }
            return;
        }

        /// <summary>
        /// Sets Neural Preprocessor internal state to initial state
        /// </summary>
        /// <param name="resetStatistics">Specifies whether to reset internal statistics</param>
        public void Reset(bool resetStatistics)
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
            //Reset reservoirs
            foreach(ReservoirInstance reservoir in ReservoirCollection)
            {
                reservoir.Reset(resetStatistics);
            }
            return;
        }

        private void ResetFeatureFilters()
        {
            foreach(BaseFeatureFilter filter in _featureFilterCollection)
            {
                filter.Reset();
            }
            return;
        }

        /// <summary>
        /// Adds inputs from internal transformers and generators to be used in reservoirs.
        /// </summary>
        /// <param name="externalInputVector">External input vector</param>
        private double[] AddInternalInputs(double[] externalInputVector)
        {
            if(_settings.InputCfg.FieldsCfg.TotalNumOfFields == externalInputVector.Length)
            {
                //Defined no internal fields
                return externalInputVector;
            }
            double[] inputVector = new double[_settings.InputCfg.FieldsCfg.TotalNumOfFields];
            inputVector.Populate(double.NaN);
            //Original external data
            externalInputVector.CopyTo(inputVector, 0);
            int index = externalInputVector.Length;
            //Transformed fields
            foreach(ITransformer transformer in _internalInputTransformerCollection)
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

        private List<double[]> AddInternalInputs(List<double[]> inputVectors)
        {
            List<double[]> outputVectors = new List<double[]>(inputVectors.Count);
            for(int i = 0; i < inputVectors.Count; i++)
            {
                outputVectors.Add(AddInternalInputs(inputVectors[i]));
            }
            return outputVectors;
        }

        /// <summary>
        /// Adds inputs from internal transformers and generators to be used in reservoirs.
        /// </summary>
        /// <param name="inputPattern">External input pattern</param>
        private InputPattern AddInternalInputs(InputPattern inputPattern)
        {
            int inputPatternTimePoints = inputPattern.VariablesDataCollection[0].Length;
            //Convert to rich vectors
            List<double[]> richVectors = new List<double[]>(inputPatternTimePoints);
            for(int timePointIndex = 0; timePointIndex < inputPatternTimePoints; timePointIndex++)
            {
                double[] externalInputVector = inputPattern.GetDataAtTimePoint(timePointIndex);
                double[] richVector = AddInternalInputs(externalInputVector);
                richVectors.Add(richVector);
            }
            //Convert back to pattern
            InputPattern outputPattern = new InputPattern(_settings.InputCfg.FieldsCfg.TotalNumOfFields);
            for(int varIdx = 0; varIdx < _settings.InputCfg.FieldsCfg.TotalNumOfFields; varIdx++)
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
        /// Updates collection of preprocessor's feature filters
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        private void UpdateFeatureFilters(List<double[]> inputVectorCollection)
        {
            Parallel.For(0, _featureFilterCollection.Length, i =>
            {
                //Update filter
                foreach (double[] vector in inputVectorCollection)
                {
                    _featureFilterCollection[i].Update(vector[i]);
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
            Parallel.For(0, _settings.InputCfg.FieldsCfg.TotalNumOfFields, varIdx =>
            {
                foreach (InputPattern pattern in inputPatternCollection)
                {
                    for(int i = 0; i < pattern.VariablesDataCollection[varIdx].Length; i++)
                    {
                        _featureFilterCollection[varIdx].Update(pattern.VariablesDataCollection[varIdx][i]);
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Applies filters on vector of features
        /// </summary>
        /// <param name="vector">Input vector</param>
        private double[] ApplyFiltersOnInputVector(double[] vector)
        {
            double[] resultVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                resultVector[i] = _featureFilterCollection[i].ApplyFilter(vector[i]);
            }
            return resultVector;
        }

        /// <summary>
        /// Applies feature filters on pattern
        /// </summary>
        /// <param name="pattern">Input pattern</param>
        private InputPattern ApplyFiltersOnInputPattern(InputPattern pattern)
        {
            InputPattern resultPattern = new InputPattern(pattern.VariablesDataCollection.Count);
            int varIdx = 0;
            foreach (double[] vector in pattern.VariablesDataCollection)
            {
                double[] resultVector = new double[vector.Length];
                for(int i = 0; i < vector.Length; i++)
                {
                    resultVector[i] = _featureFilterCollection[varIdx].ApplyFilter(vector[i]);
                }
                resultPattern.VariablesDataCollection.Add(resultVector);
                ++varIdx;
            }
            return resultPattern;
        }

        /// <summary>
        /// Pushes input vector into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="inputVector">Input values</param>
        /// <param name="collectStatistics">
        /// The parameter indicates whether to update internal statistics
        /// </param>
        private double[] PushInputVector(double[] inputVector, bool collectStatistics)
        {
            double[] normalizedInputVector = ApplyFiltersOnInputVector(inputVector);
            double[] predictors = new double[TotalNumOfPredictors];
            int predictorsIdx = 0;
            //Compute reservoir(s)
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InputUnitCollection.Length];
                for (int i = 0; i < reservoir.InputUnitCollection.Length; i++)
                {
                    reservoirInput[i] = normalizedInputVector[reservoir.InputUnitCollection[i].InputFieldIdx];
                }
                //Compute reservoir
                reservoir.Compute(reservoirInput, collectStatistics);
                reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                predictorsIdx += reservoir.NumOfPredictors;
            }
            if (_settings.InputCfg.FeedingCfg.RouteToReadout)
            {
                int fieldIdx = 0;
                foreach (ExternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
                {
                    if (fieldCfg.RouteToReadout)
                    {
                        //Route original values
                        predictors[predictorsIdx++] = inputVector[fieldIdx];
                    }
                    ++fieldIdx;
                }
                if (_settings.InputCfg.FieldsCfg.TransformedFieldsCfg != null)
                {
                    foreach (TransformedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            predictors[predictorsIdx++] = normalizedInputVector[fieldIdx];
                        }
                        ++fieldIdx;
                    }
                }
                if (_settings.InputCfg.FieldsCfg.GeneratedFieldsCfg != null)
                {
                    foreach (GeneratedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            predictors[predictorsIdx++] = normalizedInputVector[fieldIdx];
                        }
                        ++fieldIdx;
                    }
                }
            }
            return predictors;
        }

        /// <summary>
        /// Pushes input pattern into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="inputPattern">Input pattern</param>
        /// <param name="collectStatistics">
        /// The parameter indicates whether to update internal statistics
        /// </param>
        private double[] PushInputPattern(InputPattern inputPattern, bool collectStatistics)
        {
            double[] predictors = new double[TotalNumOfPredictors];
            int predictorsIdx = 0;
            //Check pattern length costraint
            if(InputPatternFixedLengthConstraint != -1)
            {
                if(inputPattern.VariablesDataCollection[0].Length != InputPatternFixedLengthConstraint)
                {
                    throw new Exception($"Incorrect length of input pattern ({inputPattern.VariablesDataCollection[0].Length}). Length must be equal to {InputPatternFixedLengthConstraint}.");
                }
            }
            //Reset SM but keep statistics
            Reset(false);
            //Apply filters
            InputPattern normalizedInputPattern = ApplyFiltersOnInputPattern(inputPattern);
            //Compute reservoir(s)
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceCfg.InputUnitsCfg.InputUnitCfgCollection.Count];
                List<InputPattern> stepInputPatterns = new List<InputPattern>() { normalizedInputPattern };
                if(((FeedingPatternedSettings)_settings.InputCfg.FeedingCfg).Bidir)
                {
                    InputPattern reversedInputPattern = new InputPattern(normalizedInputPattern.VariablesDataCollection.Count);
                    foreach(double[] data in normalizedInputPattern.VariablesDataCollection)
                    {
                        reversedInputPattern.VariablesDataCollection.Add(data.Reverse().ToArray());
                    }
                    stepInputPatterns.Add(reversedInputPattern);
                }
                foreach (InputPattern stepInputPattern in stepInputPatterns)
                {
                    for(int idx = 0; idx < stepInputPattern.VariablesDataCollection[0].Length; idx++)
                    {
                        double[] inputVector = stepInputPattern.GetDataAtTimePoint(idx);
                        for (int i = 0; i < reservoir.InputUnitCollection.Length; i++)
                        {
                            reservoirInput[i] = inputVector[reservoir.InputUnitCollection[i].InputFieldIdx];
                        }
                        //Compute the reservoir
                        reservoir.Compute(reservoirInput, collectStatistics);
                    }
                    reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                    predictorsIdx += reservoir.NumOfPredictors;
                }
            }

            //Route input fields as the predictors
            if (_settings.InputCfg.FeedingCfg.RouteToReadout)
            {
                int fieldIdx = 0;
                foreach (ExternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
                {
                    if (fieldCfg.RouteToReadout)
                    {
                        //Route to readout
                        for(int i = 0; i < inputPattern.VariablesDataCollection[fieldIdx].Length; i++)
                        {
                            predictors[predictorsIdx++] = inputPattern.VariablesDataCollection[fieldIdx][i];
                        }
                    }
                    ++fieldIdx;
                }
                if (_settings.InputCfg.FieldsCfg.TransformedFieldsCfg != null)
                {
                    foreach (TransformedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.TransformedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            //Route to readout
                            for (int i = 0; i < normalizedInputPattern.VariablesDataCollection[fieldIdx].Length; i++)
                            {
                                predictors[predictorsIdx++] = normalizedInputPattern.VariablesDataCollection[fieldIdx][i];
                            }
                        }
                        ++fieldIdx;
                    }
                }
                if (_settings.InputCfg.FieldsCfg.GeneratedFieldsCfg != null)
                {
                    foreach (GeneratedFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.GeneratedFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            //Route to readout
                            for (int i = 0; i < normalizedInputPattern.VariablesDataCollection[fieldIdx].Length; i++)
                            {
                                predictors[predictorsIdx++] = normalizedInputPattern.VariablesDataCollection[fieldIdx][i];
                            }
                        }
                        ++fieldIdx;
                    }
                }
            }
            return predictors;
        }

        /// <summary>
        /// Pushes input vector into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="input">Input values in natural form</param>
        public double[] Preprocess(double[] input)
        {
            //Check readyness
            if (_featureFilterCollection == null)
            {
                throw new Exception("Preprocessor was not initialized by the sample data bundle (feature filters are not instantiated yet).");
            }
            if(_settings.InputCfg.FeedingCfg.FeedingType == InputFeedingType.Continuous)
            {
                //Push input vector into the preprocessor and return result
                return PushInputVector(AddInternalInputs(input), false);
            }
            else
            {
                FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_settings.InputCfg.FeedingCfg;
                InputPattern inputPattern = new InputPattern(input,
                                                             _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
                                                             feedingCfg.VarSchema,
                                                             feedingCfg.UnificationCfg.Detrend,
                                                             feedingCfg.UnificationCfg.UnifyAmplitude,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.SignalBeginThreshold,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.SignalEndThreshold,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.UniformTimeScale,
                                                             feedingCfg.UnificationCfg.ResamplingCfg.TargetTimePoints
                                                             );

                //Push input pattern into the preprocessor and return result
                return PushInputPattern(AddInternalInputs(inputPattern), false);
            }
        }


        /// <summary>
        /// Continuous feeding version
        /// </summary>
        private VectorBundle PreprocessVectorBundle(VectorBundle inputVectorBundle, out PreprocessingOverview preprocessingOverview)
        {
            //Reset preprocessor
            Reset(true);
            ResetFeatureFilters();
            //Initialize total number of predictors
            InitTotalNumOfPredictors();
            //Add internal inputs
            List<double[]> inputVectors = AddInternalInputs(inputVectorBundle.InputVectorCollection);
            //Initialize feature filters
            UpdateFeatureFilters(inputVectors);
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(inputVectors.Count - BootCycles);
            //Collect predictors
            for (int dataSetIdx = 0; dataSetIdx < inputVectors.Count; dataSetIdx++)
            {
                bool afterBoot = (dataSetIdx >= BootCycles);
                //Push input data into the network
                double[] predictors = PushInputVector(inputVectors[dataSetIdx], afterBoot);
                //Is boot sequence passed? Collect predictors?
                if (afterBoot)
                {
                    //YES
                    //Predictors
                    outputBundle.InputVectorCollection.Add(predictors);
                    //Desired outputs
                    outputBundle.OutputVectorCollection.Add(inputVectorBundle.OutputVectorCollection[dataSetIdx]);
                }
                //Raise informative event
                PreprocessingProgressChanged(inputVectors.Count, dataSetIdx + 1, null);
            }

            //Predictor switches
            InitPredictorsGeneralSwitches(outputBundle.InputVectorCollection);

            //Preprocessing overview
            preprocessingOverview = new PreprocessingOverview(CollectStatatistics(),
                                                              NumOfNeurons,
                                                              NumOfSuppressedPredictors
                                                              );
            //Final informative event
            PreprocessingProgressChanged(inputVectors.Count, inputVectors.Count, preprocessingOverview);
            //Return
            return outputBundle;
        }

        /// <summary>
        /// Patterned feeding version
        /// </summary>
        private VectorBundle PreprocessPatternBundle(List<InputPattern> inputPatterns, List<double[]> outputs, out PreprocessingOverview preprocessingOverview)
        {
            //Reset preprocessor
            Reset(true);
            ResetFeatureFilters();
            //Initialize total number of predictors
            InitTotalNumOfPredictors(inputPatterns[0].VariablesDataCollection[0].Length);
            //Add internal inputs
            List<InputPattern> patterns = AddInternalInputs(inputPatterns);
            //Initialize feature filters
            UpdateFeatureFilters(patterns);
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(patterns.Count);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < patterns.Count; dataSetIdx++)
            {
                //Push input data into the network
                double[] predictors = PushInputPattern(patterns[dataSetIdx], true);
                outputBundle.InputVectorCollection.Add(predictors);
                //Add desired outputs
                outputBundle.OutputVectorCollection.Add(outputs[dataSetIdx]);
                //Raise informative event
                PreprocessingProgressChanged(patterns.Count, dataSetIdx + 1, null);
            }

            //Predictor switches
            InitPredictorsGeneralSwitches(outputBundle.InputVectorCollection);

            //Preprocessing overview
            preprocessingOverview = new PreprocessingOverview(CollectStatatistics(),
                                                              NumOfNeurons,
                                                              NumOfSuppressedPredictors
                                                              );
            //Final informative event
            PreprocessingProgressChanged(patterns.Count, patterns.Count, preprocessingOverview);
            //Return
            return outputBundle;
        }

        /// <summary>
        /// Prepares input for Readout Layer training.
        /// All input vectors are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// Function also rejects unusable predictors having no reasonable fluctuation of values.
        /// Raises PreprocessingProgressChanged event.
        /// </summary>
        /// <param name="inputBundle">The bundle containing inputs and desired outputs</param>
        /// <param name="preprocessingOverview">Reservoir(s) statistics and other important information as a result of the preprocessing phase.</param>
        public VectorBundle InitializeAndPreprocessBundle(VectorBundle inputBundle, out PreprocessingOverview preprocessingOverview)
        {
            //Process data
            if (_settings.InputCfg.FeedingCfg.FeedingType == InputFeedingType.Continuous)
            {
                //Simply use inputBundle
                return PreprocessVectorBundle(inputBundle, out preprocessingOverview);
            }
            else
            {
                //Convert input vectors to InputPatterns
                List<InputPattern> patterns = new List<InputPattern>(inputBundle.InputVectorCollection.Count);
                foreach(double[] vector in inputBundle.InputVectorCollection)
                {
                    FeedingPatternedSettings feedingCfg = (FeedingPatternedSettings)_settings.InputCfg.FeedingCfg;
                    InputPattern inputPattern = new InputPattern(vector,
                                                                 _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count,
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
                return PreprocessPatternBundle(patterns, inputBundle.OutputVectorCollection, out preprocessingOverview);
            }
        }

        /// <summary>
        /// Collects the key statistics of each reservoir instance.
        /// It is very important to follow these statistics and adjust parameters of the reservoirs so that the neurons
        /// exhibit proper dynamics.
        /// </summary>
        /// <returns>Collection of key statistics for each reservoir instance</returns>
        public List<ReservoirStat> CollectStatatistics()
        {
            List<ReservoirStat> stats = new List<ReservoirStat>();
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                stats.Add(reservoir.CollectStatistics());
            }
            return stats;
        }

        //Inner classes
        /// <summary>
        /// Reservoir(s) statistics and other important information as a result of the preprocessing phase
        /// </summary>
        [Serializable]
        public class PreprocessingOverview
        {
            //Attribute properties
            /// <summary>
            /// Collection of statistics of NeuralPreprocessor's internal reservoirs
            /// </summary>
            public List<ReservoirStat> ReservoirStatCollection { get; }
            /// <summary>
            /// Total number of NeuralPreprocessor's neurons
            /// </summary>
            public int TotalNumOfNeurons { get; }
            /// <summary>
            /// Number of suppressed predictors
            /// </summary>
            public int NumOfSuppressedPredictors { get; }

            //Constructor
            /// <summary>
            /// Creates an initialized instance
            /// </summary>
            /// <param name="reservoirStatCollection">Collection of statistics of NeuralPreprocessor's internal reservoirs</param>
            /// <param name="totalNumOfNeurons">Total number of NeuralPreprocessor's neurons</param>
            /// <param name="totalNumOfInternalSynapses">Total number of NeuralPreprocessor's internal synapses</param>
            /// <param name="numOfSuppressedPredictors">Number of NeuralPreprocessor's suppressed predictors</param>
            public PreprocessingOverview(List<ReservoirStat> reservoirStatCollection,
                                         int totalNumOfNeurons,
                                         int numOfSuppressedPredictors
                                         )
            {
                ReservoirStatCollection = reservoirStatCollection;
                TotalNumOfNeurons = totalNumOfNeurons;
                NumOfSuppressedPredictors = numOfSuppressedPredictors;
                return;
            }

            //Methods
            private string FNum(double num)
            {
                return num.ToString("N8", CultureInfo.InvariantCulture).PadLeft(12);
            }

            private string StatLine(BasicStat stat)
            {
                return $"Avg:{FNum(stat.ArithAvg)},  Max:{FNum(stat.Max)},  Min:{FNum(stat.Min)},  StdDev:{FNum(stat.StdDev)}";
            }

            private void AppendStandardStatSet(int margin, StringBuilder sb, ReservoirStat.StandardStatSet sss)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                sb.Append(leftMargin + $" Avg> {StatLine(sss.AvgStat)}" + Environment.NewLine);
                sb.Append(leftMargin + $" Max> {StatLine(sss.MaxStat)}" + Environment.NewLine);
                sb.Append(leftMargin + $" Min> {StatLine(sss.MinStat)}" + Environment.NewLine);
                sb.Append(leftMargin + $"Span> {StatLine(sss.SpanStat)}" + Environment.NewLine);
                return;
            }

            private void AppendSynapsesStat(int margin, StringBuilder sb, ReservoirStat.SynapsesByRoleStat srs)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                sb.Append(leftMargin + $"Synapses" + Environment.NewLine);
                foreach (ReservoirStat.SynapseStat synapseStat in srs.SynapseRole)
                {
                    sb.Append(leftMargin + $"    {synapseStat.Role.ToString()}: {((double)synapseStat.Count / (double)srs.Count).ToString(CultureInfo.InvariantCulture)} ({synapseStat.Count.ToString()})" + Environment.NewLine);
                    if (synapseStat.Count > 0)
                    {
                        sb.Append(leftMargin + $"       Distance: {StatLine(synapseStat.Distance)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"          Delay: {StatLine(synapseStat.Delay)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"         Weight: {StatLine(synapseStat.Weight)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"        Efficacy statistics" + Environment.NewLine);
                        AppendStandardStatSet(margin + 12, sb, synapseStat.Efficacy);
                    }
                }
                return;
            }

            private void AppendNeuronAnomalies(int margin, StringBuilder sb, ReservoirStat.NeuronsAnomaliesStat nas)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                sb.Append(leftMargin + $"Neurons anomalies" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NoResSynapses} neurons have no internal synapses from other reservoir neurons" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NoResStimuli} neurons receive no stimulation from the reservoir" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NoAnalogOutput} neurons generate zero analog signal" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.ConstAnalogOutput} neurons generate constant nonzero analog signal" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.NotFiring} neurons don't spike" + Environment.NewLine);
                sb.Append(leftMargin + $"    {nas.ConstFiring} neurons constantly fire" + Environment.NewLine);
                return;
            }

            /// <summary>
            /// Builds report of key statistics collected from all the NeuralPreprocessor's reservoirs
            /// </summary>
            /// <param name="margin">Specifies how many spaces should be at the begining of each row.</param>
            /// <returns>Built text report</returns>
            public string CreateReport(int margin = 0)
            {
                string leftMargin = margin == 0 ? string.Empty : new string(' ', margin);
                string resWording = ReservoirStatCollection.Count == 1 ? "reservoir" : "reservoirs";
                StringBuilder sb = new StringBuilder();
                sb.Append(leftMargin + $"Neural preprocessor ({ReservoirStatCollection.Count} {resWording}, {TotalNumOfNeurons} neurons)" + Environment.NewLine);
                foreach (ReservoirStat resStat in ReservoirStatCollection)
                {
                    sb.Append(leftMargin + $"    Reservoir: {resStat.InstanceName} (configuration {resStat.StructCfgName}, {resStat.TotalNumOfNeurons} neurons)" + Environment.NewLine);
                    AppendNeuronAnomalies(margin + 8, sb, resStat.NeuronsAnomalies);
                    AppendSynapsesStat(margin + 8, sb, resStat.Synapses);
                    foreach (ReservoirStat.PoolStat poolStat in resStat.Pools)
                    {
                        sb.Append(leftMargin + $"        Pool: {poolStat.PoolName} ({poolStat.NumOfNeurons} neurons)" + Environment.NewLine);
                        AppendNeuronAnomalies(margin + 12, sb, poolStat.NeuronsAnomalies);
                        AppendSynapsesStat(margin + 12, sb, poolStat.Synapses);
                        foreach (ReservoirStat.PoolStat.NeuronGroupStat groupStat in poolStat.NeuronGroups)
                        {
                            sb.Append(leftMargin + $"            Group: {groupStat.GroupName} ({groupStat.NumOfNeurons} neurons)" + Environment.NewLine);
                            AppendNeuronAnomalies(margin + 16, sb, groupStat.NeuronsAnomalies);
                            AppendSynapsesStat(margin + 16, sb, groupStat.Synapses);
                            sb.Append(leftMargin + $"                Stimulation from input neurons" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Stimuli.Input);
                            sb.Append(leftMargin + $"                Stimulation from reservoir neurons" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Stimuli.Reservoir);
                            sb.Append(leftMargin + $"                Total stimulation (including Bias)" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Stimuli.Total);
                            sb.Append(leftMargin + $"                Activation" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Activation);
                            sb.Append(leftMargin + $"                Analog output" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Signal.Analog);
                            sb.Append(leftMargin + $"                Firing output" + Environment.NewLine);
                            AppendStandardStatSet(margin + 20, sb, groupStat.Signal.Firing);
                        }
                    }
                }
                sb.Append(Environment.NewLine);
                sb.Append(leftMargin + $"Number of suppressed (unused) predictors: {NumOfSuppressedPredictors}" + Environment.NewLine);
                return sb.ToString();
            }

        }//PreprocessingOverview

    }//NeuralPreprocessor

}//Namespace
