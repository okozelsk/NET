using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OKOSW.CSVTools
{
    public class DelimitedStringValues
    {
        //Constants
        public const char CSV_DELIMITER = ';';

        //Attributes
        private char[] m_delimiter;
        private List<string> m_values;

        //Constructor
        public DelimitedStringValues(char delimiter = CSV_DELIMITER)
        {
            m_delimiter = new char[1];
            m_delimiter[0] = delimiter;
            m_values = new List<string>();
            return;
        }

        //Properties
        public int ValuesCount { get { return m_values.Count; } }
        public List<string> Values { get { return m_values; } }

        //Methods
        public void Reset()
        {
            m_values.Clear();
            return;
        }

        public int AddValue(string value)
        {
            m_values.Add(value);
            return m_values.Count;
        }

        public void LoadFromString(string delimData, bool reset = true)
        {
            if(reset)
            {
                Reset();
            }
            if (delimData.Length > 0)
            {
                m_values.AddRange(delimData.Split(m_delimiter, StringSplitOptions.None));
            }
            return;
        }

        public string GetValue(int idx)
        {
            return m_values[idx];
        }

        public int FindValueIndex(string value)
        {
            for(int i = 0; i < m_values.Count; i++)
            {
                if(m_values[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            for (int i = 0; i < m_values.Count; i++)
            {
                output.Append(m_values[i]);
                if (i < m_values.Count - 1)
                {
                    output.Append(m_delimiter);
                }
            }
            return output.ToString();
        }

    }//DelimitedStringValues

}//Namespace
