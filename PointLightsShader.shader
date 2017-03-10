Shader "Bowie Point Lights/Point Lights Shader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_AlphaMask ("Alpha Mask", 2D) = "black" {}
		_MaskStrength ( "Mask Strength", Float ) = 1.0
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct VertIn
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 ray : TEXCOORD1;
			};

			struct VertOut
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				float2 uv_depth : TEXCOORD1;
				float4 interpolatedRay : TEXCOORD2;
			};

			VertOut vert (VertIn v)
			{
				VertOut o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				o.uv_depth = v.uv.xy;

				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					o.uv.y = 1 - o.uv.y;
				#endif	

				o.interpolatedRay = v.ray;

				return o;
			}
			
			sampler2D _MainTex;
			sampler2D _AlphaMask;
			sampler2D_float _CameraDepthNormalsTexture;

			// The camera model view
			float4x4 _CameraMV;

			float _MaskStrength;
			float3 _LightPositions[128];
			float4 _LightColors[128];
			float _LightRanges[128];
			float _LightIntensitys[128];

			float3 lightDistanceNormalised ( float3 surfacePosition, float3 lightPosition, float lightRange ) {
				float dis = distance( surfacePosition, lightPosition );
				return 1.0 - clamp( dis / lightRange, 0.0, 1.0 );
			}

			float easeLightDistance ( float v ) {
				return v * v * v * v;
			}

			void addLight ( in out float4 surfaceColor, float3 surfacePosition, float3 surfaceNormal, float3 lightPosition, float lightRange, float4 lightColor, float lightIntensity ) {

				// Return the distance from the light
				float distanceFromLight = clamp( 1.0 - distance( surfacePosition, lightPosition ) / lightRange, 0, 1 );
				distanceFromLight = easeLightDistance( distanceFromLight );

				// Get the vector from the surface to the light
				float3 surfaceToLight = lightPosition - surfacePosition;

				// Get the brightness of the surface (cosine of the angle of incidence). 1 means it is facing the light. 0 means it's not
				float brightness = dot( surfaceNormal, surfaceToLight ) / (length( surfaceToLight ) * length( surfaceNormal ));
				brightness = clamp( brightness, 0, 1 );

				// Get the intensity of the light based on the distance and the direction
				float intensity = lerp( 0, brightness, distanceFromLight );
				intensity = intensity * lightIntensity;

				// Add the light to the surface color... there will be better ways
				surfaceColor = surfaceColor + ( lightColor * intensity ); 

			}

			fixed4 frag (VertOut i) : SV_Target
			{
				// Get the source & mask color
				float4 sourceColor = tex2D(_MainTex, i.uv);
				float4 maskColor = tex2D(_AlphaMask, i.uv);

				// Get the surface color
				float4 surfaceColor = sourceColor;

				// Get the depth & normal
				float rawDepth = 1.0;
				float3 viewSpaceNormal;
				DecodeDepthNormal( tex2D(_CameraDepthNormalsTexture, i.uv_depth.xy), rawDepth, viewSpaceNormal );

				// Get the world position
				float4 directionWorldSpace = rawDepth * i.interpolatedRay;
				float3 surfaceWorldPosition = _WorldSpaceCameraPos + directionWorldSpace;

				// Get the world normal
				float3 surfaceWorldNormal = mul((float3x3)_CameraMV, viewSpaceNormal);

				// Preformance test the lights
				for ( int i = 0; i < 128; i++ ) {
					addLight( surfaceColor, surfaceWorldPosition, surfaceWorldNormal, _LightPositions[ i ], _LightRanges[ i ], _LightColors[ i ], _LightIntensitys[ i ] );
				}


				return lerp( surfaceColor, sourceColor, maskColor.a * _MaskStrength );

			}

			ENDCG
		}
	}
}
