using RCNet.Extensions;
using RCNet.Neural.Data.Coders.AnalogToSpiking;
using System;
using System.Collections.Generic;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Implements the coder of input analog value to spike codes.
    /// </summary>
    [Serializable]
    public class InputSpikesCoder
    {
        //Attribute properties
        /// <summary>
        /// The length of the largest component spike code.
        /// </summary>
        public int LargestComponentLength { get; }

        /// <summary>
        /// The spike codes - component by component.
        /// </summary>
        public byte[][] ComponentSpikesCollection { get; }

        /// <summary>
        /// All the spike codes from all components in a one row.
        /// </summary>
        public byte[] AllSpikesFlatCollection { get; }

        //Attributes
        private readonly InputSpikesCoderSettings _encodingCfg;
        private readonly List<A2SCoderBase> _coderCollection;
        private readonly int _numOfComponents;

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="cfg">The configuration of the coder.</param>
        public InputSpikesCoder(InputSpikesCoderSettings cfg)
        {
            _encodingCfg = (InputSpikesCoderSettings)cfg.DeepClone();
            _coderCollection = new List<A2SCoderBase>(_encodingCfg.CoderCfgCollection.Count);
            _numOfComponents = 0;
            LargestComponentLength = 0;
            foreach (RCNetBaseSettings coderCfg in _encodingCfg.CoderCfgCollection)
            {
                A2SCoderBase coder = A2SCoderFactory.Create(coderCfg);
                _coderCollection.Add(coder);
                _numOfComponents += coder.NumOfComponents;
                LargestComponentLength = Math.Max(LargestComponentLength, coder.BaseCodeLength);
            }
            ComponentSpikesCollection = new byte[_numOfComponents][];
            switch (_encodingCfg.Regime)
            {
                case InputEncoder.InputSpikesCoding.Forbidden:
                    {
                        AllSpikesFlatCollection = new byte[0];
                    }
                    break;
                case InputEncoder.InputSpikesCoding.Horizontal:
                    {
                        int idx = 0;
                        int allSpikesLength = 0;
                        foreach (A2SCoderBase coder in _coderCollection)
                        {
                            for (int i = 0; i < coder.NumOfComponents; i++)
                            {
                                ComponentSpikesCollection[idx] = new byte[coder.BaseCodeLength];
                                ComponentSpikesCollection[idx].Populate((byte)0);
                                allSpikesLength += coder.BaseCodeLength;
                                ++idx;
                            }
                        }
                        AllSpikesFlatCollection = new byte[allSpikesLength];
                        AllSpikesFlatCollection.Populate((byte)0);
                    }
                    break;
                case InputEncoder.InputSpikesCoding.Vertical:
                    {
                        int idx = 0;
                        int allSpikesLength = 0;
                        foreach (A2SCoderBase coder in _coderCollection)
                        {
                            for (int i = 0; i < coder.NumOfComponents; i++)
                            {
                                ComponentSpikesCollection[idx] = new byte[LargestComponentLength];
                                ComponentSpikesCollection[idx].Populate((byte)0);
                                allSpikesLength += coder.BaseCodeLength;
                                ++idx;
                            }
                        }
                        AllSpikesFlatCollection = new byte[allSpikesLength];
                        AllSpikesFlatCollection.Populate((byte)0);
                    }
                    break;

            }

            return;
        }

        //Properties
        /// <inheritdoc cref="InputEncoder.InputSpikesCoding"/>
        public InputEncoder.InputSpikesCoding Regime { get { return _encodingCfg.Regime; } }

        //Methods
        /// <summary>
        /// Resets the coder to its initial state.
        /// </summary>
        public void Reset()
        {
            foreach (A2SCoderBase coder in _coderCollection)
            {
                coder.Reset();
            }
            foreach (byte[] buffer in ComponentSpikesCollection)
            {
                buffer.Populate((byte)0);
            }
            AllSpikesFlatCollection.Populate((byte)0);
            return;
        }

        /// <summary>
        /// Encodes the normalized analog value.
        /// </summary>
        /// <param name="normalizedValue">The analog value normalized between -1 and 1.</param>
        public void Encode(double normalizedValue)
        {
            if (Regime != InputEncoder.InputSpikesCoding.Forbidden)
            {
                int componentIdx = 0;
                int flatIdx = 0;
                foreach (A2SCoderBase coder in _coderCollection)
                {
                    byte[][] buffer = coder.GetCode(normalizedValue);
                    for (int i = 0; i < buffer.Length; i++)
                    {
                        ComponentSpikesCollection[componentIdx].Populate((byte)0);
                        buffer[i].CopyTo(ComponentSpikesCollection[componentIdx], 0);
                        buffer[i].CopyTo(AllSpikesFlatCollection, flatIdx);
                        flatIdx += buffer[i].Length;
                        ++componentIdx;
                    }
                }
            }
            return;
        }

    }//InputSpikesCoder

}//Namespace

