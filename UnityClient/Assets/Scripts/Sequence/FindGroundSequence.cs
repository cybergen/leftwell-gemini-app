using System.Threading.Tasks;

public class FindGroundSequence : ISequence
{
  public async Task RunAsync()
  {
    while (!PlaneManager.Instance.Ready) { await Task.Delay(10); }
    return;
  }
}