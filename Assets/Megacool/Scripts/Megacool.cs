using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using UnityEngine;


public enum MegacoolCaptureMethod {
    BLIT,
    SCREEN,
    RENDER,
}


/// <summary>
/// Not all sharing channels support both links and media. The SharingStrategy
/// sets what should be prioritized.
/// </summary>
public enum MegacoolSharingStrategy {
    /// <summary>
    /// Prioritize media.
    /// </summary>
    MEDIA = 0,

    /// <summary>
    /// Prioritize links (this is the default).
    /// </summary>
    LINK,
}


/// <summary>
/// This is the main interface to the Megacool SDK. Call `Start()` as early as possible
/// during application startup.
/// </summary>
public sealed class Megacool {

    /// <summary>
    /// How the colors in the GIF should be computed.
    /// </summary>
    public enum GifColorTableType {
        /// <summary>
        /// A fixed set of colors is used. This is very fast, but sacrifices quality for nuanced colors and gradients.
        /// </summary>
        GifColorTableFixed,

        /// <summary>
        /// Analyze the frames first. This algorithm is largely equivalent to dynamic, but uses a bit more memory.
        /// Which is faster depends on workload.
        /// </summary>
        /// <description>
        /// This is only available on iOS, on Android this is the same as dynamic.
        /// </description>
        GifColorTableAnalyzeFirst,

        /// <summary>
        /// A subset of the frames is analyzed first. This is the default and yields a good balance between quality and
        /// speed.
        /// </summary>
        GifColorTableDynamic,
    }

    private MegacoolManager captureManager = null;

#region Platform Agent
    private MegacoolIAgent _platformAgent;
#endregion

#region Instance

    private static readonly Megacool instance = new Megacool();

    private Megacool() {
#if UNITY_EDITOR
  _platformAgent = new MegacoolEditorAgent();
#elif (UNITY_IPHONE || UNITY_IOS)
  _platformAgent = MegacoolIOSAgent.Instance;
#elif UNITY_ANDROID
  _platformAgent = new MegacoolAndroidAgent();
#else
  _platformAgent = new MegacoolUnsupportedAgent();
#endif
    }

    /// <summary>
    /// Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static Megacool Instance {
        get {
            return instance;
        }
    }

#endregion

#region Delegates

    private delegate void EventHandlerDelegate(IntPtr jsonData, int length);

    /// <summary>
    /// Callback when a user has completed a share. On Android this is only available for API level
    /// 22+.
    /// </summary>
    /// <description>
    /// Please note that this isn't super reliable as the app that is being shared to has to
    /// correctly implement the sharing API provided by the OS for us to detect this correctly.
    /// On Android most apps don't report the outcome of the share at all, leading to
    /// `PossiblyCompletedSharing` being called most of the time. On iOS most apps report
    /// this correctly, but some apps will cause this to be called even if the share was dismissed,
    /// while other apps might cause `DismissedSharing` to be called even if the share was actually
    /// completed.
    /// </description>
    /// <example>
    /// Megacool.Instance.CompletedSharing += () => {
    ///     Debug.Log("User completed sharing");
    /// }
    /// </example>
    public Action CompletedSharing;


    /// <summary>
    /// Callback when a user has aborted (dismissed) a share. On Android this is only available
    /// for API level 22+.
    /// </summary>
    /// <description>
    /// Please note that this isn't super reliable as the app that is being shared to has to
    /// correctly implement the sharing API provided by the OS for us to detect this correctly.
    /// On Android most apps don't report the outcome of the share at all, leading to
    /// `PossiblyCompletedSharing` being called most of the time. On iOS most apps report
    /// this correctly, but some apps will cause this to be called even if the share was completed,
    /// while other apps might cause `CompletedSharing` to be called even if the share was actually
    /// dismissed.
    /// </description>
    /// <example>
    /// Megacool.Instance.DismissedSharing += () => {
    ///     Debug.Log("User dismissed sharing");
    /// }
    /// </example>
    public Action DismissedSharing;


    /// <summary>
    /// Callback when a user either aborted or completed a share, but we can't know which.
    /// </summary>
    /// <description>
    /// This is currently only called on Android, but note that detecting share outcome isn't super
    /// reliable on either platform. On iOS most apps report share outcome correctly.
    /// </description>
    public Action PossiblyCompletedSharing;


    /// <summary>
    /// Callback when a link click was detected. Use this for navigation within the app.
    /// </summary>
    /// <seealso cref="MegacoolLinkClickedEvent"/>
    public Action<MegacoolLinkClickedEvent> LinkClicked;


    /// <summary>
    /// Callback when the user clicks on someone else's share.
    /// </summary>
    /// <seealso cref="MegacoolReceivedShareOpenedEvent"/>
    public Action<MegacoolReceivedShareOpenedEvent> ReceivedShareOpened;


    /// <summary>
    /// Callback when a share sent by the user was clicked on by someone else.
    /// </summary>
    /// <seealso cref="MegacoolSentShareOpenedEvent"/>
    public Action<MegacoolSentShareOpenedEvent> SentShareOpened;

#endregion

#region Properties

    private const int MCRS = 0x6d637273;
    private IntPtr nativePluginCallbackPointer;

    /// <summary>
    /// The default recording config. Will be merged with the config given to CaptureFrame or
    /// StartRecording, if any.
    /// </summary>
    /// <description>
    /// Note that even though the object is mutable changes will NOT be applied without the config
    /// being set again. This also doesn't impact already started recordings.
    /// </description>
    /// <example>
    /// Usage:
    /// @code
    /// Megacool.Instance.DefaultRecordingConfig = new MegacoolRecordingConfig {
    ///     FrameRate = 15,
    ///     MaxFrames = 75,
    /// };
    /// @endcode
    ///
    /// Or, to modify it later:
    /// @code
    /// MegacoolRecordingConfig config = Megacool.Instance.DefaultRecordingConfig;
    /// config.MaxFrames = 150;
    /// Megacool.Instance.DefaultRecordingConfig = config;
    /// @endcode
    /// </example>
    public MegacoolRecordingConfig DefaultRecordingConfig {
        set {
            if (value != null) {
                value._LoadDefaults(MegacoolConfiguration.Instance);
            }
            _platformAgent.SetDefaultRecordingConfig(value);
        }
    }

    /// <summary>
    /// The default share config. Will be merged with the config given to Share, if any.
    /// </summary>
    public MegacoolShareConfig DefaultShareConfig {
        set {
            if (value != null) {
                value._LoadDefaults(MegacoolConfiguration.Instance);
            }
            _platformAgent.SetDefaultShareConfig(value);
        }
    }


    /// <summary>
    /// Set the type of GIF color table to use.
    /// </summary>
    /// <description>
    /// This only has any effect when sharing to apps where .gif gives a better experience than mp4.
    /// Try sharing to email or messages to see the impact of this.
    /// </description>
    /// <value>The gif color table type</value>
    public GifColorTableType GifColorTable {
        set {
            _platformAgent.SetGIFColorTable(value);
        }
    }

    /// <summary>
    /// Turn on / off debug mode. In debug mode calls to the SDK are stored and can be submitted to
    /// the core developers using SubmitDebugData later.
    /// </summary>
    /// <value><c>true</c> if debug mode; otherwise, <c>false</c>.</value>
    public static bool Debug {
        set {
            MegacoolConfiguration.Instance.debugMode = value;
            Instance._platformAgent.SetDebugMode(value);
        }
        get {
            return MegacoolConfiguration.Instance.debugMode;
        }
    }

    /// <summary>
    /// Whether to keep completed recordings around.
    /// </summary>
    /// <description>
    /// The default is false, which means that all completed recordings will be deleted
    /// whenever a new recording is started with either <c>captureFrame</c> or <c>startRecording</c>.
    /// Setting this to <c>true</c> means we will never delete a completed recording, which is what
    /// you want if you want to enable player to browse previous GIFs they've created. A completed
    /// recording will still be overwritten if a new recording is started with the same
    /// <c>recordingId</c>.
    /// </description>
    /// <value><c>true</c> to keep completed recordings; otherwise, <c>false</c>.</value>
    public bool KeepCompletedRecordings {
        set {
            _platformAgent.SetKeepCompletedRecordings(value);
        }
    }


    /// <summary>
    /// The scale factor to use for the encoded media.
    /// </summary>
    /// <description>
    /// The default is 0.5 for screens whose longest side is &lt; 1500 in
    /// length, or 0.25 for anything larger. If the resulting dimensions are
    /// less than 200 for either width or height, then the scale factor is
    /// increased to ensure a minimum of 200 or more in both dimensions. By
    /// passing in a value for ScaleFactor, you override this behavior. It's
    /// important to keep in mind that while a larger scale factor will produce
    /// encoded media with a higher resolution, it will make captures and
    /// encoding slower, and also increase the size of the encoded media, which
    /// will increase both disk and network usage. In any case, we will round up
    /// the scaled dimensions to be divisible by 16, as this is a requirement
    /// for many MP4 encoders.
    /// </description>
    public static double ScaleFactor;

    private RenderTexture renderTexture;

    public RenderTexture _RenderTexture {
        get {
            if (!renderTexture) {
                _platformAgent.SetUnscaledCaptureDimensions(Screen.width, Screen.height);
                int width = _platformAgent.GetScaledWidth();
                int height = _platformAgent.GetScaledHeight();

                renderTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
                renderTexture.filterMode = FilterMode.Point;
            }
            if (!renderTexture.IsCreated()) {
                // The texture can become lost on level reloads, ensure it's recreated
                renderTexture.Create();

                if (CaptureMethod != MegacoolCaptureMethod.SCREEN) {
                    _platformAgent.SignalRenderTexture(renderTexture);
                }
            }

            return renderTexture;
        }
    }

    private MegacoolCaptureMethod captureMethod = MegacoolCaptureMethod.SCREEN;

    /// <summary>
    /// Set how frames should be captured.
    /// </summary>
    public MegacoolCaptureMethod CaptureMethod {
        get {
            // SCREEN is only compatible with OpenGL ES 3 or newer, fall back to blitting
            // if unsupported.
            if (captureMethod == MegacoolCaptureMethod.SCREEN &&
                    SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3) {
                return MegacoolCaptureMethod.BLIT;
            }
            return captureMethod;
        }
        set {
            captureMethod = value;
            if (!hasStarted) {
                // Only communicate changes if it's already set, otherwise it'll be initialized
                // with the correct method.
                return;
            }
            _platformAgent.SetCaptureMethod(CaptureMethod, renderTexture);
        }
    }


    public bool _IsRecording { get; private set; }

    #endregion

    #region Functionality

    private void SetupDefaultConfiguration() {
        DefaultShareConfig = new MegacoolShareConfig();
        DefaultRecordingConfig = new MegacoolRecordingConfig();
    }

    private bool hasStarted = false;

    /// <summary>
    /// Initialize the SDK.
    /// </summary>
    /// <remarks>
    /// To listen for events for the SDK, make sure you register the delegates for
    /// <c>Megacool.Instance.LinkClicked</c>/<c>Megacool.Instance.OnReceivedShareOpened</c> and
    /// similar *before* calling this.
    /// </remarks>
    public void Start() {
        // Create a main thread action for every asynchronous callback
        MegacoolInternal.EventHandler eventHandler = new MegacoolInternal.EventHandler(this);

        if (hasStarted) {
            // Allowing multiple initializations would make it hard to maintain both thread-safety and performance
            // of the underlying capture code, and doesn't have any good use case for allowing it, thus ignoring.
            UnityEngine.Debug.Log("Megacool: Skipping duplicate init");
            return;
        }
        hasStarted = true;

        // Set debugging first so that it can be enabled before initializing the native SDK
        Debug = MegacoolConfiguration.Instance.debugMode;

        _platformAgent.Start(eventHandler);

        SetupDefaultConfiguration();

        _platformAgent.InitializeCapture(ScaleFactor, _TextureReadCompleteCallback);
        _IssuePluginEvent(MCRS);
        _platformAgent.SignalRenderTexture(renderTexture);
    }

    public void _IssuePluginEvent(int eventId) {
        _platformAgent.IssuePluginEvent(ref nativePluginCallbackPointer, eventId);
    }

    /// <summary>
    /// Start recording a GIF
    /// </summary>
    /// <remarks>
    /// This will keep a buffer of 50 frames (default). The frames are overwritten until <c>StopRecording</c> gets called.
    /// </remarks>
    public void StartRecording() {
        StartRecording(null);
    }

    private void SafeReleaseTextureReady () {
        // Release the TextureReady without throwing if already at max capacity.
        try {
            _TextureReady.Release();
#pragma warning disable 0168
        } catch (SemaphoreFullException e) {
#pragma warning restore 0168
            // Ignore
        }
    }

    /// <summary>
    /// Start customized GIF recording.
    /// </summary>
    /// <remarks>
    /// This will keep a buffer of 50 frames (default). The frames are overwritten until <c>StopRecording</c> gets called.
    /// </remarks>
    /// <param name="config">Config to customize the recording.</param>
    public void StartRecording(MegacoolRecordingConfig config) {
        captureManager = GetManager();
        if (!captureManager) {
            return;
        }
        captureManager.StartWrites(GetTimeBetweenCaptures(config));
        _platformAgent.StartRecording(config);
        _IsRecording = true;
        SafeReleaseTextureReady();
    }

    private double GetTimeBetweenCaptures(MegacoolRecordingConfig config) {
        if (config != null && config._HasFrameRate()) {
            return 1.0 / config.FrameRate;
        }
        return 1.0 / MegacoolConfiguration.Instance.recordingFrameRate;
    }

    private MegacoolManager GetManager() {
        MegacoolManager manager = null;
        foreach (Camera cam in Camera.allCameras) {
            MegacoolManager foundManager = cam.GetComponent<MegacoolManager>();
            if (foundManager) {
                manager = foundManager;
                break;
            }
        }
        if (!manager) {
            Camera mainCamera = Camera.main;
            if (!mainCamera) {
                UnityEngine.Debug.Log("No MegacoolManager already in the scene and no main camera to attach to, " +
                    "either attach it manually to a camera or tag one of the cameras as MainCamera");
                return null;
            }
            mainCamera.gameObject.AddComponent<MegacoolManager>();
            manager = mainCamera.GetComponent<MegacoolManager>();
        }

        return manager;
    }

    /// <summary>
    /// Note an event for highlight recording.
    /// </summary>
    /// <remarks>
    /// For highlight recording use only. Call this function when something interesting occurs, like a point is scored
    /// or a coin collected or the player hits an opponent. The section of the recording with the highest amount of
    /// calls to this function will be what is present in the final recording, with the peak located at located at
    /// Megacool.PeakLocation.
    /// </remarks>
    public void RegisterScoreChange() {
        RegisterScoreChange(1);
    }

    /// <summary>
    /// Note a change in score for highlight recording
    /// </summary>
    /// <remarks>
    /// For highlight recording use only. Call this function when something interesting occurs, like a point is scored
    /// or a coin collected or the player hits an opponent. The section of the recording with the highest absolute sum
    /// of deltas sent to this function will be what is present in the final recording, with the peak located at located
    /// at Megacool.PeakLocation.
    /// </remarks>
    public void RegisterScoreChange(int scoreDelta) {
        _platformAgent.RegisterScoreChange(scoreDelta);
    }

    // Indicates whether this frame should be rendered. Used by the custom cameras to detect when CaptureFrame
    // has been called.
    public bool _RenderThisFrame = false;

    // Protects access to the render texture. The blit/render cameras wait for this before writing to the texture, and
    // the native library posts to it once a read is finished.
    public Semaphore _TextureReady = new Semaphore(1, 1);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate void _TextureReadComplete ();

    [MonoPInvokeCallback(typeof(_TextureReadComplete))]
    private static void _TextureReadCompleteCallback () {
        Megacool.Instance.SafeReleaseTextureReady();
    }


    /// <summary>
    /// Capture a single frame.
    /// </summary>
    public void CaptureFrame() {
        CaptureFrame(null, false);
    }

    /// <summary>
    /// Capture a single frame.
    /// </summary>
    /// <description>
    /// If the recording doesn't already exist it'll be created with the settings specified in the
    /// config.
    /// </description>
    /// <param name="config">the configuration to use to create the recording, if it doesn't already
    /// exist.</param>
    public void CaptureFrame(MegacoolRecordingConfig config) {
        CaptureFrame(config, false);
    }

    /// <summary>
    /// Capture a single frame.
    /// </summary>
    /// <description>
    /// If the recording doesn't already exist it'll be created with the settings specified in the
    /// config.
    /// </description>
    /// <param name="config">the configuration to use to create the recording, if it doesn't already
    /// exist.</param>
    /// <param name="forceAdd">Set to true to ensure the frame is included in the recording, even if
    /// the overflow strategy otherwise would skip it. Useful for timelapse to include the last
    /// frame with a score board or final state.</param>
    public void CaptureFrame(MegacoolRecordingConfig config, bool forceAdd) {
        captureManager = GetManager();
        if (!captureManager) {
            return;
        }
        captureManager.StartWrites(0);
        _RenderThisFrame = true;
        _platformAgent.CaptureFrame(config, forceAdd);
    }


    /// <summary>
    /// Pauses the recording.
    /// </summary>
    /// <description>
    /// This does nothing if there's no recording currently in progress.
    /// </description>
    public void PauseRecording() {
        if (captureManager) {
            captureManager.StopWrites();
        }
        _IsRecording = false;
        _platformAgent.PauseRecording();
    }

    /// <summary>
    /// Stops the recording. Calling CaptureFrame or StartRecording after this will cause a new recording to be started.
    /// </summary>
    /// <description>
    /// If KeepCompletedRecordings is set to true (default is false), the recording will still be available on disk and
    /// can be shared and/or previewed later, and you have to manually call DeleteRecording when you want to clear it
    /// from disk. With the default setting it'll be deleted automatically when a new recording is started.
    /// </description>
    public void StopRecording() {
        if (captureManager) {
            captureManager.StopWrites();
        }
        _IsRecording = false;
        _platformAgent.StopRecording();
    }

    /// <summary>
    /// Delete a recording
    /// </summary>
    /// <description>
    /// Will remove any frames of the recording in memory and on disk. Both completed and incomplete
    /// recordings will take space on disk, thus particularly if you're using <c>KeepCompletedRecordings = true</c> you might want
    /// to provide an interface to your users for removing recordings they don't care about anymore to free up space for new recordings.
    /// </description>
    /// <param name="recordingId">Recording identifier.</param>
    public void DeleteRecording(string recordingId) {
        _platformAgent.DeleteRecording(recordingId);
    }

    public IMegacoolPreviewData _GetPreviewDataForRecording(string recordingId) {
        return _platformAgent.GetPreviewDataForRecording(recordingId);
    }

    /// <summary>
    /// Gets the number of frames available in a given recording.
    /// </summary>
    /// <description>
    /// If you're sanity checking a preview you should call MegacoolGifPreview.GetNumberOfFrames()
    /// after calling MegacoolGifPreview.StartPreview() instead as it's less racy, to get the count
    /// for other uses you can use this method.
    /// </description>
    /// <returns>The number of frames, or -1 if the recording doesn't exist</returns>
    /// <param name="recordingId">Which recording to get the frame count of</param>
    public int GetNumberOfFrames(string recordingId) {
        return _platformAgent.GetNumberOfFrames(recordingId);
    }


    /// <summary>
    ///  Get the total score for the given recording.
    /// </summary>
    /// <description>
    /// <para>
    /// By observing this value you can learn what scores are average and which are good in your
    /// game, and use this to only prompt the user to share if it was a high-scoring recording, or
    /// promote high-scoring recordings in the game or use it to set the share text.
    /// </para>
    /// <para>
    /// The score will be <c>0</c> if the recording doesn't use the highlight overflow strategy, or
    /// if <c>RegisterScoreChange</c> has never been called.
    /// </para>
    /// </description>
    /// <returns>
    /// The score for the given recording, or <c>-1</c> if the recording couldn't be found.
    /// </returns>
    /// <param name="recordingId">The recording to fetch the score for. Fetches the default if
    /// <c>null</c>.</param>
    public int GetRecordingScore(string recordingId) {
        return _platformAgent.GetRecordingScore(recordingId);
    }


    /// <summary>
    /// Get the user identifier. If already known the callback is called immediately, otherwise it's
    /// called once we communicate with the backend and learn the id. After that it's stored locally.
    /// </summary>
    public void GetUserId(Action<string> callback) {
        _platformAgent.GetUserId(callback);
    }


    /// <summary>
    /// Get the state of shares sent.
    /// </summary>
    /// <description>
    /// Use this if a user is wondering whether someone has clicked, installed or been re-engaged from the shares sent.
    ///
    /// This will also cause the SDK to check for new events, so you might receive MegacoolSentShareOpened events after
    /// calling this.
    /// </description>
    /// <param name="shares">Callback to receive the updated shares</param>
    public void GetShares(Action<List<MegacoolShare>> shares) {
        _platformAgent.GetShares(shares);
    }


    /// <summary>
    /// Deletes local shares.
    /// </summary>
    /// <description>
    /// Use this to clear old shares from local storage. The filter will be passed each share available locally, return
    /// true for the given share to be deleted.
    /// </description>
    /// <param name="filter">Filter.</param>
    public void DeleteShares(Func<MegacoolShare, bool> filter) {
        _platformAgent.DeleteShares(filter);
    }

    /// <summary>
    /// Share the default recording.
    /// </summary>
    public void Share() {
        _platformAgent.Share(null);
    }

    /// <summary>
    /// Share a recording according to the config.
    /// </summary>
    /// <param name="config">Config.</param>
    public void Share(MegacoolShareConfig config) {
        _platformAgent.Share(config);
    }


    /// <summary>
    /// Take a screenshot and share it immediately.
    /// </summary>
    /// <description>
    /// <para>
    /// This is a helper around {@link #captureFrame(View)} and {@link #share(Activity)} when you
    /// only need to share a screenshot and not all the other bells and whistles for recordings.
    /// </para>
    /// <para>
    /// This method is functionally equivalent to:
    /// <code>
    /// Megacool.Instance.PauseRecording();
    /// string tempRecording = "random-unused-id";
    /// Megacool.Instance.CaptureFrame(new MegacoolRecordingConfig {
    ///     RecordingId = tempRecordingId,
    ///     MaxFrames = 1,
    /// });
    /// Megacool.Instance.Share(new MegacoolShareConfig {
    ///     RecordingId = tempRecordingId,
    /// });
    /// Megacool.Instance.DeleteRecording(tempRecordingId);
    /// </code>
    /// Note that if this method is called while a recording is underway the screenshot is likely to
    /// be missing from the share. To stay on the safe side, leave a couple hundred ms between
    /// stopping a recording and sharing a screenshot.
    /// </para>
    /// </description>
    /// <param name="recordingConfig">The recording config, or <c>null</c>. Most properties don't
    /// apply to screenshots, but the last frame overlay does.</param>
    /// <param name="shareConfig">The share config, or <c>null</c>.</param>
    public void ShareScreenshot(MegacoolRecordingConfig recordingConfig = null,
            MegacoolShareConfig shareConfig = null) {
        captureManager = GetManager();
        if (captureManager) {
            // Only try to capture if we have a manager, but always call down to the SDK to make
            // sure the share happens anyway
            captureManager.StartWrites(0);
            _RenderThisFrame = true;
        }
        _platformAgent.ShareScreenshot(recordingConfig, shareConfig);
    }

    /// <summary>
    /// Share directly to SMS.
    /// </summary>
    public void ShareToMessages() {
        _platformAgent.ShareToMessages(null);
    }


    /// <summary>
    /// Share directly to SMS with custom config.
    /// </summary>
    /// <param name="config">Config.</param>
    public void ShareToMessages(MegacoolShareConfig config) {
        _platformAgent.ShareToMessages(config);
    }


    /// <summary>
    /// Share directly to email with custom config.
    /// </summary>
    public void ShareToMail() {
        _platformAgent.ShareToMail(null);
    }


    /// <summary>
    /// Share directly to email with custom config.
    /// </summary>
    /// <param name="config">Config.</param>
    public void ShareToMail(MegacoolShareConfig config) {
        _platformAgent.ShareToMail(config);
    }


    /// <summary>
    /// Submit debug data from the SDK to the developers to assist in fixing bugs.
    /// </summary>
    /// <description>
    /// If something in the SDK is not behaving as expected, set Debug=true as early as possible (preferably before
    /// Start()), after the problem has been observed call this method with a descriptive message. The developers will
    /// then receive logs and other debugging information from the device to assist in debugging.
    /// </description>
    /// <param name="message">Brief summary of what you expected to happen and what happened</param>
    public void SubmitDebugData(string message) {
        _platformAgent.SubmitDebugData(message);
    }


    /// <summary>
    /// Resets the device identity.
    /// </summary>
    /// <description>
    /// This is a test or debugging tool to make the current device appear as if it's a new device, making it possible
    /// to test referrals and link clicks from a "new" device.
    /// </description>
    /// <remarks>
    /// Must be called before Start().
    /// </remarks>
    public void ResetIdentity() {
        _platformAgent.ResetIdentity();
    }


#endregion
}
