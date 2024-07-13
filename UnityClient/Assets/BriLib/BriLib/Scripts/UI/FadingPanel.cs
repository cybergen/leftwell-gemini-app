using System;

namespace BriLib.UI
{
  /// <summary>
  /// Extendible generic fading panel
  /// </summary>
  /// <typeparam name="T"></typeparam>
  public class FadingPanel<T> : Panel<T>
  {
    public override void Show(Action onShowAnimationFinish = null)
    {
      _showAnimation = UIAnimationMethods.ShowFadeInAnimation;
      _hideAnimation = UIAnimationMethods.HideFadeOutAnimation;
      base.Show(onShowAnimationFinish);
    }
  }

  /// <summary>
  /// Extendible fading panel that expects no data supplied to it
  /// </summary>
  public class FadingPanel : Panel
  {
    public override void Show(Action onShowAnimationFinish = null)
    {
      _showAnimation = UIAnimationMethods.ShowFadeInAnimation;
      _hideAnimation = UIAnimationMethods.HideFadeOutAnimation;
      base.Show(onShowAnimationFinish);
    }
  }
}