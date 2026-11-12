using Cainos.PixelArtTopDown_Basic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[CustomEditor(typeof(SteleServicePopupUI))]
public sealed class SteleServicePopupUIEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Popup Preview", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Preview one popup in the Scene view. Switching previews hides the previously shown popup.", MessageType.Info);

        var popup = (SteleServicePopupUI)target;
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Combat")) Preview(popup, "Combat");
            if (GUILayout.Button("Forge")) Preview(popup, "Forge");
            if (GUILayout.Button("Magic")) Preview(popup, "Magic");
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Trade")) Preview(popup, "Trade");
            if (GUILayout.Button("Quest")) Preview(popup, "Quest");
            if (GUILayout.Button("Memorial")) Preview(popup, "Memorial");
        }
        if (GUILayout.Button("Hide popup preview"))
        {
            Undo.RecordObject(popup, "Hide popup preview");
            popup.HidePopupPreviews();
            EditorSceneManager.MarkSceneDirty(popup.gameObject.scene);
        }
    }

    static void Preview(SteleServicePopupUI popup, string service)
    {
        Undo.RecordObject(popup, "Preview " + service + " popup");
        popup.PreviewPopupCanvas(service);
        EditorSceneManager.MarkSceneDirty(popup.gameObject.scene);
        SceneView.RepaintAll();
    }
}
