Shader "Custom/AnimatedFog2D"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.7, 0.7, 0.8, 1)
        _Density ("Density", Range(0, 1)) = 0.5
        _ScrollSpeed ("Scroll Speed", Vector) = (0.02, 0.01, 0, 0)
        _NoiseScale ("Noise Scale", Range(0.1, 5)) = 1.5
        _LayerCount ("Layer Count", Int) = 2
        _TimeOffset ("Time Offset", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _FogColor;
                float _Density;
                float2 _ScrollSpeed;
                float _NoiseScale;
                int _LayerCount;
                float _TimeOffset;
            CBUFFER_END

            // Simple Perlin-like noise function
            float hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f); // Smoothstep

                float a = hash(i);
                float b = hash(i + float2(1.0, 0.0));
                float c = hash(i + float2(0.0, 1.0));
                float d = hash(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Fractal Brownian Motion for more organic look
            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                for (int i = 0; i < 4; i++)
                {
                    value += amplitude * noise(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Calculate scrolling UV
                float2 scrolledUV = IN.uv * _NoiseScale;
                float time = _Time.y + _TimeOffset;
                scrolledUV += _ScrollSpeed * time;

                // Generate multi-layer fog using FBM
                float fogNoise = 0.0;

                if (_LayerCount >= 1)
                {
                    fogNoise += fbm(scrolledUV) * 0.5;
                }
                if (_LayerCount >= 2)
                {
                    fogNoise += fbm(scrolledUV * 1.3 + float2(100.0, 100.0)) * 0.3;
                }
                if (_LayerCount >= 3)
                {
                    fogNoise += fbm(scrolledUV * 0.7 + float2(200.0, 200.0)) * 0.2;
                }

                // Normalize based on layer count
                if (_LayerCount == 1) fogNoise *= 2.0;
                else if (_LayerCount == 2) fogNoise *= 1.25;

                // Apply density
                float finalAlpha = saturate(fogNoise) * _Density;

                // Sample texture (if using sprite texture)
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                // Combine fog color with texture and density
                half4 finalColor = _FogColor;
                finalColor.a *= finalAlpha * texColor.a * IN.color.a;

                return finalColor;
            }
            ENDHLSL
        }
    }
}
