Shader "Puffies/Sprites/PuzzlePieceLightAdditive"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        Blend One One
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnitySprites.cginc"

            float4 _MainTex_TexelSize;

            fixed4 SamplePremultiplied(float2 uv)
            {
                fixed4 sampleColor = SampleSpriteTexture(uv);
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

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = SampleSoftLight(IN.texcoord);
                color.rgb *= IN.color.rgb * IN.color.a;
                color.a = 0.0;
                return color;
            }
            ENDCG
        }
    }
}
