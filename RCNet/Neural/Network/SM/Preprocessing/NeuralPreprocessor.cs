using System;
using System.Collections.Generic;
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
        //Static attributes
        /// <summary>
        /// Data range. Input data range has to be always between -1 and 1.
        /// </summary>
        public static readonly Interval DataRange = new Interval(-1, 1);

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
        private NeuralPreprocessorSettings _settings;
        /// <summary>
        /// Collection of the internal input generators associated with the internal input fields
        /// </summary>
        private readonly List<IGenerator> _internalInputGeneratorCollection;

        //Attribute properties
        /// <summary>
        /// Collection of reservoir instances.
        /// </summary>
        public List<Reservoir> ReservoirCollection { get; }
        /// <summary>
        /// Number of Neural Preprocessor predictors
        /// </summary>
        public int NumOfPredictors { get; }

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
            NumOfPredictors = 0;
            ReservoirCollection = new List<Reservoir>(_settings.ReservoirInstanceDefinitionCollection.Count);
            foreach(NeuralPreprocessorSettings.ReservoirInstanceDefinition instanceDefinition in _settings.ReservoirInstanceDefinitionCollection)
            {
                Reservoir reservoir = new Reservoir(instanceDefinition, DataRange, rand);
                ReservoirCollection.Add(reservoir);
                NumOfPredictors += reservoir.NumOfOutputPredictors;
            }
            if(_settings.InputConfig.RouteExternalInputToReadout)
            {
                NumOfPredictors += _settings.InputConfig.ExternalFieldCollection.Count;
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
            foreach(IGenerator generator in _internalInputGeneratorCollection)
            {
                generator.Reset();
            }
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
        private double[] AddInternalInputVector(double[] externalInputVector)
        {
            double[] smInput = new double[_settings.InputConfig.NumOfFields];
            externalInputVector.CopyTo(smInput, 0);
            for(int i = 0; i < _internalInputGeneratorCollection.Count; i++)
            {
                smInput[_settings.InputConfig.ExternalFieldCollection.Count + i] = _internalInputGeneratorCollection[i].Next();
            }
            return smInput;
        }

        /// <summary>
        /// Pushes input vector into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="externalInputVector">Input values</param>
        /// <param name="collectStatesStatistics">
        /// The parameter indicates whether to update internal statistics
        /// </param>
        public double[] PushInput(double[] externalInputVector, bool collectStatesStatistics)
        {
            double[] completedInputVector = AddInternalInputVector(externalInputVector);
            double[] predictors = new double[NumOfPredictors];
            int predictorsIdx = 0;
            //Compute reservoir(s)
            foreach (Reservoir reservoir in ReservoirCollection)
            {
                double[] reservoirInput = new double[reservoir.InstanceDefinition.NPInputFieldIdxCollection.Count];
                for(int i = 0; i < reservoir.InstanceDefinition.NPInputFieldIdxCollection.Count; i++)
                {
                    reservoirInput[i] = completedInputVector[reservoir.InstanceDefinition.NPInputFieldIdxCollection[i]];
                }
                //Compute reservoir
                reservoir.Compute(reservoirInput, collectStatesStatistics);
                reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                predictorsIdx += reservoir.NumOfOutputPredictors;
            }
            if(_settings.InputConfig.RouteExternalInputToReadout)
            {
                completedInputVector.CopyTo(predictors, predictorsIdx);
            }
            return predictors;
        }

        /// <summary>
        /// Pushes input pattern into the reservoirs and returns the predictors
        /// </summary>
        /// <param name="externalInputPattern">Input pattern</param>
        public double[] PushInput(List<double[]> externalInputPattern)
        {
            double[] predictors = new double[NumOfPredictors];
            int predictorsIdx = 0;
            //Reset SM but keep statistics
            Reset(false);
            //Add internal input
            List<double[]> completedInputPattern = new List<double[]>(externalInputPattern.Count);
            foreach(double[] externalInputVector in externalInputPattern)
            {
                completedInputPattern.Add(AddInternalInputVector(externalInputVector));
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
                    reservoir.Compute(reservoirInput, true);
                }
                reservoir.CopyPredictorsTo(predictors, predictorsIdx);
                predictorsIdx += reservoir.NumOfOutputPredictors;
            }
            return predictors;
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
        public VectorBundle PreprocessBundle(PatternBundle patternBundle,
                                             PredictorsCollectionCallbackDelegate informativeCallback = null,
                                             Object userObject = null
                                             )
        {
            if (_settings.InputConfig.FeedingType == CommonEnums.InputFeedingType.Continuous)
            {
                throw new Exception("This version of PreprocessBundle function is not useable for continuous input feeding.");
            }
            //Allocations
            VectorBundle outputBundle = new VectorBundle(patternBundle.InputPatternCollection.Count);
            //Reset the internal states and statistics
            Reset(true);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < patternBundle.InputPatternCollection.Count; dataSetIdx++)
            {
                //Push input data into the network
                double[] predictors = PushInput(patternBundle.InputPatternCollection[dataSetIdx]);
                outputBundle.InputVectorCollection.Add(predictors);
                //Add desired outputs
                outputBundle.OutputVectorCollection.Add(patternBundle.OutputVectorCollection[dataSetIdx]);
                //Informative callback
                informativeCallback?.Invoke(patternBundle.InputPatternCollection.Count, dataSetIdx + 1, userObject);
            }
            return outputBundle;
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
        public VectorBundle PreprocessBundle(VectorBundle vectorBundle,
                                             PredictorsCollectionCallbackDelegate informativeCallback = null,
                                             Object userObject = null
                                             )
        {
            if (_settings.InputConfig.FeedingType == CommonEnums.InputFeedingType.Patterned)
            {
                throw new Exception("This version of PreprocessBundle function is not useable for patterned input feeding.");
            }
            int dataSetLength = vectorBundle.InputVectorCollection.Count;
            //Allocations
            VectorBundle outputBundle = new VectorBundle(dataSetLength - _settings.InputConfig.BootCycles);
            //Reset the internal states and statistics
            Reset(true);
            //Collection
            for (int dataSetIdx = 0; dataSetIdx < dataSetLength; dataSetIdx++)
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
                informativeCallback?.Invoke(dataSetLength, dataSetIdx + 1, userObject);
            }
            return outputBundle;
        }

        /// <summary>
        /// Collects the key statistics of each reservoir instance.
        /// It is very important to follow these statistics and adjust parameters of the reservoirs so that the neurons
        /// are not oversaturated or inactive.
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
