Shader "Puffies/CardPackOpening"
{
    Properties
    {
        [MainTexture] _FrontFacesAlbedo("Front Face", 2D) = "white" {}
        [MainColor][HDR] _FrontFacesColor("Front Tint", Color) = (1, 1, 1, 1)
        [Normal] _FrontFacesNormal("Front Normal", 2D) = "bump" {}
        _BackFacesAlbedo("Back Face", 2D) = "white" {}
        [HDR] _BackFacesColor("Back Tint", Color) = (1, 1, 1, 1)
        [Normal] _BackFacesNormal("Back Normal", 2D) = "bump" {}
        _BackLightScale("Back Light Scale", Float) = 0.1
        _SmoothnessParameters("Smoothness", Range(0, 1)) = 0.8
        _MetallicParameters("Metallic", Range(0, 1)) = 0.685
        [NoScaleOffset] _CubeMap("Reflection Cubemap", Cube) = "white" {}
        _CubeMapPosition("Reflection Direction Offset", Vector) = (0.2, 0.32, 0, 0)
        _CubemapValue("Reflection Strength", Range(0, 1)) = 0.12
        _CubemapPower("Reflection Power", Range(0.01, 10)) = 3.42
        _RampTex("Lighting Ramp", 2D) = "white" {}
        [HDR] _RampColor("Ramp Tint", Color) = (1, 1, 1, 1)
        _RampUVScale("Ramp Curve", Range(-1, 1)) = -0.62
        _AmbientLightPower("Ambient Strength", Range(0, 5)) = 0.6
        _LightParametersPower("Direct Light Power", Range(0, 5)) = 3.12
        _RampTexValue("Ramp Strength", Range(0, 1)) = 0.271
        _OcclusionTex("Occlusion", 2D) = "white" {}
        _OcclutionValue("Occlusion Strength", Range(0, 1.5)) = 1
        _ClipTex("Wave Clip Mask", 2D) = "white" {}
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
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                half3 normalWS : TEXCOORD1;
                half4 tangentWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            TEXTURE2D(_FrontFacesAlbedo);
            SAMPLER(sampler_FrontFacesAlbedo);
            TEXTURE2D(_FrontFacesNormal);
            SAMPLER(sampler_FrontFacesNormal);
            TEXTURE2D(_BackFacesAlbedo);
            SAMPLER(sampler_BackFacesAlbedo);
            TEXTURE2D(_BackFacesNormal);
            SAMPLER(sampler_BackFacesNormal);
            TEXTURECUBE(_CubeMap);
            SAMPLER(sampler_CubeMap);
            TEXTURE2D(_RampTex);
            SAMPLER(sampler_RampTex);
            TEXTURE2D(_OcclusionTex);
            SAMPLER(sampler_OcclusionTex);
            TEXTURE2D(_ClipTex);
            SAMPLER(sampler_ClipTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _FrontFacesAlbedo_ST;
                float4 _FrontFacesNormal_ST;
                float4 _BackFacesAlbedo_ST;
                float4 _BackFacesNormal_ST;
                float4 _RampTex_ST;
                float4 _OcclusionTex_ST;
                float4 _ClipTex_ST;
                half4 _FrontFacesColor;
                half4 _BackFacesColor;
                half4 _RampColor;
                half4 _CubeMap_HDR;
                float4 _CubeMapPosition;
                half _BackLightScale;
                half _SmoothnessParameters;
                half _MetallicParameters;
                half _CubemapValue;
                half _CubemapPower;
                half _RampUVScale;
                half _AmbientLightPower;
                half _LightParametersPower;
                half _RampTexValue;
                half _OcclutionValue;
                half _Cutoff;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS, input.tangentOS);
                output.positionCS = positionInputs.positionCS;
                output.positionWS = positionInputs.positionWS;
                output.normalWS = normalInputs.normalWS;
                output.tangentWS = half4(
                    normalInputs.tangentWS,
                    input.tangentOS.w * GetOddNegativeScale());
                output.uv = input.uv;
                return output;
            }

            half4 Frag(Varyings input, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                half isFront = IS_FRONT_VFACE(isFrontFace, 1.0h, 0.0h);
                float2 frontUv = input.uv * _FrontFacesAlbedo_ST.xy + _FrontFacesAlbedo_ST.zw;
                float2 backUv = input.uv * _BackFacesAlbedo_ST.xy + _BackFacesAlbedo_ST.zw;
                half4 frontSample = SAMPLE_TEXTURE2D(
                    _FrontFacesAlbedo,
                    sampler_FrontFacesAlbedo,
                    frontUv) * _FrontFacesColor;
                half4 backSample = SAMPLE_TEXTURE2D(
                    _BackFacesAlbedo,
                    sampler_BackFacesAlbedo,
                    backUv) * _BackFacesColor;
                half4 albedoSample = lerp(backSample, frontSample, isFront);

                float2 clipUv = input.uv * _ClipTex_ST.xy + _ClipTex_ST.zw;
                half clipMask = SAMPLE_TEXTURE2D(_ClipTex, sampler_ClipTex, clipUv).r;
                clip(min(clipMask, albedoSample.a) - _Cutoff);

                float2 frontNormalUv = input.uv * _FrontFacesNormal_ST.xy + _FrontFacesNormal_ST.zw;
                float2 backNormalUv = input.uv * _BackFacesNormal_ST.xy + _BackFacesNormal_ST.zw;
                half3 frontNormalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_FrontFacesNormal, sampler_FrontFacesNormal, frontNormalUv),
                    1.0h);
                half3 backNormalTS = normalize(
                    SAMPLE_TEXTURE2D(
                        _BackFacesNormal,
                        sampler_BackFacesNormal,
                        backNormalUv).rgb * 2.0h - 1.0h);
                half3 normalTS = normalize(lerp(backNormalTS, frontNormalTS, isFront));
                half3 baseNormalWS = normalize(input.normalWS);
                half3 tangentWS = normalize(input.tangentWS.xyz);
                half3 bitangentWS = input.tangentWS.w * cross(baseNormalWS, tangentWS);
                half3 normalWS = TransformTangentToWorld(
                    normalTS,
                    half3x3(tangentWS, bitangentWS, baseNormalWS),
                    true);
                normalWS *= lerp(-1.0h, 1.0h, isFront);

                half3 viewDirectionWS = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));
                Light mainLight = GetMainLight();
                half3 lightDirectionWS = SafeNormalize(mainLight.direction);
                half ndotl = saturate(dot(normalWS, lightDirectionWS));
                half directLight = pow(max(ndotl, 0.0001h), max(_LightParametersPower, 0.01h));

                half3 directHighlight = albedoSample.rgb
                                        * mainLight.color
                                        * directLight
                                        * saturate(_AmbientLightPower)
                                        * 0.25h;

                half3 halfDirectionWS = SafeNormalize(lightDirectionWS + viewDirectionWS);
                half specularPower = lerp(8.0h, 128.0h, _SmoothnessParameters);
                half specularTerm = pow(saturate(dot(normalWS, halfDirectionWS)), specularPower);
                half3 specularColor = lerp(0.04h.xxx, albedoSample.rgb, _MetallicParameters);
                half3 specular = specularColor * specularTerm * mainLight.color;

                half3 reflectionDirection = reflect(-viewDirectionWS, normalWS);
                reflectionDirection = SafeNormalize(reflectionDirection + _CubeMapPosition.xyz);
                half4 encodedReflection = SAMPLE_TEXTURECUBE(
                    _CubeMap,
                    sampler_CubeMap,
                    reflectionDirection);
                half3 reflection = DecodeHDREnvironment(encodedReflection, _CubeMap_HDR);
                reflection = pow(max(reflection, 0.0001h), max(_CubemapPower, 0.01h));
                half reflectionLuminance = dot(reflection, half3(0.2126h, 0.7152h, 0.0722h));
                reflection = reflectionLuminance.xxx;
                reflection *= lerp(_BackLightScale, _CubemapValue, isFront);

                half rampCurve = max(exp2(_RampUVScale), 0.05h);
                half rampU = pow(max(ndotl, 0.0001h), rampCurve);
                float2 rampUv = float2(rampU, 0.5) * _RampTex_ST.xy + _RampTex_ST.zw;
                half3 rampSample = SAMPLE_TEXTURE2D(_RampTex, sampler_RampTex, rampUv).rgb;
                half rampLuminance = dot(rampSample, half3(0.2126h, 0.7152h, 0.0722h));
                half3 ramp = rampLuminance * _RampColor.rgb * _RampTexValue;

                float2 occlusionUv = input.uv * _OcclusionTex_ST.xy + _OcclusionTex_ST.zw;
                half occlusionSample = SAMPLE_TEXTURE2D(
                    _OcclusionTex,
                    sampler_OcclusionTex,
                    occlusionUv).r;
                half occlusion = lerp(1.0h, occlusionSample, saturate(_OcclutionValue));

                half3 color = albedoSample.rgb
                              + (directHighlight + specular + reflection + ramp) * occlusion;
                return half4(color, 1.0h);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
