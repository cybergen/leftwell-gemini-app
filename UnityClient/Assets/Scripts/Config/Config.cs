using System;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using BriLib;

public class Config : Singleton<Config>
{
  //TODO: GET THESE OUT OF HERE OH GOD
  //Create a proxy server to manage client requests instead and get secrets out of client
  public string ApiKey { get; private set; }
  public string OauthToken { get; private set; }

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

    ApiKey = configData.ApiKey;
    OauthToken = configData.OauthToken;
  }

  [Serializable]
  private class EnvData
  {
    public string ApiKey;
    public string OauthToken;
  }
}
