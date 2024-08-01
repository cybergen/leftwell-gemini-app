using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using BriLib;

public class ImageGenerationManager : Singleton<ImageGenerationManager>
{
  private const string GENERATION_MODEL = "imagen-3.0-generate-001";
  private const string LOCATION = "us-central1";
  private const string PROJECT_ID = "gen-lang-client-0643048200";
  private const string GENERATION_URL = NetworkSettings.PROXY_URL_BASE
    + "api/image/v1/projects/{1}/locations/{2}/publishers/google/models/{3}:predict";
  private const int MAX_RETRIES = 3;
  private const int TIMEOUT_SECONDS = 45;

  public enum ImageGenStatus
  {
    Succeeded,
    SucceededAfterRetry,
    FailedDueToSafetyGuidelines,
    FailedForOtherReason
  }

  public async Task<(List<string> images, ImageGenStatus status)> GetImagesBase64Encoded(
    string prompt, 
    string negativePrompt, 
    int imageCount = 1)
  {
    var url = string.Format(GENERATION_URL, LOCATION, PROJECT_ID, LOCATION, GENERATION_MODEL);
    var request = new ImageRequest
    {
      instances = new List<ImageGenerationInstance>
      {
        new ImageGenerationInstance
        {
          prompt = prompt,
          negativePrompt = negativePrompt,
          aspectRatio = GenerationSettings.IMAGE_GEN_ASPECT,
          personGeneration = GenerationSettings.IMAGE_GEN_PERSON_GEN,
          safetySettings = GenerationSettings.IMAGE_GEN_SAFETY
        }
      },
      parameters = new ImageGenerationParameters
      {
        sampleCount = imageCount
      }
    };

    using (HttpClient client = new HttpClient())
    {
      for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
      {
        try
        {
          client.Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);
          string jsonPayload = request.ToJson();
          Debug.Log($"Making image gen request:\n{jsonPayload}");

          StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
          HttpResponseMessage response = await client.PostAsync(url, content);

          if (response.IsSuccessStatusCode)
          {
            string responseBody = await response.Content.ReadAsStringAsync();

            var reply = JsonUtility.FromJson<ImageResponse>(responseBody);
            if (reply.predictions == null || reply.predictions.Count == 0)
            {
              Debug.LogWarning("Empty response received, retrying...");
              continue; // Retry if empty response
            }

            Debug.Log($"Resolved image gen for prompt: {request.instances[0].prompt}");
            var imageList = new List<string>();
            foreach (var prediction in reply.predictions)
            {
              imageList.Add(prediction.bytesBase64Encoded);
            }
            return (imageList, attempt == 0 ? ImageGenStatus.Succeeded : ImageGenStatus.SucceededAfterRetry);
          }
          else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
          {
            string errorResponse = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Image gen failed with status: {response.StatusCode} and response: {errorResponse}");
            return (null, ImageGenStatus.FailedDueToSafetyGuidelines); //Return bad response so caller can update prompt and retry
          }
          else
          {
            string errorResponse = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Image gen failed with status: {response.StatusCode} and response: {errorResponse}");
            return (null, ImageGenStatus.FailedForOtherReason); //Return bad response so caller can decide what to do
          }
        }
        catch (TaskCanceledException)
        {
          Debug.LogError("Image gen request timed out. Retrying...");
          continue;
        }
        catch (Exception e)
        {
          Debug.LogError($"Image gen failed: {e.Message}");
          return (null, ImageGenStatus.FailedForOtherReason);
        }
      }

      Debug.LogError("Max retries reached. Image gen failed.");
      return (null, ImageGenStatus.FailedForOtherReason);
    }
  }

  public async Task<(Texture2D image, ImageGenStatus status)> GetRandomlyEditedImage(Texture2D initialImage)
  {
    var bytes = initialImage.EncodeToPNG();
    var base64Encoded = Convert.ToBase64String(bytes);
    var editOptions = GenerationSettings.GetRandomEditOptions();

    var url = string.Format(GENERATION_URL, LOCATION, PROJECT_ID, LOCATION, editOptions.Model);
    ImageGenerationParameters parameters = new ImageGenerationParameters();
    if (editOptions.Model == "imagegeneration@006")
    {
      parameters.sampleCount = 1;
      parameters.editConfig = new EditConfig
      {
        editMode = editOptions.EditMode
      };
    }
    else
    {
      parameters.sampleCount = 1;
    }

    var request = new ImageRequest
    {
      instances = new List<ImageGenerationInstance>
      {
        new ImageGenerationInstance
        {
          prompt = editOptions.Prompt,
          negativePrompt = editOptions.NegativePrompt,
          aspectRatio = GenerationSettings.IMAGE_GEN_ASPECT,
          personGeneration = editOptions.PersonGeneration,
          safetySettings = GenerationSettings.IMAGE_GEN_SAFETY,
          image = new ImageGenerationImage
          {
            bytesBase64Encoded = base64Encoded
          }
        }
      },
      parameters = parameters
    };

    using (HttpClient client = new HttpClient())
    {
      for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
      {
        try
        {
          client.Timeout = TimeSpan.FromSeconds(TIMEOUT_SECONDS);
          string jsonPayload = request.ToJson();
          Debug.Log($"Making image edit request prompt: {request.instances[0].prompt} and negative: {request.instances[0].negativePrompt}");

          StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
          HttpResponseMessage response = await client.PostAsync(url, content);

          if (response.IsSuccessStatusCode)
          {
            string responseBody = await response.Content.ReadAsStringAsync();
            var reply = JsonUtility.FromJson<ImageResponse>(responseBody);
            Debug.Log($"Got response for image edit with {reply.predictions.Count} samples");

            if (reply.predictions.Count == 0)
            {
              Debug.LogWarning("Empty response received, retrying...");
              continue;
            }

            Debug.Log($"Resolved image edit for prompt: {request.instances[0].prompt}");
            var imageList = new List<string>();
            foreach (var prediction in reply.predictions)
            {
              imageList.Add(prediction.bytesBase64Encoded);
            }
            return (Base64ToTexture(imageList[0]), attempt == 0 ? ImageGenStatus.Succeeded : ImageGenStatus.SucceededAfterRetry);
          }
          else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
          {
            string errorResponse = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Image edit failed with status: {response.StatusCode} and response: {errorResponse}");
            return (null, ImageGenStatus.FailedDueToSafetyGuidelines); //Let caller know it's a bad prompt immediately
          }
          else
          {
            Debug.LogError($"Image gen failed with status: {response.StatusCode}");
            string errorResponse = await response.Content.ReadAsStringAsync();
            Debug.LogError($"Image gen error response: {errorResponse}");
            return (null, ImageGenStatus.FailedForOtherReason); //Let caller know immediately
          }
        }
        catch (TaskCanceledException)
        {
          Debug.LogError("Image edit request timed out. Retrying...");
          continue;
        }
        catch (Exception e)
        {
          Debug.LogError($"Image edit failed: {e.Message}");
          return (null, ImageGenStatus.FailedForOtherReason);
        }
      }

      Debug.LogError("Max retries reached. Image edit failed.");
      return (null, ImageGenStatus.FailedForOtherReason);
    }
  }

  public static Texture2D Base64ToTexture(string base64String)
  {
    try
    {
      byte[] imageBytes = Convert.FromBase64String(base64String);
      Texture2D texture = new Texture2D(2, 2);
      texture.LoadImage(imageBytes);
      return texture;
    }
    catch (Exception e)
    {
      Debug.LogError("Failed to convert base64 string to texture: " + e.Message);
      return null;
    }
  }
}
