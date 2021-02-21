using RCNet.MathTools;
using RCNet.Neural.Data;
using RCNet.Neural.Data.Filter;
using System;
using System.Collections.Generic;
using System.Text;

namespace Demo.DemoConsoleApp.Examples.NonRecurrent
{
    /// <summary>
    /// Implements a set of routines common to non-recurrent networks related code examples in which it is necessary to standardize data.
    /// </summary>
    public class NonRecurrentExampleBase : ExampleBase
    {
        //Constructor
        protected NonRecurrentExampleBase()
            :base()
        {
            return;
        }

        //Methods
        /// <summary>
        /// Prepares a set of filters for the input data standardization.
        /// </summary>
        /// <param name="bundle">A bundle of the input and output data vectors.</param>
        protected FeatureFilterBase[] PrepareInputFeatureFilters(VectorBundle bundle)
        {
            //Allocation of the feature filters
            FeatureFilterBase[] inputFeatureFilters = new FeatureFilterBase[bundle.InputVectorCollection[0].Length];
            for (int i = 0; i < inputFeatureFilters.Length; i++)
            {
                //Input data is always considered as the real numbers.
                inputFeatureFilters[i] = new RealFeatureFilter(Interval.IntN1P1, //Data to be standardized between -1 and 1
                                                               new RealFeatureFilterSettings(true, //We want to standardize data
                                                                                             true //We want to keep a range reserve for unseen data
                                                                                             )
                                                               );
            }
            //Update feature filters
            foreach (double[] vector in bundle.InputVectorCollection)
            {
                for (int i = 0; i < vector.Length; i++)
                {
                    //Update filter by the next known data value.
                    inputFeatureFilters[i].Update(vector[i]);
                }
            }
            return inputFeatureFilters;
        }

        /// <summary>
        /// Standardizes the vector data using specified set of filters.
        /// </summary>
        /// <param name="vector">The data vector to be standardized.</param>
        /// <param name="filters">A set of filters to be used.</param>
        protected void StandardizeVectorData(double[] vector, FeatureFilterBase[] filters)
        {
            for (int i = 0; i < filters.Length; i++)
            {
                //Apply filter on data value.
                vector[i] = filters[i].ApplyFilter(vector[i]);
            }
            return;
        }

        /// <summary>
        /// Standardizes input vectors data of given vector bundle using specified set of filters.
        /// </summary>
        /// <param name="bundle">A bundle of the input and output data vectors.</param>
        /// <param name="filters">A set of filters to be used.</param>
        protected void StandardizeInputVectors(VectorBundle bundle, FeatureFilterBase[] filters)
        {
            foreach (double[] vector in bundle.InputVectorCollection)
            {
                //Standardize all data values in the vector.
                StandardizeVectorData(vector, filters);
            }
            return;
        }



    }//NonRecurrentExampleBase
}
