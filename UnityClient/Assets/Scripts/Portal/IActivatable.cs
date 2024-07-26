public interface IActivatable
{
  public bool Activatable { get; }
  public string ActivationText { get; }
  public void Activate();
}