using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLM.Network;

public class CameraImageTest : MonoBehaviour
{
  public RawImage CameraDisplay;
  public TMP_Text InfoText;

  public async void OnTriggerScreenshot()
  {
    var camImage = await CameraManager.Instance.GetNextAvailableCameraImage();

    if (InfoText != null)
    {
      InfoText.text = camImage.ImageInfo;
    }

    if (CameraDisplay != null)
    {
      CameraDisplay.texture = camImage.Texture;
    }

    var blob = Convert.ToBase64String(camImage.Texture.EncodeToPNG());
    var payload = LLMRequestPayload.GetRequestWithMedia("Describe what is in the attached image", blob, "image/png");
    var response = await LLMInteractionManager.Instance.RequestLLMCompletion(payload);
    Debug.Log($"Got LLM response:\n{response}");
  }
}