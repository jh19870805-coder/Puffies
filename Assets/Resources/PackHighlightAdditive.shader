Shader "Puffies/UI/PackHighlightAdditive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend One One
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float4 _ClipRect;

            fixed4 SamplePremultiplied(float2 uv)
            {
                fixed4 sampleColor = tex2D(_MainTex, uv);
                sampleColor.rgb *= sampleColor.a;
                return sampleColor;
            }

            fixed4 SampleSoftLight(float2 uv)
            {
                float2 offset = _MainTex_TexelSize.xy * 1.25;
                fixed4 color = SamplePremultiplied(uv) * 4.0;
                color += SamplePremultiplied(uv + float2(offset.x, 0.0)) * 2.0;
                color += SamplePremultiplied(uv - float2(offset.x, 0.0)) * 2.0;
                color += SamplePremultiplied(uv + float2(0.0, offset.y)) * 2.0;
                color += SamplePremultiplied(uv - float2(0.0, offset.y)) * 2.0;
                color += SamplePremultiplied(uv + offset);
                color += SamplePremultiplied(uv - offset);
                color += SamplePremultiplied(uv + float2(offset.x, -offset.y));
                color += SamplePremultiplied(uv + float2(-offset.x, offset.y));
                color *= 0.0625;

                float2 edgeDistance = min(uv, 1.0 - uv) / _MainTex_TexelSize.xy;
                float edgeFade = smoothstep(0.0, 3.0, min(edgeDistance.x, edgeDistance.y));
                return color * edgeFade;
            }

            v2f vert(appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.localPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 textureColor = SampleSoftLight(input.texcoord);
                fixed4 color = textureColor;
                color.rgb *= input.color.rgb * input.color.a;
                color.a = 0.0;

                #ifdef UNITY_UI_CLIP_RECT
                color.rgb *= UnityGet2DClipping(input.localPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(textureColor.a * input.color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
