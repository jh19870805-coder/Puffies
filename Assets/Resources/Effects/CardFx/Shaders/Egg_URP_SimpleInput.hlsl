#ifndef EGG_URP_SIMPLEINPUT_INCLUDE
#define EGG_URP_SIMPLEINPUT_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


TEXTURE2D(_AlbedoTex); SAMPLER(sampler_AlbedoTex);

#if defined(_NORMALMAP)
    TEXTURE2D(_NormalTex); SAMPLER(sampler_NormalTex);
#endif
#if defined(_METALLICMAP)
    TEXTURE2D(_MetallicTex); SAMPLER(sampler_MetallicTex);
#endif

#if defined(UNUSE_SRPBATCH)
    half4 _MainColor;
    //half4 _AlbedoTex_ST;
    //half4 _NormalTex_ST;
    half _NormalScale;
    half _Cutoff;
    half _SelfShadow;
    half4 _EmissionColor;
    half _DepthOffset;
    half _PanelShadowHeight;
    half4 _PanelShadowColor;
    half _Occlusion;
    half4 _MaskColor;

    UNITY_INSTANCING_BUFFER_START(prop)
    UNITY_DEFINE_INSTANCED_PROP(half4, _AlbedoTex_ST)
    UNITY_INSTANCING_BUFFER_END(prop)
    #define AlbedoTex_ST UNITY_ACCESS_INSTANCED_PROP(prop, _AlbedoTex_ST)

#else
    CBUFFER_START(UnityPerMaterial)
        half4 _MainColor;
        half4 _AlbedoTex_ST;
        half4 _NormalTex_ST;
        half _NormalScale;
        half _Cutoff;
        half _SelfShadow;
        half4 _EmissionColor;
        half _DepthOffset;
        half _PanelShadowHeight;
        half4 _PanelShadowColor;
        half _Occlusion;
        half4 _MaskColor;
    CBUFFER_END
#endif

half4 _GlobalShadowColor;

#endif