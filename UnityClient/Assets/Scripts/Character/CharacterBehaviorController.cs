using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
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
  [Tooltip("Meters per second")][SerializeField] private float _movementSpeed;
  [Tooltip("Degrees per second")][SerializeField] private float _rotationSpeed;
  [Header("Adjustment Thresholds")]
  [SerializeField] private float _angleThresholdToTurnTowardPlayer;
  [SerializeField] private float _distanceThresholdToMoveTowardPlayer;
  [SerializeField] private float _distanceThresholdToMoveAwayFromPlayer;
  [Header("Fly In Position Configuration")]
  [SerializeField] private float _flyInStartingHeight;
  [SerializeField] private float _flyInStartingDistance;
  [SerializeField] private float _flyInStartingAngleFromPlayerForward;
  [SerializeField] private float _flyInDistanceToStartRotating;
  [Header("Item Shown Position Configuration")]
  [Tooltip("Distance to point from camera")][SerializeField] private float _shownObjectDistanceForward;
  [Tooltip("How far away from whcih to look at object")][SerializeField] private float _shownObjectLookDistance;
  [SerializeField] private float _shownObjectAngleToSeekTo;
  [Header("Idle Procedural Animation")]
  [SerializeField] private float _upDownHoverAmplitude;
  [SerializeField] private float _upDownHoverSpeed;

  private CharacterStates _currentState;
  private Vector3 _shownObjectLookPosition = Vector3.zero;
  private Vector3 _shownObjectHoverPosition = Vector3.zero;

  public void SetState(CharacterStates state)
  {
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
        var targetPosition = _cameraTransform.position + camForward * _flyInStartingDistance;
        targetPosition.y = PlaneManager.Instance.GroundHeight + _flyInStartingHeight;
        transform.position = targetPosition;
        _animationController.SetAnimation(DragonAnimation.FlyDown);
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
        _shownObjectLookPosition = _cameraTransform.position + camForward * _shownObjectDistanceForward;
        var objectToCameraDir = _shownObjectLookPosition - _cameraTransform.position;
        objectToCameraDir.Normalize();
        objectToCameraDir = Quaternion.AngleAxis(_shownObjectAngleToSeekTo, Vector3.up) * objectToCameraDir;
        _shownObjectHoverPosition = _shownObjectLookPosition + objectToCameraDir * _shownObjectLookDistance;

        _animationController.SetAnimation(DragonAnimation.Fly);
        break;
      case CharacterStates.PresentingPicture: break;
    }
  }

  private void Update()
  {
    //Apply hover amplitude


    var delta = Time.deltaTime;
    switch (_currentState)
    {
      case CharacterStates.InitialFlyIn:
        //Path toward position if not there yet

        //Rotate toward player if within a certain threshold

        //If reach point, change state

        break;
      case CharacterStates.ShownObject:
        //Continue pathing toward target hover position

        //If within threshold, start rotating to look at object position
        
        //If reach point and finish rotating, mark not busy

        break;
      case CharacterStates.IdleWithPlayer:
      case CharacterStates.Talking:
      case CharacterStates.TalkingSurprised:
      case CharacterStates.TalkingDisappointed:
      case CharacterStates.TalkingMad:
        //Check for rotation outside of threshold

        //Check for too far away

        //Check for too close


        //Apply hover amplitude

        //TODO: See if a random animation should be triggered

        break;
    }
  }
}

public enum CharacterStates
{
  InitialFlyIn,
  IdleWithPlayer,
  Talking,
  TalkingSurprised,
  TalkingDisappointed,
  TalkingMad,
  ShownObject,
  PresentingPicture
}