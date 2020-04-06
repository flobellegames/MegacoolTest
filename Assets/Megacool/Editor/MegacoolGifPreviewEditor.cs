using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[CustomEditor(typeof(MegacoolGifPreview))]
public class MegacoolGifPreviewEditor : ImageEditor {

    public override void OnInspectorGUI() {
        if (GUI.changed) {
            EditorUtility.SetDirty(target);
        }
    }

    protected override void OnEnable() {

    }

    protected override void OnDisable() {

    }
}
