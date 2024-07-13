using System;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using BriLib;

public class CameraImageManager : Singleton<CameraImageManager>
{
  [SerializeField]
  private ARCameraManager _cameraManager;
  private XRCpuImage.Transformation _transformation = XRCpuImage.Transformation.MirrorY;
  private const TextureFormat _format = TextureFormat.RGBA32;
  private Texture2D _captureTexture;

  public async Task<CameraImage> GetNextAvailableCameraImage()
  {
    CameraImage cameraImage;
    do
    {
      await Task.Delay(17); // Wait one frame between retrieval attempts
      cameraImage = GetCameraImage();
    } while (cameraImage == null);
    return cameraImage;
  }

  // Adapted from ARFoundation samples
  public unsafe CameraImage GetCameraImage()
  {
    if (!_cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
    {
      Debug.LogError("Failed to acquire CPU image from camera manager");
      return null;
    }

    var info = $"Image info:\nwidth: {image.width}\nheight: {image.height}\nplaneCount: {image.planeCount}\ntimestamp: {image.timestamp}\nformat: {image.format}";

    if (_captureTexture == null || _captureTexture.width != image.width || _captureTexture.height != image.height)
      _captureTexture = new Texture2D(image.width, image.height, _format, false);

    var conversionParams = new XRCpuImage.ConversionParams(image, _format, _transformation);
    var rawTextureData = _captureTexture.GetRawTextureData<byte>();

    try
    {
      image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
    }
    finally
    {
      image.Dispose();
    }

    _captureTexture.Apply();
    return new CameraImage
    {
      Texture = RotateTexture(_captureTexture, false),
      ImageInfo = info
    };
  }

  private Texture2D RotateTexture(Texture2D originalTexture, bool clockwise)
  {
    Color32[] original = originalTexture.GetPixels32();
    Color32[] rotated = new Color32[original.Length];
    int w = originalTexture.width;
    int h = originalTexture.height;

    int iRotated, iOriginal;

    for (int j = 0; j < h; ++j)
    {
      for (int i = 0; i < w; ++i)
      {
        iRotated = (i + 1) * h - j - 1;
        iOriginal = clockwise ? original.Length - 1 - (j * w + i) : j * w + i;
        rotated[iRotated] = original[iOriginal];
      }
    }

    Texture2D rotatedTexture = new Texture2D(h, w);
    rotatedTexture.SetPixels32(rotated);
    rotatedTexture.Apply();
    return rotatedTexture;
  }

  public class CameraImage
  {
    public Texture2D Texture;
    public string ImageInfo;
  }
}
