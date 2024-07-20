using System.Threading.Tasks;

public interface ISequence
{
  public Task RunAsync();
}

public interface ISequence<T>
{
  public Task<T> RunAsync();
}

public interface ISequence<T, K>
{
  public Task<K> RunAsync(T arg);
}