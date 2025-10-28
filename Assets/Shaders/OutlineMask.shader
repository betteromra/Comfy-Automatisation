Shader "Hidden/OutlineMask"
{
<<<<<<< HEAD
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
=======
    Properties 
    { 
        _OutlineColor("Outline Color", Color) = (0,1,0,1)
        _EdgeThickness("Edge Thickness", Range(0.1, 5.0)) = 1.0
        _DepthSensitivity("Depth Sensitivity", Range(0.0, 10.0)) = 1.0
        _NormalSensitivity("Normal Sensitivity", Range(0.0, 10.0)) = 1.0
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
<<<<<<< HEAD

        ZWrite On
        ZTest LEqual
        Cull Back

        Pass
        {
            Name "Simple Mask"

=======
        Pass
        {
            Name "Edge Detection Mask"
            
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
<<<<<<< HEAD

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

=======
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

            struct Attributes 
            { 
                float4 positionOS : POSITION; 
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings 
            { 
                float4 positionHCS : SV_POSITION; 
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float depth : TEXCOORD2;
            };

            float4 _OutlineColor;
            float _EdgeThickness;
            float _DepthSensitivity;
            float _NormalSensitivity;

>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
<<<<<<< HEAD
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Return pure white where objects are rendered
                // This creates a simple silhouette mask
                return half4(1, 1, 1, 1);
=======
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                // Calculate depth in view space
                float3 positionVS = TransformWorldToView(TransformObjectToWorld(IN.positionOS.xyz));
                OUT.depth = -positionVS.z;
                
                return OUT;
            }

            // Sobel edge detection using depth and normals
            half EdgeDetect(float2 uv, float2 texelSize)
            {
                // Sample depth and normals around the current pixel
                half depthEdge = 0;
                half normalEdge = 0;
                
                // Sobel filter for edge detection
                float2 offsets[9] = {
                    float2(-1, -1), float2(0, -1), float2(1, -1),
                    float2(-1,  0), float2(0,  0), float2(1,  0),
                    float2(-1,  1), float2(0,  1), float2(1,  1)
                };
                
                // Sobel kernels
                float sobelX[9] = { -1, 0, 1, -2, 0, 2, -1, 0, 1 };
                float sobelY[9] = { -1, -2, -1, 0, 0, 0, 1, 2, 1 };
                
                float gx_depth = 0, gy_depth = 0;
                float3 gx_normal = 0, gy_normal = 0;
                
                for (int i = 0; i < 9; i++)
                {
                    float2 sampleUV = uv + offsets[i] * texelSize * _EdgeThickness;
                    
                    // Sample depth
                    float depth = SampleSceneDepth(sampleUV);
                    gx_depth += depth * sobelX[i];
                    gy_depth += depth * sobelY[i];
                    
                    // Sample normals
                    float3 normal = SampleSceneNormals(sampleUV);
                    gx_normal += normal * sobelX[i];
                    gy_normal += normal * sobelY[i];
                }
                
                // Calculate edge magnitude
                depthEdge = sqrt(gx_depth * gx_depth + gy_depth * gy_depth) * _DepthSensitivity;
                normalEdge = length(gx_normal) + length(gy_normal);
                normalEdge *= _NormalSensitivity;
                
                return saturate(depthEdge + normalEdge);
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Get screen UV coordinates
                float2 screenUV = IN.positionHCS.xy / _ScreenParams.xy;
                float2 texelSize = 1.0 / _ScreenParams.xy;
                
                // Detect edges
                half edge = EdgeDetect(screenUV, texelSize);
                
                // Return white where edges are detected
                return half4(edge, edge, edge, edge);
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
            }
            ENDHLSL
        }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
