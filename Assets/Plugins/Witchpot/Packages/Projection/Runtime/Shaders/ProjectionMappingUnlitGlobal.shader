Shader "Witchpot/ProjectionMappingUnlitGlobal"
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
				float4 normal : NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				float4 positionProjectorSpace : TEXCOORD0;
				float3 positionWorld : TEXCOORD1;
				float3 normalWorld : TEXCOORD2;
			};

			TEXTURE2D(_ProjectionMapGlobal);
			SAMPLER(sampler_ProjectionMapGlobal);
			float4x4 _ProjectorMatrixVPGlobal;
			float4 _ProjectorPositionGlobal;

			CBUFFER_START(UnityPerMaterial)
				float4 _ProjectionMapGlobal_ST;
			CBUFFER_END

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.positionProjectorSpace = mul(mul(_ProjectorMatrixVPGlobal, unity_ObjectToWorld), IN.positionOS);
				OUT.positionProjectorSpace = ComputeScreenPos(OUT.positionProjectorSpace);
				OUT.positionWorld = mul(unity_ObjectToWorld, IN.positionOS).xyz;
				OUT.normalWorld = normalize(mul(IN.normal.xyz, (float3x3)unity_WorldToObject));

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				IN.positionProjectorSpace.xyz /= IN.positionProjectorSpace.w;
				float2 uv = IN.positionProjectorSpace.xy;
				float4 projectorColor = SAMPLE_TEXTURE2D(_ProjectionMapGlobal, sampler_ProjectionMapGlobal, uv);
				half3 isOutOfCamera = step((IN.positionProjectorSpace - 0.5) * sign(IN.positionProjectorSpace), 0.5).xyz;
				float alpha = isOutOfCamera.x * isOutOfCamera.y * isOutOfCamera.z;
				alpha *= step(-dot(lerp(-_ProjectorPositionGlobal.xyz, _ProjectorPositionGlobal.xyz - IN.positionWorld, _ProjectorPositionGlobal.w), IN.normalWorld), 0);

				return projectorColor * alpha;
			}
			ENDHLSL
		}
	}
}
