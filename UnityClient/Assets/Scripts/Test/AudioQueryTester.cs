using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using LLM.Network;

public class AudioQueryTester : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
  [SerializeField] private TMP_Text _outputText;
  private const int MAX_FILE_BYTES = 20000000;

  public void OnPointerDown(PointerEventData eventData)
  {
    _outputText.text = "Starting audio capture";
    AudioCaptureManager.Instance.StartAudioCapture();
  }

  public async void OnPointerUp(PointerEventData eventData)
  {
    _outputText.text = "Capturing image";
    AudioCaptureManager.Instance.EndAudioCapture();
    var camImage = await CameraImageManager.Instance.GetNextAvailableCameraImage();
    _outputText.text = "Encoding audio";
    var audioBytes = await AudioCaptureManager.Instance.GetNextAudioData();
    var imageBlob = Convert.ToBase64String(camImage.Texture.EncodeToPNG());

    var partList = new List<BasePart>();

    if (audioBytes.Length > MAX_FILE_BYTES)
    {
      _outputText.text = "Uploading audio separately";
      var file = await FileUploadManager.Instance.UploadFile("audio/wav", "Device audio during AR session", audioBytes);
      _outputText.text = "Audio uploaded";
      Debug.Log($"Finished upload with file {file}");
      partList.Add(new FilePart
      {
        fileData = new FilePartData
        {
          mimeType = file.file.mimeType,
          fileUri = file.file.uri
        }
      });
    }
    else
    {
      Debug.Log("Audio fits in single upload, pushing up blob directly");
      var audioBlob = Convert.ToBase64String(audioBytes);
      partList.Add(new DataPart
      {
        inlineData = new Blob
        {
          mimeType = "audio/wav",
          data = audioBlob
        }
      });
    }

    partList.Add(new DataPart
    {
      inlineData = new Blob
      {
        mimeType = "image/png",
        data = imageBlob
      }
    });

    partList.Add(new TextPart
    {
      text = "Please transcribe the attached audio into text. Then answer the question in the transcribed audio by analyzing the contents of the attached image."
    });

    _outputText.text = "Making request to Gemini";
    var payload = LLMRequestPayload.GetRequestWithMultipleParts(partList.ToArray());
    var response = await LLMInteractionManager.Instance.RequestLLMCompletion(payload);
    Debug.Log($"Got LLM response:\n{response}");
    _outputText.text = response.candidates[0].content.parts[0].text;
  }
}