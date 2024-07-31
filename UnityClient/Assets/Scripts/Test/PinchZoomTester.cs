using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinchZoomTester : MonoBehaviour
{
  [SerializeField] private Texture2D _sampleImage;

  private void Start()
  {
    UIManager.Instance.StoryResult.Show(_sampleImage, "Here is some sample text. It isn't important.", Hide, (r) => Hide());
  }

  private void Hide()
  {
    UIManager.Instance.StoryResult.Hide();
  }
}
