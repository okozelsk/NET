namespace RCNet.Neural.Network.NonRecurrent
{
    /// <summary>
    /// Common interface of the build progress information holders.
    /// </summary>
    public interface IBuildProgress
    {
        /// <summary>
        /// Indicates that the build of a new end-network has started.
        /// </summary>
        bool NewEndNetwork { get; }

        /// <summary>
        /// Indicates important progress information that should be reported.
        /// </summary>
        bool ShouldBeReported { get; }

        /// <summary>
        /// Gets currently processed end network epoch number.
        /// </summary>
        int EndNetworkEpochNum { get; }

        /// <summary>
        /// Gets the textual information about the build progress.
        /// </summary>
        /// <param name="margin">Left margin (number of spaces).</param>
        /// <param name="includeName">Specifies whether to include name of the entity being built in the textual information.</param>
        /// <returns>Built text message.</returns>
        string GetInfoText(int margin = 0, bool includeName = true);

    }//IBuildProgress


}//Namespace
