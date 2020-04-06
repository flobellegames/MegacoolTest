using UnityEditor;
using UnityEngine;

enum PlayMode {
     PLAYING,
     STOPPED,
     PAUSED
}

[InitializeOnLoad]
public class MegacoolEditorRecording {
    private static PlayMode _state = PlayMode.STOPPED;
    private static string framesDirectory = Application.temporaryCachePath + "/frames";

    static MegacoolEditorRecording() {
#if UNITY_2017_2_OR_NEWER
        EditorApplication.playModeStateChanged += OnPlayModeStateChange;
#else
        EditorApplication.playmodeStateChanged = OnPlayModeStateChange;
#endif
        if (EditorApplication.isPaused) {
            _state = PlayMode.PAUSED;
        }
    }

    private static void OnPlayStart() {
        System.IO.Directory.CreateDirectory(framesDirectory);
    }

    private static void OnPlayStop() {
        System.IO.Directory.Delete(framesDirectory, true);
    }

#if UNITY_2017_2_OR_NEWER
    private static void OnPlayModeStateChange(PlayModeStateChange state) {
#else
    private static void OnPlayModeStateChange() {
#endif
        bool isPlaying = EditorApplication.isPlaying;
        PlayMode newState = isPlaying ? PlayMode.PLAYING : PlayMode.STOPPED;
        if (_state != newState) {
            if (newState == PlayMode.PLAYING) {
                OnPlayStart();
            } else {
                OnPlayStop();
            }
        }
        _state = newState;
    }
}
