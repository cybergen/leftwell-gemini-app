using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using LLM.Network;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using TTSConstants = FrostweepGames.Plugins.GoogleCloud.TextToSpeech.Constants;

public class MultiTurnChatTester : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  [SerializeField] private TMP_Text _outputText;
  [SerializeField] private AudioSource _audio;
  private LLMRequestPayload _chatInProgress;
  private Action _onSpeakingSuccessful;
  private Action _onSpeakingFailed;
  private bool _audioReady = false;

  public void OnPointerDown(PointerEventData eventData)
  {
    _audioReady = false;
    AudioCaptureManager.Instance.StartAudioCapture();
  }

  public void OnPointerUp(PointerEventData eventData)
  {
    AudioCaptureManager.Instance.EndAudioCapture();
    _audioReady = true;
  }

  private void Awake()
  {
    Debug.Log($"LEFTWELL Awake was called");
  }

  private void Start()
  {
    Debug.Log($"LEFTWELL Start was called");
    GCTextToSpeech.Instance.apiKey = Config.Instance.ApiKey;
    GCTextToSpeech.Instance.SynthesizeSuccessEvent += OnVoiceSynthesizeSuccess;
    GCTextToSpeech.Instance.SynthesizeFailedEvent += OnVoiceSynthesizeFail;
    PlayAdventureSequence();
  }

  private async void PlayAdventureSequence()
  {
    //1. Construct payload with system prompt, settings info, and initial prompt
    Debug.Log($"LEFTWELL Creating payload");
    _chatInProgress = CreateInitialPayload();

    //2. Send out initial request to start a random adventure (pre-select)
    Debug.Log($"LEFTWELL Sending initial story request");
    _chatInProgress.contents[_chatInProgress.contents.Count - 1].parts.Add(new TextPart
    {
      text = Constants.STORY_START_PRECEDENT + "Defeat the dark lord"
    });

    while (true)
    {
      var reply = await SendRequestAndUpdateSequence(_chatInProgress);
      _chatInProgress = reply.Item1;

      //3. Audio synth play for initial reply and first item request
      Debug.Log($"LEFTWELL Requesting synth voice");
      await SpeakSSML(reply.Item2);

      for (int i = 0; i < 3; i++)
      {
        Debug.Log($"LEFTWELL Awaiting audio capture");
        await NextAudioReady();

        //4. Send a picture and audio for each item
        Debug.Log($"LEFTWELL Capturing screenshot and uploading");
        _chatInProgress = await GetScreenshotAndAddToRequest(_chatInProgress);
        Debug.Log($"LEFTWELL Encoding audio and uploading");
        _chatInProgress = await GetAudioAndAddToRequest(_chatInProgress);
        Debug.Log($"LEFTWELL Pushing up item info");
        reply = await SendRequestAndUpdateSequence(_chatInProgress);
        _chatInProgress = reply.Item1;

        //5. Wait for completion of audio clip play
        Debug.Log($"LEFTWELL Item response voice synth");
        await SpeakSSML(reply.Item2);
        //6. Repeat 2 more times
      }

      //7. Send audio to start story
      Debug.Log($"LEFTWELL Waiting for audio to start story");
      await NextAudioReady();
      _chatInProgress = await GetAudioAndAddToRequest(_chatInProgress);

      //8. Wait for completion of story audio
      Debug.Log($"LEFTWELL Sending start audio to LLM");
      reply = await SendRequestAndUpdateSequence(_chatInProgress);
      _chatInProgress = reply.Item1;

      Debug.Log($"LEFTWELL Synth voicing the story result");
      await SpeakSSML(reply.Item2);

      //9. Go back to beginning
      await NextAudioReady();
      _chatInProgress = await GetAudioAndAddToRequest(_chatInProgress);
    }
  }

  private async Task<LLMRequestPayload> GetAudioAndAddToRequest(LLMRequestPayload currentPayload)
  {
    var audioBytes = await AudioCaptureManager.Instance.GetNextAudioData();
    var fileInfo = await FileUploadManager.Instance.UploadFile("audio/wav", "Device audio during AR session", audioBytes);
    var part = new FilePart
    {
      fileData = new FilePartData
      {
        mimeType = fileInfo.file.mimeType,
        fileUri = fileInfo.file.uri
      }
    };
    currentPayload.contents[currentPayload.contents.Count - 1].parts.Add(part);
    return currentPayload;
  }

  private async Task<LLMRequestPayload> GetScreenshotAndAddToRequest(LLMRequestPayload currentPayload)
  {
    var camImage = await CameraImageManager.Instance.GetNextAvailableCameraImage();
    var bytes = camImage.Texture.EncodeToPNG();
    var fileInfo = await FileUploadManager.Instance.UploadFile("image/png", "Picture in AR mode", bytes);
    var part = new FilePart
    {
      fileData = new FilePartData
      {
        mimeType = fileInfo.file.mimeType,
        fileUri = fileInfo.file.uri
      }
    };
    currentPayload.contents[currentPayload.contents.Count - 1].parts.Add(part);
    return currentPayload;
  }

  private async Task SpeakSSML(string something)
  {
    GCTextToSpeech.Instance.Synthesize(something, new VoiceConfig()
        {
          gender = Constants.SYNTH_GENDER,
          languageCode = GCTextToSpeech.Instance.PrepareLanguage(Constants.SYNTH_LOCALE),
          name = Constants.SYNTH_VOICE
        },
      true,
      Constants.SYNTH_PITCH,
      Constants.SYNTH_SPEAKING_RATE,
      Constants.SYNTH_SAMPLE_RATE_HERTZ,
      new Enumerators.EffectsProfileId[] { });

    var outcomeTriggered = false;

    void onFailed() { outcomeTriggered = true; }
    void onSucceeded() { outcomeTriggered = true; }
    _onSpeakingFailed += onFailed;
    _onSpeakingSuccessful += onSucceeded;

    while (!outcomeTriggered) { await Task.Delay(10); }

    _onSpeakingFailed -= onFailed;
    _onSpeakingSuccessful -= onSucceeded;
  }

  private async Task<Tuple<LLMRequestPayload, string>> SendRequestAndUpdateSequence(LLMRequestPayload request)
  {
    var response = await LLMInteractionManager.Instance.RequestLLMCompletion(request);
    var newCompletion = response.candidates[0].content.parts[0].text;
    request.contents.Add(new Content
    {
      role = response.candidates[0].content.role,
      parts = new List<BasePart> { new TextPart { text = newCompletion} }
    });
    request.contents.Add(new Content
    {
      parts = new List<BasePart>(),
      role = "user"
    });
    return new Tuple<LLMRequestPayload, string>(request, newCompletion);
  }

  private LLMRequestPayload CreateInitialPayload()
  {
    var payload = new LLMRequestPayload();

    payload.generationConfig = new GenerationConfig
    {
      temperature = Constants.STORY_TEMPERATURE
    };

    payload.safetySettings = new List<SafetySetting>();
    var relevantSettings = new List<HarmCategory>
    {
      HarmCategory.HARM_CATEGORY_SEXUALLY_EXPLICIT,
      HarmCategory.HARM_CATEGORY_HATE_SPEECH,
      HarmCategory.HARM_CATEGORY_HARASSMENT,
      HarmCategory.HARM_CATEGORY_DANGEROUS_CONTENT
    };
    foreach (var category in relevantSettings)
    {
      payload.safetySettings.Add(new SafetySetting
      {
        category = Enum.GetName(typeof(HarmCategory), category),
        threshold = Enum.GetName(typeof(HarmBlockThreshold), HarmBlockThreshold.BLOCK_NONE)
      });
    }

    payload.systemInstruction = new Content
    {
      parts = new List<BasePart> { new TextPart { text = Constants.STORY_PROMPT } }
    };

    payload.contents = new List<Content>
    {
      new Content
      {
        parts = new List<BasePart>(),
        role = "user"
      }
    };

    return payload;
  }

  private async Task NextAudioReady()
  {
    while (!_audioReady) { await Task.Delay(10); }
    _audioReady = false;
  }

  private void OnVoiceSynthesizeFail(string arg1, long arg2)
  {
    Debug.LogError($"Failed to synthesize voice with arg {arg1}");
    _onSpeakingFailed?.Invoke();
  }

  private async void OnVoiceSynthesizeSuccess(PostSynthesizeResponse response, long arg2)
  {
    Debug.Log("Succeeded in voice synthesis");
    _audio.clip = GCTextToSpeech.Instance.GetAudioClipFromBase64(response.audioContent, TTSConstants.DEFAULT_AUDIO_ENCODING);
    _audio.Play();
    await Task.Delay((int)(_audio.clip.length * 1000));
    _onSpeakingSuccessful?.Invoke();
  }
}