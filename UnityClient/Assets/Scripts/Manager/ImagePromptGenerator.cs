using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BriLib;
using LLM.Network;

public class ImagePromptGenerator : Singleton<ImagePromptGenerator>
{
  public bool ReadyToGenerate { get; private set; } = false;
  private List<Content> _promptPriming;

  public void Initialize()
  {
    _ = PrimePrompt();
  }

  public async Task<Tuple<string, string>> GetPromptAndNegativePrompt(LLMRequestPayload chat)
  {
    if (!ReadyToGenerate)
    {
      Debug.LogError($"Image prompt generator is not yet ready");
      return null;
    }

    //Get a new request
    var payload = GetBasePayload();
    payload.contents = new List<Content>(_promptPriming);

    //Add all the contents of adventure sequence one by one, replacing role with user
    foreach (var content in chat.contents)
    {
      if (content.parts == null || content.parts.Count == 0) continue;
      payload.contents.Add(new Content
      {
        parts = new List<BasePart>(content.parts),
        role = "user"
      });
    }

    //Then make request
    var responseTuple = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);

    //Parse out and return prompt strings
    string prompt = string.Empty;
    string negativePrompt = string.Empty;
    var lines = responseTuple.Item2.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
    foreach (var line in lines)
    {
      if (line.StartsWith("Prompt:"))
      {
        prompt = line.Substring("Prompt:".Length).Trim();
      }
      else if (line.StartsWith("Negative Prompt:"))
      {
        negativePrompt = line.Substring("Negative Prompt:".Length).Trim();
      }
    }

    return new Tuple<string, string>(prompt, negativePrompt);
  }

  private LLMRequestPayload GetBasePayload()
  {
    //Create a payload with system instructions and basic settings
    var request = new LLMRequestPayload();
    request.generationConfig = new GenerationConfig
    {
      temperature = ImagePromptGenSettings.IMAGE_TEMP
    };
    request.safetySettings = new List<SafetySetting>();
    var relevantSettings = new List<HarmCategory>
    {
      HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT,
      HarmCategory.HARM_CATEGORY_HATE_SPEECH,
      HarmCategory.HARM_CATEGORY_HARASSMENT,
      HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT
    };
    foreach (var category in relevantSettings)
    {
      request.safetySettings.Add(new SafetySetting
      {
        category = Enum.GetName(typeof(HarmCategory), category),
        threshold = Enum.GetName(typeof(HarmBlockThreshold), HarmBlockThreshold.BLOCK_NONE)
      });
    }
    request.systemInstruction = new Content
    {
      parts = new List<BasePart> { new TextPart { text = ImagePromptGenSettings.IMAGE_GEN_PROMPT } }
    };
    return request;
  }

  private async Task PrimePrompt()
  {
    var request = GetBasePayload();
    var content = new Content
    {
      parts = new List<BasePart>(),
      role = "user"
    };
    var contentSet = new List<Content> { content };
    request.contents = contentSet;

    //Upload all images in the prompting guide
    for (int i = 1; i < 16; i++)
    {
      var path = Path.Combine(Application.streamingAssetsPath, i + ".png");
      byte[] imageBytes;
      if (Application.platform == RuntimePlatform.Android)
      {
        UnityWebRequest www = UnityWebRequest.Get(path);
        www.SendWebRequest();
        while (!www.isDone) { await Task.Delay(10); }
        imageBytes = www.downloadHandler.data;
      }
      else
      {
        imageBytes = System.IO.File.ReadAllBytes(path);
      }

      var fileInfo = await FileUploadManager.Instance.UploadFile("image/png", $"guide {i}", imageBytes);
      content.parts.Add(new FilePart
      {
        fileData = new FilePartData
        {
          mimeType = fileInfo.file.mimeType,
          fileUri = fileInfo.file.uri
        }
      });
    }

    //Prime the LLM with the guide
    content.parts.Add(new TextPart
    {
      text = "Prompt guide"
    });

    //Add a faked system reply, then persist primed content set    
    _promptPriming = new List<Content>
    {
      content,
      new Content
      {
        role = "model",
        parts = new List<BasePart>
        {
          new TextPart { text = "Ready to generate prompt!" }
        }
      }
    };
    ReadyToGenerate = true;
    Debug.Log($"Image prompt generator is ready for use");
  }
}