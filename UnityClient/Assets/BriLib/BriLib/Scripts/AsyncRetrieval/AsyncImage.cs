using UnityEngine;
using UnityEngine.UI;

namespace BriLib
{
  [RequireComponent(typeof(RawImage))]
  public class AsyncImage : MonoBehaviour
  {
    public string Url;
    public Texture PlaceholderImage;
    public GameObject LoadingImage;
    public LoadOptions LoadingBehaviorOptions;
    public ResizeOptions ResizeOnDownloadOptions;

    private RawImage _image;
    private bool _downloaded;
    private string _oldUrl = "PLACEHOLDER";
    private float _oldAlpha;

    public void Refresh()
    {
      if (_downloaded) _oldUrl = string.Empty;
    }

    private void OnEnable()
    {
      _image = gameObject.GetComponent<RawImage>();
    }

    [System.Obsolete]
    private void Update()
    {
      if (Url == _oldUrl || string.IsNullOrEmpty(Url)) return;

      _downloaded = false;
      _oldUrl = Url;

      //Configure loading state
      switch (LoadingBehaviorOptions)
      {
        case LoadOptions.None:
          break;
        case LoadOptions.ShowLoading:
          LoadingImage.SetActive(true); 
          break;
        case LoadOptions.ShowPlaceholder:
          _image.texture = PlaceholderImage;
          break;
        case LoadOptions.ShowTransparent:
          _oldAlpha = _image.color.a;
          var oldColor = _image.color;
          oldColor.a = 0f;
          _image.color = oldColor;
          break;
      }

      //Trigger download
      AsyncImageCache.Instance.GetResult(Url).ContinueWith(dlTask =>
      {
        LogManager.Info("Finished dl now resizing");
        if (dlTask.IsFaulted || dlTask.IsCanceled || dlTask.Result == null)
        {
          MainThreadQueue.Instance.QueueAction(() =>
          {
            LogManager.Error("Failed to retrieve image from cache");

            //Reset loading states
            switch (LoadingBehaviorOptions)
            {
              case LoadOptions.None:
                break;
              case LoadOptions.ShowLoading:
                LoadingImage.SetActive(false); 
                break;
              case LoadOptions.ShowPlaceholder:
                break;
              case LoadOptions.ShowTransparent:
                var oldColor = _image.color;
                oldColor.a = _oldAlpha;
                _image.color = oldColor;
                break;
            }
          });
        }
        else
        {
          MainThreadQueue.Instance.QueueAction(() =>
          {
            //Apply texture
            _image.texture = dlTask.Result;

            //Do resize behavior
            var oldSize = _image.rectTransform.sizeDelta;
            var newSize = new Vector2(dlTask.Result.width, dlTask.Result.height);
            var targetRatio = (float)dlTask.Result.width / dlTask.Result.height;

            switch (ResizeOnDownloadOptions)
            {
              case ResizeOptions.NoResize:
                newSize = oldSize;
                break;
              case ResizeOptions.MatchTarget:
                break;
              case ResizeOptions.FitWidth:
                newSize.x = oldSize.x;
                newSize.y = 1f / targetRatio * newSize.x;
                break;
              case ResizeOptions.FitHeight:
                newSize.y = oldSize.y;
                newSize.x = targetRatio * newSize.y;
                break;
              case ResizeOptions.ShrinkToFit:
                if (targetRatio >= 1.0f)
                {
                  newSize.x = oldSize.x;
                  newSize.y = 1f / targetRatio * newSize.x;
                }
                else
                {
                  newSize.y = oldSize.y;
                  newSize.x = targetRatio * newSize.y;
                }
                break;
            }

            _image.rectTransform.sizeDelta = newSize;
            
            //Resolve loading behavior
            switch (LoadingBehaviorOptions)
            {
              case LoadOptions.None:
                break;
              case LoadOptions.ShowLoading:
                LoadingImage.SetActive(false); 
                break;
              case LoadOptions.ShowPlaceholder:
                break;
              case LoadOptions.ShowTransparent:
                var oldColor = _image.color;
                oldColor.a = _oldAlpha;
                _image.color = oldColor;
                break;
            }
          });
          _downloaded = true;
        }
      });
    }

    public enum ResizeOptions
    {
      NoResize,
      MatchTarget,
      FitWidth,
      FitHeight,
      ShrinkToFit,
    }

    public enum LoadOptions
    {
      None,
      ShowPlaceholder,
      ShowLoading,
      ShowTransparent,
    }
  }
}