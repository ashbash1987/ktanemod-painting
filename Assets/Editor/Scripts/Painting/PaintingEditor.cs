using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Painting))]
public class PaintingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Repaint"))
        {
            ((Painting)target).Repaint();
        }
    }
}
