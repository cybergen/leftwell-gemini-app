using UnityEngine;
using UnityEditor;

namespace BriLib
{
  [CustomEditor(typeof(PreferencesManager))]
  public class PreferencesManagerEditor : Editor
  {
    public override void OnInspectorGUI()
    {
      DrawDefaultInspector();
      PreferencesManager manager = (PreferencesManager)target;
      if (GUILayout.Button("Delete Data"))
      {
        manager.DeleteAll();
      }
    }
  }
}