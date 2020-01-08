using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.SM.Neuron;
using RCNet.RandomValue;
using System.Globalization;
using System.Text;
using RCNet.MathTools.Hurst;

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
        public List<Reservoir> ReservoirCollection { get; }
        /// <summary>
        /// All predicting neurons.
        /// </summary>
        public List<HiddenNeuron> PredictorNeuronCollection { get; }
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
        /// Number of predictors exhibits useable values (produces meaningfully different values)
        /// </summary>
        public int NumOfValidPredictors { get; private set; }
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
            _settings = settings.DeepClone();
            _featureFilterCollection = null;
            //Internal input generators
            _internalInputGeneratorCollection = new List<IGenerator>();
            foreach(NeuralPreprocessorSettings.InputSettings.InternalField field in _settings.InputConfig.InternalFieldCollection)
            {
                if(field.GeneratorSettings.GetType() == typeof(PulseGeneratorSettings))
                {
                    _internalInputGeneratorCollection.Add(new PulseGenerator((PulseGeneratorSettings)field.GeneratorSettings));
                }
                else if (field.GeneratorSettings.GetType() == typeof(RandomValueSettings))
                {
                    _internalInputGeneratorCollection.Add(new RandomGenerator((RandomValueSettings)field.GeneratorSettings));
                }
                else if (field.GeneratorSettings.GetType() == typeof(SinusoidalGeneratorSettings))
                {
                    _internalInputGeneratorCollection.Add(new SinusoidalGenerator((SinusoidalGeneratorSettings)field.GeneratorSettings));
                }
                else if (field.GeneratorSettings.GetType() == typeof(MackeyGlassGeneratorSettings))
                {
                    _internalInputGeneratorCollection.Add(new MackeyGlassGenerator((MackeyGlassGeneratorSettings)field.GeneratorSettings));
                }
                else
                {
                    throw new Exception($"Unsupported internal signal generator for field {field.Name}");
                }
            }
            //Reservoir instance(s)
            //Random generator used for reservoir structure initialization
            Random rand = (randomizerSeek < 0 ? new Random() : new Random(randomizerSeek));
            PredictorNeuronCollection = new List<HiddenNeuron>();
            NumOfNeurons = 0;
            NumOfInternalSynapses = 0;
            InputPatternLengthConstraint = -1;
            TotalNumOfPredictors = -1;
            NumOfValidPredictors = 0;
            PredictorGeneralSwitchCollection = null;
            ReservoirCollection = new List<Reservoir>(_settings.ReservoirInstanceDefinitionCollection.Count);
            foreach(NeuralPreprocessorSettings.ReservoirInstanceDefinition instanceDefinition in _settings.ReservoirInstanceDefinitionCollection)
            {
                Reservoir reservoir = new Reservoir(instanceDefinition, _dataRange, rand);
                ReservoirCollection.Add(reservoir);
                PredictorNeuronCollection.AddRange(reservoir.PredictingNeuronCollection);
                NumOfNeurons += reservoir.Size;
                NumOfInternalSynapses += reservoir.NumOfInternalSynapses;
            }
            return;
        }

        //Properties
        /// <summary>
        /// Number of invalid predictors (exhibits no meaningfully different values)
        /// </summary>
        public int NumOfUnusedPredictors { get { return TotalNumOfPredictors - NumOfValidPredictors; } }

        //Methods
        private void InitTotalNumOfPredictors(int inputPatternLength = -1)
        {
            TotalNumOfPredictors = 0;
            //Input fields as the predictors
            int inpFieldInstancesCoeff = _settings.InputConfig.FeedingType == InputFeedingType.Patterned ? inputPatternLength : 1;
            if (_settings.InputConfig.RouteInputToReadout)
            {
                InputPatternLengthConstraint = inputPatternLength;
                foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.ExternalFieldCollection)
                {
                    if (field.AllowRoutingToReadout) TotalNumOfPredictors += inpFieldInstancesCoeff;
                }
                foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.InternalFieldCollection)
                {
                    if (field.AllowRoutingToReadout) TotalNumOfPredictors += inpFieldInstancesCoeff;
                }
            }
            //All reservoirs' predictors
            foreach (Reservoir reservoir in ReservoirCollection)
            {
                TotalNumOfPredictors += (_settings.InputConfig.Bidirectional ? 2 : 1) * reservoir.NumOfPredictors;
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
            NumOfValidPredictors = 0;
            for (int i = 0; i < TotalNumOfPredictors; i++)
            {
                if(sortedPredictorStatCollection[i].Item3.Span > MinPredictorValueDifference && i < firstInvalidOrderIndex)
                {
                    //Enable predictor
                    PredictorGeneralSwitchCollection[sortedPredictorStatCollection[i].Item1] = true;
                    ++NumOfValidPredictors;
                }
            }
            return;
        }

        private void InitPredictorsGeneralSwitches_org(List<double[]> predictorsCollection)
        {
            PredictorGeneralSwitchCollection = new bool[TotalNumOfPredictors];
            PredictorGeneralSwitchCollection.Populate(false);
            //Test predictors
            NumOfValidPredictors = 0;
            for (int row = 1; row < predictorsCollection.Count; row++)
            {
                for (int i = 0; i < PredictorGeneralSwitchCollection.Length; i++)
                {
                    if (Math.Abs(predictorsCollection[row][i] - predictorsCollection[row - 1][i]) > MinPredictorValueDifference)
                    {
                        //Update number of valid predictors
                        NumOfValidPredictors += PredictorGeneralSwitchCollection[i] ? 0 : 1;
                        //Predictor exhibits different values => enable it
                        PredictorGeneralSwitchCollection[i] = true;
                    }
                }
                if (NumOfValidPredictors == PredictorGeneralSwitchCollection.Length) break;
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
            foreach(Reservoir reservoir in ReservoirCollection)
            {
                reservoir.Reset(resetStatistics);
            }
            return;
        }

        /// <summary>
        /// Adds inputs from internal generators to be used in reservoirs.
        /// </summary>
        /// <param name="externalInputVector">External input values</param>
        /// <returns></returns>
        private double[] AddInputsFromInternalGenerators(double[] externalInputVector)
        {
            if (_settings.InputConfig.InternalFieldCollection.Count > 0)
            {
                //There are defined internal fields
                double[] inputVector = new double[_settings.InputConfig.NumOfFields];
                externalInputVector.CopyTo(inputVector, 0);
                for (int i = 0; i < _internalInputGeneratorCollection.Count; i++)
                {
                    inputVector[_settings.InputConfig.ExternalFieldCollection.Count + i] = _internalInputGeneratorCollection[i].Next();
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
        /// Initiates collection of preprocessor's feature filters
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        private void InitializeFeatureFilters(List<double[]> inputVectorCollection)
        {
            //Instantiate filters
            _featureFilterCollection = new BaseFeatureFilter[_settings.InputConfig.ExternalFieldCollection.Count];
            Parallel.For(0, _featureFilterCollection.Length, i =>
            {
                _featureFilterCollection[i] = FeatureFilterFactory.Create(_dataRange, _settings.InputConfig.ExternalFieldCollection[i].FeatureFilterCfg);
                //Update filter
                foreach (double[] vector in inputVectorCollection)
                {
                    _featureFilterCollection[i].Update(vector[i]);
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
        /// Initiates collection of preprocessor's feature filters
        /// </summary>
        /// <param name="inputPatternCollection">Collection of input patterns</param>
        private void InitializeFeatureFilters(List<List<double[]>> inputPatternCollection)
        {
            //Instantiate and adjust feature filters
            _featureFilterCollection = new BaseFeatureFilter[_settings.InputConfig.ExternalFieldCollection.Count];
            Parallel.For(0, _settings.InputConfig.ExternalFieldCollection.Count, i =>
            {
                _featureFilterCollection[i] = FeatureFilterFactory.Create(_dataRange, _settings.InputConfig.ExternalFieldCollection[i].FeatureFilterCfg);
                foreach (List<double[]> pattern in inputPatternCollection)
                {
                    foreach (double[] vector in pattern)
                    {
                        _featureFilterCollection[i].Update(vector[i]);
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Applies feature filters on pattern
        /// </summary>
        /// <param name="pattern">Input pattern</param>
        /// <returns>Filterred input pattern</returns>
        private List<double[]> ApplyFiltersOnInputPattern(List<double[]> pattern)
        {
            List<double[]> filterPattern = new List<double[]>(pattern.Count);
            foreach (double[] vector in pattern)
            {
                double[] filterVector = new double[vector.Length];
                int numOfSets = vector.Length / _featureFilterCollection.Length;
                for (int set = 0; set < numOfSets; set++)
                {
                    for (int i = 0; i < _featureFilterCollection.Length; i++)
                    {
                        int idx = set * _featureFilterCollection.Length + i;
                        filterVector[idx] = _featureFilterCollection[i].ApplyFilter(vector[idx]);
                    }
                }
                filterPattern.Add(filterVector);
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
        private double[] PushInput(double[] externalInputVector, bool collectStatistics)
        {
            double[] completedInputVector = AddInputsFromInternalGenerators(ApplyFiltersOnInputVector(externalInputVector));
            double[] predictors = new double[TotalNumOfPredictors];
            int predictorsIdx = 0;
            //Compute reservoir(s)
            foreach (Reservoir reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceDefinition.InputFieldInfoCollection.Count];
                for (int i = 0; i < reservoir.InstanceDefinition.InputFieldInfoCollection.Count; i++)
                {
                    reservoirInput[i] = completedInputVector[reservoir.InstanceDefinition.InputFieldInfoCollection[i].FieldIndex];
                }
                //Compute reservoir
                reservoir.Compute(reservoirInput, collectStatistics);
                reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                predictorsIdx += reservoir.NumOfPredictors;
            }
            if (_settings.InputConfig.RouteInputToReadout)
            {
                int fieldIdx = 0;
                foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.ExternalFieldCollection)
                {
                    if (field.AllowRoutingToReadout)
                    {
                        //Route original values
                        predictors[predictorsIdx++] = externalInputVector[fieldIdx];
                    }
                    ++fieldIdx;
                }
                foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.InternalFieldCollection)
                {
                    if (field.AllowRoutingToReadout)
                    {
                        predictors[predictorsIdx++] = completedInputVector[fieldIdx];
                    }
                    ++fieldIdx;
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
        private double[] PushInput(List<double[]> externalInputPattern, bool collectStatistics)
        {
            double[] predictors = new double[TotalNumOfPredictors];
            int predictorsIdx = 0;
            //Check pattern legth costraint
            if(InputPatternLengthConstraint != -1)
            {
                if(externalInputPattern.Count != InputPatternLengthConstraint)
                {
                    throw new Exception($"Incorrect length of input pattern ({externalInputPattern.Count}). Length must be equal to {InputPatternLengthConstraint}.");
                }
            }
            //Reset SM but keep statistics
            Reset(false);
            //Apply filters
            List<double[]> normalizedInputPattern = ApplyFiltersOnInputPattern(externalInputPattern);
            //Add internal input
            List<double[]> completedInputPattern = new List<double[]>(normalizedInputPattern.Count);
            foreach (double[] externalInputPatternVector in normalizedInputPattern)
            {
                completedInputPattern.Add(AddInputsFromInternalGenerators(externalInputPatternVector));
            }
            //Compute reservoir(s)
            foreach (Reservoir reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceDefinition.InputFieldInfoCollection.Count];
                List<List<double[]>> stepInputPatterns = new List<List<double[]>>() { completedInputPattern };
                if(_settings.InputConfig.Bidirectional)
                {
                    List<double[]> reversedInputPattern = new List<double[]>(completedInputPattern);
                    reversedInputPattern.Reverse();
                    stepInputPatterns.Add(reversedInputPattern);
                }
                foreach (List<double[]> stepInputPattern in stepInputPatterns)
                {
                    foreach (double[] inputVector in stepInputPattern)
                    {
                        for (int i = 0; i < reservoir.InstanceDefinition.InputFieldInfoCollection.Count; i++)
                        {
                            reservoirInput[i] = inputVector[reservoir.InstanceDefinition.InputFieldInfoCollection[i].FieldIndex];
                        }
                        //Compute the reservoir
                        reservoir.Compute(reservoirInput, collectStatistics);
                    }
                    reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                    predictorsIdx += reservoir.NumOfPredictors;
                }
            }

            //Input fields as the predictors
            if (_settings.InputConfig.RouteInputToReadout)
            {
                for(int vectorIdx = 0; vectorIdx < externalInputPattern.Count; vectorIdx++)
                {
                    double[] externalInputVector = externalInputPattern[vectorIdx];
                    double[] completedInputVector = completedInputPattern[vectorIdx];
                    int fieldIdx = 0;
                    foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.ExternalFieldCollection)
                    {
                        if (field.AllowRoutingToReadout)
                        {
                            //Route original values
                            predictors[predictorsIdx++] = externalInputVector[fieldIdx];
                        }
                        ++fieldIdx;
                    }
                    foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.InternalFieldCollection)
                    {
                        if (field.AllowRoutingToReadout)
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
            //Check calling consistency
            if (_settings.InputConfig.FeedingType == InputFeedingType.Patterned)
            {
                throw new Exception("Called incorrect version of Preprocess function for patterned input feeding.");
            }
            //Push input vector into the preprocessor and return result
            return PushInput(input, false);
        }

        /// <summary>
        /// Pushes input pattern into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="inputPattern">Patterned input values in natural form</param>
        public double[] Preprocess(List<double[]> inputPattern)
        {
            //Check readyness
            if (_featureFilterCollection == null)
            {
                throw new Exception("Preprocessor was not initialized by the sample data bundle (feature filters are not instantiated yet).");
            }
            //Check calling consistency
            if (_settings.InputConfig.FeedingType == InputFeedingType.Continuous)
            {
                throw new Exception("Called incorrect version of Preprocess function for continuous input feeding.");
            }
            //Push input pattern into the preprocessor and return result
            return PushInput(inputPattern, false);
        }

        /// <summary>
        /// Prepares input for Readout Layer training.
        /// All input vectors are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// Function also rejects unusable predictors having no reasonable fluctuation of values.
        /// Raises PreprocessingProgressChanged event.
        /// </summary>
        /// <param name="vectorBundle">The bundle containing known sample input and desired output vectors (in time order)</param>
        /// <param name="preprocessingOverview">Reservoir(s) statistics and other important information as a result of the preprocessing phase.</param>
        public VectorBundle InitializeAndPreprocessBundle(VectorBundle vectorBundle, out PreprocessingOverview preprocessingOverview)
        {
            //Check correctness
            if (_settings.InputConfig.FeedingType == InputFeedingType.Patterned)
            {
                throw new Exception("Called incorrect version of InitializeAndPreprocessBundle function for patterned input feeding.");
            }
            //Reset the internal states and also statistics
            Reset(true);
            //Initialize total number of predictors
            InitTotalNumOfPredictors();
            //Initialize feature filters
            InitializeFeatureFilters(vectorBundle.InputVectorCollection);
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(vectorBundle.InputVectorCollection.Count - _settings.InputConfig.BootCycles);
            //Collect predictors
            for (int dataSetIdx = 0; dataSetIdx < vectorBundle.InputVectorCollection.Count; dataSetIdx++)
            {
                bool afterBoot = (dataSetIdx >= _settings.InputConfig.BootCycles);
                //Push input data into the network
                double[] predictors = PushInput(vectorBundle.InputVectorCollection[dataSetIdx], afterBoot);
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
                                                              NumOfUnusedPredictors
                                                              );
            //Final informative event
            PreprocessingProgressChanged(vectorBundle.InputVectorCollection.Count, vectorBundle.InputVectorCollection.Count, preprocessingOverview);
            //Return
            return outputBundle;
        }

        /// <summary>
        /// Prepares input for Readout Layer training.
        /// All input patterns are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// Function also rejects unusable predictors having no reasonable fluctuation of values.
        /// Raises PreprocessingProgressChanged event.
        /// </summary>
        /// <param name="patternBundle">The bundle containing known sample input patterns and desired output vectors</param>
        /// <param name="preprocessingOverview">Reservoir(s) statistics and other important information as a result of the preprocessing phase.</param>
        public VectorBundle InitializeAndPreprocessBundle(PatternBundle patternBundle, out PreprocessingOverview preprocessingOverview)
        {
            //Check correctness
            if (_settings.InputConfig.FeedingType == InputFeedingType.Continuous)
            {
                throw new Exception("Called incorrect version of InitializeAndPreprocessBundle function for continuous input feeding.");
            }
            //Reset the internal states and also statistics
            Reset(true);
            //Initialize total number of predictors
            InitTotalNumOfPredictors(patternBundle.InputPatternCollection[0].Count);
            //Initialize feature filters
            InitializeFeatureFilters(patternBundle.InputPatternCollection);
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(patternBundle.InputPatternCollection.Count);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < patternBundle.InputPatternCollection.Count; dataSetIdx++)
            {
                //Push input data into the network
                double[] predictors = PushInput(patternBundle.InputPatternCollection[dataSetIdx], true);
                outputBundle.InputVectorCollection.Add(predictors);
                //Add desired outputs
                outputBundle.OutputVectorCollection.Add(patternBundle.OutputVectorCollection[dataSetIdx]);
                //Raise informative event
                PreprocessingProgressChanged(patternBundle.InputPatternCollection.Count, dataSetIdx + 1, null);
            }

            //Predictor switches
            InitPredictorsGeneralSwitches(outputBundle.InputVectorCollection);

            //Preprocessing overview
            preprocessingOverview = new PreprocessingOverview(CollectStatatistics(),
                                                              NumOfNeurons,
                                                              NumOfInternalSynapses,
                                                              NumOfUnusedPredictors
                                                              );
            //Final informative event
            PreprocessingProgressChanged(patternBundle.InputPatternCollection.Count, patternBundle.InputPatternCollection.Count, preprocessingOverview);
            //Return
            return outputBundle;
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
            foreach (Reservoir reservoir in ReservoirCollection)
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
            /// Number of invalid predictors (exhibits no meaningfully different values)
            /// </summary>
            public int NumOfUnusedPredictors { get; }
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
            /// <param name="numOfUnusedPredictors">Number of NeuralPreprocessor's invalid predictors</param>
            public PreprocessingOverview(List<ReservoirStat> reservoirStatCollection,
                                         int totalNumOfNeurons,
                                         int totalNumOfInternalSynapses,
                                         int numOfUnusedPredictors
                                         )
            {
                ReservoirStatCollection = reservoirStatCollection;
                TotalNumOfNeurons = totalNumOfNeurons;
                TotalNumOfInternalSynapses = totalNumOfInternalSynapses;
                NumOfUnusedPredictors = numOfUnusedPredictors;
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
                            sb.Append(leftMargin + $"                Efficacy of synapses" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    AVG>  {StatLine(groupStat.AvgSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MAX>  {StatLine(groupStat.MaxSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                    MIN>  {StatLine(groupStat.MinSynEfficacyStat)}" + Environment.NewLine);
                            sb.Append(leftMargin + $"                   SPAN>  {StatLine(groupStat.SynEfficacySpansStat)}" + Environment.NewLine);
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
                sb.Append(leftMargin + $"Number of unused (invalid) predictors: {NumOfUnusedPredictors}" + Environment.NewLine);
                return sb.ToString();
            }

        }//PreprocessingOverview

    }//NeuralPreprocessor

}//Namespace
