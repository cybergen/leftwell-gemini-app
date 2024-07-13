using System;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

public class GeneratedTextStreamHandler : DownloadHandlerScript
{
  private Action<string> onTextResponseUpdated;
  private string totalResponse = string.Empty;
  private string actualContent = string.Empty;
  private bool capturing = false;
  private int startIndex = 0;
  private static string contentAntecedentPattern = @"\s*""\s*\n\s*\}\s*\],\s*""role""\s*:\s*""model""";
  private string contentAntecedent = @" ""
          }
        ],
        ""role"": ""model""";

  private Regex startRegex = new Regex(@"\s*""text"":\s*""");
  private Regex endRegex = new Regex(contentAntecedentPattern);

  protected override byte[] GetData()
  {
    return null;
  }

  public GeneratedTextStreamHandler(Action<string> onTextResponseUpdated) : base(new byte[4096])
  {
    this.onTextResponseUpdated = onTextResponseUpdated;
  }

  protected override bool ReceiveData(byte[] data, int dataLength)
  {
    Debug.LogWarning($"Receive data was called with data length {dataLength}");

    if (data == null || data.Length < 1)
    {
      return false;
    }

    totalResponse += Encoding.UTF8.GetString(data, 0, dataLength);
    
    if (!capturing)
    {
      Match match = startRegex.Match(totalResponse);
      if (match.Success)
      {
        capturing = true;
        startIndex = match.Index + match.Length;
        actualContent = totalResponse.Substring(startIndex);
      }
    }

    // This block should still be run in the same pass as the prior one in case we completed
    // the entire download at once
    if (capturing)
    {
      if (totalResponse.Length > (startIndex + contentAntecedent.Length))
      {
        Match match = endRegex.Match(totalResponse);
        if (match.Success)
        {
          int endIndex = match.Index;
          actualContent = totalResponse.Substring(startIndex, endIndex - startIndex);
          capturing = false;
          string processedContent = actualContent.Replace("\\\"", "\"")
                                      .Replace("\\n", "\n");
          onTextResponseUpdated(processedContent);
        }
        else
        {
          int amountToTake = totalResponse.Length - startIndex;
          actualContent = totalResponse.Substring(startIndex, amountToTake);
          string processedContent = actualContent.Replace("\\\"", "\"")
                                      .Replace("\\n", "\n");
          onTextResponseUpdated(processedContent);
        }
      }
    }

    return true;
  }

  protected override void CompleteContent()
  {
    Debug.Log("Download complete");
  }
}