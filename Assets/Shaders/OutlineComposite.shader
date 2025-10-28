Shader "Hidden/OutlineComposite"
{
<<<<<<< HEAD
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _BlurTex ("Blur Texture", 2D) = "white" {}
        _MaskTex ("Mask Texture", 2D) = "white" {}
        _OutlineColor("Outline Color", Color) = (0,1,0,1)
=======
    Properties 
    { 
        _MainTex ("Main Texture", 2D) = "white" {}
        _BlurTex ("Blur Texture", 2D) = "white" {} 
        _OutlineColor("Outline Color", Color) = (0,1,0,1) 
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
<<<<<<< HEAD

=======
        
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
        // Blend to overlay on top of scene
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off
<<<<<<< HEAD

        Pass
        {
            Name "Outline Composite"

=======
        
        Pass
        {
            Name "Outline Composite"
            
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

<<<<<<< HEAD
            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlurTex);
            SAMPLER(sampler_BlurTex);
            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);
=======
            struct Attributes 
            { 
                uint vertexID : SV_VertexID;
            };
            
            struct Varyings 
            { 
                float4 positionHCS : SV_POSITION; 
                float2 uv : TEXCOORD0; 
            };

            TEXTURE2D(_MainTex); 
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlurTex); 
            SAMPLER(sampler_BlurTex);
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
            float4 _OutlineColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // Fullscreen triangle
                OUT.positionHCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                OUT.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
<<<<<<< HEAD
                // Traditional outline shader approach:
                // 1. Sample the original mask (white where objects are)
                half originalMask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, IN.uv).r;

                // 2. Sample the blurred mask (expanded white area)
                half blurredMask = SAMPLE_TEXTURE2D(_BlurTex, sampler_BlurTex, IN.uv).r;

                // 3. Multiply blurred mask by inverse of original mask
                // This gives us ONLY the outline (expanded area - original shape)
                half outlineOnly = blurredMask * (1.0 - originalMask);

                // 4. Return outline color with alpha based on outline intensity
                // The blend mode will composite this over the scene
                return half4(_OutlineColor.rgb, outlineOnly * _OutlineColor.a);
=======
                // Sample the blurred mask - use RED channel since mask outputs white (1,1,1,1)
                half4 blur = SAMPLE_TEXTURE2D(_BlurTex, sampler_BlurTex, IN.uv);
                
                // Use the blur's red channel as mask intensity
                half outlineMask = blur.r;
                
                // Return outline color with alpha based on mask
                // The blend mode will composite this over the scene
                return half4(_OutlineColor.rgb, outlineMask * _OutlineColor.a);
>>>>>>> bc3ca86f5c604b2a3ff6044e9b68b13353598e41
            }
            ENDHLSL
        }
    }
}
