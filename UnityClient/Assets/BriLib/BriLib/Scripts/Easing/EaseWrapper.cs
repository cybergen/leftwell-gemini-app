using System;

namespace BriLib
{
  public class EaseWrapper
  {
    public enum Direction
    {
      Forward,
      Backward,
    }

    private float _currentTime = 0f;
    private Direction _currentDirection = Direction.Forward;

    private float _duration;
    private float _startValue;
    private float _endValue;
    private Easing.Method _easeType;
    private Action<float> _onUpdate;

    private Action _onFinish;
    private Action _onCancel;
    private bool _easing;

    public static EaseWrapper CreateWrapperAndStart(float duration, float start, float end, Action<float> onUpdate,
      Easing.Method easingMethod = Easing.Method.ExpoOut, Direction direction = Direction.Forward,
      Action onFinish = null, Action onCancel = null)
    {
      var wrapper = new EaseWrapper(duration, start, end, easingMethod, onUpdate);
      wrapper.Ease(direction, onFinish, onCancel);
      return wrapper;
    }

    public EaseWrapper(
      float duration,
      float startValue,
      float endValue,
      Easing.Method easeType,
      Action<float> onUpdate)
    {
      _duration = duration;
      _startValue = startValue;
      _endValue = endValue;
      _easeType = easeType;
      _onUpdate = onUpdate;
    }

    public void Ease(Direction direction, Action onFinish, Action onCancel)
    {
      if (_easing) _onCancel.Execute();

      _currentDirection = direction;
      _easing = true;
      _onFinish = () => { UpdateManager.Instance.RemoveUpdater(Tick); onFinish.Execute(); };
      _onCancel = () => { UpdateManager.Instance.RemoveUpdater(Tick); onCancel.Execute(); };
      UpdateManager.Instance.AddUpdater(Tick);
    }

    public void Tick(float delta)
    {
      if (!_easing) return;

      if (_currentDirection == Direction.Forward)
      {
        _currentTime += delta;
        if (_currentTime >= _duration)
        {
          _currentTime = _duration;
          _onUpdate.Execute(Easing.Ease(_startValue, _endValue, 0f, _duration, _currentTime, _easeType));
          _easing = false;
          _onFinish.Execute();
        }
        else
        {
          _onUpdate.Execute(Easing.Ease(_startValue, _endValue, 0f, _duration, _currentTime, _easeType));
        }
      }
      else
      {
        _currentTime -= delta;
        if (_currentTime <= 0)
        {
          _currentTime = 0f;
          _onUpdate.Execute(Easing.Ease(_startValue, _endValue, 0f, _duration, _currentTime, _easeType));
          _easing = false;
          _onFinish.Execute();
        }
        else
        {
          _onUpdate.Execute(Easing.Ease(_startValue, _endValue, 0f, _duration, _currentTime, _easeType));
        }
      }

    }

    public void RevertToStart()
    {
      _currentTime = 0f;
      _onUpdate.Execute(0f);
    }
  }
}