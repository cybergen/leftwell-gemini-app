using System;
using System.Threading.Tasks;

public class RequestPermissionsSequence : ISequence<bool>
{
  public async Task<bool> RunAsync()
  {
    //TODO: Make this implementation more in-worldy and lore-driven, with gcloud voice synth
    var gotMicrophonePermissions = new TaskCompletionSource<bool>();
    var gotCameraPermissions = new TaskCompletionSource<bool>();
    PermissionsManager.Instance.RequestPermission(
      AppPermission.Microphone, (result) => gotMicrophonePermissions.SetResult(result));
    PermissionsManager.Instance.RequestPermission(
      AppPermission.Camera, (result) => gotCameraPermissions.SetResult(result));
    await Task.WhenAll(gotMicrophonePermissions.Task, gotCameraPermissions.Task);
    return gotMicrophonePermissions.Task.Result && gotCameraPermissions.Task.Result;
  }
}