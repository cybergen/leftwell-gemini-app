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
    await SpeechManager.Instance.Speak(FTEDialog.TUT_INTRO);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_SPEECH_INTRO);
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
      await SpeechManager.Instance.Speak(FTEDialog.TUT_SPEECH_SHORT);
      startTime = endTime = 0f;
      pressedAndReleasedButton = false;

      UIManager.Instance.LongPressButton.Show(onPress, onRelease);
      while (!pressedAndReleasedButton) { await Task.Delay(10); }
      UIManager.Instance.LongPressButton.Hide();
    }
    await SpeechManager.Instance.Speak(FTEDialog.TUT_SPEECH_GOOD);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);

    //Learn to capture image of Item of Power
    await SpeechManager.Instance.Speak(FTEDialog.TUT_CAM_INTRO);
    var capturedImage = await AdventureSequence.GetCameraImage("Tap to capture an Item of Power");
    dragon.SetState(CharacterState.ShownObject);
    var captureSequence = new CaptureMarkerSequence();
    //Set our dialog for tutorial finish on activation of the capture marker
    bool activatedPortal = false;
    captureSequence.SetCommentary(FTEDialog.TUT_ITEM_GOOD);
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
    dragon.SetState(CharacterState.JumpingToPlayer);
    await SpeechManager.Instance.Speak(FTEDialog.TUT_CAM_TALK_SUCCESS);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);

    //Learn to activate a transformed Item
    while (!PortalManager.Instance.GetAllMarkersActivatable()) { await Task.Delay(10); }
    Action onActivated = () => { activatedPortal = true; };
    await SpeechManager.Instance.Speak(FTEDialog.TUT_ITEM_READY);
    UIManager.Instance.PortalActivater.SetShowable(true, Camera.main.transform);
    while (!activatedPortal) { await Task.Delay(10); }
    UIManager.Instance.PortalActivater.SetShowable(false, null);    
    while (SpeechManager.Instance.Speaking) { await Task.Delay(10); }
    await Task.Delay(FTEDialog.DIALOG_PAUSE);

    //Ask whether to reply tutorial
    _ = SpeechManager.Instance.Speak(FTEDialog.TUT_DONE);
    var repliedToYesNo = false;
    var repeatTutorial = false;
    Action onYes = () => { repliedToYesNo = true; repeatTutorial = true; };
    Action onNo = () => { repliedToYesNo = true; repeatTutorial = false; };
    UIManager.Instance.YesNoScreen.Show("Repeat the tutorial?", onYes, onNo);
    while (!repliedToYesNo) { await Task.Delay(10); }

    //Fly away and break everything down
    await SpeechManager.Instance.Speak(FTEDialog.BE_RIGHT_BACK);
    dragon.SetState(CharacterState.FlyAway);
    await Task.Delay(FTEDialog.DIALOG_PAUSE);
    PortalManager.Instance.DestroyEverything();

    return repeatTutorial;
  }
}