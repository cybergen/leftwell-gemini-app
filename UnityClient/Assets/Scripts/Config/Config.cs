using System.IO;
using System;
using UnityEngine;
using BriLib;
using UnityEngine.Networking;

public class Config : Singleton<Config>
{
  public string ApiKey { get; private set; }

  public override void OnCreate()
  {
    base.OnCreate();
    LoadEnv();
  }

  private void LoadEnv()
  {
    string envPath = Path.Combine(Application.streamingAssetsPath, "env.json");
    string envText;
    if (Application.platform == RuntimePlatform.Android)
    {
      UnityWebRequest www = UnityWebRequest.Get(envPath);
      www.SendWebRequest();
      while (!www.isDone);
      envText = www.downloadHandler.text;
    }
    else
    {
      envText = File.ReadAllText(envPath);
    }
    var configData = JsonUtility.FromJson<EnvData>(envText);

    // TODO: Eliminate API keys from build entirely by moving to proxy server for all
    // credentialed calls to API's
    ApiKey = configData.ApiKey;
  }

  [Serializable]
  private class EnvData
  {
    public string ApiKey;
  }
}
