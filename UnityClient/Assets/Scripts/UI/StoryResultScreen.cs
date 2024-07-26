using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoryResultScreen : MonoBehaviour
{
  [SerializeField] private RawImage _image;
  [SerializeField] private TMP_Text _text;
  [SerializeField] private Fadable _fadable;
  [SerializeField] private VerticalSlidingElement _shareButton;
  [SerializeField] private int _animationMillis;
  private Texture2D _texture;
  private Action _onHide;
  private Action<bool> _onShare;

  public async void Show(Texture2D image, string storyText, Action onHide, Action<bool> onShare)
  {
    _texture = image;
    _image.texture = image;
    _text.text = storyText;
    _onHide = onHide;
    _onShare = onShare;
    _fadable.Show(_animationMillis);
    while (_fadable.Animating) { await Task.Delay(10); }
    _shareButton.Show(_animationMillis);
  }

  public async void Hide()
  {
    _fadable.Hide();
    while (_fadable.Animating) { await Task.Delay(10); }
    _onHide?.Invoke();
  }

  public void OnBack()
  {
    Hide();
  }

  public void OnShare()
  {
    var share = new NativeShare();
    share.AddFile(_texture);
    share.SetTitle("Teleportation Turmoil story results!");
    share.SetCallback(OnShareResult);
    share.Share();
  }

  private void OnShareResult(NativeShare.ShareResult result, string shareTarget)
  {
    Debug.Log($"Got share result {result}");
    _onShare?.Invoke(result == NativeShare.ShareResult.Shared);
  }
}