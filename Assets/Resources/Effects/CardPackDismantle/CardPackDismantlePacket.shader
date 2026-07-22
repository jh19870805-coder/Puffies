/*
* Copyright (C) 2016, BingFeng Studio（冰峰工作室）.
* All rights reserved.
* 
* 文件名称：BF_Effect_EffectPacket
* 创建标识：引擎组
* 创建日期：2020/8/11
* 文件简述：
*/
Shader "BF/Effect/EffectPacket" 
{
    Properties 
	{
		[HideInInspector] _Mode ("混合模式", Float) = 0.0
		[HideInInspector] _SrcBlend ("源颜色混合比例", Float) = 5.0
		[HideInInspector] _SrcBlendAlpha ("源颜色A通道混合比例", Float) = 5.0
		[HideInInspector] _DstBlend ("混合颜色混合比例", Float) = 6.0
		[HideInInspector] _DstBlendAlpha ("混合颜色A通道混合比例", Float) = 6.0
		[HideInInspector] _CullMode ("剔除模式", Float) = 0.0
		[HideInInspector] _Cull ("剔除", Float) = 0.0
		[HideInInspector] _ZWrite ("深度写入", Float) = 0.0

		//base
        _MainTex ("主纹理00", 2D) = "white" {}
		[HDR]_TintColor("正面混合颜色", Color) = (1, 1, 1, 0.2)  
		[HDR]_BackTintColor("背面混合颜色", Color) = (0.2, 0.2, 0.2, 1)  
		[Toggle(MAIN_TEX_01_ON)] _EnableSecondaryTex ("开启主二纹理", Float) = 0
		 _SecondaryTex ("主二纹理", 2D) = "white" {}
		 _MainTexUVAni ("UV动画(XY:主纹理00，ZW：主纹理01)", Vector) = (0,0,0,0)

		//distort
		[Toggle(DISTORT_ON)] _Distort ("扭曲", Float) = 0
		_DistortTex("扭曲贴图", 2D) = "white" {}
		//_DistortFactor("扭曲强度", Range(0,0.5)) = 0.15
		_DistortFactorX("扭曲强度X", Range(0,0.5))  = 0.15
		_DistortFactorY("扭曲强度Y", Range(0,0.5) ) = 0.15
		_DistortTexUV ("UV动画(XY:扭曲纹理）", Vector) = (0,0,0,0)
		
		//dissolve
		[Toggle(DISSOLVE_ON)] _Dissolve ("溶解", Float) = 0
		_DissolveTex("溶解贴图", 2D) = "white" {}
        _DissolveFactor("溶解进度", Range(0,1.01)) = 0.5
        _DissolveEdge("溶解边缘宽度", Range(0,0.3)) = 0.1
        [HDR]_DissolveEdgeColor("溶解边缘颜色", color) = (1,1,1,1)
		_DissolveUVAnination ("溶解动画", Vector) = (0,0,0,0)
		//[Toggle(SOFT_DISSOLVE)] _SoftDissolve ("soft Dissolve", Float) = 0
        //_SoftDissolveFac ("Soft dissolve factor", Range(0.01,0.5)) = 0

		//mask
		[Toggle(MASK_ON)] _Mask ("遮罩", Float) = 0
		_MaskTex("遮罩贴图", 2D) = "white" {}
		_MaskTexUV ("UV动画(XY:遮罩纹理）", Vector) = (0,0,0,0)

		//clock clip
		[Toggle(CLOCK_CLIP_ON)] _Enable_Clock_Clip ("ClockClip", Float) = 0
		_ClockClipFactor("ClockClip强度", Range(0, 1)) = 0.5

		//rim
		[Toggle(RIM_ON)] _Rim ("边缘光", Float) = 0
		_RimPower("边缘光范围", Range(0,10)) = 5
		_RimIntensity("边缘光强度", Range(0,10)) = 1
        [HDR]_RimColor("边缘光颜色", Color) = (1,1,1,1) 
		
		//wave
		[Toggle(WAVE_ON)] _Wave("波动", Float) = 0
		_WaveLength("WaveLength", Range(0, 5)) = 1.8
		_WaveFrequency("频率", Range(0,10)) = 0.47
		_WaveAmplitude("振幅", Range(0,5)) = 1
		_WaveScale("波动起点", Range(0,1)) = 1
		[Toggle(Y_ON)] _WaveY("Y轴向波动", Float) = 0
		[Toggle(Z_ON)] _WaveZ("Z轴向波动", Float) = 0
    }

    SubShader 
	{	
        Tags {"IgnoreProjector"="True" "Queue"="Transparent" "RenderType"="Transparent" "PreviewType"="Plane"}
        Pass 
		{
            Blend [_SrcBlend] [_DstBlend], [_SrcBlendAlpha] [_DstBlendAlpha]
            ZWrite [_ZWrite]
			Cull [_Cull]
            
            CGPROGRAM
			#pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
    
			#pragma shader_feature MAIN_TEX_01_ON
			#pragma shader_feature DOUBLE_SIDE
			//#pragma shader_feature MULTIPLY_ON
			#pragma shader_feature DISTORT_ON
			#pragma shader_feature MASK_ON
			#pragma shader_feature DISSOLVE_ON
			//#pragma shader_feature SOFT_DISSOLVE
			#pragma shader_feature ADDITIVESOFT_ON
			#pragma shader_feature CLOCK_CLIP_ON
			#pragma shader_feature RIM_ON
			#pragma shader_feature WAVE_ON

			//fp16 低精度
			#pragma fragmentoption ARB_precision_hint_fastest	
			
            sampler2D _MainTex; half4 _MainTex_ST;
			half4 _MainTexUVAni;	
			#if MAIN_TEX_01_ON
				sampler2D _SecondaryTex; half4 _SecondaryTex_ST;
			#endif
			
			#if DISTORT_ON
				sampler2D _DistortTex; half4 _DistortTex_ST;
				half _DistortFactorX, _DistortFactorY;
				half2 _DistortTexUV;
			#endif
			
			#if DISSOLVE_ON
				sampler2D _DissolveTex; half4 _DissolveTex_ST;
				half4 _DissolveUVAnination, _DissolveEdgeColor;
				float _DissolveFactor; 
				float _DissolveEdge;
				//half _SoftDissolveFac;
			#endif

			#if MASK_ON
				sampler2D _MaskTex; half4 _MaskTex_ST;
				half2 _MaskTexUV;
			#endif

			#if CLOCK_CLIP_ON
				half _ClockClipFactor;
			#endif

			#if RIM_ON
				half _RimPower;
				half _RimIntensity;
				fixed4 _RimColor;
			#endif
			
			#if WAVE_ON
				half _WaveLength;
				half _WaveFrequency;
				half _WaveAmplitude;
				half _WaveScale;
				fixed _WaveY;
				fixed _WaveZ;
			#endif


			half4 _TintColor, _BackTintColor;
			
            struct a2v 
			{
                float4 vertex : POSITION;
                half4  color  : COLOR;
				half3  normal : NORMAL;
				half4  uv     : TEXCOORD0;
            };
			
            struct v2f 
			{
                float4 pos     : SV_POSITION;
				half4  color   : COLOR;
                half4  uv0     : TEXCOORD0;
				half4  uv1     : TEXCOORD1;
				half4  uv2     : TEXCOORD2;
			//	half4  uv3     : TEXCOORD5;
			#if RIM_ON
				half3  normal  : TEXCOORD3;
				half3  viewDir : TEXCOORD4;
			#endif
				UNITY_VERTEX_OUTPUT_STEREO
            };
			
            v2f vert (a2v v) 
			{
                v2f o = (v2f)0;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				
                o.pos = UnityObjectToClipPos( v.vertex );
				o.color = v.color;
	
                o.uv0.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
				o.uv0.xy += frac(half2(_MainTexUVAni.xy * _Time.y));			
			#if MAIN_TEX_01_ON
				o.uv0.zw = TRANSFORM_TEX(v.uv.xy, _SecondaryTex);
				o.uv0.zw += frac(half2(_MainTexUVAni.zw * _Time.y));
			#endif
				
			#if DISTORT_ON
				o.uv1.xy = TRANSFORM_TEX(v.uv.xy, _DistortTex);
				o.uv1.xy += frac(half2(_DistortTexUV.xy * _Time.y));	
			#endif

			#if DISSOLVE_ON
				o.uv1.zw = TRANSFORM_TEX(v.uv.xy, _DissolveTex);
			#endif

			#if MASK_ON
				o.uv2.xy = TRANSFORM_TEX(v.uv.xy, _MaskTex);
				o.uv2.xy += frac(half2(_MaskTexUV.xy * _Time.y));
			#endif

			#if CLOCK_CLIP_ON
				o.uv2.zw = v.uv.xy;
			#endif

			#if RIM_ON
				half3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.normal = UnityObjectToWorldNormal(v.normal);
				o.viewDir = normalize(_WorldSpaceCameraPos.xyz - worldPos);
			#endif
			
			#if WAVE_ON

				v.uv.x -= _WaveScale;
				v.vertex.xyz += v.uv.x * (sin(_WaveLength * v.vertex.x + _Time.y * _WaveFrequency) * _WaveAmplitude) * lerp(lerp(float3(1,0, 0), float3(0, 1, 0), _WaveY), float3(0, 0, 1), _WaveZ);
				v.uv.x += _WaveScale;

				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv0.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
			#endif
                return o;
            }
			
            half4 frag(v2f i, half facing : VFACE) : SV_Target 
			{
			#if DISTORT_ON
				half4 distortTex = tex2D(_DistortTex, i.uv1.xy);
				half2 distortXY = (distortTex.r * 2 - 1) * half2(_DistortFactorX, _DistortFactorY);
				i.uv0.xy += distortXY;
			#endif

				half4 mainTex00 = tex2D(_MainTex, i.uv0.xy);
			#if MAIN_TEX_01_ON
				half4 mainTex01 = tex2D(_SecondaryTex, i.uv0.zw);
				mainTex00 *= mainTex01;
			#endif

			#if DOUBLE_SIDE
				half facingBack = step(facing, 0);
				half4 tintColor = (1 - facingBack) * _TintColor + facingBack * _BackTintColor;
			#else
				half4 tintColor = _TintColor;
			#endif
				
				half4 color = half4(mainTex00.rgb * tintColor.rgb * i.color.rgb, mainTex00.a * tintColor.a);
				
			#if DISSOLVE_ON
				half4 dissolveTex = tex2D(_DissolveTex, i.uv1.zw);
				half dissolveProgress = max(1.01 - i.color.a, _DissolveFactor);
				//#if SOFT_DISSOLVE
				//	half dissolveArea = smoothstep(-_SoftDissolveFac, _SoftDissolveFac, (dissolveTex.r + 0.1) - _DissolveFactor * 1.2);
				//	half dissolveEdge = saturate((dissolveTex.r + 1) - _DissolveEdge * 6.667);
				//#else
					half dissolveArea = step(0, dissolveTex.r - dissolveProgress);
					half dissolveEdge = step(0, dissolveTex.r - dissolveProgress - _DissolveEdge);
				//#endif
				color *= dissolveArea;
			#else
				color.a *= i.color.a;
			#endif
				
			#if DISSOLVE_ON
				color.rgb = lerp(_DissolveEdgeColor, color.rgb, dissolveEdge);
			#endif
				
			#if CLOCK_CLIP_ON
				half2 center = i.uv2.zw - 0.5;
				half angle = 6.2832 * (0.5 - _ClockClipFactor);
				color.a *= step(0, angle + atan2(center.y, center.x));
			#endif	
				
			#if RIM_ON
				half rim = 1 - abs(dot(i.normal, i.viewDir));
				rim = pow(rim, _RimPower) * i.color.a;
				fixed3 rimColor = rim * _RimColor * _RimIntensity;
				color.rgb += rimColor;
				color.a *= rim;
			#endif

			#if ADDITIVESOFT_ON
				color.rgb *= color.a;
			#endif

			#if MASK_ON
				half4 maskTex = tex2D(_MaskTex, i.uv2.xy);
				color.a *= maskTex.r;
			#endif

                return color;
            }
            ENDCG
        }
    }

	FallBack Off
}
