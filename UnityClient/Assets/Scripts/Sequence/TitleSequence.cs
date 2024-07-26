using System;
using System.Threading.Tasks;

public class TitleSequence : ISequence
{
  public async Task RunAsync()
  {
    var finished = false;
    Action onStartPressed = () => {
      finished = true;
    };
    UIManager.Instance.TitleScreen.Show(onStartPressed);
    while (!finished) { await Task.Delay(10); }
  }
}