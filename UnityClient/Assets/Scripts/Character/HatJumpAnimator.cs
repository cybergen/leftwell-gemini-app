using System.Threading.Tasks;
using UnityEngine;
using BriLib;

public class HatJumpAnimator : MonoBehaviour
{
  public bool Playing { get; private set; } = false;
  [SerializeField] private float _popHeight;
  private Transform _self;
  private Vector3 _startLocalPosition;
  private int _milliStep = 16;

  public async void Play(int delayMillis = 0, int durationMillis = 500)
  {
    if (Playing) { return; }

    Playing = true;
    await Task.Delay(delayMillis);

    var halfMillis = durationMillis / 2f;
    var elapsedMillis = 0f;

    //First half of animation - expo ease out upward
    while (elapsedMillis < halfMillis)
    {
      elapsedMillis += _milliStep;
      var progress = Mathf.Clamp01(Easing.ExpoEaseOut(elapsedMillis / halfMillis));
      var currentHeight = progress * _popHeight + _startLocalPosition.y;
      _self.localPosition = new Vector3(_startLocalPosition.x, currentHeight, _startLocalPosition.z);
      await Task.Delay(_milliStep);
    }

    //Reset elapsed millis to simplify calcs
    elapsedMillis = 0;

    //Second half of animation - expo ease in downward
    while (elapsedMillis < halfMillis)
    {
      elapsedMillis += _milliStep;
      var progress = Mathf.Clamp01(1f - Easing.ExpoEaseIn(elapsedMillis / halfMillis));
      var currentHeight = _popHeight * progress + _startLocalPosition.y;
      _self.localPosition = new Vector3(_startLocalPosition.x, currentHeight, _startLocalPosition.z);
      await Task.Delay(_milliStep);
    }

    _self.localPosition = _startLocalPosition;
    Playing = false;
  }

  private void Awake()
  {
    _self = transform;
    _startLocalPosition = _self.localPosition;
  }
}