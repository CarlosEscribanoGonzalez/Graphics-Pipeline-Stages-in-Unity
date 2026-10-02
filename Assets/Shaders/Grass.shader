Shader "PGATR/Grass"
{
    Properties
    {
		[Header(Shading)]
        _TopColor("Top Color", Color) = (1,1,1,1)
		_BottomColor("Bottom Color", Color) = (1,1,1,1)
		_TranslucentGain("Translucent Gain", Range(0,1)) = 0.5
		_BendRotationRandom("Bend Rotation Random", Range(0, 1)) = 0.2
		_BladeWidth("Blade Width", Float) = 0.05
		_BladeWidthRandom("Blade Width Random", Float) = 0.02
		_BladeHeight("Blade Height", Float) = 0.5
		_BladeHeightRandom("Blade Height Random", Float) = 0.3
		_WindDistortionMap("Wind Distortion Map", 2D) = "white" {}
		_WindFrequency("Wind Frequency", Vector) = (0.05, 0.05, 0, 0)
		_WindStrength("Wind Strength", Float) = 1
		_BladeForward("Blade Forward Amount", Float) = 0.38
		_BladeCurve("Blade Curve Amount", Range(1, 4)) = 2
		_TessellationUniform("Tessellation Uniform", Range(1, 64)) = 1
    }

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"
	#define BLADE_SEGMENTS 3

	// Simple noise function, sourced from http://answers.unity.com/answers/624136/view.html
	// Extended discussion on this function can be found at the following link:
	// https://forum.unity.com/threads/am-i-over-complicating-this-random-function.454887/#post-2949326
	// Returns a number in the 0...1 range.
	float rand(float3 co)
	{
		return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 53.539))) * 43758.5453);
	}

	// Construct a rotation matrix that rotates around the provided axis, sourced from:
	// https://gist.github.com/keijiro/ee439d5e7388f3aafc5296005c8c3f33
	float3x3 AngleAxis3x3(float angle, float3 axis)
	{
		float c, s;
		sincos(angle, s, c);

		float t = 1 - c;
		float x = axis.x;
		float y = axis.y;
		float z = axis.z;

		return float3x3(
			t * x * x + c, t * x * y - s * z, t * x * z + s * y,
			t * x * y + s * z, t * y * y + c, t * y * z - s * x,
			t * x * z - s * y, t * y * z + s * x, t * z * z + c
			);
	}
	ENDCG

    SubShader
    {
		//GRASS PASS:
        Pass
        {
			Cull Off

			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma geometry geo
			#pragma hull hull
			#pragma domain domain
			#pragma target 4.6
            
			#include "Lighting.cginc"

			float4 _TopColor;
			float4 _BottomColor;
			float _TranslucentGain;
			float _BendRotationRandom;
			float _BladeWidth;
			float _BladeWidthRandom;
			float _BladeHeight;
			float _BladeHeightRandom;
			sampler2D _WindDistortionMap;
			float4 _WindDistortionMap_ST;
			float2 _WindFrequency;
			float _WindStrength;
			float _BladeForward;
			float _BladeCurve;
			float _TessellationUniform;
			
			//VERTEX
			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			VertexOutput vert(VertexInput vert)
			{
				VertexOutput o;
				o.vertex = vert.vertex;
				o.normal = vert.normal;
				o.tangent = vert.tangent;
				return o;
			}

			//TESSELLATION:
			struct TessellationFactors
			{
				float edge[3] : SV_TESSFACTOR;
				float inside : SV_INSIDETESSFACTOR;
			};

			TessellationFactors patchConstantFunction (InputPatch<VertexInput, 3> patch)
			{
				TessellationFactors f;
				f.edge[0] = _TessellationUniform;
				f.edge[1] = _TessellationUniform;
				f.edge[2] = _TessellationUniform;
				f.inside = _TessellationUniform;
				return f;
			}

			[UNITY_domain("tri")]
			[UNITY_outputcontrolpoints(3)]
			[UNITY_outputtopology("triangle_cw")]
			[UNITY_partitioning("integer")]
			[UNITY_patchconstantfunc("patchConstantFunction")]
			VertexInput hull(InputPatch<VertexInput, 3> patch, uint id : SV_OUTPUTCONTROLPOINTID)
			{
				return patch[id];
			}

			[UNITY_domain("tri")]
			VertexOutput domain(TessellationFactors factors, OutputPatch<VertexInput, 3> patch, 
									float3 barycentricCoordinates : SV_DOMAINLOCATION)
			{
				VertexInput v;

				#define MY_DOMAIN_PROGRAM_INTERPOLATE(fieldName) v.fieldName = \
					patch[0].fieldName * barycentricCoordinates.x + \
					patch[1].fieldName * barycentricCoordinates.y + \
					patch[2].fieldName * barycentricCoordinates.z;

				MY_DOMAIN_PROGRAM_INTERPOLATE(vertex)
				MY_DOMAIN_PROGRAM_INTERPOLATE(normal)
				MY_DOMAIN_PROGRAM_INTERPOLATE(tangent)

				return vert(v);
			}

			//GEOMETRY:
			struct GeometryOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			GeometryOutput GenerateGrassVertex(float3 vertexPosition, float width, float height, 
				float forward, float2 uv, float3x3 transformMatrix)
			{
				GeometryOutput go;
				float3 tangentPoint = float3(width, forward, height);
				go.pos = UnityObjectToClipPos(vertexPosition + mul(transformMatrix, tangentPoint));
				go.uv = uv;
				return go;
			}

			[maxvertexcount(BLADE_SEGMENTS * 2 + 1)]
			void geo(triangle VertexOutput IN[3], inout TriangleStream<GeometryOutput> stream)
			{
				float3 pos = IN[1].vertex;
				float2 uv = pos.xz * _WindDistortionMap_ST.xy + _WindDistortionMap_ST.zw + _WindFrequency * _Time.y;
				float2 windSample = (tex2Dlod(_WindDistortionMap, float4(uv, 0, 0)).xy * 2 - 1) * _WindStrength;
				float3 wind = normalize(float3(windSample.x, windSample.y, 0));

				//TBN:
				float3 vNormal = IN[1].normal;
				float4 vTangent = IN[1].tangent;
				float3 vBinormal = cross(vNormal, vTangent) * vTangent[3];
				float3x3 TBN = float3x3(
					vTangent.x, vBinormal.x, vNormal.x,
					vTangent.y, vBinormal.y, vNormal.y,
					vTangent.z, vBinormal.z, vNormal.z
				);
				//Wind:
				float3x3 windRotation = AngleAxis3x3(UNITY_PI * windSample.x, wind);
				//Random facing:
				float3x3 faceRotation = AngleAxis3x3(rand(pos) * UNITY_TWO_PI, float3(0, 0, 1));
				//Random bend:
				float3x3 bendRotation = AngleAxis3x3(rand(pos.zzx) * _BendRotationRandom * UNITY_PI * 0.5, float3(-1, 0, 0));
				//Final transformation matrix:
				float3x3 t_onlyFacing = mul(TBN, faceRotation);
				float3x3 transformMat = mul(mul(mul(TBN, windRotation), faceRotation), bendRotation);

				//Vertex generation:
				float baseHeight = (rand(pos.zyx) * 2 - 1) * _BladeHeightRandom + _BladeHeight;
				float baseWidth = (rand(pos.xzy) * 2 - 1) * _BladeWidthRandom + _BladeWidth;
				float topForward = rand(pos.yyz) * _BladeForward;
				for(int i = 0; i < BLADE_SEGMENTS; i++)
				{
					float t = (float)i / BLADE_SEGMENTS;
					float height = baseHeight * t;
					float width = baseWidth * (1 - t);
					float forward = pow(t, _BladeCurve) * topForward;
					float3x3 transfMatrix = height == 0 ? t_onlyFacing : transformMat;
					float u1 = (width / baseWidth) * 0.5 + 0.5;
					float u2 = (-width / baseWidth) * 0.5 + 0.5;
					float v = height / baseHeight;
					stream.Append(GenerateGrassVertex(pos, width, height, forward, float2(u1, v), transfMatrix));
					stream.Append(GenerateGrassVertex(pos, -width, height, forward, float2(u2, v), transfMatrix));
				}
				stream.Append(GenerateGrassVertex(pos, 0, baseHeight, topForward, float2(0.5, 1), transformMat));
			}

			//FRAGMENTS
			float4 frag (GeometryOutput IN, fixed facing : VFACE) : SV_Target
            {	
				return lerp(_BottomColor, _TopColor, IN.uv.y);
            }
            ENDCG
        }

		//FLOOR PASS:
		Pass
        {
			Cull Back

			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma target 4.6
            
			#include "Lighting.cginc"

			float4 _TopColor;
			float4 _BottomColor;
			float _TranslucentGain;

			//VERTEX
			float4 vert(float4 vertex : POSITION) : SV_POSITION
			{
				return UnityObjectToClipPos(vertex);
			}

			//FRAGMENTS
			float4 frag (float4 vertex : SV_POSITION, fixed facing : VFACE) : SV_Target
            {	
				return _BottomColor;
            }
            ENDCG
        }
    }
}