using UnityEngine;

namespace BriLib
{
  public class PreferencesManager : Singleton<PreferencesManager>
  {
    public bool GetBool(string key)
    {
      return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public void SetBool(string key, bool value)
    {
      PlayerPrefs.SetInt(key, value ? 1 : 0);
      PlayerPrefs.Save();
    }

    public string GetString(string key)
    {
      return PlayerPrefs.GetString(key);
    }

    public void SetString(string key, string value)
    {
      PlayerPrefs.SetString(key, value);
      PlayerPrefs.Save();
    }

    public void DeleteAll()
    {
      LogManager.Info("Deleting preferences data");
      PlayerPrefs.DeleteAll();
      PlayerPrefs.Save();
    }
  }
}