using System.Threading.Tasks;
using UnityEngine;
using BriLib;
using LLM.Network;

public class AppStateManager : Singleton<AppStateManager>
{
  public NativeShare NativeShare;

  [SerializeField] private GameObject _arRig;
  [SerializeField] private AudioListener _preCameraAudioListener;
  [SerializeField] private AudioSource _preCharacterAudioSource;
  [SerializeField] private GameObject _preCameraBackdrop;
  [SerializeField] private GameObject _wizardPrefab;
  [SerializeField] private LongPressButton _pushToTalkButton;
  [SerializeField] private FadingAndSlidingScreen _takePictureButton;
  [SerializeField] private Texture2D _initializeImage;

  private const string HAS_RUN_TUTORIAL_KEY = "HasRunTutorial";

  private CharacterBehaviorController _character;
  private AudioSource _characterAudioSource;
  private LLMRequestPayload _previousLog;
  private AppState _currentState;

  public override void Begin()
  {
    base.Begin();
    SetState(AppState.Initialize);
  }

  public void LLMErrorBailout()
  {
    SetState(AppState.LLMError);
  }

  private async void SetState(AppState state)
  {
    switch (_currentState)
    {
      case AppState.Tutorial:
      case AppState.Adventure:
      case AppState.AdventureAgain:
        PortalManager.Instance.Initialize();
        break;
    }

    _currentState = state;

    //Reset character mode in case we exited a prior state at a weird time
    if (_character != null) { _character.SetMode(CharacterMode.Standard); }

    Debug.Log($"App state: {state}");
    switch (state)
    {
      case AppState.Initialize:
        _preCameraBackdrop.SetActive(false);
        UIManager.Instance.LoadingScreen.Show();

        ImagePromptGenerator.Instance.Initialize();
        PortalManager.Instance.Initialize();
        SpeechManager.Instance.SetSpeechSource(_preCharacterAudioSource);
        NativeShare = new NativeShare();

        while (!ImagePromptGenerator.Instance.ReadyToGenerate)
        {
          await Task.Delay(10);
          UIManager.Instance.LoadingScreen.SetProgress(ImagePromptGenerator.Instance.Progress);
        }
        _preCameraBackdrop.SetActive(true);
        UIManager.Instance.LoadingScreen.Hide();
        await Task.Delay(300);

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
          _arRig.SetActive(true);
          await Task.Delay(1000); //Wait a moment before dismissing camera backdrop
          _preCameraAudioListener.enabled = false;
          _preCameraBackdrop.SetActive(false);
          SetState(AppState.TitleScreen);
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
          SetState(AppState.TitleScreen);
        }
        break;
      case AppState.TitleScreen:
        await new TitleSequence().RunAsync();
        SetState(AppState.FindGround);
        break;
      case AppState.FindGround:
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
        if (PreferencesManager.Instance.GetBool(HAS_RUN_TUTORIAL_KEY))
        {
          SetState(AppState.Adventure);
        }
        else
        {
          SetState(AppState.Tutorial);
        }
        break;
      case AppState.Tutorial:
        PreferencesManager.Instance.SetBool(HAS_RUN_TUTORIAL_KEY, true);
        var redoTutorial = await new TutorialSequence().RunAsync(_character);
        if (redoTutorial) { SetState(AppState.Tutorial); }
        else { SetState(AppState.Adventure); }
        break;
      case AppState.Adventure:
        var dependencies = new AdventureDependencies
        {
          Character = _character,
          IsRepeat = false,
          ExistingPayload = null,
        };
        var result = await new AdventureSequence().RunAsync(dependencies);
        if (result.Repeat)
        {
          _previousLog = result.Chat;
          SetState(AppState.AdventureAgain);
        }
        break;
      case AppState.AdventureAgain:
        var repeatDependencies = new AdventureDependencies
        {
          Character = _character,
          IsRepeat = true,
          ExistingPayload = _previousLog,
        };
        var secondOutcome = await new AdventureSequence().RunAsync(repeatDependencies);
        if (secondOutcome.Repeat)
        {
          _previousLog = secondOutcome.Chat;
          SetState(AppState.AdventureAgain);
        }
        break;
      case AppState.LLMError:
        if (_character != null)
        {
          _character.SetState(CharacterState.JumpingToPlayer);
          while (_character.BusyPathing) { await Task.Delay(10); }
        }
        await SpeechManager.Instance.Speak(FTEDialog.LLM_ERROR);
        Application.Quit();
        break;
    }
  }
}

public enum AppState
{
  Initialize,
  TitleScreen,
  CheckPermissions,
  RequestPermissions,
  FindGround,
  CheckTutorial,
  Tutorial,
  Adventure,
  AdventureAgain,
  LLMError
}