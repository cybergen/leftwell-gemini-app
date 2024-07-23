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
    _markerIndex = PortalManager.Instance.SpawnCaptureMarker(arg);
    PortalManager.Instance.SetMarkerLoading(_markerIndex);

    _transformedTexture = await ImageGenerationManager.Instance.GetRandomlyEditedImage(arg);
    CheckFinished();

    while (!_activated) await Task.Delay(10);
    return _commentary;
  }

  private void CheckFinished()
  {
    if (_transformedTexture != null && !string.IsNullOrEmpty(_commentary))
    {
      PortalManager.Instance.SupplyTransformedImage(_markerIndex, _transformedTexture, OnActivated);
    }
  }

  private void OnActivated()
  {
    _activated = true;
    _ = SpeechManager.Instance.Speak(_commentary);
  }
}