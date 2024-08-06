public interface IProceduralAnimator
{
  public bool Animating { get; }
  public bool Cancelling { get; }
  public void Play();
  public void Stop();
}