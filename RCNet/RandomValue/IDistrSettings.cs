namespace RCNet.RandomValue
{
    /// <summary>
    /// Common interface of the random distribution configurations.
    /// </summary>
    public interface IDistrSettings
    {
        /// <inheritdoc cref="RandomCommon.DistributionType" />
        RandomCommon.DistributionType Type { get; }

    }//IDistrSettings
}
