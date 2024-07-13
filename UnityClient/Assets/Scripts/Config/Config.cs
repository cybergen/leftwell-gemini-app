using System.IO;
using System;
using UnityEngine;
using BriLib;

public class Config : Singleton<Config>
{
  public string ApiKey { get; private set; }

  public override void Begin()
  {
    base.Begin();
    LoadEnv();
  }

  private void LoadEnv()
  {
    string envPath = Path.Combine(Application.streamingAssetsPath, "env.json");
    var configData = JsonUtility.FromJson<EnvData>(File.ReadAllText(envPath));

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
