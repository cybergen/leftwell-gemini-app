using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LLM.Network;

public class MediaUploadTester : MonoBehaviour
{
  public RawImage CameraDisplay;
  public TMP_Text InfoText;

  public async void OnTriggerScreenshot()
  {
    if (InfoText == null || CameraDisplay == null)
    {
      Debug.LogError("Missing info text or camera display raw image");
      return;
    }

    InfoText.text = "Capturing image";
    var camImage = await CameraImageManager.Instance.GetNextAvailableCameraImage();
    
    InfoText.text = camImage.ImageInfo;
    CameraDisplay.texture = camImage.Texture;

    var bytes = camImage.Texture.EncodeToPNG();
    InfoText.text = "Uploading image";
    var file = await FileUploadManager.Instance.UploadFile("image/png", "Camera shot during AR session", bytes);
    Debug.Log($"Finished upload with file {file}");

    InfoText.text = "Making request to Gemini with image";
    var dataPart = new FilePart
    {
      fileData = new FileInfo
      {
        mimeType = file.file.mimeType,
        fileUri = file.file.uri
      }
    };
    var textPart = new TextPart
    {
      text = "Describe what is in the referenced picture"
    };
    var payload = LLMRequestPayload.GetRequestWithMultipleParts(dataPart, textPart);
    var response = await LLMInteractionManager.Instance.RequestLLMCompletion(payload);
    Debug.Log($"Resolved LLM request with response {response}");
    InfoText.text = response.candidates[0].content.parts[0].text;
  }
}