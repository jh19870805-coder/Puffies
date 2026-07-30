Shader "Puffies/CardPack List Unlit"
{
    Properties
    {
        _Cutoff("Mask Clip Value", Float) = 0.35
        _FrontFacesColor("Front Faces Color", Color) = (1, 1, 1, 1)
        _FrontFacesAlbedo("Front Faces Albedo", 2D) = "white" {}
        _BackFacesColor("Back Faces Color", Color) = (1, 1, 1, 1)
        _BackFacesAlbedo("Back Faces Albedo", 2D) = "white" {}
        _ClipTex("Clip Texture", 2D) = "white" {}
        [Enum(UnityEngine.Rendering.CompareFunction)] _DepthTest("Depth Test", Float) = 4
        [HideInInspector] _UiClipRect("UI Clip Rect", Vector) = (0, 0, 0, 0)
        [HideInInspector] _UseUiClipRect("Use UI Clip Rect", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "AlphaTest"
            "RenderType" = "TransparentCutout"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Cull Off
            ZWrite On
            ZTest [_DepthTest]

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _FrontFacesAlbedo;
            float4 _FrontFacesAlbedo_ST;
            float4 _FrontFacesColor;
            sampler2D _BackFacesAlbedo;
            float4 _BackFacesAlbedo_ST;
            float4 _BackFacesColor;
            sampler2D _ClipTex;
            float4 _ClipTex_ST;
            float _Cutoff;
            float4 _UiClipRect;
            float _UseUiClipRect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPosition : TEXCOORD1;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.screenPosition = ComputeScreenPos(output.vertex);
                return output;
            }

            fixed4 frag(v2f input, fixed facing : VFACE) : SV_Target
            {
                if (_UseUiClipRect > 0.5)
                {
                    float2 screenPixel = input.screenPosition.xy
                        / input.screenPosition.w * _ScreenParams.xy;
                    clip(screenPixel - _UiClipRect.xy);
                    clip(_UiClipRect.zw - screenPixel);
                }

                float2 clipUv = input.uv * _ClipTex_ST.xy + _ClipTex_ST.zw;
                clip(tex2D(_ClipTex, clipUv).r - _Cutoff);

                float2 frontUv = input.uv * _FrontFacesAlbedo_ST.xy
                    + _FrontFacesAlbedo_ST.zw;
                float2 backUv = input.uv * _BackFacesAlbedo_ST.xy
                    + _BackFacesAlbedo_ST.zw;
                fixed4 frontColor = tex2D(_FrontFacesAlbedo, frontUv) * _FrontFacesColor;
                fixed4 backColor = tex2D(_BackFacesAlbedo, backUv) * _BackFacesColor;
                fixed4 color = facing >= 0 ? frontColor : backColor;
                clip(color.a - 0.001);
                return fixed4(color.rgb, 1);
            }
            ENDCG
        }
    }

    Fallback "Unlit/Transparent Cutout"
}
