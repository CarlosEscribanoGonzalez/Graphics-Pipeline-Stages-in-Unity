Shader "PGATR/Water"
{
    Properties
    {
		[Header(Shading)]
        _Albedo("Albedo", Color) = (1,1,1,1)
		_Alpha("Alpha", Float) = 500
		[Header(Tessellation)]
		_Scale("Scale", Float) = 50
		_MaxTessellationFactor("Max Tessellation Factor", Range(1, 128)) = 50
		_MinTessellationFactor("Min Tessellation Factor", Range(1, 128)) = 1
		_MinDist("Min Dist", Float) = 3
		_MaxDist("Max Dist", Float) = 50
		_MaxHeight("Max Height", Float) = 2
		[Header(Waves)]
		_Displacement("Displacement", 2D) = "white"{}
		_NormalMap("Normal Map", 2D) = "white" {}
		_NormalMapStrength("Normal Map Strength", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Pass
        {
			Cull Back
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite On

			Tags
			{
				"Queue" = "Transparent"
				"RenderType" = "Transparent"
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
            
			#include "Lighting.cginc"

			float4 _Albedo;
			float _Alpha;
			float _Scale;
			float _MaxTessellationFactor;
			float _MinTessellationFactor;
			float _MinDist;
			float _MaxDist;
			float _MaxHeight;
			sampler2D _Displacement;
			float4 _Displacement_ST;
			sampler2D _NormalMap;
			float _NormalMapStrength;
			
			//VERTEX
			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float distToCam : TEXCOORD2;
			};

			VertexOutput vert(VertexInput vert)
			{
				VertexOutput o;
				o.vertex = vert.vertex;
				o.normal = vert.normal;
				o.tangent = vert.tangent;
				o.uv = vert.uv;
				o.distToCam = -mul(UNITY_MATRIX_MV, o.vertex).z;
				return o;
			}

			//TESSELLATION:
			struct TessellationFactors
			{
				float edge[3] : SV_TESSFACTOR;
				float inside : SV_INSIDETESSFACTOR;
			};

			TessellationFactors patchConstantFunction (InputPatch<VertexOutput, 3> patch)
			{
				float dist = min(patch[0].distToCam, min(patch[1].distToCam, patch[2].distToCam));
				dist = clamp(dist, _MinDist, _MaxDist);
				float w = (dist - _MinDist) / (_MaxDist - _MinDist);
				float tessFactor = lerp(_MinTessellationFactor, _MaxTessellationFactor, 1 - w);
				TessellationFactors f;
				f.edge[0] = tessFactor;
				f.edge[1] = tessFactor;
				f.edge[2] = tessFactor;
				f.inside = tessFactor;
				return f;
			}

			[UNITY_domain("tri")]
			[UNITY_outputcontrolpoints(3)]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_partitioning("integer")]
			[UNITY_patchconstantfunc("patchConstantFunction")]
			VertexOutput hull(InputPatch<VertexOutput, 3> patch, uint id : SV_OUTPUTCONTROLPOINTID)
			{
				return patch[id];
			}

			[UNITY_domain("tri")]
			VertexOutput domain(TessellationFactors factors, OutputPatch<VertexOutput, 3> patch, 
									float3 barycentricCoordinates : SV_DOMAINLOCATION)
			{
				VertexOutput v;

				#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
					patch[0].fieldName * barycentricCoordinates.x + \
					patch[1].fieldName * barycentricCoordinates.y + \
					patch[2].fieldName * barycentricCoordinates.z;

				MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
				MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
				MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)
				MY_DOMAIN_PROGRAM_INTERPOLATE(uv)
				float2 uv = v.uv.xy * _Displacement_ST.xy + _Displacement_ST.zw;
				float vertOffset = tex2Dlod(_Displacement, float4(uv, 0, 0)).x; //Desplazamiento vertical
				float4 pos = float4((v.vertex * _Scale).rgb, 1) + float4(0, vertOffset * _MaxHeight, 0, 0);
				v.worldPos = mul(UNITY_MATRIX_M, pos);
				v.vertex = UnityObjectToClipPos(pos);
				//Normales:
				float3 normal = normalize(UnityObjectToWorldNormal(v.normal));
				float3 tangent = normalize(UnityObjectToWorldDir(v.tangent.xyz));
				float3 bitangent = cross(normal, tangent) * v.tangent.w;
				float3x3 TBN = float3x3(tangent, bitangent, normal);
				float3 N = tex2Dlod(_NormalMap, float4(uv, 0, 0)).xyz * 2.0 - 1.0;
				v.normal = lerp(normal, normalize(mul(N, TBN)), _NormalMapStrength);
				return v;
			}

			float4 frag(float3 worldPos : TEXCOORD1, float3 N : NORMAL) : SV_Target
			{
				N = normalize(N);
				float3 V = normalize(_WorldSpaceCameraPos - worldPos);
				float3 L = normalize(_WorldSpaceLightPos0.xyz);
				float3 H = normalize(L + V);
				// Ambiental
				float3 color = UNITY_LIGHTMODEL_AMBIENT.rgb * _Albedo.rgb;
				// Difuso
				float NdotL = saturate(dot(N, L));
				color += _LightColor0.rgb * _Albedo.rgb * NdotL * 0.3;
				// Fresnel
				float fresnel = pow(1.0 - saturate(dot(N, V)), 4.0);
				fresnel = lerp(0.02, 1.0, fresnel); // F0 dieléctrico ~0.02 para agua
				// Especular Blinn-Phong atenuado por Fresnel
				float NdotH = saturate(dot(N, H));
				float spec = pow(NdotH, _Alpha);
				color += _LightColor0.rgb * spec * fresnel;
				return float4(saturate(color), _Albedo.w);
			}
            ENDCG
        }
    }
}