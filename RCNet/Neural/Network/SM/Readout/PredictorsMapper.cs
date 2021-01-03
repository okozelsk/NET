using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Implements the mapper of specific predictors to readout units.
    /// </summary>
    [Serializable]
    public class PredictorsMapper
    {
        //Attribute properties
        /// <summary>
        /// The collection of switches generally enabling/disabling the predictors.
        /// </summary>
        public bool[] PredictorGeneralSwitchCollection { get; private set; }

        //Attributes
        private readonly Dictionary<string, ReadoutUnitMap> _mapCollection;
        private readonly int _numOfAllowedPredictors;

        //Constructors
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="numOfPredictors">The total number of predictors.</param>
        public PredictorsMapper(int numOfPredictors)
        {
            PredictorGeneralSwitchCollection = new bool[numOfPredictors];
            PredictorGeneralSwitchCollection.Populate(true);
            _numOfAllowedPredictors = numOfPredictors;
            _mapCollection = new Dictionary<string, ReadoutUnitMap>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="predictorGeneralSwitchCollection">The collection of switches generally enabling/disabling the predictors.</param>
        public PredictorsMapper(bool[] predictorGeneralSwitchCollection)
        {
            PredictorGeneralSwitchCollection = (bool[])predictorGeneralSwitchCollection.Clone();
            _numOfAllowedPredictors = 0;
            for (int i = 0; i < predictorGeneralSwitchCollection.Length; i++)
            {
                if (predictorGeneralSwitchCollection[i]) ++_numOfAllowedPredictors;
            }
            if (_numOfAllowedPredictors == 0 || predictorGeneralSwitchCollection.Length == 0)
            {
                throw new ArgumentException("There is no available predictor", "predictorGeneralSwitchCollection");
            }
            _mapCollection = new Dictionary<string, ReadoutUnitMap>();
            return;
        }

        /// <summary>
        /// Adds the mapping for the specified readout untit.
        /// </summary>
        /// <param name="readoutUnitName">The name of the readout unit.</param>
        /// <param name="map">The collection of switches indicating whether to use a predictor for the readout unit.</param>
        public void Add(string readoutUnitName, bool[] map)
        {
            if (map.Length != PredictorGeneralSwitchCollection.Length)
            {
                throw new ArgumentException("Incorrect number of switches in the map", "map");
            }
            if (readoutUnitName.Length == 0)
            {
                throw new ArgumentException("ReadoutUnit name can not be empty", "readoutUnitName");
            }
            if (_mapCollection.ContainsKey(readoutUnitName))
            {
                throw new ArgumentException($"Mapping already contains mapping for ReadoutUnit {readoutUnitName}", "readoutUnitName");
            }
            //Apply general switches
            bool[] localMap = (bool[])map.Clone();
            int numOfReadoutUnitAllowedPredictors = 0;
            for (int i = 0; i < localMap.Length; i++)
            {
                if (localMap[i])
                {
                    if (!PredictorGeneralSwitchCollection[i])
                    {
                        localMap[i] = false;
                    }
                    else
                    {
                        ++numOfReadoutUnitAllowedPredictors;
                    }
                }
            }
            if (numOfReadoutUnitAllowedPredictors < 1)
            {
                throw new ArgumentException("Map contains no allowed predictors", "map");
            }
            _mapCollection.Add(readoutUnitName, new ReadoutUnitMap(localMap));
            return;
        }

        private double[] CreateVector(double[] predictors, bool[] map, int vectorLength)
        {
            if (predictors.Length != map.Length)
            {
                throw new ArgumentException("Incorrect number of predictors", "predictors");
            }
            double[] vector = new double[vectorLength];
            for (int i = 0, vIdx = 0; i < predictors.Length; i++)
            {
                if (map[i])
                {
                    vector[vIdx] = predictors[i];
                    ++vIdx;
                }
            }
            return vector;
        }

        /// <summary>
        /// Creates an input vector containing specific subset of predictors for the specified readout unit.
        /// </summary>
        /// <param name="readoutUnitName">ReadoutUnit name</param>
        /// <param name="predictors">Available predictors</param>
        public double[] CreateVector(string readoutUnitName, double[] predictors)
        {
            if (predictors.Length != PredictorGeneralSwitchCollection.Length)
            {
                throw new ArgumentException("Incorrect number of predictors", "predictors");
            }
            if (_mapCollection.ContainsKey(readoutUnitName))
            {
                ReadoutUnitMap rum = _mapCollection[readoutUnitName];
                return CreateVector(predictors, rum.Map, rum.VectorLength);
            }
            else
            {
                return CreateVector(predictors, PredictorGeneralSwitchCollection, _numOfAllowedPredictors);
            }
        }

        /// <summary>
        /// Creates an input vector collection where each vector contains the specific subset of predictors for the specified readout unit.
        /// </summary>
        /// <param name="readoutUnitName">ReadoutUnit name</param>
        /// <param name="predictorsCollection">Collection of available predictors</param>
        public List<double[]> CreateVectorCollection(string readoutUnitName, IEnumerable<double[]> predictorsCollection)
        {
            List<double[]> vectorCollection = new List<double[]>();
            ReadoutUnitMap rum = null;
            if (_mapCollection.ContainsKey(readoutUnitName))
            {
                rum = _mapCollection[readoutUnitName];
            }
            foreach (double[] predictors in predictorsCollection)
            {
                if (rum == null)
                {
                    vectorCollection.Add(CreateVector(predictors, PredictorGeneralSwitchCollection, _numOfAllowedPredictors));
                }
                else
                {
                    vectorCollection.Add(CreateVector(predictors, rum.Map, rum.VectorLength));
                }
            }
            return vectorCollection;
        }

        //Inner classes
        /// <summary>
        /// Implements the map of specific predictors to readout unit.
        /// </summary>
        [Serializable]
        private class ReadoutUnitMap
        {
            //Attribute properties
            /// <summary>
            /// The switches indicating whether to use predictor for the readout unit.
            /// </summary>
            public bool[] Map { get; set; }
            /// <summary>
            /// The length of the readout unit's input vector.
            /// </summary>
            public int VectorLength { get; private set; }

            /// <summary>
            /// Creates an initialized instance.
            /// </summary>
            /// <param name="map">The switches indicating whether to use predictor for the readout unit.</param>
            public ReadoutUnitMap(bool[] map)
            {
                Map = map;
                VectorLength = 0;
                foreach (bool bSwitch in Map)
                {
                    if (bSwitch) ++VectorLength;
                }
                return;
            }

        }//ReadoutUnitMap

    }//PredictorsMapper

}//Namespace
