Shader "Hidden/Puffies/BagSelectGaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SampleScale ("Sample Scale", Float) = 1
        _BlurDirection ("Blur Direction", Vector) = (1, 0, 0, 0)
        _ConvertOutputToLinear ("Convert Output To Linear", Float) = 0
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert_img
            #pragma fragment frag

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float2 _BlurDirection;
            float _SampleScale;
            float _ConvertOutputToLinear;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 offset =
                    _MainTex_TexelSize.xy * _BlurDirection * _SampleScale;
                fixed4 color = tex2D(_MainTex, input.uv) * 0.103152619;
                color += tex2D(_MainTex, input.uv + offset * 1.476579651) * 0.191010813;
                color += tex2D(_MainTex, input.uv - offset * 1.476579651) * 0.191010813;
                color += tex2D(_MainTex, input.uv + offset * 3.445529535) * 0.140428908;
                color += tex2D(_MainTex, input.uv - offset * 3.445529535) * 0.140428908;
                color += tex2D(_MainTex, input.uv + offset * 5.414898846) * 0.080715463;
                color += tex2D(_MainTex, input.uv - offset * 5.414898846) * 0.080715463;
                color += tex2D(_MainTex, input.uv + offset * 7.384912144) * 0.036268507;
                color += tex2D(_MainTex, input.uv - offset * 7.384912144) * 0.036268507;
#ifndef UNITY_COLORSPACE_GAMMA
                if (_ConvertOutputToLinear > 0.5)
                {
                    color.rgb = GammaToLinearSpace(color.rgb);
                }
#endif
                return color;
            }
            ENDCG
        }
    }

    Fallback Off
}
