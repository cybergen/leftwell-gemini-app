using System;
using System.Threading.Tasks;
using BriLib;

public class ChooseStorySequence : ISequence<string>
{
  private const int OPTION_COUNT = 3;

  public async Task<string> RunAsync()
  {
    await SpeechManager.Instance.Speak(DialogConstants.STORY_OPTIONS_INTRO);
    var options = DialogConstants.GetStoryStrings(OPTION_COUNT);
    string chosenOption = string.Empty;
    for (int i = 0; i < OPTION_COUNT; i++)
    {
      _ = SpeechManager.Instance.Speak(DialogConstants.GetRandomOptionPrecedent() + options[i]);
      var madeSelection = false;
      Action onYes = () => { madeSelection = true; chosenOption = options[i]; };
      Action onNo = () => { madeSelection = true; };
      UIManager.Instance.YesNoScreen.Show(options[i], onYes, onNo);
      while (!madeSelection) { await Task.Delay(10); }
      if (!string.IsNullOrEmpty(chosenOption)) break;
      await Task.Delay(600);
    }

    if (string.IsNullOrEmpty(chosenOption))
    {
      await SpeechManager.Instance.Speak(DialogConstants.NO_STORY_CHOICE);
      chosenOption = MathHelpers.SelectFromRange(options, new Random());
    }

    return chosenOption;
  }
}