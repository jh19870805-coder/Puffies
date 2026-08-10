Shader "Puffies/UI/PuzzlePlacementShine"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _ShineColor ("Shine Color", Color) = (1,0.96,0.6,0.9)
        _SweepAxis ("Sweep Axis", Vector) = (-0.58,0.82,0,0)
        _SweepCenter ("Sweep Center", Float) = 0
        _BandWidth ("Band Width", Float) = 0.045
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
        ColorMask RGB

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
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 localPosition : TEXCOORD1;
                float4 screenPosition : TEXCOORD2;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _ShineColor;
            float4 _SweepAxis;
            float _SweepCenter;
            float _BandWidth;
            float4 _ClipRect;

            v2f vert(appdata_t input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.localPosition = input.vertex;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.screenPosition = ComputeScreenPos(output.vertex);
                output.texcoord = input.texcoord;
                output.color = input.color * _Color;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed alpha = tex2D(_MainTex, input.texcoord).a * input.color.a;
                float2 screenUv = input.screenPosition.xy / input.screenPosition.w;
                float2 sweepAxis = normalize(_SweepAxis.xy);
                float axisPosition = dot(screenUv, sweepAxis);
                float distanceToBand = abs(axisPosition - _SweepCenter);
                float bandWidth = max(_BandWidth, 0.0001);
                float core = 1.0 - smoothstep(bandWidth * 0.12, bandWidth, distanceToBand);
                float glow = 1.0 - smoothstep(bandWidth, bandWidth * 2.6, distanceToBand);
                float shine = saturate(core + glow * 0.38);
                fixed additive = alpha * _ShineColor.a * shine;
                fixed4 color = fixed4(
                    _ShineColor.rgb * (1.0 + core * 0.25) * additive,
                    0.0);

                #ifdef UNITY_UI_CLIP_RECT
                color.rgb *= UnityGet2DClipping(input.localPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(additive - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
