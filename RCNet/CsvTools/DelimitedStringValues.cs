using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading;

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
        /// The tabelator
        /// </summary>
        public const char TabDelimiter = '\t';
        /// <summary>
        /// The default delimiter
        /// </summary>
        public const char DefaultDelimiter = SemicolonDelimiter;

        //Attribute properties
        /// <summary>
        /// Current delimiter of the string values
        /// </summary>
        public char Delimiter { get; private set; }
        /// <summary>
        /// Collection of string values
        /// </summary>
        public List<string> StringValueCollection { get; }

        //Constructor
        /// <summary>
        /// Creates an empty instance
        /// </summary>
        /// <param name="delimiter">The delimiter of the string values</param>
        public DelimitedStringValues(char delimiter = DefaultDelimiter)
        {
            Delimiter = delimiter;
            StringValueCollection = new List<string>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="data">String containing delimited string values</param>
        /// <param name="delimiter">The delimiter of the string values</param>
        public DelimitedStringValues(string data, char delimiter)
        {
            Delimiter = delimiter;
            StringValueCollection = new List<string>();
            LoadFromString(data, false, false);
            return;
        }

        /// <summary>
        /// Creates an initialized instance
        /// </summary>
        /// <param name="data">String containing delimited string values</param>
        public DelimitedStringValues(string data)
        {
            StringValueCollection = new List<string>();
            LoadFromString(data, false, true);
            return;
        }

        //Properties
        /// <summary>
        /// Number of string values
        /// </summary>
        public int NumOfStringValues { get { return StringValueCollection.Count; } }

        //Methods
        //Static methods
        /// <summary>
        /// Tries to recognize used delimiter
        /// </summary>
        /// <param name="sampleDelimitedData">Sample delimited data</param>
        /// <returns>Found or default delimiter</returns>
        public static char RecognizeDelimiter(string sampleDelimitedData)
        {
            //Apperance of candidate chars
            //Is tab a candidate?
            if(sampleDelimitedData.IndexOf(TabDelimiter) != -1)
            {
                //If tab is persent it is the most probable delimiter
                return TabDelimiter;
            }
            //Is semicolon a candidate?
            if (sampleDelimitedData.IndexOf(SemicolonDelimiter) != -1)
            {
                //If semicolon is persent it is the next most probable delimiter
                return SemicolonDelimiter;
            }
            //Decide floating point char
            char floatingPointChar = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            if (sampleDelimitedData.IndexOf('.') != -1)
            {
                int index = sampleDelimitedData.IndexOf('.');
                if(index > 0 && index < sampleDelimitedData.Length - 1)
                {
                    char charBefore = sampleDelimitedData[index - 1];
                    if(charBefore >= '0' && charBefore <= '9')
                    {
                        char charAfter = sampleDelimitedData[index + 1];
                        if (charAfter >= '0' && charAfter <= '9')
                        {
                            floatingPointChar = '.';
                        }
                    }
                }
            }
            //Is comma a candidate?
            bool comma = (sampleDelimitedData.IndexOf(CommaDelimiter) != -1 && floatingPointChar != CommaDelimiter);
            //Remaining decision
            if(comma)
            {
                //Comma is probable candidate
                return CommaDelimiter;
            }
            else
            {
                //Return default
                return DefaultDelimiter;
            }
        }

        /// <summary>
        /// Converts collection of string values to a single string where values are delimited by the given delimiter.
        /// </summary>
        /// <param name="stringValueCollection">Collection of string values</param>
        /// <param name="delimiter">Values delimiter to be used</param>
        /// <returns>Built string containing delimited values</returns>
        public static string ToString(IEnumerable<string> stringValueCollection, char delimiter = DefaultDelimiter)
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
        /// Splits the given string containing delimited values to the list of string values
        /// </summary>
        /// <param name="delimValues">String containing delimited values</param>
        /// <param name="delimiter">Delimiter of the values</param>
        /// <returns>List of the string values</returns>
        public static List<string> ToList(string delimValues, char delimiter = DefaultDelimiter)
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
            StringValueCollection.Clear();
            return;
        }

        /// <summary>
        /// Changes the delimiter
        /// </summary>
        /// <param name="delimiter">New delimiter</param>
        public void ChangeDelimiter(char delimiter)
        {
            Delimiter = delimiter;
            return;
        }

        /// <summary>
        /// Adds string value into the internal collection of string values
        /// </summary>
        /// <param name="value">Value to be added</param>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int AddValue(string value)
        {
            StringValueCollection.Add(value);
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Removes string value at specified position from the internal collection of string values
        /// </summary>
        /// <param name="index">The zero-based index of value to be removed</param>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int RemoveAt(int index)
        {
            StringValueCollection.RemoveAt(index);
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Removes trailing empty or white spaced string values
        /// </summary>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int RemoveTrailingWhites()
        {
            while(StringValueCollection.Count > 0 && StringValueCollection[StringValueCollection.Count - 1].Trim() == string.Empty)
            {
                StringValueCollection.RemoveAt(StringValueCollection.Count - 1);
            }
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Loads string values into the internal collection from a string containing delimited values
        /// </summary>
        /// <param name="delimValues">String containing delimited values</param>
        /// <param name="reset">Indicates if to clear the internal collection before the load</param>
        /// <param name="recognizeDelimiter">If false then the instance current delimiter will be used. If true then delimiter will be tried to recognized from the given data.</param>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int LoadFromString(string delimValues, bool reset = true, bool recognizeDelimiter = false)
        {
            if(reset)
            {
                Reset();
            }
            Delimiter = recognizeDelimiter ? RecognizeDelimiter(delimValues) : Delimiter;
            StringValueCollection.AddRange(ToList(delimValues, Delimiter));
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Loads string values into the internal collection from a string containing delimited values
        /// </summary>
        /// <param name="values">IEnumerable containing double values</param>
        /// <param name="reset">Indicates if to clear the internal collection before the load</param>
        /// <returns>Number of string values in the internal collection after the operation</returns>
        public int LoadFromDoubleEnumerable(IEnumerable<double> values, bool reset = true)
        {
            if (reset)
            {
                Reset();
            }
            foreach(double value in values)
            {
                StringValueCollection.Add(value.ToString());
            }
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Returns the string value from the internal collection having given index
        /// </summary>
        /// <param name="idx">Index</param>
        /// <returns>String value from the internal collection</returns>
        public string GetValue(int idx)
        {
            return StringValueCollection[idx];
        }

        /// <summary>
        /// The same behaviour as List.IndexOf
        /// </summary>
        public int IndexOf(string value)
        {
            return StringValueCollection.IndexOf(value);
        }

        /// <summary>
        /// Copies the values from the internal collection to a single string. Values are delimited by the instance delimiter.
        /// </summary>
        /// <returns>Single string containing delimited values</returns>
        public override string ToString()
        {
            return ToString(StringValueCollection, Delimiter);
        }
  
    }//DelimitedStringValues

}//Namespace

