using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.Neural.Data.Filter;
using RCNet.Neural.Network.SM.Neuron;
using RCNet.RandomValue;


namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Implements the neural data preprocessor, one of the main components of the State Machine.
    /// </summary>
    [Serializable]
    public class NeuralPreprocessor
    {
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
        /// Delegate of informative callback function to inform caller about predictors collection progress.
        /// </summary>
        /// <param name="totalNumOfInputs">Total number of inputs to be processed</param>
        /// <param name="numOfProcessedInputs">Number of processed inputs</param>
        /// <param name="userObject">An user object</param>
        public delegate void PredictorsCollectionCallbackDelegate(int totalNumOfInputs,
                                                                  int numOfProcessedInputs,
                                                                  Object userObject
                                                                  );
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
        /// Number of predictors
        /// </summary>
        public int NumOfPredictors { get; }
        /// <summary>
        /// Number of internal synapses
        /// </summary>
        public int NumOfInternalSynapses { get; }

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
            NumOfPredictors = 0;
            NumOfInternalSynapses = 0;
            ReservoirCollection = new List<Reservoir>(_settings.ReservoirInstanceDefinitionCollection.Count);
            foreach(NeuralPreprocessorSettings.ReservoirInstanceDefinition instanceDefinition in _settings.ReservoirInstanceDefinitionCollection)
            {
                Reservoir reservoir = new Reservoir(instanceDefinition, _dataRange, rand);
                ReservoirCollection.Add(reservoir);
                PredictorNeuronCollection.AddRange(reservoir.PredictingNeuronCollection);
                NumOfNeurons += reservoir.Size;
                NumOfPredictors += reservoir.NumOfPredictors;
                NumOfInternalSynapses += reservoir.NumOfInternalSynapses;
            }
            if(_settings.InputConfig.RouteInputToReadout)
            {
                foreach(NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.ExternalFieldCollection)
                {
                    if (field.AllowRoutingToReadout) ++NumOfPredictors;
                }
                foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.InternalFieldCollection)
                {
                    if (field.AllowRoutingToReadout) ++NumOfPredictors;
                }
            }
            return;
        }

        //Properties

        //Methods
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
            double[] predictors = new double[NumOfPredictors];
            int predictorsIdx = 0;
            //Compute reservoir(s)
            foreach (Reservoir reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceDefinition.NPInputFieldIdxCollection.Count];
                for (int i = 0; i < reservoir.InstanceDefinition.NPInputFieldIdxCollection.Count; i++)
                {
                    reservoirInput[i] = completedInputVector[reservoir.InstanceDefinition.NPInputFieldIdxCollection[i]];
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
            double[] predictors = new double[NumOfPredictors];
            int predictorsIdx = 0;
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
                double[] reservoirInput = new double[reservoir.InstanceDefinition.NPInputFieldIdxCollection.Count];
                foreach (double[] inputVector in completedInputPattern)
                {
                    for (int i = 0; i < reservoir.InstanceDefinition.NPInputFieldIdxCollection.Count; i++)
                    {
                        reservoirInput[i] = inputVector[reservoir.InstanceDefinition.NPInputFieldIdxCollection[i]];
                    }
                    //Compute the reservoir
                    reservoir.Compute(reservoirInput, collectStatistics);
                }
                reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                predictorsIdx += reservoir.NumOfPredictors;
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
        /// </summary>
        /// <param name="vectorBundle">
        /// The bundle containing known sample input and desired output vectors (in time order)
        /// </param>
        /// <param name="informativeCallback">
        /// Function to be called after each processed input.
        /// </param>
        /// <param name="userObject">
        /// The user object to be passed to informativeCallback.
        /// </param>
        public VectorBundle InitializeAndPreprocessBundle(VectorBundle vectorBundle,
                                                          PredictorsCollectionCallbackDelegate informativeCallback = null,
                                                          Object userObject = null
                                                          )
        {
            //Check correctness
            if (_settings.InputConfig.FeedingType == InputFeedingType.Patterned)
            {
                throw new Exception("Called incorrect version of InitializeAndPreprocessBundle function for patterned input feeding.");
            }
            //Reset the internal states and also statistics
            Reset(true);
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
                //An informative callback
                informativeCallback?.Invoke(vectorBundle.InputVectorCollection.Count, dataSetIdx + 1, userObject);
            }
            return outputBundle;
        }

        /// <summary>
        /// Prepares input for Readout Layer training.
        /// All input patterns are processed by internal reservoirs and the corresponding network predictors are recorded.
        /// </summary>
        /// <param name="patternBundle">
        /// The bundle containing known sample input patterns and desired output vectors
        /// </param>
        /// <param name="informativeCallback">
        /// Function to be called after each processed input.
        /// </param>
        /// <param name="userObject">
        /// The user object to be passed to informativeCallback.
        /// </param>
        public VectorBundle InitializeAndPreprocessBundle(PatternBundle patternBundle,
                                                          PredictorsCollectionCallbackDelegate informativeCallback = null,
                                                          Object userObject = null
                                                          )
        {
            //Check correctness
            if (_settings.InputConfig.FeedingType == InputFeedingType.Continuous)
            {
                throw new Exception("Called incorrect version of InitializeAndPreprocessBundle function for continuous input feeding.");
            }
            //Reset the internal states and also statistics
            Reset(true);
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
                //Informative callback
                informativeCallback?.Invoke(patternBundle.InputPatternCollection.Count, dataSetIdx + 1, userObject);
            }
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

    }//NeuralPreprocessor

}//Namespace
