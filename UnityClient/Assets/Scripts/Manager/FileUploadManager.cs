using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;
using BriLib;
using LLM.Network;

public class FileUploadManager : Singleton<FileUploadManager>
{
  private readonly HttpClient _client = new HttpClient();

  public async Task<FilePayload> UploadFile(string mimeType, string displayName, byte[] bytes)
  {
    var key = Config.Instance.ApiKey;
    var uploadUrl = $"https://generativelanguage.googleapis.com/upload/v1beta/files?key={key}&alt=json&uploadType=multipart";

    _client.DefaultRequestHeaders.Clear();
    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    _client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate");
    _client.DefaultRequestHeaders.Add("User-Agent", "Python-httplib2/0.22.0 (gzip)");
    _client.DefaultRequestHeaders.Add("x-goog-api-client", "gdcl/2.137.0 gl-python/3.11.2");

    var boundary = "===============3790691624723926432==";

    using (var content = new MultipartContent("related", boundary))
    {
      // JSON metadata part
      var jsonContent = new StringContent($"{{\"file\": {{\"displayName\": \"{displayName}\"}}}}");
      jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
      jsonContent.Headers.Add("MIME-Version", "1.0");
      content.Add(jsonContent);

      // File content part
      var fileContent = new ByteArrayContent(bytes);
      fileContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
      fileContent.Headers.Add("MIME-Version", "1.0");
      fileContent.Headers.Add("Content-Transfer-Encoding", "binary");
      content.Add(fileContent);

      var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl)
      {
        Content = content
      };

      HttpResponseMessage response = await _client.SendAsync(request);

      //Debug.Log($"Got response {response} with status {response.StatusCode}");

      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      //Debug.Log("Upload Response:");
      //Debug.Log(responseBody);
      return FilePayload.FromJSON(responseBody);
    }
  }

  public async Task<string> GetDiscoveryDocument()
  {
    var key = Config.Instance.ApiKey;
    var discoveryUrl = $"https://generativelanguage.googleapis.com/$discovery/rest?version=v1beta&key={key}";

    _client.DefaultRequestHeaders.Clear();
    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    HttpResponseMessage response = await _client.GetAsync(discoveryUrl);

    //Debug.Log($"Response from discover document {response}");

    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
  }
}
