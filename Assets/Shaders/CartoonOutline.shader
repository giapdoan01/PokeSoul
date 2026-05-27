Shader "Custom/CartoonOutline"
{
    Properties
    {
        _MainTex       ("Texture", 2D)             = "white" {}
        _Color         ("Base Color Tint", Color)  = (1,1,1,1)

        // Cel shading
        _ShadowColor   ("Shadow Color", Color)     = (0.3,0.3,0.4,1)
        _ShadowThresh  ("Shadow Threshold", Range(-1,1))  = 0.0
        _ShadowSmooth  ("Shadow Smoothness", Range(0,0.5)) = 0.05
        _Steps         ("Shading Steps", Range(1,8))      = 3

        // Specular
        _SpecColor2    ("Specular Color", Color)   = (1,1,1,1)
        _Glossiness    ("Glossiness", Range(1,256)) = 64
        _SpecThresh    ("Specular Threshold", Range(0,1)) = 0.5

        // Rim
        _RimColor      ("Rim Color", Color)        = (1,1,1,1)
        _RimPower      ("Rim Power", Range(0.1,8)) = 3.0
        _RimThresh     ("Rim Threshold", Range(0,1)) = 0.5

        // Outline
        _OutlineColor  ("Outline Color", Color)    = (0,0,0,1)
        _OutlineWidth  ("Outline Width", Range(0,0.1)) = 0.005
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        // ─────────────────────────────────────────
        // Pass 1 – Outline (inverted hull, backface)
        // ─────────────────────────────────────────
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Front

            HLSLPROGRAM
            #pragma vertex   OutlineVert
            #pragma fragment OutlineFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float  _ShadowThresh;
                float  _ShadowSmooth;
                float  _Steps;
                float4 _SpecColor2;
                float  _Glossiness;
                float  _SpecThresh;
                float4 _RimColor;
                float  _RimPower;
                float  _RimThresh;
                float4 _OutlineColor;
                float  _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings OutlineVert(Attributes IN)
            {
                Varyings OUT;
                float3 pos    = IN.positionOS.xyz + IN.normalOS * _OutlineWidth;
                OUT.positionCS = TransformObjectToHClip(pos);
                return OUT;
            }

            half4 OutlineFrag(Varyings IN) : SV_Target
            {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // ─────────────────────────────────────────
        // Pass 2 – Cel shading (forward lit)
        // ─────────────────────────────────────────
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back

            HLSLPROGRAM
            #pragma vertex   CelVert
            #pragma fragment CelFrag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ShadowColor;
                float  _ShadowThresh;
                float  _ShadowSmooth;
                float  _Steps;
                float4 _SpecColor2;
                float  _Glossiness;
                float  _SpecThresh;
                float4 _RimColor;
                float  _RimPower;
                float  _RimThresh;
                float4 _OutlineColor;
                float  _OutlineWidth;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 normalWS    : TEXCOORD1;
                float3 positionWS  : TEXCOORD2;
                float4 shadowCoord : TEXCOORD3;
                float  fogFactor   : TEXCOORD4;
            };

            Varyings CelVert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs   nrmInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS  = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = nrmInputs.normalWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                OUT.fogFactor   = ComputeFogFactor(posInputs.positionCS.z);
                return OUT;
            }

            half4 CelFrag(Varyings IN) : SV_Target
            {
                // Base texture
                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * _Color;

                float3 N = normalize(IN.normalWS);
                float3 V = normalize(GetCameraPositionWS() - IN.positionWS);

                // Main light + shadow
                Light mainLight = GetMainLight(IN.shadowCoord);
                float3 L = normalize(mainLight.direction);
                float  shadow = mainLight.shadowAttenuation;

                // ── Cel diffuse (stepped) ──────────────────────
                float NdotL = dot(N, L) * shadow;
                float ramp  = smoothstep(_ShadowThresh - _ShadowSmooth,
                                         _ShadowThresh + _ShadowSmooth,
                                         NdotL);
                // Posterize into _Steps bands
                ramp = floor(ramp * _Steps) / _Steps;

                float3 diffuse = lerp(_ShadowColor.rgb, mainLight.color, ramp);

                // ── Cel specular ───────────────────────────────
                float3 H       = normalize(L + V);
                float  NdotH   = saturate(dot(N, H));
                float  spec    = pow(NdotH, _Glossiness);
                float  specCel = step(_SpecThresh, spec);
                float3 specular = specCel * _SpecColor2.rgb * mainLight.color;

                // ── Rim light ──────────────────────────────────
                float  rim    = 1.0 - saturate(dot(N, V));
                float  rimCel = step(_RimThresh, pow(rim, _RimPower));
                float3 rimOut = rimCel * _RimColor.rgb;

                // ── Combine ────────────────────────────────────
                float3 col = albedo.rgb * diffuse + specular + rimOut;

                // Fog
                col = MixFog(col, IN.fogFactor);

                return half4(col, albedo.a);
            }
            ENDHLSL
        }

        // Shadow caster (needed so the object casts shadows)
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
