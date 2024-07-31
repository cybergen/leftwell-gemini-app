using System;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using BriLib;
using LLM.Network;

public class LLMInteractionManager : Singleton<LLMInteractionManager>
{
  private const string MODEL = "gemini-1.5-pro";
  private const string INFERENCE_URL = NetworkSettings.PROXY_URL_BASE + "api/llm/models/{0}:generateContent";
  private const string STREAM_URL = NetworkSettings.PROXY_URL_BASE + "api/llm/models/{0}:generateContent";

  public async Task<LLMTextResponse> RequestLLMCompletion(LLMRequestPayload request)
  {
    using (HttpClient client = new HttpClient())
    {
      try
      {
        string jsonPayload = request.ToJSON();
        var lastContent = request.contents[request.contents.Count - 1];
        var lastPart = lastContent.parts[lastContent.parts.Count - 1];
        Debug.Log($"Requesting generation:\n{JsonUtility.ToJson(lastPart)}");
        StringContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        string urlWithKey = string.Format(INFERENCE_URL, MODEL);
        HttpResponseMessage response = await client.PostAsync(urlWithKey, content);

        if (response.IsSuccessStatusCode)
        {
          string responseBody = await response.Content.ReadAsStringAsync();
          var reply = LLMTextResponse.FromJson(responseBody);
          Debug.Log($"Got generation response {JsonUtility.ToJson(reply.candidates[0])}");
          return reply;
        }
        else
        {
          Debug.LogError($"RequestLLMCompletion failed with status code: {response.StatusCode}");
          string errorResponse = await response.Content.ReadAsStringAsync();
          Debug.LogError($"Error response: {errorResponse}");
          return null;
        }
      }
      catch (Exception e)
      {
        Debug.LogError($"RequestLLMCompletion failed: {e.Message}");
        return null;
      }
    }
  }

  public async void RequestLLMCompletionStream(LLMRequestPayload request, Action<string> onTextResponseUpdated)
  {
    Debug.Log($"Sending LLM stream request");
    string jsonPayload = request.ToJSON();
    byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonPayload);
    string urlWithKey = string.Format(STREAM_URL, MODEL);

    using (UnityWebRequest webRequest = new UnityWebRequest(urlWithKey, UnityWebRequest.kHttpVerbPOST))
    {
      webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
      webRequest.downloadHandler = new GeneratedTextStreamHandler(onTextResponseUpdated);
      webRequest.SetRequestHeader("Content-Type", "application/json");

      var operation = webRequest.SendWebRequest();

      while (!operation.isDone)
      {
        await Task.Delay(10);
      }

      if (webRequest.result != UnityWebRequest.Result.Success)
      {
        Debug.LogError($"Request failed: {webRequest.error}");
      }
    }
  }

  public async Task<Tuple<LLMRequestPayload, string>> SendRequestAndUpdateSequence(LLMRequestPayload request)
  {
    var response = await RequestLLMCompletion(request);
    var newCompletion = response.candidates[0].content.parts[0].text;
    request.contents.Add(new Content
    {
      role = response.candidates[0].content.role,
      parts = new List<BasePart> { new TextPart { text = newCompletion } }
    });
    request.contents.Add(new Content
    {
      parts = new List<BasePart>(),
      role = "user"
    });
    return new Tuple<LLMRequestPayload, string>(request, newCompletion);
  }
}
