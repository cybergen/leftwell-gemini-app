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
      Texture2D texture = ImageGenerationManager.Base64ToTexture(imageEncodings[i]);
      if (texture != null)
      {
        _uiImages[i].texture = texture;
        _uiImages[i].SetNativeSize();
      }
    }

    var bigImage = await ImageGenerationManager.Instance.UpscaleImageBase64Ecoded(imageEncodings[0], prompt);
    Texture2D textureTwo = ImageGenerationManager.Base64ToTexture(bigImage);
    if (textureTwo != null)
    {
      _uiImages[4].texture = textureTwo;
      _uiImages[4].SetNativeSize();
    }
  }
}