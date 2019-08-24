using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RCNet.Extensions;
using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Generators;
using RCNet.RandomValue;


namespace RCNet.Neural.Network.SM.Preprocessing
{
    /// <summary>
    /// Implements the neural data preprocessor, one of the main components of the State Machine.
    /// </summary>
    [Serializable]
    public class NeuralPreprocessor
    {
        //Constants
        private const double NormalizerDefaultReserve = 0.1d;

        //Static attributes
        /// <summary>
        /// Input data will be normalized to this range before the usage in the reservoirs
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
        /// Collection of input fields normalizers
        /// </summary>
        private Normalizer[] _inputNormalizerCollection;

        //Attribute properties
        /// <summary>
        /// Collection of reservoir instances.
        /// </summary>
        public List<Reservoir> ReservoirCollection { get; }
        /// <summary>
        /// All predictor neurons.
        /// </summary>
        public List<Reservoir.PredictorNeuron> PredictorNeuronCollection { get; }
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
            _inputNormalizerCollection = null;
            //Internal input generators
            _internalInputGeneratorCollection = new List<IGenerator>();
            foreach(NeuralPreprocessorSettings.InputSettings.InternalField field in _settings.InputConfig.InternalFieldCollection)
            {
                if(field.GeneratorSettings.GetType() == typeof(ConstGeneratorSettings))
                {
                    _internalInputGeneratorCollection.Add(new ConstGenerator((ConstGeneratorSettings)field.GeneratorSettings));
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
            PredictorNeuronCollection = new List<Reservoir.PredictorNeuron>();
            NumOfNeurons = 0;
            NumOfPredictors = 0;
            NumOfInternalSynapses = 0;
            ReservoirCollection = new List<Reservoir>(_settings.ReservoirInstanceDefinitionCollection.Count);
            foreach(NeuralPreprocessorSettings.ReservoirInstanceDefinition instanceDefinition in _settings.ReservoirInstanceDefinitionCollection)
            {
                Reservoir reservoir = new Reservoir(instanceDefinition, _dataRange, rand);
                ReservoirCollection.Add(reservoir);
                PredictorNeuronCollection.AddRange(reservoir.PredictorNeuronCollection);
                NumOfNeurons += reservoir.Size;
                NumOfPredictors += reservoir.NumOfOutputPredictors;
                NumOfInternalSynapses += reservoir.NumOfInternalSynapses;
            }
            if(_settings.InputConfig.RouteExternalInputToReadout)
            {
                foreach(NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.ExternalFieldCollection)
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
        /// Initiates collection of preprocessor's normalizers
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        private void InitializeInputNormalizers(List<double[]> inputVectorCollection)
        {
            //Instantiate normalizers
            _inputNormalizerCollection = new Normalizer[_settings.InputConfig.ExternalFieldCollection.Count];
            Parallel.For(0, _inputNormalizerCollection.Length, i =>
            {
                _inputNormalizerCollection[i] = new Normalizer(_dataRange, NormalizerDefaultReserve, true, false);
                //Adjust normalizer
                foreach (double[] vector in inputVectorCollection)
                {
                    _inputNormalizerCollection[i].Adjust(vector[i]);
                }
            });
            return;
        }

        /// <summary>
        /// Normalizes given input vector
        /// </summary>
        /// <param name="vector">Input vector</param>
        /// <returns>Normalized input vector</returns>
        private double[] NormalizeInputVector(double[] vector)
        {
            //Normalize data
            double[] nrmVector = new double[vector.Length];
            for (int i = 0; i < vector.Length; i++)
            {
                nrmVector[i] = _inputNormalizerCollection[i].Normalize(vector[i]);
            }
            return nrmVector;
        }

        /// <summary>
        /// Initiates collection of preprocessor's normalizers and normalizes given collection of input vectors
        /// </summary>
        /// <param name="inputVectorCollection">Collection of input vectors</param>
        /// <returns>Normalized collection of input vectors</returns>
        private double[][] NormalizeInputVectorCollection(List<double[]> inputVectorCollection)
        {
            //Instantiate normalizers
            InitializeInputNormalizers(inputVectorCollection);
            //Normalize data
            double[][] nrmInputVectorCollection = new double[inputVectorCollection.Count][];
            Parallel.For(0, inputVectorCollection.Count, i =>
            {
                nrmInputVectorCollection[i] = NormalizeInputVector(inputVectorCollection[i]);

            });
            return nrmInputVectorCollection;
        }

        /// <summary>
        /// Initiates collection of preprocessor's normalizers
        /// </summary>
        /// <param name="inputPatternCollection">Collection of input patterns</param>
        private void InitializeInputNormalizers(List<List<double[]>> inputPatternCollection)
        {
            //Instantiate and adjust normalizers
            _inputNormalizerCollection = new Normalizer[_settings.InputConfig.ExternalFieldCollection.Count];
            Parallel.For(0, _settings.InputConfig.ExternalFieldCollection.Count, i =>
            {
                _inputNormalizerCollection[i] = new Normalizer(NormalizerDefaultReserve, true, false);
                foreach (List<double[]> pattern in inputPatternCollection)
                {
                    foreach (double[] vector in pattern)
                    {
                        _inputNormalizerCollection[i].Adjust(vector[i]);
                    }
                }
            });
            return;
        }

        /// <summary>
        /// Normalizes given input pattern
        /// </summary>
        /// <param name="pattern">Input pattern</param>
        /// <returns>Normalized input pattern</returns>
        private List<double[]> NormalizeInputPattern(List<double[]> pattern)
        {
            //Normalize data
            List<double[]> nrmPattern = new List<double[]>(pattern.Count);
            foreach (double[] vector in pattern)
            {
                double[] nrmVector = new double[vector.Length];
                int numOfSets = vector.Length / _inputNormalizerCollection.Length;
                for (int set = 0; set < numOfSets; set++)
                {
                    for (int i = 0; i < _inputNormalizerCollection.Length; i++)
                    {
                        nrmVector[set * _inputNormalizerCollection.Length + i] = _inputNormalizerCollection[i].Normalize(vector[set * _inputNormalizerCollection.Length + i]);
                    }
                }
                nrmPattern.Add(nrmVector);
            }
            return nrmPattern;
        }

        /// <summary>
        /// Initiates collection of preprocessor's normalizers and normalizes given collection of input patterns
        /// </summary>
        /// <param name="inputPatternCollection">Collection of input patterns</param>
        /// <returns>Normalized collection of input patterns</returns>
        private List<double[]>[] NormalizeInputPatternCollection(List<List<double[]>> inputPatternCollection)
        {
            //Instantiate normalizers
            InitializeInputNormalizers(inputPatternCollection);
            //Normalize data
            List<double[]>[] nrmInputPatternCollection = new List<double[]>[inputPatternCollection.Count];
            Parallel.For(0, inputPatternCollection.Count, i =>
            {
                nrmInputPatternCollection[i] = NormalizeInputPattern(inputPatternCollection[i]);
            });
            return nrmInputPatternCollection;
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
            double[] completedInputVector = AddInputsFromInternalGenerators(externalInputVector);
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
                predictorsIdx += reservoir.NumOfOutputPredictors;
            }
            if (_settings.InputConfig.RouteExternalInputToReadout)
            {
                int fieldIdx = 0;
                foreach (NeuralPreprocessorSettings.InputSettings.Field field in _settings.InputConfig.ExternalFieldCollection)
                {
                    if (field.AllowRoutingToReadout)
                    {
                        predictors[predictorsIdx] = completedInputVector[fieldIdx];
                        ++predictorsIdx;
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
            //Add internal input
            List<double[]> completedInputPattern = new List<double[]>(externalInputPattern.Count);
            foreach (double[] externalInputVector in externalInputPattern)
            {
                completedInputPattern.Add(AddInputsFromInternalGenerators(externalInputVector));
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
                predictorsIdx += reservoir.NumOfOutputPredictors;
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
            if (_inputNormalizerCollection == null)
            {
                throw new Exception("Preprocessor was not initialized by the sample data bundle (normalizers are not instantiated yet).");
            }
            //Check calling consistency
            if (_settings.InputConfig.FeedingType == CommonEnums.InputFeedingType.Patterned)
            {
                throw new Exception("Called incorrect version of Preprocess function for patterned input feeding.");
            }
            //Normalize vector, push it into the preprocessor and return result
            return PushInput(NormalizeInputVector(input), false);
        }

        /// <summary>
        /// Pushes input pattern into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="inputPattern">Patterned input values in natural form</param>
        public double[] Preprocess(List<double[]> inputPattern)
        {
            //Check readyness
            if (_inputNormalizerCollection == null)
            {
                throw new Exception("Preprocessor was not initialized by the sample data bundle (normalizers are not instantiated yet).");
            }
            //Check calling consistency
            if (_settings.InputConfig.FeedingType == CommonEnums.InputFeedingType.Continuous)
            {
                throw new Exception("Called incorrect version of Preprocess function for continuous input feeding.");
            }
            //Normalize pattern, push it into the preprocessor and return result
            return PushInput(NormalizeInputPattern(inputPattern), false);
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
            if (_settings.InputConfig.FeedingType == CommonEnums.InputFeedingType.Patterned)
            {
                throw new Exception("Called incorrect version of InitializeAndPreprocessBundle function for patterned input feeding.");
            }
            //Reset the internal states and also statistics
            Reset(true);
            //Initialize normalizers and normalize input data
            double[][] nrmInputVectorCollection = NormalizeInputVectorCollection(vectorBundle.InputVectorCollection);
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(vectorBundle.InputVectorCollection.Count - _settings.InputConfig.BootCycles);
            //Collect predictors
            for (int dataSetIdx = 0; dataSetIdx < vectorBundle.InputVectorCollection.Count; dataSetIdx++)
            {
                bool afterBoot = (dataSetIdx >= _settings.InputConfig.BootCycles);
                //Push input data into the network
                double[] predictors = PushInput(nrmInputVectorCollection[dataSetIdx], afterBoot);
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
            if (_settings.InputConfig.FeedingType == CommonEnums.InputFeedingType.Continuous)
            {
                throw new Exception("Called incorrect version of InitializeAndPreprocessBundle function for continuous input feeding.");
            }
            //Reset the internal states and also statistics
            Reset(true);
            //Initialize normalizers and normalize input data
            List<double[]>[] nrmInputPatternCollection = NormalizeInputPatternCollection(patternBundle.InputPatternCollection);
            //Allocate output bundle
            VectorBundle outputBundle = new VectorBundle(patternBundle.InputPatternCollection.Count);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < nrmInputPatternCollection.Length; dataSetIdx++)
            {
                //Push input data into the network
                double[] predictors = PushInput(nrmInputPatternCollection[dataSetIdx], true);
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
