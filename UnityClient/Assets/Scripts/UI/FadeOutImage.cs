using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using BriLib;

public class FadeOutImage : MonoBehaviour
{  
  [SerializeField] private Image _self;
  [SerializeField] private AudioSource _source;
  [SerializeField] private int _animationMillis;
  [SerializeField] private int _milliStep;
  private Color _color;

  public async void TriggerAnimate()
  {
    _color.a = 1f;
    _self.color = _color;
    _source.Play();
    await Animate();
  }

  private async Task Animate()
  {
    var elapsedMillis = 0f;

    while (elapsedMillis < _animationMillis)
    {
      var progress = Easing.ExpoEaseOut(elapsedMillis / _animationMillis);

      var alpha = Mathf.Lerp(1f, 0f, progress);
      _color.a = alpha;
      _self.color = _color;

      await Task.Delay(_milliStep);
      elapsedMillis += _milliStep;
    }
  }

  private void Awake()
  {
    _color = _self.color;
    _color.a = 0f;
    _self.color = _color;
  }
}