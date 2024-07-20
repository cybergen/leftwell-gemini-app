using UnityEngine;
using LLM.Network;
using System.Threading.Tasks;

public class AdventureSequence : ISequence<CharacterBehaviorController, AdventureResult>
{
  public async Task<AdventureResult> RunAsync(CharacterBehaviorController character)
  {
    character.SetState(CharacterStates.InitialFlyIn);
    while (character.BusyPathing) { await Task.Delay(10); }
    await SpeechManager.Instance.SpeakSSML($"<speak>Here's an example of <break time=\"1s\"/>post character audio</speak>");
    return new AdventureResult();
  }
}

public class AdventureResult
{
  public Texture2D ResultImage;
  public string Synopsis;
  public LLMRequestPayload Chat;
}