using UnityEngine;
using System.Threading.Tasks;

public class PlayAfterDelay : MonoBehaviour
{
  [SerializeField] private int _delayMillis;
  [SerializeField] private AudioSource _sourceToPlay;

  public async void Play()
  {
    await Task.Delay(_delayMillis);
    _sourceToPlay.Play();
  }
}