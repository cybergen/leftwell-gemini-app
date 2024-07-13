using UnityEngine;

namespace BriLib {
  public class CameraCapture : MonoBehaviour
  {
    public int resWidth = 1920;
    public int resHeight = 1080;

    private Camera _cam;

    private void Start()
    {
      _cam = GetComponent<Camera>();
    }

    public string Directory()
    {
      return $"{Application.dataPath}/Screenshots";
    }

    public string ScreenShotName(int width, int height)
    {
      return string.Format("{0}/screen_{1}x{2}_{3}.png",
                           Directory(),
                           width, height,
                           System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
    }

    private void LateUpdate()
    {
      if (Input.GetKeyDown(KeyCode.Space))
      {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        _cam.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        _cam.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        _cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = ScreenShotName(resWidth, resHeight);
        if (!System.IO.Directory.Exists(Directory()))
        {
          System.IO.Directory.CreateDirectory(Directory());
        }
        System.IO.File.WriteAllBytes(filename, bytes);
        LogManager.Info(string.Format("Took screenshot to: {0}", filename));
      }
    }
  }
}
