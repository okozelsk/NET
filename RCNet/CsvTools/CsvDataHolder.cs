using RCNet.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace RCNet.CsvTools
{
    /// <summary>
    /// Provides simple loading and saving of csv data
    /// </summary>
    public class CsvDataHolder
    {
        //Constants
        /// <summary>
        /// Special char code (0) for delimiter auto detection
        /// </summary>
        public const char AutoDetectDelimiter = (char)0;

        //Attribute properties
        /// <summary>
        /// Delimiter of data items
        /// </summary>
        public char DataDelimiter { get; private set; }

        /// <summary>
        /// Column names
        /// </summary>
        public DelimitedStringValues ColNameCollection { get; private set; }

        /// <summary>
        /// Data rows
        /// </summary>
        public List<DelimitedStringValues> DataRowCollection { get; }

        /// <summary>
        /// Instantiates an uninitialized instance.
        /// </summary>
        /// <param name="delimiter">Data items delimiter</param>
        public CsvDataHolder(char delimiter)
        {
            DataDelimiter = delimiter;
            ColNameCollection = new DelimitedStringValues(DataDelimiter);
            DataRowCollection = new List<DelimitedStringValues>();
            return;
        }

        /// <summary>
        /// Loads data and instantiates an initialized instance.
        /// </summary>
        /// <param name="streamReader">Data stream reader</param>
        /// <param name="header">Specifies whether first row contains column names</param>
        /// <param name="delimiter">Data items delimiter. If AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(StreamReader streamReader, bool header, char delimiter = AutoDetectDelimiter)
        {
            DataRowCollection = new List<DelimitedStringValues>();
            DataDelimiter = delimiter;
            AppendFromStream(streamReader);
            if (header && DataRowCollection.Count > 0)
            {
                ColNameCollection = DataRowCollection[0];
                DataRowCollection.RemoveAt(0);
            }
            else
            {
                ColNameCollection = new DelimitedStringValues(DataDelimiter);
            }
            return;
        }

        /// <summary>
        /// Loads data and instantiates an initialized instance.
        /// </summary>
        /// <param name="streamReader">Data stream reader</param>
        /// <param name="delimiter">Data items delimiter. If AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(StreamReader streamReader, char delimiter = AutoDetectDelimiter)
        {
            DataRowCollection = new List<DelimitedStringValues>();
            DataDelimiter = delimiter;
            AppendFromStream(streamReader);
            InitColNames();
            return;
        }

        /// <summary>
        /// Loads data and instantiates an initialized instance.
        /// </summary>
        /// <param name="fileName">Data file</param>
        /// <param name="header">Specifies whether first row contains column names</param>
        /// <param name="delimiter">Data items delimiter. If AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(string fileName, bool header, char delimiter = AutoDetectDelimiter)
            : this(new StreamReader(new FileStream(fileName, FileMode.Open)), header, delimiter)
        {
            return;
        }

        /// <summary>
        /// Loads data and instantiates an initialized instance.
        /// </summary>
        /// <param name="fileName">Data file</param>
        /// <param name="delimiter">Data items delimiter. If AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(string fileName, char delimiter = AutoDetectDelimiter)
            : this(new StreamReader(new FileStream(fileName, FileMode.Open)), delimiter)
        {
            return;
        }

        //Methods
        /// <summary>
        /// Checks if string values contain data items
        /// </summary>
        /// <param name="dsv">String values</param>
        private bool ContainsDataItems(DelimitedStringValues dsv)
        {
            foreach (string item in dsv.StringValueCollection)
            {
                //Numerical
                if (!double.IsNaN(item.ParseDouble(false)))
                {
                    return true;
                }
                else if (item.ParseInt(false) != int.MinValue)
                {
                    return true;
                }
                //Datetime
                else if (item.ParseDateTime(false) != DateTime.MinValue)
                {
                    return true;
                }
                //Boolean
                try
                {
                    item.ParseBool(true, "failed");
                    return true;
                }
                catch
                {
                    //Do nothing
                    ;
                }

            }
            return false;
        }

        /// <summary>
        /// Initializes column names
        /// </summary>
        private void InitColNames()
        {
            if (DataRowCollection.Count == 0)
            {
                //No data
                ColNameCollection = new DelimitedStringValues(DataDelimiter);
            }
            if (ContainsDataItems(DataRowCollection[0]))
            {
                //First row contains data -> Empty column names
                ColNameCollection = new DelimitedStringValues(DataRowCollection[0].Delimiter);
            }
            else
            {
                //First row probably contains column names
                ColNameCollection = DataRowCollection[0];
                DataRowCollection.RemoveAt(0);
            }
            return;
        }

        /// <summary>
        /// Appends data rows from given stream reader
        /// </summary>
        /// <param name="streamReader">Data stream reader</param>
        /// <param name="maxRows">Maximum rows to be loaded. If GT 0 is specified then loading stops when maxRows is reached.</param>
        /// <returns>Number of loaded rows</returns>
        public int AppendFromStream(StreamReader streamReader, int maxRows = 0)
        {
            int numOfLoadedRows = 0;
            while (!streamReader.EndOfStream)
            {
                //Add data row
                if (DataDelimiter == AutoDetectDelimiter)
                {
                    //Unknown delimiter
                    DelimitedStringValues dsv = new DelimitedStringValues(streamReader.ReadLine());
                    //Set recognized delimiter
                    DataDelimiter = dsv.Delimiter;
                    DataRowCollection.Add(dsv);
                }
                else
                {
                    //Known delimiter
                    DataRowCollection.Add(new DelimitedStringValues(streamReader.ReadLine(), DataDelimiter));
                }
                ++numOfLoadedRows;
                if (maxRows > 0 && numOfLoadedRows == maxRows)
                {
                    //Maximim limit reached
                    break;
                }
            }
            return numOfLoadedRows;
        }

        /// <summary>
        /// Sets the new data delimiter for whole data rows and column names
        /// </summary>
        /// <param name="delimiter">Delimiter to be set</param>
        public void SetDataDelimiter(char delimiter)
        {
            DataDelimiter = delimiter;
            ColNameCollection.ChangeDelimiter(DataDelimiter);
            foreach (DelimitedStringValues dsv in DataRowCollection)
            {
                dsv.ChangeDelimiter(DataDelimiter);
            }
            return;
        }

        /// <summary>
        /// Writes content to a specified stream
        /// </summary>
        /// <param name="streamWriter">Target stream</param>
        public void WriteToStream(StreamWriter streamWriter)
        {
            if (ColNameCollection.NumOfStringValues > 0)
            {
                streamWriter.WriteLine(ColNameCollection.ToString());
            }
            foreach (DelimitedStringValues dsv in DataRowCollection)
            {
                streamWriter.WriteLine(dsv.ToString());
            }
            return;
        }

        /// <summary>
        /// Saves content to a specified file
        /// </summary>
        /// <param name="fileName">Target file name</param>
        public void Save(string fileName)
        {
            using (StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create)))
            {
                WriteToStream(streamWriter);
            }
            return;
        }


    }//CsvDataHolder

}//Namespace
