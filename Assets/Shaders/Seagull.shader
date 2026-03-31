Shader "PGATR/Seagull"
{
    Properties
    {
		[Header(Properties)]
		_WingsPosition("WingsPosition", Range(0, 1)) = 0.9
		_MaxWingAngle("MaxWingAngle", Range(-120, 120)) = 45
		_SeagullTexture("SeagullTexture", 2D) = "white" {}
		_StartingRot("StartingRot", Float) = 90
    }

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"
	#define NUM_VERTEX 18
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
        Pass
        {
			Cull Off

			Tags
			{
				"RenderType" = "Opaque"
				"LightMode" = "ForwardBase"
			}

            CGPROGRAM
			#pragma exclude_renderers gles
            #pragma vertex vert
            #pragma fragment frag
			#pragma geometry geo
			#pragma target 4.6
            
			#include "Lighting.cginc"

			float _WingsPosition;
			sampler2D _SeagullTexture;
			float _MaxWingAngle;
			float _StartingRot;

			//VERTEX
			struct VertexInput
			{
				float4 vertex : POSITION;
				uint ID : SV_VERTEXID;
			};

			struct VertexOutput
			{
				float4 vertex : SV_POSITION;
				uint ID : TEXCOORD0;
			};

			VertexOutput vert(VertexInput vert)
			{
				VertexOutput o;
				o.vertex = mul(UNITY_MATRIX_MV, vert.vertex);
				o.ID = vert.ID;
				return o;
			}
			
			//GEOMETRY:
			struct EntityData
			{
				bool flip;
				float rotation;
				float speed;
				float sizeX;
				float sizeY;
			};
			StructuredBuffer<EntityData> entityData;

			struct GeometryOutput
			{
				float4 pos : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			GeometryOutput GenerateVertex(float3 pos, float2 uv)
			{
				GeometryOutput go;
				go.pos = mul(UNITY_MATRIX_P, float4(pos, 1));
				go.uv = uv;
				return go;
			}

			[maxvertexcount(NUM_VERTEX)]
			void geo(point VertexOutput IN[1], inout TriangleStream<GeometryOutput> stream)
			{
				int idx = IN[0].ID;
				//Configuración de ejes:
				float startRot = _StartingRot * (entityData[idx].flip ? -1 : 1);
				float3x3 rotation = AngleAxis3x3(radians(startRot), float3(0, 0, -1));
				float3 right = mul(rotation, float3(1, 0, 0));
				float3 forward = mul(rotation, float3(0, 0, 1));
				float3 up = cross(forward, right);
				float3 offset_x = 0.5f * entityData[idx].sizeX * right * (entityData[idx].flip ? -1 : 1);
				float3 offset_y = 0.5f * entityData[idx].sizeY * up;
				//Creación de vértices:
				float3 center = IN[0].vertex.xyz;
				float3 point0 = center - offset_x + offset_y; //Arriba izquierda
				float3 point1 = center - offset_x - offset_y; //Abajo izquierda
				float3 point2 = center + offset_x - offset_y; //Abajo derecha
				float3 point3 = center + offset_x + offset_y; //Arriba derecha
				float3 midpoint0 = point0 + _WingsPosition * (point3 - point0); //Punto ala izq arriba
				float3 midpoint1 = point1 + _WingsPosition * (point2 - point1); //Punto ala izq abajo
				float3 midpoint2 = point0 + (1 - _WingsPosition) * (point3 - point0); //Punto ala dcha arriba
				float3 midpoint3 = point1 + (1 - _WingsPosition) * (point2 - point1); //Punto ala dcha abajo
				//Coleteo:
				float angle = sin(_Time.y * entityData[idx].speed) * _MaxWingAngle;
				float3x3 wingRot_left = AngleAxis3x3(radians(angle), up);
				float3x3 wingRot_right = AngleAxis3x3(radians(-angle), up);
				point0 = mul(wingRot_left, point0 - midpoint0) + midpoint0;
				point1 = mul(wingRot_left, point1 - midpoint1) + midpoint1;
				point2 = mul(wingRot_right, point2 - midpoint3) + midpoint3;
				point3 = mul(wingRot_right, point3 - midpoint2) + midpoint2;
				//Ala izquierda: 
				stream.Append(GenerateVertex(point0, float2(0, 1)));
				stream.Append(GenerateVertex(point1, float2(0, 0)));
				stream.Append(GenerateVertex(midpoint1, float2(_WingsPosition, 0)));
				stream.Append(GenerateVertex(point0, float2(0, 1)));
				stream.Append(GenerateVertex(midpoint1, float2(_WingsPosition, 0)));
				stream.Append(GenerateVertex(midpoint0, float2(_WingsPosition, 1)));
				//Cuerpo:
				stream.Append(GenerateVertex(midpoint0, float2(_WingsPosition, 1)));
				stream.Append(GenerateVertex(midpoint1, float2(_WingsPosition, 0)));
				stream.Append(GenerateVertex(midpoint3, float2(1 - _WingsPosition, 0)));
				stream.Append(GenerateVertex(midpoint0, float2(_WingsPosition, 1)));
				stream.Append(GenerateVertex(midpoint3, float2(1 - _WingsPosition, 0)));
				stream.Append(GenerateVertex(midpoint2, float2(1 - _WingsPosition, 1)));
				////Ala derecha: 
				stream.Append(GenerateVertex(midpoint2, float2(1 - _WingsPosition, 1)));
				stream.Append(GenerateVertex(midpoint3, float2(1 - _WingsPosition, 0)));
				stream.Append(GenerateVertex(point2, float2(1, 0)));
				stream.Append(GenerateVertex(midpoint2, float2(1 - _WingsPosition, 1)));
				stream.Append(GenerateVertex(point2, float2(1, 0)));
				stream.Append(GenerateVertex(point3, float2(1, 1)));
			}

			//FRAGMENTS
			float4 frag (GeometryOutput IN, fixed facing : VFACE) : SV_Target
            {	
				float4 c = tex2Dlod(_SeagullTexture, float4(IN.uv, 0, 0));
				clip(c - 0.05);
				return c;
            }
            ENDCG
        }
    }
}