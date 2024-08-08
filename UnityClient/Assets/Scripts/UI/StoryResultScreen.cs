using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BriLib;

public class StoryResultScreen : MonoBehaviour
{
  [SerializeField] private RawImage _image;
  [SerializeField] private TMP_Text _text;
  [SerializeField] private Fadable _fadable;
  [SerializeField] private VerticalSlidingElement _shareButton;
  [SerializeField] private int _animationMillis;
  [SerializeField] private RectTransform _zoomableContentPane;
  [Tooltip("Units of scale per second")][SerializeField] private float _paneUnzoomDuration = 2f;
  private Vector3 _contentPaneTargetScale;
  private Vector3 _contentPaneStartScale;
  private float _elapsedUnzoomTime;
  private bool _animatingPanelScale;
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
    _fadable.Show(_animationMillis / 1000f);
    while (_fadable.Animating) { await Task.Delay(10); }
    _shareButton.Show(_animationMillis / 1000f);
  }

  public async void Hide()
  {
    _fadable.Hide(_animationMillis / 1000f);
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
    share.SetText(_text.text);
    share.SetCallback(OnShareResult);
    share.Share();
  }

  public void OnStartPinchZoom()
  {
    _animatingPanelScale = false;
  }

  public void OnReleasePinchZoom()
  {
    _animatingPanelScale = true;
    _contentPaneStartScale = _zoomableContentPane.localScale;
    _elapsedUnzoomTime = 0f;
  }

  private void OnShareResult(NativeShare.ShareResult result, string shareTarget)
  {
    Debug.Log($"Got share result {result}");
    _onShare?.Invoke(result == NativeShare.ShareResult.Shared);
  }

  private void Update()
  {
    if (_animatingPanelScale)
    {
      _elapsedUnzoomTime += Time.deltaTime;
      var progress = Mathf.Clamp01(Easing.ExpoEaseOut(_elapsedUnzoomTime / _paneUnzoomDuration));
      _zoomableContentPane.localScale = Vector3.Lerp(_contentPaneStartScale, _contentPaneTargetScale, progress);
      if (progress >= 1f) { _animatingPanelScale = false; }
    }
  }

  private void Awake()
  {
    _contentPaneTargetScale = _zoomableContentPane.localScale;
  }
}