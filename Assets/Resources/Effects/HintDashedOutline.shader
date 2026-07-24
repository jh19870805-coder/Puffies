Shader "Puffies/HintDashedOutline"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1, 1, 1, 1)
        _OutlineWidth ("Outline Width", Range(1, 6)) = 3
        _DashCount ("Dash Count", Range(4, 160)) = 120
        _DashFill ("Dash Fill", Range(0.1, 0.9)) = 0.85
        _ScrollSpeed ("Scroll Speed", Range(-4, 4)) = 0.85
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            struct AppData
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
                float2 localPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            float _OutlineWidth;
            float _DashCount;
            float _DashFill;
            float _ScrollSpeed;

            Varyings Vert(AppData input)
            {
                Varyings output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.color = input.color * _Color;
                output.uv = input.uv;
                output.localPosition = input.vertex.xy;
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float2 texel = _MainTex_TexelSize.xy * _OutlineWidth;
                half center = tex2D(_MainTex, input.uv).a;
                half minimum = center;
                minimum = min(minimum, tex2D(_MainTex, input.uv + float2(texel.x, 0)).a);
                minimum = min(minimum, tex2D(_MainTex, input.uv - float2(texel.x, 0)).a);
                minimum = min(minimum, tex2D(_MainTex, input.uv + float2(0, texel.y)).a);
                minimum = min(minimum, tex2D(_MainTex, input.uv - float2(0, texel.y)).a);
                minimum = min(minimum, tex2D(_MainTex, input.uv + texel).a);
                minimum = min(minimum, tex2D(_MainTex, input.uv - texel).a);
                minimum = min(minimum, tex2D(_MainTex, input.uv + float2(texel.x, -texel.y)).a);
                minimum = min(minimum, tex2D(_MainTex, input.uv + float2(-texel.x, texel.y)).a);

                half edge = saturate((center - minimum) * 4) * smoothstep(0.05, 0.35, center);
                float angle = atan2(input.localPosition.y, input.localPosition.x) / 6.2831853;
                float phase = frac((angle + 0.5) * _DashCount - _Time.y * _ScrollSpeed);
                half dash = step(1 - _DashFill, phase);

                fixed4 color = input.color;
                color.a *= edge * dash;
                clip(color.a - 0.01);
                return color;
            }
            ENDCG
        }
    }
}
