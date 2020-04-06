using System;
using System.Collections.Generic;
using UnityEngine;

public interface MegacoolIAgent {

    void Start(MegacoolInternal.EventHandler eventHandler);

    void StartRecording(MegacoolRecordingConfig config);

    void RegisterScoreChange(int scoreDelta);

    void CaptureFrame(MegacoolRecordingConfig config, bool forceAdd);

    void SetCaptureMethod(MegacoolCaptureMethod captureMethod, RenderTexture renderTexture);

    void PauseRecording();

    void StopRecording();

    void DeleteRecording(string recordingId);

    void DeleteShares(Func<MegacoolShare, bool> filter);

    IMegacoolPreviewData GetPreviewDataForRecording(string recordingId);

    int GetNumberOfFrames(string recordingId);

    int GetRecordingScore(string recordingId);

    void GetUserId(Action<string> callback);

    void GetShares(Action<List<MegacoolShare>> shares = null, Func<MegacoolShare, bool> filter = null);

    void Share(MegacoolShareConfig config);

    void ShareScreenshot(MegacoolRecordingConfig recordingConfig, MegacoolShareConfig shareConfig);

    void ShareToMessages(MegacoolShareConfig config);

    void ShareToMail(MegacoolShareConfig config);

    void SetDefaultShareConfig(MegacoolShareConfig config);

    void SetDefaultRecordingConfig(MegacoolRecordingConfig config);

    void SetDebugMode(bool debugMode);

    bool GetDebugMode();

    void SetKeepCompletedRecordings(bool keep);

    void SubmitDebugData(string message);

    void ResetIdentity();

    void SetGIFColorTable(Megacool.GifColorTableType gifColorTable);

    void SignalRenderTexture(RenderTexture texture);

    void IssuePluginEvent(ref IntPtr nativePluginCallbackPointer, int eventId);

    void InitializeCapture(double scaleFactor, Megacool._TextureReadComplete textureReadCallback);

    int GetScaledWidth();

    int GetScaledHeight();

    void SetUnscaledCaptureDimensions(int width, int height);
}
