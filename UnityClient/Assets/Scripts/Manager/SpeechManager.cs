using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using BriLib;

public class SpeechManager : Singleton<SpeechManager>
{
  public bool Speaking { get; private set; } = false;
  public bool Loading { get; private set; } = false;
  private const string _urlBase = NetworkSettings.PROXY_URL_BASE + "api/speech/";
  private const string apiUrl = _urlBase
    + "{0}?output_format={1}&enable_logging=true&optimize_streaming_latency={2}";
  private AudioSource _speechSource;
  private const int MAX_RETRIES = 3;
  private const int TIMEOUT_SECONDS = 45;

  public void SetSpeechSource(AudioSource source)
  {
    _speechSource = source;
  }

  public async Task Speak(string something, bool ssml = true)
  {
    Loading = true;
    var clip = await GetAudioClipFromText(something);
    Loading = false;
    if (clip != null)
    {
      await PlayClip(clip);
    }
  }

  public async Task PlayClip(AudioClip clip)
  {
    if (clip == null)
    {
      Debug.LogError("Got null clip");
      return;
    }

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
    if (cachedClip != null) { return cachedClip; }

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
        SpeechSettings.AERISITA_VOICE_ID,
        SpeechSettings.OUTPUT_FORMAT,
        SpeechSettings.OPTIMIZE_STREAM_LATENCY);
    string payload = JsonUtility.ToJson(body);

    for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
    {
      try
      {
        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
          byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(payload);
          request.uploadHandler = new UploadHandlerRaw(bodyRaw);
          request.downloadHandler = new DownloadHandlerBuffer();
          request.SetRequestHeader("Content-Type", "application/json");

          var requestTask = SendWebRequestAsync(request);
          if (await Task.WhenAny(requestTask, Task.Delay(TIMEOUT_SECONDS * 1000)) == requestTask)
          {
            await requestTask; //Ensure any exception/cancellation is re-thrown

            //In a general failure case, just retry
            if (request.result != UnityWebRequest.Result.Success)
            {
              Debug.LogError("Error: " + request.error);
              continue; // Retry on failure
            }
            else //Otherwise, attempt to save mp3 file bytes and then load as AudioClip
            {
              byte[] responseData = request.downloadHandler.data;

              string tempFilePath = Path.Combine(Application.persistentDataPath, "temp.mp3");
              File.WriteAllBytes(tempFilePath, responseData);

              using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip("file://" + tempFilePath, AudioType.MPEG))
              {
                var audioRequestTask = SendWebRequestAsync(audioRequest);
                if (await Task.WhenAny(audioRequestTask, Task.Delay(TIMEOUT_SECONDS * 1000)) == audioRequestTask)
                {
                  await audioRequestTask; // Ensure any exception/cancellation is re-thrown

                  if (audioRequest.result == UnityWebRequest.Result.ConnectionError || audioRequest.result == UnityWebRequest.Result.ProtocolError)
                  {
                    Debug.LogError("Error: " + audioRequest.error);
                    continue;
                  }
                  else
                  {
                    AudioClip audioClip = DownloadHandlerAudioClip.GetContent(audioRequest);
                    if (audioClip == null)
                    {
                      Debug.LogError("Failed to deserialize audio clip. Retrying...");
                      continue;
                    }
                    return audioClip;
                  }
                }
                else
                {
                  Debug.LogError("Audio load from local files timed out. Retrying...");
                  continue;
                }
              }
            }
          }
          else
          {
            Debug.LogError("Initial voice synth http request timed out. Retrying...");
            continue;
          }
        }
      }
      catch (Exception ex)
      {
        Debug.LogError($"Error in GetAudioClipFromText: {ex.Message}");
        continue;
      }
    }

    Debug.LogError("Max retries reached. GetAudioClipFromText failed.");
    return ErrorStateManager.Instance.FailedSynthClip;
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
