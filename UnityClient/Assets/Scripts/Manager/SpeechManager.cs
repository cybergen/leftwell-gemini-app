using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using BriLib;

public class SpeechManager : Singleton<SpeechManager>
{
  public bool Speaking { get; private set; } = false;
  private const string _urlBase = "https://api.elevenlabs.io/v1/text-to-speech/";
  private const string apiUrl = _urlBase + "{0}?output_format={1}&enable_logging=true&optimize_streaming_latency={2}";
  private AudioSource _speechSource;

  public void SetSpeechSource(AudioSource source)
  {
    _speechSource = source;
  }

  public async Task Speak(string something, bool ssml=true)
  {
    var clip = await GetAudioClipFromText(something);
    Speaking = true;
    _speechSource.Stop();
    _speechSource.clip = clip;
    _speechSource.Play();
    await Task.Delay((int)(_speechSource.clip.length * 1000));
    Speaking = false;
  }

  private async Task<AudioClip> GetAudioClipFromText(string text)
  {
    // Create the request payload
    var body = new ElevenLabsRequestBody
    {
      text = text,
      voice_settings = new VoiceSettings
      {
        stability = SpeechConstants.STABILITY_BOOST,
        similarity_boost = SpeechConstants.SIMILARITY_BOOST,
        style = SpeechConstants.STYLE
      }
    };
    var url = string.Format(
      apiUrl, 
      SpeechConstants.KING_OF_NY_VOICE_ID, 
      SpeechConstants.OUTPUT_FORMAT, 
      SpeechConstants.OPTIMIZE_STREAM_LATENCY);
    string payload = JsonUtility.ToJson(body);

    // Set up the UnityWebRequest
    using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
    {
      byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
      request.uploadHandler = new UploadHandlerRaw(bodyRaw);
      request.downloadHandler = new DownloadHandlerBuffer();
      request.SetRequestHeader("Content-Type", "application/json");
      request.SetRequestHeader("xi-api-key", Config.Instance.ElevenLabsKey);

      // Send the request and await the response
       await SendWebRequestAsync(request);

      if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
      {
        Debug.LogError("Error: " + request.error);
        return null;
      }
      else
      {
        // Get the response data
        byte[] responseData = request.downloadHandler.data;

        // Save the response data to a temporary file
        string tempFilePath = Path.Combine(Application.persistentDataPath, "temp.mp3");
        File.WriteAllBytes(tempFilePath, responseData);

        // Load the AudioClip from the temporary file
        using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip(tempFilePath, AudioType.MPEG))
        {
          await SendWebRequestAsync(audioRequest);

          if (audioRequest.result == UnityWebRequest.Result.ConnectionError || audioRequest.result == UnityWebRequest.Result.ProtocolError)
          {
            Debug.LogError("Error: " + audioRequest.error);
            return null;
          }
          else
          {
            AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
            return audioClip;
          }
        }
      }
    }
  }

  private static Task SendWebRequestAsync(UnityWebRequest request)
  {
    var tcs = new TaskCompletionSource<bool>();
    request.SendWebRequest().completed += operation =>
    {
      Debug.Log($"Got response from eleven labs: {request.result}");
      if (request.result == UnityWebRequest.Result.Success)
      {
        tcs.SetResult(true);
      }
      else
      {
        tcs.SetException(new Exception(request.error));
      }
    };
    return tcs.Task;
  }
}

[Serializable]
public class ElevenLabsRequestBody
{
  public string text;
  public VoiceSettings voice_settings;
}

[Serializable]
public class VoiceSettings
{
  public float stability;
  public float similarity_boost;
  public float style;
}