using UnityEngine;
using TMPro;
using BriLib;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using System;
using System.Threading.Tasks;
using TTSConstants = FrostweepGames.Plugins.GoogleCloud.TextToSpeech.Constants;

public class AppStateManager : Singleton<AppStateManager>
{
  [SerializeField] private GameObject _arRig;
  [SerializeField] private AudioListener _preCameraAudioListener;
  [SerializeField] private AudioSource _preCharacterAudioSource;
  [SerializeField] private GameObject _wizardPrefab;
  [SerializeField] private TMP_Text _outputText;

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
    _outputText.text = $"App state: {state}";
    Debug.LogWarning($"App state: {state}");
    switch (state)
    {
      case AppState.Initialize:
        ImagePromptGenerator.Instance.Initialize();
        SpeechManager.Instance.Initialize();
        SpeechManager.Instance.SetSpeechSource(_preCharacterAudioSource);
        await SpeechManager.Instance.SpeakSSML($"<speak>Here's an example of <break time=\"1s\"/>pre character audio</speak>");
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
        if (!await new RequestPermissionsSequence().RunAsync())
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
        _preCharacterAudioSource.enabled = false;
        _arRig.SetActive(true);
        await new FindGroundSequence().RunAsync();
        var go = Instantiate(_wizardPrefab, new Vector3(0f, 500f), Quaternion.identity);
        _character = go?.GetComponent<CharacterBehaviorController>();
        _characterAudioSource = go?.GetComponent<AudioSource>();
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