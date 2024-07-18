Shader "Custom/Invisible"
{
  SubShader
  {
    Tags { "Queue" = "Overlay" }
    Pass
    {
      // Do not render anything
      ColorMask 0

      // Disable depth writing
      ZWrite Off

      // Disable depth test
      ZTest Always

      // No blending needed
      Blend Off
    }
  }
  // Fallback needed for legacy systems
  Fallback Off
}