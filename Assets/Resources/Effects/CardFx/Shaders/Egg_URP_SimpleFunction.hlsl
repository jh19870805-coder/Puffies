#ifndef EGG_URP_SIMPLEFUNCTION_INCLUDE
#define EGG_URP_SIMPLEFUNCTION_INCLUDE

#ifdef VERTEX_ALPHA
    #define _NEEDVERTEXCOLOR
#endif

#include "Egg_URP_SimpleInput.hlsl"
#include "Egg_URP_CommonStruct.hlsl"

void setup()
{

}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

v2f_Lit vert_Lit(a2v_Lit v, uint instanceID : SV_InstanceID)
{
    v2f_Lit o = (v2f_Lit)0;

    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        o = InitializeVertInputInstance(v, _AlbedoTex_ST, _NormalTex_ST, half4(0, 0, 0, 0), instanceID);
    #elif defined(UNUSE_SRPBATCH)
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);

        o = InitializeVertInput(v, AlbedoTex_ST, AlbedoTex_ST);
    #else
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o);

        o = InitializeVertInput(v, _AlbedoTex_ST, _NormalTex_ST);
    #endif

    #if defined(_NEEDVERTEXCOLOR)
        o.color = v.color;
    #endif

    #if UNITY_REVERSED_Z
        o.positionCS.z += _DepthOffset;
        // o.positionCS.z = min(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        o.positionCS.z -= _DepthOffset;
        // o.positionCS.z = max(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    return o;
}

half4 frag_Lit(v2f_Lit i) : SV_TARGET
{
    half4 albedo = SAMPLE_TEXTURE2D(_AlbedoTex, sampler_AlbedoTex, i.uv.xy);
    half aoMask = 1.0;
    half emissionMask = 1.0;
    #if defined(_METALLICMAP)
        half4 specGloss = SAMPLE_TEXTURE2D(_MetallicTex, sampler_MetallicTex, i.uv.xy);
        specGloss.rgb = saturate((specGloss.rgb - half3(0.1h, 0.1h, 0.1h)) * 1.1);
        aoMask = lerp(1, specGloss.g, _Occlusion);
        emissionMask = specGloss.b;
    #endif

   
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED) && defined(INSTANCE_PERCOLOR_ENABLED)
        albedo.rgb *= i.instanceColor.rgb;
    #else
        albedo *= _MainColor;
    #endif

    half alpha = albedo.a;
    //遮罩颜色
    #if defined(_MaskColorOn)
        float factor = smoothstep(
                        0.0 - 0.05, 
                        1.0, 
                        albedo.a
                    );
        albedo.xyz = lerp(albedo.xyz * _MaskColor.rgb, albedo.xyz, factor);
    #endif
    
    
    
    #if defined(VERTEX_ALPHA)
        alpha *= i.color.r;
    #endif
    AlphaDiscard(alpha, _Cutoff);

    InputData_Lit inputData;
    #if defined(_NORMALMAP)
        half4 normalTex = SAMPLE_TEXTURE2D(_NormalTex, sampler_NormalTex, i.uv.zw);
        EGG_InitializeInputData(i, inputData, normalTex, _NormalScale);
    #else
        EGG_InitializeInputData(i, inputData);
    #endif

    Light mainLight = GetMainLight(inputData.shadowCoord, inputData.positionWS, inputData.shadowMask);
    half3 attenuatedLightColor = lerp(_GlobalShadowColor.rgb, mainLight.color, mainLight.distanceAttenuation * mainLight.shadowAttenuation);
    half3 diffuse = EGG_LightingLambert(attenuatedLightColor, mainLight.direction, inputData.normalWS, _SelfShadow, inputData.bakedGI);
    half3 diffuseCol = diffuse * albedo.rgb * aoMask;

    #if defined(_EMISSIONACTIVE)
        half3 emission = _EmissionColor.rgb * emissionMask;
        diffuseCol += emission;
    #endif

    half4 outCol = half4(diffuseCol, alpha);
    return outCol;
}

///////////////////////////////////////////////////////////////////////////////
//                            Shadow Caster Pass                             //
///////////////////////////////////////////////////////////////////////////////
v2f_shadow vert_shadow(a2v_shadow v, uint instanceID : SV_InstanceID)
{
    v2f_shadow o;

    o.positionCS = Egg_GetShadowPositionHClip(v, instanceID);

    #if defined(UNUSE_SRPBATCH)
        o.uv = v.texcoord * AlbedoTex_ST.xy + AlbedoTex_ST.zw;
    #else
        o.uv = TRANSFORM_TEX(v.texcoord, _AlbedoTex);
    #endif
    
    return o;
}

half4 frag_shadow(v2f_shadow i) : SV_TARGET
{
    #if defined(_ALPHATEST_ON)
        half alpha = SAMPLE_TEXTURE2D(_AlbedoTex, sampler_AlbedoTex, i.uv.xy).a * _MainColor.a;
        //AlphaDiscard(alpha, _Cutoff);
        clip(alpha - _Cutoff);
    #endif
    return 0;
}

///////////////////////////////////////////////////////////////////////////////
//                               Panel  Shadow                               //
///////////////////////////////////////////////////////////////////////////////
v2f_panel_shadow vert_panel_shadow(a2v_panel_shadow i)
{
    v2f_panel_shadow o = (v2f_panel_shadow)0;

    // 得到阴影的世界空间坐标
    float3 shadow_pos = ShadowProjectPos(i.positionOS.xyz, _PanelShadowHeight);
    o.positionCS = TransformWorldToHClip(shadow_pos);
    o.color = _PanelShadowColor;
    return o;
}

half4 frag_panel_shadow(v2f_panel_shadow i) : SV_Target
{
    return i.color;
}

///////////////////////////////////////////////////////////////////////////////
//                              Depth Only Pass                              //
///////////////////////////////////////////////////////////////////////////////
v2f_shadow vert_depthOnly(a2v_shadow v)
{
    v2f_shadow o = (v2f_shadow)0;
    UNITY_SETUP_INSTANCE_ID(v);

    o.positionCS = TransformObjectToHClip(v.positionOS.xyz);

    return o;
}

half4 frag_depthOnly(v2f_shadow i) : SV_TARGET
{
    // #ifdef SCENESELECTIONPASS
    //     return half4(_ObjectId, _PassValue, 1.0, 1.0);
    // #endif
    return 0;
}

#endif