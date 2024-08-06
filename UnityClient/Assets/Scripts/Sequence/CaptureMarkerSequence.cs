using UnityEngine;
using System.Threading.Tasks;
using static ImageGenerationManager;

public class CaptureMarkerSequence : ISequence<Texture2D, string>
{
  private Texture2D _transformedTexture;
  private string _commentary;
  private int _markerIndex;
  private bool _activated;
  private string _itemString;

  public CaptureMarkerSequence(string itemString)
  {
    _itemString = itemString;
  }

  public void SetCommentary(string commentary)
  {
    _commentary = commentary;
    CheckFinished();
  }

  public async Task<string> RunAsync(Texture2D arg)
  {
    _markerIndex = PortalManager.Instance.SpawnCaptureMarker();

    (Texture2D image, ImageGenStatus status) imageResponse;
    int tries = 0;
    do
    {
      tries++;
      imageResponse = await ImageGenerationManager.Instance.GetRandomlyEditedImage(arg);
    }
    while (tries < 3 && (imageResponse.status == ImageGenStatus.FailedDueToSafetyGuidelines
      || imageResponse.status == ImageGenStatus.FailedForOtherReason));

    if (imageResponse.status != ImageGenStatus.Succeeded && imageResponse.status != ImageGenStatus.SucceededAfterRetry)
    {
      await SpeechManager.Instance.Speak(AdventureDialog.FAILED_TO_GET_ITEM_IMAGE);
      _transformedTexture = arg;
    }
    else
    {
      _transformedTexture = imageResponse.image;
    }

    CheckFinished();

    while (!_activated) await Task.Delay(10);
    PortalManager.Instance.SetMarkerSharable(_markerIndex, "Share Transformed Item", null);
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
    await Task.Delay(AdventureDialog.DIALOG_PAUSE);
    _ = SpeechManager.Instance.Speak(_commentary);
  }
}