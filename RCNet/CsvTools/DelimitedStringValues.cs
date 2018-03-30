using System;
using System.Collections.Generic;
using System.Text;

namespace RCNet.CsvTools
{
    /// <summary>
    /// Helper class for csv string operations.
    /// </summary>
    public class DelimitedStringValues
    {
        //Constants
        //Delimiters
        /// <summary>
        /// The semicolon
        /// </summary>
        public const char SemicolonDelimiter = ';';
        /// <summary>
        /// The comma
        /// </summary>
        public const char CommaDelimiter = ',';
        /// <summary>
        /// The default delimiter
        /// </summary>
        public const char DefaultDelimiter = SemicolonDelimiter;

        //Attributes
        private char _delimiter;
        private List<string> _stringValueCollection;

        //Constructor
        /// <summary>
        /// Creates the new instance
        /// </summary>
        /// <param name="delimiter">The delimiter of the string values</param>
        public DelimitedStringValues(char delimiter = DefaultDelimiter)
        {
            _delimiter = delimiter;
            _stringValueCollection = new List<string>();
            return;
        }

        //Properties
        /// <summary>
        /// Current delimiter of the string values
        /// </summary>
        public char Delimiter { get { return _delimiter; } }
        /// <summary>
        /// Number of string values
        /// </summary>
        public int NumOfStringValues { get { return _stringValueCollection.Count; } }
        /// <summary>
        /// Collection of string values
        /// </summary>
        public List<string> StringValueCollection { get { return _stringValueCollection; } }

        //Methods
        //Static methods
        /// <summary>
        /// Tries to recognize used delimiter
        /// </summary>
        /// <param name="sampleDelimitedData">Sample delimited data</param>
        /// <returns>Found or default delimiter</returns>
        public static char RecognizeDelimiter(string sampleDelimitedData)
        {
            if (sampleDelimitedData.IndexOf(SemicolonDelimiter) != -1)
            {
                return SemicolonDelimiter;
            }
            if (sampleDelimitedData.IndexOf(CommaDelimiter) != -1)
            {
                return CommaDelimiter;
            }
            return DefaultDelimiter;
        }

        /// <summary>
        /// Converts collection of string values to a single string where values are delimited by the given delimiter.
        /// </summary>
        /// <param name="stringValueCollection">Collection of string values</param>
        /// <param name="delimiter">Values delimiter to be used</param>
        /// <returns>Built string containing delimited values</returns>
        public static string ValuesToDelimited(IEnumerable<string> stringValueCollection, char delimiter = DefaultDelimiter)
        {
            StringBuilder output = new StringBuilder();
            bool firstVal = true;
            foreach(string value in stringValueCollection)
            {
                if(!firstVal)
                {
                    output.Append(delimiter);
                }
                output.Append(value);
                firstVal = false;
            }
            return output.ToString();
        }

        /// <summary>
        /// Splits the given string containing delimited values to the collection of string values
        /// </summary>
        /// <param name="delimValues">String containing delimited values</param>
        /// <param name="delimiter">Delimiter of the values</param>
        /// <returns>Collection of the string values</returns>
        public static List<string> DelimitedToValues(string delimValues, char delimiter = DefaultDelimiter)
        {
            List<string> values = new List<string>();
            if(delimValues.Length > 0)
            {
                char[] allowedDelims = new char[1];
                allowedDelims[0] = delimiter;
                values.AddRange(delimValues.Split(allowedDelims, StringSplitOptions.None));
            }
            return values;
        }

        //Instance methods
        /// <summary>
        /// Cleares the internal collection of string values
        /// </summary>
        public void Reset()
        {
            _stringValueCollection.Clear();
            return;
        }

        /// <summary>
        /// Changes the delimiter
        /// </summary>
        /// <param name="delimiter">New delimiter</param>
        public void ChangeDelimiter(char delimiter)
        {
            _delimiter = delimiter;
            return;
        }

        /// <summary>
        /// Adds string value into the internal collection of string values
        /// </summary>
        /// <param name="value">Value to be added</param>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int AddValue(string value)
        {
            _stringValueCollection.Add(value);
            return _stringValueCollection.Count;
        }

        /// <summary>
        /// Removes string value at specified position from the internal collection of string values
        /// </summary>
        /// <param name="index">The zero-based index of value to be removed</param>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int RemoveAt(int index)
        {
            _stringValueCollection.RemoveAt(index);
            return _stringValueCollection.Count;
        }

        /// <summary>
        /// Removes trailing empty or white spaced string values
        /// </summary>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int RemoveTrailingWhites()
        {
            while(_stringValueCollection.Count > 0 && _stringValueCollection[_stringValueCollection.Count - 1].Trim() == string.Empty)
            {
                _stringValueCollection.RemoveAt(_stringValueCollection.Count - 1);
            }
            return _stringValueCollection.Count;
        }

        /// <summary>
        /// Loads string values into the internal collection from a string containing delimited values
        /// </summary>
        /// <param name="delimValues">String containing delimited values</param>
        /// <param name="reset">Indicates if to clear the internal collection before the load</param>
        /// <param name="recognizeDelimiter">If false then the instance delimiter will be used. If true then delimiter will be tried to recognized from the given data.</param>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int LoadFromString(string delimValues, bool reset = true, bool recognizeDelimiter = false)
        {
            if(reset)
            {
                Reset();
            }
            _stringValueCollection.AddRange(DelimitedToValues(delimValues,
                                            recognizeDelimiter ? RecognizeDelimiter(delimValues) : _delimiter
                                            ));
            return _stringValueCollection.Count;
        }

        /// <summary>
        /// Returns the string value from the internal collection having given index
        /// </summary>
        /// <param name="idx">Index</param>
        /// <returns>String value from the internal collection</returns>
        public string GetValue(int idx)
        {
            return _stringValueCollection[idx];
        }

        /// <summary>
        /// The same behaviour as List.IndexOf
        /// </summary>
        public int IndexOf(string value)
        {
            return _stringValueCollection.IndexOf(value);
        }

        /// <summary>
        /// Copies the values from the internal collection to a single string. Values are delimited by the instance delimiter.
        /// </summary>
        /// <returns>Single string containing delimited values</returns>
        public override string ToString()
        {
            return ValuesToDelimited(_stringValueCollection, _delimiter);
        }
  
    }//DelimitedStringValues

}//Namespace

