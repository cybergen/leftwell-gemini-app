using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class AudioClipDownloader : EditorWindow
{
  private string inputText = "";
  private List<string> lines = new List<string>();
  private bool isDownloading = false;

  [MenuItem("Tools/AudioClip Downloader")]
  public static void ShowWindow()
  {
    GetWindow<AudioClipDownloader>("AudioClip Downloader");
  }

  private void OnGUI()
  {
    GUILayout.Label("Paste the list of strings below:", EditorStyles.boldLabel);
    inputText = EditorGUILayout.TextArea(inputText, GUILayout.Height(100));

    if (GUILayout.Button("Download and Save AudioClips") && !isDownloading)
    {
      lines = new List<string>(inputText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries));
      isDownloading = true;
      DownloadAndSaveAudioClips();
    }

    if (isDownloading)
    {
      GUILayout.Label("Downloading...", EditorStyles.boldLabel);
    }
  }

  private async void DownloadAndSaveAudioClips()
  {
    string folderPath = "Assets/Sound/VO";
    if (!Directory.Exists(folderPath))
    {
      Directory.CreateDirectory(folderPath);
    }

    for (int i = 0; i < lines.Count; i++)
    {
      string line = lines[i];
      AudioClip audioClip = await SpeechManager.Instance.GetAudioClipFromText(line);

      if (audioClip != null)
      {
        string sanitizedFileName = CachedAudioManager.SanitizeFileName(line) + ".wav";
        string filePath = Path.Combine(folderPath, sanitizedFileName);
        SaveAudioClipToFile(audioClip, filePath);
        Debug.Log($"Saved: {filePath}");
      }
      else
      {
        Debug.LogError($"Failed to generate AudioClip for text: {line}");
      }
    }

    AssetDatabase.Refresh();
    isDownloading = false;
  }

  private void SaveAudioClipToFile(AudioClip clip, string filePath)
  {
    if (clip == null) return;

    var samples = new float[clip.samples * clip.channels];
    clip.GetData(samples, 0);

    var file = new FileStream(filePath, FileMode.Create);
    var writer = new BinaryWriter(file);

    int headerSize = 44; // default for uncompressed wav
    int fileSize = samples.Length * 2 + headerSize;

    writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
    writer.Write(fileSize);
    writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
    writer.Write(new char[4] { 'f', 'm', 't', ' ' });
    writer.Write(16);
    writer.Write((short)1);
    writer.Write((short)clip.channels);
    writer.Write(clip.frequency);
    writer.Write(clip.frequency * clip.channels * 2);
    writer.Write((short)(clip.channels * 2));
    writer.Write((short)16);

    writer.Write(new char[4] { 'd', 'a', 't', 'a' });
    writer.Write(samples.Length * 2);

    int rescaleFactor = 32767; // to convert float to Int16

    for (int i = 0; i < samples.Length; i++)
    {
      writer.Write((short)(samples[i] * rescaleFactor));
    }

    writer.Close();
    file.Close();
  }
}