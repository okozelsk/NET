using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.CsvTools;

namespace RCNet.Neural.Data
{
    /// <summary>
    /// The class allows to upload patterned sample data from a csv file.
    /// </summary>
    public static class PatternDataLoader
    {
        /// <summary>
        /// Loads the data and prepares PatternBundle.
        /// 1st row of the file must start with the #RepetitiveGroupOfAttributes keyword followed by
        /// attribute names.
        /// 2nd row of the file must start with the #Outputs keyword followed by
        /// output field names.
        /// 3rd+ rows are the data rows.
        /// The data row must begin with at least one set of values for defined repetitive attributes.
        /// The data row must end with a value for each defined output.
        /// </summary>
        /// <param name="fileName">
        /// Data file name
        /// </param>
        /// <param name="inputFieldNameCollection">
        /// Input fields
        /// </param>
        /// <param name="outputFieldNameCollection">
        /// Output fields
        /// </param>
        /// <param name="outputFieldTaskCollection">
        /// Output field tasks
        /// </param>
        /// <param name="normRange">
        /// Range of normalized values
        /// </param>
        /// <param name="normReserveRatio">
        /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="dataStandardization">
        /// Specifies whether to apply data standardization.
        /// </param>
        /// <param name="bundleNormalizer">
        /// Returned initialized instance of BundleNormalizer.
        /// </param>
        public static PatternBundle Load(string fileName,
                                         List<string> inputFieldNameCollection,
                                         List<string> outputFieldNameCollection,
                                         List<CommonEnums.TaskType> outputFieldTaskCollection,
                                         Interval normRange,
                                         double normReserveRatio,
                                         bool dataStandardization,
                                         out BundleNormalizer bundleNormalizer
                                         )
        {
            PatternBundle bundle = new PatternBundle();
            bundleNormalizer = new BundleNormalizer(normRange);
            using (StreamReader streamReader = new StreamReader(new FileStream(fileName, FileMode.Open)))
            {
                //The first row contains the "#RepetitiveGroupOfAttributes" keyword followed by name(s) of attribute(s)
                string delimitedRepetitiveGroupOfAttributes = streamReader.ReadLine();
                if(!delimitedRepetitiveGroupOfAttributes.StartsWith("#RepetitiveGroupOfAttributes"))
                {
                    throw new FormatException("1st row of the file doesn't start with the #RepetitiveGroupOfAttributes keyword.");
                }
                //What data delimiter is used?
                char csvDelimiter = DelimitedStringValues.RecognizeDelimiter(delimitedRepetitiveGroupOfAttributes);
                //Split column names
                DelimitedStringValues repetitiveGroupOfAttributes = new DelimitedStringValues(csvDelimiter);
                repetitiveGroupOfAttributes.LoadFromString(delimitedRepetitiveGroupOfAttributes);
                repetitiveGroupOfAttributes.RemoveTrailingWhites();
                //Check if the recognized data delimiter works properly
                if (repetitiveGroupOfAttributes.NumOfStringValues < 2)
                {
                    throw new FormatException("The value delimiter was not recognized or missing repetitive attribute(s) name(s).");
                }
                //Remove the #RepetitiveGroupOfAttributes keyword from the collection
                repetitiveGroupOfAttributes.RemoveAt(0);
                //Check if attribute names match with the input fields collection
                if(repetitiveGroupOfAttributes.NumOfStringValues != inputFieldNameCollection.Count)
                {
                    throw new FormatException("Different number of attributes in the file and number of specified input fields.");
                }
                foreach(string inputFieldName in inputFieldNameCollection)
                {
                    if(repetitiveGroupOfAttributes.IndexOf(inputFieldName) < 0)
                    {
                        throw new FormatException($"Input field name {inputFieldName} was not found among the repetitive attributes specified in the file.");
                    }
                }
                //The second row contains the "#Outputs" keyword followed by name(s) of output class(es) or values(s)
                string delimitedOutputNames = streamReader.ReadLine();
                if (!delimitedOutputNames.StartsWith("#Outputs"))
                {
                    throw new FormatException("2nd row of the file doesn't start with the #Outputs keyword.");
                }
                DelimitedStringValues outputNames = new DelimitedStringValues(csvDelimiter);
                outputNames.LoadFromString(delimitedOutputNames);
                outputNames.RemoveTrailingWhites();
                //Check if the there is at least one output name
                if (outputNames.NumOfStringValues < 2)
                {
                    throw new FormatException("Missing output name(es).");
                }
                //Remove the #Outputs keyword from the collection
                outputNames.RemoveAt(0);
                //Check if output names match with the output fields collection
                if (outputNames.NumOfStringValues != outputFieldNameCollection.Count)
                {
                    throw new FormatException("Different number of outputs in the file and number of specified output fields.");
                }
                foreach (string outputFieldName in outputFieldNameCollection)
                {
                    if (outputNames.IndexOf(outputFieldName) < 0)
                    {
                        throw new FormatException($"Output field name {outputFieldName} was not found among the outputs specified in the file.");
                    }
                }
                //Bundle normalizer setup
                foreach (string fieldName in inputFieldNameCollection)
                {
                    bundleNormalizer.DefineField(fieldName, fieldName, normReserveRatio, dataStandardization);
                    bundleNormalizer.DefineInputField(fieldName);
                }
                for(int i = 0; i < outputFieldNameCollection.Count; i++)
                {
                    bundleNormalizer.DefineField(outputFieldNameCollection[i],
                                                 outputFieldNameCollection[i],
                                                 outputFieldTaskCollection[i] == CommonEnums.TaskType.Classification ? 0 : normReserveRatio,
                                                 outputFieldTaskCollection[i] == CommonEnums.TaskType.Classification ? false : dataStandardization
                                                 );
                    bundleNormalizer.DefineOutputField(outputFieldNameCollection[i]);
                }
                bundleNormalizer.FinalizeStructure();
                //Load data
                DelimitedStringValues dataRow = new DelimitedStringValues(csvDelimiter);
                while (!streamReader.EndOfStream)
                {
                    dataRow.LoadFromString(streamReader.ReadLine());
                    dataRow.RemoveTrailingWhites();
                    //Check data length
                    if (dataRow.NumOfStringValues < repetitiveGroupOfAttributes.NumOfStringValues + outputNames.NumOfStringValues ||
                       ((dataRow.NumOfStringValues - outputNames.NumOfStringValues) % repetitiveGroupOfAttributes.NumOfStringValues)!= 0)
                    {
                        throw new FormatException("Incorrect length of data row.");
                    }
                    //Pattern data
                    List<double[]> patternData = new List<double[]>();
                    for(int grpIdx = 0; grpIdx < (dataRow.NumOfStringValues - outputNames.NumOfStringValues) / repetitiveGroupOfAttributes.NumOfStringValues; grpIdx++)
                    {
                        double[] inputVector = new double[repetitiveGroupOfAttributes.NumOfStringValues];
                        for(int attrIdx = 0; attrIdx < repetitiveGroupOfAttributes.NumOfStringValues; attrIdx++)
                        {
                            inputVector[attrIdx] = dataRow.GetValue(grpIdx * repetitiveGroupOfAttributes.NumOfStringValues + attrIdx).ParseDouble(true, "Can't parse double data value.");
                        }//attrIdx
                        patternData.Add(inputVector);
                    }//grpIdx
                    //Output data
                    double[] outputVector = new double[outputNames.NumOfStringValues];
                    for(int outputIdx = (dataRow.NumOfStringValues - outputNames.NumOfStringValues), i = 0; outputIdx < dataRow.NumOfStringValues; outputIdx++, i++)
                    {
                        outputVector[i] = dataRow.GetValue(outputIdx).ParseDouble(true, $"Can't parse double value {dataRow.GetValue(outputIdx)}.");
                    }//outputIdx
                    bundle.AddPair(patternData, outputVector);
                }//while !EOF
            }//using streamReader
            //Data normalization
            bundleNormalizer.Normalize(bundle);
            return bundle;
        }//Load

    }//PatternDataLoader

}//Namespace
