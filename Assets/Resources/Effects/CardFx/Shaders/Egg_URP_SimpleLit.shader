Shader "URP/Common/Egg_URP_SimpleLit"
{
    Properties
    {
        [HideInInspector][Enum(Opaque, 0, Transparent, 1)] _SurfaceType ("Surface Type", Float) = 0
        [HideInInspector][Toggle(_ALPHATEST_ON)]_AlphaClip ("Alpha Clipping", float) = 0
        [HideInInspector][Toggle(VERTEX_ALPHA)]_VertexAlpha ("Vertex Alpha", float) = 0
        [HideInInspector]_Cutoff ("Clip", Range(0, 1)) = 0.5
        //[HideInInspector][Enum(UnityEngine.Rendering.BlendOp)]  _BlendOp ("BlendOp", Float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
        [HideInInspector][Enum(Off, 0, On, 1)]_ZWriteMode ("ZWriteMode", float) = 1
        [HideInInspector][Enum(UnityEngine.Rendering.CullMode)]_CullMode ("CullMode", float) = 2
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode ("ZTestMode", Float) = 4
        [HideInInspector]_StencilRefValue ("Stencil Value", float) = 0
        [HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)]_StencilComp ("Stencil Comp", Float) = 8
        [HideInInspector][Enum(UnityEngine.Rendering.StencilOp)]_StencilOp ("Stencil Pass Operation", Float) = 0
        [HideInInspector]_DepthOffset ("Depth Offset", Range(0, 1)) = 0.0
        [HideInInspector]_PanelShadowHeight ("Panel Shadow Height", float) = 0
        [HideInInspector]_PanelShadowColor ("Panel Shadow Color", Color) = (0, 0.035, 0.13, 0.58)
        
        [HideInInspector][HDR]_MainColor ("Main Color", Color) = (1, 1, 1, 1)
        [HideInInspector][Toggle] _MaskColorOn ("遮罩颜色开关", float) = 0
        [HideInInspector]_MaskColor ("遮罩颜色", Color) = (1, 1, 1, 1)
        [HideInInspector]_AlbedoTex ("Main Texture", 2D) = "white" { }
        [HideInInspector]_NormalTex ("Normal Texture", 2D) = "bump" { }
        [HideInInspector]_NormalScale ("Normal Scale", Range(0, 3)) = 1
        [HideInInspector]_MetallicTex ("Metallic Map", 2D) = "white" { }
        [HideInInspector]_Occlusion ("Occlusion", Range(0, 1)) = 1
        [HideInInspector]_SelfShadow ("Self Shadow", Range(0, 1)) = 1
        [HideInInspector][Toggle(_EMISSIONACTIVE)]_EmissionActive ("Emission Active", float) = 0
        [HideInInspector][HDR]_EmissionColor ("Emission Color", Color) = (1, 1, 1, 1)
        //[HideInInspector]_EmissionTex ("Emission Map", 2D) = "white" {}

        [HideInInspector][Toggle(INSTANCE_PERCOLOR_ENABLED)]_InstancePerColor ("Instance Per Color", float) = 0
        [HideInInspector][ToggleOff]_Receive_Shadows ("Receive Shadow", float) = 1
        [HideInInspector][Toggle(LIGHTMAP_ON)]_Lightmap ("Enable Lightmap", float) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardRender"
            Tags { "LightMode" = "UniversalForward" }

            // Stencil
            // {
            //     Ref [_StencilRefValue]
            //     Comp [_StencilComp]
            //     Pass [_StencilOp]
            //     Fail keep
            //     ZFail keep
            // }

            Blend [_SrcBlend] [_DstBlend]
            Cull [_CullMode]
            ZWrite [_ZWriteMode]
            ZTest [_ZTestMode]

            HLSLPROGRAM
            #pragma target 4.5

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            //#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            // #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            // #pragma multi_compile _ SHADOWS_SHADOWMASK

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #pragma shader_feature_local INSTANCE_PERCOLOR_ENABLED
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local LIGHTMAP_ON
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF
            #pragma shader_feature_local VERTEX_ALPHA

            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _MaskColorOn
            #pragma shader_feature_local_fragment _EMISSIONACTIVE
            #pragma shader_feature_local_fragment _METALLICMAP

            #pragma vertex vert_Lit
            #pragma fragment frag_Lit

            #include "Egg_URP_SimpleFunction.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_CullMode]

            HLSLPROGRAM
            #pragma target 4.5

            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #pragma shader_feature_local_fragment _ALPHATEST_ON

            #pragma vertex vert_shadow
            #pragma fragment frag_shadow

            #include "Egg_URP_SimpleFunction.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "PanelShadow"
            Tags { "LightMode" = "PanelShadow" }
            
            //用使用模板测试以保证alpha显示正确
            Stencil
            {
                Ref 0
                Comp equal
                Pass incrWrap
                Fail keep
                ZFail keep
            }
            
            // 透明混合模式
            Blend SrcAlpha OneMinusSrcAlpha

            // 关闭深度写入
            // ZWrite off

            // 深度稍微偏移防止阴影与地面穿插
            Offset -4, 0

            HLSLPROGRAM
            //#pragma target 4.5

            #pragma multi_compile_instancing

            #pragma vertex vert_panel_shadow
            #pragma fragment frag_panel_shadow

            #include "Egg_URP_SimpleFunction.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma target 2.0

            #pragma multi_compile_instancing
            // #pragma instancing_options assumeuniformscaling nomatrices nolightprobe nolightmap

            #pragma vertex vert_depthOnly
            #pragma fragment frag_depthOnly


            #include "Egg_URP_SimpleFunction.hlsl"

            ENDHLSL
        }
    }
    Fallback "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "EGG_ShaderGUI.Egg_URP_SimpleLit_ShaderGUI"
}
