Shader "Witchpot/PostProcess/Normal"
{
	Properties
	{ }

	SubShader
	{
		Tags { "RenderType" = "Unlit" "RenderPipeline" = "UniversalPipeline" }
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			Name "NormalPass"

			HLSLPROGRAM

			#include "./witchpot.cginc"

			#if UNITY_VERSION >= UNITY_URP_14
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
				#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
			#else
				#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
			#endif

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareNormalsTexture.hlsl"

			#pragma shader_feature_local _ TO_TEXTURE

			#pragma vertex Vert
			#pragma fragment Frag

			float4x4 _WorldToView;

			half4 Frag(Varyings input) : SV_Target
			{
				#if UNITY_VERSION >= UNITY_URP_14
					float3 normal = SampleSceneNormals(input.texcoord);
				#else
					float3 normal = SampleSceneNormals(input.uv);
				#endif

				normal = mul((float3x3)_WorldToView, normal);
				normal.x = -normal.x;
				normal = abs((normal + 1.0) * 0.5);

				#if !UNITY_COLORSPACE_GAMMA && !TO_TEXTURE
					normal = SRGBToLinear(normal);
				#endif

				float4 color = float4(normal, 1.0);
				return color;
			}
			ENDHLSL
		}
	}
}