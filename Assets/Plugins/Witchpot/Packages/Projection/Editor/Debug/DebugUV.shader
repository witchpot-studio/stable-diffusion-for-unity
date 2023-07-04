shader "Witchpot/Debug/DebugUV"
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

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.uv = IN.uv.xy;

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				half4 color = half4(IN.uv.xy, 0, 1);

				return color;
			}

			ENDHLSL
		}
	}
}
