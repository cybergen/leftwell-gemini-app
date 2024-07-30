using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using LLM.Network;
using Request = LLM.Network.LLMRequestPayload;

public class AdventureSequence : ISequence<CharacterBehaviorController, AdventureResult>
{
  private const int ITEM_COUNT = 3;
  private int _activatedPortals = 0;
  private List<CaptureMarkerSequence> _captureMarketSequences = new List<CaptureMarkerSequence>();
  private Texture2D _finalImage;
  private string _finalStory;
  private bool _bigPortalActivated = false;
  private int _imagesUploaded = 0;
  private int _audioUploaded = 0;
  private Request _payload;

  public async Task<AdventureResult> RunAsync(CharacterBehaviorController character)
  {
    _payload = CreateInitialPayload();

    //Get intro text from LLM
    var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_payload);
    _payload = payloadReplyPair.Item1;
    var stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);

    //Make wizard fly in and start talking while arriving
    character.SetState(CharacterStates.InitialFlyIn);
    _ = SpeechManager.Instance.Speak(stateReplyPair.Item2);
    while (character.BusyPathing) await Task.Delay(10);

    //Run intro convo until state changes
    _payload = (await RunConvoUntilStateChanges(_payload, StoryState.Intro)).Item1;
    
    //Choose which story to play
    //var convoResult = await RunConvoUntilStateChanges(_payload, StoryState.StorySelect);
    //_payload = convoResult.Item1;
    var storyOption = await new ChooseStorySequence().RunAsync();
    _payload.contents[_payload.contents.Count - 1].parts.Add(new TextPart
    {
      text = storyOption
    });
    payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_payload);
    _payload = payloadReplyPair.Item1;
    stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);
    await SpeechManager.Instance.Speak(stateReplyPair.Item2);

    //Add images of magical items (and audio descriptions) to payload one by one
    var itemStrings = DialogConstants.GetItemStrings(ITEM_COUNT);// GetItemStrings(convoResult.Item2);
    for (int i = 0; i < ITEM_COUNT; i++)
    {
      _ = SpeechManager.Instance.Speak(itemStrings[i]);
      var tex = await GetCameraImage(itemStrings[i]);

      //Add a delay so audio UI and picture UI don't stomp on each other
      await Task.Delay(450);
      character.SetState(CharacterStates.ShownObject);

      //Kick off image editing sequence in the background
      //Increment activated items after each one has been triggered to move on to the big portal
      var captureMarkerSequence = new CaptureMarkerSequence();
      _ = captureMarkerSequence.RunAsync(tex).ContinueWith((task) => _activatedPortals++);
      _captureMarketSequences.Add(captureMarkerSequence);

      //Kick off addition of image to payload
      _ = CameraManager.Instance.UploadImageAndGetFilePart(tex).ContinueWith((task) =>
      {
        _imagesUploaded++;
        //Need to add like this to avoid out of order execution stomping on each other
        _payload.contents[_payload.contents.Count - 1].parts.Add(task.Result);
      });

      await SpeechManager.Instance.Speak(DialogConstants.GetRandomItemCaptureDialog());
      await UseAudioCaptureUI();

      character.SetState(CharacterStates.JumpingToPlayer);
      _ = AudioCaptureManager.Instance.GetAudioAndUpload().ContinueWith((task) =>
      {
        _audioUploaded++;
        //Need to add like this to avoid out of order execution stomping on each other
        _payload.contents[_payload.contents.Count - 1].parts.Add(task.Result);
        //After our audio has been added, add text as well
        _payload.contents[_payload.contents.Count - 1].parts.Add(new TextPart
        {
          text = $"Item {i}: {itemStrings[i]}"
        });
      });

      //Add a delay so audio UI and picture UI don't stomp on each other
      await Task.Delay(450);
    }

    //Get starting pose for big portal
    _ = SpeechManager.Instance.Speak(DialogConstants.POSITION_PORTAL);
    await UseFullScreenTapUI("Tap to spawn big portal", false);
    PortalManager.Instance.SpawnBigPortal();
    character.SetState(CharacterStates.FlyingToPortal);
    _ = SpeechManager.Instance.Speak(DialogConstants.PORTAL_PLACED);

    //Ensure all images and audio are ready in the payload before advancing
    while (_imagesUploaded < ITEM_COUNT || _audioUploaded < ITEM_COUNT) await Task.Delay(10);

    //Get commentary for each item and apply
    payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_payload);
    _payload = payloadReplyPair.Item1;
    ApplyCommentaryToMarkers(payloadReplyPair.Item2);

    //Immediately kick off final story sequence
    _ = new StoryAndImagePromptSeqeuence().RunAsync(_payload).ContinueWith((task) =>
    {
      _finalImage = task.Result.FinalImage;
      _payload = task.Result.Payload;
      _finalStory = ParseInfoFromReply(task.Result.Reply).Item2;
    });

    //Wait for both edited image and commentary reply to come in before allowing activation
    while (!PortalManager.Instance.GetAllMarkersActivatable()) await Task.Delay(10);
    _ = SpeechManager.Instance.Speak(DialogConstants.ITEMS_READY);

    //Require user explores magical items before advancing
    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);
    while (_activatedPortals < 3) await Task.Delay(10);
    UIManager.Instance.PortalActivater.SetShowable(false, Camera.main.transform);

    //Wait for results for big portal to be available before advancing at this point
    if (string.IsNullOrEmpty(_finalStory) || _finalImage != null)
    {
      _ = SpeechManager.Instance.Speak(DialogConstants.PORTAL_NOT_READY);
      while (string.IsNullOrEmpty(_finalStory) || _finalImage == null) await Task.Delay(10);
    }

    PortalManager.Instance.SetBigPortalActivatable(() => _bigPortalActivated = true);
    _ = SpeechManager.Instance.Speak(DialogConstants.PORTAL_READY);
    await Task.Delay(2000);
    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);

    //Wait for activation of big portal
    while (!_bigPortalActivated) await Task.Delay(10);
    UIManager.Instance.PortalActivater.SetShowable(false, null);
    _ = SpeechManager.Instance.Speak("It's actually opening! I can't believe we-I mean, completely as expected!");
    await Task.Delay(4000);

    //Speak final story while showing the UI
    _ = SpeechManager.Instance.Speak(_finalStory);
    bool hidden = false;
    Action onHide = () => { hidden = true; };
    Action<bool> onShare = (successful) => { Debug.Log($"Share was {successful}"); };
    UIManager.Instance.StoryResult.Show(_finalImage, _finalStory, onHide, onShare);
    while (!hidden) { await Task.Delay(10); }
    _ = SpeechManager.Instance.Speak("I'll just close this little rip in reality back up then.");
    await Task.Delay(1000);
    PortalManager.Instance.SetBigPortalClosable(null);
    PortalManager.Instance.ActivatePortal();

    //TODO: GO AGAIN
    return new AdventureResult
    {
      ResultImage = _finalImage,
      Chat = _payload,
      Synopsis = _finalStory,
    };
  }

  private async Task<Texture2D> GetCameraImage(string buttonText)
  {
    await UseFullScreenTapUI(buttonText);
    //Run twice because TryAcquireLatestCpuImage is BROKEN AND RETURNS AN OLD FRAME
    await CameraManager.Instance.GetNextAvailableCameraImage();
    var camImage = await CameraManager.Instance.GetNextAvailableCameraImage();
    return camImage.Texture;
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

  private async Task<Tuple<Request, string>> RunConvoUntilStateChanges(Request payload, StoryState state)
  {
    var stateReplyPair = new Tuple<StoryState, string>(state, string.Empty);
    while (stateReplyPair.Item1 == state)
    {
      await UseAudioCaptureUI();

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

  private async Task UseAudioCaptureUI()
  {
    var audioReady = false;
    Action audioCaptured = () =>
    {
      audioReady = true;
      AudioCaptureManager.Instance.EndAudioCapture();
    };
    UIManager.Instance.LongPressButton.Show(() => AudioCaptureManager.Instance.StartAudioCapture(), audioCaptured);
    while (!audioReady) await Task.Delay(10);
    UIManager.Instance.LongPressButton.Hide();
  }

  private async Task UseFullScreenTapUI(string buttonText, bool useTakePicture = true)
  {
    bool imageReady = false;
    Action onPictureCaptured = () =>
    {
      imageReady = true;
    };

    if (useTakePicture) { UIManager.Instance.TakePictureButton.Show(buttonText, onPictureCaptured); }
    else { UIManager.Instance.PortalSpawn.Show(buttonText, onPictureCaptured); }

    while (!imageReady) await Task.Delay(10);
    UIManager.Instance.TakePictureButton.Hide();
    UIManager.Instance.PortalSpawn.Hide();
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
        string matchedLine = match.Groups[1].Value.Trim();
        string modifiedResponse = regex.Replace(matchedLine, string.Empty).Trim();
        _captureMarketSequences[i - 1].SetCommentary(modifiedResponse);
      }
      else
      {
        Debug.LogError($"Failed to get item {i} from reply");
      }
    }
  }

  private Request CreateInitialPayload()
  {
    var payload = new Request();

    payload.generationConfig = new GenerationConfig
    {
      temperature = PromptConstants.STORY_TWO_TEMPERATURE
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
      parts = new List<BasePart> { new TextPart { text = PromptConstants.STORY_PROMPT_TWO_FIRST_TIME } }
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
  public Request Chat;
}