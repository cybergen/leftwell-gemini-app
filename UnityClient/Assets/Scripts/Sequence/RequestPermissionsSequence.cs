using System.Threading.Tasks;
using UnityEngine;

public class RequestPermissionsSequence : ISequence<CameraRigBundle, bool>
{
  public async Task<bool> RunAsync(CameraRigBundle rigBundle)
  {
    if (!PermissionsManager.Instance.CheckPermission(AppPermission.Camera))
    {
      await SpeechManager.Instance.Speak(FTEDialog.REQUEST_CAMERA);
#if UNITY_ANDROID
      rigBundle.SetCameraActive(true);

      await Task.Delay(1000);
      if (!PermissionsManager.Instance.CheckPermission(AppPermission.Camera))
      {
        rigBundle.SetCameraActive(false);
        await SpeechManager.Instance.Speak(FTEDialog.REJECTED_PERMISSION);
        return false;
      }
#elif UNITY_IOS
      bool gotResponse = false;
      bool accepted = false;
      PermissionsManager.Instance.RequestPermission(AppPermission.Camera, (response) =>
      {
        accepted = response;
        gotResponse = true;
      });
      while (!gotResponse) { await Task.Delay(10); }
    
      if (accepted)
      {
        rigBundle.SetCameraActive(true);
        await Task.Delay(2000); //Wait a bit for low end devices - my iPhoneXR goes crazy
      }
      else
      {
        await SpeechManager.Instance.Speak(FTEDialog.REJECTED_PERMISSION);
        return false;
      }
#endif

      await SpeechManager.Instance.Speak(FTEDialog.GOT_CAMERA);
    }

    var gotMicrophonePermissions = new TaskCompletionSource<bool>();
    if (!PermissionsManager.Instance.CheckPermission(AppPermission.Microphone))
    {
      Debug.Log("In request mic permissions block");
      await SpeechManager.Instance.Speak(FTEDialog.REQUEST_MIC);
      PermissionsManager.Instance.RequestPermission(
        AppPermission.Microphone, (result) => gotMicrophonePermissions.SetResult(result));
      await gotMicrophonePermissions.Task;
      await Task.Delay(1000);
      Debug.Log($"Got result from microphone permission task {gotMicrophonePermissions.Task.Result}");
      if (!PermissionsManager.Instance.CheckPermission(AppPermission.Microphone))
      {
        Debug.Log("Saw that user rejected, speaking and exiting");
        await SpeechManager.Instance.Speak(FTEDialog.REJECTED_PERMISSION);
        return false;
      }
    }
    Debug.Log("Got out of request mic perms block with success");
    await SpeechManager.Instance.Speak(FTEDialog.GOT_MIC);
    return true;
  }
}

public class CameraRigBundle
{
  public GameObject CameraRig;
  public AudioListener PreCameraAudioListener;
  public GameObject PreCameraBackdrop;

  public async void SetCameraActive(bool active)
  {
    CameraRig.SetActive(active);
    await Task.Delay(1000); //Wait a moment before dismissing camera backdrop
    PreCameraAudioListener.enabled = !active;
    PreCameraBackdrop.SetActive(!active);
  }
}