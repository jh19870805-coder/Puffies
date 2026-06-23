#ifndef EGG_URP_COMMON_INCLUDE
#define EGG_URP_COMMON_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"


float3 EGG_TangentToWorld(float3x3 T2W, float3 v)
{
    return SafeNormalize(mul(v, T2W));
}

// float3 EGG_NormalTSToWorld(float3 normalTS, v2f_Lit IN)
// {
//     #if defined (_NORMALMAP)
//         return EGG_TangentToWorld(IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz, normalTS);
//     #else
//         return IN.normalWS;
//     #endif
// }

// half3 EGG_UnpackNormalRG(half4 packedNormal, half scale = 1.0)
// {
//     real3 normal;
//     normal.xy = packedNormal.rg * 2.0 - 1.0;
//     normal.z = max(1.0e-16, sqrt(1.0 - saturate(dot(normal.xy, normal.xy))));

//     // must scale after reconstruction of normal.z which also
//     // mirrors UnpackNormalRGB(). This does imply normal is not returned
//     // as a unit length vector but doesn't need it since it will get normalized after TBN transformation.
//     // If we ever need to blend contributions with built-in shaders for URP
//     // then we should consider using UnpackDerivativeNormalAG() instead like
//     // HDRP does since derivatives do not use renormalization and unlike tangent space
//     // normals allow you to blend, accumulate and scale contributions correctly.
//     normal.xy *= scale;
//     return normal;
// }

half3 EGG_UnpackNormalScale(half4 normalTex, half scale = 1.0h)
{
    #if defined(UNITY_ASTC_NORMALMAP_ENCODING)
        return UnpackNormalAG(normalTex, scale);
    #elif defined(UNITY_NO_DXT5nm)
        return UnpackNormalRGB(normalTex, scale);
    #else
        return UnpackNormalmapRGorAG(normalTex, scale);
        // return UnpackNormalRGB(normalTex, scale);
    #endif
}

float3 EGG_GetNormalWS(float3x3 T2W, half4 normalTex, half scale = 1.0h)
{
    half3 normalTS = EGG_UnpackNormalScale(normalTex, scale);

    return EGG_TangentToWorld(T2W, normalTS);
}

float3 EGG_NormalBlendUnpack(float3 N1, float3 N2, half mask)
{
    float3 nb = float3(N1.xy + N2.xy, N1.z * N2.z);
    float3 nOut = lerp(N1, nb, mask);
    //float3 nOut = mask > 0.0 ? nb : n1;
    return nOut;
}

half3 EGG_NormalBlendUnpack(half3 N1, half3 N2)
{
    half3 nb = float3(N1.xy + N2.xy, N1.z * N2.z);
    return nb;
}

float3 EGG_NormalBlend_UDN(half4 N1, half scale1, half4 N2, half scale2, half mask)
{
    float3 n1 = EGG_UnpackNormalScale(N1, scale1);
    float3 n2 = UnpackNormalAG(N2, scale2);

    float3 nOut = EGG_NormalBlendUnpack(n1, n2, mask);
    return nOut;
}

// float3 EGG_NormalBlend_RNM(float3 N1, float3 N2)
// {
//     float3 n1 = N1 * 2 + float3(-1, -1 , 0);
//     float3 n2 = N2 * float3(-2, -2, 2) + float3(1, 1, -1);
//     float3 r = n1 * dot(n1, n2) / n1.z - n2;
//     return r * 0.5 + 0.5;
// }

// float3 EGG_SampleNormalWithDetail(v2f_Lit IN, half4 normalTex, half4 detailNormalTex, half scale = 1.0h, half detailScale = 1.0h, half detailMask = 1.0h)
// {
//     float3 normalTS = EGG_UnpackNormalScale(normalTex, scale);
//     float3 detailNormalTS = EGG_UnpackNormalScale(detailNormalTex, detailScale);
//     //detailNormalTS = normalize(detailNormalTS);
//     normalTS = lerp(normalTS, BlendNormalRNM(normalTS, detailNormalTS), detailMask);//BlendNormalRNM include CommonMaterial.hlsl
//     return EGG_NormalTSToWorld(normalTS, IN);
// }

// float3 EGG_SampleNormalWithFabricDetail(v2f_Lit IN, float3 normalTex, float3 detailNormalTex, half scale = 1.0h, half detailScale = 1.0h, half detailMask = 1.0h)
// {
//     float3 normalTS = EGG_NormalBlend_UDN(normalTex, scale, detailNormalTex, detailScale, detailMask);

//     return EGG_NormalTSToWorld(normalTS, IN);
// }

// float4x4 inverseMatrix(float4x4 input)
// {
// 	#define minor(a,b,c) determinant(float3x3(input.a, input.b, input.c))

// 		float4x4 cofactors = float4x4(
// 			minor(_22_23_24, _32_33_34, _42_43_44),
// 			-minor(_21_23_24, _31_33_34, _41_43_44),
// 			minor(_21_22_24, _31_32_34, _41_42_44),
// 			-minor(_21_22_23, _31_32_33, _41_42_43),

// 			-minor(_12_13_14, _32_33_34, _42_43_44),
// 			minor(_11_13_14, _31_33_34, _41_43_44),
// 			-minor(_11_12_14, _31_32_34, _41_42_44),
// 			minor(_11_12_13, _31_32_33, _41_42_43),

// 			minor(_12_13_14, _22_23_24, _42_43_44),
// 			-minor(_11_13_14, _21_23_24, _41_43_44),
// 			minor(_11_12_14, _21_22_24, _41_42_44),
// 			-minor(_11_12_13, _21_22_23, _41_42_43),

// 			-minor(_12_13_14, _22_23_24, _32_33_34),
// 			minor(_11_13_14, _21_23_24, _31_33_34),
// 			-minor(_11_12_14, _21_22_24, _31_32_34),
// 			minor(_11_12_13, _21_22_23, _31_32_33)
// 			);
// 	#undef minor
// 	return transpose(cofactors) / determinant(input);
// }

VertexNormalInputs EGG_GetVertexNormalInputs(float3 normalOS, float4 tangentOS)
{
    VertexNormalInputs tbn;

    real sign = tangentOS.w * unity_WorldTransformParams.w;
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    tbn.tangentWS = TransformObjectToWorldDir(tangentOS.xyz);
    tbn.bitangentWS = cross(tbn.normalWS, tbn.tangentWS) * sign;
    return tbn;
}

VertexNormalInputs EGG_GetVertexNormalInputsInstance(float3 normalOS, float4 tangentOS, float4x4 worldMatrix)
{
    VertexNormalInputs tbn;

    real sign = tangentOS.w * unity_WorldTransformParams.w;
    //float4x4 worldToObject = inverseMatrix(worldMatrix);
    //tbn.normalWS = normalize(mul(normalOS, (float3x3)worldToObject));
    tbn.normalWS = SafeNormalize(mul((float3x3)worldMatrix, normalOS)).xyz;
    tbn.tangentWS = SafeNormalize(mul((float3x3)worldMatrix, tangentOS.xyz));
    tbn.bitangentWS = cross(tbn.normalWS, tbn.tangentWS) * sign;
    return tbn;
}

VertexNormalInputs EGG_GetVertexNormalInputs(float3 normalOS)
{
    VertexNormalInputs tbn;
    tbn.tangentWS = real3(1.0, 0.0, 0.0);
    tbn.bitangentWS = real3(0.0, 1.0, 0.0);
    tbn.normalWS = TransformObjectToWorldNormal(normalOS);
    return tbn;
}

half3 EGG_LightingLambert(half3 lightColor, half3 lightDir, half3 normal, half selfShadow, half3 bakedGI)
{
    half NdotL = saturate(dot(normal, lightDir));
    half3 outColor = lightColor * NdotL;
    half3 gi = (1.0 - NdotL) * (1.0 - selfShadow) * bakedGI + bakedGI;
    return outColor + gi;
}

// half3 EGG_LightingLambert(half3 lightColor, half3 lightDir, half3 normal, out half NdotL)
// {
//     NdotL = saturate(dot(normal, lightDir));
//     return lightColor * NdotL;
// }

// half3 EGG_WaterSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
// {
//     half NdotV = dot(abs(viewDir), normal);
//     half NaddV = SafeNormalize(normal + (-viewDir));

//     //float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
//     half NdotH = saturate(dot(NdotV, NaddV));


//     half modifier = pow(NdotH, smoothness);
//     half3 specularReflection = specular.rgb * modifier;
//     return lightColor * specularReflection;
// }

half3 EGG_WaterSpecular2(half3 lightAtten, half3 lightDir, half3 normal, half3 viewDir, half3 specular, half smoothness, half specIntensity)
{
    float3 halfVec = SafeNormalize((lightDir) + (abs(viewDir)));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = saturate(pow(NdotH, smoothness));
    half3 specularReflection = (specular.rgb + specIntensity) * modifier;
    return lightAtten * specularReflection;
    // return modifier;

}

float4 URP_ComputeScreenPos(float4 pos, float projectionSign)
{
    float4 o = pos * 0.5f;
    o.xy = float2(o.x, o.y * projectionSign) + o.w;
    o.zw = pos.zw;
    return o;
}

half3 EnvBRDFApprox(half3 SpecularColor, half3 EvnColor, half NdotV)
{
    // half4 c0 = half4(-1.0, -0.0275, -0.573, 0.0229);
    half4 c1 = half4(1.0, 0.0425, 1.0417, -0.0417);
    // half4 r = Roughness * c0 + c1;
    half4 r = c1;
    half a004 = min(r.x * r.x, exp2(-9.28 * NdotV)) * r.x + r.y;
    half2 AB = half2(-1.0417, 1.0417) * a004 + r.zw;
    return SpecularColor * AB.x + AB.y * EvnColor;
}

float ComputeMipmapLevel(float2 uv)
{
    float2 dx = ddx(uv);
    float2 dy = ddy(uv);
    
    // 计算纹理坐标变化率（偏导数的平方和的最大值）
    float delta = max(dot(dx, dx), dot(dy, dy));
    
    // 使用log2转换为mipmap级别
    // 0.5 * log2(delta)是从像素空间变化率到mip级别的标准转换公式
    return max(0.5 * log2(delta), 0.0);
}

#endif