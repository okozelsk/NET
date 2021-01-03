using System;
using System.Collections.Generic;

namespace RCNet.MathTools.Probability
{
    /// <summary>
    /// Implements a template class of the probabilistic selector of an element from the inner collection.
    /// </summary>
    /// <remarks>
    /// The ad-hoc selection is based on the probability distribution.
    /// </remarks>
    [Serializable]
    public class ProbabilisticSelector<T>
    {
        //Attribute properties
        /// <summary>
        /// Gets the collection of elements to choose from.
        /// </summary>
        public List<T> Elements { get; }

        //Attributes
        private readonly Random _rand;
        private readonly List<Tuple<double, T>> _origElements;
        private List<Tuple<double, T>> _probElements;

        //Constructors
        /// <summary>
        /// Creates an unitialized instance.
        /// </summary>
        /// <param name="seek">The initial seek of the random generator.</param>
        public ProbabilisticSelector(int seek = 0)
        {
            _rand = new Random(seek);
            Elements = new List<T>();
            _origElements = new List<Tuple<double, T>>();
            _probElements = null;
            return;
        }

        /// <summary>
        /// The copy constructor.
        /// </summary>
        /// <param name="sourceOrigElements"></param>
        public ProbabilisticSelector(List<Tuple<double, T>> sourceOrigElements)
            : this()
        {
            _origElements.AddRange(sourceOrigElements);
            return;
        }

        //Methods
        /// <summary>
        /// Adds the new element.
        /// </summary>
        /// <param name="relShare">The relative share of the element's probability.</param>
        /// <param name="element">The element to be added.</param>
        public void Add(double relShare, T element)
        {
            if (_probElements != null)
            {
                throw new InvalidOperationException($"The selector was already finalized so it can not be modified.");
            }
            if (relShare > 0d)
            {
                Elements.Add(element);
                _origElements.Add(new Tuple<double, T>(relShare, element));
            }
            return;
        }

        private void FinalizeProbabilities()
        {
            if (_probElements == null)
            {
                double sum = 0;
                foreach (Tuple<double, T> element in _origElements)
                {
                    sum += element.Item1;
                }
                _probElements = new List<Tuple<double, T>>(_origElements.Count);
                double borderP = 0;
                foreach (Tuple<double, T> element in _origElements)
                {
                    borderP += element.Item1 / sum;
                    _probElements.Add(new Tuple<double, T>(borderP, element.Item2));
                }
            }
            return;
        }

        /// <summary>
        /// Selects the next element.
        /// </summary>
        public T SelectNext()
        {
            if (_probElements == null) FinalizeProbabilities();
            double p = _rand.NextDouble();
            foreach (Tuple<double, T> element in _probElements)
            {
                if (p < element.Item1)
                {
                    return element.Item2;
                }
            }
            throw new InvalidOperationException($"Can't select an element.");
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            ProbabilisticSelector<T> cmpObj = obj as ProbabilisticSelector<T>;
            if (_origElements.Count != cmpObj._origElements.Count)
            {
                return false;
            }
            for (int i = 0; i < _origElements.Count; i++)
            {
                if (_origElements[i].Item1 != cmpObj._origElements[i].Item1 ||
                   !Equals(_origElements[i].Item2, cmpObj._origElements[i].Item2)
                    )
                {
                    return false;
                }
            }
            return true;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance.
        /// </summary>
        public ProbabilisticSelector<T> DeepClone()
        {
            ProbabilisticSelector<T> clone = new ProbabilisticSelector<T>(this._origElements);
            return clone;
        }

    }//ProbabilisticSelector

}//Namespace
