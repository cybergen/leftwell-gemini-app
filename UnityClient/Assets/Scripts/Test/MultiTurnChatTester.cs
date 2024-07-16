using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using LLM.Network;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using BriLib;
using System.IO;

public class MultiTurnChatTester : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  [SerializeField] private TMP_Text _outputText;
  [SerializeField] private AudioSource _audioSource;
  private const int MAX_FILE_BYTES = 20000000;
  private LLMRequestPayload _chatInProgress;
  private Action _onSpeakingSuccessful;
  private Action _onSpeakingFailed;
  private Action _onAudioCaptured;

  public void OnPointerDown(PointerEventData eventData)
  {
    _outputText.text = "Starting audio capture";
    AudioCaptureManager.Instance.StartAudioCapture();
  }

  public void OnPointerUp(PointerEventData eventData)
  {

    _outputText.text = "Capturing image";
    AudioCaptureManager.Instance.EndAudioCapture();
    _onAudioCaptured!();
  }

  private void Start()
  {
    PlayAdventureSequence();
  }

  private async void PlayAdventureSequence()
  {
    //1. Construct payload with system prompt, settings info, and initial prompt
    _chatInProgress = CreateInitialPayload();
    //2. Send out initial request to start a random adventure (pre-select)
    _chatInProgress.contents[_chatInProgress.contents.Count - 1].parts.Add(new TextPart
    {
      text = Constants.STORY_START_PRECEDENT + "Defeat the dark lord"
    });
    _chatInProgress = await SendRequestAndUpdateSequence(_chatInProgress);
    //3. Audio synth play for initial reply and first item request

    //4. Send a picture and audio, or just audio, or just a picture for item 1
    //5. Wait for completion of audio clip play
    //6. Repeat 2 more times
    //7. Send audio to start story
    //8. Wait for completion of story audio
    //9. Go back to beginning
  }

  private async Task<LLMRequestPayload> GetAudioAndAddToRequest(LLMRequestPayload currentPayload)
  {
    var audioBytes = await AudioCaptureManager.Instance.GetNextAudioData();
    var fileInfo = await FileUploadManager.Instance.UploadFile("audio/wav", "Device audio during AR session", audioBytes);
    var part = new FilePart
    {
      fileData = new LLM.Network.FileInfo
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
      fileData = new LLM.Network.FileInfo
      {
        mimeType = fileInfo.file.mimeType,
        fileUri = fileInfo.file.uri
      }
    };
    currentPayload.contents[currentPayload.contents.Count - 1].parts.Add(part);
    return currentPayload;
  }

  private async void SpeakSSML(string something)
  {
    GCTextToSpeech.Instance.apiKey = Config.Instance.ApiKey;
    GCTextToSpeech.Instance.SynthesizeSuccessEvent += OnVoiceSynthesizeSuccess;
    GCTextToSpeech.Instance.SynthesizeFailedEvent += OnVoiceSynthesizeFail;
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
    void onSucceeded() {  outcomeTriggered = true; }
    _onSpeakingFailed += onFailed;
    _onSpeakingSuccessful += onSucceeded;

    while (!outcomeTriggered) { await Task.Delay(10); }

    _onSpeakingFailed -= onFailed;
    _onSpeakingSuccessful -= onSucceeded;
  }

  private async Task<LLMRequestPayload> SendRequestAndUpdateSequence(LLMRequestPayload request)
  {
    var response = await LLMInteractionManager.Instance.RequestLLMCompletion(request);
    request.contents.Add(response.candidates[0].content);
    request.contents.Add(new Content
    {
      parts = new List<BasePart>(),
      role = "user"
    });
    return request;
  }

  private LLMRequestPayload CreateInitialPayload()
  {
    var payload = new LLMRequestPayload();
    payload.generationConfig = new GenerationConfig
    {
      temperature = Constants.STORY_TEMPERATURE
    };
    payload.safetySettings = new List<SafetySetting>();
    foreach (var category in Enum.GetValues(typeof(HarmCategory)))
    {
      payload.safetySettings.Add(new SafetySetting
      {
        category = (HarmCategory)category,
        threshold = HarmBlockThreshold.BLOCK_NONE
      });
    }
    payload.systemInstruction = new Content
    {
      parts = new List<BasePart>
      {
        new TextPart
        {
          text = Constants.STORY_PROMPT
        }
      }
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

  private void OnVoiceSynthesizeFail(string arg1, long arg2)
  {
    Debug.LogError($"Failed to synthesize voice with arg {arg1}");
    _onSpeakingFailed!();
  }

  private async void OnVoiceSynthesizeSuccess(PostSynthesizeResponse response, long arg2)
  {
    Debug.Log("Succeeded in voice synthesis");
    _audioSource.clip = GCTextToSpeech.Instance.GetAudioClipFromBase64(response.audioContent, FrostweepGames.Plugins.GoogleCloud.TextToSpeech.Constants.DEFAULT_AUDIO_ENCODING);
    _audioSource.Play();
    await Task.Delay((int)(_audioSource.clip.length * 1000));
    _onSpeakingSuccessful!();
  }
}