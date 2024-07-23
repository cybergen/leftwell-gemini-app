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
  private List<CaptureMarkerSequence> _captureMarketSequences = new List<CaptureMarkerSequence>();
  private Texture2D _finalImage;
  private string _finalStory;
  private bool _bigPortalActivated = false;

  public async Task<AdventureResult> RunAsync(CharacterBehaviorController character)
  {
    var adventureResult = new AdventureResult();
    var payload = CreateInitialPayload();

    //Get intro text from LLM
    var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);
    payload = payloadReplyPair.Item1;
    var stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);

    //Make wizard fly in and start talking while arriving
    character.SetState(CharacterStates.InitialFlyIn);
    _ = SpeechManager.Instance.Speak(stateReplyPair.Item2);
    while (character.BusyPathing) await Task.Delay(10);

    //Run intro convo until state changes
    payload = (await RunConvoUntilStateChanges(payload, StoryState.Intro)).Item1;
    
    //Choose which story to play
    var convoResult = await RunConvoUntilStateChanges(payload, StoryState.StorySelect);
    payload = convoResult.Item1;

    //Add images of magical items (and audio descriptions) to payload one by one
    var itemStrings = GetItemStrings(convoResult.Item2);
    int imagesUploaded = 0;
    int audioUploaded = 0;
    for (int i = 0; i < 3; i++)
    {
      var tex = await GetCameraImage(itemStrings[i]);
      character.SetState(CharacterStates.ShownObject);

      //Kick off image editing sequence, incrementing activated portals after each one has been activated
      var captureMarkerSequence = new CaptureMarkerSequence();
      _ = captureMarkerSequence.RunAsync(tex).ContinueWith((task) => {
        Debug.LogWarning($"Got portal activated with prior activation count: {_activatedPortals}");
        _activatedPortals++;
      });
      _captureMarketSequences.Add(captureMarkerSequence);

      //Kick off addition of image to payload
      _ = CameraManager.Instance.UploadImageAndGetFilePart(tex).ContinueWith((task) =>
      {
        imagesUploaded++;
        //Need to add like this to avoid out of order execution stomping on each other
        payload.contents[payload.contents.Count - 1].parts.Add(task.Result);
      });

      await SpeechManager.Instance.Speak(GetRandomItemCaptureDialog());
      await GetAudioCaptured();

      character.SetState(CharacterStates.JumpingToPlayer);
      _ = AudioCaptureManager.Instance.GetAudioAndUpload().ContinueWith((task) =>
      {
        audioUploaded++;
        //Need to add like this to avoid out of order execution stomping on each other
        payload.contents[payload.contents.Count - 1].parts.Add(task.Result);
      });
    }

    //Get starting pose for big portal
    _ = SpeechManager.Instance.Speak("I'll get these items ready. In the meantime, give me an open area where I can form the portal");
    await AwaitableTakePicturePress("Tap to spawn big portal");
    PortalManager.Instance.SpawnBigPortal();
    PortalManager.Instance.SetBigPortalLoading();
    character.SetState(CharacterStates.FlyingToPortal);

    //TODO: Does this make sense here to be discarded?
    _ = SpeechManager.Instance.Speak("Perfect! I'll start working the big magic to get this spun up!");

    //Ensure all images and audio are ready in the payload before advancing
    while (imagesUploaded < 3 || audioUploaded < 3) await Task.Delay(10);

    //Get commentary for each item and apply
    payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);
    payload = payloadReplyPair.Item1;
    ApplyCommentaryToMarkers(payloadReplyPair.Item2);

    //Immediately kick off final story sequence
    _ = new BigPortalSequence().RunAsync(payload).ContinueWith((task) =>
    {
      Debug.LogWarning($"Final image, payload, and story have arrived");
      _finalImage = task.Result.FinalImage;
      payload = task.Result.Payload;
      _finalStory = ParseInfoFromReply(task.Result.Reply).Item2;
      PortalManager.Instance.SetBigPortalActivatable(_finalImage, () =>
      {
        _bigPortalActivated = true;
      });
    });

    //Wait for both edited image and commentary reply to come in before allowing activation
    while (!PortalManager.Instance.GetAllMarkersActivatable()) await Task.Delay(10);
    Debug.LogWarning($"All markers were activatable, moving on!");
    _ = SpeechManager.Instance.Speak("I put a little bit of magic on each of your items and transformed them! Go check out each one and activate it!");

    //TODO: Actual way to activate these portals
    //Require user explores magical items before advancing
    UIManager.Instance.TakePictureButton.Show("Tap to activate small portal", () => PortalManager.Instance.ActivateMarker(_activatedPortals));
    while (_activatedPortals < 3) await Task.Delay(10);
    UIManager.Instance.TakePictureButton.Hide();
    
    //Wait for results for big portal to be available before advancing at this point
    if (!string.IsNullOrEmpty(_finalStory) && _finalImage != null)
    {
      _ = SpeechManager.Instance.Speak("Just a moment, I'm still trying to get this portal opened...");
    }
    else
    {
      while (string.IsNullOrEmpty(_finalStory) || _finalImage == null) await Task.Delay(10);
    }
    _ = SpeechManager.Instance.Speak("The big portal is ready!");
    await AwaitableTakePicturePress("Tap to activate big portal");
    PortalManager.Instance.ActivatePortal();

    //Wait for activation of big portal
    while (!_bigPortalActivated) await Task.Delay(10);
    await SpeechManager.Instance.Speak(_finalStory);
  
    //TODO: GO AGAIN
    return adventureResult;
  }

  private async Task<Texture2D> GetCameraImage(string buttonText)
  {
    await AwaitableTakePicturePress(buttonText);
    //Run twice because TryAcquireLatestCpuImage is BROKEN AND RETURNS AN OLD FRAME
    var camImage = await CameraManager.Instance.GetNextAvailableCameraImage();
    camImage = await CameraManager.Instance.GetNextAvailableCameraImage();
    return camImage.Texture;
  }

  private async Task AwaitableTakePicturePress(string buttonText)
  {
    bool imageReady = false;
    Action onPictureCaptured = () =>
    {
      imageReady = true;
    };
    UIManager.Instance.TakePictureButton.Show(buttonText, onPictureCaptured);
    while (!imageReady) await Task.Delay(10);
    UIManager.Instance.TakePictureButton.Hide();
  }

  private List<string> GetItemStrings(string input)
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

  private async Task<Tuple<Request, string>> RunConvoUntilStateChanges(Request payload, StoryState state)
  {
    var stateReplyPair = new Tuple<StoryState, string>(state, string.Empty);
    while (stateReplyPair.Item1 == state)
    {
      await GetAudioCaptured();

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

  private async Task GetAudioCaptured()
  {
    //Show reply button and wait for user to respond with it
    var audioReady = false;
    Action audioCaptured = () =>
    {
      audioReady = true;
      AudioCaptureManager.Instance.EndAudioCapture();
    };
    UIManager.Instance.PushToTalkButton.Show(() => AudioCaptureManager.Instance.StartAudioCapture(), audioCaptured);
    while (!audioReady) await Task.Delay(10);
    //Hide reply button again
    UIManager.Instance.PushToTalkButton.Hide();
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

  private void ApplyCommentaryToMarkers(string reply)
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
        _captureMarketSequences[i - 1].SetCommentary(modifiedResponse);
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