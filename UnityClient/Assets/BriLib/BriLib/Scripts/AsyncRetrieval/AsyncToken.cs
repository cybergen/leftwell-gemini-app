using System.Threading.Tasks;

namespace BriLib
{
  public class AsyncToken<T>
  {
    public T Result;

    public async Task<T> GetResult()
    {
      while (Result == null) await Task.Delay(250);
      return Result;
    }
  }
}