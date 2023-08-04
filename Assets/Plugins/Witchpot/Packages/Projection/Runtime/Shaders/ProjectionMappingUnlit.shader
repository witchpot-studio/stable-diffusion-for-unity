Shader "Witchpot/ProjectionMappingUnlit"
{
	Properties
	{
		[HideInInspector] _ProjectionMap("ProjectionMap", 2D) = "white" {}
		[HideInInspector] _ProjectorPosition("ProjectorPosition", Vector) = (0, 0, 0, 0)
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

			TEXTURE2D(_ProjectionMap);
			SAMPLER(sampler_ProjectionMap);
			float4x4 _ProjectorMatrixVP;
			float4 _ProjectorPosition;

			CBUFFER_START(UnityPerMaterial)
				float4 _ProjectionMap_ST;
			CBUFFER_END

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.positionProjectorSpace = mul(mul(_ProjectorMatrixVP, unity_ObjectToWorld), IN.positionOS);
				OUT.positionProjectorSpace = ComputeScreenPos(OUT.positionProjectorSpace);
				OUT.positionWorld = mul(unity_ObjectToWorld, IN.positionOS).xyz;
				OUT.normalWorld = normalize(mul(IN.normal.xyz, (float3x3)unity_WorldToObject));

				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				IN.positionProjectorSpace.xyz /= IN.positionProjectorSpace.w;
				float2 uv = IN.positionProjectorSpace.xy;
				float4 projectorColor = SAMPLE_TEXTURE2D(_ProjectionMap, sampler_ProjectionMap, uv);
				half3 isOutOfCamera = step((IN.positionProjectorSpace - 0.5) * sign(IN.positionProjectorSpace), 0.5).xyz;
				float alpha = isOutOfCamera.x * isOutOfCamera.y * isOutOfCamera.z;
				alpha *= step(-dot(lerp(-_ProjectorPosition.xyz, _ProjectorPosition.xyz - IN.positionWorld, _ProjectorPosition.w), IN.normalWorld), 0);

				return projectorColor * alpha;
			}
			ENDHLSL
		}

		Pass
		{
			Name "DepthNormals"
			Tags { "LightMode" = "DepthNormals" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS : POSITION;
				float3 normalOS : NORMAL;
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float3 normalWS : TEXCOORD0;
			};

			Varyings vert(Attributes IN)
			{
				Varyings OUT = (Varyings)0;
				OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.normalWS = normalize(mul(IN.normalOS.xyz, (float3x3)unity_WorldToObject));

				return OUT;
			}

			half4 frag(Varyings IN) : SV_TARGET
			{
				return half4(IN.normalWS, 1);
			}

			ENDHLSL
		}
	}
}
