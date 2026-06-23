#ifndef URP_FXINPUT_INCLUDE
#define URP_FXINPUT_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
#ifndef _MASKLAYERACTIVE_OFF
    TEXTURE2D(_MaskTex); SAMPLER(sampler_MaskTex);
#endif
#if defined(_DISSOLVEACTIVE)
    TEXTURE2D(_DissolveTex); SAMPLER(sampler_DissolveTex);
#endif
#if defined(_DISTORTACTIVE)
    //TEXTURE2D(_CameraOpaqueTexture); SAMPLER(sampler_CameraOpaqueTexture);
    //TEXTURE2D(_CameraColorTexture); SAMPLER(sampler_CameraColorTexture);
    TEXTURE2D(_DistortTex); SAMPLER(sampler_DistortTex);
#endif
#if defined(_GPUSKELETONACTIVE)
    TEXTURE2D(_GPUSkeletonTex); SAMPLER(sampler_GPUSkeletonTex);
#endif

CBUFFER_START(UnityPerMaterial)
    half4 _MainColor;
    half4 _MainTex_ST;
    half4 _MaskTex_ST;
    //half _MaskLayerUV;
    half4 _MaskLayerColor;
    half4 _UVAniSpeed;
    half4 _RimColor;
    half _RimFade;
    half4 _DissolveTex_ST;
    half _DissolveFactor;
    half _DissolveWidth;
    half4 _DissolveEdgeCol;
    half4 _DistortTex_ST;
    half _DistortIntensity;
    half4 _DissolveAndDistortSpeed;
    half4 _GPUSkeletonTex_TexelSize;
    half4 _GPUSkeletonParam;
    half _SequenceFrameSpeed;
    half _DepthOffset;
    half _NoiseStrength;
    half _NoiseSpeed;
    half _NoiseAniOffset;
CBUFFER_END

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_DEFINE_INSTANCED_PROP(half4, _GPUSkeletonFrameParam)
UNITY_INSTANCING_BUFFER_END(Props)

#endif