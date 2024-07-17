using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class ImageRequest
{
  public string sampleImageStyle; //Must be photograph, digital_art, landscape, sketch, watercolor, cyberpunk, or pop_art

  //Only necessary for upscaling
  public ImageGenerationParameters parameters;
  public List<ImageGenerationInstance> instances;

  public string ToJson()
  {
    return JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings
    {
      NullValueHandling = NullValueHandling.Ignore,
      TypeNameHandling = TypeNameHandling.None
    });
  }
}

[Serializable]
public class ImageGenerationInstance
{
  public string prompt;
  public string negativePrompt;
  public string aspectRatio;
  public string personGeneration;
  public string safetySettings;
  public ImageGenerationImage image;
}

[Serializable]
public class ImageGenerationImage
{
  public string bytesBase64Encoded;
}

[Serializable]
public class ImageGenerationParameters
{
  public int sampleCount;
  public string mode;
  public UpscaleConfig upscaleConfig;
}

[Serializable]
public class UpscaleConfig
{
  public string upscaleFactor; //x2 or x4
}

[Serializable]
public class ImageResponse
{
  public List<VisionGenerativeModelResults> predictions;
}

[Serializable]
public class VisionGenerativeModelResults
{
  public string bytesBase64Encoded;
  public string mimeType;
}