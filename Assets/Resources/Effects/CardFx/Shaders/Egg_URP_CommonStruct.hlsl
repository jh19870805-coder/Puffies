#ifndef EGG_URP_COMMONSTRUCT_INCLUDE
#define EGG_URP_COMMONSTRUCT_INCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "Egg_URP_CommonFunction.hlsl"

struct a2v_Lit
{
    float4 positionOS : POSITION;
    float2 texcoord : TEXCOORD0;
    float2 lightmapUV : TEXCOORD1;
    #if defined(_GPUSKIN)
        float2 weight_uv : TEXCOORD2;
    #endif
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    #if defined(_NEEDVERTEXCOLOR)
        float4 color : COLOR;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f_Lit
{
    #if defined(CUSTOMLIGHTMAP_ON)
        float2 lightmapUV : TEXCOORD1;
    #else
        DECLARE_LIGHTMAP_OR_SH(lightmapUV, vertexSH, 1);
    #endif

    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        #if defined(_NORMALMAP)
            float4 uv : TEXCOORD0;
            float4 normalWS : TEXCOORD2;
            float4 tangentWS : TEXCOORD3;
            float4 bitangentWS : TEXCOORD4;
        #else
            float2 uv : TEXCOORD0;
            float3 normalWS : TEXCOORD2;
            float3 viewDir : TEXCOORD3;
            float3 positionWS : TEXCOORD4;
        #endif

        half4 fogFactorAndVertexLight : TEXCOORD5; // x: fogFactor, yzw: vertex light
        float4 shadowCoord : TEXCOORD6;
        #if defined(_DETAILACTIVE)
            float2 detailUV : TEXCOORD7;
        #endif
        #if defined(_NEEDVERTEXCOLOR)
            half4 color : TEXCOORD8;
        #endif
        #if defined(INSTANCE_PERCOLOR_ENABLED)
            half4 instanceColor : TEXCOORD9;
        #endif
        // float3 viewDir : TEXCOORD10;
        float4 positionCS : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID

    #else
        #if defined(_NORMALMAP) || defined(_HAIRRENDER)
            float4 uv : TEXCOORD0;
            float4 normalWS : TEXCOORD2;
            float4 tangentWS : TEXCOORD3;
            float4 bitangentWS : TEXCOORD4;
        #else
            #if defined(ADDITION_ON)
                float4 uv : TEXCOORD0;
            #else
                float2 uv : TEXCOORD0;
            #endif
            float3 normalWS : TEXCOORD2;
            float3 viewDir : TEXCOORD3;
            float3 positionWS : TEXCOORD4;
        #endif

        half4 fogFactorAndVertexLight : TEXCOORD5; // x: fogFactor, yzw: vertex light
        float4 shadowCoord : TEXCOORD6;
        #if defined(_DETAILACTIVE)
            float2 detailUV : TEXCOORD7;
        #endif
        #if defined(_FABRICRENDER)
            float4 detailUV12 : TEXCOORD7;
            float4 detailUV34 : TEXCOORD8;
        #endif
        #if defined(_NEEDVERTEXCOLOR)
            half4 color : TEXCOORD9;
        #endif
        // float3 viewDir : TEXCOORD10;
        #if defined(_WATERRENDER)
            float2 uv2 : TEXCOORD10;
            // float2 phase0                     : TEXCOORD11;
            // float2 phase1                     : TEXCOORD12;
            float timeSpeed : TEXCOORD11;
            float3 viewDirTS : TEXCOORD12;
        #endif
        float4 positionCS : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    #endif
};

struct InputData_Lit
{
    float3 positionWS;
    float3 normalWS;
    #if defined(_NEEDVIEWDIR) || defined(_NEEDFRESNEL)
        float3 viewDirWS;
    #endif
    #if defined(_NEEDFRESNEL)
        half NdotV;
        half fresnelTerm;
    #endif
    float4 shadowCoord;
    half fogCoord;
    half3 vertexLighting;
    half3 bakedGI;
    //float2  normalizedScreenSpaceUV;
    half4 shadowMask;
};

//#if SHADER_TARGET >= 45
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
    struct CullResultInfo
    {
        float4x4 matrices;
        int meshTypeID;
        int groupID;
        int meshLOD;
        int matricesIndex;
    };

    struct BoundingInfo
    {
        float3 boundMin;
        float3 boundMax;
        float maxDistance;
        float distanceLOD1;
        float distanceLOD2;
        uint typeOffset;
        uint typeCulling;
        uint perTypeCount;
        uint maxLODLevel;
    };

    StructuredBuffer<CullResultInfo> resultBuffer;
    StructuredBuffer<BoundingInfo> boundBuffer;
    int typeID;
#endif

#if !defined(_HAIRRENDER)

    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        v2f_Lit InitializeVertInputInstance(a2v_Lit v, half4 _AlbedoTex_ST, half4 _NormalTex_ST, half4 _DetailTex_ST = half4(1, 1, 0, 0), uint instanceID = 0)
        {
            v2f_Lit o = (v2f_Lit)0;

            // 找到该类资源索引
            uint offset = boundBuffer[typeID].typeOffset;
            // 找到该类资源的具体实例索引（InstanceID）
            uint index = offset +instanceID;
            // 给定buffer在索引位置
            uint matrixID = resultBuffer[index].matricesIndex;
            float4x4 _matrix = resultBuffer[matrixID].matrices;
            float3 worldPos = mul(_matrix, v.positionOS).xyz;

            #if defined(INSTANCE_PERCOLOR_ENABLED)
                o.instanceColor = _matrix._m30_m31_m32_m33;
            #endif

            o.positionCS = TransformWorldToHClip(worldPos);

            o.uv.xy = v.texcoord * _AlbedoTex_ST.xy + _AlbedoTex_ST.zw;

            // half3 viewDirWS = GetWorldSpaceViewDir(worldPos);
            // o.viewDir = viewDirWS;

            #if defined(_NORMALMAP)
                o.uv.zw = v.texcoord * _NormalTex_ST.xy + _NormalTex_ST.zw;
                VertexNormalInputs normalInput = EGG_GetVertexNormalInputsInstance(v.normalOS, v.tangentOS, _matrix);

                o.normalWS.xyz = normalInput.normalWS;
                o.tangentWS.xyz = normalInput.tangentWS;
                o.bitangentWS.xyz = normalInput.bitangentWS;
                o.normalWS.w = worldPos.x;
                o.tangentWS.w = worldPos.y;
                o.bitangentWS.w = worldPos.z;
            #else
                //float4x4 worldToObject = inverseMatrix(_matrix);
                //o.normalWS = normalize(mul(v.normalOS, (float3x3)worldToObject));
                o.normalWS = SafeNormalize(mul((float3x3)_matrix, v.normalOS)).xyz;
                o.viewDir = _WorldSpaceCameraPos - worldPos;
                o.positionWS = worldPos;
            #endif

            #if defined(_DETAILACTIVE)
                o.detailUV = v.texcoord * _DetailTex_ST.xy + _DetailTex_ST.zw;
            #endif

            #if defined(CUSTOMLIGHTMAP_ON)
                o.lightmapUV = v.lightmapUV;
            #else
                OUTPUT_LIGHTMAP_UV(v.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS.xyz, o.vertexSH);
            #endif


            o.fogFactorAndVertexLight.x = ComputeFogFactor(o.positionCS.z);
            o.fogFactorAndVertexLight.yzw = VertexLighting(worldPos, o.normalWS.xyz);

            o.shadowCoord = TransformWorldToShadowCoord(worldPos);

            return o;
        }
    #else
        v2f_Lit InitializeVertInput(a2v_Lit v, half4 _AlbedoTex_ST, half4 _NormalTex_ST, half4 _DetailTex_ST = half4(1, 1, 0, 0), half4 _FabricDetailTex1_ST = half4(1, 1, 0, 0), half4 _FabricDetailTex2_ST = half4(1, 1, 0, 0), half4 _FabricDetailTex3_ST = half4(1, 1, 0, 0), half4 _FabricDetailTex4_ST = half4(1, 1, 0, 0))
        {
            v2f_Lit o = (v2f_Lit)0;

            float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
            o.positionCS = TransformWorldToHClip(worldPos);

            o.uv.xy = v.texcoord * _AlbedoTex_ST.xy + _AlbedoTex_ST.zw;

            // half3 viewDirWS = GetWorldSpaceViewDir(worldPos);
            // o.viewDir = viewDirWS;

            #if defined(_NORMALMAP)
                o.uv.zw = v.texcoord * _NormalTex_ST.xy + _NormalTex_ST.zw;
                VertexNormalInputs normalInput = EGG_GetVertexNormalInputs(v.normalOS, v.tangentOS);

                o.normalWS.xyz = normalInput.normalWS;
                o.tangentWS.xyz = normalInput.tangentWS;
                o.bitangentWS.xyz = normalInput.bitangentWS;
                o.normalWS.w = worldPos.x;
                o.tangentWS.w = worldPos.y;
                o.bitangentWS.w = worldPos.z;
            #else
                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.viewDir = _WorldSpaceCameraPos - worldPos;
                // o.viewDir = viewDirWS;
                o.positionWS = worldPos;
            #endif

            #if defined(_DETAILACTIVE)
                o.detailUV = v.texcoord * _DetailTex_ST.xy + _DetailTex_ST.zw;
            #endif

            #if defined(_FABRICRENDER)
                o.detailUV12.xy = v.texcoord * _FabricDetailTex1_ST.xy + _FabricDetailTex1_ST.zw;
                o.detailUV12.zw = v.texcoord * _FabricDetailTex2_ST.xy + _FabricDetailTex2_ST.zw;
                o.detailUV34.xy = v.texcoord * _FabricDetailTex3_ST.xy + _FabricDetailTex3_ST.zw;
                o.detailUV34.zw = v.texcoord * _FabricDetailTex4_ST.xy + _FabricDetailTex4_ST.zw;
            #endif

            #if defined(CUSTOMLIGHTMAP_ON)
                o.lightmapUV = v.lightmapUV;
            #else
                OUTPUT_LIGHTMAP_UV(v.lightmapUV, unity_LightmapST, o.lightmapUV);
                OUTPUT_SH(o.normalWS.xyz, o.vertexSH);
            #endif

            o.fogFactorAndVertexLight.x = ComputeFogFactor(o.positionCS.z);
            o.fogFactorAndVertexLight.yzw = VertexLighting(worldPos, o.normalWS.xyz);

            o.shadowCoord = TransformWorldToShadowCoord(worldPos);

            return o;
        }
    #endif

#endif

#if !defined(_SUBSURFACESCATTER) && !defined(_HAIRRENDER)
    void EGG_InitializeInputData(v2f_Lit IN, out InputData_Lit input, half4 normalTS = half4(0, 0, 1, 1), half normalScale = 1)
    {
        input = (InputData_Lit)0;
        #if defined(_NORMALMAP)
            input.positionWS = float3(IN.normalWS.w, IN.tangentWS.w, IN.bitangentWS.w);
            float3x3 T2W = float3x3(IN.tangentWS.xyz, IN.bitangentWS.xyz, IN.normalWS.xyz);
            #if defined(_DETAILACTIVE)// && defined(_DETAILMAP)
                input.normalWS = EGG_TangentToWorld(T2W, normalTS.xyz);
            #elif defined(_FABRICRENDER) && defined(_FABRICMAP)
                input.normalWS = EGG_TangentToWorld(T2W, normalTS.xyz);
            #else
                input.normalWS = EGG_GetNormalWS(T2W, normalTS, normalScale);
            #endif
            #if defined(_NEEDVIEWDIR) || defined(_NEEDFRESNEL)
                input.viewDirWS = SafeNormalize(_WorldSpaceCameraPos - input.positionWS);
                // input.viewDirWS = SafeNormalize(IN.viewDir);
            #endif
        #else
            input.positionWS = IN.positionWS;
            input.normalWS = IN.normalWS.xyz;
            #if defined(_NEEDVIEWDIR) || defined(_NEEDFRESNEL)
                input.viewDirWS = SafeNormalize(IN.viewDir);
            #endif
        #endif

        #if defined(_NEEDFRESNEL)
            input.NdotV = saturate(dot(input.normalWS, input.viewDirWS));
            input.fresnelTerm = Pow4(1.0 - input.NdotV);
        #endif

        //input.normalWS = NormalizeNormalPerPixel(input.normalWS);

        input.shadowCoord = IN.shadowCoord;
        // input.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

        input.fogCoord = IN.fogFactorAndVertexLight.x;
        input.vertexLighting = IN.fogFactorAndVertexLight.yzw;

        #ifndef CUSTOMLIGHTMAP_ON
            input.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, input.normalWS);
            input.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUV);
        #endif
    }
#endif

#if defined(_SUBSURFACESCATTER)
    void EGG_InitializeSSSInputData(v2f_Lit IN, float3x3 T2W, out InputData_Lit input, half4 normalTS = half4(0, 0, 1, 0), half normalScale = 1)
    {
        input = (InputData_Lit)0;
        #if defined(_NORMALMAP)
            input.positionWS = float3(IN.normalWS.w, IN.tangentWS.w, IN.bitangentWS.w);
            #if defined(_DETAILACTIVE)// && defined(_DETAILMAP)
                input.normalWS = SafeNormalize(mul(normalTS.xyz, T2W));
            #else
                normalTS.xyz = EGG_UnpackNormalScale(normalTS, normalScale);
                input.normalWS = SafeNormalize(mul(normalTS.xyz, T2W));
            #endif
            #if defined(_NEEDVIEWDIR) || defined(_NEEDFRESNEL)
                input.viewDirWS = SafeNormalize(_WorldSpaceCameraPos - input.positionWS);;
            #endif
        #else
            input.positionWS = IN.positionWS;
            input.normalWS = IN.normalWS.xyz;
            #if defined(_NEEDVIEWDIR) || defined(_NEEDFRESNEL)
                input.viewDirWS = SafeNormalize(IN.viewDir);
            #endif
        #endif

        #if defined(_NEEDFRESNEL)
            input.NdotV = saturate(dot(input.normalWS, input.viewDirWS));
            input.fresnelTerm = Pow4(1.0 - input.NdotV);
        #endif

        //input.normalWS = NormalizeNormalPerPixel(input.normalWS);

        input.shadowCoord = IN.shadowCoord;
        // input.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

        input.fogCoord = IN.fogFactorAndVertexLight.x;
        input.vertexLighting = IN.fogFactorAndVertexLight.yzw;

        input.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, input.normalWS);
        input.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUV);
    }
#endif

#if defined(_HAIRRENDER)
    void EGG_InitializeHairInputData(v2f_Lit IN, float3x3 T2W, out InputData_Lit input, half4 normalTS = half4(0, 0, 1, 0), half normalScale = 1)
    {
        input = (InputData_Lit)0;

        input.positionWS = float3(IN.normalWS.w, IN.tangentWS.w, IN.bitangentWS.w);
        #if defined(_NEEDVIEWDIR) || defined(_NEEDFRESNEL)
            input.viewDirWS = SafeNormalize(_WorldSpaceCameraPos - input.positionWS);;
        #endif
        #if defined(_NORMALMAP)
            normalTS.xyz = UnpackNormalRGB(normalTS, normalScale);
            input.normalWS = SafeNormalize(mul(normalTS.xyz, T2W));
        #else
            input.normalWS = IN.normalWS.xyz;
        #endif

        #if defined(_NEEDFRESNEL)
            input.NdotV = saturate(dot(input.normalWS, input.viewDirWS));
            input.fresnelTerm = Pow4(1.0 - input.NdotV);
        #endif

        //input.normalWS = NormalizeNormalPerPixel(input.normalWS);

        input.shadowCoord = IN.shadowCoord;
        // input.shadowCoord = TransformWorldToShadowCoord(input.positionWS);

        input.fogCoord = IN.fogFactorAndVertexLight.x;
        input.vertexLighting = IN.fogFactorAndVertexLight.yzw;

        input.bakedGI = SAMPLE_GI(IN.lightmapUV, IN.vertexSH, input.normalWS);
        input.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUV);
    }
#endif

///////////////////////////////////////////////////////////////////////////////
//                                   ShadowCaster                            //
///////////////////////////////////////////////////////////////////////////////
float3 _LightDirection;

struct a2v_shadow
{
    float4 positionOS : POSITION;
    float3 normalOS : NORMAL;
    float2 texcoord : TEXCOORD0;
    #if defined(_GPUSKIN)
        float2 index_uv : TEXCOORD1;
        float2 weight_uv : TEXCOORD2;
    #endif
    #if defined(_NEEDVERTEXCOLOR)
        float4 color : COLOR;
    #endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f_shadow
{
    float2 uv : TEXCOORD0;
    float4 positionCS : SV_POSITION;
    #if defined(_NEEDVERTEXCOLOR)
        float4 color : TEXCOORD1;
    #endif
};

float4 Egg_GetShadowPositionHClip(a2v_shadow input, uint instanceID = 0)
{
    #if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
        uint offset = boundBuffer[typeID].typeOffset;
        uint index = offset +instanceID;
        uint matrixID = resultBuffer[index].matricesIndex;
        float4x4 _matrix = resultBuffer[matrixID].matrices;
        float3 positionWS = mul(_matrix, input.positionOS).xyz;
        float3 normalWS = SafeNormalize(mul((float3x3)_matrix, input.normalOS)).xyz;
        //float4x4 worldToObject = inverseMatrix(_matrix);
        //float3 normalWS = normalize(mul(input.normalOS, (float3x3)worldToObject));
    #else
        UNITY_SETUP_INSTANCE_ID(input);
        float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
        float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
    #endif

    float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));

    #if UNITY_REVERSED_Z
        positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #else
        positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
    #endif

    return positionCS;
}

///////////////////////////////////////////////////////////////////////////////
//                                   PanelShadow                             //
///////////////////////////////////////////////////////////////////////////////

struct a2v_panel_shadow
{
    float4 positionOS : POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct v2f_panel_shadow
{
    float4 positionCS : SV_POSITION;
    float4 color : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

float3 ShadowProjectPos(float3 vert_pos, float shadow_height)
{
    float3 shadow_pos;

    // 得到顶点的世界空间坐标
    float3 world_pos = TransformObjectToWorld(vert_pos);

    // 灯光方向
    Light light = GetMainLight();
    float3 light_dir = SafeNormalize(light.direction);

    // 阴影的物体空间坐标（低于地面的部分不做改变）
    shadow_pos.y = min(world_pos.y, shadow_height);
    shadow_pos.xz = world_pos.xz - light_dir.xz * max(0, world_pos.y - shadow_height) / light_dir.y;

    return shadow_pos;
}

#endif