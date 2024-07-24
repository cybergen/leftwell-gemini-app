Shader "Custom/Checker" 
{
    Properties
    {
        _MainTex ("MainTex", 2D) = "white" {}
        _ColorGrid ("Color Grid", Color) = (1,1,1,1)
        _ColorChecker01 ("Color Checker 01", Color) = (1,1,1,1)
        _ColorChecker02 ("Color Checker 02", Color) = (1,1,1,1)
        _ColorFog ("Color Fog", Color) = (1,1,1,1)
        _IntensityFog ("IntensityFog", Range(0, 1)) = 0
        _OffsetFog ("Offset Fog", Range(0, 1)) = 0
    }

    SubShader 
    {
        Pass 
        {
            Name "FORWARD"
            Tags 
            {
                "LightMode"="ForwardBase"
                "RenderType"="Opaque"
            }            
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"

            float4 _ColorChecker01;
            sampler2D _MainTex; 
            float4 _MainTex_ST;
            float4 _ColorFog;
            float _IntensityFog;
            float _OffsetFog;
            float4 _ColorChecker02;
            float4 _ColorGrid;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                float4 posWorld : TEXCOORD1;
                float4 objPos : TEXCOORD2; 
                LIGHTING_COORDS(3,4) 
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.uv0 = v.texcoord0;
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.objPos = mul(unity_ObjectToWorld, float4(0,0,0,1)); 
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            float4 frag(v2f i) : COLOR 
            {
                float attenuation = LIGHT_ATTENUATION(i);
                float4 MainTex = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
                float2 MainTexChnl = MainTex.rgb.rg;
                float Wpos = (i.posWorld.g * _OffsetFog);
                float OPosDist = distance(i.posWorld.rgb, i.objPos.rgb); 
                float OPos = (_OffsetFog * OPosDist);
                float BlendPos = saturate((Wpos + OPos - 1.0));
                float3 col = (lerp(lerp(lerp(_ColorGrid.rgb, _ColorChecker01.rgb, MainTexChnl.r), _ColorChecker02.rgb, MainTexChnl.g), _ColorFog.rgb, (BlendPos * _IntensityFog)) * (attenuation * _LightColor0.a));
                return fixed4(col, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}


