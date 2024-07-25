using UnityEngine;
using TMPro;
using BriLib;

public class AppStateManager : Singleton<AppStateManager>
{
  [SerializeField] private GameObject _arRig;
  [SerializeField] private AudioListener _preCameraAudioListener;
  [SerializeField] private AudioSource _preCharacterAudioSource;
  [SerializeField] private GameObject _preCameraBackdrop;
  [SerializeField] private GameObject _wizardPrefab;
  [SerializeField] private LongPressButton _pushToTalkButton;
  [SerializeField] private FullScreenTapButton _takePictureButton;

  private const string TUTORIAL_PREFS_KEY = "HasRunTutorial";

  private CharacterBehaviorController _character;
  private AudioSource _characterAudioSource;

  public override void Begin()
  {
    base.Begin();
    SetState(AppState.Initialize);
  }

  private async void SetState(AppState state)
  {
    Debug.Log($"App state: {state}");
    switch (state)
    {
      case AppState.Initialize:
        ImagePromptGenerator.Instance.Initialize();
        SpeechManager.Instance.Initialize();
        SpeechManager.Instance.SetSpeechSource(_preCharacterAudioSource);
        SetState(AppState.CheckPermissions);
        break;
      case AppState.CheckPermissions:
        if (!PermissionsManager.Instance.CheckPermission(AppPermission.Microphone)
          || !PermissionsManager.Instance.CheckPermission(AppPermission.Camera))
        {
          SetState(AppState.RequestPermissions);
        }
        else
        {
          SetState(AppState.FindGround);
        }
        break;
      case AppState.RequestPermissions:
        var rigBundle = new CameraRigBundle
        {
          CameraRig = _arRig,
          PreCameraAudioListener = _preCameraAudioListener,
          PreCameraBackdrop = _preCameraBackdrop
        };
        if (!await new RequestPermissionsSequence().RunAsync(rigBundle))
        {
          Debug.LogError("User rejected device permissions, re-running");
          SetState(AppState.CheckPermissions);
        }
        else
        {
          SetState(AppState.FindGround);
        }
        break;
      case AppState.FindGround:
        _preCameraAudioListener.enabled = false;
        _preCameraBackdrop.SetActive(false);
        _arRig.SetActive(true);
        await new FindGroundSequence().RunAsync();
        var go = Instantiate(_wizardPrefab, new Vector3(0f, 500f), Quaternion.identity);
        _character = go?.GetComponent<CharacterBehaviorController>();
        _characterAudioSource = go?.GetComponent<AudioSource>();
        _preCharacterAudioSource.enabled = false;
        if (_character == null || _characterAudioSource == null)
        {
          Debug.LogError($"Failed to create character or prefab missing required components, re-running");
        }
        else
        {
          SpeechManager.Instance.SetSpeechSource(_characterAudioSource);
          SetState(AppState.CheckTutorial);
        }
        break;
      case AppState.CheckTutorial:
        if (PreferencesManager.Instance.GetBool(TUTORIAL_PREFS_KEY))
        {
          SetState(AppState.Adventure);
        }
        else
        {
          //TODO: Implement actual tutorial! For now, just skip to adventure
          //SetState(AppState.Tutorial);
          SetState(AppState.Adventure);
        }
        break;
      case AppState.Tutorial:
        Debug.LogError("Tutorial not yet implemented");
        break;
      case AppState.Adventure:
        var result = await new AdventureSequence().RunAsync(_character);
        //TODO: Save images and history stuff
        //TODO: Allow running a second sequence
        break;
    }
  }
}

public enum AppState
{
  Initialize,
  CheckPermissions,
  RequestPermissions,
  FindGround,
  CheckTutorial,
  Tutorial,
  Adventure
}