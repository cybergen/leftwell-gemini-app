using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LLM.Network
{
  [Serializable]
  public class LLMRequestPayload
  {
    public List<Content> contents;
    public List<SafetySetting> safetySettings;
    public Content systemInstruction;
    public GenerationConfig generationConfig;

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
    public string role; //Must be either user or model
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
    public FilePartData fileData;
  }

  [Serializable]
  public class Blob
  {
    public string mimeType;
    public string data;
  }

  [Serializable]
  public class FilePartData
  {
    public string mimeType;
    public string fileUri;
  }

  [Serializable]
  public class GenerationConfig
  {
    public float temperature;
  }

  [Serializable]
  public class SafetySetting
  {
    public string category;
    public string threshold;
  }

  public enum HarmCategory
  {
    HARM_CATEGORY_UNSPECIFIED,
    HARM_CATEGORY_DEROGATORY,
    HARM_CATEGORY_TOXICITY,
    HARM_CATEGORY_VIOLENCE,
    HARM_CATEGORY_SEXUAL,
    HARM_CATEGORY_MEDICAL,
    HARM_CATEGORY_DANGEROUS,
    HARM_CATEGORY_HARASSMENT,
    HARM_CATEGORY_HATE_SPEECH,
    HARM_CATEGORY_SEXUALLY_EXPLICIT,
    HARM_CATEGORY_DANGEROUS_CONTENT
  }

  public enum HarmBlockThreshold
  {
    HARM_BLOCK_THRESHOLD_UNSPECIFIED,
    BLOCK_LOW_AND_ABOVE,
    BLOCK_MEDIUM_AND_ABOVE,
    BLOCK_NONE
  }
}