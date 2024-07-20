using System.Threading.Tasks;

public class TutorialSequence : ISequence
{
  public async Task RunAsync()
  {
    await Task.Delay(10);
    return;
  }
}