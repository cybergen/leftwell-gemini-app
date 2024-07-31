using System;
using TMPro;
using UnityEngine;

public class YesNoScreen : MonoBehaviour
{
  [SerializeField] private Fadable _fadable;
  [SerializeField] private VerticalSlidingElement _buttons;
  [SerializeField] private int _animationMillis;
  [SerializeField] private TMP_Text _text;
  private Action _onYes;
  private Action _onNo;

  public void Show(string text, Action onYes, Action onNo)
  {
    _text.text = text;
    _onYes = onYes;
    _onNo = onNo;
    _fadable.Show(_animationMillis / 1000f);
    _buttons.Show(_animationMillis / 1000f);
  }
  
  public void Hide()
  {
    _onYes = null;
    _onNo = null;
    _fadable.Hide(_animationMillis / 1000f);
    _buttons.Hide(_animationMillis / 1000f);
  }

  public void OnYes()
  {
    _onYes?.Invoke();
    Hide();
  }

  public void OnNo()
  {
    _onNo?.Invoke();
    Hide();
  }
}