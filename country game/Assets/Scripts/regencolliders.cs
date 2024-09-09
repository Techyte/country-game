using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class regencolliders : MonoBehaviour
{
    public void Regenerate()
    {
        List<PixelCollider2D> collider2D = FindObjectsOfType<PixelCollider2D>().ToList();

        foreach (var col in collider2D)
        {
            col.Regenerate();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(regencolliders))]
public class regencollidersEditor : Editor
{
    // Rendering code for the PixelCollider2D custom inspector
    public override void OnInspectorGUI()
    {
        regencolliders regencolliders = (regencolliders)target;
        if (GUILayout.Button("Regenerate Collider"))
        {
            regencolliders.Regenerate();
        }
    }
}
#endif