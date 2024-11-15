using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using LLM.Network;
using UnityEngine.UI;

public class MultiTurnChatTester : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  [SerializeField] private TMP_Text _outputText;
  [SerializeField] private AudioSource _audio;
  [SerializeField] private List<RawImage> _images;
  private LLMRequestPayload _chatInProgress;
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

  private void Start()
  {
    PermissionsManager.Instance.RequestPermission(AppPermission.Microphone, null);
    SpeechManager.Instance.SetSpeechSource(_audio);
    PlayAdventureSequence();
  }

  private async void PlayAdventureSequence()
  {
    //1. Construct payload with system prompt, settings info, and initial prompt
    _chatInProgress = CreateInitialPayload();

    //2. Send out initial request to start a random adventure (pre-select)
    _chatInProgress.contents[_chatInProgress.contents.Count - 1].parts.Add(new TextPart
    {
      text = StoryPromptSettings.STORY_START_PRECEDENT + "Defeat the dark lord"
    });

    while (true)
    {
      var reply = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_chatInProgress);
      _chatInProgress = reply.Item1;

      //3. Audio synth play for initial reply and first item request
      await SpeechManager.Instance.Speak(reply.Item2);

      for (int i = 0; i < 3; i++)
      {
        await NextAudioReady();

        //4. Send a picture and audio for each item
        _chatInProgress = await CameraManager.Instance.GetScreenshotAndAddToRequest(_chatInProgress);
        _chatInProgress = await AudioCaptureManager.Instance.GetAudioAndAddToRequest(_chatInProgress);
        reply = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_chatInProgress);
        _chatInProgress = reply.Item1;

        //5. Wait for completion of audio clip play
        await SpeechManager.Instance.Speak(reply.Item2);
        //6. Repeat 2 more times
      }

      //7. Send audio to start story
      await NextAudioReady();
      _chatInProgress = await AudioCaptureManager.Instance.GetAudioAndAddToRequest(_chatInProgress);

      //8. Wait for completion of story audio
      reply = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_chatInProgress);
      _chatInProgress = reply.Item1;

      _ = SpeechManager.Instance.Speak(reply.Item2);

      //8.1. Get an image prompt from the story
      var prompts = await ImagePromptGenerator.Instance.GetPromptAndNegativePrompt(_chatInProgress);

      //8.2. Get set of images from the image generator
      var imageGenResponse = await ImageGenerationManager.Instance.GetImagesBase64Encoded(prompts.Item1, prompts.Item2);

      //8.3. Decode and assign images to UI
      for (int i = 0; i < imageGenResponse.images.Count; i++)
      {
        Texture2D texture = ImageGenerationManager.Base64ToTexture(imageGenResponse.images[i]);
        if (texture != null)
        {
          _images[i].texture = texture;
        }
      }

      //8.4 Now wait for audio to resolve
      while (_audio.isPlaying) { await Task.Delay(10); }

      //8.5 Say the image gen prompts
      await SpeechManager.Instance.Speak($"<speak>I generated these using the prompt: <break time=\"1s\"/>{prompts.Item1} <break time=\"1s\"/>and the negative prompt: <break time=\"1s\"/>{prompts.Item2}</speak>");

      //9. Trigger back to beginning
      await NextAudioReady();
      _chatInProgress = await AudioCaptureManager.Instance.GetAudioAndAddToRequest(_chatInProgress);
      //TODO: Actually delete the contents of the old adventure here in full adventure implementation
    }
  }

  private LLMRequestPayload CreateInitialPayload()
  {
    var payload = new LLMRequestPayload();

    payload.generationConfig = new GenerationConfig
    {
      temperature = StoryPromptSettings.STORY_TEMPERATURE
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
      parts = new List<BasePart> { new TextPart { text = StoryPromptSettings.STORY_PROMPT } }
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
}