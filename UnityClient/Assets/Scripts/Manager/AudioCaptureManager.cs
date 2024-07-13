using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using BriLib;

public class AudioCaptureManager : Singleton<AudioCaptureManager>
{
  private AudioClip audioClip;
  private string microphoneDevice;
  private bool isRecording = false;
  private List<float> audioData = new List<float>();
  private int lastSamplePosition = 0;
  private int sampleRate = 44100;

  public void StartAudioCapture()
  {
    Debug.Log("Starting audio capture");
    if (isRecording) return;

    microphoneDevice = Microphone.devices[0];
    audioClip = Microphone.Start(microphoneDevice, true, 1, sampleRate); // Use a standard sample rate like 44.1 kHz
    isRecording = true;
    lastSamplePosition = 0;
    StartCoroutine(CaptureAudioData());
  }

  public void EndAudioCapture()
  {
    Debug.Log("Ending audio capture");
    if (!isRecording) return;

    Microphone.End(microphoneDevice);
    isRecording = false;
  }

  private IEnumerator CaptureAudioData()
  {
    while (isRecording)
    {
      if (Microphone.IsRecording(microphoneDevice))
      {
        int currentPosition = Microphone.GetPosition(microphoneDevice);
        int sampleCount = (currentPosition - lastSamplePosition + audioClip.samples) % audioClip.samples;

        if (sampleCount > 0)
        {
          float[] samples = new float[sampleCount * audioClip.channels];
          audioClip.GetData(samples, lastSamplePosition);
          audioData.AddRange(samples);
          lastSamplePosition = currentPosition;
        }
      }
      yield return null;
    }
  }

  public async Task<byte[]> GetNextAudioData()
  {
    if (isRecording)
    {
      Debug.LogError("Audio capture is still in progress. Please stop the capture before getting audio data.");
      return null;
    }

    float[] samples = audioData.ToArray();
    int channels = audioClip.channels;
    int frequency = sampleRate;
    byte[] wavData = await Task.Run(() => EncodeToWAV(samples, channels, frequency));
    audioData.Clear();
    return wavData;
  }

  private byte[] EncodeToWAV(float[] samples, int channels, int sampleRate)
  {
    int sampleCount = samples.Length;
    int byteCount = sampleCount * sizeof(short);
    using (MemoryStream memoryStream = new MemoryStream())
    {
      WriteWAVHeader(memoryStream, channels, sampleRate, byteCount);

      short[] intData = new short[sampleCount];
      byte[] bytesData = new byte[byteCount];
      float rescaleFactor = 32767; // to convert float to Int16

      for (int i = 0; i < sampleCount; i++)
      {
        intData[i] = (short)(samples[i] * rescaleFactor);
        byte[] byteArr = BitConverter.GetBytes(intData[i]);
        byteArr.CopyTo(bytesData, i * sizeof(short));
      }

      memoryStream.Write(bytesData, 0, bytesData.Length);
      return memoryStream.ToArray();
    }
  }

  private void WriteWAVHeader(Stream stream, int channels, int sampleRate, int byteCount)
  {
    int blockAlign = channels * sizeof(short);
    int bytesPerSecond = sampleRate * blockAlign;

    using (BinaryWriter writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
    {
      writer.Write("RIFF".ToCharArray());
      writer.Write(36 + byteCount);
      writer.Write("WAVE".ToCharArray());
      writer.Write("fmt ".ToCharArray());
      writer.Write(16);
      writer.Write((short)1);
      writer.Write((short)channels);
      writer.Write(sampleRate);
      writer.Write(bytesPerSecond);
      writer.Write((short)blockAlign);
      writer.Write((short)16);
      writer.Write("data".ToCharArray());
      writer.Write(byteCount);
      writer.Flush();
    }
  }
}
