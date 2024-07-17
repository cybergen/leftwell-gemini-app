using UnityEngine;
using BriLib;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System;

public class ImageGenerationManager : Singleton<ImageGenerationManager>
{
  private const string GENERATION_MODEL = "imagegeneration@006";
  private const string UPSCALE_MODEL = "imagegeneration@002";
  private const string LOCATION = "us-central1";
  private const string PROJECT_ID = "gen-lang-client-0643048200";
  private const string GENERATION_URL 
    = "https://{0}-aiplatform.googleapis.com/v1/projects/{1}/locations/{2}/publishers/google/models/{3}:predict";
  private const string UPSCALE_URL
    = "https://{0}-aiplatform.googleapis.com/v1/projects/{1}/locations/{2}/publishers/google/models/{3}:predict";

  public async Task<List<string>> GetImagesBase64Encoded(string prompt, string negativePrompt)
  {
    var url = string.Format(GENERATION_URL, LOCATION, PROJECT_ID, LOCATION, GENERATION_MODEL);
    Debug.Log($"Got url: {url}");
    var request = new ImageRequest
    {
      instances = new List<ImageGenerationInstance>
      {
        new ImageGenerationInstance
        {
          prompt = prompt,
          negativePrompt = negativePrompt,
          aspectRatio = Constants.IMAGE_GEN_ASPECT,
          personGeneration = Constants.IMAGE_GEN_PERSON_GEN,
          safetySettings = Constants.IMAGE_GEN_SAFETY
        }
      },
      parameters = new ImageGenerationParameters
      {        
        sampleCount = Constants.IMAGE_GEN_SAMPLES
      }
    };

    using (HttpClient client = new HttpClient())
    {
      try
      {
        string jsonPayload = request.ToJson();
        Debug.Log($"Making image gen request:\n{jsonPayload}");

        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Config.Instance.OauthToken);
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        Debug.Log($"Created body");
        HttpResponseMessage response = await client.PostAsync(url, content);
        Debug.Log($"Got response");

        // Check if the response is successful
        if (response.IsSuccessStatusCode)
        {
          // Read the response content
          string responseBody = await response.Content.ReadAsStringAsync();
          Debug.Log($"Image gen response body {responseBody}");

          // Deserialize the response JSON to a ReplyObject and return image as base64 string
          var reply = JsonUtility.FromJson<ImageResponse>(responseBody);
          var imageList = new List<string>();
          foreach (var prediction in reply.predictions)
          {
            imageList.Add(prediction.bytesBase64Encoded);
          }
          return imageList;
        }
        else
        {
          Debug.LogError($"Image gen failed with status: {response.StatusCode}");
          string errorResponse = await response.Content.ReadAsStringAsync();
          Debug.LogError($"Error response: {errorResponse}");
          return null;
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"Image gen failed: {e.Message}");
        return null;
      }
    }
  }

  public async Task<string> UpscaleImageBase64Ecoded(string base64EncodedImage, string prompt)
  {
    var url = string.Format(UPSCALE_URL, LOCATION, PROJECT_ID, LOCATION, UPSCALE_MODEL);
    var request = new ImageRequest
    {
      instances = new List<ImageGenerationInstance>
      {
        new ImageGenerationInstance
        {
          prompt = prompt,
          image = new ImageGenerationImage
          {
            bytesBase64Encoded = base64EncodedImage
          }
        }
      },
      parameters = new ImageGenerationParameters
      {
        sampleCount = 1,
        mode = "upscale",
        upscaleConfig = new UpscaleConfig
        {
          upscaleFactor = "x2"
        }
      }
    };

    using (HttpClient client = new HttpClient())
    {
      try
      {
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + Config.Instance.OauthToken);
        string jsonPayload = request.ToJson();
        Debug.Log($"Making image upscale request:\n{jsonPayload}");
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync(url, content);

        // Check if the response is successful
        if (response.IsSuccessStatusCode)
        {
          // Read the response content
          string responseBody = await response.Content.ReadAsStringAsync();
          Debug.Log($"Image response body {responseBody}");

          // Deserialize the response JSON to a ReplyObject and return image as base64 string
          var reply = JsonUtility.FromJson<ImageResponse>(responseBody);
          return reply.predictions[0].bytesBase64Encoded;
        }
        else
        {
          Debug.LogError($"Image upscale failed with status: {response.StatusCode}");
          string errorResponse = await response.Content.ReadAsStringAsync();
          Debug.LogError($"Error response: {errorResponse}");
          return null;
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"Image upscale failed: {e.Message}");
        return null;
      }
    }
  }
}