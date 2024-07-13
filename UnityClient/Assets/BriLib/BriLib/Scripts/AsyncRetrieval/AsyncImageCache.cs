using System.Threading.Tasks;
using UnityEngine;

namespace BriLib
{
  public class AsyncImageCache : Singleton<AsyncImageCache>
  {
    [System.Obsolete]
    private AsyncCache<Texture> _cache = new AsyncCache<Texture>((www) =>
    {
      LogManager.Info("Got texture off of www obj");
      return www.texture;
    });

    [System.Obsolete]
    public Task<Texture> GetResult(string url)
    {
      return _cache.GetResult(url);
    }
  }
}