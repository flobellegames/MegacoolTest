using System;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
#endif
[Serializable]
/// <summary>
/// Global configuration of the SDK.
/// </summary>
/// <description>
/// This is the configuration available through the configuration panel in the Editor, but you can also set the fields
/// on the object directly from code if desired. If configured from code it has to be set before the call to
/// Megacool.Start(), after that the SDK can be configured with the individual setters on the Megacool object.
/// </description>
public class MegacoolConfiguration : ScriptableObject {
    public const int DEFAULT_MAX_FRAMES = 50;
    public const int DEFAULT_FRAME_RATE = 10;
    public const int DEFAULT_LAST_FRAME_DELAY = 1000;
    public const double DEFAULT_PEAK_LOCATION = 0.7;
    public const int DEFAULT_PLAYBACK_FRAME_RATE = 12;
    public const MegacoolOverflowStrategy DEFAULT_OVERFLOW_STRATEGY = MegacoolOverflowStrategy.LATEST;
    public const MegacoolSharingStrategy DEFAULT_SHARING_STRATEGY = MegacoolSharingStrategy.LINK;
    // Not exposed in the config panel since it doesn't make much sense, but used by the configs to
    // resolve the default.
    public const string DEFAULT_RECORDING_ID = "";
    public const string DEFAULT_SHARE_MESSAGE = "Check out this game I'm playing!";
    public const string DEFAULT_SHARE_MODAL_TITLE = "Share GIF";
    private static MegacoolConfiguration instance;
    private static readonly string configurationAsset = "MegacoolConfiguration";

    // Configuration properties that should be exposed in the inspector. Ordering
    // and grouping is done by MegacoolEditor.
    public string appIdentifier;

    public string appConfigAndroid;

    public string appConfigIos;

    [ContextMenuItem("Default", "defaultShareMessage")]
    [Tooltip("Set the text to be shared with the GIF")]
    public string sharingText = DEFAULT_SHARE_MESSAGE;

    [ContextMenuItem("Default", "defaultSharingStrategy")]
    [Tooltip("What to prioritize when sharing")]
    public MegacoolSharingStrategy sharingStrategy = DEFAULT_SHARING_STRATEGY;

    [Tooltip("A file to share if there is no recording. Should be a path relative to StreamingAssets.")]
    public string sharingFallback;

    [ContextMenuItem("Default", "defaultModalTitle")]
    [Tooltip("The title to use for the share modal on supported platforms")]
    public string shareModalTitle = DEFAULT_SHARE_MODAL_TITLE;

    public bool universalLinksIOS = true;
    public bool universalLinksAndroid = true;

    [Tooltip("The base url to use when building the absolute share urls. Defaults to " +
        "mgcl.co/<app-identifier>. \n\nNote: Requires additional configuration on the dashboard.")]
    public string baseUrl;

    [Tooltip("Which URL scheme to respond to as fallback where normal links doesn't work")]
    public string schemeIOS;

    [Tooltip("Which URL scheme to respond to as fallback where normal links doesn't work")]
    public string schemeAndroid;

    [Tooltip("Whether to register as a referral receiver in the Android manifest. Needed for referrals to work on Android.")]
    public bool androidReferrals = true;

    [ContextMenuItem("Default", "defaultMaxFrames")]
    [Tooltip("Max number of frames in the buffer.")]
    public int maxFrames = DEFAULT_MAX_FRAMES;

    [ContextMenuItem("Default", "defaultPeakLocation")]
    [Tooltip("Set at what percentage of recording the maximum score should occur.")]
    public double peakLocation = DEFAULT_PEAK_LOCATION;

    [ContextMenuItem("Default", "defaultFrameRate")]
    [Tooltip("Set number of frames per second to record.")]
    public int recordingFrameRate = DEFAULT_FRAME_RATE;

    [ContextMenuItem("Default", "defaultPlaybackFrameRate")]
    [Tooltip("Set playback speed in number of frames per second")]
    public int playbackFrameRate = DEFAULT_PLAYBACK_FRAME_RATE;

    [ContextMenuItem("Default", "defaultLastFrameDelay")]
    [Tooltip("Set a delay (in milliseconds) on the last frame in the animation.")]
    public int lastFrameDelay = DEFAULT_LAST_FRAME_DELAY;

    [Tooltip("Path to an image to overlay on the last frame of the recording, relative to StreamingAssets")]
    public string lastFrameOverlay;

    [Tooltip("Turn on / off debug mode. In debug mode calls to the SDK are stored and can be submitted to the core developers using SubmitDebugData later.")]
    public bool debugMode = false;

    [Tooltip("The default is com.unity3d.player.UnityPlayerActivity, leave blank to use that.")]
    public string customAndroidActivity = null;

    private void defaultShareMessage() {
        sharingText = DEFAULT_SHARE_MESSAGE;
    }

    private void defaultSharingStrategy() {
        sharingStrategy = DEFAULT_SHARING_STRATEGY;
    }

    private void defaultModalTitle() {
        shareModalTitle = DEFAULT_SHARE_MODAL_TITLE;
    }

    private void defaultMaxFrames() {
        maxFrames = DEFAULT_MAX_FRAMES;
    }

    private void defaultPeakLocation() {
        peakLocation = DEFAULT_PEAK_LOCATION;
    }

    private void defaultFrameRate() {
        recordingFrameRate = DEFAULT_FRAME_RATE;
    }

    private void defaultPlaybackFrameRate() {
        playbackFrameRate = DEFAULT_PLAYBACK_FRAME_RATE;
    }

    private void defaultLastFrameDelay() {
        lastFrameDelay = DEFAULT_LAST_FRAME_DELAY;
    }

    public static MegacoolConfiguration Instance {
        get {
            if (instance == null) {
                LoadInstance();
            }
            return instance;
        }
    }


    public string GetAndroidLaunchActivity () {
        if (!string.IsNullOrEmpty(customAndroidActivity)) {
            return customAndroidActivity;
        }
        return "com.unity3d.player.UnityPlayerActivity";
    }

    public Uri CustomBaseUrlOrDefault() {
        var url = CustomBaseUrl();
        if (url != null) {
            return new Uri(url);
        }
        return new Uri("https://mgcl.co/" + appIdentifier);
    }

    public string CustomBaseUrl() {
        if (string.IsNullOrEmpty(baseUrl)) {
            return null;
        }
        string url = baseUrl;
        if (!(url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))) {
            url = "https://" + url;
        }
        return url;
    }

    private static void LoadInstance() {
        instance = Resources.Load(configurationAsset) as MegacoolConfiguration;
        if (instance == null) {
            instance = CreateInstance<MegacoolConfiguration>();

#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Megacool")) {
                AssetDatabase.CreateFolder("Assets", "Megacool");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Megacool/Resources")) {
                AssetDatabase.CreateFolder("Assets/Megacool", "Resources");
            }
            string configurationAssetPath = Path.Combine("Assets/Megacool/Resources", configurationAsset + ".asset");
            AssetDatabase.CreateAsset(instance, configurationAssetPath);
#endif
        }
    }
}
