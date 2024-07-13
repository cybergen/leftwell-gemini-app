using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLM.Network
{
  [Serializable]
  public class FilePayload
  {
    public File file;

    public string ToJSON()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.None
      });
    }

    public static FilePayload FromJSON(string json)
    {
      return JsonUtility.FromJson<FilePayload>(json);
    }
  }

  [Serializable]
  public class File
  {
    public string name; // up to 40 characters, lowercase alphanumeric or dashes, can't start/end with dash
    public string displayName; // optional
    public string mimeType; // Output only
    public string sizeBytes; // Output only. int64 format
    public string createTime; // Output only. Timestamp format - 2014-10-02T15:01:23Z
    public string updateTime; // Output only. Timestamp format - 2014-10-02T15:01:23.045123456Z
    public string expirationTime; // Output only. Timestamp format
    public string sha256Hash; // Output only. base64 encoded string
    public string uri; // Output only
    public string state; // Output only. Can be one of:
                         // STATE_UNSPECIFIED
                         // PROCESSING
                         // ACTIVE
                         // FAILED
    public Status error; // Output only

    // Union field metadata can be only one of the following:
    public VideoMetadata videoMetadata;
  }

  [Serializable]
  public class VideoMetadata
  {
    public string videoDuration;
  }

  [Serializable]
  public class Status
  {
    public int code;
    public string message;
    public List<Detail> details;
  }

  [Serializable]
  public class Detail
  {
    public string @type;
  }
}