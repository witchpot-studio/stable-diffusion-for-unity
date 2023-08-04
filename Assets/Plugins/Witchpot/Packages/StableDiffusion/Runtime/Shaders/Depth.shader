Shader "Witchpot/PostProcess/Depth"
{
    Properties
    { }

        SubShader
    {
        Tags { "RenderType" = "Unlit" "RenderPipeline" = "UniversalPipeline"}
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "DepthPass"

            HLSLPROGRAM

            #include "./witchpot.cginc"

            #if UNITY_VERSION >= UNITY_URP_14
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
            #else
                #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            #endif

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            #pragma shader_feature_local _ TO_TEXTURE

            #pragma vertex Vert
            #pragma fragment Frag

            half4 Frag(Varyings input) : SV_Target
            {
                #if UNITY_VERSION >= UNITY_URP_14
                    float d = SampleSceneDepth(input.texcoord);
                #else
                    float d = SampleSceneDepth(input.uv);
                #endif

                float3 depth = float3(d, d, d);
                depth = abs(depth);

                #if !UNITY_COLORSPACE_GAMMA && !TO_TEXTURE
                    depth = SRGBToLinear(depth);
                #endif

                float4 color = float4(depth, 1.0);
                return color;
            }
            ENDHLSL
        }
    }
}