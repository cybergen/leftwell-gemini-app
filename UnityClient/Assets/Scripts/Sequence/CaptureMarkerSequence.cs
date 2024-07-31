using UnityEngine;
using System.Threading.Tasks;

public class CaptureMarkerSequence : ISequence<Texture2D, string>
{
  private Texture2D _transformedTexture;
  private string _commentary;
  private int _markerIndex;
  private bool _activated;

  public void SetCommentary(string commentary)
  {
    _commentary = commentary;
    CheckFinished();
  }

  public async Task<string> RunAsync(Texture2D arg)
  {
    _markerIndex = PortalManager.Instance.SpawnCaptureMarker();

    _transformedTexture = await ImageGenerationManager.Instance.GetRandomlyEditedImage(arg);
    CheckFinished();

    while (!_activated) await Task.Delay(10);
    return _commentary;
  }

  private void CheckFinished()
  {
    if (_transformedTexture != null && !string.IsNullOrEmpty(_commentary))
    {
      PortalManager.Instance.SetMarkerActivatable(_markerIndex, _transformedTexture, OnActivated);
    }
  }

  private async void OnActivated()
  {
    _activated = true;
    while (SpeechManager.Instance.Speaking) { await Task.Delay(10); }
    await Task.Delay(DialogConstants.DIALOG_PAUSE);
    _ = SpeechManager.Instance.Speak(_commentary);
  }
}