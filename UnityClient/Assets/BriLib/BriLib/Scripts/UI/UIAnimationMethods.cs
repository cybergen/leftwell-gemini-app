using System.Threading.Tasks;
using UnityEngine;

namespace BriLib.UI
{
  /// <summary>
  /// Some task-driven methods for animating UI with minimal additional boilerplate
  /// </summary>
  public static class UIAnimationMethods
  {
    private const int FADE_DURATION_MILLIS = 400;
    private const int ANIM_STEP_MILLIS = 16;

    public static async Task ShowFadeInAnimation(GameObject obj)
    {
      var rect = obj.GetComponent<RectTransform>();
      if (rect != null)
      {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = obj.AddComponent<CanvasGroup>();
        var startAlpha = canvasGroup.alpha;

        var targetMilliseconds = FADE_DURATION_MILLIS;
        var stepMillis = ANIM_STEP_MILLIS;
        var accumulatedMillis = FADE_DURATION_MILLIS * startAlpha / 1f;
        while (accumulatedMillis < targetMilliseconds)
        {
          canvasGroup.alpha = Easing.ExpoEaseOut(((float)accumulatedMillis / targetMilliseconds));
          await Task.Delay(stepMillis);
          accumulatedMillis += stepMillis;
        }
      }
    }

    public static async Task HideFadeOutAnimation(GameObject obj)
    {
      var rect = obj.GetComponent<RectTransform>();
      if (rect != null)
      {
        var canvasGroup = obj.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = obj.AddComponent<CanvasGroup>();
        var startAlpha = canvasGroup.alpha;

        var targetMilliseconds = FADE_DURATION_MILLIS;
        var stepMillis = ANIM_STEP_MILLIS;
        var accumulatedMillis = targetMilliseconds - ((1f - startAlpha) * FADE_DURATION_MILLIS);
        while (accumulatedMillis > 0)
        {
          canvasGroup.alpha = Easing.ExpoEaseIn(((float)accumulatedMillis / targetMilliseconds));
          await Task.Delay(stepMillis);
          accumulatedMillis -= stepMillis;
        }
      }
    }

    public static async Task Fade(this GameObject obj, float duration, float targetAlpha, Easing.Method method)
    {
      var rect = obj.GetComponent<RectTransform>();
      if (rect == null)
      {
        LogManager.Warn("Attempted to fade non-UI object");
        return;
      }

      var canvasGroup = obj.GetComponent<CanvasGroup>();
      if (canvasGroup == null) canvasGroup = obj.AddComponent<CanvasGroup>();
      var startAlpha = canvasGroup.alpha;

      var accumulatedMillis = 0;
      while (accumulatedMillis < (duration * 1000))
      {
        canvasGroup.alpha = Easing.Ease(startAlpha, targetAlpha, 0, duration, accumulatedMillis / 1000f, method);
        await Task.Delay(ANIM_STEP_MILLIS);
        accumulatedMillis += ANIM_STEP_MILLIS;
      }
    }
  }
}