using System;

namespace RCNet.Neural.Network.SM.Preprocessing.Neuron
{
    /// <summary>
    /// Implements the holder of an information about a neuron location within the neural preprocessor.
    /// </summary>
    [Serializable]
    public class NeuronLocation
    {
        /// <summary>
        /// An identifier of the neuron's origin reservoir.
        /// </summary>
        public int ReservoirID { get; }
        /// <summary>
        /// The zero-based index of the neuron in the flat array of the reservoir's neurons.
        /// </summary>
        public int ReservoirFlatIdx { get; }
        /// <summary>
        /// An identifier of the neuron's origin pool.
        /// </summary>
        public int PoolID { get; }
        /// <summary>
        /// The zero-based index of the neuron in the flat array of the pool's neurons.
        /// </summary>
        public int PoolFlatIdx { get; }
        /// <summary>
        /// An identifier of the neuron's origin group.
        /// </summary>
        public int PoolGroupID { get; }
        /// <summary>
        /// The neuron's coordinates (x,y,z) within the reservoir.
        /// </summary>
        public int[] ReservoirCoordinates { get; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="reservoirID">An identifier of the neuron's origin reservoir.</param>
        /// <param name="reservoirFlatIdx">The zero-based index of the neuron in the flat array of the reservoir's neurons.</param>
        /// <param name="poolID">An identifier of the neuron's origin pool.</param>
        /// <param name="poolFlatIdx">The zero-based index of the neuron in the flat array of the pool's neurons.</param>
        /// <param name="poolGroupID">An identifier of the neuron's origin group.</param>
        /// <param name="x">The neuron's X coordinate within the reservoir.</param>
        /// <param name="y">The neuron's Y coordinate within the reservoir.</param>
        /// <param name="z">The neuron's Z coordinate within the reservoir.</param>
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
        /// The copy constructor.
        /// </summary>
        /// <param name="source">The source instance.</param>
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

    }//NeuronLocation

}//Namespace
