#if (!UNITY_IPHONE && !UNITY_IOS && !UNITY_ANDROID && !UNITY_EDITOR)
using System;
using System.Collections.Generic;
using UnityEngine;

public class MegacoolUnsupportedAgent : MegacoolIAgent {

    public void Start(MegacoolInternal.EventHandler eventHandler) {
        Debug.LogWarning("Megacool: Unsupported platform, all functionality will be a no-op." +
            "This version of the SDK is only supported on iOS and Android.");
    }

    public void StartRecording(MegacoolRecordingConfig config) {
    }

    public void RegisterScoreChange(int scoreDelta) {
    }

    public void CaptureFrame(MegacoolRecordingConfig config, bool forceAdd) {
    }

    public void SetCaptureMethod(MegacoolCaptureMethod captureMethod, RenderTexture renderTexture) {
    }

    public void PauseRecording() {
    }

    public void StopRecording() {
    }

    public void DeleteRecording(string recordingId) {
    }

    public IMegacoolPreviewData GetPreviewDataForRecording(string recordingId) {
        return null;
    }

    public int GetNumberOfFrames(string recordingId) {
        return 0;
    }

    public int GetRecordingScore(string recordingId) {
        return -1;
    }

    public void GetUserId(Action<string> callback) {
    }

    public void Share(MegacoolShareConfig config) {
    }

    public void ShareScreenshot(MegacoolRecordingConfig recordingConfig, MegacoolShareConfig shareConfig) {
    }

    public void ShareToMessages(MegacoolShareConfig config) {
    }

    public void ShareToMail(MegacoolShareConfig config) {
    }

    public void GetShares(Action<List<MegacoolShare>> shares, Func<MegacoolShare, bool> filter = null) {
    }

    public void SetDebugMode(bool debugMode) {
    }

    public bool GetDebugMode() {
        return false;
    }

    public void SetKeepCompletedRecordings(bool keep) {
    }

    public void DeleteShares(Func<MegacoolShare, bool> filter) {
    }

    public void SubmitDebugData(string message) {
    }

    public void ResetIdentity() {
    }

    public void SetGIFColorTable(Megacool.GifColorTableType gifColorTable) {
    }

    public void SetDefaultShareConfig(MegacoolShareConfig config) {
    }

    public void SetDefaultRecordingConfig(MegacoolRecordingConfig config) {
    }

    public void SignalRenderTexture(RenderTexture texture) {
    }

    public void IssuePluginEvent(ref IntPtr nativePluginCallbackPointer, int eventId) {
    }

    public void InitializeCapture(double scaleFactor, Megacool._TextureReadComplete textureReadCallback) {
    }

    public int GetScaledWidth() {
        // Not returning 0 to avoid error when creating render texture
        return 1;
    }

    public int GetScaledHeight() {
        return 1;
    }

    public void SetUnscaledCaptureDimensions(int width, int height) {
    }
}
#endif
