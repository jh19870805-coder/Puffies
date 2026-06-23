// URP的UI特效通用Shader

Shader "URP/Effect/URP_UI_FX_Common"
{
    Properties
    {
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        
        [HideInInspector][Enum(Off, 0, On, 1)]_ZWriteMode ("ZWriteMode", float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode ("ZTestMode", Float) = 4
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [HideInInspector][Enum(Multiply, 0, Add, 1)]_BlendMode ("Blend Mode", float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 5
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 10
        [HideInInspector]_DepthOffset ("Depth Offset", Range(0, 1)) = 0.0

        [HideInInspector][HDR]_MainColor ("Main Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_MainTex ("Main Texture", 2D) = "white" { }
        [HideInInspector][Toggle(_WHITEISALPHA)]_WhiteIsAlpha ("Use White To Alpha", float) = 0

        [HideInInspector][Enum(UV1, 0, UV2, 1)]_LayersUV ("Layers UV", float) = 0
        [HideInInspector][KeywordEnum(Off, Add, Multiply)]_MaskLayerActive ("Mask Layer Active", float) = 0
        [HideInInspector][HDR]_MaskLayerColor ("Mask Layer Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_MaskTex ("Mask Texture", 2D) = "white" { }
        [HideInInspector]_UVAniSpeed ("UV Ani Speed", vector) = (0, 0, 0, 0)
        // [HideInInspector]_MaskUVScale ("Mask UV Scale", Range(0, 100)) = 1

        [HideInInspector][Toggle(_RIMACTIVE)]_RimActive ("Rim Active", float) = 0
        [HideInInspector][HDR]_RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_RimFade ("Rim Fade", Range(0, 10)) = 1

        [HideInInspector][Toggle(_DISSOLVEACTIVE)]_DissolveActive ("Dissolve Active", float) = 0
        [HideInInspector]_DissolveTex ("Dissolve Texture", 2D) = "white" { }
        [HideInInspector]_DissolveFactor ("Dissolve Factor", Range(0, 1)) = 0
        [HideInInspector]_DissolveWidth ("Dissolve Width", Range(0, 10)) = 0.1
        [HideInInspector][HDR]_DissolveEdgeCol ("Dissolve Edge Color", Color) = (1, 1, 1, 1)

        [HideInInspector][Toggle(_DISTORTACTIVE)]_DistortActive ("Distort Active", float) = 0
        [HideInInspector]_DistortTex ("Distort Texture", 2D) = "white" { }
        [HideInInspector]_DistortIntensity ("Distort Intensity", Range(0, 10)) = 1
        [HideInInspector]_DissolveAndDistortSpeed ("Dissolve & Distort Speed", vector) = (0, 0, 0, 0)

        [HideInInspector][Toggle(_CUSTOMDATA1_DISSOLVEFACTOR)]_CustomData1_DissolveFactor ("CustomData1_DissolveFactor", float) = 0
        [HideInInspector][Toggle(_CUSTOMDATA2_DISTORTINTENSITY)]_CustomData2_DistortIntensity ("CustomData2_DistortIntensity", float) = 0
        [HideInInspector][Toggle(_CUSTOMDATA3_MAINTEXUVOFFSET)]_CustomData3_MainTexUVOffset ("CustomData3_MainTexUVOffset", float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }
            ZWrite [_ZWriteMode]
            ZTest [_ZTestMode]
            Cull [_CullMode]
            Blend [_SrcBlend] [_DstBlend]

            HLSLPROGRAM
            
            #pragma shader_feature_local _WHITEISALPHA
            #pragma shader_feature_local _MASKLAYERACTIVE_OFF _MASKLAYERACTIVE_ADD _MASKLAYERACTIVE_MULTIPLY
            #pragma shader_feature_local _LAYERSUV1
            #pragma shader_feature_local _RIMACTIVE
            #pragma shader_feature_local _DISSOLVEACTIVE
            #pragma shader_feature_local _DISTORTACTIVE
            #pragma shader_feature_local _CUSTOMDATA1_DISSOLVEFACTOR
            #pragma shader_feature_local _CUSTOMDATA2_DISTORTINTENSITY
            #pragma shader_feature_local _CUSTOMDATA3_MAINTEXUVOFFSET

            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #include "URP_FXFunction.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "EGG_ShaderGUI.Egg_URP_UI_FXCommon_ShaderGUI"
}
