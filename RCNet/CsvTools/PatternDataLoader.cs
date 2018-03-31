using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RCNet.MathTools;
using RCNet.Extensions;
using RCNet.Neural.Network.Data;

namespace RCNet.CsvTools
{
    /// <summary>
    /// The class allows to upload sample data for a Categorization task from an .csv file.
    /// </summary>
    public static class PatternDataLoader
    {
        /// <summary>
        /// Loads the data and prepares PatternVectorPairBundle.
        /// 1st row of the file must start with the #RepetitiveGroupOfAttributes keyword followed by
        /// attribute names.
        /// 2nd row of the file must start with the #CategorizationClasses keyword followed by
        /// class names.
        /// 3rd+ rows are the data rows.
        /// The data row must begin with at least one set of values for defined repetitive attributes.
        /// Value groups may be more in sequence.
        /// The data row must end with a value for each defined categorization class.
        /// </summary>
        /// <param name="fileName">
        /// Data file name
        /// </param>
        /// <param name="normRange">
        /// Range of normalized values
        /// </param>
        /// <param name="normReserveRatio">
        /// Reserve held by a normalizer to cover cases where future data exceeds a known range of sample data.
        /// </param>
        /// <param name="inputDataStandardization">
        /// Specifies whether to apply data standardization to input data.
        /// Output data is never standardized.
        /// </param>
        /// <param name="bundleNormalizer">
        /// Returned initialized instance of BundleNormalizer.
        /// </param>
        public static PatternVectorPairBundle Load(string fileName,
                                                   Interval normRange,
                                                   double normReserveRatio,
                                                   bool inputDataStandardization,
                                                   out BundleNormalizer bundleNormalizer
                                                   )
        {
            PatternVectorPairBundle bundle = new PatternVectorPairBundle();
            bundleNormalizer = new BundleNormalizer(normRange, normReserveRatio, inputDataStandardization, false);
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
                //The second row contains the "#CategorizationClasses" keyword followed by name(s) of categorization class(es)
                string delimitedClassNames = streamReader.ReadLine();
                if (!delimitedClassNames.StartsWith("#CategorizationClasses"))
                {
                    throw new FormatException("2nd row of the file doesn't start with the #CategorizationClasses keyword.");
                }
                DelimitedStringValues classNames = new DelimitedStringValues(csvDelimiter);
                classNames.LoadFromString(delimitedClassNames);
                classNames.RemoveTrailingWhites();
                //Check if the there is at least one categorization class name
                if (classNames.NumOfStringValues < 2)
                {
                    throw new FormatException("Missing categorization class(es) name(es).");
                }
                //Remove the #CategorizationClasses keyword from the collection
                classNames.RemoveAt(0);
                //Bundle handler setup
                foreach (string attrName in repetitiveGroupOfAttributes.StringValueCollection)
                {
                    bundleNormalizer.DefineField(attrName, attrName);
                    bundleNormalizer.DefineInputField(attrName);
                }
                foreach (string className in classNames.StringValueCollection)
                {
                    bundleNormalizer.DefineField(className, className);
                    bundleNormalizer.DefineOutputField(className);
                }
                bundleNormalizer.FinalizeStructure();
                //Load data
                DelimitedStringValues dataRow = new DelimitedStringValues(csvDelimiter);
                while (!streamReader.EndOfStream)
                {
                    dataRow.LoadFromString(streamReader.ReadLine());
                    dataRow.RemoveTrailingWhites();
                    //Check data length
                    if (dataRow.NumOfStringValues < repetitiveGroupOfAttributes.NumOfStringValues + classNames.NumOfStringValues ||
                       ((dataRow.NumOfStringValues - classNames.NumOfStringValues) % repetitiveGroupOfAttributes.NumOfStringValues)!= 0)
                    {
                        throw new FormatException("Incorrect length of data row.");
                    }
                    //Pattern data
                    List<double[]> patternData = new List<double[]>();
                    for(int grpIdx = 0; grpIdx < (dataRow.NumOfStringValues - classNames.NumOfStringValues) / repetitiveGroupOfAttributes.NumOfStringValues; grpIdx++)
                    {
                        double[] inputVector = new double[repetitiveGroupOfAttributes.NumOfStringValues];
                        for(int attrIdx = 0; attrIdx < repetitiveGroupOfAttributes.NumOfStringValues; attrIdx++)
                        {
                            inputVector[attrIdx] = dataRow.GetValue(grpIdx * repetitiveGroupOfAttributes.NumOfStringValues + attrIdx).ParseDouble(true, "Can't parse double data value.");
                        }//attrIdx
                        patternData.Add(inputVector);
                    }//grpIdx
                    //Classification classes data
                    double[] outputVector = new double[classNames.NumOfStringValues];
                    for(int classIdx = (dataRow.NumOfStringValues - classNames.NumOfStringValues), i = 0; classIdx < dataRow.NumOfStringValues; classIdx++, i++)
                    {
                        outputVector[i] = dataRow.GetValue(classIdx).ParseDouble(true, $"Can't parse double value {dataRow.GetValue(classIdx)}.");
                    }//classIdx
                    bundle.AddPair(patternData, outputVector);
                }//while !EOF
            }//using streamReader
            //Data normalization
            bundleNormalizer.Normalize(bundle);
            return bundle;
        }//Load

    }//PatternDataLoader

}//Namespace
