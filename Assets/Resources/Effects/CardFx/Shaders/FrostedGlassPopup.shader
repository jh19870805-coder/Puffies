// FrostedGlassPopup.shader
// Unity Built-in Render Pipeline
// Frosted glass popup effect:
//   1. GrabPass captures the background
//   2. Two-stage blur: iterative box blur, then a mean (average) filter on top
//   3. A dark color darkens the corners (color / opacity / range / softness adjustable)
//
// Usage:
//   1. Assign this material to a Quad that covers the popup area, or a UI image.
//   2. Render it after the background but before the popup content.
//   3. UI: prefer Screen Space - Camera mode; GrabPass has limits in Overlay mode.
//   4. Note: this object fully covers the background behind it.
//
// Performance note:
//   Worst case taps = (2*meanHalf+1)^2 * (4*_BoxBlurIterations+1). Keep values small on mobile GPUs.

Shader "Custom/FrostedGlassPopup"
{
    Properties
    {
        [Header(Box Blur)]
        _BoxBlurSize       ("Box Blur Size", Range(0, 10)) = 1
        _BoxBlurIterations ("Box Blur Iterations", Range(1, 5)) = 1

        [Header(Mean Blur)]
        _MeanRadius    ("Mean Blur Radius", Range(0.5, 8)) = 3

        [Header(Vignette)]
        _VignetteColor   ("Vignette Color", Color) = (0, 0, 0, 1)
        _VignetteOpacity ("Vignette Opacity", Range(0, 1)) = 0.5
        _VignetteRange   ("Vignette Range", Range(0, 1)) = 0.6
        _VignetteSoftness("Vignette Softness", Range(0.01, 1)) = 0.3
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        LOD 200

        // Grab the current screen background into _FrostedGrab (before this object draws)
        GrabPass { "_FrostedGrab" }

        Pass
        {
            Name "FrostedGlass"
            Tags { "LightMode" = "ForwardBase" }
            Cull Off
            ZWrite Off
            ZTest Always
            Blend One Zero

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _FrostedGrab;
            float4 _FrostedGrab_TexelSize;

            float _BoxBlurSize;
            int   _BoxBlurIterations;
            float _MeanRadius;

            fixed4 _VignetteColor;
            float  _VignetteOpacity;
            float  _VignetteRange;
            float  _VignetteSoftness;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0; // panel local UV, used for the vignette
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float4 grabUV : TEXCOORD0; // screen-space UV into the grabbed texture
                float2 uv     : TEXCOORD1; // panel local UV (0..1)
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos    = UnityObjectToClipPos(v.vertex);
                o.grabUV = ComputeGrabScreenPos(o.pos);
                o.uv     = v.uv;
                return o;
            }

            // Stage 1: iterative box blur
            fixed4 BoxBlur (float2 uv)
            {
                float2 texel = _FrostedGrab_TexelSize.xy * _BoxBlurSize;

                fixed4 c = 0;
                [loop]
                for (int it = 0; it < _BoxBlurIterations; it++)
                {
                    float2 off = (float2(it, it) + 0.5) * texel;
                    c += tex2D(_FrostedGrab, uv + float2( off.x,  off.y));
                    c += tex2D(_FrostedGrab, uv + float2(-off.x,  off.y));
                    c += tex2D(_FrostedGrab, uv + float2( off.x, -off.y));
                    c += tex2D(_FrostedGrab, uv + float2(-off.x, -off.y));
                }
                c += tex2D(_FrostedGrab, uv); // center sample
                return c / (4 * _BoxBlurIterations + 1);
            }

            // Stage 2: mean (average) blur over the box-blurred result
            fixed4 MeanBlur (float2 uv)
            {
                int half = int(min(ceil(_MeanRadius), 8.0));
                fixed4 c = 0;
                int   n = 0;
                [loop]
                for (int y = -half; y <= half; y++)
                {
                    [loop]
                    for (int x = -half; x <= half; x++)
                    {
                        c += BoxBlur(uv + float2(x, y) * _FrostedGrab_TexelSize.xy);
                        n++;
                    }
                }
                return c / n;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv   = i.grabUV.xy / i.grabUV.w;

                // ===== 1. Two-stage blur: mean blur on top of the box blur =====
                fixed4 col = MeanBlur(uv);

                // ===== 2. Corner darkening (vignette based on panel local UV) =====
                float2 centered = i.uv * 2.0 - 1.0;        // center at (0,0), range -1..1
                float  dist     = length(centered);        // distance from panel center

                // Pixels farther than _VignetteRange get progressively darker
                float mask = smoothstep(_VignetteRange - _VignetteSoftness, _VignetteRange, dist);

                fixed4 vignette = _VignetteColor * (mask * _VignetteOpacity);

                // Blend the dark color over the blurred background
                col.rgb = lerp(col.rgb, vignette.rgb, vignette.a);

                return col;
            }
            ENDCG
        }
    }

    Fallback Off
}
