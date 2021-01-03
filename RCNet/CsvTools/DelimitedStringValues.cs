using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RCNet.CsvTools
{
    /// <summary>
    /// Implements the single row of the delimited string values (csv format).
    /// </summary>
    public class DelimitedStringValues
    {
        //Constants
        //Delimiters
        /// <summary>
        /// The semicolon delimiter.
        /// </summary>
        public const char SemicolonDelimiter = ';';
        /// <summary>
        /// The comma delimiter.
        /// </summary>
        public const char CommaDelimiter = ',';
        /// <summary>
        /// The tabelator delimiter.
        /// </summary>
        public const char TabDelimiter = '\t';
        /// <summary>
        /// The default delimiter.
        /// </summary>
        public const char DefaultDelimiter = SemicolonDelimiter;

        //Attribute properties
        /// <summary>
        /// The current delimiter.
        /// </summary>
        public char Delimiter { get; private set; }
        /// <summary>
        /// The collection of the string values.
        /// </summary>
        public List<string> StringValueCollection { get; }

        //Constructor
        /// <summary>
        /// Creates an empty instance.
        /// </summary>
        /// <param name="delimiter">The delimiter of the string values.</param>
        public DelimitedStringValues(char delimiter = DefaultDelimiter)
        {
            Delimiter = delimiter;
            StringValueCollection = new List<string>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="data">The string consisting of the delimited values.</param>
        /// <param name="delimiter">The delimiter of the string values.</param>
        public DelimitedStringValues(string data, char delimiter)
        {
            Delimiter = delimiter;
            StringValueCollection = new List<string>();
            LoadFromString(data, false, false);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="data">The string consisting of the delimited values.</param>
        public DelimitedStringValues(string data)
        {
            StringValueCollection = new List<string>();
            LoadFromString(data, false, true);
            return;
        }

        //Properties
        /// <summary>
        /// Number of stored string values.
        /// </summary>
        public int NumOfStringValues { get { return StringValueCollection.Count; } }

        //Methods
        //Static methods
        /// <summary>
        /// Tries to recognize a delimiter used in the sample data.
        /// </summary>
        /// <param name="sampleDelimitedData">The sample data row.</param>
        /// <returns>The recognized delimiter or the default delimiter.</returns>
        public static char RecognizeDelimiter(string sampleDelimitedData)
        {
            //Check of the presence of candidate chars
            //Is "tab" char the candidate?
            if (sampleDelimitedData.IndexOf(TabDelimiter) != -1)
            {
                //If tab is present then it is the most probable delimiter
                return TabDelimiter;
            }
            //Is "semicolon" char the candidate?
            if (sampleDelimitedData.IndexOf(SemicolonDelimiter) != -1)
            {
                //If semicolon is present then it is the next most probable delimiter
                return SemicolonDelimiter;
            }
            //Recognize a floating point char
            char floatingPointChar = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];
            if (sampleDelimitedData.IndexOf('.') != -1)
            {
                int index = sampleDelimitedData.IndexOf('.');
                if (index > 0 && index < sampleDelimitedData.Length - 1)
                {
                    char charBefore = sampleDelimitedData[index - 1];
                    if (charBefore >= '0' && charBefore <= '9')
                    {
                        char charAfter = sampleDelimitedData[index + 1];
                        if (charAfter >= '0' && charAfter <= '9')
                        {
                            floatingPointChar = '.';
                        }
                    }
                }
            }
            //Is "comma" char the candidate?
            if (sampleDelimitedData.IndexOf(CommaDelimiter) != -1 && floatingPointChar != CommaDelimiter)
            {
                //Comma is the probable delimiter
                return CommaDelimiter;
            }
            else
            {
                //Remaining default delimiter
                return DefaultDelimiter;
            }
        }

        /// <summary>
        /// Builds the single string consisting of the string values delimited by specified delimiter.
        /// </summary>
        /// <param name="stringValueCollection">The collection of alone string values.</param>
        /// <param name="delimiter">The delimiter to be used.</param>
        /// <returns>The built single string.</returns>
        public static string ToString(IEnumerable<string> stringValueCollection, char delimiter = DefaultDelimiter)
        {
            StringBuilder output = new StringBuilder();
            bool firstVal = true;
            foreach (string value in stringValueCollection)
            {
                if (!firstVal)
                {
                    output.Append(delimiter);
                }
                output.Append(value);
                firstVal = false;
            }
            return output.ToString();
        }

        /// <summary>
        /// Splits the string consisting of the delimited values.
        /// </summary>
        /// <param name="stringRow">The string consisting of the delimited values.</param>
        /// <param name="delimiter">The used delimiter of the values.</param>
        /// <returns>List of alone string values.</returns>
        public static List<string> ToList(string stringRow, char delimiter = DefaultDelimiter)
        {
            List<string> values = new List<string>();
            if (stringRow.Length > 0)
            {
                char[] allowedDelims = new char[1];
                allowedDelims[0] = delimiter;
                values.AddRange(stringRow.Split(allowedDelims, StringSplitOptions.None));
            }
            return values;
        }

        //Instance methods
        /// <summary>
        /// Clears the internal collection of the string values.
        /// </summary>
        public void Reset()
        {
            StringValueCollection.Clear();
            return;
        }

        /// <summary>
        /// Changes the delimiter.
        /// </summary>
        /// <param name="delimiter">The new delimiter.</param>
        public void ChangeDelimiter(char delimiter)
        {
            Delimiter = delimiter;
            return;
        }

        /// <summary>
        /// Adds the next string value into the internal collection of string values.
        /// </summary>
        /// <param name="value">The value to be added.</param>
        /// <returns>The number of string values within the internal collection after the operation.</returns>
        public int AddValue(string value)
        {
            StringValueCollection.Add(value);
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Removes a string value at the specified position from the internal collection.
        /// </summary>
        /// <param name="index">The zero-based index of a string value to be removed.</param>
        /// <returns>The number of string values within the internal collection after the operation.</returns>
        public int RemoveAt(int index)
        {
            StringValueCollection.RemoveAt(index);
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Removes the trailing empty or white spaced string values from the inner collection.
        /// </summary>
        /// <returns>The number of string values within the internal collection after the operation.</returns>
        public int RemoveTrailingWhites()
        {
            while (StringValueCollection.Count > 0 && StringValueCollection[StringValueCollection.Count - 1].Trim() == string.Empty)
            {
                StringValueCollection.RemoveAt(StringValueCollection.Count - 1);
            }
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Loads the delimited string values into the inner collection.
        /// </summary>
        /// <param name="stringRow">The string consisting of the delimited values.</param>
        /// <param name="reset">Specifies whether to clear the internal collection before the load.</param>
        /// <param name="recognizeDelimiter">When false then instance delimiter to be used. When true then method tries to recognize a proper delimiter from the data.</param>
        /// <returns>The number of string values within the internal collection after the operation.</returns>
        public int LoadFromString(string stringRow, bool reset = true, bool recognizeDelimiter = false)
        {
            if (reset)
            {
                Reset();
            }
            Delimiter = recognizeDelimiter ? RecognizeDelimiter(stringRow) : Delimiter;
            StringValueCollection.AddRange(ToList(stringRow, Delimiter));
            return StringValueCollection.Count;
        }

        /// <summary>
        /// Gets the string value from the internal collection at the specified zero-based index.
        /// </summary>
        /// <param name="idx">The zero-based index of the value.</param>
        /// <returns>A string value from the internal collection.</returns>
        public string GetValueAt(int idx)
        {
            return StringValueCollection[idx];
        }

        ///<inheritdoc cref="List{T}.IndexOf(T)"/>
        public int IndexOf(string item)
        {
            return StringValueCollection.IndexOf(item);
        }

        ///<inheritdoc/>
        public override string ToString()
        {
            return ToString(StringValueCollection, Delimiter);
        }

    }//DelimitedStringValues

}//Namespace

