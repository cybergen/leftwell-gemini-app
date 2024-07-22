using System.Threading.Tasks;
using UnityEngine;

public class RequestPermissionsSequence : ISequence<CameraRigBundle, bool>
{
  public async Task<bool> RunAsync(CameraRigBundle rigBundle)
  {
    var gotMicrophonePermissions = new TaskCompletionSource<bool>();
    if (!PermissionsManager.Instance.CheckPermission(AppPermission.Microphone))
    {
      await SpeechManager.Instance.Speak($"<speak>Hey, I can't hear what you're saying. Please give me permission to hear your voice</speak>");
      PermissionsManager.Instance.RequestPermission(
        AppPermission.Microphone, (result) => gotMicrophonePermissions.SetResult(result));
      await gotMicrophonePermissions.Task;
      //TODO: Respond if rejected
    }

    if (!PermissionsManager.Instance.CheckPermission(AppPermission.Camera))
    {
      await SpeechManager.Instance.Speak($"<speak>That's better! <break time=\"1s\"/> I still can't see you though. Can you give me permission to see as well, please?</speak>");
      rigBundle.SetCameraActive(true);
      //TODO: Respond if rejected
    }
    await SpeechManager.Instance.Speak($"<speak>There we go! Incoming!</speak>");
    return gotMicrophonePermissions.Task.Result && PermissionsManager.Instance.CheckPermission(AppPermission.Camera);
  }
}

public class CameraRigBundle
{
  public GameObject CameraRig;
  public AudioListener PreCameraAudioListener;
  public GameObject PreCameraBackdrop;

  public void SetCameraActive(bool active)
  {
    CameraRig.SetActive(active);
    PreCameraAudioListener.enabled = !active;
    PreCameraBackdrop.SetActive(!active);
  }
}