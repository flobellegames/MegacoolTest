/// <summary>
/// How long recordings are compressed to fit within the target maxFrames.
/// </summary>
public enum MegacoolOverflowStrategy {
    // NOTE: Enum values here must match what the native SDKs expect.

    /// <summary>
    /// Only keep the last maxFrames frames in the recording. This is the default.
    /// </summary>
    LATEST = 0,

    /// <summary>
    /// Speed up the recording to create a timelapse.
    /// </summary>
    TIMELAPSE,

    /// <summary>
    /// Extract the highlight in the recording based on calls to Megacool.RegisterScoreChange and
    /// MegacoolConfiguration.PeakLocation.
    /// </summary>
    HIGHLIGHT,
}
