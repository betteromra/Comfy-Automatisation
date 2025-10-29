Shader "Custom/BuildingPreview"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _ValidColor ("Valid Placement Color", Color) = (0,1,0,0.5)
        _InvalidColor ("Invalid Placement Color", Color) = (1,0,0,0.5)
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _FresnelPower ("Fresnel Power", Range(0.1,5)) = 2.0
        _FresnelIntensity ("Fresnel Intensity", Range(0,2)) = 1.0
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _PulseSpeed ("Pulse Speed", Range(0,5)) = 1.0
        _PulseIntensity ("Pulse Intensity", Range(0,1)) = 0.3
        [Toggle] _IsValidPlacement ("Is Valid Placement", Float) = 1
        [Toggle] _UseGridPattern ("Use Grid Pattern", Float) = 0
        _GridScale ("Grid Scale", Float) = 10.0
        _GridThickness ("Grid Thickness", Range(0,1)) = 0.1
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
        Cull Back

        Pass
        {
            Name "BuildingPreview"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 viewDirWS : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                float fogFactor : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _ValidColor;
                float4 _InvalidColor;
                float4 _RimColor;
                float _Transparency;
                float _FresnelPower;
                float _FresnelIntensity;
                float _PulseSpeed;
                float _PulseIntensity;
                float _IsValidPlacement;
                float _UseGridPattern;
                float _GridScale;
                float _GridThickness;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.viewDirWS = GetWorldSpaceViewDir(vertexInput.positionWS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                // Sample texture
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Choose color based on placement validity
                half4 placementColor = lerp(_InvalidColor, _ValidColor, _IsValidPlacement);
                
                // Apply tint color
                half4 finalColor = texColor * _Color * placementColor;

                // Fresnel effect for rim lighting
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                float fresnel = pow(1.0 - saturate(dot(normalWS, viewDirWS)), _FresnelPower);
                fresnel *= _FresnelIntensity;

                // Rim color contribution
                half3 rimContribution = _RimColor.rgb * fresnel;
                finalColor.rgb += rimContribution;

                // Pulsing effect
                float pulse = sin(_Time.y * _PulseSpeed) * 0.5 + 0.5;
                float pulseEffect = lerp(1.0, 1.0 + _PulseIntensity, pulse);
                finalColor.rgb *= pulseEffect;

                // Optional grid pattern
                if (_UseGridPattern > 0.5)
                {
                    float2 gridUV = input.positionWS.xz * _GridScale;
                    float2 grid = abs(frac(gridUV - 0.5) - 0.5) / fwidth(gridUV);
                    float gridLine = min(grid.x, grid.y);
                    float gridPattern = 1.0 - saturate(gridLine - _GridThickness);
                    finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * 1.5, gridPattern * 0.5);
                }

                // Apply transparency with fresnel boost
                finalColor.a = saturate(_Transparency + fresnel * 0.3) * placementColor.a;

                // Apply fog
                finalColor.rgb = MixFog(finalColor.rgb, input.fogFactor);

                return finalColor;
            }
            ENDHLSL
        }
    }
    
    Fallback "Universal Render Pipeline/Unlit"
}
