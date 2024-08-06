public interface IActivatable
{
  public bool Activatable { get; }
  public string ActivationText { get; }
  public bool ShareMode { get; }
  public void Activate();
}