Shader "Puffies/CardPackOpening"
{
    Properties
    {
        [MainTexture] _FrontFacesAlbedo("Front Face", 2D) = "white" {}
        [MainColor] _FrontFacesColor("Front Tint", Color) = (1, 1, 1, 1)
        _BackFacesAlbedo("Back Face", 2D) = "white" {}
        _BackFacesColor("Back Tint", Color) = (1, 1, 1, 1)
        _ClipTex("Clip Mask", 2D) = "white" {}
        _Cutoff("Alpha Cutoff", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
        }

        Pass
        {
            Name "CardPackForward"
            Tags { "LightMode" = "SRPDefaultUnlit" }

            Cull Off
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_FrontFacesAlbedo);
            SAMPLER(sampler_FrontFacesAlbedo);
            TEXTURE2D(_BackFacesAlbedo);
            SAMPLER(sampler_BackFacesAlbedo);
            TEXTURE2D(_ClipTex);
            SAMPLER(sampler_ClipTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _FrontFacesAlbedo_ST;
                float4 _BackFacesAlbedo_ST;
                float4 _ClipTex_ST;
                half4 _FrontFacesColor;
                half4 _BackFacesColor;
                half _Cutoff;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                float2 maskUv = input.uv * _ClipTex_ST.xy + _ClipTex_ST.zw;
                half mask = SAMPLE_TEXTURE2D(_ClipTex, sampler_ClipTex, maskUv).r;
                clip(mask - _Cutoff);

                float2 frontUv = input.uv * _FrontFacesAlbedo_ST.xy + _FrontFacesAlbedo_ST.zw;
                float2 backUv = input.uv * _BackFacesAlbedo_ST.xy + _BackFacesAlbedo_ST.zw;
                half4 frontColor = SAMPLE_TEXTURE2D(
                    _FrontFacesAlbedo,
                    sampler_FrontFacesAlbedo,
                    frontUv) * _FrontFacesColor;
                half4 backColor = SAMPLE_TEXTURE2D(
                    _BackFacesAlbedo,
                    sampler_BackFacesAlbedo,
                    backUv) * _BackFacesColor;

                return IS_FRONT_VFACE(isFrontFace, frontColor, backColor);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
