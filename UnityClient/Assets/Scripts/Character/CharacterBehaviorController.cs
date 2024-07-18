using UnityEngine;
using BriLib;
using System;

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
  [Header("Path to Player Config")]
  [SerializeField] private float _pathingSpinDuration = 0.25f;
  [SerializeField] private float _pathingJumpDuration = 1f;

  private CharacterStates _currentState = CharacterStates.None;
  //Static position traversal tracking
  private Vector3 _startPosition = Vector3.zero;
  private Vector3 _targetPosition = Vector3.zero;
  private float _traverseProgress = 0f;
  private Vector3 _shownObjectLookPosition = Vector3.zero;
  private Quaternion _prePathRotation;

  public void SetState(CharacterStates state)
  {
    Debug.Log($"Got set state call {state}");

    switch (_currentState)
    {
      case CharacterStates.InitialFlyIn:
      case CharacterStates.ShownObject:
        BusyPathing = false;
        break;
    }

    _currentState = state;

    var camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
    camForward.Normalize();

    switch (_currentState)
    {
      case CharacterStates.InitialFlyIn:
        BusyPathing = true;

        camForward = Quaternion.AngleAxis(_flyInStartingAngleFromPlayerForward, Vector3.up) * camForward;
        _startPosition = _cameraTransform.position + camForward * _flyInStartingDistance;
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
      case CharacterStates.ShownObject:
        BusyPathing = true;

        var flattenedDir = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up).normalized;
        _shownObjectLookPosition = _cameraTransform.position + flattenedDir * _shownObjectDistanceForward;

        var objectToCameraDir = (_cameraTransform.position - _shownObjectLookPosition).normalized;
        objectToCameraDir = Quaternion.AngleAxis(_shownObjectAngleToSeekTo, Vector3.up) * objectToCameraDir;

        _targetPosition = _shownObjectLookPosition + objectToCameraDir * _shownObjectLookDistance;
        _startPosition = transform.position;
        _traverseProgress = 0f;

        _animationController.SetAnimation(DragonAnimation.Fly);
        break;
      case CharacterStates.PresentingPicture: break;
    }
  }

  private Vector3 GetStandardPositionByPlayer()
  {
    var camForward = Vector3.ProjectOnPlane(_cameraTransform.forward, Vector3.up);
    camForward.Normalize();
    camForward = Quaternion.AngleAxis(_angleFromPlayerForwardToSeek, Vector3.up) * camForward;
    var newPosition = _cameraTransform.position + camForward * _distanceFromPlayerToSeek;
    newPosition.y = PlaneManager.Instance.GroundHeight + _heightFromGroundToSeek;
    return newPosition;
  }

  private void Update()
  {
    //TODO: Apply hover amplitude in a way that doesn't impact movement calcs
    var delta = Time.deltaTime;
    var offset = Mathf.Sin(Time.time * _upDownHoverSpeed) * _upDownHoverAmplitude;
    var camPos = _cameraTransform.position;
    var selfPos = transform.position;

    switch (_currentState)
    {
      case CharacterStates.InitialFlyIn:
        _traverseProgress = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, camPos);

        var distanceToPoint = Vector3.Distance(transform.position, _targetPosition);
        if (distanceToPoint > _distanceToTargetBeforeRotationStart) _animationController.SetAnimation(DragonAnimation.Fly);

        //If reach point, mark no longer pathing and change state
        if (_traverseProgress >= 1f)
        {
          BusyPathing = false;
          SetState(CharacterStates.IdleWithPlayer);
        }
        break;
      case CharacterStates.ShownObject:
        _traverseProgress = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, _shownObjectLookPosition);

        //If reach point, mark no longer pathing but don't change state
        if (_traverseProgress >= 1f)
        {
          BusyPathing = false;
        }
        break;
      case CharacterStates.JumpingToPlayer:
        var preJumpRot = GetFlattenedRotation(_targetPosition - _startPosition);

        if (_traverseProgress < _pathingSpinDuration)
        {
          var spinProgress = Easing.ExpoEaseOut(_traverseProgress / _pathingSpinDuration);
          transform.rotation = Quaternion.Slerp(_prePathRotation, preJumpRot, spinProgress);
        }
        else
        {
          _animationController.SetAnimation(DragonAnimation.Jump);
          var jumpPath = _targetPosition - _startPosition;
          transform.position = _startPosition + jumpPath * (_traverseProgress - _pathingSpinDuration);

          var finalSpinProgress = Easing.ExpoEaseOut(Mathf.Clamp01(_traverseProgress - _pathingSpinDuration * 2f));
          var postJumpRot = GetFlattenedRotation(_cameraTransform.position - _targetPosition);
          transform.rotation = Quaternion.Slerp(preJumpRot, postJumpRot, finalSpinProgress);
        }

        _traverseProgress += delta;

        //Stop if we've made it all the way to target point
        if (_traverseProgress >= _pathingSpinDuration + _pathingJumpDuration)
        {
          BusyPathing = false;
          SetState(CharacterStates.IdleWithPlayer);
        }
        break;
      case CharacterStates.FlyingToPlayer:
        _traverseProgress = DoMotionTowardPointAndRotationTowardTarget(delta, _traverseProgress, _cameraTransform.position);
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
        var flattenedForward = GetFlattened(transform.forward);
        var flattenedCamDir = GetFlattened(camPos - selfPos);
        var distanceToCamera = Vector3.Distance(selfPos, camPos);

        //Check for rotation outside of threshold and instruct to path to player if so
        var viewAngleThreshold = Mathf.Cos(_angleThresholdToTurnTowardPlayer * Mathf.Deg2Rad);
        if (Vector3.Dot(flattenedForward, flattenedCamDir) < viewAngleThreshold)
        {
          SetState(CharacterStates.FlyingToPlayer);
          break;
        }
        else if (distanceToCamera > _distanceThresholdToMoveTowardPlayer
          || distanceToCamera < _distanceThresholdToMoveAwayFromPlayer)
        {
          SetState(CharacterStates.FlyingToPlayer);
          break;
        }

        //TODO: See if a random animation should be triggered

        break;
    }
  }

  private float DoMotionTowardPointAndRotationTowardTarget(float delta, float progress, Vector3 positionToLookAt)
  {
    //Path toward position if not there yet
    var flightDistance = Vector3.Distance(_startPosition, _targetPosition);
    if (progress < 1f)
    {
      progress += delta / (flightDistance / _movementSpeed);
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
    return Quaternion.LookRotation(GetFlattened(direction));
  }

  private Vector3 GetFlattened(Vector3 direction)
  {
    return Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
  }

  private void Awake()
  {
    _cameraTransform = FindObjectOfType<Camera>().transform;
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
  PresentingPicture
}