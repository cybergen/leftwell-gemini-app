using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using BriLib;

public class CachedAudioManager : Singleton<CachedAudioManager>
{
  [SerializeField] private List<AudioClip> _audioClips = new List<AudioClip>();
  private Dictionary<string, AudioClip> _clipDictionary = new Dictionary<string, AudioClip>();

  private void Awake()
  {
    InitializeDictionary();
    Debug.Log("Cached audio manager initialized");
  }

  private void InitializeDictionary()
  {
    _clipDictionary.Clear();
    foreach (var clip in _audioClips)
    {
      string key = Path.GetFileNameWithoutExtension(clip.name);
      if (!_clipDictionary.ContainsKey(key))
      {
        _clipDictionary.Add(SanitizeFileName(key), clip);
      }
    }
  }

  public void AddAudioClip(string key, AudioClip clip)
  {
    if (!_clipDictionary.ContainsKey(key))
    {
      _clipDictionary[key] = clip;
    }
  }

  public AudioClip GetAudioClip(string key)
  {
    var transformedKey = SanitizeFileName(key);
    if (_clipDictionary.TryGetValue(transformedKey, out AudioClip clip))
    {
      return clip;
    }
    return null;
  }

  public static string SanitizeFileName(string input)
  {
    string invalidChars = @".,'""!_?";
    string invalidRegStr = $"[{Regex.Escape(invalidChars)}]";
    string sanitized = Regex.Replace(input, invalidRegStr, "");
    sanitized = sanitized.Replace(" ", "");
    return sanitized;
  }
}
