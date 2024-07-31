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
      negativePrompt: "too many fingers",
      imageCount: 4
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
  }
}