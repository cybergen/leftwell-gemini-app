using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BriLib;
using LLM.Network;
using System.Text.RegularExpressions;
using UnityEngine.Rendering;

public class ImagePromptGenerator : Singleton<ImagePromptGenerator>
{
  public bool ReadyToGenerate { get; private set; } = false;
  public float Progress { get; private set; } = 0f;
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
    var lines = Regex.Split(responseTuple.Item2, @"\r\n|\n|\r");
    foreach (var line in lines)
    {
      if (line.StartsWith(ImagePromptGenSettings.IMAGE_PROMPT_PRECEDENT))
      {
        prompt = line.Substring(ImagePromptGenSettings.IMAGE_PROMPT_PRECEDENT.Length).Trim();
      }
      else if (line.StartsWith(ImagePromptGenSettings.IMAGE_NEGATIVE_PROMPT_PRECEDENT))
      {
        negativePrompt = line.Substring(ImagePromptGenSettings.IMAGE_NEGATIVE_PROMPT_PRECEDENT.Length).Trim();
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
    for (int i = 1; i < 16; i += 4)
    {
      Progress = i / 16f;

      var pathOne = Path.Combine(Application.streamingAssetsPath, i + ".png");
      var pathTwo = Path.Combine(Application.streamingAssetsPath, (i + 1) + ".png");
      var pathThree = Path.Combine(Application.streamingAssetsPath, (i + 2) + ".png");
      var pathFour = Path.Combine(Application.streamingAssetsPath, (i + 3) + ".png");

      var files = await ReadFilesAndUpload(new List<string> { pathOne, pathTwo, pathThree, pathFour });
      foreach (var file in files)
      {
        content.parts.Add(file);
      }
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

  //Execute batches of uploads in parallel during loading phase
  private async Task<List<FilePart>> ReadFilesAndUpload(List<string> files)
  {
    var result = new List<FilePart>();
    var uploadTasks = new List<Task<FilePart>>();

    foreach (var file in files)
    {
      byte[] imageBytes;

      if (Application.platform == RuntimePlatform.Android)
      {
        UnityWebRequest www = UnityWebRequest.Get(file);
        www.SendWebRequest();
        while (!www.isDone) { await Task.Delay(10); }

        if (www.result != UnityWebRequest.Result.Success) 
        {
          Debug.Log($"Did not succeed in file load for android: {file}");
          continue; 
        }
        imageBytes = www.downloadHandler.data;
      }
      else if (Application.platform == RuntimePlatform.IPhonePlayer)
      {
        if (!System.IO.File.Exists(file)) { continue; }
        imageBytes = System.IO.File.ReadAllBytes(file);
      }
      else
      {
        if (!System.IO.File.Exists(file)) { continue; }
        imageBytes = System.IO.File.ReadAllBytes(file);
      }

      var uploadTask = Task.Run(async () =>
      {
        var fileInfo = await FileUploadManager.Instance.UploadFile("image/png", "Prompt guid image", imageBytes);
        return new FilePart
        {
          fileData = new FilePartData
          {
            mimeType = fileInfo.file.mimeType,
            fileUri = fileInfo.file.uri
          }
        };
      });
      uploadTasks.Add(uploadTask);
    }

    var uploadResults = await Task.WhenAll(uploadTasks);
    result.AddRange(uploadResults);

    return result;
  }

}