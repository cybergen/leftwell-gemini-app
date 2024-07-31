using System.Threading.Tasks;
using UnityEngine;

public class RequestPermissionsSequence : ISequence<CameraRigBundle, bool>
{
  public async Task<bool> RunAsync(CameraRigBundle rigBundle)
  {

    if (!PermissionsManager.Instance.CheckPermission(AppPermission.Camera))
    {
      await SpeechManager.Instance.Speak(FTEDialog.REQUEST_CAMERA);
      rigBundle.SetCameraActive(true);

      if (!PermissionsManager.Instance.CheckPermission(AppPermission.Camera))
      {
        await SpeechManager.Instance.Speak(FTEDialog.REJECTED_PERMISSION);
        return false;
      }

      await SpeechManager.Instance.Speak(FTEDialog.GOT_CAMERA);
    }

    var gotMicrophonePermissions = new TaskCompletionSource<bool>();
    if (!PermissionsManager.Instance.CheckPermission(AppPermission.Microphone))
    {
      await SpeechManager.Instance.Speak(FTEDialog.REQUEST_MIC);
      PermissionsManager.Instance.RequestPermission(
        AppPermission.Microphone, (result) => gotMicrophonePermissions.SetResult(result));
      await gotMicrophonePermissions.Task;
      if (!gotMicrophonePermissions.Task.Result)
      {
        await SpeechManager.Instance.Speak(FTEDialog.REJECTED_PERMISSION);
        return false;
      }
    }

    await SpeechManager.Instance.Speak(FTEDialog.GOT_MIC);
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