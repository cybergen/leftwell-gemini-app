using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using LLM.Network;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;

public class SpeechReplyTester : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  [SerializeField] private TMP_Text _outputText;
  [SerializeField] private AudioSource _audioSource;
  private const int MAX_FILE_BYTES = 20000000;

  public void OnPointerDown(PointerEventData eventData)
  {
    _outputText.text = "Starting audio capture";
    AudioCaptureManager.Instance.StartAudioCapture();
  }

  public async void OnPointerUp(PointerEventData eventData)
  {
    _outputText.text = "Capturing image";
    AudioCaptureManager.Instance.EndAudioCapture();
    var camImage = await CameraImageManager.Instance.GetNextAvailableCameraImage();
    _outputText.text = "Encoding audio";
    var audioBytes = await AudioCaptureManager.Instance.GetNextAudioData();
    var imageBlob = Convert.ToBase64String(camImage.Texture.EncodeToPNG());

    var partList = new List<BasePart>();

    if (audioBytes.Length > MAX_FILE_BYTES)
    {
      _outputText.text = "Uploading audio separately";
      var file = await FileUploadManager.Instance.UploadFile("audio/wav", "Device audio during AR session", audioBytes);
      _outputText.text = "Audio uploaded";
      Debug.Log($"Finished upload with file {file}");
      partList.Add(new FilePart
      {
        fileData = new FilePartData
        {
          mimeType = file.file.mimeType,
          fileUri = file.file.uri
        }
      });
    }
    else
    {
      Debug.Log("Audio fits in single upload, pushing up blob directly");
      var audioBlob = Convert.ToBase64String(audioBytes);
      partList.Add(new DataPart
      {
        inlineData = new Blob
        {
          mimeType = "audio/wav",
          data = audioBlob
        }
      });
    }

    partList.Add(new DataPart
    {
      inlineData = new Blob
      {
        mimeType = "image/png",
        data = imageBlob
      }
    });

    partList.Add(new TextPart
    {
      text = "Please follow the spoken instructions in the audio clip, making use of the attached image as needed. Use correct, valid SSML to markup your response for use by a speech synthesis tool. Speak with a bit of sarcasm and humor, please."
    });

    _outputText.text = "Making request to Gemini";
    var payload = LLMRequestPayload.GetRequestWithMultipleParts(partList.ToArray());
    var response = await LLMInteractionManager.Instance.RequestLLMCompletion(payload);
    // TODO: Better way to interact with this code
    GCTextToSpeech.Instance.apiKey = Config.Instance.ApiKey;
    GCTextToSpeech.Instance.SynthesizeSuccessEvent += OnVoiceSynthesizeSuccess;
    GCTextToSpeech.Instance.SynthesizeFailedEvent += OnVoiceSynthesizeFail;
    GCTextToSpeech.Instance.Synthesize(response.candidates[0].content.parts[0].text, new VoiceConfig()
      {
        gender = Enumerators.SsmlVoiceGender.MALE,
        languageCode = GCTextToSpeech.Instance.PrepareLanguage(Enumerators.LanguageCode.en_GB),
        name = "en-GB-Neural2-B"
    },
      true,
      0.15d,
      0.95d,
      16000, 
      new Enumerators.EffectsProfileId[] { });
    Debug.Log($"Got LLM response:\n{response}");
    _outputText.text = (response.candidates[0].content.parts[0] as TextPart).text;
  }

  private void OnVoiceSynthesizeFail(string arg1, long arg2)
  {
    Debug.LogError($"Failed to synthesize voice with arg {arg1}");
  }

  private void OnVoiceSynthesizeSuccess(PostSynthesizeResponse response, long arg2)
  {
    Debug.Log("Succeeded in voice synthesis");
    _audioSource.clip = GCTextToSpeech.Instance.GetAudioClipFromBase64(response.audioContent, FrostweepGames.Plugins.GoogleCloud.TextToSpeech.Constants.DEFAULT_AUDIO_ENCODING);
    _audioSource.Play();
  }
}