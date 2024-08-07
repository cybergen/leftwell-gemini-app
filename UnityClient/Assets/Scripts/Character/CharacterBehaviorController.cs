using UnityEngine;
using BriLib;
using System.Collections.Generic;

public class CharacterBehaviorController : MonoBehaviour
{
  public bool BusyPathing { get; private set; } = false;

  [Header("Scene References")]
  [SerializeField] private CharacterAnimationController _animationController;
  [SerializeField] private Transform _cameraTransform;
  [SerializeField] private Transform _emptySparkTransform;
  [SerializeField] private ProceduralAnimator _proceduralAnimator;
  [SerializeField] private GameObject _whooshSource;
  [Header("---------------")]
  [Header("Basic Position and Motion Configuration")]
  [SerializeField] private float _distanceFromPlayerToSeek;
  [SerializeField] private float _angleFromPlayerForwardToSeek;
  [SerializeField] private float _distanceToTargetBeforeRotationStart;
  [SerializeField] private float _heightAdditionWhenAvoidCenter;
  [SerializeField] private float _standardModeYOffset = 0.1f;
  [Tooltip("Meters per second")][SerializeField] private float _movementSpeed;
  [SerializeField] private float _rotationMultiplier;
  [Header("Position Adjustment Thresholds")]
  [SerializeField] private float _angleThresholdToTurnTowardPlayer;
  [SerializeField] private float _distanceThresholdToMoveTowardPlayer;
  [SerializeField] private float _distanceThresholdToMoveAwayFromPlayer;
  [Header("Fly In Position Configuration")]
  [SerializeField] private float _flyInStartingHeight;
  [SerializeField] private float _flyInStartingDistance;
  [SerializeField] private float _flyInStartingAngleFromPlayerForward;
  [Header("Item Shown Position Configuration")]
  [Tooltip("How far away from which to look at object")][SerializeField] private float _shownObjectLookDistance;
  [SerializeField] private float _shownObjectAngleToSeekTo;
  [Header("Jump to Player Config")]
  [SerializeField] private float _pathingSpinDuration = 0.25f;
  [SerializeField] private float _pathingJumpDuration = 1f;
  [Header("Fly to Player Config")]
  [SerializeField] private float _uncomfortableDurationThreshold = 2.5f;
  [SerializeField] private float _flyBackToPlayerSpeed = 1.5f;
  [Header("Fly Away Config")]
  [SerializeField] private float _flyAwayAngle = -90f;
  [SerializeField] private float _flyAwayDistance = 30f;
  [Tooltip("Meters per second")][SerializeField] private float _flyAwaySpeed;
  [Header("Portal Position Config")]
  [SerializeField] private float _dieAnimDuration = 0.6f;
  [SerializeField] private float _distanceFromPortal;
  [SerializeField] private float _portalAngleToSeekTo = -15f;
  [Header("Item Orbit")]
  [SerializeField] private OrbitAnimator _orbiter;
  [SerializeField] private GameObject _orbitParticles;
  [SerializeField] private float _orbitSpeed;
  [SerializeField] private float _rotationSpeed;
  [Header("Talk Animation Retrigger")]
  [SerializeField] private float _minTalkRetriggerDuration = 0.5f;
  [SerializeField] private float _maxTalkRetriggerDuration = 3f;
  private float _nextRetriggerTime = 0f;
  private CharacterState _currentState = CharacterState.None;
  private CharacterState _priorState = CharacterState.None;
  private List<DragonAnimation> _talkingStates = new List<DragonAnimation>
  {
    DragonAnimation.No, DragonAnimation.Roar, DragonAnimation.Yes, DragonAnimation.Damage
  };
  private System.Random _rand = new System.Random();

  //Static position traversal tracking
  private Vector3 _startPosition = Vector3.zero;
  private Vector3 _targetPosition = Vector3.zero;
  private float _progress = 0f;
  private float _stateTimeElapsed = 0f;
  private float _uncomfortableTimeElapsed = 0f;
  private Quaternion _prePathRotation;
  private CharacterMode _mode;

  public void SetMode(CharacterMode mode)
  {
    _mode = mode;
  }

  public void SetState(CharacterState state)
  {
    Debug.Log($"Setting wizard state to {state} with prior state {_currentState}");

    switch (_currentState)
    {
      case CharacterState.ShownObject:
        _proceduralAnimator.Play();
        BusyPathing = false;
        break;
      case CharacterState.JumpingToPlayer:
        _proceduralAnimator.Play();
        BusyPathing = false;
        break;
      case CharacterState.InitialFlyIn:
        _orbitParticles.SetActive(false);
        BusyPathing = false;
        break;
      case CharacterState.FlyingToPlayer:
      case CharacterState.FlyingToPortal:
        BusyPathing = false;
        break;
      case CharacterState.Flabbergasted:
        _proceduralAnimator.Play();
        break;
      case CharacterState.MagicingItem:
        BusyPathing = false;
        _orbitParticles.SetActive(false);
        break;
    }

    _priorState = _currentState;
    _currentState = state;
    _stateTimeElapsed = 0f;
    _uncomfortableTimeElapsed = 0f;

    switch (_currentState)
    {
      case CharacterState.InitialFlyIn:
        _whooshSource.SetActive(true);
        _orbitParticles.SetActive(true);
        BusyPathing = true;

        var dir = Quaternion.AngleAxis(_flyInStartingAngleFromPlayerForward, Vector3.up) * GetFlat(_cameraTransform.forward);
        _startPosition = _cameraTransform.position + dir * _flyInStartingDistance;
        _startPosition.y = PlaneManager.Instance.GroundHeight + _flyInStartingHeight;

        transform.position = _startPosition;
        _targetPosition = GetPositionByPlayer();
        _progress = 0f;

        _animationController.SetAnimation(DragonAnimation.FlyDown);
        break;
      case CharacterState.JumpingToPlayer:
        _proceduralAnimator.Stop();
        BusyPathing = true;

        _startPosition = transform.position;
        _targetPosition = GetPositionByPlayer();
        _prePathRotation = transform.rotation;

        _progress = 0f;
        break;
      case CharacterState.ReturnThenResume:
        BusyPathing = true;

        _startPosition = transform.position;
        _targetPosition = GetPositionByPlayer();
        _animationController.PlayOnce(DragonAnimation.Run, DragonAnimation.Fly);
        _prePathRotation = transform.rotation;

        _progress = 0f;
        break;
      case CharacterState.FlyingToPlayer:
        BusyPathing = true;

        _startPosition = transform.position;
        _targetPosition = GetPositionByPlayer();
        _animationController.PlayOnce(DragonAnimation.Run, DragonAnimation.Fly);
        _prePathRotation = transform.rotation;

        _progress = 0f;
        break;
      case CharacterState.IdleWithPlayer:
        _animationController.SetAnimation(DragonAnimation.Fly);
        _proceduralAnimator.Play();
        break;
      case CharacterState.Talking:
        var anim = MathHelpers.SelectFromRange(_talkingStates, _rand);
        _animationController.PlayOnce(anim, DragonAnimation.Fly);
        _proceduralAnimator.Play();
        _nextRetriggerTime = MathHelpers.GetRandomFromRange(_minTalkRetriggerDuration, _maxTalkRetriggerDuration, _rand);
        break;
      case CharacterState.Flabbergasted:
        _proceduralAnimator.Stop();
        _animationController.SetAnimation(DragonAnimation.Die);
        _startPosition = transform.position;
        _targetPosition = _startPosition;
        _targetPosition.y = PlaneManager.Instance.GroundHeight;
        break;
      case CharacterState.ShownObject:
        _proceduralAnimator.Stop();
        BusyPathing = true;
        
        var pos = _cameraTransform.position + _cameraTransform.forward * PortalManager.Instance.MarkerSpawnDistance;
        _emptySparkTransform.position = pos;

        var objectToCameraDir = (_cameraTransform.position - pos).normalized;
        objectToCameraDir = Quaternion.AngleAxis(_shownObjectAngleToSeekTo, Vector3.up) * objectToCameraDir;

        _targetPosition = pos + objectToCameraDir * _shownObjectLookDistance;
        _startPosition = transform.position;
        _progress = 0f;

        _animationController.SetAnimation(DragonAnimation.Fly);
        break;
      case CharacterState.MagicingItem:
        BusyPathing = true;
        _ = _orbiter.Play(_emptySparkTransform.position, _orbitSpeed, _rotationSpeed);
        _orbitParticles.SetActive(true);
        _animationController.SetAnimation(DragonAnimation.FlyLeft);
        break;
      case CharacterState.PathToPlayerAndPresentPicture:
        _targetPosition = GetPositionByPlayer();
        _startPosition = transform.position;
        _progress = 0f;
        BusyPathing = true;
        break;
      case CharacterState.PresentingPicture:
        _animationController.PlayOnce(DragonAnimation.Roar, DragonAnimation.Jump);
        break;
      case CharacterState.FlyingToPortal:
        _progress = 0f;
        _startPosition = transform.position;
        var dirFromPortal = GetFlat(_cameraTransform.position - PortalManager.Instance.GetHeroPortalPosition());
        dirFromPortal = Quaternion.AngleAxis(_portalAngleToSeekTo, Vector3.up) * dirFromPortal;
        _targetPosition = PortalManager.Instance.GetHeroPortalPosition() + dirFromPortal * _distanceFromPortal;
        _animationController.PlayOnce(DragonAnimation.Yes, DragonAnimation.Fly);
        BusyPathing = true;
        break;
      case CharacterState.IdleByPortal:
        transform.rotation = Quaternion.LookRotation(GetFlat(_cameraTransform.position - transform.position));
        break;
      case CharacterState.FlyAway:
        _startPosition = transform.position;
        var cameraLeft = Quaternion.AngleAxis(_flyAwayAngle, Vector3.up) * _cameraTransform.forward;
        cameraLeft = GetFlat(cameraLeft);
        _targetPosition = _startPosition + cameraLeft * _flyAwayDistance;
        _progress = 0f;
        BusyPathing = true;
        break;
    }
  }

  private void Update()
  {
    //TODO: Apply hover amplitude in a way that doesn't impact movement calcs
    var delta = Time.deltaTime;
    _stateTimeElapsed += delta;

    switch (_currentState)
    {
      case CharacterState.InitialFlyIn:
        _progress = DoMoveToPointAndLookToTarget(delta, _progress, _cameraTransform, _movementSpeed);

        var distanceToPoint = Vector3.Distance(transform.position, _targetPosition);
        if (distanceToPoint > _distanceToTargetBeforeRotationStart)
        {
          _animationController.SetAnimation(DragonAnimation.Fly);
        }

        //If reach target point, mark no longer pathing and change to idle state
        if (_progress >= 1f)
        {
          BusyPathing = false;
          SetState(CharacterState.Talking);
        }
        break;
      case CharacterState.ShownObject:
        _progress = DoMoveToPointAndLookToTarget(delta, _progress, _emptySparkTransform, _movementSpeed);

        //If reach point, mark no longer pathing but don't change state
        if (_progress >= 1f)
        {
          BusyPathing = false;
        }
        break;
      case CharacterState.MagicingItem:
        if (!_orbiter.Animating) { SetState(CharacterState.JumpingToPlayer); }
        break;
      case CharacterState.JumpingToPlayer:
        var preJumpRot = GetFlattenedRotation(_targetPosition - _startPosition);

        if (_progress < _pathingSpinDuration) //Start off in a spin phase to aim toward target
        {
          var spinProgress = Easing.ExpoEaseOut(_progress / _pathingSpinDuration);
          transform.rotation = Quaternion.Slerp(_prePathRotation, preJumpRot, spinProgress);
        }
        else //Then trigger jump anim and start moving toward target
        {
          _animationController.SetAnimation(DragonAnimation.Jump);
          var jumpPath = _targetPosition - _startPosition;
          transform.position = _startPosition + jumpPath * (_progress - _pathingSpinDuration);

          var finalSpinProgress = Easing.ExpoEaseOut(Mathf.Clamp01(_progress - _pathingSpinDuration * 2f));
          var postJumpRot = GetFlattenedRotation(_cameraTransform.position - _targetPosition);
          transform.rotation = Quaternion.Slerp(preJumpRot, postJumpRot, finalSpinProgress);
        }

        _progress += delta;

        //Stop if enough time has elapsed that we should be at the target point
        if (_progress >= _pathingSpinDuration + _pathingJumpDuration)
        {
          BusyPathing = false;
          SetState(CharacterState.IdleWithPlayer);
        }
        break;
      case CharacterState.FlyingToPlayer:
        _progress = DoMoveToPointAndLookToTarget(delta, _progress, _cameraTransform, _flyBackToPlayerSpeed);
        if (_progress >= 1f) { SetState(CharacterState.IdleWithPlayer); }
        break;
      case CharacterState.ReturnThenResume:
        _progress = DoMoveToPointAndLookToTarget(delta, _progress, _cameraTransform, _flyBackToPlayerSpeed);
        if (_progress >= 1f) { SetState(_priorState); }
        break;
      case CharacterState.IdleWithPlayer:
        RunVisibilityCheck(delta);
        break;
      case CharacterState.Talking:
        RunVisibilityCheck(delta);
        _nextRetriggerTime -= delta;
        if (_nextRetriggerTime <= 0f )
        {
          _nextRetriggerTime = MathHelpers.GetRandomFromRange(_minTalkRetriggerDuration, _maxTalkRetriggerDuration, _rand);
          var anim = MathHelpers.SelectFromRange(_talkingStates, _rand);
          _animationController.PlayOnce(anim, DragonAnimation.Fly);
        }
        break;
      case CharacterState.FlyingToPortal:
        _progress = DoMoveToPointAndLookToTarget(delta, _progress, _cameraTransform, _movementSpeed);
        if (_progress >= 1f) { BusyPathing = false; }
        break;
      case CharacterState.FlyAway:
        var exitVector = (_targetPosition - transform.forward).normalized;
        if (Vector3.Dot(transform.forward, exitVector) < 0.8f)
        {
          var exitRotation = Quaternion.LookRotation(exitVector);
          transform.rotation = Quaternion.Slerp(transform.rotation, exitRotation, delta * _rotationMultiplier);
        }
        else if (_progress < 1f)
        {
          BusyPathing = false;
          _progress
            = DoMoveToPointAndLookToTarget(delta, _progress, _cameraTransform, _movementSpeed);
        }
        break;
      case CharacterState.Flabbergasted:
        var progress = Mathf.Clamp01(_stateTimeElapsed / _dieAnimDuration);
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, progress);
        break;
      case CharacterState.PathToPlayerAndPresentPicture:
        if (_progress < 1f)
        {
          _progress
            = DoMoveToPointAndLookToTarget(delta, _progress, _cameraTransform, _movementSpeed);
        }
        else
        {
          SetState(CharacterState.PresentingPicture);
        }
        break;
    }
  }

  private Vector3 GetPositionByPlayer()
  {
    if (_mode == CharacterMode.Standard)
    {
      var rotatedDir = Quaternion.AngleAxis(_angleFromPlayerForwardToSeek, Vector3.up) * _cameraTransform.forward;
      var newPosition = _cameraTransform.position + rotatedDir * _distanceFromPlayerToSeek;
      var up = _cameraTransform.up;
      newPosition += up * _standardModeYOffset;
      return newPosition;
    }
    else
    {
      var rotatedDir = Quaternion.AngleAxis(_angleFromPlayerForwardToSeek, Vector3.up) * _cameraTransform.forward;
      var newPosition = _cameraTransform.position + rotatedDir * _distanceFromPlayerToSeek;
      newPosition.y += _heightAdditionWhenAvoidCenter;
      return newPosition;
    }
  }

  private void RunVisibilityCheck(float delta)
  {
    var distanceToCamera = Vector3.Distance(transform.position, _cameraTransform.position);
    var viewAngleThreshold = Mathf.Cos(_angleThresholdToTurnTowardPlayer * Mathf.Deg2Rad);
    var camToCharacter = (transform.position - _cameraTransform.position).normalized;
    var currentDirDot = Vector3.Dot(_cameraTransform.forward, camToCharacter);

    //If player can't see dragon for too long, or if it is too close or too far, eventually path back
    if (currentDirDot < viewAngleThreshold || distanceToCamera > _distanceThresholdToMoveTowardPlayer
      || distanceToCamera < _distanceThresholdToMoveAwayFromPlayer)
    {
      _uncomfortableTimeElapsed += delta;
      if (_uncomfortableTimeElapsed >= _uncomfortableDurationThreshold)
      {
        SetState(CharacterState.ReturnThenResume);
      }
    }
    else
    {
      _uncomfortableTimeElapsed = 0f;
    }
  }

  private float DoMoveToPointAndLookToTarget(float delta, float progress, Transform positionToLookAt, float speed, System.Action doAtTurn = null)
  {
    //Path toward position if not there yet
    var flightDistance = Vector3.Distance(_startPosition, _targetPosition);
    if (progress < 1f)
    {
      progress += delta / (flightDistance / speed);
      Mathf.Clamp01(progress);
      var flightPath = _targetPosition - _startPosition;
      transform.position = (Easing.ExpoEaseOut(progress) * flightPath) + _startPosition;
    }

    var distanceToPoint = Vector3.Distance(transform.position, _targetPosition);
    if (distanceToPoint > _distanceToTargetBeforeRotationStart)
    {
      //Look at target point (not look point) during flight if not within distance threshold to rotate yet
      var flattenedRot = GetFlattenedRotation(_targetPosition - _startPosition);
      transform.rotation = Quaternion.Slerp(transform.rotation, flattenedRot, delta * _rotationMultiplier);
    }
    else
    {
      //Start rotating toward look point if within a certain threshold
      var rotProgress = Mathf.Clamp01(Easing.QuadEaseOut(1f - (distanceToPoint / _distanceToTargetBeforeRotationStart)));

      //Start rotation should be from initial point to target point
      var startRot = GetFlattenedRotation(_targetPosition - _startPosition);

      //Slerp to final rotation - looking at look point from target point
      var targetRot = GetFlattenedRotation(positionToLookAt.position - _targetPosition);
      transform.rotation = Quaternion.Slerp(startRot, targetRot, rotProgress);
    }

    return progress;
  }

  private Quaternion GetFlattenedRotation(Vector3 direction)
  {
    return Quaternion.LookRotation(GetFlat(direction));
  }

  private Vector3 GetFlat(Vector3 direction)
  {
    return Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
  }

  private void Awake()
  {
    _cameraTransform = FindObjectOfType<Camera>().transform;
    if (_cameraTransform == null)
    {
      Debug.LogError($"Failed to find camera during character initialization");
    }
  }
}

public enum CharacterState
{
  None,
  InitialFlyIn,
  JumpingToPlayer,
  FlyingToPlayer,
  ReturnThenResume,
  IdleWithPlayer,
  Talking,
  ShownObject,
  PathToPlayerAndPresentPicture,
  PresentingPicture,
  Flabbergasted,
  FlyingToPortal,
  IdleByPortal,
  FlyAway,
  MagicingItem,
}

public enum CharacterMode
{
  Standard,
  AvoidCenter,
}