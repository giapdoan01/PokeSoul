Shader "UI/URP/RarityFrame"
{
    Properties
    {
        [Header(Main)]
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Frame Shape Mask)]
        [NoScaleOffset] _FrameShapeTex ("Frame Shape Texture", 2D) = "white" {}
        _UseFrameShapeTex ("Use Frame Shape Texture", Range(0.0, 1.0)) = 0.0
        _FrameShapeThreshold ("Frame Shape Threshold", Range(0.0, 1.0)) = 0.1
        _FrameShapeSoftness ("Frame Shape Softness", Range(0.0001, 0.5)) = 0.05

        [Header(Rarity Colors)]
        _Rarity ("Rarity: 0 C, 1 R, 2 S, 3 SSR, 4 SSS", Range(0.0, 4.0)) = 0.0
        _CColor ("C Color", Color) = (0.62,0.68,0.72,1)
        _RColor ("R Color", Color) = (0.25,0.92,1.00,1)
        _SColor ("S Color", Color) = (1.00,0.74,0.22,1)
        _SSRColor ("SSR Color", Color) = (1.00,0.32,0.75,1)
        _SSSColor ("SSS Color", Color) = (0.65,1.00,0.92,1)

        [Header(Shared Frame)]
        _CornerRadiusPixels ("Corner Radius Pixels", Range(0.0, 64.0)) = 7.0
        _CornerSoftnessPixels ("Corner Softness Pixels", Range(0.1, 8.0)) = 1.0
        _BorderWidth ("Border Width", Range(0.0, 0.5)) = 0.08
        _BorderSoftness ("Border Softness", Range(0.0001, 0.25)) = 0.03
        _InnerLineWidth ("Inner Line Width", Range(0.0, 0.25)) = 0.018
        _OuterGlowSpread ("Outer Glow Spread", Range(0.0, 0.35)) = 0.08
        _GlowStrength ("Glow Strength", Range(0.0, 12.0)) = 1.5
        _PulseStrength ("Pulse Strength", Range(0.0, 2.0)) = 0.25

        [Header(C and R Border Pulse)]
        _CommonBlinkStrength ("C/R Emission Pulse Strength", Range(0.0, 2.0)) = 0.35
        _CommonBlinkSpeed ("C/R Emission Pulse Speed", Range(0.0, 8.0)) = 1.4

        [Header(R Rare)]
        _RSlashInterval ("R Slash Interval", Range(0.2, 8.0)) = 2.2
        _RSlashWidth ("R Slash Width", Range(0.005, 0.5)) = 0.075
        _RSlashStrength ("R Slash Strength", Range(0.0, 12.0)) = 3.5

        [Header(S Super)]
        [NoScaleOffset] _SweepTex ("Sweep Texture", 2D) = "white" {}
        _SweepSpeed ("Sweep Speed", Range(0.0, 10.0)) = 1.0
        _SweepLength ("Sweep Length", Range(0.01, 1.0)) = 0.12
        _SweepSoftness ("Sweep Softness", Range(0.0001, 0.5)) = 0.05
        _SweepStrength ("Sweep Strength", Range(0.0, 12.0)) = 2.5
        _SHoloStrength ("S Edge Hologram Strength", Range(0.0, 4.0)) = 0.35

        [Header(SSR Super Special Rare)]
        _SSRBorderEmission ("SSR Border Emission", Range(0.0, 12.0)) = 1.7
        _SSRShimmerStrength ("SSR Etched Shimmer Strength", Range(0.0, 10.0)) = 1.6
        _SSRShimmerEmission ("SSR Etched Shimmer Emission", Range(0.0, 12.0)) = 1.1
        _SSRCornerEmission ("SSR Corner Seal Emission", Range(0.0, 12.0)) = 1.4
        _SparkleStrength ("Sparkle Strength", Range(0.0, 8.0)) = 1.0
        _SparkleDensity ("Sparkle Density", Range(8.0, 160.0)) = 48.0

        [Header(SSS Mythic)]
        _SSSBorderEmission ("SSS Border Emission", Range(0.0, 12.0)) = 2.2
        _SSSPrismStrength ("SSS Crystal Facet Strength", Range(0.0, 12.0)) = 2.6
        _SSSPrismEmission ("SSS Crystal Facet Emission", Range(0.0, 12.0)) = 1.1
        _SSSAuroraEmission ("SSS Chromatic Edge Emission", Range(0.0, 12.0)) = 0.9
        _SSSCornerEmission ("SSS Crown Corner Emission", Range(0.0, 12.0)) = 1.6

        [Header(SSR and SSS Hologram)]
        _HoloStrength ("Hologram Strength", Range(0.0, 8.0)) = 1.0
        _HoloColorBlend ("Hologram Color Blend", Range(0.0, 1.0)) = 0.35

        [Header(UI Internal)]
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
            Name "UIRarityFrame"
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
            fixed4 _CColor;
            fixed4 _RColor;
            fixed4 _SColor;
            fixed4 _SSRColor;
            fixed4 _SSSColor;
            sampler2D _MainTex;
            sampler2D _FrameShapeTex;
            sampler2D _SweepTex;
            float4 _MainTex_ST;
            float4 _ClipRect;
            float _UseFrameShapeTex;
            float _FrameShapeThreshold;
            float _FrameShapeSoftness;
            float _Rarity;
            float _CornerRadiusPixels;
            float _CornerSoftnessPixels;
            float _BorderWidth;
            float _BorderSoftness;
            float _InnerLineWidth;
            float _OuterGlowSpread;
            float _SweepSpeed;
            float _SweepLength;
            float _SweepSoftness;
            float _SweepStrength;
            float _SHoloStrength;
            float _RSlashInterval;
            float _RSlashWidth;
            float _RSlashStrength;
            float _GlowStrength;
            float _SparkleStrength;
            float _SparkleDensity;
            float _SSRBorderEmission;
            float _SSRShimmerStrength;
            float _SSRShimmerEmission;
            float _SSRCornerEmission;
            float _SSSBorderEmission;
            float _SSSPrismStrength;
            float _SSSPrismEmission;
            float _SSSAuroraEmission;
            float _SSSCornerEmission;
            float _CommonBlinkStrength;
            float _CommonBlinkSpeed;
            float _HoloStrength;
            float _HoloColorBlend;
            float _PulseStrength;

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

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float PerimeterCoord(float2 uv)
            {
                float left = uv.x;
                float right = 1.0 - uv.x;
                float bottom = uv.y;
                float top = 1.0 - uv.y;

                if (left <= right && left <= bottom && left <= top)
                    return 4.0 - uv.y;

                if (bottom <= right && bottom <= top)
                    return uv.x;

                if (right <= top)
                    return 1.0 + uv.y;

                return 3.0 - uv.x;
            }

            float RingDistance(float a, float b)
            {
                float d = abs(a - b);
                return min(d, 4.0 - d);
            }

            float RoundedRectMask(float2 uv)
            {
                float2 uvPerPixel = max(fwidth(uv), float2(0.00001, 0.00001));
                float2 halfSizePixels = 0.5 / uvPerPixel;
                float2 positionPixels = (uv - 0.5) / uvPerPixel;
                float radiusPixels = min(_CornerRadiusPixels, min(halfSizePixels.x, halfSizePixels.y));
                float2 q = abs(positionPixels) - (halfSizePixels - radiusPixels);
                float signedDistance = length(max(q, 0.0)) + min(max(q.x, q.y), 0.0) - radiusPixels;
                return 1.0 - smoothstep(0.0, _CornerSoftnessPixels, signedDistance);
            }

            fixed4 RarityColor(float rarity)
            {
                fixed4 color = _CColor;
                color = lerp(color, _RColor, step(0.5, rarity));
                color = lerp(color, _SColor, step(1.5, rarity));
                color = lerp(color, _SSRColor, step(2.5, rarity));
                color = lerp(color, _SSSColor, step(3.5, rarity));
                return color;
            }

            float RarityLevel(float minLevel, float rarity)
            {
                return step(minLevel - 0.5, rarity);
            }

            float SweepPulse(float perimeterCoord, float phase)
            {
                float position = frac(_Time.y * _SweepSpeed + phase) * 4.0;
                float distance = RingDistance(perimeterCoord, position);
                float pulse = 1.0 - smoothstep(_SweepLength, _SweepLength + _SweepSoftness, distance);
                float2 sweepUV = float2(saturate(1.0 - distance / max(_SweepLength, 0.0001)), 0.5);
                fixed4 sweepTex = tex2D(_SweepTex, sweepUV);
                float texMask = max(max(sweepTex.a, dot(sweepTex.rgb, float3(0.3333, 0.3333, 0.3333))), 0.25);
                return pulse * texMask;
            }

            float DiagonalSlash(float2 uv)
            {
                float phase = frac(_Time.y / max(_RSlashInterval, 0.001));
                float coord = uv.x + uv.y;
                float center = lerp(-0.35, 2.35, phase);
                float core = 1.0 - smoothstep(_RSlashWidth, _RSlashWidth + _SweepSoftness, abs(coord - center));
                float tail = 1.0 - smoothstep(_RSlashWidth * 3.5, _RSlashWidth * 3.5 + _SweepSoftness, abs(coord - center + _RSlashWidth * 2.0));
                float edgeFade = smoothstep(0.02, 0.12, phase) * (1.0 - smoothstep(0.88, 0.98, phase));
                return saturate(core + tail * 0.35) * edgeFade;
            }

            float CornerMask(float2 uv, float radius)
            {
                float2 cornerDistance = min(uv, 1.0 - uv);
                return 1.0 - smoothstep(radius, radius + _BorderSoftness, length(cornerDistance));
            }

            fixed3 HoloColor(float2 uv, float perimeterCoord)
            {
                float t = perimeterCoord * 1.35 + _Time.y * 0.8 + uv.y * 0.9;
                return 0.5 + 0.5 * cos(6.28318 * (t + fixed3(0.00, 0.33, 0.67)));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, IN.texcoord) * IN.color;
                fixed4 shapeTex = tex2D(_FrameShapeTex, IN.texcoord);

                float rarity = floor(_Rarity + 0.5);
                fixed4 rarityColor = RarityColor(rarity);

                float2 uv = saturate(IN.texcoord);
                float edgeDist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float frameBand = 1.0 - smoothstep(_BorderWidth, _BorderWidth + _BorderSoftness, edgeDist);
                float innerLine = 1.0 - smoothstep(_InnerLineWidth, _InnerLineWidth + _BorderSoftness, abs(edgeDist - _BorderWidth));
                float glowBand = 1.0 - smoothstep(max(0.0, _BorderWidth - _OuterGlowSpread), _BorderWidth + _BorderSoftness + _OuterGlowSpread, edgeDist);
                float shapeMask = smoothstep(_FrameShapeThreshold, _FrameShapeThreshold + _FrameShapeSoftness, shapeTex.a);
                float roundedMask = RoundedRectMask(uv);
                float frameMask = lerp(frameBand, shapeMask, saturate(_UseFrameShapeTex)) * roundedMask;

                float perimeterCoord = PerimeterCoord(uv);

                float isC = 1.0 - step(0.5, rarity);
                float isR = step(0.5, rarity) * (1.0 - step(1.5, rarity));
                float isS = step(1.5, rarity) * (1.0 - step(2.5, rarity));
                float isSSR = step(2.5, rarity) * (1.0 - step(3.5, rarity));
                float isSSS = step(3.5, rarity);

                float sSweep = saturate(SweepPulse(perimeterCoord, 0.00) + SweepPulse(perimeterCoord, 0.50)) * frameMask * isS;
                float rSlash = DiagonalSlash(uv) * isR;

                float sparkleSeed = Hash21(floor(float2(perimeterCoord * _SparkleDensity, edgeDist * 80.0)));
                float sparkleBlink = step(0.94, Hash21(floor(float2(perimeterCoord * _SparkleDensity, _Time.y * 12.0))));
                float ssrSparkle = sparkleBlink * sparkleSeed * frameMask * isSSR;
                float sssSparkle = step(0.88, Hash21(floor(float2(perimeterCoord * (_SparkleDensity * 1.65), _Time.y * 18.0)))) * sparkleSeed * frameMask * isSSS;

                float pulse = 1.0 + sin(_Time.y * (1.5 + rarity * 0.45)) * _PulseStrength * saturate(rarity * 0.25);
                fixed3 holo = HoloColor(uv, perimeterCoord);
                float holoMask = (isSSR * 0.35 + isSSS * 0.75) * _HoloStrength;
                float holoBlend = saturate(holoMask * _HoloColorBlend);
                fixed3 tintedHolo = rarityColor.rgb * (0.55 + holo * 1.45);
                fixed3 gradeColor = lerp(rarityColor.rgb, tintedHolo, holoBlend);
                float lowRarityPulse = saturate(isC + isR);
                float commonEmissionWave = 0.5 + 0.5 * sin(_Time.y * _CommonBlinkSpeed);
                float commonEmission = 1.0 + commonEmissionWave * _CommonBlinkStrength * lowRarityPulse;
                float commonGlowBoost = commonEmissionWave * _CommonBlinkStrength * lowRarityPulse;

                float ssrEtch = pow(0.5 + 0.5 * sin(perimeterCoord * 24.0 - _Time.y * 1.15), 10.0);
                float ssrSilk = pow(0.5 + 0.5 * sin((uv.x - uv.y) * 18.0 + perimeterCoord * 1.5 + _Time.y * 0.9), 4.0);
                float ssrRibbon = (ssrEtch * innerLine + ssrSilk * glowBand * 0.35) * frameMask * isSSR * _SSRShimmerStrength;

                float corner = CornerMask(uv, 0.16);
                float ssrCornerFlare = corner * pow(0.5 + 0.5 * sin(_Time.y * 1.45 + perimeterCoord * 2.2), 5.0) * frameMask * isSSR;

                float facetA = abs(frac((uv.x + uv.y) * 7.0 + _Time.y * 0.22) - 0.5) * 2.0;
                float facetB = abs(frac((uv.x - uv.y) * 8.0 - _Time.y * 0.17) - 0.5) * 2.0;
                float sssFacet = (pow(1.0 - facetA, 4.0) + pow(1.0 - facetB, 4.0) * 0.75) * frameMask * isSSS * _SSSPrismStrength;
                float sssChromaticEdge = (frameBand + innerLine * 0.6) * frameMask * isSSS;
                float sssAurora = pow(0.5 + 0.5 * sin(uv.x * 13.0 + uv.y * 17.0 + perimeterCoord * 0.8 + _Time.y * 0.75), 2.0) * frameMask * isSSS;
                float sssCornerBurst = corner * pow(0.5 + 0.5 * sin(_Time.y * 1.9 + perimeterCoord * 5.0), 8.0) * frameMask * isSSS;
                float sHoloEdge = (frameBand + innerLine * 0.75) * frameMask * isS;
                fixed3 sHoloRgb = lerp(rarityColor.rgb, holo, 0.28) * sHoloEdge * _SHoloStrength;

                float rarityBoost = 0.65 + rarity * 0.32;
                float borderRarityBoost = lerp(rarityBoost, 0.65, lowRarityPulse);
                float borderPulse = lerp(pulse, 1.0, lowRarityPulse);
                float borderEmission = _GlowStrength;
                borderEmission = lerp(borderEmission, _SSRBorderEmission, isSSR);
                borderEmission = lerp(borderEmission, _SSSBorderEmission, isSSS);
                fixed3 baseRgb = tex.rgb * tex.a * frameMask;
                fixed3 lineRgb = gradeColor * (frameBand + innerLine * 0.7) * frameMask * borderRarityBoost * commonEmission;
                fixed3 slashRgb = gradeColor * rSlash * frameMask * _RSlashStrength * (1.0 + commonGlowBoost);
                fixed3 sSweepColor = lerp(rarityColor.rgb, fixed3(1.0, 0.95, 0.65), 0.25);
                fixed3 sweepRgb = sSweepColor * sSweep * _SweepStrength * (1.35 + _SHoloStrength * 0.15);
                fixed3 glowRgb = gradeColor * glowBand * frameMask * (borderEmission + commonGlowBoost * 1.6) * borderRarityBoost * borderPulse * commonEmission;
                fixed3 ssrSealColor = lerp(rarityColor.rgb, fixed3(1.0, 0.92, 0.72), 0.22);
                fixed3 ssrRgb = (
                    ssrSealColor * ssrRibbon * _SSRShimmerEmission +
                    ssrSealColor * ssrCornerFlare * _SSRCornerEmission +
                    lerp(rarityColor.rgb, holo, 0.35) * ssrSparkle * _SparkleStrength * 0.6
                ) * frameMask;

                fixed3 sssRgb = (
                    holo * sssFacet * _SSSPrismEmission +
                    (holo - 0.35) * sssChromaticEdge * _SSSAuroraEmission +
                    gradeColor * sssAurora * _SSSAuroraEmission * 0.45 +
                    holo * sssCornerBurst * _SSSCornerEmission +
                    holo * sssSparkle * _SparkleStrength * 0.9
                ) * frameMask;

                fixed3 rgb = baseRgb + lineRgb + slashRgb + sweepRgb + sHoloRgb + glowRgb + ssrRgb + sssRgb;
                float alpha = saturate(tex.a * frameMask + frameMask * 0.35 + rSlash * frameMask * 0.7 + sSweep * 0.75 + ssrRibbon * 0.14 + ssrSparkle * 0.65 + sssFacet * 0.12 + sssCornerBurst * 0.2 + sssSparkle * 0.75);
                float clipMask = 1.0;
                #ifdef UNITY_UI_CLIP_RECT
                clipMask = UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                rgb *= clipMask;
                alpha *= clipMask;

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
