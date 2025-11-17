using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class MyScriptEditor : Editor {
    public override void OnInspectorGUI() {
        // This pulls the data from the target(MyScript)
        LevelGenerator script = (LevelGenerator)target;

        // Draw the default inspector fields
        DrawDefaultInspector();

        // Add a button that calls the MyButtonFunction
        if (GUILayout.Button("Generate level")) {
            script.Start();
        }
    }
}
