/// <summary>
/// Pass to Megacool.StartRecording to configure how recordings are made.
/// </summary>
public class MegacoolRecordingConfig {

    protected MegacoolOverflowStrategy? overflowStrategy;
    /// <summary>
    /// How to compress longer recordings.
    /// </summary>
    public MegacoolOverflowStrategy OverflowStrategy {
        get {
            return overflowStrategy ?? MegacoolOverflowStrategy.LATEST;
        }
        set {
            overflowStrategy = value;
        }
    }


    private string recordingId;
    /// <summary>
    /// An identifier for the recording.
    /// </summary>
    /// <description>
    /// Set this to enable having multiple recordings in progress at the same time. Pass the same
    /// identifier to <c>MegacoolGifPreview.StartPreview</c> and <c>MegacoolShareConfig</c>later to
    /// preview and share the different recordings.
    /// </description>
    public string RecordingId {
        get {
            return recordingId ?? MegacoolConfiguration.DEFAULT_RECORDING_ID;
        }
        set {
            recordingId = value;
        }
    }


    private int? maxFrames;
    /// <summary>
    /// Set the max number of frames in a recording. If set to <c>1</c> the recording will be shared
    /// as a still image instead of as a GIF. Must be <c>1</c> or larger.
    /// </summary>
    /// <description>
    /// <para>
    /// The default is 50 frames. What happens when a recording grows above the <c>MaxFrames</c>
    /// limit is determined by the overflow strategy.
    /// </para>
    /// <para>
    /// If you set this to <c>1</c> you probably also want to
    /// use <c>MegacoolShareConfig.ModalTitle</c> to change the default modal title so that it
    /// doesn't say a GIF is about to be shared.
    /// </para>
    /// </description>
    public int MaxFrames {
        get {
            return maxFrames ?? MegacoolConfiguration.DEFAULT_MAX_FRAMES;
        }
        set {
            maxFrames = value;
        }
    }


    private double? peakLocation;
    /// <summary>
    /// Set the location of the highest scoring moment (the peak) in a highlight recording. Must be
    /// between <c>0</c> and <c>1</c>, the default is <c>0.7</c>. Set the score throughout the game
    /// with <c>Megacool.RegisterScoreChange()</c>. The recording must be using the highlight
    /// overflow strategy for this to have any effect.
    /// </summary>
    /// <description>
    /// For example, in a recording with 10 frames, a peak location of <c>0.2</c> means that the
    /// highest scoring frame will occur near the beginning at frame 2, and a peak location of
    /// <c>0.8</c> means that the peak will occur near the end at frame 8.
    /// </description>
    public double PeakLocation {
        get {
            return peakLocation ?? MegacoolConfiguration.DEFAULT_PEAK_LOCATION;
        }
        set {
            peakLocation = value;
        }
    }


    private int? frameRate;
    /// <summary>
    /// Set the capture frame rate.
    /// </summary>
    public int FrameRate {
        get {
            return frameRate ?? MegacoolConfiguration.DEFAULT_FRAME_RATE;
        }
        set {
            frameRate = value;
        }
    }


    private int? playbackFrameRate;
    /// <summary>
    /// Set the playback frame rate.
    /// </summary>
    /// <description>
    /// Set this different from FrameRate to speed up or slow down the recording.
    /// </description>
    public int PlaybackFrameRate {
        get {
            return playbackFrameRate ?? MegacoolConfiguration.DEFAULT_PLAYBACK_FRAME_RATE;
        }
        set {
            playbackFrameRate = value;
        }
    }


    private int? lastFrameDelay;
    /// <summary>
    /// Set a custom delay on the last frame, in ms. Defaults to 1000.
    /// </summary>
    public int LastFrameDelay {
        get {
            return lastFrameDelay ?? MegacoolConfiguration.DEFAULT_LAST_FRAME_DELAY;
        }
        set {
            lastFrameDelay = value;
        }
    }


    /// <summary>
    /// Overlay an image over the last frame of the GIF.
    /// </summary>
    /// <remarks>
    /// Default is none. The path should be relative to the StreamingAssets directory.
    /// </remarks>
    public string LastFrameOverlay { get; set; }


    /// <summary>
    /// Configuration options for recording a GIF.
    /// </summary>
    public MegacoolRecordingConfig() {
    }


    public void _LoadDefaults(MegacoolConfiguration config) {
        if (!_HasMaxFrames()) {
            MaxFrames = config.maxFrames;
        }
        if (!_HasFrameRate()) {
            FrameRate = config.recordingFrameRate;
        }
        if (!_HasPeakLocation()) {
            PeakLocation = config.peakLocation;
        }
        if (!_HasLastFrameDelay()) {
            LastFrameDelay = config.lastFrameDelay;
        }
        if (!_HasPlaybackFrameRate()) {
            PlaybackFrameRate = config.playbackFrameRate;
        }
        if (LastFrameOverlay == null && !string.IsNullOrEmpty(config.lastFrameOverlay)) {
            LastFrameOverlay = config.lastFrameOverlay;
        }
    }


    public bool _HasOverflowStrategy() {
        return overflowStrategy != null;
    }


    public bool _HasRecordingId() {
        return recordingId != null;
    }


    public bool _HasMaxFrames() {
        return maxFrames != null;
    }


    public bool _HasPeakLocation() {
        return peakLocation != null;
    }


    public bool _HasFrameRate() {
        return frameRate != null;
    }


    public bool _HasPlaybackFrameRate() {
        return playbackFrameRate != null;
    }


    public bool _HasLastFrameDelay() {
        return lastFrameDelay != null;
    }


    public override string ToString() {
        return string.Format("MegacoolRecordingConfig(RecordingId={0}, OverflowStrategy={1}, " +
            "MaxFrames={2}, FrameRate={3}, PlaybackFrameRate={4}, LastFrameDelay={5}, " +
            "LastFrameOverlay={6}, PeakLocation={7})",
             _HasRecordingId() ? string.Format("\"{0}\"", recordingId) : string.Format("default(\"{0}\")", RecordingId),
             _HasOverflowStrategy() ? overflowStrategy.ToString() : string.Format("default({0})", OverflowStrategy),
             _HasMaxFrames() ? maxFrames.ToString() : string.Format("default({0})", MaxFrames),
             _HasFrameRate() ? frameRate.ToString() : string.Format("default({0})", FrameRate),
             _HasPlaybackFrameRate() ? playbackFrameRate.ToString() : string.Format("default({0})", PlaybackFrameRate),
             _HasLastFrameDelay() ? lastFrameDelay + "ms" : string.Format("default({0}ms)", LastFrameDelay),
             LastFrameOverlay != null ? string.Format("\"{0}\"", LastFrameOverlay) : null,
             _HasPeakLocation() ? peakLocation.ToString() : string.Format("default({0})", PeakLocation));
    }
}
