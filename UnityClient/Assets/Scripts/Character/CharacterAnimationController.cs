using System.Threading.Tasks;
using UnityEngine;

public class CharacterAnimationController : MonoBehaviour
{
  [SerializeField] private Animator _animator;
  [SerializeField] private GameObject _fireParticles;
  [SerializeField] private PlayAfterDelay _fireSound;
  [SerializeField] private HatJumpAnimator _hatJump;
  [SerializeField] private int _hatJumpDelay = 600;
  [SerializeField] private int _hatJumpDurationMillis = 600;
  [SerializeField] private int _hatRoarDelay = 300;
  [SerializeField] private int _hatRoarDurationMillis = 400;

  public void SetAnimation(DragonAnimation animation)
  {
    if (animation == DragonAnimation.Jump)
    {
      _hatJump.Play(_hatJumpDelay, _hatJumpDurationMillis);
    }
    _animator.SetInteger("animation", (int)animation);
  }

  public async void PlayOnce(DragonAnimation animation, DragonAnimation subsequentAnimation)
  {
    int delayTime = 0;
    SetAnimation(animation);

    //Delay based on animation durations
    switch (animation)
    {
      case DragonAnimation.Yes:
        delayTime = 1150;
        break;
      case DragonAnimation.No:
        delayTime = 1150;
        break;
      case DragonAnimation.Roar:
        _hatJump.Play(_hatRoarDelay, _hatRoarDurationMillis);
        delayTime = 1550;
        break;
      case DragonAnimation.Jump:
        _hatJump.Play(_hatJumpDelay, _hatJumpDurationMillis);
        delayTime = 950;
        break;
      case DragonAnimation.Fire:
        delayTime = 560;
        _fireSound.Play();
        _fireParticles.SetActive(false);
        _fireParticles.SetActive(true);
        break;
      case DragonAnimation.Run:
        delayTime = 500;
        break;
      case DragonAnimation.Damage:
        delayTime = 250;
        break;
      case DragonAnimation.Die:
        delayTime = 917;
        break;
    }
    await Task.Delay(delayTime);

    //Transition to next animation when first one has resolved
    SetAnimation(subsequentAnimation);
  }

  private void Awake()
  {
    _fireParticles.SetActive(false);
  }
}

public enum DragonAnimation
{
  Idle = 1,
  Yes = 2,
  No = 3,
  Eat = 4,
  Roar = 5,
  Jump = 6,
  Die = 7,
  Rest = 8,
  Walk = 9,
  WalkLeft = 10,
  WalkRight = 11,
  Run = 12,
  RunLeft = 13,
  RunRight = 14,
  Fire = 15,
  Sick = 16,
  Fly = 17,
  FlyLeft = 18,
  FlyRight = 19,
  FlyUp = 20,
  FlyDown = 21,
  FlyFire = 22,
  Damage = 23
}