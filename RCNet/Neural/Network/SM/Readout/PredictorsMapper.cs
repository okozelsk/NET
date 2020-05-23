using RCNet.Extensions;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Readout
{
    /// <summary>
    /// Maps specific predictors to readout units
    /// </summary>
    [Serializable]
    public class PredictorsMapper
    {
        //Attribute properties
        /// <summary>
        /// Collection of switches generally enabling/disabling predictors
        /// </summary>
        public bool[] PredictorGeneralSwitchCollection { get; private set; }
        //Attributes
        /// <summary>
        /// Mapping of readout unit to switches determining what predictors are assigned to.
        /// </summary>
        private readonly Dictionary<string, ReadoutUnitMap> _mapCollection;
        private readonly int _numOfAllowedPredictors;
        /// <summary>
        /// Creates initialized instance
        /// </summary>
        /// <param name="numOfPredictors">Total number of available predictors</param>
        public PredictorsMapper(int numOfPredictors)
        {
            PredictorGeneralSwitchCollection = new bool[numOfPredictors];
            PredictorGeneralSwitchCollection.Populate(true);
            _numOfAllowedPredictors = numOfPredictors;
            _mapCollection = new Dictionary<string, ReadoutUnitMap>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="predictorGeneralSwitchCollection">Collection of switches generally enabling/disabling predictors</param>
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
        /// Adds new mapping for ReadoutUntit
        /// </summary>
        /// <param name="readoutUnitName"></param>
        /// <param name="map">Boolean switches indicating if to use available prdictor for the ReadoutUnit</param>
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
        /// Creates input vector containing specific subset of predictors for the ReadoutUnit.
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
        /// Creates input vector collection where each vector containing specific subset of predictors for the ReadoutUnit.
        /// </summary>
        /// <param name="readoutUnitName">ReadoutUnit name</param>
        /// <param name="predictorsCollection">Collection of available predictors</param>
        public List<double[]> CreateVectorCollection(string readoutUnitName, List<double[]> predictorsCollection)
        {
            List<double[]> vectorCollection = new List<double[]>(predictorsCollection.Count);
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
        /// Maps specific predictors to readout unit
        /// </summary>
        [Serializable]
        private class ReadoutUnitMap
        {
            //Attribute properties
            /// <summary>
            /// Boolean switches indicating if to use available prdictor for this ReadoutUnit
            /// </summary>
            public bool[] Map { get; set; }
            /// <summary>
            /// Resulting length of ReadoutUnit's input vector (number of true switches in the Map)
            /// </summary>
            public int VectorLength { get; private set; }

            /// <summary>
            /// Creates initialized instance
            /// </summary>
            /// <param name="map">Boolean switches indicating if to use available prdictor for this ReadoutUnit.</param>
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
