#if UNITY_IPHONE || UNITY_IOS
using AOT;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

using Json = MegacoolThirdParty_MiniJSON.Json;

public class MegacoolIOSAgent : MegacoolIAgent {

    [StructLayout(LayoutKind.Sequential)]
    private struct MCLPreviewFrameDetails {
        public IntPtr pixels; // pixel format is identical to Color32
        public int width;
        public int height;
        public int delay_ms;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MCLReferralCode {
        public string user_id;
        public string share_id;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MCLShare {
        public MCLReferralCode referral_code;
        public MegacoolShare.ShareState state;
        public double created_at;
        public double updated_at;
        public string data_json;
        public string url;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MCLLinkClickedEvent {
        public string url;
        public MCLReferralCode referral_code;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MCLReceivedShareOpenedEvent {
        public string sender_user_id;
        [MarshalAs(UnmanagedType.U1)]
        public bool is_first_session;
        public double created_at;
        public MCLShare share;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct MCLSentShareOpenedEvent {
        public string receiver_user_id;
        [MarshalAs(UnmanagedType.U1)]
        public bool is_first_session;
        public double created_at;
        public MCLShare share;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct MCLRect {
        public float x;
        public float y;
        public float width;
        public float height;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct MCLShareConfig {
        public string recordingId;
        public string message;
        public string fallback_image;
        public string url;
        public string jsonData;
        public int strategy;
        public MCLRect popover_source_rect;
        public int popover_permitted_arrow_directions;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct MCLRecordingConfig {
        public string recordingId;
        public int overflowStrategy;
        public int maxFrames;
        public int frameRate;
        public int playbackFrameRate;
        public int lastFrameDelay;
        public double peakLocation;
        public string lastFrameOverlay;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct MCLConfig {
        public string wrapper;
        public string wrapper_version;
        public string base_url;
        public IntPtr link_clicked_handler;
        public IntPtr received_share_opened_handler;
        public IntPtr sent_share_opened_handler;
        public IntPtr share_completed_handler;
        public IntPtr share_dismissed_handler;
    }


    [DllImport("__Internal")]
    private static extern void mcl_set_capture_method_with_scale_factor(double scaleFactor, int captureMethod);

    [DllImport("__Internal")]
    private static extern void mcl_set_capture_texture(IntPtr texturePointer);

    [DllImport("__Internal")]
    private static extern IntPtr mcl_get_unity_render_event_pointer();

    [DllImport("__Internal")]
    private static extern int mcl_get_scaled_width();

    [DllImport("__Internal")]
    private static extern int mcl_get_scaled_height();

    [DllImport("__Internal")]
    private static extern void mcl_set_unscaled_capture_dimensions(int width, int height);

    [DllImport("__Internal")]
    private static extern void mcl_set_texture_read_complete_callback([MarshalAs(UnmanagedType.FunctionPtr)] Megacool._TextureReadComplete callbackPointer);

    [DllImport("__Internal")]
    private static extern void mcl_start_with_app_config(string app_config, IntPtr config);

    [DllImport("__Internal")]
    private static extern void mcl_start_recording(IntPtr config);

    [DllImport("__Internal")]
    private static extern void mcl_register_score_change(int scoreDelta);

    [DllImport("__Internal")]
    private static extern void mcl_capture_frame(IntPtr config, [MarshalAs(UnmanagedType.U1)] bool forceAdd);

    [DllImport("__Internal")]
    private static extern void mcl_pause_recording();

    [DllImport("__Internal")]
    private static extern void mcl_stop_recording();

    [DllImport("__Internal")]
    private static extern void mcl_delete_recording(string recordingId);

    [DllImport("__Internal")]
    private static extern void mcl_delete_shares(IntPtr filter);

    [DllImport("__Internal")]
    private static extern IntPtr mcl_get_preview_for_recording(string recordingId);

    [DllImport("__Internal")]
    private static extern int mcl_get_preview_frame_count(IntPtr previewData);

    [DllImport("__Internal")]
    private static extern void mcl_free_frame_details(IntPtr frame);

    [DllImport("__Internal")]
    private static extern void mcl_free_preview_data(IntPtr previewData);

    [DllImport("__Internal")]
    private static extern IntPtr mcl_get_next_frame_details(IntPtr previewData, int origin);

    [DllImport("__Internal")]
    private static extern void mcl_get_shares();

    [DllImport("__Internal")]
    private static extern void mcl_present_share(IntPtr config);

    [DllImport("__Internal")]
    private static extern void mcl_share_screenshot(IntPtr recordingConfig, IntPtr shareConfig);

    [DllImport("__Internal")]
    private static extern void mcl_present_share_to_messages(IntPtr config);

    [DllImport("__Internal")]
    private static extern void mcl_present_share_to_mail(IntPtr config);

    [DllImport("__Internal")]
    private static extern void mcl_set_default_share_config(IntPtr config);

    [DllImport("__Internal")]
    private static extern void mcl_set_default_recording_config(IntPtr config);

    [DllImport("__Internal")]
    private static extern int mcl_get_number_of_frames(string recordingId);

    [DllImport("__Internal")]
    private static extern int mcl_get_recording_score(string recordingId);

    [DllImport("__Internal")]
    private static extern void mcl_get_user_id(IntPtr callback);

    [DllImport("__Internal")]
    private static extern void mcl_set_debug_mode([MarshalAs(UnmanagedType.U1)] bool debugMode);

    [DllImport("__Internal")]
    [return: MarshalAs(UnmanagedType.U1)]
    private static extern bool mcl_get_debug_mode();

    [DllImport("__Internal")]
    private static extern void mcl_set_keep_completed_recordings([MarshalAs(UnmanagedType.U1)] bool keep);

    [DllImport("__Internal")]
    private static extern void mcl_submit_debug_data_with_message(string message);

    [DllImport("__Internal")]
    private static extern void mcl_reset_identity();

    [DllImport("__Internal")]
    private static extern void mcl_set_gif_color_table(int gifColorTable);

    [DllImport("__Internal")]
    private static extern void mcl_manual_application_did_become_active();

    [DllImport("__Internal")]
    private static extern void mcl_set_on_retrieved_shares_delegate(IntPtr f);

    [DllImport("__Internal")]
    private static extern void mcl_memcpy(IntPtr dst, IntPtr src, int length);

#region Delegates

    // Each callback needs to declare signature of the callback (the delegate), a static method that
    // implements that signature annotated with MonoPInvokeCallback, and a field for an instance of
    // the delegate to ensure the callback pointer has a a lifetime as long as the agent itself.
    // The delegates do not need to be pinned, as the native pointer derived from them exist outside
    // the GC heap, ref https://blogs.msdn.microsoft.com/cbrumme/2003/05/06/asynchronous-operations-pinning/

    private delegate void OnLinkClickedEventDelegate(MCLLinkClickedEvent e);
    private delegate void OnReceivedShareOpenedEventDelegate(MCLReceivedShareOpenedEvent e);
    private delegate void OnSentShareOpenedEventDelegate(MCLSentShareOpenedEvent e);
    private delegate void MegacoolDidCompleteShareDelegate();
    private delegate void MegacoolDidDismissShareDelegate();
    private delegate void OnRetrievedSharesDelegate(/*MCLShare[]*/ IntPtr shares, int size);
    private delegate void OnGetUserIdDelegate(string userId);

    private MegacoolDidCompleteShareDelegate shareCompletedDelegate;
    private MegacoolDidDismissShareDelegate shareDismissedDelegate;
    private OnLinkClickedEventDelegate linkClickedDelegate;
    private OnReceivedShareOpenedEventDelegate receivedShareOpenedDelegate;
    private OnSentShareOpenedEventDelegate sentShareOpenedDelegate;
    private OnRetrievedSharesDelegate sharesRetrievedDelegate;
    private OnGetUserIdDelegate getUserIdDelegate;
    private static Action<string> OnUserIdReceived = delegate { };

    [MonoPInvokeCallback(typeof(OnLinkClickedEventDelegate))]
    private static void OnLinkClickedEvent(MCLLinkClickedEvent e) {
        MegacoolLinkClickedEvent megacoolEvent = BuildLinkClickedEvent(ref e);
        Instance.eventHandler.LinkClicked(megacoolEvent);
    }

    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool NativeShouldDelete(MCLShare share);

    [MonoPInvokeCallback(typeof(OnGetUserIdDelegate))]
    private static void NativeGetUserIdCallback(string userId) {
        Instance.GetUserIdCallback(userId);
    }

    private void GetUserIdCallback(string userId) {
        Action<string> callback = OnUserIdReceived;
        if (callback != null) {
            string copiedUserId = string.Copy(userId);
            callback(copiedUserId);
        }
        OnUserIdReceived = null;
    }

    [MonoPInvokeCallback(typeof(NativeShouldDelete))]
    private static bool DeleteSharesFilter(MCLShare shareData) {
        MegacoolShare share = BuildShare(ref shareData);
        return deleteSharesFilter(share);
    }

    [MonoPInvokeCallback(typeof(OnSentShareOpenedEventDelegate))]
    private static void OnSentShareOpenedEvent(MCLSentShareOpenedEvent e) {
        MegacoolSentShareOpenedEvent megacoolEvent = BuildSentShareOpenedEvent(ref e);
        Instance.eventHandler.SentShareOpened(megacoolEvent);
    }

    [MonoPInvokeCallback(typeof(OnRetrievedSharesDelegate))]
    private static void OnRetrievedShares(IntPtr shares, int size) {
        Instance.OnRetrievedShare(shares, size);
    }

    [MonoPInvokeCallback(typeof(MegacoolDidCompleteShareDelegate))]
    static void DidCompleteShare() {
        Instance.eventHandler.ShareCompleted();
    }

    [MonoPInvokeCallback(typeof(MegacoolDidDismissShareDelegate))]
    static void DidDismissShare() {
        Instance.eventHandler.ShareDismissed();
    }

    [MonoPInvokeCallback(typeof(OnReceivedShareOpenedEventDelegate))]
    private static void OnReceivedShareOpenedEvent(MCLReceivedShareOpenedEvent e) {
        MegacoolReceivedShareOpenedEvent megacoolEvent = BuildReceivedShareOpenedEvent(ref e);
        Instance.eventHandler.ReceivedShareOpened(megacoolEvent);
    }

#endregion


    public static readonly MegacoolIOSAgent Instance = new MegacoolIOSAgent();

    private MegacoolInternal.EventHandler eventHandler;
    private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);


    public static MegacoolLinkClickedEvent BuildLinkClickedEvent(ref MCLLinkClickedEvent e) {
        string url = string.Copy(e.url);
        Uri uri = new Uri(url, UriKind.Relative);
        MegacoolReferralCode referralCode = BuildReferralCode(ref e.referral_code);
        return new MegacoolLinkClickedEvent(uri, referralCode);
    }


    private static MegacoolReferralCode BuildReferralCode(ref MCLReferralCode referralCode) {
        if (referralCode.user_id == null) {
            return null;
        }
        string userId = string.Copy(referralCode.user_id);
        string shareId = string.Copy(referralCode.share_id);
        return new MegacoolReferralCode(userId, shareId);
    }


    public static MegacoolReceivedShareOpenedEvent BuildReceivedShareOpenedEvent(ref MCLReceivedShareOpenedEvent e) {
        MegacoolShare share = BuildShare(ref e.share);
        string senderUserId = string.Copy(e.sender_user_id);
        DateTime createdAt = epoch.AddSeconds(e.created_at).ToLocalTime();
        return new MegacoolReceivedShareOpenedEvent(e.is_first_session, senderUserId, share,
            createdAt);
    }


    private static MegacoolShare BuildShare(ref MCLShare share) {
        if (share.url == null) {
            return null;
        }
        MegacoolReferralCode referralCode = BuildReferralCode(ref share.referral_code);
        DateTime createdAt = epoch.AddSeconds(share.created_at).ToLocalTime();
        DateTime updatedAt = epoch.AddSeconds(share.updated_at).ToLocalTime();
        string url = string.Copy(share.url);
        Uri uri = new Uri(url, UriKind.Relative);

        Dictionary<string, object> data;
        if (share.data_json != null) {
            data = DeserializeDataObject(share.data_json);
        } else {
            data = new Dictionary<string, object>();
        }
        return new MegacoolShare(referralCode, share.state, createdAt, updatedAt, data, uri);
    }


    private static Dictionary<string, object> DeserializeDataObject(string bytes) {
        var m_Data = Json.Deserialize(bytes) as Dictionary<string, object>;
        if (m_Data == null) {
            return new Dictionary<string, object>();
        }
        return m_Data;
    }


    public static MegacoolSentShareOpenedEvent BuildSentShareOpenedEvent(ref MCLSentShareOpenedEvent e) {
        string receiverUserId = string.Copy(e.receiver_user_id);
        MegacoolShare share = BuildShare(ref e.share);
        DateTime createdAt = epoch.AddSeconds(e.created_at).ToLocalTime();
        return new MegacoolSentShareOpenedEvent(e.is_first_session, receiverUserId, share,
            createdAt);
    }


    private static Func<MegacoolShare, bool> deleteSharesFilter = delegate(MegacoolShare arg) {
        return true;
    };

    private MegacoolIOSAgent() {
    }

    //****************** API Implementation  ******************//
    public void Start(MegacoolInternal.EventHandler eventHandler) {
        this.eventHandler = eventHandler;

        shareCompletedDelegate = new MegacoolDidCompleteShareDelegate(DidCompleteShare);
        shareDismissedDelegate = new MegacoolDidDismissShareDelegate(DidDismissShare);
        linkClickedDelegate = new OnLinkClickedEventDelegate(OnLinkClickedEvent);
        receivedShareOpenedDelegate = new OnReceivedShareOpenedEventDelegate(OnReceivedShareOpenedEvent);
        sentShareOpenedDelegate = new OnSentShareOpenedEventDelegate(OnSentShareOpenedEvent);
        sharesRetrievedDelegate = new OnRetrievedSharesDelegate(OnRetrievedShares);

        MCLConfig config = new MCLConfig {
            wrapper = "Unity",
            wrapper_version = Application.unityVersion,
            base_url = MegacoolConfiguration.Instance.CustomBaseUrl(),
            link_clicked_handler = Marshal.GetFunctionPointerForDelegate(linkClickedDelegate),
            received_share_opened_handler = Marshal.GetFunctionPointerForDelegate(receivedShareOpenedDelegate),
            sent_share_opened_handler = Marshal.GetFunctionPointerForDelegate(sentShareOpenedDelegate),
            share_completed_handler = Marshal.GetFunctionPointerForDelegate(shareCompletedDelegate),
            share_dismissed_handler = Marshal.GetFunctionPointerForDelegate(shareDismissedDelegate),
        };

        IntPtr configPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLConfig)));
        Marshal.StructureToPtr(config, configPointer, false);
        mcl_start_with_app_config(MegacoolConfiguration.Instance.appConfigIos, configPointer);
        Marshal.FreeHGlobal(configPointer);

        mcl_set_on_retrieved_shares_delegate(Marshal.GetFunctionPointerForDelegate(sharesRetrievedDelegate));
        mcl_manual_application_did_become_active();
    }

    public void StartRecording(MegacoolRecordingConfig config) {
        if (config == null) {
            mcl_start_recording(IntPtr.Zero);
            return;
        }
        MCLRecordingConfig nativeConfig = GetNativeRecordingConfig(config);
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLRecordingConfig)));
        Marshal.StructureToPtr(nativeConfig, pointer, false);
        mcl_start_recording(pointer);
        Marshal.FreeHGlobal(pointer);
    }

    public void RegisterScoreChange(int scoreDelta) {
        mcl_register_score_change(scoreDelta);
    }

    public void CaptureFrame(MegacoolRecordingConfig config, bool forceAdd) {
        if (config == null) {
            mcl_capture_frame(IntPtr.Zero, forceAdd);
            return;
        }
        MCLRecordingConfig nativeConfig = GetNativeRecordingConfig(config);
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLRecordingConfig)));
        Marshal.StructureToPtr(nativeConfig, pointer, false);
        mcl_capture_frame(pointer, forceAdd);
        Marshal.FreeHGlobal(pointer);
    }

    public void SetCaptureMethod(MegacoolCaptureMethod captureMethod, RenderTexture renderTexture) {
        if (captureMethod == MegacoolCaptureMethod.SCREEN) {
            mcl_set_capture_texture(IntPtr.Zero);
        } else {
            SignalRenderTexture(renderTexture);
        }
    }

    public void PauseRecording() {
        mcl_pause_recording();
    }

    public void StopRecording() {
        mcl_stop_recording();
    }

    public void DeleteRecording(string recordingId) {
        mcl_delete_recording(recordingId);
    }

    public void DeleteShares(Func<MegacoolShare, bool> filter) {
        deleteSharesFilter = filter;
        NativeShouldDelete callback = new NativeShouldDelete(DeleteSharesFilter);
        mcl_delete_shares(Marshal.GetFunctionPointerForDelegate(callback));
        GC.KeepAlive(callback);
    }

    private class IOSPreviewData : IMegacoolPreviewData {
        IntPtr PreviewData;

        public IOSPreviewData(IntPtr previewData) {
            PreviewData = previewData;
        }

        public IMegacoolPreviewFrame GetNextFrame() {
            // This value must match the enum definitions in mcl.h
            int MCL_BOTTOM_LEFT = 1;
            IntPtr frameDataPointer = mcl_get_next_frame_details(PreviewData, MCL_BOTTOM_LEFT);
            if (frameDataPointer == IntPtr.Zero) {
                return null;
            }

            return new IOSPreviewFrame(frameDataPointer);
        }

        public int GetNumberOfFrames() {
            return mcl_get_preview_frame_count(PreviewData);
        }

        public void Release() {
            mcl_free_preview_data(PreviewData);
            PreviewData = IntPtr.Zero;
        }

        ~IOSPreviewData() {
            Release();
        }
    }


    private class IOSPreviewFrame : IMegacoolPreviewFrame {
        private MCLPreviewFrameDetails frameDetails;
        private IntPtr framePointer;

        public IOSPreviewFrame(IntPtr framePointer) {
            this.framePointer = framePointer;
            frameDetails = (MCLPreviewFrameDetails)Marshal.PtrToStructure(framePointer,
                typeof(MCLPreviewFrameDetails));
        }

        public int GetDelayMs() {
            return frameDetails.delay_ms;
        }

        public bool LoadToTexture(Texture2D texture) {
            if (framePointer == IntPtr.Zero) {
                return false;
            }
            if (texture.width != frameDetails.width || texture.height != frameDetails.height ||
                    texture.format != TextureFormat.RGBA32) {
                texture.Resize(frameDetails.width, frameDetails.height, TextureFormat.RGBA32, false);
            }
            int pixelCount = frameDetails.width * frameDetails.height;
            byte[] texturePixels = texture.GetRawTextureData();
            GCHandle handle = GCHandle.Alloc(texturePixels, GCHandleType.Pinned);
            mcl_memcpy(handle.AddrOfPinnedObject(), frameDetails.pixels, pixelCount * 4);
            handle.Free();
            texture.LoadRawTextureData(texturePixels);
            texture.Apply();
            return true;
        }

        public void Release() {
            mcl_free_frame_details(framePointer);
            framePointer = IntPtr.Zero;
        }

        ~IOSPreviewFrame() {
            Release();
        }
    }


    public IMegacoolPreviewData GetPreviewDataForRecording(string recordingId) {
        IntPtr previewDataPointer = mcl_get_preview_for_recording(recordingId);
        if (previewDataPointer == IntPtr.Zero) {
            return null;
        }
        IOSPreviewData previewData = new IOSPreviewData(previewDataPointer);

        return previewData;
    }

    public int GetNumberOfFrames(string recordingId) {
        return mcl_get_number_of_frames(recordingId);
    }

    public int GetRecordingScore(string recordingId) {
        return mcl_get_recording_score(recordingId);
    }

    public void GetUserId(Action<string> callback) {
        // Note that this implementation does not allow several concurrent calls to GetUserId
        // as the cached delegate would be overridden.
        OnUserIdReceived = callback;
        getUserIdDelegate = new OnGetUserIdDelegate(NativeGetUserIdCallback);
        mcl_get_user_id(Marshal.GetFunctionPointerForDelegate(getUserIdDelegate));
    }

    private static Action<List<MegacoolShare>> OnSharesRetrieved = delegate { };

    public void GetShares(Action<List<MegacoolShare>> shares = null, Func<MegacoolShare, bool> filter = null) {
        OnSharesRetrieved = shares;
        mcl_get_shares();
    }

    private void OnRetrievedShare(IntPtr shares, int size) {
        long longPtr = shares.ToInt64();

        var shs = new List<MegacoolShare>(size);

        for (int i = 0; i < size; i++) {
            IntPtr structPtr = new IntPtr(longPtr);
            MCLShare shareData = (MCLShare)Marshal.PtrToStructure(structPtr, typeof(MCLShare));
            longPtr += Marshal.SizeOf(typeof(MCLShare));
            MegacoolShare share = BuildShare(ref shareData);
            shs.Add(share);
        }

        Action<List<MegacoolShare>> handler = OnSharesRetrieved;
        if (handler != null) {
            handler(shs);
        }
    }

    public void SetDefaultShareConfig(MegacoolShareConfig config) {
        if (config == null) {
            mcl_set_default_share_config(IntPtr.Zero);
            return;
        }
        MCLShareConfig nativeConfig = GetNativeShareConfig(config);
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLShareConfig)));
        Marshal.StructureToPtr(nativeConfig, pointer, false);
        mcl_set_default_share_config(pointer);
        Marshal.FreeHGlobal(pointer);
    }


    private MCLRecordingConfig GetNativeRecordingConfig(MegacoolRecordingConfig config) {
        MCLRecordingConfig nativeConfig = new MCLRecordingConfig {
            overflowStrategy = -1,
            maxFrames = -1,
            frameRate = -1,
            playbackFrameRate = -1,
            peakLocation = -1.0,
            lastFrameDelay = -1,
        };
        if (config._HasOverflowStrategy()) {
            nativeConfig.overflowStrategy = (int)config.OverflowStrategy;
        }
        if (config._HasFrameRate()) {
            nativeConfig.frameRate = config.FrameRate;
        }
        if (config._HasPlaybackFrameRate()) {
            nativeConfig.playbackFrameRate = config.PlaybackFrameRate;
        }
        if (config._HasMaxFrames()) {
            nativeConfig.maxFrames = config.MaxFrames;
        }
        if (config._HasRecordingId()) {
            nativeConfig.recordingId = config.RecordingId;
        }
        if (config._HasPeakLocation()) {
            nativeConfig.peakLocation = config.PeakLocation;
        }
        if (config._HasLastFrameDelay()) {
            nativeConfig.lastFrameDelay = config.LastFrameDelay;
        }
        if (config.LastFrameOverlay != null) {
            nativeConfig.lastFrameOverlay =
                Application.streamingAssetsPath + "/" + config.LastFrameOverlay;
        }
        return nativeConfig;
    }


    private MCLShareConfig GetNativeShareConfig(MegacoolShareConfig config) {
        MCLShareConfig nativeConfig = new MCLShareConfig {
            strategy = -1,
            popover_source_rect = new MCLRect {
                x = -1,
                y = -1,
                width = -1,
                height = -1,
            },
            popover_permitted_arrow_directions = -1,
        };
        if (config._HasStrategy()) {
            nativeConfig.strategy = (int)config.Strategy;
        }
        if (config._HasMessage()) {
            nativeConfig.message = config.Message;
        }
        if (config._HasRecordingId()) {
            nativeConfig.recordingId = config.RecordingId;
        }
        if (config.Url != null) {
            nativeConfig.url = config.Url.ToString();
        }
        if (config.FallbackImage != null) {
            nativeConfig.fallback_image = Application.streamingAssetsPath + "/" + config.FallbackImage;
        }
        if (config.Data != null) {
            nativeConfig.jsonData = Json.Serialize(config.Data);
        }
        if (config._HasModalLocation()) {
            nativeConfig.popover_source_rect.x = config.ModalLocation.x;
            nativeConfig.popover_source_rect.y = config.ModalLocation.y;
            nativeConfig.popover_source_rect.width = config.ModalLocation.width;
            nativeConfig.popover_source_rect.height = config.ModalLocation.height;
        }
        if (config._HasModalPermittedArrowDirections()) {
            nativeConfig.popover_permitted_arrow_directions = (int)config.ModalPermittedArrowDirections;
        }
        return nativeConfig;
    }


    public void SetDefaultRecordingConfig(MegacoolRecordingConfig config) {
        if (config == null) {
            mcl_set_default_recording_config(IntPtr.Zero);
            return;
        }
        MCLRecordingConfig nativeConfig = GetNativeRecordingConfig(config);
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLRecordingConfig)));
        Marshal.StructureToPtr(nativeConfig, pointer, false);
        mcl_set_default_recording_config(pointer);
        Marshal.FreeHGlobal(pointer);
    }

    public void Share(MegacoolShareConfig config) {
        if (config == null) {
            mcl_present_share(IntPtr.Zero);
            return;
        }
        MCLShareConfig nativeConfig = GetNativeShareConfig(config);
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLShareConfig)));
        Marshal.StructureToPtr(nativeConfig, pointer, false);
        mcl_present_share(pointer);
        Marshal.FreeHGlobal(pointer);
    }

    public void ShareScreenshot(MegacoolRecordingConfig recordingConfig,
            MegacoolShareConfig shareConfig) {
        IntPtr recordingPointer = IntPtr.Zero;
        if (recordingConfig != null) {
            recordingPointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLRecordingConfig)));
            MCLRecordingConfig nativeRecordingConfig = GetNativeRecordingConfig(recordingConfig);
            Marshal.StructureToPtr(nativeRecordingConfig, recordingPointer, false);
        }
        IntPtr sharePointer = IntPtr.Zero;
        if (shareConfig != null) {
            sharePointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLShareConfig)));
            MCLShareConfig nativeShareConfig = GetNativeShareConfig(shareConfig);
            Marshal.StructureToPtr(nativeShareConfig, sharePointer, false);
        }

        mcl_share_screenshot(recordingPointer, sharePointer);

        if (recordingConfig != null) {
            Marshal.FreeHGlobal(recordingPointer);
        }
        if (shareConfig != null) {
            Marshal.FreeHGlobal(sharePointer);
        }
    }

    public void ShareToMessages(MegacoolShareConfig config) {
        if (config == null) {
            mcl_present_share_to_messages(IntPtr.Zero);
            return;
        }
        MCLShareConfig nativeConfig = GetNativeShareConfig(config);
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLShareConfig)));
        Marshal.StructureToPtr(nativeConfig, pointer, false);
        mcl_present_share_to_messages(pointer);
        Marshal.FreeHGlobal(pointer);
    }

    public void ShareToMail(MegacoolShareConfig config) {
        if (config == null) {
            mcl_present_share_to_mail(IntPtr.Zero);
            return;
        }
        MCLShareConfig nativeConfig = GetNativeShareConfig(config);
        IntPtr pointer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(MCLShareConfig)));
        Marshal.StructureToPtr(nativeConfig, pointer, false);
        mcl_present_share_to_mail(pointer);
        Marshal.FreeHGlobal(pointer);
    }

    public void SetDebugMode(bool debugMode) {
        mcl_set_debug_mode(debugMode);
    }

    public bool GetDebugMode() {
        return mcl_get_debug_mode();
    }

    public void SetKeepCompletedRecordings(bool keep) {
        mcl_set_keep_completed_recordings(keep);
    }

    public void SubmitDebugData(string message) {
        mcl_submit_debug_data_with_message(message);
    }

    public void ResetIdentity() {
        mcl_reset_identity();
    }

    public void SetGIFColorTable(Megacool.GifColorTableType gifColorTable) {
        int iosValue = 0; // dynamic
        switch (gifColorTable) {
        case Megacool.GifColorTableType.GifColorTableFixed:
            iosValue = 1;
            break;
        case Megacool.GifColorTableType.GifColorTableAnalyzeFirst:
            iosValue = 2;
            break;
        }
        mcl_set_gif_color_table(iosValue);
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

    public void InitializeCapture(double scaleFactor, Megacool._TextureReadComplete callback) {
        int iosValue;
        switch (SystemInfo.graphicsDeviceType) {
            case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                iosValue = 1;
                break;
            case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                iosValue = 2;
                break;
            case UnityEngine.Rendering.GraphicsDeviceType.Metal:
                iosValue = 3;
                break;
            default:
                return;
        }
        mcl_set_capture_method_with_scale_factor(scaleFactor, iosValue);
        mcl_set_texture_read_complete_callback(callback);
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
}
#endif
