using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class TitleScreen : MonoBehaviour
{
  [SerializeField] private Fadable _leftwell;
  [SerializeField] private Fadable _title;
  [SerializeField] private Fadable _star;
  [SerializeField] private Image _leftFill;
  [SerializeField] private Image _rightFill;
  [SerializeField] private VerticalSlidingElement _start;
  [SerializeField] private int _fadeMillis;
  [SerializeField] private int _starFadeMillis;
  [SerializeField] private int _fillMillis;
  [SerializeField] private int _slideMillis;
  [SerializeField] private int _pauseMillis;
  [SerializeField] private int _milliStep = 16;
  private Action _onStartPressed;

  public void Show(Action onStartPressed)
  {
    _onStartPressed = onStartPressed;
    SoundFXManager.Instance.PlaySound(Sound.IntroFlourish);
    Animate();
  }

  public void Hide()
  {
    gameObject.SetActive(false);
  }

  private async void Animate()
  {
    _leftwell.Show(_fadeMillis / 1000f);
    while (_leftwell.Animating) { await Task.Delay(10); }
    await Task.Delay(_pauseMillis);

    _title.Show(_fadeMillis / 1000f);
    while (_title.Animating) { await Task.Delay(10); }
    await Task.Delay(_pauseMillis);

    var elapsedMillis = 0f;
    while (elapsedMillis < _fillMillis / 2f) 
    {
      _leftFill.fillAmount = _rightFill.fillAmount = elapsedMillis / _fillMillis;
      elapsedMillis += _milliStep;
      await Task.Delay(_milliStep);
    }
    _leftFill.fillAmount = _rightFill.fillAmount = 1f;
    SoundFXManager.Instance.PlaySound(Sound.Chime);
    _star.Show(_starFadeMillis / 1000f);
    await Task.Delay(_pauseMillis);

    _start.Show(_slideMillis / 1000f);
    while (_start.Animating) { await Task.Delay(10); }
  }

  public void OnStartPressed()
  {
    Hide(); 
    SoundFXManager.Instance.PlaySound(Sound.Select);
    _onStartPressed?.Invoke();
  }

  private void Awake()
  {
    _leftFill.fillAmount = _rightFill.fillAmount = 0f;
  }
}