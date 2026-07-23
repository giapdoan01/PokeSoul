Shader "UI/URP/FrameTrail"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [NoScaleOffset] _FrameShapeTex ("Frame Shape Texture", 2D) = "white" {}
        _FrameShapeThreshold ("Frame Shape Threshold", Range(0.0, 1.0)) = 0.1
        _FrameShapeSoftness ("Frame Shape Softness", Range(0.0001, 0.5)) = 0.05

        _BorderWidth ("Border Width", Range(0.0, 0.5)) = 0.08
        _BorderSoftness ("Border Softness", Range(0.0001, 0.25)) = 0.03
        _TrailTrackWidth ("Trail Track Width", Range(0.0, 0.25)) = 0.03

        _TrailColor ("Trail Color", Color) = (0.4, 0.9, 1.0, 1)
        [NoScaleOffset] _TrailTexture ("Trail Texture", 2D) = "white" {}
        _TrailStrength ("Trail Strength", Range(0.0, 8.0)) = 2.5
        _TrailLength ("Trail Length", Range(0.01, 1.0)) = 0.10
        _TrailSoftness ("Trail Softness", Range(0.0001, 0.5)) = 0.05
        _TrailHardness ("Trail Hardness", Range(0.5, 8.0)) = 2.5
        _GlowSpread ("Glow Spread", Range(0.0, 0.25)) = 0.08
        _GlowColorBoost ("Glow Color Boost", Range(0.0, 8.0)) = 2.0
        _GlowAlpha ("Glow Alpha", Range(0.0, 4.0)) = 1.15
        _TrailSpeed ("Trail Speed", Range(0.0, 10.0)) = 1.0

        _Trail1Phase ("Trail 1 Phase", Range(0.0, 1.0)) = 0.00
        _Trail2Phase ("Trail 2 Phase", Range(0.0, 1.0)) = 0.37
        _Trail3Phase ("Trail 3 Phase", Range(0.0, 1.0)) = 0.73
        _TrailCount ("Trail Count", Range(1.0, 3.0)) = 3.0
        _Trail1Intensity ("Trail 1 Intensity", Range(0.0, 3.0)) = 1.0
        _Trail2Intensity ("Trail 2 Intensity", Range(0.0, 3.0)) = 1.0
        _Trail3Intensity ("Trail 3 Intensity", Range(0.0, 3.0)) = 1.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
        _ClipRect ("Clip Rect", Vector) = (-32767, -32767, 32767, 32767)
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
            "RenderPipeline"="UniversalPipeline"
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
        Fog { Mode Off }
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "UIFrameTrail"
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
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                half2 texcoord       : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            fixed4 _Color;
            fixed4 _TrailColor;
            sampler2D _MainTex;
            sampler2D _FrameShapeTex;
            sampler2D _TrailTexture;
            float4 _MainTex_ST;
            float4 _ClipRect;
            float _FrameShapeThreshold;
            float _FrameShapeSoftness;
            float _BorderWidth;
            float _BorderSoftness;
            float _TrailStrength;
            float _TrailLength;
            float _TrailSoftness;
            float _TrailHardness;
            float _GlowSpread;
            float _GlowColorBoost;
            float _GlowAlpha;
            float _TrailSpeed;
            float _Trail1Phase;
            float _Trail2Phase;
            float _Trail3Phase;
            float _TrailCount;
            float _Trail1Intensity;
            float _Trail2Intensity;
            float _Trail3Intensity;
            float _TrailTrackWidth;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = TRANSFORM_TEX(IN.texcoord, _MainTex);
                OUT.color = IN.color * _Color;
                #ifdef UNITY_HALF_TEXEL_OFFSET
                OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1, 1);
                #endif
                return OUT;
            }

            float PerimeterCoord(float2 uv)
            {
                float left = uv.x;
                float right = 1.0 - uv.x;
                float bottom = uv.y;
                float top = 1.0 - uv.y;
                float side = 0.0;

                if (left <= right && left <= bottom && left <= top)
                {
                    side = 4.0 - uv.y;
                }
                else if (bottom <= right && bottom <= top)
                {
                    side = uv.x;
                }
                else if (right <= top)
                {
                    side = 1.0 + uv.y;
                }
                else
                {
                    side = 3.0 - uv.x;
                }

                return side;
            }

            float RingDistance(float a, float b)
            {
                float d = abs(a - b);
                return min(d, 4.0 - d);
            }

            float TrailPulse(float perimeterCoord, float phase)
            {
                float position = frac(_Time.y * _TrailSpeed + phase) * 4.0;
                float distance = RingDistance(perimeterCoord, position);
                float pulse = 1.0 - smoothstep(_TrailLength, _TrailLength + _TrailSoftness, distance);
                return pow(saturate(pulse), _TrailHardness);
            }

            float TrailTextureMask(float perimeterCoord, float phase)
            {
                float position = frac(_Time.y * _TrailSpeed + phase) * 4.0;
                float distance = RingDistance(perimeterCoord, position);
                float normalizedDistance = saturate(distance / max(_TrailLength, 0.0001));
                float2 trailUV = float2(1.0 - normalizedDistance, 0.5);
                fixed4 trailTex = tex2D(_TrailTexture, trailUV);
                return saturate(max(trailTex.a, dot(trailTex.rgb, float3(0.3333, 0.3333, 0.3333))));
            }

            float TrailTrackMask(float edgeDist)
            {
                float d = abs(edgeDist - _BorderWidth);
                return 1.0 - smoothstep(_TrailTrackWidth, _TrailTrackWidth + _BorderSoftness, d);
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, IN.texcoord) * IN.color;
                fixed4 shapeTex = tex2D(_FrameShapeTex, IN.texcoord);

                float2 uv = saturate(IN.texcoord);
                float edgeDist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float glowStart = max(0.0, _BorderWidth - _GlowSpread);
                float glowEnd = _BorderWidth + _BorderSoftness + _GlowSpread;
                float borderMask = 1.0 - smoothstep(glowStart, glowEnd, edgeDist);
                float shapeMask = smoothstep(_FrameShapeThreshold, _FrameShapeThreshold + _FrameShapeSoftness, shapeTex.a);
                borderMask *= shapeMask;

                float perimeterCoord = PerimeterCoord(uv);

                float trailTex1 = TrailTextureMask(perimeterCoord, _Trail1Phase);
                float trailTex2 = TrailTextureMask(perimeterCoord, _Trail2Phase);
                float trailTex3 = TrailTextureMask(perimeterCoord, _Trail3Phase);

                float trackMask = TrailTrackMask(edgeDist);
                float trailCount = floor(_TrailCount + 0.5);
                float enable1 = step(0.5, trailCount);
                float enable2 = step(1.5, trailCount);
                float enable3 = step(2.5, trailCount);

                float trail1 = TrailPulse(perimeterCoord, _Trail1Phase) * _Trail1Intensity * trackMask * trailTex1 * enable1;
                float trail2 = TrailPulse(perimeterCoord, _Trail2Phase) * _Trail2Intensity * trackMask * trailTex2 * enable2;
                float trail3 = TrailPulse(perimeterCoord, _Trail3Phase) * _Trail3Intensity * trackMask * trailTex3 * enable3;

                float trail = max(trail1, max(trail2, trail3));
                trail *= borderMask;

                float3 baseRgb = tex.rgb * tex.a * shapeMask;
                float3 trailTexRgb = tex2D(_TrailTexture, float2(frac(perimeterCoord * 0.25 - _Time.y * _TrailSpeed), 0.5)).rgb;
                float3 glowRgb = _TrailColor.rgb * trail * _TrailStrength * _GlowColorBoost * max(trailTexRgb, 0.2);
                float3 rgb = baseRgb + glowRgb;

                float alpha = saturate(tex.a * shapeMask * borderMask + trail * _GlowAlpha);
                #ifdef UNITY_UI_CLIP_RECT
                alpha *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                fixed4 outColor = fixed4(rgb, alpha);

                #ifdef UNITY_UI_ALPHACLIP
                clip(outColor.a - 0.001);
                #endif

                return outColor;
            }
            ENDCG
        }
    }
}
