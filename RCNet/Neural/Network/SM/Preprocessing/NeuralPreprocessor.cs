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
        /// Input data will be transformed by feature filters to this range before the usage in the reservoirs
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

        //Attributes
        /// <summary>
        /// Settings used for instance creation.
        /// </summary>
        private readonly NeuralPreprocessorSettings _settings;

        /// <summary>
        /// Collection of the internal input generators associated with the internal input fields
        /// </summary>
        private readonly List<IGenerator> _internalInputGeneratorCollection;

        /// <summary>
        /// Collection of input feature filters
        /// </summary>
        private BaseFeatureFilter[] _featureFilterCollection;

        //Attribute properties
        /// <summary>
        /// Collection of reservoir instances.
        /// </summary>
        public List<ReservoirInstance> ReservoirCollection { get; }
        
        /// <summary>
        /// All predicting neurons.
        /// </summary>
        public List<HiddenNeuron> PredictorNeuronCollection { get; }
        
        /// <summary>
        /// Number of boot cycles
        /// </summary>
        public int BootCycles { get; }
        
        /// <summary>
        /// Number of neurons
        /// </summary>
        public int NumOfNeurons { get; }
        
        /// <summary>
        /// Number of internal synapses
        /// </summary>
        public int NumOfInternalSynapses { get; }
        
        /// <summary>
        /// -1 (no constraint) for continuous feeding or patterned feeding without routing input to readout.
        /// Required constant length of the input pattern  for patterned feeding without routing input to readout.
        /// </summary>
        public int InputPatternLengthConstraint { get; private set; }
        
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

        //Constructor
        /// <summary>
        /// Constructs an instance of Neural Preprocessor
        /// </summary>
        /// <param name="settings">Neural Preprocessor settings</param>
        /// A value greater than or equal to 0 will always ensure the same initialization of the internal
        /// random number generator and therefore the same network structure, which is good for tuning
        /// network parameters. A value less than 0 causes a fully random initialization when creating a network instance.
        /// <param name="randomizerSeek">
        /// </param>
        public NeuralPreprocessor(NeuralPreprocessorSettings settings, int randomizerSeek)
        {
            _settings = (NeuralPreprocessorSettings)settings.DeepClone();
            _featureFilterCollection = null;
            //Internal input generators
            _internalInputGeneratorCollection = new List<IGenerator>();
            if (_settings.InputCfg.FieldsCfg.InternalFieldsCfg != null)
            {
                foreach (InternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.InternalFieldsCfg.FieldCfgCollection)
                {
                    if (fieldCfg.GeneratorCfg.GetType() == typeof(PulseGeneratorSettings))
                    {
                        _internalInputGeneratorCollection.Add(new PulseGenerator((PulseGeneratorSettings)fieldCfg.GeneratorCfg));
                    }
                    else if (fieldCfg.GeneratorCfg.GetType() == typeof(RandomValueSettings))
                    {
                        _internalInputGeneratorCollection.Add(new RandomGenerator((RandomValueSettings)fieldCfg.GeneratorCfg));
                    }
                    else if (fieldCfg.GeneratorCfg.GetType() == typeof(SinusoidalGeneratorSettings))
                    {
                        _internalInputGeneratorCollection.Add(new SinusoidalGenerator((SinusoidalGeneratorSettings)fieldCfg.GeneratorCfg));
                    }
                    else if (fieldCfg.GeneratorCfg.GetType() == typeof(MackeyGlassGeneratorSettings))
                    {
                        _internalInputGeneratorCollection.Add(new MackeyGlassGenerator((MackeyGlassGeneratorSettings)fieldCfg.GeneratorCfg));
                    }
                    else
                    {
                        throw new Exception($"Unsupported internal signal generator for field {fieldCfg.Name}");
                    }
                }
            }
            //Reservoir instance(s)
            BootCycles = 0;
            //Random generator used for reservoir structure initialization
            Random rand = (randomizerSeek < 0 ? new Random() : new Random(randomizerSeek));
            PredictorNeuronCollection = new List<HiddenNeuron>();
            NumOfNeurons = 0;
            NumOfInternalSynapses = 0;
            InputPatternLengthConstraint = -1;
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
                PredictorNeuronCollection.AddRange(reservoir.PredictingNeuronCollection);
                NumOfNeurons += reservoir.Size;
                NumOfInternalSynapses += reservoir.NumOfInternalSynapses;
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
        private void InitTotalNumOfPredictors(int inputPatternLength = -1)
        {
            TotalNumOfPredictors = 0;
            //Input fields as the predictors
            int inpFieldInstancesCoeff = _settings.InputCfg.FeedingCfg.FeedingType == InputFeedingType.Patterned ? inputPatternLength : 1;
            if (_settings.InputCfg.FeedingCfg.RouteToReadout)
            {
                InputPatternLengthConstraint = inputPatternLength;
                foreach (ExternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection)
                {
                    if (fieldCfg.RouteToReadout) TotalNumOfPredictors += inpFieldInstancesCoeff;
                }
                if (_settings.InputCfg.FieldsCfg.InternalFieldsCfg != null)
                {
                    foreach (InternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.InternalFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout) TotalNumOfPredictors += inpFieldInstancesCoeff;
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
            //Reset generators
            foreach(IGenerator generator in _internalInputGeneratorCollection)
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

        /// <summary>
        /// Adds inputs from internal generators to be used in reservoirs.
        /// </summary>
        /// <param name="externalInputVector">External input values</param>
        private double[] AddInputsFromInternalGenerators(double[] externalInputVector)
        {
            if (_settings.InputCfg.FieldsCfg.InternalFieldsCfg != null)
            {
                //There are defined internal fields
                double[] inputVector = new double[_settings.InputCfg.FieldsCfg.TotalNumOfFields];
                externalInputVector.CopyTo(inputVector, 0);
                for (int i = 0; i < _internalInputGeneratorCollection.Count; i++)
                {
                    inputVector[_settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count + i] = _internalInputGeneratorCollection[i].Next();
                }
                return inputVector;
            }
            else
            {
                //Defined no internal fields
                return externalInputVector;
            }
        }

        /// <summary>
        /// Adds inputs from internal generators to be used in reservoirs.
        /// </summary>
        /// <param name="inputPattern">Input pattern containing external data</param>
        private InputPattern AddInputsFromInternalGenerators(InputPattern inputPattern)
        {
            InputPattern pattern = new InputPattern(inputPattern);
            for (int genIdx = 0; genIdx < _internalInputGeneratorCollection.Count; genIdx++)
            {
                double[] inputVector = new double[inputPattern.VariablesDataCollection[0].Length];
                for (int i = 0; i < inputVector.Length; i++)
                {
                    inputVector[i] = _internalInputGeneratorCollection[genIdx].Next();
                }
                pattern.VariablesDataCollection.Add(inputVector);
            }
            return pattern;
        }

        /// <summary>
        /// Initiates collection of preprocessor's feature filters
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        private void InitializeFeatureFilters(List<double[]> inputVectorCollection)
        {
            //Instantiate filters
            _featureFilterCollection = new BaseFeatureFilter[_settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count];
            Parallel.For(0, _featureFilterCollection.Length, i =>
            {
                _featureFilterCollection[i] = FeatureFilterFactory.Create(_dataRange, _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection[i].FeatureFilterCfg);
                //Update filter
                foreach (double[] vector in inputVectorCollection)
                {
                    _featureFilterCollection[i].Update(vector[i]);
                }
            });
            return;
        }

        /// <summary>
        /// Initiates collection of preprocessor's feature filters
        /// </summary>
        /// <param name="inputPatternCollection">Collection of input patterns</param>
        private void InitializeFeatureFilters(List<InputPattern> inputPatternCollection)
        {
            //Instantiate and adjust feature filters
            _featureFilterCollection = new BaseFeatureFilter[_settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count];
            Parallel.For(0, _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection.Count, varIdx =>
            {
                _featureFilterCollection[varIdx] = FeatureFilterFactory.Create(_dataRange, _settings.InputCfg.FieldsCfg.ExternalFieldsCfg.FieldCfgCollection[varIdx].FeatureFilterCfg);
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
        /// <returns>Filterred input vector</returns>
        private double[] ApplyFiltersOnInputVector(double[] vector)
        {
            double[] filterVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                filterVector[i] = _featureFilterCollection[i].ApplyFilter(vector[i]);
            }
            return filterVector;
        }

        /// <summary>
        /// Applies feature filters on pattern
        /// </summary>
        /// <param name="pattern">Input pattern</param>
        /// <returns>Filterred input pattern</returns>
        private InputPattern ApplyFiltersOnInputPattern(InputPattern pattern)
        {
            InputPattern filterPattern = new InputPattern(pattern.VariablesDataCollection.Count);
            int varIdx = 0;
            foreach (double[] vector in pattern.VariablesDataCollection)
            {
                double[] filterVector = new double[vector.Length];
                for(int i = 0; i < vector.Length; i++)
                {
                    filterVector[i] = _featureFilterCollection[varIdx].ApplyFilter(vector[i]);
                }
                filterPattern.VariablesDataCollection.Add(filterVector);
                ++varIdx;
            }
            return filterPattern;
        }

        /// <summary>
        /// Pushes input vector into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="externalInputVector">Input values</param>
        /// <param name="collectStatistics">
        /// The parameter indicates whether to update internal statistics
        /// </param>
        private double[] PushInputVector(double[] externalInputVector, bool collectStatistics)
        {
            double[] completedInputVector = AddInputsFromInternalGenerators(ApplyFiltersOnInputVector(externalInputVector));
            double[] predictors = new double[TotalNumOfPredictors];
            int predictorsIdx = 0;
            //Compute reservoir(s)
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InputUnitCollection.Length];
                for (int i = 0; i < reservoir.InputUnitCollection.Length; i++)
                {
                    reservoirInput[i] = completedInputVector[reservoir.InputUnitCollection[i].InputFieldIdx];
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
                        predictors[predictorsIdx++] = externalInputVector[fieldIdx];
                    }
                    ++fieldIdx;
                }
                if (_settings.InputCfg.FieldsCfg.InternalFieldsCfg != null)
                {
                    foreach (InternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.InternalFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            predictors[predictorsIdx++] = completedInputVector[fieldIdx];
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
        /// <param name="externalInputPattern">Input pattern</param>
        /// <param name="collectStatistics">
        /// The parameter indicates whether to update internal statistics
        /// </param>
        private double[] PushInputPattern(InputPattern externalInputPattern, bool collectStatistics)
        {
            double[] predictors = new double[TotalNumOfPredictors];
            int predictorsIdx = 0;
            //Check pattern length costraint
            if(InputPatternLengthConstraint != -1)
            {
                if(externalInputPattern.VariablesDataCollection[0].Length != InputPatternLengthConstraint)
                {
                    throw new Exception($"Incorrect length of input pattern ({externalInputPattern.VariablesDataCollection[0].Length}). Length must be equal to {InputPatternLengthConstraint}.");
                }
            }
            //Reset SM but keep statistics
            Reset(false);
            //Apply filters
            InputPattern normalizedInputPattern = ApplyFiltersOnInputPattern(externalInputPattern);
            //Add internal input
            InputPattern completedInputPattern = AddInputsFromInternalGenerators(normalizedInputPattern);
            //Compute reservoir(s)
            foreach (ReservoirInstance reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceCfg.InputUnitsCfg.InputUnitCfgCollection.Count];
                List<InputPattern> stepInputPatterns = new List<InputPattern>() { completedInputPattern };
                if(((FeedingPatternedSettings)_settings.InputCfg.FeedingCfg).Bidir)
                {
                    InputPattern reversedInputPattern = new InputPattern(completedInputPattern.VariablesDataCollection.Count);
                    foreach(double[] data in completedInputPattern.VariablesDataCollection)
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
                        //Route original values
                        for(int i = 0; i < externalInputPattern.VariablesDataCollection[fieldIdx].Length; i++)
                        {
                            predictors[predictorsIdx++] = externalInputPattern.VariablesDataCollection[fieldIdx][i];
                        }
                    }
                    ++fieldIdx;
                }
                if (_settings.InputCfg.FieldsCfg.InternalFieldsCfg != null)
                {
                    foreach (InternalFieldSettings fieldCfg in _settings.InputCfg.FieldsCfg.InternalFieldsCfg.FieldCfgCollection)
                    {
                        if (fieldCfg.RouteToReadout)
                        {
                            //Route internal values
                            for (int i = 0; i < completedInputPattern.VariablesDataCollection[fieldIdx].Length; i++)
                            {
                                predictors[predictorsIdx++] = completedInputPattern.VariablesDataCollection[fieldIdx][i];
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
                return PushInputVector(input, false);
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
                return PushInputPattern(inputPattern, false);
            }
        }


        /// <summary>
        /// Continuous feeding version
        /// </summary>
        private VectorBundle PreprocessVectorBundle(VectorBundle vectorBundle, out PreprocessingOverview preprocessingOverview)
        {
            //Reset the internal states and also statistics
            Reset(true);
            //Initialize total number of predictors
            InitTotalNumOfPredictors();
            //Initialize feature filters
            InitializeFeatureFilters(vectorBundle.InputVectorCollection);
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(vectorBundle.InputVectorCollection.Count - BootCycles);
            //Collect predictors
            for (int dataSetIdx = 0; dataSetIdx < vectorBundle.InputVectorCollection.Count; dataSetIdx++)
            {
                bool afterBoot = (dataSetIdx >= BootCycles);
                //Push input data into the network
                double[] predictors = PushInputVector(vectorBundle.InputVectorCollection[dataSetIdx], afterBoot);
                //Is boot sequence passed? Collect predictors?
                if (afterBoot)
                {
                    //YES
                    //Predictors
                    outputBundle.InputVectorCollection.Add(predictors);
                    //Desired outputs
                    outputBundle.OutputVectorCollection.Add(vectorBundle.OutputVectorCollection[dataSetIdx]);
                }
                //Raise informative event
                PreprocessingProgressChanged(vectorBundle.InputVectorCollection.Count, dataSetIdx + 1, null);
            }

            //Predictor switches
            InitPredictorsGeneralSwitches(outputBundle.InputVectorCollection);

            //Preprocessing overview
            preprocessingOverview = new PreprocessingOverview(CollectStatatistics(),
                                                              NumOfNeurons,
                                                              NumOfInternalSynapses,
                                                              NumOfSuppressedPredictors
                                                              );
            //Final informative event
            PreprocessingProgressChanged(vectorBundle.InputVectorCollection.Count, vectorBundle.InputVectorCollection.Count, preprocessingOverview);
            //Return
            return outputBundle;
        }

        /// <summary>
        /// Patterned feeding version
        /// </summary>
        private VectorBundle PreprocessPatternBundle(List<InputPattern> patterns, List<double[]> outputs, out PreprocessingOverview preprocessingOverview)
        {
            //Reset the internal states and also statistics
            Reset(true);
            //Initialize total number of predictors
            InitTotalNumOfPredictors(patterns[0].VariablesDataCollection[0].Length);
            //Initialize feature filters
            InitializeFeatureFilters(patterns);
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
                                                              NumOfInternalSynapses,
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
            if(_settings.InputCfg.FeedingCfg.FeedingType == InputFeedingType.Continuous)
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
            /// <summary>
            /// Total number of NeuralPreprocessor's internal synapses
            /// </summary>
            public int TotalNumOfInternalSynapses { get; }

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
                                         int totalNumOfInternalSynapses,
                                         int numOfSuppressedPredictors
                                         )
            {
                ReservoirStatCollection = reservoirStatCollection;
                TotalNumOfNeurons = totalNumOfNeurons;
                TotalNumOfInternalSynapses = totalNumOfInternalSynapses;
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
                return $"Avg:{FNum(stat.ArithAvg)},  Max:{FNum(stat.Max)},  Min:{FNum(stat.Min)},  SDdev:{FNum(stat.StdDev)}";
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
                sb.Append(leftMargin + $"Neural preprocessor ({ReservoirStatCollection.Count} {resWording}, {TotalNumOfNeurons} neurons, {TotalNumOfInternalSynapses} internal synapses)" + Environment.NewLine);
                foreach (ReservoirStat resStat in ReservoirStatCollection)
                {
                    sb.Append(leftMargin + $"    Reservoir instance: {resStat.ReservoirInstanceName} (configuration {resStat.ReservoirSettingsName}, {resStat.TotalNumOfNeurons} neurons, {Math.Round(resStat.ExcitatoryNeuronsRatio * 100, 1).ToString(CultureInfo.InvariantCulture)}% excitatory neurons, {resStat.TotalNumOfInternalSynapses} internal synapses)" + Environment.NewLine);
                    sb.Append(leftMargin + $"        Activity" + Environment.NewLine);
                    sb.Append(leftMargin + $"            {resStat.NumOfNoRStimuliNeurons} neurons receive no stimulation from the reservoir" + Environment.NewLine);
                    sb.Append(leftMargin + $"            {resStat.NumOfNoAnalogOutputSignalNeurons} neurons produce no analog signal" + Environment.NewLine);
                    sb.Append(leftMargin + $"            {resStat.NumOfConstAnalogOutputSignalNeurons} neurons produce constant analog signal" + Environment.NewLine);
                    sb.Append(leftMargin + $"            {resStat.NumOfNotFiringNeurons} neurons don't spike" + Environment.NewLine);
                    sb.Append(leftMargin + $"            {resStat.NumOfConstFiringNeurons} neurons are constantly firing" + Environment.NewLine);
                    foreach (ReservoirStat.PoolStat poolStat in resStat.PoolStatCollection)
                    {
                        sb.Append(leftMargin + $"        Pool: {poolStat.PoolName} ({poolStat.NumOfNeurons} neurons, {Math.Round(poolStat.ExcitatoryNeuronsRatio * 100, 1).ToString(CultureInfo.InvariantCulture)}% excitatory neurons, {poolStat.InternalAnalogWeightsStat.NumOfSamples + poolStat.InternalSpikingWeightsStat.NumOfSamples} internal synapses)" + Environment.NewLine);
                        sb.Append(leftMargin + $"            Activity" + Environment.NewLine);
                        sb.Append(leftMargin + $"                {poolStat.NumOfNoRStimuliNeurons} neurons receive no stimulation from the reservoir" + Environment.NewLine);
                        sb.Append(leftMargin + $"                {poolStat.NumOfNoAnalogOutputSignalNeurons} neurons produce no analog signal" + Environment.NewLine);
                        sb.Append(leftMargin + $"                {poolStat.NumOfConstAnalogOutputSignalNeurons} neurons produce constant analog signal" + Environment.NewLine);
                        sb.Append(leftMargin + $"                {poolStat.NumOfNotFiringNeurons} neurons don't spike" + Environment.NewLine);
                        sb.Append(leftMargin + $"                {poolStat.NumOfConstFiringNeurons} neurons are constantly firing" + Environment.NewLine);
                        sb.Append(leftMargin + $"            Weights of synapses" + Environment.NewLine);
                        sb.Append(leftMargin + $"                Input       >  {StatLine(poolStat.InputWeightsStat)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"                Int. Analog >  {StatLine(poolStat.InternalAnalogWeightsStat)}" + Environment.NewLine);
                        sb.Append(leftMargin + $"                Int. Spiking>  {StatLine(poolStat.InternalSpikingWeightsStat)}" + Environment.NewLine);
                        foreach (ReservoirStat.PoolStat.NeuronGroupStat groupStat in poolStat.NeuronGroupStatCollection)
                        {
                            sb.Append(leftMargin + $"            Group of neurons: {groupStat.GroupName} ({groupStat.AvgAnalogSignalStat.NumOfSamples} neurons)" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Activity" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    {groupStat.NumOfNoRStimuliNeurons} neurons receive no stimulation from the reservoir" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    {groupStat.NumOfNoAnalogOutputSignalNeurons} neurons produce no analog signal" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    {groupStat.NumOfConstAnalogOutputSignalNeurons} neurons produce constant analog signal" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    {groupStat.NumOfNotFiringNeurons} neurons don't spike" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    {groupStat.NumOfConstFiringNeurons} neurons are constantly firing" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Stimulation from input neurons" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgIStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxIStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinIStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                   SPAN>  {StatLine(groupStat.IStimuliSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Stimulation from reservoir neurons" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgRStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxRStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinRStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                   SPAN>  {StatLine(groupStat.RStimuliSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Total stimulation (including Bias)" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgTStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxTStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinTStimuliStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                   SPAN>  {StatLine(groupStat.TStimuliSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Efficacy of input synapses" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgInpSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxInpSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinInpSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                   SPAN>  {StatLine(groupStat.InpSynEfficacySpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Efficacy of internal synapses" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgIntSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxIntSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinIntSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                   SPAN>  {StatLine(groupStat.IntSynEfficacySpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Activation" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgActivationStatesStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxActivationStatesStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinActivationStatesStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                   SPAN>  {StatLine(groupStat.ActivationStateSpansStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Analog output" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgAnalogSignalStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxAnalogSignalStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinAnalogSignalStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                Spiking signal" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgFiringStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxFiringStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinFiringStat)}" + Environment.NewLine);
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
