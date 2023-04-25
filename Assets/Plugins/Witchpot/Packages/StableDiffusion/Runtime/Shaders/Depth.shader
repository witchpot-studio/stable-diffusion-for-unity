Shader "Witchpot/PostProcess/Depth"
{
    Properties
    { }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            TEXTURE2D_X(_MainTex);
            SAMPLER(sampler_MainTex);
            float _Weight;
            float4x4 _ViewToWorld;

            half4 Frag(Varyings IN) : SV_Target
            {
                float4 mainColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float depth = SampleSceneDepth(IN.uv);

                return lerp(mainColor, depth, _Weight);
            }

            ENDHLSL
        }
    }
}
