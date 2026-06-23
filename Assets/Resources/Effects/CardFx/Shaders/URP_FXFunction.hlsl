#ifndef URP_FXFUNCTION_INCLUDE
#define URP_FXFUNCTION_INCLUDE

#include "URP_FXInput.hlsl"
#include "Egg_URP_GPUSkeletonFunction.hlsl"

struct a2v
{
    float4 positionOS : POSITION;
    float4 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
    float4 uv2 : TEXCOORD1;
    float4 uv3 : TEXCOORD2;
    half4 color : COLOR;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f
{
    #ifndef _USEMASKLAYER_OFF
        float4 uv : TEXCOORD0;
    #else
        float2 uv : TEXCOORD0;
    #endif
    float4 positionCS : SV_POSITION;
    half4 color : TEXCOORD1;
    #if defined(_DISSOLVEACTIVE) || defined(_DISTORTACTIVE) || defined(_GPUSKELETONACTIVE)
        float4 uv2 : TEXCOORD2;
    #endif
    #if defined(_CUSTOMDATA1_DISSOLVEFACTOR)
        float dissolveFactorData : TEXCOORD3;
    #endif
    #if defined(_CUSTOMDATA2_DISTORTINTENSITY)
        float distortIntensityData : TEXCOORD4;
    #endif
    #if defined(_RIMACTIVE)
        //half4 rimColor : TEXCOORD5;
        float3 normalWS : TEXCOORD5;
        float3 viewDirWS : TEXCOORD6;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

v2f vert(a2v v)
{
    v2f o = (v2f)0;
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);

    // 加权计算出最终位置
    #ifdef _GPUSKELETONACTIVE
        float4 frame_param = UNITY_ACCESS_INSTANCED_PROP(Props, _GPUSkeletonFrameParam);
        float4 pos = GetVertexPos(v.positionOS, v.uv2, v.uv3.xy, _GPUSkeletonTex, _GPUSkeletonTex_TexelSize, _GPUSkeletonParam, frame_param);
        v.positionOS.xyz = pos.xyz;
    #endif

    #if defined(_VERTEXANIMATION_NOISE)
        float noise = v.uv.z * 2 - 1;
        float noiseStrength = _NoiseStrength * saturate(v.uv.y + _NoiseAniOffset);
        noise = sin(_Time.y * noise * _NoiseSpeed) * noiseStrength;
        float3 offset = noise * v.normalOS;
        v.positionOS.xyz += offset;
    #endif

    float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
    o.positionCS = TransformWorldToHClip(worldPos);

    #if UNITY_REVERSED_Z
        o.positionCS.z += _DepthOffset;
        // o.positionCS.z = min(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        o.positionCS.z -= _DepthOffset;
        // o.positionCS.z = max(o.positionCS.z, o.positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    #if defined(_CUSTOMDATA3_MAINTEXUVOFFSET)
        o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + frac(_Time.yy * _UVAniSpeed.xy) + v.uv2.zw;
    #else
        o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex) + frac(_Time.yy * _UVAniSpeed.xy);
    #endif
    
    // 序列帧动画
    #ifdef _SEQUENCEACTIVE
        half2 cell = 1 / _MainTex_ST.xy;
        int id = fmod(_Time.y * _SequenceFrameSpeed, _MainTex_ST.x * _MainTex_ST.y);
        int row = floor(id / _MainTex_ST.x);
        int column = fmod(id, _MainTex_ST.x);
        half2 uvOffset = float2(column * cell.x, 1 - row * cell.y);
        o.uv.xy = v.uv * half2(cell.x, cell.y) + half2(0, 1 - cell.y);
        o.uv.xy += uvOffset + _MainTex_ST.zw;
    #endif

    #if defined(_LAYERSUV1)
        float2 layersUV = v.uv2.xy;
    #else
        float2 layersUV = v.uv.xy;
    #endif

    #ifndef _MASKLAYERACTIVE_OFF
        o.uv.zw = TRANSFORM_TEX(layersUV, _MaskTex) + frac(_Time.yy * _UVAniSpeed.zw);
    #endif

    #if defined(_DISSOLVEACTIVE) && defined(_DISTORTACTIVE)
        o.uv2.xy = TRANSFORM_TEX(layersUV, _DissolveTex) + frac(_Time.yy * _DissolveAndDistortSpeed.xy);
        o.uv2.zw = TRANSFORM_TEX(layersUV, _DistortTex) + frac(_Time.yy * _DissolveAndDistortSpeed.zw);
    #elif defined(_DISSOLVEACTIVE) && !defined(_DISTORTACTIVE)
        o.uv2.xy = TRANSFORM_TEX(layersUV, _DissolveTex) + frac(_Time.yy * _DissolveAndDistortSpeed.xy);
        o.uv2.zw = half2(0.0, 0.0);
    #elif !defined(_DISSOLVEACTIVE) && defined(_DISTORTACTIVE)
        o.uv2.xy = half2(0.0, 0.0);
        o.uv2.zw = TRANSFORM_TEX(layersUV, _DistortTex) + frac(_Time.yy * _DissolveAndDistortSpeed.zw);
    #endif

    #if defined(_RIMACTIVE)
        o.normalWS = TransformObjectToWorldNormal(v.normalOS);
        o.viewDirWS = SafeNormalize(_WorldSpaceCameraPos - worldPos);
        //half fresnel = saturate(1 - dot(normalWS, viewDirWS));
        //o.rimColor.rgb = _RimColor.rgb;
        //o.rimColor.a = _RimColor.a * pow(fresnel, _RimFade);
    #endif
    
    o.color = v.color * _MainColor;
    #if defined(_CUSTOMDATA1_DISSOLVEFACTOR) && !defined(_VERTEXANIMATION_NOISE)
        o.dissolveFactorData = v.uv.z;
    #endif
    #if defined(_CUSTOMDATA2_DISTORTINTENSITY)
        o.distortIntensityData = v.uv.w;
    #endif
    
    return o;
}

half4 frag(v2f i) : SV_Target
{

    #if defined(_DISTORTACTIVE)
        half4 distortTex = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, i.uv2.zw);
        #if defined(_CUSTOMDATA2_DISTORTINTENSITY)
            half distortIntensity = _DistortIntensity * i.distortIntensityData * 0.1;
        #else
            half distortIntensity = _DistortIntensity * 0.1;
        #endif
        half2 uvOffset = distortTex.rr * distortIntensity;
        half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy + uvOffset);
    #else
        half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv.xy);
    #endif

    #if defined(_SMOOTHVERTEXALPHA)
        i.color.a = Smoothstep01(i.color.a);
    #endif

    half4 mainCol = mainTex * i.color;

    #if defined(_WHITEISALPHA)
        mainCol.a = saturate(mainTex.r * i.color.a);
    #endif

    #ifndef _MASKLAYERACTIVE_OFF
        #if defined(_DISTORTACTIVE) && defined(_MASKLAYERACTIVE_ADD)
            half4 maskLayer = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv.zw + uvOffset).r * _MaskLayerColor;
        #else
            half4 maskLayer = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv.zw).r * _MaskLayerColor;
        #endif
        
        #if defined(_MASKLAYERACTIVE_ADD)
            mainCol += maskLayer;
        #else
            mainCol.a *= maskLayer.a;
        #endif
        mainCol.a = saturate(mainCol.a);
    #endif
    
    half4 outCol = mainCol;

    #if defined(_RIMACTIVE)
        //outCol *= i.rimColor;
        half fresnel = saturate(1 - dot(i.normalWS, i.viewDirWS));
        outCol.rgb *= _RimColor.rgb;
        outCol.a *= _RimColor.a * pow(fresnel, _RimFade);
    #endif

    #if defined(_DISSOLVEACTIVE)
        #if defined(_DISTORTACTIVE)
            half4 dissolveCol = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.uv2.xy + uvOffset);
        #else
            half4 dissolveCol = SAMPLE_TEXTURE2D(_DissolveTex, sampler_DissolveTex, i.uv2.xy);
        #endif
        
        dissolveCol.r = saturate(dissolveCol.r + 0.01);
        #if defined(_CUSTOMDATA1_DISSOLVEFACTOR) && !defined(_VERTEXANIMATION_NOISE)
            half dissolveFactor = saturate(_DissolveFactor + i.dissolveFactorData);
        #else
            half dissolveFactor = _DissolveFactor;
        #endif
        half dissolve = saturate((dissolveCol.r - dissolveFactor) / (_DissolveWidth * dissolveFactor + HALF_MIN));
        outCol.rgb = lerp(outCol.rgb, _DissolveEdgeCol.rgb, 1 - dissolve);
        outCol.a = outCol.a * dissolve;
    #endif

    // #if defined (_DISTORTACTIVE)
    //     float2 screenUV = i.positionCS.xy / _ScreenParams.xy;
    //     float4 distortTex = SAMPLE_TEXTURE2D(_DistortTex, sampler_DistortTex, i.uv2.zw);
    //     #if defined (_CUSTOMDATA2_DISTORTINTENSITY)
    //         half distortIntensity = _DistortIntensity * i.distortIntensityData * 0.1;
    //     #else
    //         half distortIntensity = _DistortIntensity * 0.1;
    //     #endif
    //     screenUV += distortTex.rr * distortIntensity;
    //     float4 distortCol = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, screenUV);
    //     outCol.rgb *= distortCol.rgb;
    // #endif
    
    return outCol;
}

#endif