using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// Preview a recording before share.
/// </summary>
[AddComponentMenu("Megacool/Gif Preview")]
public class MegacoolGifPreview : MonoBehaviour {

    private GUISystem guiSystem = null;
    private Coroutine _playGifIEnumerator;
    private IMegacoolPreviewData previewData;

    Texture2D previewTexture;

    /// <summary>
    /// Starts previewing the given recording.
    /// </summary>
    /// <param name="recordingIdentifier">Recording identifier.</param>
    public void StartPreview(string recordingIdentifier = default(string)) {

        if (guiSystem == null) {
            guiSystem = new GUISystem(gameObject);
        }

        StopPreview();

        this.previewData = Megacool.Instance._GetPreviewDataForRecording(recordingIdentifier);
        if (previewData == null) {
            return;
        }

        _playGifIEnumerator = StartCoroutine(
            PreviewMegacoolGif()
        );
    }

    /// <summary>
    /// Stops the preview.
    /// </summary>
    public void StopPreview() {
        if (_playGifIEnumerator != null) {
            StopCoroutine(_playGifIEnumerator);

            guiSystem.HidePreview();

            // Only destroy the preview texture if it has been created, might not be the case if there were no frames
            // in the preview or it was stopped before any frames were loaded.
            if (previewTexture) {
                Destroy(previewTexture);
            }

            _playGifIEnumerator = null;
        }

        if (this.previewData != null) {
            this.previewData.Release();
            this.previewData = null;
        }
    }

    /// <summary>
    /// Get the number of frames available in the recording. Only available after calling
    /// StartPreview and before calling StopPreview.
    /// </summary>
    /// <description>
    /// Use Megacool.Instance.GetNumberOfFrames() to get the number available without creating a
    /// preview.
    /// </description>
    /// <returns>If StartPreview is called, the number of frames available in the preview, otherwise
    /// -1.</returns>
    public int GetNumberOfFrames() {
        if (previewData != null) {
            return previewData.GetNumberOfFrames();
        }
        return -1;
    }

    void OnDisable() {
        // Make sure the preview data is released even if StopPreview isn't called
        StopPreview();
    }

    private IEnumerator PreviewMegacoolGif() {
        previewTexture = new Texture2D(2, 2, TextureFormat.ARGB32, false);

        IMegacoolPreviewFrame nextFrame = loadNextFrame(previewData);
        if (nextFrame == null) {
            Debug.Log("Could not load any frames in preview");
            yield break;
        }
        bool isFirstFrame = true;

        int currentFrameTargetTimeMs = nextFrame.GetDelayMs();
        double currentFrameTimeMs = currentFrameTargetTimeMs - Time.deltaTime * 1000;

        // The preview is stopped in StopPreview by stopping the coroutine
        while (true) {
            if (nextFrame == null) {
                Debug.Log("Couldn't load next frame in preview, skipping");
                yield return null;
                nextFrame = loadNextFrame(previewData);
                continue;
            }

            currentFrameTimeMs += Time.deltaTime * 1000;
            if (currentFrameTimeMs < currentFrameTargetTimeMs) {
                yield return null;
                continue;
            }

            bool loadSuccess = nextFrame.LoadToTexture(previewTexture);
            nextFrame.Release();
            if (!loadSuccess) {
                yield return null;
                nextFrame = loadNextFrame(previewData);
                continue;
            }

            guiSystem.SetTexture(previewTexture);

            if (isFirstFrame) {
                isFirstFrame = false;
                guiSystem.ShowPreview();
            }

            currentFrameTimeMs = currentFrameTimeMs - currentFrameTargetTimeMs;
            currentFrameTargetTimeMs = nextFrame.GetDelayMs();

            yield return null;

            nextFrame = loadNextFrame(previewData);
        }
    }

    private IMegacoolPreviewFrame loadNextFrame(IMegacoolPreviewData preview) {
        for (int i = 0; i < preview.GetNumberOfFrames(); i++) {
            IMegacoolPreviewFrame frame = preview.GetNextFrame();
            if (frame == null) {
                Debug.Log("Skipping preview frame that failed to load");
                continue;
            }
            return frame;
        }
        return null;
    }

    private class GUISystem {

        private GameObject gameObject;
        private Component guiComponent;

        private const string requiredNGUIComponent = "UITexture";
        private const string requiredUGUIComponent = "RawImage";
        string textureProperty;

        public GUISystem(GameObject gameObject) {
            this.gameObject = gameObject;
            AssignGuiFramework();
        }

        private void AssignGuiFramework() {
            var nguiComponent = gameObject.GetComponent(requiredNGUIComponent);
            var uguiComponent = gameObject.GetComponent(requiredUGUIComponent);
            if (uguiComponent != null) {
                textureProperty = "texture";
            } else if (nguiComponent != null) {
                textureProperty = "mainTexture";
            } else {
                string errorMessage = "Missing Required Component ";

                // check if the ngui exists in project
                var nguiAssembly = (from assembly in System.AppDomain.CurrentDomain.GetAssemblies()
                    from type in assembly.GetTypes ()
                    where type.Name == requiredNGUIComponent
                    select type).FirstOrDefault ();
                if (nguiAssembly != null) {
                    errorMessage += requiredNGUIComponent;
                } else {
                    errorMessage += requiredNGUIComponent;
                }
                Debug.LogWarning("Megacool: " + errorMessage);
            }
            guiComponent = nguiComponent ?? uguiComponent;
        }

        public void ShowPreview() {
            if (guiComponent != null) {
                guiComponent.GetType().GetProperty("enabled").SetValue(guiComponent, true, null);
            }
        }

        public void HidePreview() {
            if (guiComponent != null) {
                guiComponent.GetType().GetProperty("enabled").SetValue(guiComponent, false, null);
                guiComponent.GetType().GetProperty(textureProperty).SetValue(guiComponent, null, null);
            }
        }

        public void SetTexture(Texture2D texture) {
            if (guiComponent != null) {
                guiComponent.GetType().GetProperty(textureProperty).SetValue(guiComponent, texture, null);
            }
        }
    }

}
