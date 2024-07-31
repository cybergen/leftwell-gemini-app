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
    var permissionAccepted = false;
    var permissionsDismissed = false;
    var permissionsCallback = new PermissionCallbacks();
    permissionsCallback.PermissionGranted += (s) => { permissionAccepted = true; permissionsDismissed = true; };
    permissionsCallback.PermissionDenied += (s) => { permissionAccepted = false; permissionsDismissed = true; };
    permissionsCallback.PermissionDeniedAndDontAskAgain += (s) => { permissionAccepted = false; permissionsDismissed = true; };
    if (!Permission.HasUserAuthorizedPermission(permission))
    {
      Permission.RequestUserPermission(permission, permissionsCallback);
      while (!permissionsDismissed) { yield return null; }
    }
    callback?.Invoke(permissionAccepted);
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