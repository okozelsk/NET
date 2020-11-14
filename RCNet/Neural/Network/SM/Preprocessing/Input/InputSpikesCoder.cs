using System;
using System.Collections.Generic;
using System.Text;
using RCNet.Extensions;
using RCNet.Neural.Data.Coders.AnalogToSpiking;

namespace RCNet.Neural.Network.SM.Preprocessing.Input
{
    /// <summary>
    /// Provides composite coding of input analog value to spikes
    /// </summary>
    [Serializable]
    public class InputSpikesCoder
    {
        //Attribute properties
        /// <summary>
        /// Number of spikes of the largest component
        /// </summary>
        public int LargestComponentLength { get; }

        /// <summary>
        /// Spikes of component by component
        /// </summary>
        public byte[][] ComponentSpikesCollection { get; }

        /// <summary>
        /// Spikes of all components in one row
        /// </summary>
        public byte[] AllSpikesFlatCollection { get; }

        //Attributes
        private readonly InputSpikesCoderSettings _encodingCfg;
        private readonly List<A2SCoderBase> _coderCollection;
        private readonly int _numOfComponents;

        //Constructor
        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="encodingCfg">Encoding configuration</param>
        public InputSpikesCoder(InputSpikesCoderSettings encodingCfg)
        {
            _encodingCfg = (InputSpikesCoderSettings)encodingCfg.DeepClone();
            _coderCollection = new List<A2SCoderBase>(_encodingCfg.CoderCfgCollection.Count);
            _numOfComponents = 0;
            LargestComponentLength = 0;
            foreach(RCNetBaseSettings coderCfg in _encodingCfg.CoderCfgCollection)
            {
                A2SCoderBase coder = A2SCoderFactory.Create(coderCfg);
                _coderCollection.Add(coder);
                _numOfComponents += coder.NumOfComponents;
                LargestComponentLength = Math.Max(LargestComponentLength, coder.BaseCodeLength);
            }
            ComponentSpikesCollection = new byte[_numOfComponents][];
            switch (_encodingCfg.Regime)
            {
                case InputEncoder.SpikingInputEncodingRegime.Forbidden:
                    {
                        AllSpikesFlatCollection = new byte[0];
                    }
                    break;
                case InputEncoder.SpikingInputEncodingRegime.Horizontal:
                    {
                        int idx = 0;
                        int allSpikesLength = 0;
                        foreach(A2SCoderBase coder in _coderCollection)
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
                case InputEncoder.SpikingInputEncodingRegime.Vertical:
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
        /// <summary>
        /// Encoding regime
        /// </summary>
        public InputEncoder.SpikingInputEncodingRegime Regime { get { return _encodingCfg.Regime; } }

        //Methods
        /// <summary>
        /// Resets coders and buffers
        /// </summary>
        public void Reset()
        {
            foreach (A2SCoderBase coder in _coderCollection)
            {
                coder.Reset();
            }
            foreach(byte[] buffer in ComponentSpikesCollection)
            {
                buffer.Populate((byte)0);
            }
            AllSpikesFlatCollection.Populate((byte)0);
            return;
        }

        /// <summary>
        /// Encodes given analog value as the spikes
        /// </summary>
        /// <param name="normalizedValue">Normalized analog value between -1 and 1</param>
        public void Encode(double normalizedValue)
        {
            if(Regime != InputEncoder.SpikingInputEncodingRegime.Forbidden)
            {
                int componentIdx = 0;
                int flatIdx = 0;
                foreach(A2SCoderBase coder in _coderCollection)
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
}
