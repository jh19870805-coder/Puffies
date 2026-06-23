// URP的特效通用Shader

Shader "URP/Effect/UPR_FX_Common"
{
    Properties
    {
        [HideInInspector][Enum(Off, 0, On, 1)]_ZWriteMode ("ZWriteMode", float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode ("ZTestMode", Float) = 4
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [HideInInspector][Enum(Multiply, 0, Add, 1)]_BlendMode ("Blend Mode", float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 5
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 10
        [HideInInspector]_DepthOffset ("Depth Offset", Range(0, 1)) = 0.0

        [HideInInspector][HDR]_MainColor ("Main Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_MainTex ("Main Texture", 2D) = "white" { }
        [HideInInspector][Toggle(_WHITEISALPHA)]_WhiteIsAlpha ("Use White To Alpha", float) = 0
        [HideInInspector][Toggle(_SMOOTHVERTEXALPHA)]_SmoothVertexAlpha ("Smooth Vertex Alpha", float) = 0

        [HideInInspector][Enum(UV1, 0, UV2, 1)]_LayersUV ("Layers UV", float) = 0
        [HideInInspector][KeywordEnum(Off, Add, Multiply)]_MaskLayerActive ("Mask Layer Active", float) = 0
        [HideInInspector][HDR]_MaskLayerColor ("Mask Layer Color", Color) = (1, 1, 1, 1)
        [HideInInspector]_MaskTex ("Mask Texture", 2D) = "white" { }
        [HideInInspector]_UVAniSpeed ("UV Ani Speed", vector) = (0, 0, 0, 0)

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
        
        [HideInInspector][Toggle(_GPUSKELETONACTIVE)]_GPUSkeletonActive ("GPUSkeleton Active", float) = 0
        [HideInInspector]_GPUSkeletonTex ("GPUSkeleton Texture", 2D) = "white" { }
        [HideInInspector]_GPUSkeletonParam ("Param", vector) = (30, 0, 0, 0)
        [HideInInspector]_GPUSkeletonFrameParam ("FrameParam", vector) = (0, 0, 0, 0)
        
        [HideInInspector][Toggle(_SEQUENCEACTIVE)]_Sequence ("Enable Sequence", float) = 0
        [HideInInspector]_SequenceFrameSpeed ("Frame Speed", float) = 0

        [HideInInspector][Toggle(_VERTEXANIMATION_NOISE)]_VertexAnimationNoise ("Vertex Animation Noise", float) = 0
        [HideInInspector]_NoiseStrength ("Noise Strength", Range(0, 10)) = 0.1
        [HideInInspector]_NoiseSpeed ("Noise Speed", Range(0, 100)) = 1
        [HideInInspector]_NoiseAniOffset ("Noise Ani Offset", Range(-1, 1)) = 0

        [HideInInspector][Toggle(_CUSTOMDATA1_DISSOLVEFACTOR)]_CustomData1_DissolveFactor ("CustomData1_DissolveFactor", float) = 0
        [HideInInspector][Toggle(_CUSTOMDATA2_DISTORTINTENSITY)]_CustomData2_DistortIntensity ("CustomData2_DistortIntensity", float) = 0
        [HideInInspector][Toggle(_CUSTOMDATA3_MAINTEXUVOFFSET)]_CustomData3_MainTexUVOffset ("CustomData3_MainTexUVOffset", float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

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
            #pragma shader_feature_local _GPUSKELETONACTIVE
            #pragma shader_feature_local _SEQUENCEACTIVE
            #pragma shader_feature_local _CUSTOMDATA1_DISSOLVEFACTOR
            #pragma shader_feature_local _CUSTOMDATA2_DISTORTINTENSITY
            #pragma shader_feature_local _CUSTOMDATA3_MAINTEXUVOFFSET
            #pragma shader_feature_local _VERTEXANIMATION_NOISE
            #pragma shader_feature_local _SMOOTHVERTEXALPHA

            #pragma multi_compile_instancing

            #pragma vertex vert
            #pragma fragment frag

            #include "URP_FXFunction.hlsl"

            ENDHLSL
        }
    }
    CustomEditor "EGG_ShaderGUI.Egg_URP_FXCommon_ShaderGUI"
}
