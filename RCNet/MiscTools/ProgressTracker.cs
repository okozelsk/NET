using System;
using System.Globalization;

namespace RCNet.MiscTools
{
    /// <summary>
    /// Implements simple progress tracking.
    /// </summary>
    [Serializable]
    public class ProgressTracker
    {
        //Attribute properties
        /// <summary>
        /// The number of current step.
        /// </summary>
        public uint Current { get; private set; }

        /// <summary>
        /// The target number of steps.
        /// </summary>
        public uint Target { get; private set; }

        //Constructor
        /// <summary>
        /// Creates an initialized instance.
        /// </summary>
        /// <param name="target">The target number of steps.</param>
        /// <param name="current">The current number of steps.</param>
        public ProgressTracker(uint target, uint current = 0)
        {
            Target = target;
            SetCurrent(current);
            return;
        }

        //Properties
        /// <summary>
        /// Indicates whether the current step is the last step.
        /// </summary>
        public bool Last { get { return Current == Target; } }

        /// <summary>
        /// Gets the current step to target steps ratio.
        /// </summary>
        public double Ratio { get { return (double)Current / Target; } }

        //Methods
        /// <summary>
        /// Adjusts the number of current step.
        /// </summary>
        /// <param name="increment">An increment.</param>
        public void IncCurrent(int increment)
        {
            int current = (int)Current;
            current += increment;
            current = Math.Max(0, Math.Min(current, (int)Target));
            Current = (uint)current;
            return;
        }

        /// <summary>
        /// Sets the number of current step.
        /// </summary>
        /// <param name="newCurrent">New number of current step.</param>
        public void SetCurrent(uint newCurrent)
        {
            Current = Math.Max(0, Math.Min(newCurrent, Target));
            return;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Current.ToString(CultureInfo.InvariantCulture)}/{Target.ToString(CultureInfo.InvariantCulture)}";
        }

        /// <summary>
        /// Gets current step to target steps ratio in percentage as a double number.
        /// </summary>
        /// <param name="decPlaces">Number of decimal places.</param>
        public double GetPercentageNum(int decPlaces = 0)
        {
            return Math.Round(Ratio * 100d, decPlaces);

        }

        /// <summary>
        /// Gets current step to target steps ratio in percentage as a string.
        /// </summary>
        /// <param name="decPlaces">Number of decimal places.</param>
        public string GetPercentageStr(int decPlaces = 0)
        {
            return $"{GetPercentageNum(decPlaces).ToString($"F{decPlaces}", CultureInfo.InvariantCulture)}%";
        }


    }//ProgressTracker

}//Namespace
