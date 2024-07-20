using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using BriLib;

public class PermissionsManager : Singleton<PermissionsManager>
{
  public bool CheckPermission(AppPermission permission)
  {
#if UNITY_ANDROID
    var perm = permission == AppPermission.Microphone ? Permission.Microphone : Permission.Camera;
    return Permission.HasUserAuthorizedPermission(perm);
#elif UNITY_IOS
    var perm = permission == AppPermission.Microphone ? UserAuthorization.Microphone : UserAuthorization.WebCam;
    return Application.HasUserAuthorization(perm);
#else
    return true;
#endif
  }

  public void RequestPermission(AppPermission permission, Action<bool> callback)
  {
#if UNITY_ANDROID
    var perm = permission == AppPermission.Microphone ? Permission.Microphone : Permission.Camera;
    StartCoroutine(RequestAndroidPermission(Permission.Microphone, callback));
#elif UNITY_IOS
    var perm = permission == AppPermission.Microphone ? UserAuthorization.Microphone : UserAuthorization.WebCam;
    StartCoroutine(RequestiOSPermission(perm, callback));
#else
    callback?.Invoke(true);
#endif
  }

#if UNITY_ANDROID
  private IEnumerator RequestAndroidPermission(string permission, Action<bool> callback)
  {
    if (!Permission.HasUserAuthorizedPermission(permission))
    {
      Permission.RequestUserPermission(permission);
      yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(permission));
    }
    callback?.Invoke(Permission.HasUserAuthorizedPermission(permission));
  }
#endif

#if UNITY_IOS
  private IEnumerator RequestiOSPermission(UserAuthorization permission, Action<bool> callback)
  {
    // For iOS, request microphone permission
    yield return Application.RequestUserAuthorization(permission);
    callback?.Invoke(Application.HasUserAuthorization(permission));
  }
#endif
}

public enum AppPermission
{
  Microphone,
  Camera
}