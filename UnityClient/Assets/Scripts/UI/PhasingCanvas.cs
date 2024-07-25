using UnityEngine;

public class PhasingCanvas : MonoBehaviour
{
  [SerializeField] private float _targetAlpha;
  [SerializeField] private float _alphaAmplitude;
  [SerializeField] private float _phaseTime;
  [SerializeField] private CanvasGroup _canvasGroup;
  private float _startingAlpha;

  private void Update()
  {
    var sineTime = Mathf.Sin(((Time.time % _phaseTime) / _phaseTime * 360f) * Mathf.Deg2Rad);
    var alpha = sineTime * _alphaAmplitude;
    _canvasGroup.alpha = _startingAlpha + alpha;
  }

  private void Awake()
  {
    _canvasGroup.interactable = false;
    _canvasGroup.blocksRaycasts = false;
    _canvasGroup.alpha = 0f;
    _startingAlpha = 0f;
  }
}