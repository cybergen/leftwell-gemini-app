using System.Threading.Tasks;

public class CharacterHelpers
{
  private const int ANIM_DELAY_MILLIS = 1200;

  public static void AnimateEventually(CharacterStates state, CharacterBehaviorController dragon)
  {
    _ = AnimateEventuallyAsync(state, dragon);
  }

  private static async Task AnimateEventuallyAsync(CharacterStates state, CharacterBehaviorController dragon)
  {
    await Task.Delay(ANIM_DELAY_MILLIS);
    dragon.SetState(state);
  }
}