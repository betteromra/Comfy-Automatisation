Shader "Hidden/OutlineComposite"
{
    Properties 
    { 
        _MainTex ("Main Texture", 2D) = "white" {}
        _BlurTex ("Blur Texture", 2D) = "white" {} 
        _OutlineColor("Outline Color", Color) = (0,1,0,1) 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        // Blend to overlay on top of scene
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off
        
        Pass
        {
            Name "Outline Composite"
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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
                // Sample the blurred mask - use RED channel since mask outputs white (1,1,1,1)
                half4 blur = SAMPLE_TEXTURE2D(_BlurTex, sampler_BlurTex, IN.uv);
                
                // Use the blur's red channel as mask intensity
                half outlineMask = blur.r;
                
                // Return outline color with alpha based on mask
                // The blend mode will composite this over the scene
                return half4(_OutlineColor.rgb, outlineMask * _OutlineColor.a);
            }
            ENDHLSL
        }
    }
}
