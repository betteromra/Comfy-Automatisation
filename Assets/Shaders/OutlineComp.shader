Shader "Hidden/OutlineComposite"
{
    Properties 
    { 
        _MainTex ("Main Texture", 2D) = "white" {} 
        _BlurTex("Blur Texture", 2D) = "white" {} 
        _OutlineColor("Outline Color", Color) = (0,1,0,1) 
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            TEXTURE2D(_BlurTex); SAMPLER(sampler_BlurTex);
            float4 _OutlineColor;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 sceneColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half blur = SAMPLE_TEXTURE2D(_BlurTex, sampler_BlurTex, IN.uv).r;
                
                // Simple outline: add colored blur to scene
                half outline = smoothstep(0.1, 0.3, blur);
                half3 outlineColor = _OutlineColor.rgb * outline;
                
                return half4(sceneColor.rgb + outlineColor, sceneColor.a);
            }
            ENDHLSL
        }
    }
}