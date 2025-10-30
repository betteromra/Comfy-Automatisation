Shader "Custom/StylizedWater"
{
    Properties
    {
        [Header(Water Colors)]
        _ShallowColor ("Shallow Water Color", Color) = (0.325, 0.807, 0.971, 0.725)
        _DeepColor ("Deep Water Color", Color) = (0.086, 0.407, 1, 0.749)
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        
        [Header(Wave Settings)]
        _WaveSpeed ("Wave Speed", Range(0, 3)) = 1.0
        _WaveHeight ("Wave Height", Range(0, 2)) = 0.4
        _WaveLength ("Wave Length", Range(0.1, 10)) = 2.0
        _Steepness ("Wave Steepness", Range(0, 1)) = 0.5
        
        [Header(Foam Settings)]
        _FoamAmount ("Foam Amount", Range(0, 1)) = 0.5
        _FoamCutoff ("Foam Threshold", Range(0, 1)) = 0.6
        _FoamSpeed ("Foam Speed", Range(0, 2)) = 0.5
        
        [Header(Cardboard Style)]
        _CardboardScale ("Cardboard Scale", Range(0, 50)) = 10.0
        _CardboardIntensity ("Cardboard Intensity", Range(0, 1)) = 0.2
        
        [Header(Style)]
        _ColorBands ("Color Bands (lower = more stylized)", Range(2, 20)) = 5
        
        [Header(Transparency)]
        _Transparency ("Transparency", Range(0, 1)) = 0.8
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "StylizedWater"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float fogFactor : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _ShallowColor;
                float4 _DeepColor;
                float4 _FoamColor;
                float _WaveSpeed;
                float _WaveHeight;
                float _WaveLength;
                float _Steepness;
                float _FoamAmount;
                float _FoamCutoff;
                float _FoamSpeed;
                float _CardboardScale;
                float _CardboardIntensity;
                float _ColorBands;
                float _Transparency;
            CBUFFER_END

            // Simple noise function
            float noise(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            // Improved noise
            float smoothNoise(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);
                
                float a = noise(i);
                float b = noise(i + float2(1.0, 0.0));
                float c = noise(i + float2(0.0, 1.0));
                float d = noise(i + float2(1.0, 1.0));
                
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Gerstner Wave function
            float3 GerstnerWave(float3 position, float2 direction, float wavelength, float steepness, float time)
            {
                float k = 2.0 * PI / wavelength; // Wave number
                float c = sqrt(9.8 / k); // Wave speed (gravity / wave number)
                float2 d = normalize(direction);
                float f = k * (dot(d, position.xz) - c * time);
                float a = steepness / k; // Amplitude
                
                float3 gerstner;
                gerstner.x = d.x * (a * cos(f));
                gerstner.y = a * sin(f);
                gerstner.z = d.y * (a * cos(f));
                
                return gerstner;
            }

            // Cardboard-like texture
            float cardboardTexture(float2 uv)
            {
                float2 scaledUV = uv * _CardboardScale;
                float lines = abs(sin(scaledUV.x * 10.0) * sin(scaledUV.y * 10.0));
                float grain = smoothNoise(scaledUV * 5.0);
                return lerp(grain, lines, 0.3);
            }

            // Posterize/band colors for stylization
            float3 posterize(float3 color, float steps)
            {
                return floor(color * steps) / steps;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float time = _Time.y * _WaveSpeed;
                
                // Apply multiple Gerstner waves for realistic water movement
                float3 waveOffset = float3(0, 0, 0);
                
                // Wave 1 - Main direction
                waveOffset += GerstnerWave(positionWS, float2(1, 0), _WaveLength, _Steepness, time);
                
                // Wave 2 - Slight angle
                waveOffset += GerstnerWave(positionWS, float2(0.7, 0.7), _WaveLength * 0.8, _Steepness * 0.8, time * 1.1);
                
                // Wave 3 - Different direction for complexity
                waveOffset += GerstnerWave(positionWS, float2(-0.5, 0.9), _WaveLength * 1.2, _Steepness * 0.6, time * 0.9);
                
                // Scale by wave height
                positionWS += waveOffset * _WaveHeight;
                
                output.positionWS = positionWS;
                output.positionCS = TransformWorldToHClip(positionWS);
                output.uv = input.uv;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.fogFactor = ComputeFogFactor(output.positionCS.z);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                
                // Sample depth for water depth calculation
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float sceneDepth = LinearEyeDepth(SampleSceneDepth(screenUV), _ZBufferParams);
                float surfaceDepth = LinearEyeDepth(input.positionCS.z, _ZBufferParams);
                float depthDifference = sceneDepth - surfaceDepth;
                
                // Animated foam pattern - multiple layers of moving dots
                float2 foamUV = input.positionWS.xz * 0.5 + time * _FoamSpeed * 0.1;
                float foam = smoothNoise(foamUV * 5.0);
                foam += smoothNoise(foamUV * 10.0) * 0.5;
                foam += smoothNoise(foamUV * 20.0) * 0.25;
                foam = saturate(foam * _FoamAmount);
                
                // Edge foam based on depth
                float edgeFoam = 1.0 - saturate(depthDifference * 0.5);
                edgeFoam = pow(edgeFoam, 3.0);
                
                // Combine foam with sharp cutoff
                float totalFoam = saturate(foam + edgeFoam);
                totalFoam = step(_FoamCutoff, totalFoam);
                
                // Base water color with depth variation
                float depthFactor = saturate(depthDifference * 0.15);
                float3 waterColor = lerp(_ShallowColor.rgb, _DeepColor.rgb, depthFactor);
                
                // Apply cardboard texture
                float cardboard = cardboardTexture(input.positionWS.xz);
                waterColor = lerp(waterColor, waterColor * cardboard, _CardboardIntensity);
                
                // Posterize for stylized look
                waterColor = posterize(waterColor, _ColorBands);
                
                // Mix with foam
                float3 finalColor = lerp(waterColor, _FoamColor.rgb, totalFoam);
                
                // Calculate alpha
                float alpha = lerp(_Transparency, 1.0, totalFoam);
                
                // Apply fog
                finalColor = MixFog(finalColor, input.fogFactor);
                
                return half4(finalColor, alpha);
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Unlit"
}
