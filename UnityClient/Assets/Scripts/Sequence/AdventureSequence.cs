using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using BriLib;
using LLM.Network;
using Request = LLM.Network.LLMRequestPayload;

public class AdventureSequence : ISequence<CharacterBehaviorController, AdventureResult>
{
  private AdventureResult _adventureResult = new AdventureResult();
  private int _activatedPortals = 0;

  public async Task<AdventureResult> RunAsync(CharacterBehaviorController character)
  {
    var adventureResult = new AdventureResult();
    var payload = CreateInitialPayload();

    //1. Introductions from wizard
    var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);
    payload = payloadReplyPair.Item1;
    var stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);

    //Fly in and start talking while arriving
    character.SetState(CharacterStates.InitialFlyIn);
    _ = SpeechManager.Instance.Speak(stateReplyPair.Item2);
    while (character.BusyPathing) await Task.Delay(10);

    //Run convo until state changes
    payload = (await RunConvoUntilStateChanges(payload, StoryState.Intro, UIManager.Instance.PushToTalkButton)).Item1;

    //2. Give you an option of stories to choose from
    //Also comes from prompt - prime with 3 story prompts
    var convoResult = await RunConvoUntilStateChanges(payload, StoryState.StorySelect, UIManager.Instance.PushToTalkButton);
    payload = convoResult.Item1;

    //3. Makes you select three items to bring with you on your quest - instructs you to find them

    //4. Tap image to select
    //4.1 Captures an image and pushes up to image gen for restyling
    //4.2 Asks if you want to tell him anything about it - not from prompt
    //4.3 If yes, you can press and hold to talk
    //4.4 Adds incipient portal vfx to area you took picture from (right in front of your camera)
    //4.5 Tells you if the item looked powerful, gonna help to magic it up, etc (not LLM)
    //4.6 Repeat x 3
    var itemStrings = GetItemStrings(convoResult.Item2);
    for (int i = 0; i < 3; i++)
    {
      bool imageReady = false;
      Action onPictureCaptured = () =>
      {
        imageReady = true;
      };
      UIManager.Instance.TakePictureButton.Show(itemStrings[i], onPictureCaptured);
      while (!imageReady) await Task.Delay(10);
      UIManager.Instance.TakePictureButton.Hide();
      character.SetState(CharacterStates.ShownObject);

      //RUN THIS TWICE BECAUSE IT'S BROKEN AND RETURNS AN OLD IMAGE
      var camImage = await CameraManager.Instance.GetNextAvailableCameraImage();
      camImage = await CameraManager.Instance.GetNextAvailableCameraImage();

      PortalManager.Instance.SpawnCaptureMarker(camImage.Texture);
      //TODO: Kick off image editing
      _ = SpeechManager.Instance.Speak(GetRandomItemCaptureDialog());
      bool audioReady = false;
      Action onAudioCaptureStart = () =>
      {
        AudioCaptureManager.Instance.StartAudioCapture();
      };
      Action onAudioCaptureEnd = () =>
      {
        AudioCaptureManager.Instance.EndAudioCapture();
        audioReady = true;
      };
      UIManager.Instance.PushToTalkButton.Show(onAudioCaptureStart, onAudioCaptureEnd);
      while (!audioReady) await Task.Delay(10);
      UIManager.Instance.PushToTalkButton.Hide();
      character.SetState(CharacterStates.JumpingToPlayer);
      payload = await AudioCaptureManager.Instance.GetAudioAndAddToRequest(payload);
    }
    //Say something random while feedback is requested
    //_ = SpeechManager.Instance.Speak(GetRandomProcessingDialog());

    //5.1 Requests you generate a big portal
    //5.2 Flies next to it
    //5.3 Pushes up all images, audio, etc, to LLM for final story sequence
    //5.4 Says that the spell is in progress, but you need to verify each of the anchors
    //5.5 Anchor portal states change from very spinny/loady to more stable once ready
    //TODO: Actually get this starting pose by asking you to find an open area for the portal
    await SpeechManager.Instance.Speak("I'll get these items ready. In the meantime, give me an open area where I can form the portal");
    var portalSpawnReady = false;
    UIManager.Instance.TakePictureButton.Show("Tap to spawn big portal", () =>
    {
      portalSpawnReady = true;
    });
    while (!portalSpawnReady) await Task.Delay(10);
    UIManager.Instance.TakePictureButton.Hide();
    PortalManager.Instance.SpawnBigPortal();
    PortalManager.Instance.SetBigPortalLoading();
    character.SetState(CharacterStates.FlyingToPortal);
    _ = SpeechManager.Instance.Speak("Perfect! Now you're going to help me get this spun up.");

    payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);
    payload = payloadReplyPair.Item1;
    //TODO: Actually get the edited images before triggering activatable
    _ = SpeechManager.Instance.Speak("Those items are ready. Go check each marker out and activate them so we can get a move on!");
    SetMarkersActivatable(payloadReplyPair.Item2);

    //TODO: Actual way to activate these portals
    UIManager.Instance.TakePictureButton.Show("Tap activate small portal", () => PortalManager.Instance.ActivateMarker(_activatedPortals));

    //6. Explore the magical items
    //6.1 Each one flashes and changes to the edited image
    //6.2 Adds magical particle trails to big portal
    //6.3 Repeat x 3
    while (_activatedPortals < 3) await Task.Delay(10);
    UIManager.Instance.TakePictureButton.Hide();

    payload.contents[payload.contents.Count - 1].parts.Add(new TextPart
    {
      text = "Ready to journey!"
    });
    payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);
    payload = payloadReplyPair.Item1;
    stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);

    var imagePrompts = await ImagePromptGenerator.Instance.GetPromptAndNegativePrompt(payload);
    var images = await ImageGenerationManager.Instance.GetImagesBase64Encoded(imagePrompts.Item1, imagePrompts.Item2);
    //TODO: Actually select the best one somehow
    var image = ImageGenerationManager.Base64ToTexture(images[0]);
    var readyToNarrateFinish = false;
    PortalManager.Instance.SetBigPortalActivatable(image, () =>
    {
      readyToNarrateFinish = true;
    });
    UIManager.Instance.TakePictureButton.Show("Tap to activate big portal", () => PortalManager.Instance.ActivatePortal());
    while (!readyToNarrateFinish) await Task.Delay(10);
    await SpeechManager.Instance.Speak(stateReplyPair.Item2);

    //7.1 If final story and image gen are not ready, he let's you know that the portal is not quite there yet
    //7.2 If ready, he tells you to go to the big portal and activate it
    //7.3 Big image appears in portal, he narrates what he saw

    //8. Tells you you're a pretty good apprentice, asks if you want to give it another go
    return adventureResult;
  }

  public static List<string> GetItemStrings(string input)
  {
    List<string> resultList = new List<string>();

    string[] lines = input.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
    Regex pattern = new Regex(@"^Item [1-3]:\s(.+)$");

    foreach (var line in lines)
    {
      Match match = pattern.Match(line);
      if (match.Success)
      {
        resultList.Add(match.Groups[1].Value);
      }
    }

    return resultList;
  }

  private void AdvancePortalState()
  {
    if (_activatedPortals < 3)
    {
      PortalManager.Instance.ActivateMarker(_activatedPortals);
    }
    else
    {
      PortalManager.Instance.ActivatePortal();
    }
  }

  private string GetRandomProcessingDialog()
  {
    var possibilities = new List<string>
    {
      "That's all three. Give me a moment here...",
      "Okay, got 'em. Just a moment...",
      "Got it! Let me just start this process off...",
      "Wow, alright. That's all three. Just a sec.",
      "Hold up a sec now. Let me just... figure this out."
    };
    return MathHelpers.SelectFromRange(possibilities, new System.Random());
  }

  private string GetRandomItemCaptureDialog()
  {
    var possibilities = new List<string>
    {
      "Uh... Interesting. Tell me about this.",
      "Wow. What do you have to say about this?",
      "Anything I should know about this?",
      "Why did you pick this?",
      "That looks powerful. Care to explain?"
    };
    return MathHelpers.SelectFromRange(possibilities, new System.Random());
  }

  private async Task<Tuple<Request, string>> RunConvoUntilStateChanges(Request payload, StoryState state, PushToTalkButton button)
  {
    var stateReplyPair = new Tuple<StoryState, string>(state, string.Empty);
    while (stateReplyPair.Item1 == state)
    {
      //Show reply button and wait for user to respond with it
      var audioReady = false;
      Action audioCaptured = () =>
      {
        audioReady = true;
        AudioCaptureManager.Instance.EndAudioCapture();
      };
      button.Show(() => AudioCaptureManager.Instance.StartAudioCapture(), audioCaptured);
      while (!audioReady) await Task.Delay(10);

      //Hide reply button again
      button.Hide();

      //Capture audio, upload it, and use it in next LLM request
      payload = await AudioCaptureManager.Instance.GetAudioAndAddToRequest(payload);
      var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);

      //Update state for checking if we've moved on or not
      payload = payloadReplyPair.Item1;
      stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);

      //Say latest message
      await SpeechManager.Instance.Speak(stateReplyPair.Item2);
    }
    return new Tuple<Request, string>(payload, stateReplyPair.Item2);
  }

  private Tuple<StoryState, string> ParseInfoFromReply(string reply)
  {
    string pattern = @"^State:\s*(.*)$";
    Regex regex = new Regex(pattern, RegexOptions.Multiline);
    Match match = regex.Match(reply);

    if (match.Success)
    {
      string state = match.Groups[1].Value.Trim();
      string modifiedResponse = regex.Replace(reply, string.Empty).Trim();
      return new Tuple<StoryState, string>(Enum.Parse<StoryState>(state), modifiedResponse);
    }
    else
    {
      Debug.LogError("Failed to get state from reply");
      return null;
    }
  }

  private void SetMarkersActivatable(string reply)
  {
    for (int i = 1; i <= 3; i++)
    {
      var pattern = @"^Item " + i + @":\s*(.*)$";
      Regex regex = new Regex(pattern, RegexOptions.Multiline);
      Match match = regex.Match(reply);

      if (match.Success)
      {
        string state = match.Groups[1].Value.Trim();
        string modifiedResponse = regex.Replace(reply, string.Empty).Trim();
        PortalManager.Instance.SupplyTransformedImage(i, null, () => {
          _activatedPortals++;
          SpeakItemReply(modifiedResponse);
        });
      }
      else
      {
        Debug.LogError($"Failed to get item {i} from reply");
      }
    }    
  }

  private void SpeakItemReply(string reply)
  {
    _ = SpeechManager.Instance.Speak(reply);
  }

  private LLMRequestPayload CreateInitialPayload()
  {
    var payload = new LLMRequestPayload();

    payload.generationConfig = new GenerationConfig
    {
      temperature = Constants.STORY_TWO_TEMPERATURE
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
      parts = new List<BasePart> { new TextPart { text = Constants.STORY_PROMPT_TWO_FIRST_TIME } }
    };

    payload.contents = new List<Content>
    {
      new Content
      {
        parts = new List<BasePart>{ new TextPart { text = "Begin" } },
        role = "user"
      }
    };

    return payload;
  }
}

public enum StoryState
{
  None,
  Intro,
  StorySelect,
  ItemSelect,
  ItemComment,
  TellingStory,
  AskRestart
}

public class AdventureResult
{
  public Texture2D ResultImage;
  public string Synopsis;
  public LLMRequestPayload Chat;
}