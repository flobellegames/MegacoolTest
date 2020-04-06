using System.IO;
using UnityEditor;
using UnityEngine;

public class MegacoolAndroidManifestEditor : ScriptableObject {
    readonly static FileInfo stringsDotXmlTemplate = new FileInfo(Application.dataPath +
        "/Megacool/Editor/strings.byte");
    readonly static FileInfo stringsDotXml = new FileInfo(Application.dataPath +
        "/Plugins/Android/Megacool/res/values/strings.xml");
    readonly static FileInfo androidManifestTemplate = new FileInfo(Application.dataPath +
        "/Plugins/Android/Megacool/AndroidManifest.byte");
    readonly static FileInfo androidManifest = new FileInfo(Application.dataPath +
        "/Plugins/Android/Megacool/AndroidManifest.xml");

    public static bool IsDefaultApplicationIdentifer() {
        return ApplicationIdentifier == "" || ApplicationIdentifier == "com.Company.ProductName";
    }

    public static void WriteAndroidManifest() {
        string template = ReadAllFileText(androidManifestTemplate);
        string manifestText = string.Format(template, LaunchActivity, ApplicationIdentifier);

        File.WriteAllText(androidManifest.FullName, manifestText);
    }

    public static void WriteStringsDotXML(bool UniversalLinksEnabled,
            string Scheme, System.Uri BaseUrl) {
        CreateStringsDotXmlParentDirectory();

        var template = ReadAllFileText(stringsDotXmlTemplate);
        var stringsDotXmlText = string.Format(template, Scheme, BaseUrl.Host, BaseUrl.AbsolutePath);

        File.WriteAllText(stringsDotXml.FullName, stringsDotXmlText);
    }


    private static string ApplicationIdentifier {
        get {
#if UNITY_5_6_OR_NEWER
            return PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
#else
            return PlayerSettings.bundleIdentifier;
#endif
        }
    }

    private static string LaunchActivity {
        get {
            return MegacoolConfiguration.Instance.GetAndroidLaunchActivity();
        }
    }

    private static string ReadAllFileText(FileInfo file) {
        var ret = "";
        using (StreamReader sr = new StreamReader(file.FullName)) {
            string line;
            while ((line = sr.ReadLine()) != null) {
                ret += line + '\n';
            }
        }
        return ret;
    }

    private static void CreateStringsDotXmlParentDirectory() {
        DirectoryInfo parentDir = stringsDotXml.Directory;
        if (!parentDir.Exists) {
            parentDir.Create();
        }
    }
}
