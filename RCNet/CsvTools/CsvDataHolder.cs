using RCNet.Extensions;
using System;
using System.Collections.Generic;
using System.IO;

namespace RCNet.CsvTools
{
    /// <summary>
    /// Implements the simple loading and saving of csv data.
    /// </summary>
    public class CsvDataHolder
    {
        //Constants
        /// <summary>
        /// Special char code (0) to specify data delimiter auto detection requirement.
        /// </summary>
        public const char AutoDetectDelimiter = (char)0;

        //Attribute properties
        /// <summary>
        /// The delimiter of the data items.
        /// </summary>
        public char DataDelimiter { get; private set; }

        /// <summary>
        /// Column names.
        /// </summary>
        public DelimitedStringValues ColNameCollection { get; private set; }

        /// <summary>
        /// Data rows.
        /// </summary>
        public List<DelimitedStringValues> DataRowCollection { get; private set; }

        /// <summary>
        /// Creates an uninitialized instance.
        /// </summary>
        /// <param name="delimiter">The delimiter of the data items.</param>
        public CsvDataHolder(char delimiter)
        {
            DataDelimiter = delimiter;
            ColNameCollection = new DelimitedStringValues(DataDelimiter);
            DataRowCollection = new List<DelimitedStringValues>();
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="streamReader">Data stream reader.</param>
        /// <param name="header">Specifies whether the first row contains the column names.</param>
        /// <param name="delimiter">The delimiter of the data items. If CsvDataHolder.AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(StreamReader streamReader, bool header, char delimiter = AutoDetectDelimiter)
        {
            InitFromStream(streamReader, header, delimiter);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="streamReader">Data stream reader.</param>
        /// <param name="delimiter">The delimiter of the data items. If CsvDataHolder.AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(StreamReader streamReader, char delimiter = AutoDetectDelimiter)
        {
            InitFromStream(streamReader, delimiter);
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fileName">Name of the file containing the data to be loaded.</param>
        /// <param name="header">Specifies whether the first row contains the column names.</param>
        /// <param name="delimiter">The delimiter of the data items. If CsvDataHolder.AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(string fileName, bool header, char delimiter = AutoDetectDelimiter)
        {
            using (StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                InitFromStream(streamReader, header, delimiter);
            }
            return;
        }

        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="fileName">Name of the file containing the data to be loaded.</param>
        /// <param name="delimiter">The delimiter of the data items. If CsvDataHolder.AutoDetectDelimiter is specified than delimiter will be recognized automatically.</param>
        public CsvDataHolder(string fileName, char delimiter = AutoDetectDelimiter)
        {
            var dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            fileName = Path.Combine(dir, fileName);
            using (StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                InitFromStream(streamReader, delimiter);
            }
            return;
        }

        //Methods
        private void InitFromStream(StreamReader streamReader, bool header, char delimiter = AutoDetectDelimiter)
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

        private void InitFromStream(StreamReader streamReader, char delimiter = AutoDetectDelimiter)
        {
            DataRowCollection = new List<DelimitedStringValues>();
            DataDelimiter = delimiter;
            AppendFromStream(streamReader);
            InitColNames();
            return;
        }

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
        /// Appends data rows from given stream reader.
        /// </summary>
        /// <param name="streamReader">Data stream reader.</param>
        /// <param name="maxRows">Maximum rows to be loaded. If GT 0 is specified then loading stops when maxRows is reached.</param>
        /// <returns>Number of loaded rows.</returns>
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
        /// Changes the delimiter of data items. The delimiter is changed for the whole content.
        /// </summary>
        /// <param name="delimiter">The new delimiter to be set.</param>
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
        /// Writes the whole content into the specified stream.
        /// </summary>
        /// <param name="streamWriter">The stream to be writing in.</param>
        public void Write(StreamWriter streamWriter)
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
        /// Saves the whole content into the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to be writing in.</param>
        public void Save(string fileName)
        {
            using (StreamWriter streamWriter = new StreamWriter(new FileStream(fileName, FileMode.Create)))
            {
                Write(streamWriter);
            }
            return;
        }

    }//CsvDataHolder

}//Namespace
