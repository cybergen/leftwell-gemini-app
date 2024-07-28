using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public static class HttpRequestLogger
{
  public static async Task LogHttpRequestMessageAsync(HttpRequestMessage request)
  {
    // Log the request method and URL
    Debug.Log($"Request Method: {request.Method}");
    Debug.Log($"Request URL: {request.RequestUri}");

    // Log the headers
    Debug.Log("Request Headers:");
    foreach (var header in request.Headers)
    {
      Debug.Log($"{header.Key}: {string.Join(", ", header.Value)}");
    }

    // Log the content headers if there is content
    if (request.Content != null)
    {
      Debug.Log("Request Content Headers:");
      foreach (var header in request.Content.Headers)
      {
        Debug.Log($"{header.Key}: {string.Join(", ", header.Value)}");
      }

      // Log the content body
      var content = await request.Content.ReadAsStringAsync();
      Debug.Log($"Request Content: {content}");
    }
  }
}
