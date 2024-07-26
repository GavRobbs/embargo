using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameMap))]
public class GameMapEditor : Editor
{
    /* In previous prototypes, since I was so keen to get the generation working, the map was generated from the array stored in the GameMap class each time.
     The results were fairly consistent, but they weren't viewable in the editor. This made it hard to place lighting and other stuff such as the main base.
    By creating this editor plugin, I've made things more deterministic.*/
    public override void OnInspectorGUI()
    {
        GameMap map = (GameMap)target;
        base.OnInspectorGUI();

        if(GUILayout.Button("Regenerate Map"))
        {
            map.Regenerate();
            EditorUtility.SetDirty(map);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(map.gameObject.scene);
        }
    }
}
