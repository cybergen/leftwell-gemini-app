using UnityEngine;

public class HeroPortal : MonoBehaviour, IActivatable
{
  public bool Activatable { get; private set; }

  public void Activate()
  {
    Activatable = false;
  }

  public void SetState(PortalState state)
  {

  }

  public enum PortalState
  {
    Start,
    Loading,
    Activatable,
    Activating
  }
}