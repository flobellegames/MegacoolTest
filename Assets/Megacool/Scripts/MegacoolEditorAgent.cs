#if UNITY_EDITOR
using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class MegacoolEditorAgent : MegacoolIAgent {

    MegacoolEditorRecordingManager recordingManager;

    private const int MCTR = 0x6d637472;
    private double scaleFactor = 0.5;
    private int scaledWidth = 0;
    private int scaledHeight = 0;

    private void ImplementationWarning(string message) {
        Debug.LogWarning("Megacool: " + message + " does not work in the Editor, but will work " +
            "when running on device");
    }

    //****************** API Implementation  ******************//
    public void Start(MegacoolInternal.EventHandler eventHandler) {
        recordingManager = new MegacoolEditorRecordingManager();
    }

    public void StartRecording(MegacoolRecordingConfig config) {
        recordingManager.StartRecording(config);
    }

    public void RegisterScoreChange(int scoreDelta) {
        recordingManager.RegisterScoreChange(scoreDelta);
    }

    public void CaptureFrame(MegacoolRecordingConfig config, bool forceAdd) {
        recordingManager.CaptureFrame(config, forceAdd);
    }

    public void SetCaptureMethod(MegacoolCaptureMethod captureMethod, RenderTexture renderTexture) {
        // Ignore, the editor only supports texture reads and the Megacool class prevents
        // setting anything else if the renderer isn't OpenGL ES3, which it never is in the Editor.
    }

    public void PauseRecording() {
        recordingManager.PauseRecording();
    }

    public void StopRecording() {
        recordingManager.StopRecording();
    }

    public void DeleteRecording(string recordingId) {
        recordingManager.DeleteRecording(recordingId);
    }

    public IMegacoolPreviewData GetPreviewDataForRecording(string recordingId) {
        return recordingManager.GetPreviewInfoForRecording(recordingId);
    }

    public int GetNumberOfFrames(string recordingId) {
        return recordingManager.GetNumberOfFrames(recordingId);
    }

    public int GetRecordingScore(string recordingId) {
        return recordingManager.GetRecordingScore(recordingId);
    }

    public void GetUserId(Action<string> callback) {
        ImplementationWarning("GetUserId");
    }

    public void Share(MegacoolShareConfig config) {
        ImplementationWarning("Share");
    }

    public void ShareScreenshot(MegacoolRecordingConfig recordingConfig,
            MegacoolShareConfig shareConfig) {
        ImplementationWarning("ShareScreenshot");
    }

    public void ShareToMessages(MegacoolShareConfig config) {
        ImplementationWarning("ShareToMessages");
    }

    public void ShareToMail(MegacoolShareConfig config) {
        ImplementationWarning("ShareToMail");
    }

    public void GetShares(Action<List<MegacoolShare>> shares, Func<MegacoolShare, bool> filter = null) {
        ImplementationWarning("GetShares");
    }

    public void SetDefaultShareConfig(MegacoolShareConfig config) {
    }

    public void SetDefaultRecordingConfig(MegacoolRecordingConfig config) {
        recordingManager.DefaultRecordingConfig = config ?? new MegacoolRecordingConfig();
    }

    public void SetDebugMode(bool debugMode) {
    }

    public bool GetDebugMode() {
        return false;
    }

    public void SetKeepCompletedRecordings(bool keep) {
        recordingManager.keepCompletedRecordings = keep;
    }

    public void DeleteShares(Func<MegacoolShare, bool> filter) {
    }

    public void SubmitDebugData(string message) {
        ImplementationWarning("SubmitDebugData");
    }

    public void ResetIdentity() {
        ImplementationWarning("ResetIdentity");
    }

    public void SetGIFColorTable(Megacool.GifColorTableType gifColorTable) {
    }

    public void SignalRenderTexture(RenderTexture texture) {
    }

    public void IssuePluginEvent(ref IntPtr nativePluginCallbackPointer, int eventId) {
        if (eventId == MCTR) {
            recordingManager.SignalEndOfFrame();
            CaptureFrame(null, false);
        }
    }

    public void InitializeCapture(double scaleFactor, Megacool._TextureReadComplete TextureReadCompleteCallback) {
#pragma warning disable RECS0018
        if (scaleFactor == 0.0) {
#pragma warning restore RECS0018
            // Revert to default on 0
            scaleFactor = 0.5;
        }
        this.scaleFactor = scaleFactor;

        recordingManager.SetReadCompleteCallback(TextureReadCompleteCallback);
    }

    public int GetScaledWidth() {
        return scaledWidth;
    }

    public int GetScaledHeight() {
        return scaledHeight;
    }

    public void SetUnscaledCaptureDimensions(int width, int height) {
        scaledWidth = (int)(width * scaleFactor);
        scaledHeight = (int)(height * scaleFactor);
    }
}

public class MegacoolEditorRecordingManager {

    public MegacoolRecordingConfig DefaultRecordingConfig = new MegacoolRecordingConfig();
    MegacoolRecordingPersistent persistentRecordings = new MegacoolRecordingPersistent();
    private MegacoolRecording currentRecording;
    private static bool capturingFrame = false;
    public bool keepCompletedRecordings;
    private Megacool._TextureReadComplete ReadCompleteCallback;

    public void SetReadCompleteCallback(Megacool._TextureReadComplete callback) {
        ReadCompleteCallback = callback;
    }

    public void StartRecording(MegacoolRecordingConfig config) {
        if (currentRecording != null && !currentRecording.isFinished) {
            currentRecording = persistentRecordings.Restore(config.RecordingId);
            return;
        }
        if (!keepCompletedRecordings) {
            persistentRecordings.Clear();
        }
        MegacoolRecordingConfig mergedConfig = MergeWithDefault(config);
        currentRecording = new MegacoolRecording(mergedConfig);
        currentRecording.Start();
    }

    MegacoolRecordingConfig MergeWithDefault(MegacoolRecordingConfig config) {
        if (config == null) {
            return DefaultRecordingConfig;
        }
        MegacoolRecordingConfig merged = new MegacoolRecordingConfig();
        merged.MaxFrames = config._HasMaxFrames() ? config.MaxFrames : DefaultRecordingConfig.MaxFrames;
        merged.FrameRate = config._HasFrameRate() ? config.FrameRate : DefaultRecordingConfig.FrameRate;
        merged.LastFrameDelay = config._HasLastFrameDelay() ? config.LastFrameDelay : DefaultRecordingConfig.LastFrameDelay;
        merged.LastFrameOverlay = config.LastFrameOverlay ?? DefaultRecordingConfig.LastFrameOverlay;
        merged.OverflowStrategy = config._HasOverflowStrategy() ? config.OverflowStrategy : DefaultRecordingConfig.OverflowStrategy;
        merged.PeakLocation = config._HasPeakLocation() ? config.PeakLocation : DefaultRecordingConfig.PeakLocation;
        merged.RecordingId = config._HasRecordingId() ? config.RecordingId : DefaultRecordingConfig.RecordingId;
        merged.PlaybackFrameRate = config._HasPlaybackFrameRate() ? config.PlaybackFrameRate : DefaultRecordingConfig.PlaybackFrameRate;
        return merged;
    }

    public void StopRecording() {
        if (currentRecording != null) {
            currentRecording.isFinished = true;
        }
    }

    public void PauseRecording() {
        if (currentRecording != null) {
            persistentRecordings.Save(currentRecording);
        }
    }

    public void DeleteRecording(string recordingId) {
        persistentRecordings.Delete(recordingId);
    }

    public void RegisterScoreChange(int scoreDelta) {
        if (currentRecording != null) {
            currentRecording.GetOverflowStrategy().RegisterScoreChange(scoreDelta);
        }
    }

    public void CaptureFrame(MegacoolRecordingConfig config, bool forceAdd) {
        if (currentRecording == null || currentRecording.isFinished) {
            MegacoolRecordingConfig mergedConfig = MergeWithDefault(config);
            currentRecording = new MegacoolRecording(mergedConfig);
            currentRecording.Start();
        }
        capturingFrame = true;
        currentRecording.CaptureFrame(forceAdd);
    }

    public void SignalEndOfFrame() {
        if (currentRecording != null && (Megacool.Instance._IsRecording || capturingFrame)) {
            currentRecording.SignalEndOfFrame(ReadCompleteCallback);
            capturingFrame = false;
        }
    }

    public class EditorPreviewData : IMegacoolPreviewData {
        readonly string[] FramePaths;
        readonly int PlaybackFrameRate = 0;
        readonly int LastFrameDelay = 0;

        int CurrentFrame = 0;

        public EditorPreviewData(string[] framePaths, int playbackFrameRate, int lastFrameDelay) {
            FramePaths = framePaths;
            PlaybackFrameRate = playbackFrameRate;
            LastFrameDelay = lastFrameDelay;
        }

        public IMegacoolPreviewFrame GetNextFrame() {
            if (this.FramePaths.Length == 0) {
                return null;
            }
            string nextPath = this.FramePaths[this.CurrentFrame];

            IMegacoolPreviewFrame frame;
            if (this.CurrentFrame == this.FramePaths.Length - 1) {
                frame = new EditorPreviewFrame(nextPath, this.LastFrameDelay);
            } else {
                frame = new EditorPreviewFrame(nextPath, (1000/this.PlaybackFrameRate));
            }

            this.CurrentFrame = (this.CurrentFrame + 1) % this.FramePaths.Length;
            return frame;
        }

        public int GetNumberOfFrames() {
            return FramePaths.Length;
        }

        public void Release() {
        }
    }


    private class EditorPreviewFrame : IMegacoolPreviewFrame {

        private readonly string path;
        private readonly int frameDelayMs;

        public EditorPreviewFrame(string path, int frameDelay) {
            this.path = path;
            this.frameDelayMs = frameDelay;
        }

        public int GetDelayMs() {
            return frameDelayMs;
        }

        public bool LoadToTexture(Texture2D texture) {
            byte[] bytes;
            try {
                bytes = File.ReadAllBytes(path);
            } catch (Exception e) {
                Debug.Log("Failed to load bytes for frame " + path + ": " + e);
                return false;
            }
            return texture.LoadImage(bytes);
        }

        public void Release() {

        }
    }


    public IMegacoolPreviewData GetPreviewInfoForRecording(string recordingId) {
        MegacoolRecording recording = GetRecording(recordingId);
        if (recording == null) {
            return null;
        }
        return recording.GetPreviewInfo();
    }

    public int GetNumberOfFrames(string recordingId) {
        MegacoolRecording recording = GetRecording(recordingId);
        if (recording == null) {
            return 0;
        }
        return recording.GetNumberOfFrames();
    }

    public int GetRecordingScore(string recordingId) {
        MegacoolRecording recording = GetRecording(recordingId);
        if (recording == null) {
            return -1;
        }
        return recording.GetScore();
    }

    private MegacoolRecording GetRecording(string recordingId) {
        if (recordingId == null) {
            recordingId = "";
        }

        MegacoolRecording recording = currentRecording;
        if (recording == null || !recording.RecordingId.Equals(recordingId)) {
            recording = persistentRecordings.Restore(recordingId);
        }

        return recording;
    }

    private class MegacoolRecording {
        public bool isFinished = false;
        private Buffer buffer;
        private string recordingId;
        private int lastFrameDelay;
        private int playbackFrameRate;
        private int maxFrames;
        private double peakLocation;

        public string RecordingId { get { return recordingId; } }
        public int MaxFrames { get { return maxFrames; } }
        public float PeakLocation { get { return (float)peakLocation; } }

        public MegacoolRecording(MegacoolRecordingConfig config) {
            recordingId = config.RecordingId;
            lastFrameDelay = config.LastFrameDelay;
            playbackFrameRate = config.PlaybackFrameRate;
            maxFrames = config.MaxFrames;
            peakLocation = config.PeakLocation;
            switch (config.OverflowStrategy) {
                case MegacoolOverflowStrategy.HIGHLIGHT:
                    buffer = new HighlightBuffer(this);
                    break;
                case MegacoolOverflowStrategy.TIMELAPSE:
                    buffer = new TimelapseBuffer(recordingId, maxFrames);
                    break;
                default:
                    buffer = new CircularBuffer(recordingId, maxFrames);
                    break;
            }
        }

        public void Start() {
            string recordingDirectory = buffer.GetRecordingDirectory(recordingId);
            if (Directory.Exists(recordingDirectory)) {
                Directory.Delete(recordingDirectory, true);
            }
            Directory.CreateDirectory(recordingDirectory);
        }

        public IMegacoolPreviewData GetPreviewInfo() {
            string recordingDirectory = buffer.GetRecordingDirectory(recordingId);
            if (!Directory.Exists(recordingDirectory)){
                return null;
            }

            string[] framePaths = buffer.GetFramePaths();
            return new EditorPreviewData(framePaths, playbackFrameRate, lastFrameDelay);
        }

        public int GetNumberOfFrames() {
            return buffer.GetFramePaths().Length;
        }

        public void CaptureFrame(bool forceAdd) {
            if (buffer.ShouldCapture() || forceAdd) {
                buffer.PushFrame();
            }
        }

        public void SignalEndOfFrame(Megacool._TextureReadComplete ReadCompleteCallback) {
            buffer.SignalEndOfFrame(ReadCompleteCallback);
        }

        public Buffer GetOverflowStrategy() {
            return buffer;
        }

        public int GetScore() {
            return buffer.GetScore();
        }
    }

    private class MegacoolRecordingPersistent {
        private Dictionary<string, MegacoolRecording> savedRecordings = new Dictionary<string, MegacoolRecording>();

        public void Save(MegacoolRecording recording) {
            string recordingId = recording.RecordingId;
            if (savedRecordings.ContainsKey(recordingId)) {
                savedRecordings[recordingId] = recording;
            } else {
                savedRecordings.Add(recording.RecordingId, recording);
            }
        }

        public void Delete(string recordingId) {
            if (recordingId == null) {
                recordingId = "";
            }
            savedRecordings.Remove(recordingId);
            string recordingDirectory = GetRecordingFramesDirectory(recordingId);
            try {
                Directory.Delete(recordingDirectory, true);
            } catch (DirectoryNotFoundException) {
            }
        }

        public MegacoolRecording Restore(string recordingId) {
            if (recordingId == default(string)) {
                recordingId = "";
            }
            MegacoolRecording savedRecording = null;
            savedRecordings.TryGetValue(recordingId, out savedRecording);
            return savedRecording;
        }

        public void Clear() {
            string framesDirectory = GetFramesDirectory();
            Directory.Delete(framesDirectory, true);
            Directory.CreateDirectory(framesDirectory);
            savedRecordings.Clear();
        }

        public static string GetFramesDirectory() {
            return Application.temporaryCachePath + "/frames/";
        }

        public static string GetRecordingFramesDirectory(string recordingId) {
            string framesDirectory = GetFramesDirectory();
            if (recordingId == null) {
                recordingId = "";
            }
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(recordingId);
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);
            string hashString = "";
            for (int i = 0; i < hashBytes.Length; i++) {
                hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }
            hashString.PadLeft(32, '0');
            return framesDirectory + hashString + "/";
        }

    }

    private abstract class Buffer {
        protected string framesDirectory = MegacoolRecordingPersistent.GetFramesDirectory();
        protected int width = Screen.width;
        protected int height = Screen.height;
        private Texture2D currentFrame = null;
        private bool pixelsRead;

        public string GetRecordingDirectory(string recordingId) {
            return MegacoolRecordingPersistent.GetRecordingFramesDirectory(recordingId);
        }

        public Buffer(){
            int renderTextureWidth = Megacool.Instance._RenderTexture.width;
            int renderTextureHeight = Megacool.Instance._RenderTexture.height;
            currentFrame = new Texture2D(renderTextureWidth, renderTextureHeight, TextureFormat.RGB24, false);
            pixelsRead = false;
        }

        private void ReadPixels(Megacool._TextureReadComplete ReadCompleteCallback) {
            RenderTexture previous = RenderTexture.active;
            RenderTexture active = Megacool.Instance._RenderTexture;
            RenderTexture.active = active;
            currentFrame.ReadPixels(new Rect(0,0, active.width, active.height), 0, 0);
            RenderTexture.active = previous;
            pixelsRead = true;
            if (ReadCompleteCallback != null) {
                ReadCompleteCallback();
            }
        }

        public void SignalEndOfFrame(Megacool._TextureReadComplete ReadCompleteCallback) {
            if (pixelsRead == false) {
                ReadPixels(ReadCompleteCallback);
            }
        }

        protected bool WriteFrameToFile(string directory, string name) {
            if (pixelsRead == true) {
                string frameName = "frame-" + name;
                string frameFileName = directory + frameName + ".png";
                byte[] frameData = currentFrame.EncodeToPNG();
                var newFile = File.Create(frameFileName);
                newFile.Write(frameData, 0, frameData.Length);
                newFile.Close();
                pixelsRead = false;
                return true;
            } else {
                return false;
            }
        }

        public virtual bool ShouldCapture(){ return true; }
        public virtual void RegisterScoreChange(int scoreDelta) {
            Debug.LogWarning("Megacool: The current recording does not use the highlight overflow strategy");
        }
        public virtual int GetScore() {
            return 0;
        }
        public abstract string[] GetFramePaths();
        public abstract void PushFrame();
    }

    private class CircularBuffer : Buffer {
        private int size;
        private int maxSize;
        private string recordingId;

        public CircularBuffer(string recordingId, int maxSize) {
            this.size = 0;
            this.maxSize = maxSize;
            this.recordingId = recordingId;
        }

        public override void PushFrame() {
            string recordingDirectory = GetRecordingDirectory(recordingId);
            int frameNumber = (size + 1) % maxSize;
            bool frameWritten = WriteFrameToFile(recordingDirectory, frameNumber.ToString());
            if (frameWritten) {
                size++;
            }
        }

        public override string[] GetFramePaths() {
            string recordingDirectory = GetRecordingDirectory(recordingId);
            List<string> framePaths = new List<string>();
            if (size <= maxSize) {
                for (int i = 1; i < size; i++) {
                    string frameNumber = "frame-" + i.ToString();
                    string frameName = recordingDirectory + frameNumber + ".png";
                    framePaths.Add(frameName);
                }
            } else {
                for (int index = size % maxSize, counter = maxSize; counter > 0;
                  index = (index + 1) % maxSize, counter--) {
                      string frameNumber = "frame-" + index.ToString();
                      string frameName = recordingDirectory + frameNumber + ".png";
                      framePaths.Add(frameName);
                  }
            }
            return framePaths.ToArray();
        }
    }

    private class TimelapseBuffer : Buffer {
        private int rate;
        private int maxFrames;
        private int frameNumber;
        private int framesBeforeNextStorage;
        private int framesOnDisk;
        private string recordingId;

        public TimelapseBuffer(string recordingId, int maxSize) {
            this.rate = 1;
            this.maxFrames = maxSize;
            this.frameNumber = -1;
            this.recordingId = recordingId;
        }

        public override bool ShouldCapture() {
            framesBeforeNextStorage -= 1;
            frameNumber++;
            return framesBeforeNextStorage <= 0;
        }

        public override void PushFrame() {
            string recordingDirectory = GetRecordingDirectory(recordingId);
            bool frameWritten = WriteFrameToFile(recordingDirectory, frameNumber.ToString());
            if (frameWritten) {
                framesOnDisk += 1;
                if (framesOnDisk > maxFrames) {
                    rate = (int)Mathf.Min(rate * 2, Mathf.Pow(2, 20));
                    framesOnDisk -= Trim(frameNumber, rate);
                }
                framesBeforeNextStorage = rate - (frameNumber % rate);
            }
        }

        private int Trim(int frameNumber, int rate) {
            string recordingDirectory = GetRecordingDirectory(recordingId);
            int framesDeleted = 0;
            int lastFrameToDelete = frameNumber % rate == 0 ? frameNumber - 1 : frameNumber;
            for (int i = rate / 2; i <= lastFrameToDelete; i += rate) {
                string file = recordingDirectory + "frame-" + i.ToString() + ".png";
                if (File.Exists(file)) {
                    File.Delete(file);
                    framesDeleted++;
                }
            }
            return framesDeleted;
        }

        private int GetFileNumberFromName(string fileName) {
            int start = fileName.IndexOf("frame-") + "frame-".Length;
            int end = fileName.LastIndexOf(".png");
            string frameNumber = fileName.Substring(start, end - start);
            return int.Parse(frameNumber);
        }

        public override string[] GetFramePaths() {
            string recordingDirectory = GetRecordingDirectory(recordingId);
            string[] info = Directory.GetFiles(recordingDirectory, "*.png");
            List<string> files = new List<string>(info);
            files.Sort(
                delegate(string file1, string file2) {
                    int fileNumber1 = GetFileNumberFromName(file1);
                    int fileNumber2 = GetFileNumberFromName(file2);
                    return fileNumber1.CompareTo(fileNumber2);
                }
            );
            return files.ToArray();
        }
    }

    private class HighlightBuffer : Buffer {
        private const float decay = .9f;
        private int maxFrames;
        private int frameNumber;
        private int frameScore;
        private int framesAfterPeak;
        private double maxIntensity;
        private int boringFrameNumber;
        private MegacoolRecording recording;
        private List<int> frameScores;
        private HighlightWindow curHighlight;
        private HighlightWindow curWindow;

        public HighlightBuffer(MegacoolRecording recording) {
            this.maxFrames = recording.MaxFrames;
            this.recording = recording;
            curHighlight = new HighlightWindow(0,0,0);
            curWindow = new HighlightWindow(0,0,0);
            frameScore = 0;
            frameScores = new List<int>();
            maxIntensity = 0;
            boringFrameNumber = 0;
            frameNumber = 0;
            CalculateFramesAfterPeak(recording);
        }

        private void CalculateFramesAfterPeak(MegacoolRecording recording) {
            int framesBeforePeak = (int) Mathf.Ceil(recording.PeakLocation * maxFrames);
            if (framesBeforePeak > maxFrames) {
                framesBeforePeak = maxFrames;
            } else if (framesBeforePeak <= 0) {
                framesBeforePeak = 1;
            }
            framesAfterPeak = maxFrames - framesBeforePeak;
        }

        private bool FrameWithinDeletableBounds(int frameIndex) {
            if (frameIndex < 0) {
                return false;
            }
            if (frameIndex >= curHighlight.start && frameIndex <= curHighlight.end) {
                return false;
            }
            return true;
        }

        private void SetHighlightFromWindow() {
            curHighlight.end = curWindow.end;
            curHighlight.start = curWindow.start;
            curHighlight.score = curWindow.score;
        }

        private void CheckPeak() {
            double curIntensity = CalculateIntensity();
            if (curIntensity >= maxIntensity) {
                maxIntensity = curIntensity;
                boringFrameNumber = 0;
            } else {
                boringFrameNumber++;
            }
        }

        private double CalculateIntensity() {
            double intensity = 0.0;
            for (int i = 0; i < frameScores.Count - 1; i++) {
                intensity += frameScores[i] * Mathf.Pow(decay, (frameScores.Count - i));
            }
            intensity += frameScores[frameScores.Count - 1];
            return intensity;
        }

        private void CheckAddHighlight() {
            if (curWindow.score > curHighlight.score || (curWindow.start > curHighlight.end)) {
                DeleteHighlight();
                SetHighlightFromWindow();
            }
        }

        private void DeleteFrame(int frameIndex) {
            string recordingDirectory = GetRecordingDirectory(recording.RecordingId);
            string frameName = "frame-" + frameIndex.ToString() + ".png";
            if (File.Exists(recordingDirectory + frameName)) {
                File.Delete(recordingDirectory + frameName);
            }
        }

        private void DeleteHighlight() {
            int stopIndex = Mathf.Min(curHighlight.end, curWindow.start - 1);
            for (int i = curHighlight.start; i <= stopIndex; i++) {
                DeleteFrame(i);
            }
        }

        private void ShiftWindowStart() {
            int oldStartFrame = curWindow.end - maxFrames;
            if (FrameWithinDeletableBounds(oldStartFrame)) {
                DeleteFrame(oldStartFrame);
            }
            if (frameNumber >= maxFrames) {
                int oldStartScore = frameScores[0];
                frameScores.RemoveAt(0);
                curWindow.start = oldStartFrame + 1;
                curWindow.score = curWindow.score - oldStartScore;
            }
        }

        private bool ShiftWindowEnd() {
            string recordingDirectory = GetRecordingDirectory(recording.RecordingId);
            int currentFramescore = frameScore;
            bool frameWritten = WriteFrameToFile(recordingDirectory, frameNumber.ToString());
            if (frameWritten) {
                frameScore = 0;
                frameScores.Add(currentFramescore);
                curWindow.end = frameNumber;
                curWindow.score = curWindow.score + currentFramescore;
            }
            return frameWritten;
        }

        public override void PushFrame() {
            if (ShiftWindowEnd()) {
                ShiftWindowStart();
                CheckPeak();
                if (boringFrameNumber == framesAfterPeak) {
                    CheckAddHighlight();
                }
                frameNumber++;
            }
        }

        public override void RegisterScoreChange(int scoreDelta) {
            frameScore += scoreDelta;
        }

        public override string[] GetFramePaths() {
            if (frameNumber == 0) {
                return (new string[0]);
            }
            CheckAddHighlight();
            int start = curHighlight.start;
            int end = curHighlight.end;
            if (start == -1 && end == -1) {
                start = curWindow.start;
                end = curWindow.end;
            }
            string recordingDirectory = GetRecordingDirectory(recording.RecordingId);
            List<string> framePaths = new List<string>();
            for (int i = start; i <= end; i++) {
                string frameNumber = "frame-" + i.ToString();
                string frameName = recordingDirectory + frameNumber + ".png";
                framePaths.Add(frameName);
            }
            return framePaths.ToArray();
        }

        public override int GetScore() {
            return Math.Max(curWindow.score, curHighlight.score);
        }
    }

    private class HighlightWindow {
        public int start;
        public int end;
        public int score;

        public HighlightWindow(int start, int end, int score) {
            this.start = start;
            this.end = end;
            this.score = score;
        }
    }
}
#endif
