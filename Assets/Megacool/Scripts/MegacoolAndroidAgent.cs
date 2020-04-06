#if UNITY_ANDROID
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MegacoolAndroidAgent : MegacoolIAgent {

    private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    public bool Debug {
        set {
            Android.CallStatic("setDebug", value);
        }
    }

    private AndroidJavaObject CurrentActivity {
        get {
            AndroidJavaClass jclass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return jclass.GetStatic<AndroidJavaObject>("currentActivity");
        }
    }

    private AndroidJavaObject android;

    private AndroidJavaObject Android {
        get {
            if (android == null) {
                android = new AndroidJavaClass("co.megacool.megacool.Megacool");

                if (Megacool.Debug) {
                    // Ensure debugging is enabled as early as possible
                    Debug = true;
                }
            }
            return android;
        }
    }

    private AndroidJavaObject megacool;

    private void ImplementationWarning(string message) {
        UnityEngine.Debug.LogWarning(message + " is not implemented on Android");
    }

    //*******************  Native Libraries  *******************//
    [DllImport("megacool")]
    private static extern void mcl_set_capture_texture(IntPtr texturePointer);

    [DllImport("megacool")]
    private static extern void mcl_set_source_origin(int origin);

    [DllImport("megacool")]
    private static extern void mcl_set_texture_read_complete_callback([MarshalAs(UnmanagedType.FunctionPtr)] Megacool._TextureReadComplete callbackPointer);

    [DllImport("megacool")]
    private static extern IntPtr mcl_get_unity_render_event_pointer();

    [DllImport("megacool")]
    private static extern int mcl_get_scaled_width();

    [DllImport("megacool")]
    private static extern int mcl_get_scaled_height();

    [DllImport("megacool")]
    private static extern void mcl_set_unscaled_capture_dimensions(int width, int height);

    [DllImport("megacool")]
    private static extern IntPtr mcl_get_preview_for_recording(string recordingId);

    [DllImport("megacool")]
    private static extern IntPtr mcl_get_next_frame_details(IntPtr preview, int origin);

    [DllImport("megacool")]
    private static extern void mcl_free_preview_data(IntPtr preview);

    [DllImport("megacool")]
    private static extern void mcl_free_frame_details(IntPtr frame);

    [DllImport("megacool")]
    private static extern void mcl_memcpy(IntPtr dst, IntPtr src, int length);

    [DllImport("megacool")]
    private static extern int mcl_get_preview_frame_count(IntPtr preview);

    [StructLayout(LayoutKind.Sequential)]
    private struct MCLPreviewFrameDetails {
        public IntPtr pixels; // pixel format is identical to Color32
        public int width;
        public int height;
        public int delay_ms;
    }


    //****************** API Implementation  ******************//
    public void Start(MegacoolInternal.EventHandler eventHandler) {
        AndroidJavaObject jConfig = new AndroidJavaObject("co.megacool.megacool.MegacoolConfig");
        jConfig.Call<AndroidJavaObject>("wrapper", "Unity", Application.unityVersion);
        jConfig.Call<AndroidJavaObject>("baseEventListener", new UnityEventsListener(eventHandler));
        AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
        string baseUrl = MegacoolConfiguration.Instance.CustomBaseUrl();
        if (baseUrl != null) {
            AndroidJavaObject jBaseUrl = uriClass.CallStatic<AndroidJavaObject>("parse", baseUrl);
            jConfig.Call<AndroidJavaObject>("baseUrl", jBaseUrl);
        }

        megacool = Android.CallStatic<AndroidJavaObject>("start", CurrentActivity,
            MegacoolConfiguration.Instance.appConfigAndroid, jConfig);
    }

    public void StartRecording(MegacoolRecordingConfig config) {
        AndroidJavaObject jConfig = RecordingConfigToJavaObject(config);
        megacool.Call("startRecording", null, jConfig);
    }

    public void RegisterScoreChange(int scoreDelta) {
        megacool.Call("registerScoreChange", scoreDelta);
    }

    public void CaptureFrame(MegacoolRecordingConfig config, bool forceAdd) {
        AndroidJavaObject jConfig = RecordingConfigToJavaObject(config);
        megacool.Call("captureFrame", null, jConfig, forceAdd);
    }

    public void SetCaptureMethod(MegacoolCaptureMethod captureMethod, RenderTexture renderTexture) {
        if (captureMethod == MegacoolCaptureMethod.SCREEN){
            mcl_set_capture_texture(IntPtr.Zero);
        } else {
            SignalRenderTexture(renderTexture);
        }
    }

    public void PauseRecording() {
        megacool.Call("pauseRecording");
    }

    public void StopRecording() {
        megacool.Call("stopRecording");
    }

    public void DeleteRecording(string recordingId) {
        megacool.Call("deleteRecording", recordingId);
    }

    public IMegacoolPreviewData GetPreviewDataForRecording(string recordingId) {
        IntPtr preview = mcl_get_preview_for_recording(recordingId);
        if (preview == IntPtr.Zero) {
            return null;
        }

        return new AndroidPreviewData(preview);
    }

    public int GetNumberOfFrames(string recordingId) {
        return megacool.Call<int>("getNumberOfFrames", recordingId);
    }

    public int GetRecordingScore(string recordingId) {
        return megacool.Call<int>("getRecordingScore", recordingId);
    }

    public void GetUserId(Action<string> callback) {
        UserIdListener listener = new UserIdListener(callback);
        megacool.Call("setOnUserIdReceivedListener", listener);
    }

    public void Share(MegacoolShareConfig config) {
        AndroidJavaObject jConfig = ShareConfigToJavaObject(config);
        megacool.Call("share", CurrentActivity, jConfig);
    }

    public void ShareScreenshot(MegacoolRecordingConfig recordingConfig,
            MegacoolShareConfig shareConfig) {
        AndroidJavaObject jRecordingConfig = RecordingConfigToJavaObject(recordingConfig);
        AndroidJavaObject jShareConfig = ShareConfigToJavaObject(shareConfig);
        megacool.Call("shareScreenshot", CurrentActivity, null, jRecordingConfig, jShareConfig);
    }

    public void ShareToMessages(MegacoolShareConfig config) {
        AndroidJavaObject jConfig = ShareConfigToJavaObject(config);
        megacool.Call("shareToMessages", CurrentActivity, jConfig);
    }

    public void ShareToMail(MegacoolShareConfig config) {
        AndroidJavaObject jConfig = ShareConfigToJavaObject(config);
        megacool.Call("shareToMail", CurrentActivity, jConfig);
    }

    public void SetDefaultShareConfig(MegacoolShareConfig config) {
        AndroidJavaObject jConfig = ShareConfigToJavaObject(config);
        megacool.Call("setDefaultShareConfig", jConfig);
    }

    public void SetDefaultRecordingConfig(MegacoolRecordingConfig config) {
        AndroidJavaObject jConfig = RecordingConfigToJavaObject(config);
        megacool.Call("setDefaultRecordingConfig", jConfig);
    }

    private AndroidJavaObject RecordingConfigToJavaObject(MegacoolRecordingConfig config) {
        if (config == null) {
            return null;
        }
        AndroidJavaObject jConfig = new AndroidJavaObject("co.megacool.megacool.RecordingConfig");
        if (config._HasRecordingId()) {
            jConfig.Call<AndroidJavaObject>("id", config.RecordingId);
        }
        if (config._HasMaxFrames()) {
            jConfig.Call<AndroidJavaObject>("maxFrames", config.MaxFrames);
        }
        if (config._HasFrameRate()) {
            jConfig.Call<AndroidJavaObject>("frameRate", config.FrameRate);
        }
        if (config._HasPlaybackFrameRate()) {
            jConfig.Call<AndroidJavaObject>("playbackFrameRate", config.PlaybackFrameRate);
        }
        if (config._HasLastFrameDelay()) {
            jConfig.Call<AndroidJavaObject>("lastFrameDelay", config.LastFrameDelay);
        }
        if (config._HasPeakLocation()) {
            jConfig.Call<AndroidJavaObject>("peakLocation", config.PeakLocation);
        }
        if (config.LastFrameOverlay != null) {
            jConfig.Call<AndroidJavaObject>("lastFrameOverlayAsset", config.LastFrameOverlay);
        }
        if (config._HasOverflowStrategy()) {
            AndroidJavaClass overflowStrategyClass = new AndroidJavaClass("co.megacool.megacool.OverflowStrategy");
            AndroidJavaObject jOverflowStrategy;
            if (config.OverflowStrategy == MegacoolOverflowStrategy.HIGHLIGHT) {
                jOverflowStrategy = overflowStrategyClass.GetStatic<AndroidJavaObject>("HIGHLIGHT");
            } else if (config.OverflowStrategy == MegacoolOverflowStrategy.TIMELAPSE) {
                jOverflowStrategy = overflowStrategyClass.GetStatic<AndroidJavaObject>("TIMELAPSE");
            } else {
                jOverflowStrategy = overflowStrategyClass.GetStatic<AndroidJavaObject>("LATEST");
            }
            jConfig.Call<AndroidJavaObject>("overflowStrategy", jOverflowStrategy);
        }
        return jConfig;
    }

    private AndroidJavaObject ShareConfigToJavaObject(MegacoolShareConfig config) {
        if (config == null) {
            return null;
        }
        AndroidJavaObject jConfig = new AndroidJavaObject("co.megacool.megacool.ShareConfig");
        if (config._HasRecordingId()) {
            jConfig.Call<AndroidJavaObject>("recordingId", config.RecordingId);
        }
        if (config._HasMessage()) {
            jConfig.Call<AndroidJavaObject>("message", config.Message);
        }
        if (config._HasStrategy()) {
            AndroidJavaClass strategyClass = new AndroidJavaClass("co.megacool.megacool.SharingStrategy");
            AndroidJavaObject jStrategy;
            if (config.Strategy == MegacoolSharingStrategy.MEDIA) {
                jStrategy = strategyClass.GetStatic<AndroidJavaObject>("MEDIA");
            } else {
                jStrategy = strategyClass.GetStatic<AndroidJavaObject>("LINK");
            }
            jConfig.Call<AndroidJavaObject>("strategy", jStrategy);
        }
        if (config.FallbackImage != null) {
            jConfig.Call<AndroidJavaObject>("fallbackImageAsset", config.FallbackImage);
        }
        if (config.Url != null) {
            AndroidJavaClass jUriClass = new AndroidJavaClass("android.net.Uri");
            AndroidJavaObject jUri = jUriClass.CallStatic<AndroidJavaObject>("parse", config.Url.ToString());
            jConfig.Call<AndroidJavaObject>("url", jUri);
        }
        if (config.Data != null) {
            jConfig.Call<AndroidJavaObject>("dataAsJson", MegacoolThirdParty_MiniJSON.Json.Serialize(config.Data));
        }
        if (config._HasModalTitle()) {
            jConfig.Call<AndroidJavaObject>("modalTitle", config.ModalTitle);
        }
        return jConfig;
    }

    public void GetShares(Action<List<MegacoolShare>> shares, Func<MegacoolShare, bool> filter = null) {
        megacool.Call<AndroidJavaObject>("getShares", new ShareCallback(shares), filter != null ? new ShareFilter(filter) : null);
    }

    public void SetDebugMode(bool debugMode) {
        Android.CallStatic("setDebug", debugMode);
    }

    public bool GetDebugMode() {
        ImplementationWarning("getDebugMode");
        return false;
    }

    public void SetKeepCompletedRecordings(bool keep) {
        megacool.Call("setKeepCompletedRecordings", keep);
    }

    public void DeleteShares(Func<MegacoolShare, bool> filter) {
        megacool.Call("deleteShares", new ShareFilter(filter));
    }

    public void SubmitDebugData(string message) {
        Android.CallStatic("submitDebugData", message);
    }

    public void ResetIdentity() {
        Android.CallStatic("resetIdentity");
    }

    public void SetGIFColorTable(Megacool.GifColorTableType gifColorTable) {
        AndroidJavaClass jGifColorTableClass = new AndroidJavaClass("co.megacool.megacool.GifColorTable");
        AndroidJavaObject jGifColorTable;
        switch (gifColorTable) {
        case Megacool.GifColorTableType.GifColorTableFixed:
            jGifColorTable = jGifColorTableClass.GetStatic<AndroidJavaObject>("FIXED");
            break;
        default:
            // This covers both dynamic and analyzeFirst, the latter is iOS only but largely equivalent
            jGifColorTable = jGifColorTableClass.GetStatic<AndroidJavaObject>("DYNAMIC");
            break;
        }
        megacool.Call("setGifColorTable", jGifColorTable);
    }

    public void SignalRenderTexture(RenderTexture texture) {
        if (!texture) {
            texture = Megacool.Instance._RenderTexture;
            // this automatically does the signalling
            return;
        }
        mcl_set_capture_texture(texture.GetNativeTexturePtr());
    }

    public void IssuePluginEvent(ref IntPtr nativePluginCallbackPointer, int eventId) {
        if (nativePluginCallbackPointer == IntPtr.Zero) {
            nativePluginCallbackPointer = mcl_get_unity_render_event_pointer();
        }
        GL.IssuePluginEvent(nativePluginCallbackPointer, eventId);
    }

    public void InitializeCapture(double scaleFactor, Megacool._TextureReadComplete textureReadCompleteCallback) {
        AndroidJavaClass captureMethodClass = new AndroidJavaClass("co.megacool.megacool.Megacool$CaptureMethod");
        switch (SystemInfo.graphicsDeviceType) {
            case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                megacool.Call(
                    "setCaptureMethod",
                    captureMethodClass.GetStatic<AndroidJavaObject>("OPENGLES2"),
                    scaleFactor
                );
                break;
            case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                megacool.Call(
                    "setCaptureMethod",
                    captureMethodClass.GetStatic<AndroidJavaObject>("OPENGLES3"),
                    scaleFactor
                );
                break;
            case UnityEngine.Rendering.GraphicsDeviceType.Vulkan:
                UnityEngine.Debug.LogWarning("Megacool: Capturing from Vulkan isn't supported " +
                    "yet, switch to ES3 or ES2 to make capture work");
                return;
            default:
                return;
        }

        // 1 is MCF_BOTTOM_LEFT
        mcl_set_source_origin(1);
        mcl_set_texture_read_complete_callback(textureReadCompleteCallback);
    }

    public int GetScaledWidth() {
        return mcl_get_scaled_width();
    }

    public int GetScaledHeight() {
        return mcl_get_scaled_height();
    }

    public void SetUnscaledCaptureDimensions(int width, int height) {
        mcl_set_unscaled_capture_dimensions(width, height);
    }

    private class AndroidPreviewData : IMegacoolPreviewData {
        IntPtr preview;

        public AndroidPreviewData(IntPtr preview) {
            this.preview = preview;
        }

        public IMegacoolPreviewFrame GetNextFrame() {
            // This value must match the enum definitions in mcl.h
            int MCL_BOTTOM_LEFT = 1;
            IntPtr nativeFrame = mcl_get_next_frame_details(preview, MCL_BOTTOM_LEFT);
            if (nativeFrame == IntPtr.Zero) {
                return null;
            }
            return new AndroidPreviewFrame(nativeFrame);
        }

        public int GetNumberOfFrames() {
            return mcl_get_preview_frame_count(preview);
        }

        public void Release() {
            mcl_free_preview_data(preview);
            preview = IntPtr.Zero;
        }

        ~AndroidPreviewData() {
            Release();
        }
    }


    private class AndroidPreviewFrame : IMegacoolPreviewFrame {
        private IntPtr nativePreview;
        private MCLPreviewFrameDetails frameDetails;

        public AndroidPreviewFrame(IntPtr nativePreview) {
            this.nativePreview = nativePreview;
            frameDetails = (MCLPreviewFrameDetails)Marshal.PtrToStructure(nativePreview,
                typeof(MCLPreviewFrameDetails));
        }

        public int GetDelayMs() {
            return frameDetails.delay_ms;
        }

        public bool LoadToTexture(Texture2D texture) {
            if (nativePreview == IntPtr.Zero) {
                return false;
            }
            if (texture.width != frameDetails.width || texture.height != frameDetails.height ||
                    texture.format != TextureFormat.RGBA32) {
                texture.Resize(frameDetails.width, frameDetails.height, TextureFormat.RGBA32, false);
            }
            byte[] texturePixels = texture.GetRawTextureData();
            GCHandle handle = GCHandle.Alloc(texturePixels, GCHandleType.Pinned);
            mcl_memcpy(handle.AddrOfPinnedObject(), frameDetails.pixels, texturePixels.Length);
            handle.Free();
            texture.LoadRawTextureData(texturePixels);
            texture.Apply();
            return true;
        }

        public void Release() {
            mcl_free_frame_details(nativePreview);
            nativePreview = IntPtr.Zero;
        }

        ~AndroidPreviewFrame() {
            Release();
        }
    }


    public static MegacoolLinkClickedEvent BuildLinkClickedEvent(AndroidJavaObject jEvent) {
        AndroidJavaObject jUrl = jEvent.Call<AndroidJavaObject>("getUrl");
        Uri uri = new Uri(jUrl.Call<string>("toString"), UriKind.Relative);
        AndroidJavaObject jReferralCode = null;
        try {
            jReferralCode = jEvent.Call<AndroidJavaObject>("getReferralCode");
#pragma warning disable RECS0022 // Methods that return null will throw on older versions of Unity
        } catch (Exception) { }
#pragma warning restore RECS0022
        MegacoolReferralCode referralCode = BuildReferralCode(jReferralCode);
        return new MegacoolLinkClickedEvent(uri, referralCode);
    }


    public static MegacoolReceivedShareOpenedEvent BuildReceivedShareOpenedEvent(AndroidJavaObject jEvent) {
        bool isFirstSession = jEvent.Call<bool>("isFirstSession");
        string senderUserId = jEvent.Call<string>("getSenderUserId");
        long createdTime = jEvent.Call<AndroidJavaObject>("getCreatedAt").Call<long>("getTime");
        DateTime createdAt = _epoch.AddMilliseconds(createdTime).ToLocalTime();

        AndroidJavaObject jShare = null;
        try {
            jShare = jEvent.Call<AndroidJavaObject>("getShare");
#pragma warning disable RECS0022 // Methods that return null will throw on older versions of Unity
        } catch (Exception) { }
#pragma warning restore RECS0022
        MegacoolShare share = BuildShare(jShare);
        return new MegacoolReceivedShareOpenedEvent(isFirstSession, senderUserId, share, createdAt);
    }


    public static MegacoolSentShareOpenedEvent BuildSentShareOpenedEvent(AndroidJavaObject jEvent) {
        bool isFirstSession = jEvent.Call<bool>("isFirstSession");
        string receiverUserId = jEvent.Call<string>("getReceiverUserId");
        long createdTime = jEvent.Call<AndroidJavaObject>("getCreatedAt").Call<long>("getTime");
        DateTime createdAt = _epoch.AddMilliseconds(createdTime).ToLocalTime();

        AndroidJavaObject jShare = null;
        try {
            jShare = jEvent.Call<AndroidJavaObject>("getShare");
#pragma warning disable RECS0022 // Methods that return null will throw on older versions of Unity
        } catch (Exception) { }
#pragma warning restore RECS0022
        MegacoolShare share = BuildShare(jShare);
        return new MegacoolSentShareOpenedEvent(isFirstSession, receiverUserId, share, createdAt);
    }


    public static MegacoolReferralCode BuildReferralCode(AndroidJavaObject jReferralCode) {
        if (jReferralCode == null) {
            return null;
        }
        return new MegacoolReferralCode(
            jReferralCode.Call<string>("getUserId"), jReferralCode.Call<string>("getShareId"));
    }


    public static MegacoolShare BuildShare(AndroidJavaObject jShare) {
        if (jShare == null) {
            return null;
        }
        AndroidJavaObject jReferralCode = jShare.Call<AndroidJavaObject>("getReferralCode");
        MegacoolReferralCode referralCode = BuildReferralCode(jReferralCode);

        string state = jShare.Call<AndroidJavaObject>("getState").Call<string>("name");
        MegacoolShare.ShareState shareState = MegacoolShare.ShareState.SENT;
        switch (state) {
            case "OPENED":
                shareState = MegacoolShare.ShareState.OPENED;
                break;
            case "INSTALLED":
                shareState = MegacoolShare.ShareState.INSTALLED;
                break;
            case "CLICKED":
                shareState = MegacoolShare.ShareState.CLICKED;
                break;
        }

        long createdTime = jShare.Call<AndroidJavaObject>("getCreatedAt").Call<long>("getTime");
        DateTime createdAt = _epoch.AddMilliseconds(createdTime).ToLocalTime();

        long updatedTime = jShare.Call<AndroidJavaObject>("getUpdatedAt").Call<long>("getTime");
        DateTime updatedAt = _epoch.AddMilliseconds(updatedTime).ToLocalTime();

        Uri url = new Uri(jShare.Call<AndroidJavaObject>("getUrl").Call<string>("toString"), UriKind.Relative);

        string dataJsonString = jShare.Call<string>("getDataAsJson");
        Dictionary<string, object> data = MegacoolThirdParty_MiniJSON.Json.Deserialize(dataJsonString) as Dictionary<string, object>;
        return new MegacoolShare(referralCode, shareState, createdAt, updatedAt, data, url);
    }
}

class UnityEventsListener : AndroidJavaProxy {
    private MegacoolInternal.EventHandler eventHandler;

    public UnityEventsListener(MegacoolInternal.EventHandler eventHandler) : base("co.megacool.megacool.BaseEventListener") {
        this.eventHandler = eventHandler;
    }


    void linkClicked(AndroidJavaObject jEvent) {
        MegacoolLinkClickedEvent megacoolEvent = MegacoolAndroidAgent.BuildLinkClickedEvent(jEvent);
        eventHandler.LinkClicked(megacoolEvent);
    }


    void receivedShareOpened(AndroidJavaObject jEvent) {
        MegacoolReceivedShareOpenedEvent megacoolEvent = MegacoolAndroidAgent.BuildReceivedShareOpenedEvent(jEvent);
        eventHandler.ReceivedShareOpened(megacoolEvent);
    }


    void sentShareOpened(AndroidJavaObject jEvent) {
        MegacoolSentShareOpenedEvent megacoolEvent = MegacoolAndroidAgent.BuildSentShareOpenedEvent(jEvent);
        eventHandler.SentShareOpened(megacoolEvent);
    }


    void shareCompleted() {
        eventHandler.ShareCompleted();
    }


    void shareDismissed() {
        eventHandler.ShareDismissed();
    }


    void sharePossiblyCompleted() {
        eventHandler.SharePossiblyCompleted();
    }
}


class ShareCallback : AndroidJavaProxy {
    private Action<List<MegacoolShare>> shareHandler;

    public ShareCallback(Action<List<MegacoolShare>> shareHandler) : base("co.megacool.megacool.Megacool$ShareCallback") {
        this.shareHandler = shareHandler;
    }

    void shares(AndroidJavaObject jShares) {
        int size = jShares.Call<int>("size");
        List<MegacoolShare> result = new List<MegacoolShare>(size);
        for (int i = 0; i < size; i++) {
            AndroidJavaObject jShare = jShares.Call<AndroidJavaObject>("get", i);
            MegacoolShare share = MegacoolAndroidAgent.BuildShare(jShare);
            result.Add(share);
        }
        shareHandler(result);
    }
}

class ShareFilter : AndroidJavaProxy {
    private Func<MegacoolShare, bool> filter;

    public ShareFilter(Func<MegacoolShare, bool> filter) : base("co.megacool.megacool.Megacool$ShareFilter") {
        this.filter = filter;
    }

    bool accept(AndroidJavaObject jShare) {
        MegacoolShare share = MegacoolAndroidAgent.BuildShare(jShare);
        return filter(share);
    }
}

class UserIdListener : AndroidJavaProxy {
    private Action<string> listener;

    public UserIdListener(Action<string> callback) : base("co.megacool.megacool.Megacool$OnUserIdReceivedListener") {
        listener = callback;
    }

    void onUserIdReceived(string userId) {
        listener(userId);
    }
}

#endif
