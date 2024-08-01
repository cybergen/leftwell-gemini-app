using UnityEngine;
using System.Threading.Tasks;
using static ImageGenerationManager;

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

    (Texture2D image, ImageGenStatus status) imageGenResponse;
    int tries = 0;
    do
    {
      tries++;
      imageGenResponse = await ImageGenerationManager.Instance.GetRandomlyEditedImage(arg);
    }
    while (tries < 3 && (imageGenResponse.status == ImageGenStatus.FailedDueToSafetyGuidelines
      || imageGenResponse.status == ImageGenStatus.FailedForOtherReason));

    if (imageGenResponse.status != ImageGenStatus.Succeeded && imageGenResponse.status != ImageGenStatus.SucceededAfterRetry)
    {
      await SpeechManager.Instance.Speak(DialogConstants.FAILED_TO_GET_ITEM_IMAGE);
      _transformedTexture = arg;
    }
    else
    {
      _transformedTexture = imageGenResponse.image;
    }

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