using System;

[Serializable]
public class ElevenLabsRequestBody
{
  public string text;
  public string model_id;
  public VoiceSettings voice_settings;
}

[Serializable]
public class VoiceSettings
{
  public float stability;
  public float similarity_boost;
  public float style;
  public bool use_speaker_boost;
}