using System.Threading.Tasks;

public class CharacterHelpers
{
  private const int ANIM_DELAY_MILLIS = 1200;

  public static void AnimateEventually(CharacterState state, CharacterBehaviorController dragon)
  {
    _ = AnimateEventuallyAsync(state, dragon);
  }

  private static async Task AnimateEventuallyAsync(CharacterState state, CharacterBehaviorController dragon)
  {
    await Task.Delay(ANIM_DELAY_MILLIS);
    dragon.SetState(state);
  }
}