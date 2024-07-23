using System;
using System.Threading.Tasks;
using UnityEngine;
using FrostweepGames.Plugins.GoogleCloud.TextToSpeech;
using TTSConstants = FrostweepGames.Plugins.GoogleCloud.TextToSpeech.Constants;
using BriLib;

public class SpeechManager : Singleton<SpeechManager>
{
  private AudioSource _speechSource;
  private Action _onSpeakingFailed;
  private Action _onSpeakingSuccessful;

  public void Initialize()
  {
    GCTextToSpeech.Instance.apiKey = Config.Instance.ApiKey;
    GCTextToSpeech.Instance.SynthesizeSuccessEvent += OnVoiceSynthesizeSuccess;
    GCTextToSpeech.Instance.SynthesizeFailedEvent += OnVoiceSynthesizeFail;
  }

  public void SetSpeechSource(AudioSource source)
  {
    _speechSource = source;
  }

  public async Task Speak(string something, bool ssml=true)
  {
    GCTextToSpeech.Instance.Synthesize(something, new VoiceConfig()
    {
      gender = Constants.SYNTH_GENDER,
      languageCode = GCTextToSpeech.Instance.PrepareLanguage(Constants.SYNTH_LOCALE),
      name = Constants.SYNTH_VOICE
    },
      ssml,
      Constants.SYNTH_PITCH,
      Constants.SYNTH_SPEAKING_RATE,
      Constants.SYNTH_SAMPLE_RATE_HERTZ,
      new Enumerators.EffectsProfileId[] { });

    var outcomeTriggered = false;

    void onFailed() { outcomeTriggered = true; }
    void onSucceeded() { outcomeTriggered = true; }
    _onSpeakingFailed += onFailed;
    _onSpeakingSuccessful += onSucceeded;

    while (!outcomeTriggered) { await Task.Delay(10); }

    _onSpeakingFailed -= onFailed;
    _onSpeakingSuccessful -= onSucceeded;
  }

  private void OnVoiceSynthesizeFail(string arg1, long arg2)
  {
    Debug.LogError($"Voice synthesis failed with message {arg1}");
    _onSpeakingFailed?.Invoke();
  }

  private async void OnVoiceSynthesizeSuccess(PostSynthesizeResponse response, long arg2)
  {
    _speechSource.Stop();
    _speechSource.clip = GCTextToSpeech.Instance.GetAudioClipFromBase64(response.audioContent, TTSConstants.DEFAULT_AUDIO_ENCODING);
    _speechSource.Play();
    await Task.Delay(10);// (int)(_speechSource.clip.length * 1000));
    _onSpeakingSuccessful?.Invoke();
  }
}