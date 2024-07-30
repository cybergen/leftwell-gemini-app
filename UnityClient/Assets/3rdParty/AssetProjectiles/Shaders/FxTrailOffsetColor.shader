Shader "Custom/FxTrailOffsetColor"
{
    Properties
    {
        _Texture("Texture", 2D) = "black" {}
        _Speed_x("Speed X", Float) = 0
        _AddBlend("Add = 0, Blend = 1", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags
        {
            "IgnoreProjector" = "True"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off ZWrite Off Lighting Off

        CGINCLUDE
        #include "UnityCG.cginc"

        sampler2D _Texture;
        float4 _Texture_ST;
        float _Speed_x;
        float _AddBlend;

        struct appdata
        {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            float4 color : COLOR;
        };

        struct v2f
        {
            float4 pos : SV_POSITION;
            float2 uv : TEXCOORD0;
            float4 color : COLOR;
        };

        v2f vert(appdata v)
        {
            v2f o;
            o.pos = UnityObjectToClipPos(v.vertex);
            o.color = v.color;
            o.uv = TRANSFORM_TEX(v.texcoord, _Texture) + _Time.r * float2(_Speed_x * 5, 0);
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
                float4 tex = tex2D(_Texture, i.uv);
                float3 col = tex.rgb * i.color.rgb;
                return float4(col, tex.a * i.color.a);
            }
            ENDCG
        }
    }
}

