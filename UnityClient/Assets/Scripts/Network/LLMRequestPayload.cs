using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LLM.Network
{
  [Serializable]
  public class LLMRequestPayload
  {
    public List<Content> contents;

    public string ToJSON()
    {
      return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.None
      });
    }

    public static LLMRequestPayload GetRequestFromSingleRequestString(string request)
    {
      var payload = new LLMRequestPayload
      {
        contents = new List<Content>
        {
          new Content
          {
            parts = new List<BasePart>
            {
              new TextPart
              {
                text = request
              }
            }
          }
        }
      };
      return payload;
    }

    public static LLMRequestPayload GetRequestWithMedia(string request, string dataBlob, string mimeType)
    {
      var payload = new LLMRequestPayload
      {
        contents = new List<Content>
        {
          new Content
          {
            parts = new List<BasePart>
            {
              new DataPart
              {
                inlineData = new Blob
                {
                  mimeType = mimeType,
                  data = dataBlob
                }
              },
              new TextPart
              {
                text = request
              }
            }
          }
        }
      };
      return payload;
    }

    public static LLMRequestPayload GetRequestWithMultipleParts(params BasePart[] parts)
    {
      var payload = new LLMRequestPayload
      {
        contents = new List<Content>
        {
          new Content
          {
            parts = new List<BasePart>(parts)
          }
        }
      };
      return payload;
    }
  }

  [Serializable]
  public class Content
  {
    public List<BasePart> parts;
  }

  [Serializable]
  [JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
  public abstract class BasePart { }

  [Serializable]
  public class TextPart : BasePart
  {
    public string text;
  }

  [Serializable]
  public class DataPart : BasePart
  {
    public Blob inlineData;
  }

  [Serializable]
  public class FilePart : BasePart
  {
    public FileInfo fileData;
  }

  [Serializable]
  public class Blob
  {
    public string mimeType;
    public string data;
  }

  [Serializable]
  public class FileInfo
  {
    public string mimeType;
    public string fileUri;
  }
}