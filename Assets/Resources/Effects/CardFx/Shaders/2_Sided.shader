Shader "Puffies/2_Sided"
{
	Properties
	{
		_Cutoff( "Mask Clip Value", Float ) = 0.35
		[HDR]_FrontFacesColor("Front Faces Color", Color) = (1,0,0,0)
		_FrontFacesAlbedo("Front Faces Albedo", 2D) = "white" {}
		_FrontFacesNormal("Front Faces Normal", 2D) = "bump" {}
		[HDR]_BackFacesColor("Back Faces Color", Color) = (1,1,1,1)
		_BackFacesAlbedo("Back Faces Albedo", 2D) = "white" {}
		_BackFacesNormal("Back Faces Normal", 2D) = "white" {}
		_BackLightScale("Back Light Scale", Float) = 0
		_CubeMap("CubeMap", CUBE) = "white" {}
		_CubeMapPosition("CubeMapPosition", Vector) = (0,0,0,0)
		_CubemapValue("CubemapValue", Range( 0 , 1)) = 0
		_CubemapPower("CubemapPower", Range( 0 , 10)) = 0
		_RampTex("RampTex", 2D) = "white" {}
		[HDR]_RampColor("RampColor", Color) = (0,0,0,0)
		_RampUVScale("RampUVScale", Range( -1 , 1)) = 24.4
		_AmbientLightPower("AmbientLightPower", Range( 0 , 5)) = 0.5
		_LightParametersPower("LightParametersPower", Range( 0 , 5)) = 0.5
		_RampTexValue("RampTexValue", Range( 0 , 1)) = 0
		_ClipTex("ClipTex", 2D) = "white" {}
		_OcclusionTex("OcclusionTex", 2D) = "white" {}
		_OcclutionValue("OcclutionValue", Range( 0 , 1.5)) = 0
		_MetallicTex("MetallicTex", 2D) = "white" {}
		_MetallicPower("MetallicPower", Range( 0 , 2)) = 0.5
		_MetallicScale("MetallicScale", Range( 0 , 1)) = 0.5
		_SmoothnessParameters("SmoothnessParameters", Range( 0 , 1)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
		[Enum(UnityEngine.Rendering.CompareFunction)] _DepthTest("Depth Test", Float) = 4
		[HideInInspector] _UiClipRect("UI Clip Rect", Vector) = (0,0,0,0)
		[HideInInspector] _UseUiClipRect("Use UI Clip Rect", Float) = 0
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IsEmissive" = "true"  }
		Cull Off
		ZTest [_DepthTest]
		Stencil
		{
			Ref 1
			CompFront Always
			PassFront Replace
		}
		CGPROGRAM
		#include "UnityCG.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows noinstancing dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
			float3 worldRefl;
			float4 screenPos;
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
		uniform samplerCUBE _CubeMap;
		uniform float3 _CubeMapPosition;
		uniform float _CubemapPower;
		uniform float _CubemapValue;
		uniform sampler2D _RampTex;
		uniform float _RampUVScale;
		uniform float4 _RampColor;
		uniform float _AmbientLightPower;
		uniform float _LightParametersPower;
		uniform float _RampTexValue;
		uniform float _BackLightScale;
		uniform sampler2D _MetallicTex;
		SamplerState sampler_MetallicTex;
		uniform float4 _MetallicTex_ST;
		uniform float _MetallicPower;
		uniform float _MetallicScale;
		uniform float _SmoothnessParameters;
		uniform sampler2D _OcclusionTex;
		uniform float4 _OcclusionTex_ST;
		uniform float _OcclutionValue;
		uniform sampler2D _ClipTex;
		SamplerState sampler_ClipTex;
		uniform float4 _ClipTex_ST;
		uniform float _Cutoff = 0.35;
		uniform float4 _UiClipRect;
		uniform float _UseUiClipRect;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			if (_UseUiClipRect > 0.5)
			{
				float2 screenPixel = i.screenPos.xy / max(i.screenPos.w, 0.00001) * _ScreenParams.xy;
				clip(screenPixel - _UiClipRect.xy);
				clip(_UiClipRect.zw - screenPixel);
			}

			float2 uv_FrontFacesNormal = i.uv_texcoord * _FrontFacesNormal_ST.xy + _FrontFacesNormal_ST.zw;
			float3 tex2DNode50 = UnpackNormal( tex2D( _FrontFacesNormal, uv_FrontFacesNormal ) );
			float3 FrontFacesNormal51 = tex2DNode50;
			float2 uv_BackFacesNormal = i.uv_texcoord * _BackFacesNormal_ST.xy + _BackFacesNormal_ST.zw;
			float4 tex2DNode53 = tex2D( _BackFacesNormal, uv_BackFacesNormal );
			float4 BackFacesNormal54 = tex2DNode53;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float dotResult20 = dot( ase_worldNormal , ase_worldViewDir );
			float FaceSign48 = (1.0 + (sign( dotResult20 ) - -1.0) * (0.0 - 1.0) / (1.0 - -1.0));
			float4 lerpResult64 = lerp( float4( FrontFacesNormal51 , 0.0 ) , BackFacesNormal54 , FaceSign48);
			o.Normal = lerpResult64.rgb;
			float2 uv_FrontFacesAlbedo = i.uv_texcoord * _FrontFacesAlbedo_ST.xy + _FrontFacesAlbedo_ST.zw;
			float4 FrontFacesAlbedo44 = ( _FrontFacesColor * tex2D( _FrontFacesAlbedo, uv_FrontFacesAlbedo ) );
			float2 uv_BackFacesAlbedo = i.uv_texcoord * _BackFacesAlbedo_ST.xy + _BackFacesAlbedo_ST.zw;
			float4 BackFacesAlbedo47 = ( _BackFacesColor * tex2D( _BackFacesAlbedo, uv_BackFacesAlbedo ) );
			float4 lerpResult24 = lerp( FrontFacesAlbedo44 , BackFacesAlbedo47 , FaceSign48);
			o.Albedo = lerpResult24.rgb;
			float4 texCUBENode67 = texCUBE( _CubeMap, normalize( WorldReflectionVector( i , ( tex2DNode50 + _CubeMapPosition ) ) ) );
			float4 temp_cast_3 = (_CubemapPower).xxxx;
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
			float3 temp_cast_6 = (_AmbientLightPower).xxx;
			float4 lerpResult145 = lerp( ( ( pow( texCUBENode67 , temp_cast_3 ) * _CubemapValue ) + ( ( tex2D( _RampTex, appendResult110.xy ) * _RampColor ) * float4( ( pow( desaturateVar158 , temp_cast_6 ) * pow( clampResult218 , _LightParametersPower ) ) , 0.0 ) * _RampTexValue ) ) , ( texCUBE( _CubeMap, WorldReflectionVector( i , tex2DNode53.rgb ) ) * _BackLightScale ) , FaceSign48);
			o.Emission = lerpResult145.rgb;
			float2 uv_MetallicTex = i.uv_texcoord * _MetallicTex_ST.xy + _MetallicTex_ST.zw;
			o.Metallic = ( pow( tex2D( _MetallicTex, uv_MetallicTex ).r , _MetallicPower ) * _MetallicScale );
			o.Smoothness = _SmoothnessParameters;
			float2 uv_OcclusionTex = i.uv_texcoord * _OcclusionTex_ST.xy + _OcclusionTex_ST.zw;
			o.Occlusion = ( tex2D( _OcclusionTex, uv_OcclusionTex ) * _OcclutionValue ).r;
			o.Alpha = 1;
			float2 uv_ClipTex = i.uv_texcoord * _ClipTex_ST.xy + _ClipTex_ST.zw;
			clip( tex2D( _ClipTex, uv_ClipTex ).r - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
