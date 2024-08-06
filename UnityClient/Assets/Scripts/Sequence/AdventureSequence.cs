using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using LLM.Network;
using Request = LLM.Network.LLMRequestPayload;

public class AdventureSequence : ISequence<AdventureDependencies, AdventureResult>
{
  private const int ITEM_COUNT = 3;
  private int _activatedPortals = 0;
  private List<CaptureMarkerSequence> _captureMarkerSequences = new List<CaptureMarkerSequence>();
  private Texture2D _finalImage;
  private string _finalStory;
  private bool _bigPortalActivated = false;
  private int _imagesUploaded = 0;
  private int _audioUploaded = 0;
  private Request _payload;
  private CharacterAnimationController _charAnimator;

  public async Task<AdventureResult> RunAsync(AdventureDependencies dependencies)
  {
    if (!dependencies.IsRepeat)
    { 
      _payload = CreateInitialPayload();
    }
    else 
    { 
      _payload = dependencies.ExistingPayload;
      _payload.contents[_payload.contents.Count - 1].parts.Add(new TextPart { text = StoryPromptSettings.RESTART_TRIGGER });
    }

    var character = dependencies.Character;
    _charAnimator = character.gameObject.GetComponent<CharacterAnimationController>();

    //Get initial text from LLM
    var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_payload);
    _payload = payloadReplyPair.Item1;
    var stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);

    //Make wizard fly in and start talking while arriving
    character.SetState(CharacterState.InitialFlyIn);
    _ = SpeechManager.Instance.Speak(stateReplyPair.Item2);
    while (character.BusyPathing) await Task.Delay(10);
    CharacterHelpers.AnimateEventually(CharacterState.Talking, character);

    //Run intro convo until state changes
    if (!dependencies.IsRepeat) 
    { 
      _payload = (await TalkUntilStateChanges(_payload, StoryState.Intro, character)).Item1; 
    }
    
    //Choose which story to play
    var storyOption = await new ChooseStorySequence().RunAsync(character);
    _payload.contents[_payload.contents.Count - 1].parts.Add(new TextPart
    {
      text = storyOption
    });
    payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_payload);
    _payload = payloadReplyPair.Item1;
    stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);
    while (SpeechManager.Instance.Speaking || SpeechManager.Instance.Loading) { await Task.Delay(10); }
    CharacterHelpers.AnimateEventually(CharacterState.Talking, character);
    await SpeechManager.Instance.Speak(stateReplyPair.Item2);

    //Add images of magical items (and audio descriptions) to payload one by one
    var itemStrings = AdventureDialog.GetItemStrings(ITEM_COUNT);// GetItemStrings(convoResult.Item2);
    for (int i = 0; i < ITEM_COUNT; i++)
    {
      _ = SpeechManager.Instance.Speak(itemStrings[i]);
      character.SetState(CharacterState.Talking);
      var tex = await GetCameraImage(itemStrings[i]);

      //Add a delay so audio UI and picture UI don't stomp on each other
      await Task.Delay(450);
      character.SetState(CharacterState.ShownObject);

      //Kick off image editing sequence in the background
      //Increment activated items after each one has been triggered to move on to the big portal
      var captureMarkerSequence = new CaptureMarkerSequence(itemStrings[i]);
      _ = captureMarkerSequence.RunAsync(tex).ContinueWith((task) => _activatedPortals++);
      _captureMarkerSequences.Add(captureMarkerSequence);

      //Kick off addition of image to payload
      _ = CameraManager.Instance.UploadImageAndGetFilePart(tex).ContinueWith((task) =>
      {
        _imagesUploaded++;
        //Need to add like this to avoid out of order execution stomping on each other
        _payload.contents[_payload.contents.Count - 1].parts.Add(task.Result);
      });

      await SpeechManager.Instance.Speak(AdventureDialog.GetRandomItemCaptureDialog());
      await UseAudioCaptureUI();

      character.SetState(CharacterState.MagicingItem);
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
      await Task.Delay(AdventureDialog.DIALOG_PAUSE);
      _ = SpeechManager.Instance.Speak(AdventureDialog.GetRandomMagicAppliedDialog());

      //Add a delay so audio UI, picture UI, and dragon animation sequence don't stomp on each other
      await Task.Delay(4000);
    }

    //Get starting pose for big portal
    _ = SpeechManager.Instance.Speak(AdventureDialog.POSITION_PORTAL);
    await UseFullScreenTapUI("Tap to spawn big portal", false);
    PortalManager.Instance.SpawnHeroPortal();
    character.SetState(CharacterState.FlyingToPortal);
    _ = SpeechManager.Instance.Speak(AdventureDialog.PORTAL_PLACED);

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
    while (SpeechManager.Instance.Speaking || SpeechManager.Instance.Loading) { await Task.Delay(10); }
    _charAnimator.PlayOnce(DragonAnimation.Roar, DragonAnimation.Fly);
    _ = SpeechManager.Instance.Speak(AdventureDialog.ITEMS_READY);

    //Require user explores magical items before advancing
    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);
    while (_activatedPortals < ITEM_COUNT) await Task.Delay(10);
    await Task.Delay(AdventureDialog.DIALOG_PAUSE);
    UIManager.Instance.PortalActivater.SetShowable(false, null);

    //Wait for results for big portal to be available before advancing at this point
    while (SpeechManager.Instance.Speaking || SpeechManager.Instance.Loading) { await Task.Delay(10); }
    if (string.IsNullOrEmpty(_finalStory) || _finalImage == null)
    {
      _charAnimator.PlayOnce(DragonAnimation.Damage, DragonAnimation.Fly);
      _ = SpeechManager.Instance.Speak(AdventureDialog.PORTAL_NOT_READY);
      while (string.IsNullOrEmpty(_finalStory) || _finalImage == null) await Task.Delay(10);
    }

    PortalManager.Instance.SetHeroPortalActivatable(() => _bigPortalActivated = true);
    await Task.Delay(AdventureDialog.DIALOG_PAUSE);
    while (SpeechManager.Instance.Speaking || SpeechManager.Instance.Loading) { await Task.Delay(10); }
    _charAnimator.PlayOnce(DragonAnimation.Fire, DragonAnimation.Jump);
    _ = SpeechManager.Instance.Speak(AdventureDialog.PORTAL_READY);
    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);

    //Wait for activation of big portal
    while (!_bigPortalActivated) await Task.Delay(10);
    UIManager.Instance.PortalActivater.SetShowable(false, null);
    await Task.Delay(AdventureDialog.DIALOG_PAUSE);
    while (SpeechManager.Instance.Speaking || SpeechManager.Instance.Loading) { await Task.Delay(10); }
    character.SetState(CharacterState.FlyingToPlayer);
    _ = SpeechManager.Instance.Speak(AdventureDialog.OPENING_PORTAL);
    await Task.Delay(6000);

    //Speak final story while showing the UI
    CharacterHelpers.AnimateEventually(CharacterState.Talking, character);
    _ = SpeechManager.Instance.Speak(_finalStory);
    bool hidden = false;
    Action onHide = () => { hidden = true; };
    Action<bool> onShare = (successful) => { Debug.Log($"Share was {successful}"); };
    UIManager.Instance.StoryResult.Show(_finalImage, _finalStory, onHide, onShare);
    while (!hidden) { await Task.Delay(10); }
    _ = SpeechManager.Instance.Speak(AdventureDialog.CLOSE_PORTAL_QUESTION);
    await Task.Delay(1000);

    //Wait for portal closing before moving on
    bool portalClosed = false;
    Action onPortalClosed = () => { portalClosed = true; };
    PortalManager.Instance.SetHeroPortalClosable(onPortalClosed);
    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);
    while (!portalClosed) { await Task.Delay(10); }
    UIManager.Instance.PortalActivater.SetShowable(false, Camera.main.transform);
    await Task.Delay(1000);

    //Ask whether to go again
    _ = SpeechManager.Instance.Speak(AdventureDialog.GO_AGAIN_QUESTION);
    var repliedToYesNo = false;
    var repeatAdventure = false;
    Action onYes = () => { repliedToYesNo = true; repeatAdventure = true; };
    Action onNo = () => { repliedToYesNo = true; repeatAdventure = false; };
    UIManager.Instance.YesNoScreen.Show("Go on another adventure?", onYes, onNo);
    while (!repliedToYesNo) { await Task.Delay(10); }

    //Fly off
    if (repeatAdventure)
    {
      await SpeechManager.Instance.Speak(FTEDialog.BE_RIGHT_BACK);
      character.SetState(CharacterState.FlyAway);
      await Task.Delay(FTEDialog.DIALOG_PAUSE);
      PortalManager.Instance.DestroyEverything();
    }
    else
    {
      //Enable share UI again
      UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);

      character.SetState(CharacterState.FlyingToPlayer);
      var textPart = new TextPart { text = StoryPromptSettings.FREE_CONVERSE_TRIGGER };
      _payload.contents[_payload.contents.Count - 1].parts.Add(textPart);
      payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(_payload);
      _payload = payloadReplyPair.Item1;
      stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);
      await SpeechManager.Instance.Speak(stateReplyPair.Item2);
      Action onUIShown = () => { UIManager.Instance.PortalActivater.SetShowable(false, null); };
      Action onUIHidden = () => { UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform); };
      payloadReplyPair = await TalkUntilStateChanges(_payload, StoryState.FreeConversation, character, onUIShown, onUIHidden);
    }

    return new AdventureResult
    {
      ResultImage = _finalImage,
      Chat = _payload,
      Synopsis = _finalStory,
      Repeat = repeatAdventure
    };
  }

  public static async Task<Texture2D> GetCameraImage(string buttonText)
  {
    await UseFullScreenTapUI(buttonText);
    //Run twice because TryAcquireLatestCpuImage is BROKEN AND RETURNS AN OLD FRAME
    await CameraManager.Instance.GetNextAvailableCameraImage();
    var camImage = await CameraManager.Instance.GetNextAvailableCameraImage();
    return camImage.Texture;
  }

  public async Task UseAudioCaptureUI()
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

  private async Task<Tuple<Request, string>> TalkUntilStateChanges(Request payload, StoryState state, CharacterBehaviorController character, Action onUIShown = null, Action onUIHidden = null)
  {
    var stateReplyPair = new Tuple<StoryState, string>(state, string.Empty);
    while (stateReplyPair.Item1 == state)
    {
      onUIShown?.Invoke();
      await UseAudioCaptureUI();
      onUIHidden?.Invoke();

      //Capture audio, upload it, and use it in next LLM request
      payload = await AudioCaptureManager.Instance.GetAudioAndAddToRequest(payload);
      var payloadReplyPair = await LLMInteractionManager.Instance.SendRequestAndUpdateSequence(payload);

      //Update state for checking if we've moved on or not
      payload = payloadReplyPair.Item1;
      stateReplyPair = ParseInfoFromReply(payloadReplyPair.Item2);

      //Say latest message
      while (SpeechManager.Instance.Speaking || SpeechManager.Instance.Loading) { await Task.Delay(10); }
      CharacterHelpers.AnimateEventually(CharacterState.Talking, character);
      await SpeechManager.Instance.Speak(stateReplyPair.Item2);
    }
    return new Tuple<Request, string>(payload, stateReplyPair.Item2);
  }

  public static async Task UseFullScreenTapUI(string buttonText, bool useTakePicture = true)
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
      return new Tuple<StoryState, string>(StoryState.None, reply);
    }
  }

  private void ApplyCommentaryToMarkers(string reply)
  {
    //Persist random so we get the next random in sequence without repeating due to Time as seed
    var rand = new System.Random();
    for (int i = 1; i <= 3; i++)
    {
      var pattern = @"^Item " + i + @":\s*(.*)$";
      Regex regex = new Regex(pattern, RegexOptions.Multiline);
      Match match = regex.Match(reply);

      if (match.Success)
      {
        string matchedLine = match.Groups[1].Value.Trim();
        string modifiedResponse = regex.Replace(matchedLine, string.Empty).Trim();
        _captureMarkerSequences[i - 1].SetCommentary(modifiedResponse);
      }
      else
      {
        Debug.LogError($"Failed to get item {i} from reply");
        _captureMarkerSequences[i - 1].SetCommentary(AdventureDialog.GetRandomFailedCommentary(rand));
      }
    }
  }

  private Request CreateInitialPayload()
  {
    var payload = new Request();

    payload.generationConfig = new GenerationConfig
    {
      temperature = StoryPromptSettings.STORY_TWO_TEMPERATURE
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
      parts = new List<BasePart> { new TextPart { text = StoryPromptSettings.STORY_PROMPT_TWO_FIRST_TIME } }
    };

    payload.contents = new List<Content>
    {
      new Content
      {
        parts = new List<BasePart>{ new TextPart { text = StoryPromptSettings.BEGIN_TRIGGER } },
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
  FreeConversation
}

public class AdventureDependencies
{
  public CharacterBehaviorController Character;
  public Request ExistingPayload;
  public bool IsRepeat;
}

public class AdventureResult
{
  public Texture2D ResultImage;
  public string Synopsis;
  public Request Chat;
  public bool Repeat;
}