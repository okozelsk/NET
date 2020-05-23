using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Information about a neuron location within the neural preprocessor
    /// </summary>
    [Serializable]
    public class NeuronLocation
    {
        /// <summary>
        /// Neuron's home reservoir ID
        /// </summary>
        public int ReservoirID { get; }
        /// <summary>
        /// Neuron's index in a reservoir flat stucture
        /// </summary>
        public int ReservoirFlatIdx { get; }
        /// <summary>
        /// Neuron's home pool ID
        /// </summary>
        public int PoolID { get; }
        /// <summary>
        /// Neuron index in a pool flat stucture
        /// </summary>
        public int PoolFlatIdx { get; }
        /// <summary>
        /// Neuron's group index within the pool
        /// </summary>
        public int PoolGroupID { get; }
        /// <summary>
        /// Coordinates (x,y,z) within the reservoir
        /// </summary>
        public int[] ReservoirCoordinates { get; }

        //Constructor
        /// <summary>
        /// Instantiates an initialized instance
        /// </summary>
        /// <param name="reservoirID">Home reservoir index.</param>
        /// <param name="reservoirFlatIdx">Index of the neuron in a reservoir flat structure.</param>
        /// <param name="poolID">Home pool index.</param>
        /// <param name="poolFlatIdx">Index of the neuron in a pool flat structure.</param>
        /// <param name="poolGroupID">Index of the neuron's group in a pool.</param>
        /// <param name="x">X coordinate in the reservoir</param>
        /// <param name="y">Y coordinate in the reservoir</param>
        /// <param name="z">Z coordinate in the reservoir</param>
        public NeuronLocation(int reservoirID,
                               int reservoirFlatIdx,
                               int poolID,
                               int poolFlatIdx,
                               int poolGroupID,
                               int x,
                               int y,
                               int z
                               )
        {
            ReservoirID = reservoirID;
            ReservoirFlatIdx = reservoirFlatIdx;
            PoolID = poolID;
            PoolFlatIdx = poolFlatIdx;
            PoolGroupID = poolGroupID;
            ReservoirCoordinates = new int[3];
            ReservoirCoordinates[0] = x;
            ReservoirCoordinates[1] = y;
            ReservoirCoordinates[2] = z;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="source">Source instance.</param>
        public NeuronLocation(NeuronLocation source)
            : this(source.ReservoirID,
                  source.ReservoirFlatIdx,
                  source.PoolID,
                  source.PoolFlatIdx,
                  source.PoolGroupID,
                  source.ReservoirCoordinates[0],
                  source.ReservoirCoordinates[1],
                  source.ReservoirCoordinates[2])
        {
            return;
        }

        /// <summary>
        /// Creates deep copy of this instance
        /// </summary>
        public NeuronLocation DeepClone()
        {
            return new NeuronLocation(this);
        }

    }//NeuronPlacement

}//Namespace
