// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "ASESampleShaders/Community/TFHC/2 Sided_w_01"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[HDR]_FrontFacesColor("Front Faces Color", Color) = (1,0,0,0)
		_FrontFacesAlbedo("Front Faces Albedo", 2D) = "white" {}
		_FrontFacesNormal("Front Faces Normal", 2D) = "white" {}
		[HDR]_BackFacesColor("Back Faces Color", Color) = (1,1,1,1)
		_BackFacesAlbedo("Back Faces Albedo", 2D) = "white" {}
		_BackFacesNormal("Back Faces Normal", 2D) = "white" {}
		_BackLightScale("Back Light Scale", Float) = 0
		_OpacityMask("Opacity Mask", 2D) = "white" {}
		_smoothnessParameters("smoothnessParameters", Range( 0 , 1)) = 0.5
		_metallicParameters("metallicParameters", Range( 0 , 1)) = 0.5
		_AO("AO", 2D) = "white" {}
		_AOParameters("AOParameters", Range( 0 , 1)) = 0.5
		_Cubemap("Cubemap", CUBE) = "white" {}
		_RampTex("RampTex", 2D) = "white" {}
		[HDR]_RampColor("RampColor", Color) = (0,0,0,0)
		_RampUVScale("Ramp UV Scale", Range( -1 , 1)) = 24.4
		_RampAndCubemapLerp("Ramp And Cubemap Lerp", Range( 0 , 1)) = 0
		_LightParametersPower("Light Parameters Power", Range( 0 , 5)) = 0.5
		_AmbientLightParametersPower("Ambient Light Parameters Power", Range( 0 , 5)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		Stencil
		{
			Ref 1
			CompFront Always
			PassFront Replace
		}
		CGINCLUDE
		#include "UnityCG.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float3 worldRefl;
		};

		uniform sampler2D _FrontFacesNormal;
		uniform float4 _FrontFacesNormal_ST;
		uniform sampler2D _BackFacesNormal;
		uniform float4 _BackFacesNormal_ST;
		uniform float4 _FrontFacesColor;
		uniform sampler2D _FrontFacesAlbedo;
		uniform float4 _FrontFacesAlbedo_ST;
		uniform float4 _BackFacesColor;
		uniform sampler2D _BackFacesAlbedo;
		uniform float4 _BackFacesAlbedo_ST;
		uniform samplerCUBE _Cubemap;
		uniform sampler2D _RampTex;
		uniform float _RampUVScale;
		uniform float _RampAndCubemapLerp;
		uniform float4 _RampColor;
		uniform float _AmbientLightParametersPower;
		uniform float _LightParametersPower;
		uniform float _BackLightScale;
		uniform float _metallicParameters;
		uniform float _smoothnessParameters;
		uniform sampler2D _AO;
		SamplerState sampler_AO;
		uniform float4 _AO_ST;
		uniform float _AOParameters;
		uniform sampler2D _OpacityMask;
		SamplerState sampler_OpacityMask;
		uniform float4 _OpacityMask_ST;
		uniform float _Cutoff = 0.5;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_FrontFacesNormal = i.uv_texcoord * _FrontFacesNormal_ST.xy + _FrontFacesNormal_ST.zw;
			float4 tex2DNode50 = tex2D( _FrontFacesNormal, uv_FrontFacesNormal );
			float4 FrontFacesNormal51 = tex2DNode50;
			float2 uv_BackFacesNormal = i.uv_texcoord * _BackFacesNormal_ST.xy + _BackFacesNormal_ST.zw;
			float4 tex2DNode53 = tex2D( _BackFacesNormal, uv_BackFacesNormal );
			float4 BackFacesNormal54 = tex2DNode53;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult20 = dot( ase_worldNormal , ase_worldViewDir );
			float FaceSign48 = (1.0 + (sign( dotResult20 ) - -1.0) * (0.0 - 1.0) / (1.0 - -1.0));
			float4 lerpResult64 = lerp( FrontFacesNormal51 , BackFacesNormal54 , FaceSign48);
			o.Normal = lerpResult64.rgb;
			float2 uv_FrontFacesAlbedo = i.uv_texcoord * _FrontFacesAlbedo_ST.xy + _FrontFacesAlbedo_ST.zw;
			float4 FrontFacesAlbedo44 = ( _FrontFacesColor * tex2D( _FrontFacesAlbedo, uv_FrontFacesAlbedo ) );
			float2 uv_BackFacesAlbedo = i.uv_texcoord * _BackFacesAlbedo_ST.xy + _BackFacesAlbedo_ST.zw;
			float4 BackFacesAlbedo47 = ( _BackFacesColor * tex2D( _BackFacesAlbedo, uv_BackFacesAlbedo ) );
			float4 lerpResult24 = lerp( FrontFacesAlbedo44 , BackFacesAlbedo47 , FaceSign48);
			o.Albedo = lerpResult24.rgb;
			float4 texCUBENode67 = texCUBE( _Cubemap, WorldReflectionVector( i , tex2DNode50.rgb ) );
			float3 desaturateInitialColor158 = texCUBENode67.rgb;
			float desaturateDot158 = dot( desaturateInitialColor158, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar158 = lerp( desaturateInitialColor158, desaturateDot158.xxx, 1.0 );
			float3 objToWorldDir172 = mul( unity_ObjectToWorld, float4( desaturateVar158, 0 ) ).xyz;
			float3 normalizeResult101 = normalize( ase_worldNormal );
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float3 normalizeResult100 = normalize( ase_worldlightDir );
			float dotResult104 = dot( normalizeResult101 , normalizeResult100 );
			float clampResult218 = clamp( dotResult104 , 0.0 , 1.0 );
			float temp_output_109_0 = ( pow( objToWorldDir172.x , _RampUVScale ) * clampResult218 );
			float4 appendResult110 = (float4(temp_output_109_0 , temp_output_109_0 , 0.0 , 0.0));
			float4 lerpResult130 = lerp( texCUBENode67 , tex2D( _RampTex, appendResult110.xy ) , _RampAndCubemapLerp);
			float temp_output_186_0 = ( pow( objToWorldDir172.x , _AmbientLightParametersPower ) * pow( clampResult218 , _LightParametersPower ) );
			float clampResult219 = clamp( temp_output_186_0 , 0.0 , 1.0 );
			float4 lerpResult145 = lerp( ( ( lerpResult130 * _RampColor ) * clampResult219 ) , ( lerpResult24 * texCUBE( _Cubemap, WorldReflectionVector( i , tex2DNode53.rgb ) ) * _BackLightScale ) , FaceSign48);
			o.Emission = lerpResult145.rgb;
			o.Metallic = _metallicParameters;
			o.Smoothness = _smoothnessParameters;
			float2 uv_AO = i.uv_texcoord * _AO_ST.xy + _AO_ST.zw;
			o.Occlusion = ( tex2D( _AO, uv_AO ).r * _AOParameters );
			o.Alpha = 1;
			float2 uv_OpacityMask = i.uv_texcoord * _OpacityMask_ST.xy + _OpacityMask_ST.zw;
			float OpacityMask56 = tex2D( _OpacityMask, uv_OpacityMask ).a;
			clip( OpacityMask56 - _Cutoff );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.worldRefl = -worldViewDir;
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18500
1594;330;1294;1059;1317.36;-428.7163;1.62321;True;True
Node;AmplifyShaderEditor.CommentaryNode;52;-1776.875,-811.7521;Inherit;False;870.9222;707.2373;Comment;6;43;44;28;42;50;51;Front Faces;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;50;-1700.404,-334.5151;Inherit;True;Property;_FrontFacesNormal;Front Faces Normal;3;0;Create;True;0;0;False;0;False;-1;None;a9487980a3c0c0d448aa2ea6319e676d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldReflectionVector;66;-1653.799,597.5328;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;67;-1437.066,563.488;Inherit;True;Property;_Cubemap;Cubemap;13;0;Create;True;0;0;False;0;False;-1;None;d09ad14bb4e3d4840b7238f7863dac52;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;98;-1658.634,989.6877;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;97;-1692.27,1186.296;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;101;-1437.738,1018.878;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;158;-1089.02,554.2665;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;100;-1431.388,1201.685;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;49;-1774.799,4.739527;Inherit;False;1094.131;402.4268;Comment;6;20;22;23;48;19;41;Face Sign (0 = Front, 1 = Back);1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;104;-1229.365,1120.99;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;19;-1699.579,223.1664;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;106;-794.4743,832.7117;Inherit;False;Property;_RampUVScale;Ramp UV Scale;16;0;Create;True;0;0;False;0;False;24.4;-0.95;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;41;-1724.799,54.73954;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TransformDirectionNode;172;-803.9325,547.1687;Inherit;True;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ClampOpNode;218;-967.4573,1131.424;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;55;-864.2166,-813.8594;Inherit;False;865.924;714.2354;Comment;6;45;46;47;29;53;54;Back Faces;1,1,1,1;0;0
Node;AmplifyShaderEditor.DotProductOpNode;20;-1466.548,149.8606;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;142;-491.4043,758.345;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;45;-814.2166,-575.4326;Inherit;True;Property;_BackFacesAlbedo;Back Faces Albedo;5;0;Create;True;0;0;False;0;False;-1;None;83c1d98296efe1c4a8ac0af170378b1f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;42;-1726.875,-565.9415;Inherit;True;Property;_FrontFacesAlbedo;Front Faces Albedo;2;0;Create;True;0;0;False;0;False;-1;None;71437da5deb21644682305b4eded2069;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;28;-1708.367,-761.7521;Float;False;Property;_FrontFacesColor;Front Faces Color;1;1;[HDR];Create;True;0;0;False;0;False;1,0,0,0;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-246.5929,1038.032;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SignOpNode;22;-1298.996,161.4731;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;29;-772.7956,-763.8594;Float;False;Property;_BackFacesColor;Back Faces Color;4;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;2.828427,2.828427,2.828427,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCRemapNode;23;-1136.493,143.3126;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-500.4238,-616.3195;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-1358.749,-630.0837;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-940.3336,989.379;Float;False;Property;_AmbientLightParametersPower;Ambient Light Parameters Power;19;0;Create;True;0;0;False;0;False;0.5;1.51;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;110;68.84894,1204.437;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-870.629,1468.525;Float;False;Property;_LightParametersPower;Light Parameters Power;18;0;Create;True;0;0;False;0;False;0.5;2.450001;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-914.667,139.6586;Float;False;FaceSign;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;197;-632.7239,968.7781;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-1157.953,-630.0831;Float;False;FrontFacesAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;117;272.1323,1540.306;Inherit;False;Property;_RampAndCubemapLerp;Ramp And Cubemap Lerp;17;0;Create;True;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;99;242.8372,1177.63;Inherit;True;Property;_RampTex;RampTex;14;0;Create;True;0;0;False;0;False;-1;None;0c71dde550a5c5743aac28af5620d411;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;53;-819.411,-325.8131;Inherit;True;Property;_BackFacesNormal;Back Faces Normal;6;0;Create;True;0;0;False;0;False;-1;None;51d19367059fb964b8b95b52edda653f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;152;-550.6585,1398.401;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;-257.0097,-614.5519;Float;False;BackFacesAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;141.1735,-548.2174;Inherit;False;44;FrontFacesAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;57;-614.5056,32.69087;Inherit;False;626.0693;280;Comment;2;56;27;Opacity Mask;1,1,1,1;0;0
Node;AmplifyShaderEditor.ColorNode;151;823.0829,1468.287;Inherit;False;Property;_RampColor;RampColor;15;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;0.7490196,0.7490196,0.7490196,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-248.4384,1394.032;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;164.2536,-450.2709;Inherit;False;47;BackFacesAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;310.1458,-359.425;Inherit;False;48;FaceSign;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldReflectionVector;202;-44.01163,468.5547;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.LerpOp;130;701.0878,1135.302;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;27;-564.5056,82.69086;Inherit;True;Property;_OpacityMask;Opacity Mask;8;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;54;-430.3735,-316.0782;Float;False;BackFacesNormal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;219;70.31248,1343.364;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;24;698.1185,-422.5713;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;132;1048.387,1140.257;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;196;184.5203,437.7863;Inherit;True;Property;_TextureSample0;Texture Sample 0;13;0;Create;True;0;0;False;0;False;67;None;None;True;0;False;white;Auto;False;Instance;67;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;205;703.7949,737.0381;Inherit;False;Property;_BackLightScale;Back Light Scale;7;0;Create;True;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-1358.372,-334.5151;Float;False;FrontFacesNormal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;187;1122.278,670.6543;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;1,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;76;410.471,341.6598;Float;False;Property;_AOParameters;AOParameters;12;0;Create;True;0;0;False;0;False;0.5;0.148;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;56;-222.4362,170.9077;Float;False;OpacityMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;119.0541,-269.2288;Inherit;False;51;FrontFacesNormal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;74;323.091,108.7893;Inherit;True;Property;_AO;AO;11;0;Create;True;0;0;False;0;False;74;None;8624829b76b9cc74cae619387b8ad273;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;63;117.0938,-175.0307;Inherit;False;54;BackFacesNormal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;146;1552.747,1449.923;Inherit;False;48;FaceSign;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;65;143.0204,-84.28967;Inherit;False;48;FaceSign;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;174;1369.027,1179.859;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;183;1084.284,1397.243;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;58;633.1229,424.0071;Inherit;False;56;OpacityMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;72;421.3334,-90.61838;Float;False;Property;_metallicParameters;metallicParameters;10;0;Create;True;0;0;False;0;False;0.5;0.124;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;64;375.2734,-209.8589;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;430.6948,-0.6807989;Float;False;Property;_smoothnessParameters;smoothnessParameters;9;0;Create;True;0;0;False;0;False;0.5;0.959;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;145;1715.216,1136.521;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;682.471,148.6598;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2283.252,-155.6987;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;ASESampleShaders/Community/TFHC/2 Sided_w_01;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.5;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;True;1;False;-1;255;False;-1;255;False;-1;7;False;-1;3;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;1,0.4344827,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;66;0;50;0
WireConnection;67;1;66;0
WireConnection;101;0;98;0
WireConnection;158;0;67;0
WireConnection;100;0;97;0
WireConnection;104;0;101;0
WireConnection;104;1;100;0
WireConnection;172;0;158;0
WireConnection;218;0;104;0
WireConnection;20;0;41;0
WireConnection;20;1;19;0
WireConnection;142;0;172;1
WireConnection;142;1;106;0
WireConnection;109;0;142;0
WireConnection;109;1;218;0
WireConnection;22;0;20;0
WireConnection;23;0;22;0
WireConnection;46;0;29;0
WireConnection;46;1;45;0
WireConnection;43;0;28;0
WireConnection;43;1;42;0
WireConnection;110;0;109;0
WireConnection;110;1;109;0
WireConnection;48;0;23;0
WireConnection;197;0;172;1
WireConnection;197;1;198;0
WireConnection;44;0;43;0
WireConnection;99;1;110;0
WireConnection;152;0;218;0
WireConnection;152;1;69;0
WireConnection;47;0;46;0
WireConnection;186;0;197;0
WireConnection;186;1;152;0
WireConnection;202;0;53;0
WireConnection;130;0;67;0
WireConnection;130;1;99;0
WireConnection;130;2;117;0
WireConnection;54;0;53;0
WireConnection;219;0;186;0
WireConnection;24;0;59;0
WireConnection;24;1;60;0
WireConnection;24;2;61;0
WireConnection;132;0;130;0
WireConnection;132;1;151;0
WireConnection;196;1;202;0
WireConnection;51;0;50;0
WireConnection;187;0;24;0
WireConnection;187;1;196;0
WireConnection;187;2;205;0
WireConnection;56;0;27;4
WireConnection;174;0;132;0
WireConnection;174;1;219;0
WireConnection;183;0;186;0
WireConnection;64;0;62;0
WireConnection;64;1;63;0
WireConnection;64;2;65;0
WireConnection;145;0;174;0
WireConnection;145;1;187;0
WireConnection;145;2;146;0
WireConnection;75;0;74;1
WireConnection;75;1;76;0
WireConnection;0;0;24;0
WireConnection;0;1;64;0
WireConnection;0;2;145;0
WireConnection;0;3;72;0
WireConnection;0;4;71;0
WireConnection;0;5;75;0
WireConnection;0;10;58;0
ASEEND*/
//CHKSM=B9507820DF47DB7268951BA5A1EBEFDF0C68B12F