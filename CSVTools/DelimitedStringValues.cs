using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKOSW.CSVTools
{
    /// <summary>
    /// Helper class for CSV parsing/generating operations.
    /// </summary>
    public class DelimitedStringValues
    {
        //Constants
        public const char SemicolonDelimiter = ';';
        public const char CommaDelimiter = ',';
        public const char DefaultDelimiter = SemicolonDelimiter;

        //Attributes
        private char _delimiter;
        private List<string> _values;

        //Constructor
        public DelimitedStringValues(char delimiter = DefaultDelimiter)
        {
            _delimiter = delimiter;
            _values = new List<string>();
            return;
        }

        //Properties
        public char Delimiter { get { return _delimiter; } }
        public int ValuesCount { get { return _values.Count; } }
        public List<string> Values { get { return _values; } }

        //Methods
        //Static methods
        /// <summary>
        /// Tries to recognize delimiter
        /// </summary>
        /// <param name="data">Sample delimited data</param>
        /// <returns>Found or default delimiter</returns>
        public static char RecognizeDelimiter(string data)
        {
            if (data.IndexOf(SemicolonDelimiter) != -1)
            {
                return SemicolonDelimiter;
            }
            if (data.IndexOf(CommaDelimiter) != -1)
            {
                return CommaDelimiter;
            }
            return DefaultDelimiter;
        }

        /// <summary>
        /// Converts collection of string values to string where values are delimited by given delimiter.
        /// </summary>
        /// <param name="values">Collection of values</param>
        /// <param name="delimiter">Desired values delimiter</param>
        /// <returns>Built string containing delimited values</returns>
        public static string ValuesToDelimited(IEnumerable<string> values, char delimiter = DefaultDelimiter)
        {
            StringBuilder output = new StringBuilder();
            bool firstVal = true;
            foreach(string value in values)
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
        /// Splits given string containing delimited values to the list of values
        /// </summary>
        /// <param name="delimValues">String containing delimited values</param>
        /// <param name="delimiter">Delimiter used in the delimValues string</param>
        /// <returns>Built list of values</returns>
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
        /// Cleares internal list of values
        /// </summary>
        public void Reset()
        {
            _values.Clear();
            return;
        }

        /// <summary>
        /// Changes delimiter
        /// </summary>
        /// <param name="delimiter">New delimiter</param>
        public void ChangeDelimiter(char delimiter)
        {
            _delimiter = delimiter;
            return;
        }

        /// <summary>
        /// Adds string value to internal list of values
        /// </summary>
        /// <param name="value">Value to be added</param>
        /// <returns>Count of values after operation</returns>
        public int AddValue(string value)
        {
            _values.Add(value);
            return _values.Count;
        }

        /// <summary>
        /// Loads new values from string containing delimited values
        /// </summary>
        /// <param name="delimValues">String containing delimited values</param>
        /// <param name="reset">Indicates if to clear internal values before operation</param>
        /// <param name="recognizeDelimiter">If false (default) then instance delimiter is used. If true then delimiter will be tried to recognize before split </param>
        /// <returns>Count of values after operation</returns>
        public int LoadFromString(string delimValues, bool reset = true, bool recognizeDelimiter = false)
        {
            if(reset)
            {
                Reset();
            }
            _values.AddRange(DelimitedToValues(delimValues,
                                                recognizeDelimiter ? RecognizeDelimiter(delimValues) : _delimiter
                                                ));
            return _values.Count;
        }

        /// <summary>
        /// Returns value corresponding with given index
        /// </summary>
        /// <param name="idx">Index</param>
        /// <returns>Corresponding string value</returns>
        public string GetValue(int idx)
        {
            return _values[idx];
        }

        /// <summary>
        /// The same behaviour as List.IndexOf
        /// </summary>
        public int IndexOf(string value)
        {
            return _values.IndexOf(value);
        }

        /// <summary>
        /// Converts internal list of string values to the string where values are delimited by internally used delimiter.
        /// </summary>
        /// <returns>Built string containing delimited values</returns>
        public override string ToString()
        {
            return ValuesToDelimited(_values, _delimiter);
        }

    }//DelimitedStringValues

}//Namespace
