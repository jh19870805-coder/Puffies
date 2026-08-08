Shader "UI/BlurGrab"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,0.6)
        _BlurSize ("Blur Strength", Range(0,10)) = 2

        _VignetteColor ("Darken Color", Color) = (0,0,0,1)
        _VignetteIntensity ("Darken Opacity", Range(0,1)) = 0.5
        _VignetteRange ("Darken Range", Range(0,1)) = 0.5

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        GrabPass { "_BackgroundTex" } // 抓取当前屏幕画面

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
            };

            sampler2D _BackgroundTex;
            float4 _BackgroundTex_ST;
            float _BlurSize;
            fixed4 _Color;

            fixed4 _VignetteColor;
            float _VignetteIntensity;
            float _VignetteRange;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color * _Color;
                o.uv = TRANSFORM_TEX(v.uv, _BackgroundTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 pixel = 1.0 / _ScreenParams.xy * _BlurSize;
                // 四向采样模糊
                fixed4 col = tex2D(_BackgroundTex, i.uv + float2(-pixel.x, -pixel.y)) * 0.15;
                col += tex2D(_BackgroundTex, i.uv + float2( pixel.x, -pixel.y)) * 0.15;
                col += tex2D(_BackgroundTex, i.uv + float2(-pixel.x,  pixel.y)) * 0.15;
                col += tex2D(_BackgroundTex, i.uv + float2( pixel.x,  pixel.y)) * 0.15;
                col += tex2D(_BackgroundTex, i.uv) * 0.4;

                // 边角压暗（暗角效果）
                float2 centered = i.uv * 2.0 - 1.0;
                float dist = length(centered);
                float maxDist = 1.41421356;
                float inner = maxDist * (1.0 - _VignetteRange);
                float v = smoothstep(inner, maxDist + 0.0001, dist) * _VignetteIntensity;
                col.rgb = lerp(col.rgb, _VignetteColor.rgb, v);

                return col * i.color;
            }
            ENDCG
        }
    }
}
