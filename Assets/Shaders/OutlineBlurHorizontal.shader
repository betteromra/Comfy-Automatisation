Shader "Hidden/OutlineBlurHorizontal"
{
    Properties 
    { 
        _MainTex ("Texture", 2D) = "white" {} 
        _BlurSize("Blur Size", Range(0.5, 4)) = 1 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        ZWrite Off
        ZTest Always
        Cull Off
        
        Pass
        {
            Name "Horizontal Blur"
            
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

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _BlurSize;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = GetFullScreenTriangleVertexPosition(IN.vertexID);
                OUT.uv = GetFullScreenTriangleTexCoord(IN.vertexID);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 col = 0;
                for (int i = -2; i <= 2; i++)
                    col += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(i, 0) * _MainTex_TexelSize.xy * _BlurSize);
                return col / 5.0;
            }
            ENDHLSL
        }
    }
}