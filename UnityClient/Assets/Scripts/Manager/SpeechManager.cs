using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using BriLib;

public class SpeechManager : Singleton<SpeechManager>
{
  public bool Speaking { get; private set; } = false;
  private const string _urlBase = NetworkSettings.PROXY_URL_BASE + "api/speech/";
  private const string apiUrl = _urlBase 
    + "{0}?output_format={1}&enable_logging=true&optimize_streaming_latency={2}";
  private AudioSource _speechSource;

  public void SetSpeechSource(AudioSource source)
  {
    _speechSource = source;
  }

  public async Task Speak(string something, bool ssml = true)
  {
    try
    {
      var clip = await GetAudioClipFromText(something);
      if (clip != null)
      {
        await PlayClip(clip);
      }
    }
    catch (Exception ex)
    {
      Debug.LogError($"Error during speech synthesis: {ex.Message}");
    }
  }

  public async Task PlayClip(AudioClip clip)
  {
    Speaking = true;
    _speechSource.Stop();
    _speechSource.clip = clip;
    _speechSource.Play();
    await Task.Delay((int)(_speechSource.clip.length * 1000));
    Speaking = false;
  }

  public async Task<AudioClip> GetAudioClipFromText(string text)
  {
    var cachedClip = CachedAudioManager.Instance.GetAudioClip(text);
    if (cachedClip != null) { Debug.LogWarning("Got audio from cache!");  return cachedClip; }

    var body = new ElevenLabsRequestBody
    {
      text = text,
      model_id = SpeechSettings.MODEL,
      voice_settings = new VoiceSettings
      {
        stability = SpeechSettings.STABILITY_BOOST,
        similarity_boost = SpeechSettings.SIMILARITY_BOOST,
        style = SpeechSettings.STYLE
      }
    };
    var url = string.Format(
        apiUrl,
        SpeechSettings.KING_OF_NY_VOICE_ID,
        SpeechSettings.OUTPUT_FORMAT,
        SpeechSettings.OPTIMIZE_STREAM_LATENCY);
    string payload = JsonUtility.ToJson(body);

    try
    {
      using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
      {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        await SendWebRequestAsync(request);

        if (request.result != UnityWebRequest.Result.Success)
        {
          Debug.LogError("Error: " + request.error);
          return null;
        }
        else
        {
          byte[] responseData = request.downloadHandler.data;

          string tempFilePath = Path.Combine(Application.persistentDataPath, "temp.mp3");
          File.WriteAllBytes(tempFilePath, responseData);

          using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + tempFilePath, AudioType.MPEG))
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
    catch (Exception ex)
    {
      Debug.LogError($"Error in GetAudioClipFromText: {ex.Message}");
      return null;
    }
  }

  private static Task SendWebRequestAsync(UnityWebRequest request)
  {
    var tcs = new TaskCompletionSource<bool>();
    request.SendWebRequest().completed += operation =>
    {
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