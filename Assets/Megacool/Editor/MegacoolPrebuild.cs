#if UNITY_5_6_OR_NEWER
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

#if UNITY_2018_1_OR_NEWER
using UnityEditor.Build.Reporting;

class MegacoolPrebuild : IPreprocessBuildWithReport
#else
class MegacoolPrebuild : IPreprocessBuild
#endif
{
    public int callbackOrder { get { return 0; } }

#if UNITY_2018_1_OR_NEWER
    public void OnPreprocessBuild(BuildReport report) {
        BuildTarget target = report.summary.platform;
#else
    public void OnPreprocessBuild(BuildTarget target, string path) {
#endif
#if UNITY_ANDROID || (UNITY_IPHONE || UNITY_IOS)
        string identifierError = appIdentifierSetup() ? "" : "\nThe Megacool app identifier has not been set";
        string appConfigError = appConfigSetup(target) ? "" : "\nThe Megacool " + target.ToString() + " key has not been set";
        string errorMessage = identifierError + appConfigError + "\nGo to 'Megacool->Configuration' to setup configuation and rebuild application";

        if (!appIdentifierSetup() || !appConfigSetup(target)) {
            EditorUtility.DisplayDialog("Megacool Configuration Error", errorMessage, "Ok");
            Debug.LogError("Megacool Configuration Error" + errorMessage);
        }
#endif

#if UNITY_ANDROID
        AndroidSdkVersions minSupportedApiVersion = AndroidSdkVersions.AndroidApiLevel16;
        AndroidSdkVersions apiVersion = PlayerSettings.Android.minSdkVersion;
        if (apiVersion < minSupportedApiVersion) {
            string minApiError = "\nThe application's minimum API version (" + apiVersion + ") is less than the required API version by Megacool (" +
                minSupportedApiVersion + ")\n\nGo to 'Edit->Project Settings->Player' to set the minimum API version and rebuild application";

            EditorUtility.DisplayDialog("Megacool API Error", minApiError, "Ok");
            Debug.LogError("Megacool Configuration Error" + minApiError);
        }
#endif
        MegacoolEditor.WarnIfAndroidAndVulkanEnabled();
    }
      
    private bool appIdentifierSetup() {
        string appIdentifier = MegacoolConfiguration.Instance.appIdentifier;
        return appIdentifier != null && appIdentifier != "";
    }

    private bool appConfigSetup(BuildTarget target) {
        bool configSetupCorrect = false;
        string appConfigAndroid = MegacoolConfiguration.Instance.appConfigAndroid;
        string appConfigIos = MegacoolConfiguration.Instance.appConfigIos;

        switch (target) {
        case BuildTarget.Android:
            configSetupCorrect = appConfigAndroid != null && appConfigAndroid != "";
            break;
        case BuildTarget.iOS:
            configSetupCorrect = appConfigIos != null && appConfigIos != "";
            break;
        }
        return configSetupCorrect;
    }
}
#endif
