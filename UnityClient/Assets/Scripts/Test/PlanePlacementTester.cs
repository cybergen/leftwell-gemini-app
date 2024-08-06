using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class PlanePlacementTester : MonoBehaviour
{
  [SerializeField] private ARPlaneManager _planeManager;
  [SerializeField] private GameObject _dragonPrefab;
  [SerializeField] private Transform _cameraTransform;
  [SerializeField] private float _distanceFromCamera;
  [SerializeField] private float _heightAboveGround;
  [SerializeField] private float _angleFromPlayerForwardToSeek;
  private GameObject _activeDragon;
  private CharacterBehaviorController _behaviorController;
  private CharacterAnimationController _animationController;

  public void OnFlyInClicked()
  {
    _behaviorController.SetState(CharacterState.InitialFlyIn);
  }

  public void OnShowObjectClicked()
  {
    _behaviorController.SetState(CharacterState.ShownObject);
  }

  public void OnIdleClicked()
  {
    _behaviorController.SetState(CharacterState.IdleWithPlayer);
  }

  public void OnPlayYesClicked()
  {
    _animationController.PlayOnce(DragonAnimation.Yes, DragonAnimation.Fly);
  }

  public void OnPlayNoClicked()
  {
    _animationController.PlayOnce(DragonAnimation.No, DragonAnimation.Fly);
  }

  public void OnPlayRoarClicked()
  {
    _animationController.PlayOnce(DragonAnimation.Roar, DragonAnimation.Fly);
  }

  public void OnPlayJumpClicked()
  {
    _animationController.PlayOnce(DragonAnimation.Jump, DragonAnimation.Fly);
  }

  public void OnPlayFireClicked()
  {
    _animationController.PlayOnce(DragonAnimation.Fire, DragonAnimation.Fly);
  }

  public void OnPlayDamageClicked()
  {
    _animationController.PlayOnce(DragonAnimation.Damage, DragonAnimation.Fly);
  }

  public void OnPathToPlayerClicked()
  {
    _behaviorController.SetState(CharacterState.JumpingToPlayer);
  }

  public void OnTalkingMadClicked()
  {
    _behaviorController.SetState(CharacterState.Talking);
  }

  public void OnPresentingPictureClicked()
  {
    _behaviorController.SetState(CharacterState.PathToPlayerAndPresentPicture);
  }

  public void OnFlabbergastedClicked()
  {
    _behaviorController.SetState(CharacterState.Flabbergasted);
  }

  public void OnFlyBackToPlayerClicked()
  {
    _behaviorController.SetState(CharacterState.FlyingToPlayer);
  }

  public void OnMagicItemClicked()
  {
    _behaviorController.SetState(CharacterState.MagicingItem);
  }

  private void Start()
  {
    _planeManager.planesChanged += OnPlanesChanged;
  }

  private void OnDestroy()
  {
    _planeManager.planesChanged -= OnPlanesChanged;
  }

  private void OnPlanesChanged(ARPlanesChangedEventArgs args)
  {
    if (args.added == null || args.added.Count == 0) return;

    if (_activeDragon == null) 
    { 
      _activeDragon =  Instantiate(_dragonPrefab);
      _behaviorController = _activeDragon.GetComponent<CharacterBehaviorController>();
      _animationController = _activeDragon.GetComponent<CharacterAnimationController>();
    }
  }
}