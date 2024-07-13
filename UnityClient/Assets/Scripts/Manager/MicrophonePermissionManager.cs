using UnityEngine;
using System.Collections;
using UnityEngine.Android;
using BriLib;

public class MicrophonePermissionManager : Singleton<MicrophonePermissionManager>
{
  public override void Begin()
  {
    base.Begin();
    RequestMicrophonePermission((response) => {
      Debug.Log($"User accepted microphone permission: {response}");
    });
  }

  public void RequestMicrophonePermission(System.Action<bool> callback)
  {
    StartCoroutine(RequestMicrophonePermissionCoroutine(callback));
  }

  private IEnumerator RequestMicrophonePermissionCoroutine(System.Action<bool> callback)
  {
#if UNITY_ANDROID
    // For Android, request microphone permission
    if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
    {
      Permission.RequestUserPermission(Permission.Microphone);
      yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.Microphone));
    }
    callback?.Invoke(Permission.HasUserAuthorizedPermission(Permission.Microphone));
#elif UNITY_IOS
    // For iOS, request microphone permission
    yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
    callback?.Invoke(Application.HasUserAuthorization(UserAuthorization.Microphone));
#else
    // For other platforms, assume permission is granted
    callback?.Invoke(true);
#endif
  }
}
