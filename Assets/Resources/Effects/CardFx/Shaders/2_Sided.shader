// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
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
		_SmoothnessParameters("SmoothnessParameters", Range( 0 , 1)) = 0.5
		_MetallicParameters("MetallicParameters", Range( 0 , 1)) = 0.5
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
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "AlphaTest+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" "IsEmissive" = "true"  }
		Cull Off
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
		uniform float _MetallicParameters;
		uniform float _SmoothnessParameters;
		uniform sampler2D _OcclusionTex;
		uniform float4 _OcclusionTex_ST;
		uniform float _OcclutionValue;
		uniform sampler2D _ClipTex;
		SamplerState sampler_ClipTex;
		uniform float4 _ClipTex_ST;
		uniform float _Cutoff = 0.35;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
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
			o.Metallic = _MetallicParameters;
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
/*ASEBEGIN
Version=18500
1204;123;1575;827;172.5415;-1089.319;1;True;True
Node;AmplifyShaderEditor.CommentaryNode;52;-1776.875,-811.7521;Inherit;False;870.9222;707.2373;Comment;6;43;44;28;42;50;51;Front Faces;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;230;-2105.739,653.4383;Inherit;False;Property;_CubeMapPosition;CubeMapPosition;11;0;Create;True;0;0;False;0;False;0,0,0;0.2,0.32,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;50;-1736.404,-341.5151;Inherit;True;Property;_FrontFacesNormal;Front Faces Normal;3;0;Create;True;0;0;False;0;False;-1;None;f728ccca48b0c5a4682cdfde2c56904a;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;229;-1888.515,523.6193;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldReflectionVector;66;-1677.113,486.2811;Inherit;False;True;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;67;-1376.441,516.0869;Inherit;True;Property;_CubeMap;CubeMap;10;0;Create;True;0;0;False;0;False;-1;None;5d83cf49ac003d54686219ce46968ceb;True;0;False;white;Auto;False;Object;-1;Auto;Cube;8;0;SAMPLERCUBE;0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;1;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldNormalVector;98;-1714.395,952.5137;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;97;-1726.789,1186.296;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.NormalizeNode;101;-1437.738,1018.878;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;158;-1078.765,590.0549;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NormalizeNode;100;-1431.388,1201.685;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TransformDirectionNode;172;-752.8279,588.4002;Inherit;True;Object;World;False;Fast;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;104;-1172.578,1114.527;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;106;-943.0554,839.1265;Inherit;False;Property;_RampUVScale;RampUVScale;16;0;Create;True;0;0;False;0;False;24.4;-0.55;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;49;-1774.799,4.739527;Inherit;False;1094.131;402.4268;Comment;6;20;22;23;48;19;41;Face Sign (0 = Front, 1 = Back);1,1,1,1;0;0
Node;AmplifyShaderEditor.ClampOpNode;218;-985.2316,1244.534;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;142;-456.5054,759.5779;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;19;-1699.579,223.1664;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;109;-246.5929,1038.032;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;41;-1724.799,54.73954;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;55;-864.2166,-813.8594;Inherit;False;865.924;714.2354;Comment;6;45;46;47;29;53;54;Back Faces;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;110;60.4047,1162.216;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-870.629,1467.525;Float;False;Property;_LightParametersPower;LightParametersPower;18;0;Create;True;0;0;False;0;False;0.5;2.69;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;198;-950.7089,1014.801;Float;False;Property;_AmbientLightPower;AmbientLightPower;17;0;Create;True;0;0;False;0;False;0.5;0.6;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DotProductOpNode;20;-1466.548,149.8606;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;29;-772.7956,-763.8594;Float;False;Property;_BackFacesColor;Back Faces Color;4;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;28;-1708.367,-761.7521;Float;False;Property;_FrontFacesColor;Front Faces Color;1;1;[HDR];Create;True;0;0;False;0;False;1,0,0,0;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;45;-814.2166,-575.4326;Inherit;True;Property;_BackFacesAlbedo;Back Faces Albedo;5;0;Create;True;0;0;False;0;False;-1;None;83c1d98296efe1c4a8ac0af170378b1f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;42;-1726.875,-565.9415;Inherit;True;Property;_FrontFacesAlbedo;Front Faces Albedo;2;0;Create;True;0;0;False;0;False;-1;None;a2142fc0e36dbad46b91883b6e70c4c0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SignOpNode;22;-1298.996,161.4731;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;99;284.2517,1133.526;Inherit;True;Property;_RampTex;RampTex;14;0;Create;True;0;0;False;0;False;-1;None;0c71dde550a5c5743aac28af5620d411;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;228;-193.4628,738.2912;Inherit;True;Property;_CubemapPower;CubemapPower;13;0;Create;True;0;0;False;0;False;0;1.52;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;197;-631.8405,971.8245;Inherit;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;53;-819.411,-325.8131;Inherit;True;Property;_BackFacesNormal;Back Faces Normal;6;0;Create;True;0;0;False;0;False;-1;None;62bb87dca07951d46bfb2a96432d912a;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;152;-550.6585,1398.401;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;151;414.027,1461.949;Inherit;False;Property;_RampColor;RampColor;15;1;[HDR];Create;True;0;0;False;0;False;0,0,0,0;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldReflectionVector;202;-12.18661,374.3381;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;46;-500.4238,-616.3195;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;43;-1358.749,-630.0837;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;23;-1136.493,143.3126;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-1;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;186;-259.4224,1378.929;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;132;743.6895,1141.66;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;224;757.7506,1470.737;Inherit;False;Property;_RampTexValue;RampTexValue;19;0;Create;True;0;0;False;0;False;0;0.487;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;227;137.0023,636.085;Inherit;False;False;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;225;247.3722,890.0875;Inherit;True;Property;_CubemapValue;CubemapValue;12;0;Create;True;0;0;False;0;False;0;0.179;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;44;-1157.953,-630.0831;Float;False;FrontFacesAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;196;295.7458,382.8044;Inherit;True;Global;TextureSample0;Texture Sample 0;10;0;Create;True;0;0;False;0;False;67;None;None;True;0;False;white;Auto;False;Instance;67;Auto;Cube;8;0;SAMPLERCUBE;;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;48;-914.667,139.6586;Float;False;FaceSign;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;54;-467.0463,-344.288;Float;False;BackFacesNormal;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;-257.0097,-614.5519;Float;False;BackFacesAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;174;1115.779,1138.26;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-1358.372,-334.5151;Float;False;FrontFacesNormal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;205;721.3438,584.7562;Inherit;False;Property;_BackLightScale;Back Light Scale;7;0;Create;True;0;0;False;0;False;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;226;575.6662,743.4603;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;117.0938,-175.0307;Inherit;False;54;BackFacesNormal;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;61;373.1781,-426.3968;Inherit;False;48;FaceSign;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;187;1006.539,564.7241;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;146;1405.266,911.1301;Inherit;False;48;FaceSign;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;275.1171,-867.3183;Inherit;False;44;FrontFacesAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;103.296,-403.1723;Inherit;False;51;FrontFacesNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;231;1500.889,308.3454;Inherit;True;Property;_OcclusionTex;OcclusionTex;21;0;Create;True;0;0;False;0;False;-1;None;9554619d8e2d0fd41aa966bd06d82093;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;65;100.8384,45.50126;Inherit;False;48;FaceSign;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;233;1813.109,473.7595;Inherit;False;Property;_OcclutionValue;OcclutionValue;22;0;Create;True;0;0;False;0;False;0;1.2;0;1.5;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;221;1291.565,786.4508;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;60;183.9512,-647.2467;Inherit;False;47;BackFacesAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;72;1066.212,-50.46007;Float;False;Property;_MetallicParameters;MetallicParameters;9;0;Create;True;0;0;False;0;False;0.5;0.728;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;220;1108.829,274.1527;Inherit;True;Property;_ClipTex;ClipTex;20;0;Create;True;0;0;False;0;False;-1;None;6af09fac80524fa4b8249aa88d84fc2e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;24;920.3965,-466.1103;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;64;442.7174,-224.9795;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;145;1593.936,558.0706;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;71;1124.634,119.3479;Float;False;Property;_SmoothnessParameters;SmoothnessParameters;8;0;Create;True;0;0;False;0;False;0.5;0.398;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;232;1886.109,303.7595;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2022.998,-103.8552;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;Puffies/2_Sided;False;False;False;False;False;False;False;False;False;False;False;False;True;False;True;True;True;True;True;True;True;Off;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Masked;0.35;True;True;0;False;TransparentCutout;;AlphaTest;All;14;all;True;True;True;True;0;False;-1;True;1;False;-1;255;False;-1;255;False;-1;7;False;-1;3;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;0;4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;1;False;-1;1;False;-1;0;False;0;1,0.4344827,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.CommentaryNode;57;-614.5056,32.69087;Inherit;False;626.0693;280;Comment;0;Opacity Mask;1,1,1,1;0;0
WireConnection;229;0;50;0
WireConnection;229;1;230;0
WireConnection;66;0;229;0
WireConnection;67;1;66;0
WireConnection;101;0;98;0
WireConnection;158;0;67;0
WireConnection;100;0;97;0
WireConnection;172;0;158;0
WireConnection;104;0;101;0
WireConnection;104;1;100;0
WireConnection;218;0;104;0
WireConnection;142;0;172;1
WireConnection;142;1;106;0
WireConnection;109;0;142;0
WireConnection;109;1;218;0
WireConnection;110;0;109;0
WireConnection;110;1;109;0
WireConnection;20;0;41;0
WireConnection;20;1;19;0
WireConnection;22;0;20;0
WireConnection;99;1;110;0
WireConnection;197;0;158;0
WireConnection;197;1;198;0
WireConnection;152;0;218;0
WireConnection;152;1;69;0
WireConnection;202;0;53;0
WireConnection;46;0;29;0
WireConnection;46;1;45;0
WireConnection;43;0;28;0
WireConnection;43;1;42;0
WireConnection;23;0;22;0
WireConnection;186;0;197;0
WireConnection;186;1;152;0
WireConnection;132;0;99;0
WireConnection;132;1;151;0
WireConnection;227;0;67;0
WireConnection;227;1;228;0
WireConnection;44;0;43;0
WireConnection;196;1;202;0
WireConnection;48;0;23;0
WireConnection;54;0;53;0
WireConnection;47;0;46;0
WireConnection;174;0;132;0
WireConnection;174;1;186;0
WireConnection;174;2;224;0
WireConnection;51;0;50;0
WireConnection;226;0;227;0
WireConnection;226;1;225;0
WireConnection;187;0;196;0
WireConnection;187;1;205;0
WireConnection;221;0;226;0
WireConnection;221;1;174;0
WireConnection;24;0;59;0
WireConnection;24;1;60;0
WireConnection;24;2;61;0
WireConnection;64;0;62;0
WireConnection;64;1;63;0
WireConnection;64;2;65;0
WireConnection;145;0;221;0
WireConnection;145;1;187;0
WireConnection;145;2;146;0
WireConnection;232;0;231;0
WireConnection;232;1;233;0
WireConnection;0;0;24;0
WireConnection;0;1;64;0
WireConnection;0;2;145;0
WireConnection;0;3;72;0
WireConnection;0;4;71;0
WireConnection;0;5;232;0
WireConnection;0;10;220;1
ASEEND*/
//CHKSM=7078505FB4401FB8BF0D612991E444A2E930D123