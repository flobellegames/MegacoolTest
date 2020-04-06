using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
class Startup {
    static Startup() {
#if UNITY_ANDROID
        AndroidSdkVersions minSupportedApiVersion = AndroidSdkVersions.AndroidApiLevel16;
        AndroidSdkVersions apiVersion = PlayerSettings.Android.minSdkVersion;
        if (apiVersion < minSupportedApiVersion) {
            string minApiError = "The application's minimum API version (" + apiVersion + ") is less than the required API version by Megacool (" +
                minSupportedApiVersion + ")";

            if (EditorUtility.DisplayDialog("Megacool API", minApiError,
                "Change to " + minSupportedApiVersion, "Cancel")) {
                    PlayerSettings.Android.minSdkVersion = minSupportedApiVersion;
            }
        }
        MegacoolEditor.WarnIfAndroidAndVulkanEnabled();
#endif
    }
}

[CustomEditor(typeof(MegacoolConfiguration))]
public class MegacoolEditor : Editor {
    private SerializedObject serializedConfiguration;
    private SerializedProperty serializedAppIdentifier;
    private SerializedProperty serializedAppConfigAndroid;
    private SerializedProperty serializedAppConfigIos;
    private SerializedProperty serializedShareMessage;
    private SerializedProperty serializedSharingStrategy;
    private SerializedProperty serializedSharingFallback;
    private SerializedProperty serializedShareModalTitle;
    private SerializedProperty serializedBaseUrl;
    private SerializedProperty serializedSchemeIOS;
    private SerializedProperty serializedSchemeAndroid;
    private SerializedProperty serializedMaxFrames;
    private SerializedProperty serializedPeakLocation;
    private SerializedProperty serializedRecordingFrameRate;
    private SerializedProperty serializedPlaybackFrameRate;
    private SerializedProperty serializedLastFrameDelay;
    private SerializedProperty serializedLastFrameOverlay;
    private SerializedProperty serializedUniversalLinksIOS;
    private SerializedProperty serializedUniversalLinksAndroid;
    private SerializedProperty serializedAndroidReferrals;
    private SerializedProperty serializedCustomAndroidActivity;
    readonly static string unsetBundleIdentifierWarning = "The Android Bundle Identifier seems to be unset, " +
            "ensure it's set in the Android Player Settings. This is necessary for GIF sharing " +
            "to work.";
    readonly static string vulkanEnabledWarning = "Your current player settings for Android " +
        "enables Vulkan, but capture isn't supported for Vulkan yet. If you want to include " +
        "recordings with your shares, go to Edit>Project Settings>Player, and in the Android " +
        "tab unselect it from the graphics API list (unselect \"Auto graphics API\" first) and " +
        "use ES3 or ES2 instead";

    void OnEnable () {
        serializedConfiguration = new SerializedObject(MegacoolConfiguration.Instance);
        serializedAppIdentifier = serializedConfiguration.FindProperty("appIdentifier");
        serializedAppConfigAndroid = serializedConfiguration.FindProperty("appConfigAndroid");
        serializedAppConfigIos = serializedConfiguration.FindProperty("appConfigIos");
        serializedShareMessage = serializedConfiguration.FindProperty("sharingText");
        serializedSharingStrategy = serializedConfiguration.FindProperty("sharingStrategy");
        serializedSharingFallback = serializedConfiguration.FindProperty("sharingFallback");
        serializedShareModalTitle = serializedConfiguration.FindProperty("shareModalTitle");
        serializedBaseUrl = serializedConfiguration.FindProperty("baseUrl");
        serializedSchemeIOS = serializedConfiguration.FindProperty("schemeIOS");
        serializedSchemeAndroid = serializedConfiguration.FindProperty("schemeAndroid");
        serializedMaxFrames = serializedConfiguration.FindProperty("maxFrames");
        serializedPeakLocation = serializedConfiguration.FindProperty("peakLocation");
        serializedRecordingFrameRate = serializedConfiguration.FindProperty("recordingFrameRate");
        serializedPlaybackFrameRate = serializedConfiguration.FindProperty("playbackFrameRate");
        serializedLastFrameDelay = serializedConfiguration.FindProperty("lastFrameDelay");
        serializedLastFrameOverlay = serializedConfiguration.FindProperty("lastFrameOverlay");
        serializedUniversalLinksIOS = serializedConfiguration.FindProperty("universalLinksIOS");
        serializedUniversalLinksAndroid = serializedConfiguration.FindProperty("universalLinksAndroid");
        serializedAndroidReferrals = serializedConfiguration.FindProperty("androidReferrals");
        serializedCustomAndroidActivity = serializedConfiguration.FindProperty("customAndroidActivity");
    }

    public override void OnInspectorGUI() {
        serializedConfiguration.Update();

        addWarning("Remember to click \"Save changes\" below to persist any changes");

        if (!string.IsNullOrEmpty(serializedAppConfigAndroid.stringValue) && IsVulkanEnabled()) {
            addWarning(vulkanEnabledWarning);
        }

        addHeader("Core config");
        addProperty(serializedAppIdentifier, "App identifier");
        addHelpBox("Required field. This is the identifier you picked for your app when creating it on the " +
            "dashboard.");

        addProperty(serializedAppConfigAndroid, "Android key");
        addProperty(serializedAppConfigIos, "iOS key");
        addHelpBox("The keys above are required for the platforms you develop for. You can find " +
            "them on your dashboard at https://dashboard.megacool.co");

        addHeader("Linking");
        addProperty(serializedBaseUrl, "Custom base url");
        addLabel("Scheme");
        addProperty(serializedSchemeIOS, "    iOS");
        addProperty(serializedSchemeAndroid, "    Android");
        addProperty(serializedAndroidReferrals, "Android referrals");
        addLabel("Universal linking");
        addProperty(serializedUniversalLinksIOS, "    iOS");
        addProperty(serializedUniversalLinksAndroid, "    Android");

        addHelpBox("The properties below can also be set programmatically, see the documentation " +
                   "for details");
        addHeader("Recording");
        addProperty(serializedLastFrameDelay, "Last frame delay (ms)");
        addProperty(serializedLastFrameOverlay, "Last frame overlay");
        addProperty(serializedRecordingFrameRate, "Recording frame rate");
        addProperty(serializedPlaybackFrameRate, "Playback frame rate");
        addProperty(serializedMaxFrames, "Max frames");
        addProperty(serializedPeakLocation, "Peak location");

        addHeader("Shares");
        addProperty(serializedShareMessage, "Message");
        addProperty(serializedSharingStrategy, "Strategy");
        addProperty(serializedSharingFallback, "Fallback media");
        addProperty(serializedShareModalTitle, "Modal title");

        addHeader("Advanced");
        addHelpBox("If you've customized the Android launch activity elsewhere, set the name of " +
                   "the new activity here so that the manifests get merged correctly. This is " +
                   "necessary if you're for example using Firebase Cloud Messaging, in which " +
                   "case you should set this to " +
                   "\"com.google.firebase.MessagingUnityPlayerActivity\".");
        addProperty(serializedCustomAndroidActivity, "Custom Android activity");

        if (MegacoolAndroidManifestEditor.IsDefaultApplicationIdentifer()) {
            // Fail hard if this is unset since otherwise it'll build successfully but only fail once a share is
            // attempted, making it easy to ship something broken.
            addWarning(unsetBundleIdentifierWarning);
        }

        if (GUILayout.Button("Save changes")) {
            serializedConfiguration.ApplyModifiedProperties();

            if (MegacoolAndroidManifestEditor.IsDefaultApplicationIdentifer()) {
                // Fail hard if this is unset since otherwise it'll build successfully but only fail once a share is
                // attempted, making it easy to ship something broken.
                Debug.LogError(unsetBundleIdentifierWarning);
                return;
            }

            MegacoolAndroidManifestEditor.WriteAndroidManifest();

            MegacoolAndroidManifestEditor.WriteStringsDotXML(
                serializedUniversalLinksAndroid.boolValue,
                serializedSchemeAndroid.stringValue,
                MegacoolConfiguration.Instance.CustomBaseUrlOrDefault()
            );

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Megacool configuration saved! Remember to commit the resulting AndroidManifest.xml, " +
                "strings.xml and the MegacoolConfiguration.asset!");
        }

        if (GUI.changed) {
            serializedConfiguration.ApplyModifiedProperties();
        }
    }

    [MenuItem("Window/Megacool/Configuration")]
    public static void GenerateMegacoolConfiguration() {
        MegacoolConfiguration configuration = MegacoolConfiguration.Instance;
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = configuration;
    }

    [MenuItem("Window/Megacool/Quickstart")]
    public static void OpenQuickstart() {
        Application.OpenURL("https://docs.megacool.co/quickstart/unity");
    }

    [MenuItem("Window/Megacool/SDK docs")]
    public static void OpenDocs() {
        Application.OpenURL("https://docs.megacool.co/sdk/unity/5.0.2/");
    }

    private void addProperty(SerializedProperty serializedProperty, string label) {
        EditorGUILayout.PropertyField(serializedProperty, new GUIContent(label), new GUILayoutOption[]{});
    }

    private void addHeader(string header) {
        EditorGUILayout.LabelField(header, EditorStyles.boldLabel, new GUILayoutOption[]{});
    }

    private void addLabel(string label) {
        EditorGUILayout.LabelField(label, new GUILayoutOption[]{});
    }

    private void addHelpBox(string helpText) {
        EditorGUILayout.HelpBox(helpText, MessageType.Info);
    }

    private void addWarning(string warning) {
        EditorGUILayout.HelpBox(warning, MessageType.Warning);
    }

    public static void WarnIfAndroidAndVulkanEnabled() {
#if UNITY_ANDROID
        bool isVulkanEnabled = IsVulkanEnabled();
        if (isVulkanEnabled) {
            Debug.LogWarning("Megacool: " + vulkanEnabledWarning);
        }
#endif
    }

    static bool IsVulkanEnabled() {
        bool isVulkanEnabled = false;

        GraphicsDeviceType[] enabledAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
        foreach (GraphicsDeviceType deviceType in enabledAPIs) {
            if (deviceType == GraphicsDeviceType.Vulkan) {
                isVulkanEnabled = true;
            }
        }
#if UNITY_2019_1_OR_NEWER
        if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android)) {
            isVulkanEnabled = true;
        }
#endif
        return isVulkanEnabled;
    }
}
