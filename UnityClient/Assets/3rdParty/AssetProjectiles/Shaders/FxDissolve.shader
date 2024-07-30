Shader "Custom/FxDissolve" 
{
    Properties 
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _AddBlend ("Additive = 0, Alpha Blend = 1", Range(0, 1)) = 1
    }

    SubShader 
    {

        Tags 
        {
            "IgnoreProjector"="True"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass 
        {
            Blend One OneMinusSrcAlpha
            ColorMask RGB
            Cull Off Lighting Off ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc" 

            sampler2D _MainTex; 
            float4 _MainTex_ST;
            float _AddBlend;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
                float4 texcoord1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 vertexColor : COLOR;
            };

            v2f vert (appdata v) 
            {
                v2f o;
                o.uv0 = v.texcoord0;
                o.uv1 = v.texcoord1;
                o.vertexColor = v.vertexColor;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float4 mainTex = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
                float dissolveFactor = mainTex.g - (((mainTex.r * 2.8 - 1.2) + 1.0 + saturate(i.uv1.r * 2.2 + 0.1)) * i.uv1.b);
                clip(saturate(dissolveFactor) - 0.5);
                
                float opacity = mainTex.a * saturate(dissolveFactor * 2.0 - 1.0) * i.vertexColor.a;
                float3 col = mainTex.r * opacity * i.vertexColor.rgb;
                
                return float4(col, opacity * _AddBlend);
            }
            ENDCG
        }
    }
}

