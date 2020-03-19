using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RCNet.MathTools.Probability
{
    /// <summary>
    /// Simple class for realtime decision what item to select based on probability distribution (relative shares)
    /// </summary>
    [Serializable]
    public class RelShareSelector<T>
    {
        //Attribute properties
        /// <summary>
        /// collection of elements to choose from
        /// </summary>
        public List<T> Elements { get; }

        //Attributes
        private readonly Random _rand;
        private readonly List<Tuple<double, T>> _origElements;
        private List<Tuple<double, T>> _probElements;

        //Constructors
        /// <summary>
        /// Creates an unitialized instance
        /// </summary>
        /// <param name="seek">Initial random generator seek</param>
        public RelShareSelector(int seek = 0)
        {
            _rand = new Random(seek);
            Elements = new List<T>();
            _origElements = new List<Tuple<double, T>>();
            _probElements = null;
            return;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="sourceOrigElements"></param>
        public RelShareSelector(List<Tuple<double, T>> sourceOrigElements)
            :this()
        {
            _origElements.AddRange(sourceOrigElements);
            return;
        }

        //Methods
        /// <summary>
        /// Adds new item
        /// </summary>
        /// <param name="relShare">Relative share</param>
        /// <param name="element">Element</param>
        public void Add(double relShare, T element)
        {
            if(_probElements != null)
            {
                throw new Exception("Selector was already finalized and can not be modified.");
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
        /// Probabilistic selection of the next element
        /// </summary>
        public T SelectNext()
        {
            if (_probElements == null) FinalizeProbabilities();
            double p = _rand.NextDouble();
            foreach (Tuple<double, T> element in _probElements)
            {
                if(p < element.Item1)
                {
                    return element.Item2;
                }
            }
            throw new Exception("Can't select item.");
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            RelShareSelector<T> cmpObj = obj as RelShareSelector<T>;
            if (_origElements.Count != cmpObj._origElements.Count)
            {
                return false;
            }
            for(int i = 0; i < _origElements.Count; i++)
            {
                if(_origElements[i].Item1 != cmpObj._origElements[i].Item1 ||
                   !Equals(_origElements[i].Item2, cmpObj._origElements[i].Item2)
                    )
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// See the base.
        /// </summary>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Creates the deep copy instance of this instance
        /// </summary>
        public RelShareSelector<T> DeepClone()
        {
            RelShareSelector<T> clone = new RelShareSelector<T>(this._origElements);
            return clone;
        }

    }//RelShareSelector

}//Namespace
