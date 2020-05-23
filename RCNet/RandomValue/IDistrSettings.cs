namespace RCNet.RandomValue
{
    /// <summary>
    /// Technical interface for random distributions
    /// </summary>
    public interface IDistrSettings
    {
        /// <summary>
        /// Type of random distribution
        /// </summary>
        RandomCommon.DistributionType Type { get; }

    }//IDistrSettings
}
