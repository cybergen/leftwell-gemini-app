using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using BriLib;

public class VortexEffectController : MonoBehaviour
{
  [SerializeField] private Material _vortexMaterial;
  [Header("Phase Effect Config")]
  [SerializeField] private float _shownOpacity;
  [SerializeField] private float _opacityAmplitude;
  [SerializeField] private float _phaseTime;
  [Header("Show/Hide Config")]
  [SerializeField] private int _showAnimationMillis;
  [SerializeField] private int _milliStep = 16;
  private float _actualOpacity;
  private CancellationTokenSource _cancellationTokenSource;

  public async void Show()
  {
    gameObject.SetActive(true);

    //Cancel the previous animation if any
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();

    await Animate(true, _cancellationTokenSource.Token);
  }

  public async void Hide()
  {
    //Cancel the previous animation if any
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource = new CancellationTokenSource();

    await Animate(false, _cancellationTokenSource.Token);
    gameObject.SetActive(false);
  }

  private async Task Animate(bool shown, CancellationToken cancellationToken)
  {
    var startOpacity = _actualOpacity;
    var endOpacity = shown ? _shownOpacity : 0f;
    var elapsedMillis = 0;
    while (elapsedMillis < _showAnimationMillis)
    {
      if (cancellationToken.IsCancellationRequested)
      {
        return; //Exit if animation is cancelled
      }

      var progress = Easing.ExpoEaseOut((float)elapsedMillis / _showAnimationMillis);
      _actualOpacity = (endOpacity - startOpacity) * progress + startOpacity;
      elapsedMillis += _milliStep;
      await Task.Delay(_milliStep, cancellationToken);
    }
    _actualOpacity = endOpacity;
  }

  private void Update()
  {
    var sineTime = Mathf.Sin(((Time.time % _phaseTime) / _phaseTime * 360f) * Mathf.Deg2Rad);
    var alpha = sineTime * _opacityAmplitude * _actualOpacity; //Multiply by opacity to allow to fully fade
    _vortexMaterial.SetFloat("_Opacity", _actualOpacity + alpha);
  }

  private void Awake()
  {
    _vortexMaterial.SetFloat("_Opacity", 0f);
    _actualOpacity = 0f;
  }

  private void OnDestroy()
  {
    _cancellationTokenSource?.Cancel();
  }
}
