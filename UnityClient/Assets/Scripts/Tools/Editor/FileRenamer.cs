using System.IO;
using UnityEngine;
using UnityEditor;

public class FileRenamerEditor : EditorWindow
{
  private string folderPath = "";

  [MenuItem("Tools/File Renamer")]
  public static void ShowWindow()
  {
    GetWindow<FileRenamerEditor>("File Renamer");
  }

  private void OnGUI()
  {
    GUILayout.Label("Select Folder to Rename Files", EditorStyles.boldLabel);

    if (GUILayout.Button("Select Folder"))
    {
      folderPath = EditorUtility.OpenFolderPanel("Select Folder", "", "");
    }

    GUILayout.Label("Selected Folder:", EditorStyles.label);
    GUILayout.TextField(folderPath);

    if (!string.IsNullOrEmpty(folderPath))
    {
      if (GUILayout.Button("Rename Files"))
      {
        RenameFilesInFolder(folderPath);
      }
    }
  }

  private void RenameFilesInFolder(string folderPath)
  {
    var files = Directory.GetFiles(folderPath);

    foreach (var filePath in files)
    {
      string fileName = Path.GetFileName(filePath);
      string sanitizedFileName 
        = CachedAudioManager.SanitizeFileName(Path.GetFileNameWithoutExtension(fileName)) + Path.GetExtension(fileName);
      string newFilePath = Path.Combine(folderPath, sanitizedFileName);

      if (filePath != newFilePath)
      {
        File.Move(filePath, newFilePath);
        Debug.Log($"Renamed {fileName} to {sanitizedFileName}");
      }
    }

    AssetDatabase.Refresh();
  }
}