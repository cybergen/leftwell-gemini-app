using System;
using System.Collections.Generic;
using UnityEngine;

namespace LLM.Network
{
  [Serializable]
  public class LLMTextResponse
  {
    public List<Candidate> candidates;
    public UsageMetadata usageMetadata;

    [Serializable]
    public class Candidate
    {
      public Content content;
      public string finishReason;
      public int index;
      public List<SafetyRating> safetyRatings;

      [Serializable]
      public class Content
      {
        public List<Part> parts;
        public string role;

        [Serializable]
        public class Part
        {
          public string text;
        }
      }

      [Serializable]
      public class SafetyRating
      {
        public string category;
        public string probability;
      }
    }

    [Serializable]
    public class UsageMetadata
    {
      public int promptTokenCount;
      public int candidatesTokenCount;
      public int totalTokenCount;
    }

    public override string ToString()
    {
      return JsonUtility.ToJson(this);
    }

    public static LLMTextResponse FromJson(string json)
    {
      return JsonUtility.FromJson<LLMTextResponse>(json);
    }
  }
}