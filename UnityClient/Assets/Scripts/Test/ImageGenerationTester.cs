using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageGenerationTester : MonoBehaviour
{
  [SerializeField]
  private List<RawImage> _uiImages;

  public void Start()
  {
    GrabImages();
  }

  public async void GrabImages()
  {
    var prompt = "A picture of an extremely cute cat with default tabby coat";
    var imageEncodings = await ImageGenerationManager.Instance.GetImagesBase64Encoded(
      prompt: prompt,
      negativePrompt: "too many fingers"
    );

    for (int i = 0; i < imageEncodings.Count; i++)
    {
      Texture2D texture = Base64ToTexture(imageEncodings[i]);
      if (texture != null)
      {
        _uiImages[i].texture = texture;
        _uiImages[i].SetNativeSize();
      }
    }

    var bigImage = await ImageGenerationManager.Instance.UpscaleImageBase64Ecoded(imageEncodings[0], prompt);
    Texture2D textureTwo = Base64ToTexture(bigImage);
    if (textureTwo != null)
    {
      _uiImages[4].texture = textureTwo;
      _uiImages[4].SetNativeSize();
    }
  }

  private Texture2D Base64ToTexture(string base64String)
  {
    try
    {
      byte[] imageBytes = Convert.FromBase64String(base64String);
      Texture2D texture = new Texture2D(2, 2); // Create a small placeholder texture
      texture.LoadImage(imageBytes); // Load the image bytes into the texture
      return texture;
    }
    catch (Exception e)
    {
      Debug.LogError("Failed to convert base64 string to texture: " + e.Message);
      return null;
    }
  }
}