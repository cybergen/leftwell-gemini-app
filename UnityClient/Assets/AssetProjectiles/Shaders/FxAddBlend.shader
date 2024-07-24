Shader "Custom/AddBlend"
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white" {}
        _Intensity("Intensity", float) = 1
    }

    SubShader
    {
        Tags {
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }
        Blend One OneMinusSrcAlpha
        Cull Off Lighting Off Fog { Color(0, 0, 0, 0) }
        ZWrite Off

        CGINCLUDE
        #include "UnityCG.cginc"
        sampler2D _MainTex;
        float4 _MainTex_ST;
        float _Intensity;

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            fixed4 color : COLOR;
        };

        v2f vert(appdata_full v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.texcoord.xy, _MainTex);
            o.color = v.color;
            return o;
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            float4 frag(v2f i) : SV_Target
            {
                float4 tex = tex2D(_MainTex, i.uv);
                float3 col = tex.rgb * tex.a * i.color.rgb * i.color.a * _Intensity;
                return float4(col, tex.a * 0.5 * i.color.a);
            }
            ENDCG
        }
    }
}

