Shader "Witchpot/Debug/DebugProjectionImageDirect"
{
	Properties
	{
	}

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
			"ForceNoShadowCasting" = "True"
		}
		LOD 100

		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

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

			TEXTURE2D(_ProjectionMap);
			SAMPLER(sampler_ProjectionMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _ProjectionMap_ST;
			CBUFFER_END

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.uv = TRANSFORM_TEX(IN.uv, _ProjectionMap);

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				half4 color = SAMPLE_TEXTURE2D(_ProjectionMap, sampler_ProjectionMap, IN.uv);

				return color;
			}
			ENDHLSL
		}
	}
}
