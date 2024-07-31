using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BriLib;
using LLM.Network;

public class FileUploadManager : Singleton<FileUploadManager>
{
  private readonly HttpClient _client = new HttpClient();

  public async Task<FilePayload> UploadFile(string mimeType, string displayName, byte[] bytes)
  {
    var uploadUrl = NetworkSettings.PROXY_URL_BASE + "api/file-upload/upload/v1beta/files?alt=json&uploadType=multipart";
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

      response.EnsureSuccessStatusCode();
      string responseBody = await response.Content.ReadAsStringAsync();
      return FilePayload.FromJSON(responseBody);
    }
  }

  public async Task<string> GetDiscoveryDocument()
  {
    var discoveryUrl = $"http://localhost:3000/api/file-upload/$discovery/rest?version=v1beta";

    _client.DefaultRequestHeaders.Clear();
    _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    HttpResponseMessage response = await _client.GetAsync(discoveryUrl);

    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
  }
}
