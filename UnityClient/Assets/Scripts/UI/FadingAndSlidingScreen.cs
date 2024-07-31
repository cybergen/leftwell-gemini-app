using System;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class FadingAndSlidingScreen : MonoBehaviour
{
  [SerializeField] private int _animationMillis;
  [SerializeField] private Fadable _fadable;
  [SerializeField] private TMP_Text _text;
  [SerializeField] private VerticalSlidingElement _slidingButton;
  private bool _shown;
  private Action _onPress;

  public void OnPress()
  {
    _onPress?.Invoke();
  }

  public void Show(string text, Action onPress)
  {
    _onPress = onPress;
    _text.text = text;

    if (_shown) return;

    gameObject.SetActive(true);
    _shown = true;

    _slidingButton.Show(_animationMillis / 1000f);
    _fadable.Show(_animationMillis / 1000f);
  }

  public async void Hide()
  {
    if (!_shown) return;

    _shown = false;

    _slidingButton.Hide(_animationMillis / 1000f);
    _fadable.Hide(_animationMillis / 1000f);
    while (_fadable.Animating) { await Task.Delay(10); }
    gameObject.SetActive(false);
  }

  private void Awake()
  {
    _shown = false;
    gameObject.SetActive(false);
  }
}