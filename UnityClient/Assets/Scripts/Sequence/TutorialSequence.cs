using System;
using System.Threading.Tasks;
using UnityEngine;

public class TutorialSequence : ISequence<CharacterBehaviorController, bool>
{
  private const float MIN_SPEAK_DURATION = 0.5f;

  public async Task<bool> RunAsync(CharacterBehaviorController dragon)
  {
    dragon.SetState(CharacterState.InitialFlyIn);
    while (dragon.BusyPathing) { await Task.Delay(10); }

    //Learn to use push to talk button
    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_INTRO);
    dragon.SetState(CharacterState.IdleWithPlayer);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);

    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_SPEECH_INTRO);
    dragon.SetState(CharacterState.IdleWithPlayer);

    bool pressedAndReleasedButton = false;
    float startTime = 0f;
    float endTime = 0f;
    Action onPress = () => { startTime = Time.time; };
    Action onRelease = () => { endTime = Time.time; pressedAndReleasedButton = true; };
    UIManager.Instance.LongPressButton.Show(onPress, onRelease);
    while (!pressedAndReleasedButton) { await Task.Delay(10); }
    UIManager.Instance.LongPressButton.Hide();

    //If pressed duration too short, repeat
    while (endTime - startTime < MIN_SPEAK_DURATION)
    {
      dragon.SetState(CharacterState.Talking);
      await SpeechManager.Instance.Speak(FTEDialog.TUT_SPEECH_SHORT);
      dragon.SetState(CharacterState.IdleWithPlayer);
      startTime = endTime = 0f;
      pressedAndReleasedButton = false;

      UIManager.Instance.LongPressButton.Show(onPress, onRelease);
      while (!pressedAndReleasedButton) { await Task.Delay(10); }
      UIManager.Instance.LongPressButton.Hide();
    }

    //Congratulate player on audio capture
    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_SPEECH_GOOD);
    dragon.SetState(CharacterState.IdleWithPlayer);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);

    //Learn to capture image of Item of Power
    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_CAM_INTRO);
    dragon.SetState(CharacterState.IdleWithPlayer);
    var capturedImage = await AdventureSequence.GetCameraImage("Tap to capture an Item of Power");
    dragon.SetState(CharacterState.ShownObject);
    var captureSequence = new CaptureMarkerSequence("Item of Power");

    //Kick off marker image edit sequence
    bool activatedPortal = false;
    _ = captureSequence.RunAsync(capturedImage).ContinueWith((s) =>
    {
      activatedPortal = true;
    });

    //Request description of Item
    await SpeechManager.Instance.Speak(FTEDialog.TUT_CAM_TALK);
    startTime = endTime = 0f;
    pressedAndReleasedButton = false;
    UIManager.Instance.LongPressButton.Show(onPress, onRelease);
    while (!pressedAndReleasedButton) { await Task.Delay(10); }
    UIManager.Instance.LongPressButton.Hide();

    //If pressed duration too short, repeat again
    while (endTime - startTime < MIN_SPEAK_DURATION)
    {
      await SpeechManager.Instance.Speak(FTEDialog.TUT_CAM_TALK_SHORT);
      startTime = endTime = 0f;
      pressedAndReleasedButton = false;

      UIManager.Instance.LongPressButton.Show(onPress, onRelease);
      while (!pressedAndReleasedButton) { await Task.Delay(10); }
      UIManager.Instance.LongPressButton.Hide();
    }

    //Sprinkle some magic and congratulate player
    dragon.SetState(CharacterState.MagicingItem);
    while (dragon.BusyPathing) { await Task.Delay(10); }
    dragon.SetState(CharacterState.JumpingToPlayer);

    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_CAM_TALK_SUCCESS);
    dragon.SetState(CharacterState.IdleWithPlayer);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);
    
    //Set dialog and wait for marker to finish image edit and become activatable before proceeding
    captureSequence.SetCommentary(FTEDialog.TUT_ITEM_GOOD);
    while (!PortalManager.Instance.GetAllMarkersActivatable()) { await Task.Delay(10); }
    Action onActivated = () => { activatedPortal = true; };

    //Learn to activate a transformed Item
    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_ITEM_READY);
    dragon.SetState(CharacterState.IdleWithPlayer);

    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);
    while (!activatedPortal) { await Task.Delay(10); }
    await Task.Delay(FTEDialog.DIALOG_PAUSE);
    while (SpeechManager.Instance.Speaking) { await Task.Delay(10); }
    await Task.Delay(FTEDialog.DIALOG_PAUSE);

    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_ITEM_SHARABLE);
    dragon.SetState(CharacterState.IdleWithPlayer);
    await Task.Delay(FTEDialog.SHARE_PAUSE);
    UIManager.Instance.PortalActivater.SetShowable(false, null);

    //Ask whether to reply tutorial
    dragon.SetState(CharacterState.Talking);
    _ = SpeechManager.Instance.Speak(FTEDialog.TUT_DONE);
    var repliedToYesNo = false;
    var repeatTutorial = false;
    Action onYes = () => { repliedToYesNo = true; repeatTutorial = true; };
    Action onNo = () => { repliedToYesNo = true; repeatTutorial = false; };
    UIManager.Instance.YesNoScreen.Show("Repeat the tutorial?", onYes, onNo);
    dragon.SetState(CharacterState.IdleWithPlayer);
    while (!repliedToYesNo) { await Task.Delay(10); }

    //Fly away and break everything down
    dragon.SetState(CharacterState.Talking);
    await SpeechManager.Instance.Speak(FTEDialog.BE_RIGHT_BACK);
    dragon.SetState(CharacterState.FlyAway);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);
    PortalManager.Instance.DestroyEverything();

    return repeatTutorial;
  }
}