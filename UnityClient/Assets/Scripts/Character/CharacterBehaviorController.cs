using UnityEngine;
using BriLib;

public class CharacterBehaviorController : MonoBehaviour
{
  public bool BusyPathing { get; private set; } = false;
  [Header("Scene References")]
  [SerializeField] private CharacterAnimationController _animationController;
  [SerializeField] private Transform _cameraTransform;
  [Header("Active Position and Motion Configuration")]
  [SerializeField] private float _heightFromGroundToSeek;
  [SerializeField] private float _distanceFromPlayerToSeek;
  [SerializeField] private float _angleFromPlayerForwardToSeek;
  [SerializeField] private float _distanceToTargetBeforeRotationStart;
  [Tooltip("Meters per second")][SerializeField] private float _movementSpeed;
  [SerializeField] private float _rotationMultiplier;
  [Header("Adjustment Thresholds")]
  [SerializeField] private float _angleThresholdToTurnTowardPlayer;
  [SerializeField] private float _distanceThresholdToMoveTowardPlayer;
  [SerializeField] private float _distanceThresholdToMoveAwayFromPlayer;
  [Header("Fly In Position Configuration")]
  [SerializeField] private float _flyInStartingHeight;
  [SerializeField] private float _flyInStartingDistance;
  [SerializeField] private float _flyInStartingAngleFromPlayerForward;
  [Header("Item Shown Position Configuration")]
  [Tooltip("Distance to point from camera")][SerializeField] private float _shownObjectDistanceForward;
  [Tooltip("How far away from whcih to look at object")][SerializeField] private float _shownObjectLookDistance;
  [SerializeField] private float _shownObjectAngleToSeekTo;
  [Header("Idle Procedural Animation")]
  [SerializeField] private float _upDownHoverAmplitude;
  [SerializeField] private float _upDownHoverSpeed;
  [Header("Jump to Player Config")]
  [SerializeField] private float _pathingSpinDuration = 0.25f;
  [SerializeField] private float _pathingJumpDuration = 1f;
  [Header("Fly to Player Config")]
  [SerializeField] private float _uncomfortableDurationThreshold = 2.5f;
  [SerializeField] private float _flyBackToPlayerSpeed = 1.5f;
  [Header("Assorted Config")]
  [SerializeField] private float _dieAnimDuration = 0.6f;

  private CharacterStates _currentState = CharacterStates.None;

  //Static position traversal tracking
  private Vector3 _startPosition = Vector3.zero;
  private Vector3 _targetPosition = Vector3.zero;
  private float _traverseProgress = 0f;
  private float _stateTimeElapsed = 0f;
  private float _uncomfortableTimeElapsed = 0f;
  private Vector3 _shownObjectLookPosition = Vector3.zero;
  private Quaternion _prePathRotation;

  public void SetState(CharacterStates state)
  {
    Debug.Log($"Setting wizard state to {state}");

    switch (_currentState)
    {
      case CharacterStates.InitialFlyIn:
      case CharacterStates.ShownObject:
      case CharacterStates.JumpingToPlayer:
      case CharacterStates.FlyingToPlayer:
      case CharacterStates.FlyingToPortal:
        BusyPathing = false;
        break;
    }

    _currentState = state;
    _stateTimeElapsed = 0f;
    _uncomfortableTimeElapsed = 0f;

    switch (_currentState)
    {
      case CharacterStates.InitialFlyIn:
        BusyPathing = true;

        var dir = Quaternion.AngleAxis(_flyInStartingAngleFromPlayerForward, Vector3.up) * GetFlat(_cameraTransform.forward);
        _startPosition = _cameraTransform.position + dir * _flyInStartingDistance;
        _startPosition.y = PlaneManager.Instance.GroundHeight + _flyInStartingHeight;

        transform.position = _startPosition;
        _targetPosition = GetStandardPositionByPlayer();
        _traverseProgress = 0f;

        _animationController.SetAnimation(DragonAnimation.FlyDown);
        break;
      case CharacterStates.JumpingToPlayer:
        BusyPathing = true;

        _startPosition = transform.position;
        _targetPosition = GetStandardPositionByPlayer();
        _animationController.SetAnimation(DragonAnimation.FlyLeft);
        _prePathRotation = transform.rotation;

        _traverseProgress = 0f;
        break;
      case CharacterStates.FlyingToPlayer:
        BusyPathing = true;

        _startPosition = transform.position;
        _targetPosition = GetStandardPositionByPlayer();
        _animationController.PlayOnce(DragonAnimation.Run, DragonAnimation.Fly);
        _prePathRotation = transform.rotation;

        _traverseProgress = 0f;
        break;
      case CharacterStates.IdleWithPlayer:
        _animationController.SetAnimation(DragonAnimation.Fly);
        break;
      case CharacterStates.TalkingSurprised:
        _animationController.PlayOnce(DragonAnimation.Yes, DragonAnimation.Fly);
        break;
      case CharacterStates.TalkingDisappointed:
        _animationController.PlayOnce(DragonAnimation.No, DragonAnimation.Fly);
        break;
      case CharacterStates.TalkingMad:
        _animationController.PlayOnce(DragonAnimation.Roar, DragonAnimation.Fly);
        break;
      case CharacterStates.Flabbergasted:
        _animationController.SetAnimation(DragonAnimation.Die);
        _startPosition = transform.position;
        _targetPosition = GetStandardPositionByPlayer();
        _targetPosition.y = PlaneManager.Instance.GroundHeight;
        break;
      case CharacterStates.ShownObject:
        BusyPathing = true;

        _shownObjectLookPosition = _cameraTransform.position + GetFlat(_cameraTransform.forward) * _shownObjectDistanceForward;

        var objectToCameraDir = GetFlat(_cameraTransform.position - _shownObjectLookPosition);
        objectToCameraDir = Quaternion.AngleAxis(_shownObjectAngleToSeekTo, Vector3.up) * objectToCameraDir;

        _targetPosition = _shownObjectLookPosition + objectToCameraDir * _shownObjectLookDistance;
        _targetPosition.y = PlaneManager.Instance.GroundHeight + _heightFromGroundToSeek;
        _startPosition = transform.position;
        _traverseProgress = 0f;

        _animationController.SetAnimation(DragonAnimation.Fly);
        break;
      case CharacterStates.PathToPlayerAndPresentPicture:
        _targetPosition = GetStandardPositionByPlayer();
        _startPosition = transform.position;
        _traverseProgress = 0f;
        BusyPathing = true;
        break;
      case CharacterStates.PresentingPicture:
        _animationController.PlayOnce(DragonAnimation.Roar, DragonAnimation.Jump);
        break;
      case CharacterStates.FlyingToPortal:
        _startPosition = transform.position;
        _targetPosition = PortalManager.Instance.GetBigPortalPosition() + Vector3.left * _distanceFromPlayerToSeek;
        _animationController.PlayOnce(DragonAnimation.Yes, DragonAnimation.Fly);
        BusyPathing = true;
        break;
      case CharacterStates.IdleByPortal:
        transform.rotation = Quaternion.LookRotation(GetFlat(_cameraTransform.position - transform.position));
        break;
    }
  }

  private Vector3 GetStandardPositionByPlayer()
  {
    var rotatedDir = Quaternion.AngleAxis(_angleFromPlayerForwardToSeek, Vector3.up) * GetFlat(_cameraTransform.forward);
    var newPosition = _cameraTransform.position + rotatedDir * _distanceFromPlayerToSeek;
    newPosition.y = PlaneManager.Instance.GroundHeight + _heightFromGroundToSeek;
    return newPosition;
  }

  private void Update()
  {
    //TODO: Apply hover amplitude in a way that doesn't impact movement calcs
    var delta = Time.deltaTime;
    _stateTimeElapsed += delta;

    switch (_currentState)
    {
      case CharacterStates.InitialFlyIn:
        _traverseProgress 
          = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, _cameraTransform.position, _movementSpeed);

        var distanceToPoint = Vector3.Distance(transform.position, _targetPosition);
        if (distanceToPoint > _distanceToTargetBeforeRotationStart) _animationController.SetAnimation(DragonAnimation.Fly);

        //If reach target point, mark no longer pathing and change to idle state
        if (_traverseProgress >= 1f)
        {
          BusyPathing = false;
          SetState(CharacterStates.IdleWithPlayer);
        }
        break;
      case CharacterStates.ShownObject:
        _traverseProgress 
          = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, _shownObjectLookPosition, _movementSpeed);

        //If reach point, mark no longer pathing but don't change state
        if (_traverseProgress >= 1f)
        {
          BusyPathing = false;
        }
        break;
      case CharacterStates.JumpingToPlayer:
        var preJumpRot = GetFlattenedRotation(_targetPosition - _startPosition);

        if (_traverseProgress < _pathingSpinDuration) //Start off in a spin phase to aim toward target
        {
          var spinProgress = Easing.ExpoEaseOut(_traverseProgress / _pathingSpinDuration);
          transform.rotation = Quaternion.Slerp(_prePathRotation, preJumpRot, spinProgress);
        }
        else //Then trigger jump anim and start moving toward target
        {
          _animationController.SetAnimation(DragonAnimation.Jump);
          var jumpPath = _targetPosition - _startPosition;
          transform.position = _startPosition + jumpPath * (_traverseProgress - _pathingSpinDuration);

          var finalSpinProgress = Easing.ExpoEaseOut(Mathf.Clamp01(_traverseProgress - _pathingSpinDuration * 2f));
          var postJumpRot = GetFlattenedRotation(_cameraTransform.position - _targetPosition);
          transform.rotation = Quaternion.Slerp(preJumpRot, postJumpRot, finalSpinProgress);
        }

        _traverseProgress += delta;

        //Stop if enough time has elapsed that we should be at the target point
        if (_traverseProgress >= _pathingSpinDuration + _pathingJumpDuration)
        {
          BusyPathing = false;
          SetState(CharacterStates.IdleWithPlayer);
        }
        break;
      case CharacterStates.FlyingToPlayer:
        _traverseProgress 
          = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, _cameraTransform.position, _flyBackToPlayerSpeed);
        if (_traverseProgress >= 1f)
        {
          SetState(CharacterStates.IdleWithPlayer);
        }
        break;
      case CharacterStates.IdleWithPlayer:
      case CharacterStates.Talking:
      case CharacterStates.TalkingSurprised:
      case CharacterStates.TalkingDisappointed:
      case CharacterStates.TalkingMad:
        var distanceToCamera = Vector3.Distance(transform.position, _cameraTransform.position);
        var viewAngleThreshold = Mathf.Cos(_angleThresholdToTurnTowardPlayer * Mathf.Deg2Rad);
        var currentDirDot = Vector3.Dot(GetFlat(_cameraTransform.forward), GetFlat(transform.position - _cameraTransform.position));

        //If player can't see dragon for too long, or if it is too close or too far, eventually path back
        if (currentDirDot < viewAngleThreshold || distanceToCamera > _distanceThresholdToMoveTowardPlayer
          || distanceToCamera < _distanceThresholdToMoveAwayFromPlayer)
        {
          _uncomfortableTimeElapsed += delta;
          if (_uncomfortableTimeElapsed >= _uncomfortableDurationThreshold)
          {
            SetState(CharacterStates.FlyingToPlayer);
          }
        }
        else
        {
          _uncomfortableTimeElapsed = 0f;
        }

        //TODO: See if a random animation should be triggered

        break;
      case CharacterStates.FlyingToPortal:
        _traverseProgress = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, _cameraTransform.position, _movementSpeed);
        if (_traverseProgress >= 1f)
        {
          BusyPathing = false;
        }
        break;
      case CharacterStates.Flabbergasted:
        var progress = Mathf.Clamp01(_stateTimeElapsed / _dieAnimDuration);
        transform.position = Vector3.Lerp(_startPosition, _targetPosition, progress);
        break;
      case CharacterStates.PathToPlayerAndPresentPicture:
        if (_traverseProgress < 1f)
        {
          _traverseProgress
            = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, _cameraTransform.position, _movementSpeed);
        }
        else
        {
          SetState(CharacterStates.PresentingPicture);
        }
        break;
    }
  }

  private float DoMotionTowardPointAndRotationTowardTarget(float delta, float progress, Vector3 positionToLookAt, float speed)
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
      var targetRot = GetFlattenedRotation(positionToLookAt - _targetPosition);
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

public enum CharacterStates
{
  None,
  InitialFlyIn,
  JumpingToPlayer,
  FlyingToPlayer,
  IdleWithPlayer,
  Talking,
  TalkingSurprised,
  TalkingDisappointed,
  TalkingMad,
  ShownObject,
  PathToPlayerAndPresentPicture,
  PresentingPicture,
  Flabbergasted,
  FlyingToPortal,
  IdleByPortal
}