namespace RCNet.RandomValue
{
    /// <summary>
    /// Common interface of random distributions configurations
    /// </summary>
    public interface IDistrSettings
    {
        /// <inheritdoc cref="RandomCommon.DistributionType" />
        RandomCommon.DistributionType Type { get; }

    }//IDistrSettings
}
