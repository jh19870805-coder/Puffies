Shader "Puffies/UI/PackCoverShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Cover Tint", Color) = (1,1,1,1)
        _ShadowColor ("Shadow Color / Opacity", Color) = (0.078,0.098,0.106,0.7)
        _ShadowOffsetX ("Shadow Offset X (Pixels)", Range(-200, 200)) = 0
        _ShadowOffsetY ("Shadow Offset Y (Pixels)", Range(-200, 200)) = -55
        _ShadowBlurX ("Shadow Blur X (Pixels)", Range(0, 80)) = 10
        _ShadowBlurY ("Shadow Blur Y (Pixels)", Range(0, 120)) = 32
        _ShadowSpread ("Shadow Spread (Pixels)", Range(0, 50)) = 8
        _PaddingX ("Render Padding X (Pixels)", Range(0, 200)) = 40
        _PaddingY ("Render Padding Y (Pixels)", Range(0, 240)) = 140
        [HideInInspector] _SpritePixelsPerUnit ("Sprite Pixels Per Unit", Float) = 100
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "False"
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

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            #pragma multi_compile_local _ PACK_SHADOW_SPRITE_RENDERER

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 contentUv : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _TextureSampleAdd;
            fixed4 _Color;
            fixed4 _ShadowColor;
            float _ShadowOffsetX;
            float _ShadowOffsetY;
            float _ShadowBlurX;
            float _ShadowBlurY;
            float _ShadowSpread;
            float _PaddingX;
            float _PaddingY;
            float _SpritePixelsPerUnit;
            float4 _ClipRect;

            fixed IsInsideSprite(float2 uv)
            {
                return step(0.0, uv.x)
                    * step(uv.x, 1.0)
                    * step(0.0, uv.y)
                    * step(uv.y, 1.0);
            }

            fixed SampleSpriteAlpha(float2 uv)
            {
                fixed inside = IsInsideSprite(uv);
                return (tex2D(_MainTex, saturate(uv)) + _TextureSampleAdd).a * inside;
            }

            fixed SampleShadowAlpha(float2 uv)
            {
                float2 radius =
                    (float2(_ShadowBlurX, _ShadowBlurY) + _ShadowSpread)
                    * _MainTex_TexelSize.xy;
                fixed alpha = SampleSpriteAlpha(uv) * 0.20;
                alpha += SampleSpriteAlpha(uv + float2(radius.x, 0.0)) * 0.12;
                alpha += SampleSpriteAlpha(uv - float2(radius.x, 0.0)) * 0.12;
                alpha += SampleSpriteAlpha(uv + float2(0.0, radius.y)) * 0.12;
                alpha += SampleSpriteAlpha(uv - float2(0.0, radius.y)) * 0.12;
                alpha += SampleSpriteAlpha(uv + radius) * 0.08;
                alpha += SampleSpriteAlpha(uv - radius) * 0.08;
                alpha += SampleSpriteAlpha(uv + float2(radius.x, -radius.y)) * 0.08;
                alpha += SampleSpriteAlpha(uv + float2(-radius.x, radius.y)) * 0.08;
                return saturate(alpha);
            }

            v2f vert(appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                float2 paddingRatio = float2(_PaddingX, _PaddingY) * _MainTex_TexelSize.xy;
                float2 expansion = 1.0 + paddingRatio * 2.0;
                output.localPosition = input.vertex;

                #ifdef PACK_SHADOW_SPRITE_RENDERER
                float2 cornerDirection = step(0.5, input.texcoord) * 2.0 - 1.0;
                output.localPosition.xy += cornerDirection
                    * float2(_PaddingX, _PaddingY)
                    / max(_SpritePixelsPerUnit, 1.0);
                #endif

                output.vertex = UnityObjectToClipPos(output.localPosition);
                output.contentUv = input.texcoord * expansion - paddingRatio;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed inside = IsInsideSprite(input.contentUv);
                fixed4 coverTexture =
                    (tex2D(_MainTex, saturate(input.contentUv)) + _TextureSampleAdd) * inside;
                fixed4 cover = coverTexture * input.color;

                float2 shadowOffset =
                    float2(_ShadowOffsetX, _ShadowOffsetY) * _MainTex_TexelSize.xy;
                fixed shadowAlpha = SampleShadowAlpha(input.contentUv - shadowOffset)
                    * _ShadowColor.a
                    * input.color.a;
                shadowAlpha *= 1.0 - cover.a;

                fixed outputAlpha = cover.a + shadowAlpha;
                fixed3 premultipliedColor =
                    cover.rgb * cover.a + _ShadowColor.rgb * shadowAlpha;
                fixed3 outputColor = premultipliedColor / max(outputAlpha, 0.0001);
                fixed4 color = fixed4(outputColor, outputAlpha);

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(input.localPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
