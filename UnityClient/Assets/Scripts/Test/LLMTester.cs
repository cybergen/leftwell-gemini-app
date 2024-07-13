using UnityEngine;
using System.Threading.Tasks;
using LLM.Network;

public class LLMTester: MonoBehaviour
{
  private void Start()
  {
    TestAfterStart();
  }

  private async void TestAfterStart()
  {
    await Task.Delay(5000);
    var request = "Please describe what it is like to only live while a block of text passes through your attention heads";
    LLMRequestPayload payload = LLMRequestPayload.GetRequestFromSingleRequestString(request);
    var response = await LLMInteractionManager.Instance.RequestLLMCompletion(payload);
    Debug.Log($"Got response {response}");
  }
}