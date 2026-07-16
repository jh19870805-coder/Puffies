//  OutlineFx © NullTale - https://x.com/NullTale
Shader "Hidden/OutlineFx/Main"
{
    SubShader
    {
		Pass	// 0
        {
            name "Transparent"
		        
		    Cull Off
		    ZWrite Off
		    ZTest LEqual
        	Blend SrcAlpha OneMinusSrcAlpha
        	
            HLSLPROGRAM
            
            #include "Utils.hlsl"
            
            #pragma vertex vert_mesh
            #pragma fragment frag
            
            sampler2D _MainTex;
			float4    _Color;
            float     _Alpha;
            
            // =======================================================================
            float4 frag(fragIn i) : SV_Target
            {
            	if (tex2D(_MainTex, i.uv).a < _Alpha)
            		discard;
            	
            	return _Color;
            }
            ENDHLSL
        }
    	
        Pass	// 1
        {
            name "Outline"
        	
		    Cull Off
		    ZWrite Off
		    ZTest Off
        	Blend SrcAlpha OneMinusSrcAlpha
        	
            HLSLPROGRAM
            #include "Utils.hlsl"
            
            #pragma vertex vert_screen
            #pragma fragment frag
            
            #pragma multi_compile_local BOX CROSS
            #pragma multi_compile_local SOFT HARD
            #pragma multi_compile_local _ ALPHA_MASK
            
			#define	BLUR_LENGTH 9
			#define	BLUR_LENGTH_HALF ((BLUR_LENGTH - 1) / 2)
			static const float	k_BlurWeights[BLUR_LENGTH] =
			{
				0.046995 * 2,
				0.064759 * 2,
				0.120985 * 2,
				0.176033 * 2,
				0.199471 * 2,
				0.176033 * 2,
				0.120985 * 2,
				0.064759 * 2,
				0.046995 * 2,
			};

            sampler2D _MainTex;
            sampler2D _AlphaTex;
			float4    _AlphaTO;
			float2    _Step;
			float     _Solid;

            // =======================================================================
            float4 _sample_soft(float2 uv, in const float2 step)
            {
				float4 result = 0;
				uv -= BLUR_LENGTH_HALF * step;
            	
            	[unroll]
				for (int n = 0; n < BLUR_LENGTH; n ++)
				{
					result += tex2D(_MainTex, uv) * k_BlurWeights[n];
					uv += step;
				}
            	
            	return result;
            }
            
            float4 _sample_hard(float2 uv, in const float2 step)
            {
				float4 result = 0;
            	
				uv -= BLUR_LENGTH_HALF * step;
            	
            	[unroll]
				for (int n = 0; n < BLUR_LENGTH; n ++)
				{
					float4 sample = tex2D(_MainTex, uv);
					result = max(sample, result);
					uv += step;
				}
            	
            	return result;
            }
            
            float4 _sample(const float2 uv, in const float2 step)
            {
#ifdef SOFT
            	return _sample_soft(uv, step);
#endif
#ifdef HARD
            	return _sample_hard(uv, step);
#endif
            }
            
            float4 frag(fragIn i) : SV_Target
            {
            	float4 color = tex2D(_MainTex, i.uv);
				float4 result = 0;
				float erodedAlpha = 1;

#ifdef BOX
				const float2 stepX = float2(_Step.x, 0);
				const float2 stepY = float2(0, _Step.y);
				float2 uv = i.uv - BLUR_LENGTH_HALF * stepX;
            	
            	[unroll]
				for (int n = 0; n < BLUR_LENGTH; n ++)
				{
#ifdef SOFT
					result += _sample(uv, stepY) * k_BlurWeights[n];
#endif
#ifdef HARD
					result = max(result, _sample(uv, stepY));
#endif
					float2 erosionUv = uv - BLUR_LENGTH_HALF * stepY;
					[unroll]
					for (int m = 0; m < BLUR_LENGTH; m ++)
					{
						erodedAlpha = min(erodedAlpha, tex2D(_MainTex, erosionUv).a);
						erosionUv += stepY;
					}
					
					uv += stepX;
				}
#endif            	
#ifdef CROSS
				result = (_sample(i.uv, _Step) + _sample(i.uv, float2(_Step.x, -_Step.y))) * .5f;
				[unroll]
				for (int n = -BLUR_LENGTH_HALF; n <= BLUR_LENGTH_HALF; n ++)
				{
					erodedAlpha = min(erodedAlpha, tex2D(_MainTex, i.uv + _Step * n).a);
					erodedAlpha = min(erodedAlpha, tex2D(_MainTex, i.uv + float2(_Step.x, -_Step.y) * n).a);
				}
#endif

				float solidAlpha = color.a * _Solid;
#ifdef ALPHA_MASK
				solidAlpha *= tex2D(_AlphaTex, mad(i.uv, _AlphaTO.xy, _AlphaTO.zw)).a;
#endif
				return float4(result.rgb, max(result.a - erodedAlpha, solidAlpha));
            }
            ENDHLSL
        }
    	
    	Pass	// 2
        {
            name "Overlay"
        	
		    Cull Off
		    ZWrite Off
		    ZTest Off
        	Blend SrcAlpha OneMinusSrcAlpha
        	
            HLSLPROGRAM
            #include "Utils.hlsl"
            
            #pragma vertex vert_screen
            #pragma fragment frag
            
            sampler2D _MainTex;

            // =======================================================================
            float4 frag(fragIn i) : SV_Target
            {
            	return tex2D(_MainTex, i.uv);
            }
            ENDHLSL
        }

        Pass   // 3: horizontal dilation before closing mask gaps
        {
            name "DilateMaskHorizontal"

            Cull Off
            ZWrite Off
            ZTest Off
            Blend One Zero

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_screen
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _GapStep;

            float4 frag(fragIn i) : SV_Target
            {
                float4 result = 0;
                [unroll]
                for (int offset = -3; offset <= 3; offset++)
                    result = max(result, tex2D(_MainTex, i.uv + float2(_GapStep.x * offset, 0)));
                return result;
            }
            ENDHLSL
        }

        Pass   // 4: vertical dilation before closing mask gaps
        {
            name "DilateMaskVertical"

            Cull Off
            ZWrite Off
            ZTest Off
            Blend One Zero

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_screen
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _GapStep;

            float4 frag(fragIn i) : SV_Target
            {
                float4 result = 0;
                [unroll]
                for (int offset = -3; offset <= 3; offset++)
                    result = max(result, tex2D(_MainTex, i.uv + float2(0, _GapStep.y * offset)));
                return result;
            }
            ENDHLSL
        }

        Pass   // 5: horizontal erosion restores the original outer extent
        {
            name "ErodeMaskHorizontal"

            Cull Off
            ZWrite Off
            ZTest Off
            Blend One Zero

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_screen
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _GapStep;

            float4 frag(fragIn i) : SV_Target
            {
                float4 center = tex2D(_MainTex, i.uv);
                float alpha = 1;
                [unroll]
                for (int offset = -3; offset <= 3; offset++)
                    alpha = min(alpha, tex2D(_MainTex, i.uv + float2(_GapStep.x * offset, 0)).a);
                return float4(center.rgb, alpha);
            }
            ENDHLSL
        }

        Pass   // 6: vertical erosion restores the original outer extent
        {
            name "ErodeMaskVertical"

            Cull Off
            ZWrite Off
            ZTest Off
            Blend One Zero

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_screen
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _GapStep;

            float4 frag(fragIn i) : SV_Target
            {
                float4 center = tex2D(_MainTex, i.uv);
                float alpha = 1;
                [unroll]
                for (int offset = -3; offset <= 3; offset++)
                    alpha = min(alpha, tex2D(_MainTex, i.uv + float2(0, _GapStep.y * offset)).a);
                return float4(center.rgb, alpha);
            }
            ENDHLSL
        }

        Pass   // 7: expand the closed mask horizontally
        {
            name "ExpandMaskHorizontal"

            Cull Off
            ZWrite Off
            ZTest Off
            Blend One Zero

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_screen
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _GapStep;

            float4 frag(fragIn i) : SV_Target
            {
                float4 result = 0;
                [unroll]
                for (int offset = -3; offset <= 3; offset++)
                    result = max(result, tex2D(_MainTex, i.uv + float2(_GapStep.x * offset, 0)));
                return result;
            }
            ENDHLSL
        }

        Pass   // 8: expand the closed mask vertically
        {
            name "ExpandMaskVertical"

            Cull Off
            ZWrite Off
            ZTest Off
            Blend One Zero

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_screen
            #pragma fragment frag

            sampler2D _MainTex;
            float2 _GapStep;

            float4 frag(fragIn i) : SV_Target
            {
                float4 result = 0;
                [unroll]
                for (int offset = -3; offset <= 3; offset++)
                    result = max(result, tex2D(_MainTex, i.uv + float2(0, _GapStep.y * offset)));
                return result;
            }
            ENDHLSL
        }

        Pass   // 9: keep only centered active outline pixels on the expanded full-puzzle exterior
        {
            name "MaskInteriorOutline"

            Cull Off
            ZWrite Off
            ZTest Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #include "Utils.hlsl"

            #pragma vertex vert_screen
            #pragma fragment frag

			#define BLUR_LENGTH 9
			#define BLUR_LENGTH_HALF ((BLUR_LENGTH - 1) / 2)

            sampler2D _MainTex;
            sampler2D _MaskTex;
			float2 _Step;

            float4 frag(fragIn i) : SV_Target
            {
				float4 outline = tex2D(_MainTex, i.uv);
				float minOccupied = 1;
				float maxOccupied = 0;
				[unroll]
				for (int x = -BLUR_LENGTH_HALF; x <= BLUR_LENGTH_HALF; x ++)
				{
					[unroll]
					for (int y = -BLUR_LENGTH_HALF; y <= BLUR_LENGTH_HALF; y ++)
					{
						float occupied = tex2D(_MaskTex, i.uv + float2(_Step.x * x, _Step.y * y)).a;
						minOccupied = min(minOccupied, occupied);
						maxOccupied = max(maxOccupied, occupied);
					}
				}

				bool isPuzzleEdge = maxOccupied > 0.001 && minOccupied < 0.999;
				return float4(outline.rgb, isPuzzleEdge ? outline.a : 0);
            }
            ENDHLSL
        }
    }
}
