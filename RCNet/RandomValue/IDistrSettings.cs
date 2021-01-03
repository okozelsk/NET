namespace RCNet.RandomValue
{
    /// <summary>
    /// The common interface of random distribution configurations.
    /// </summary>
    public interface IDistrSettings
    {
        /// <inheritdoc cref="RandomCommon.DistributionType" />
        RandomCommon.DistributionType Type { get; }

    }//IDistrSettings
}
