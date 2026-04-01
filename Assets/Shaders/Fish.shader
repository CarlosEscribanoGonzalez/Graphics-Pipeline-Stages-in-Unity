Shader "PGATR/Fish"
{
    Properties
    {
		[Header(Properties)]
		_TailPosition("TailPosition", Range(0, 1)) = 0.9
		_MaxTailAngle("MaxTailAngle", Range(-120, 120)) = 45
		_FishTexture("FishTexture", 2D) = "white" {}
    }

	CGINCLUDE
	#include "UnityCG.cginc"
	#include "Autolight.cginc"
	#define NUM_VERTEX 12
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
			#pragma multi_compile_fog
            
			#include "Lighting.cginc"

			float _TailPosition;
			sampler2D _FishTexture;
			float _MaxTailAngle;

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
				UNITY_FOG_COORDS(1)
			};

			GeometryOutput GenerateVertex(float3 pos, float2 uv)
			{
				GeometryOutput go;
				go.pos = mul(UNITY_MATRIX_P, float4(pos, 1));
				go.uv = uv;
				UNITY_TRANSFER_FOG(go, go.pos);
				return go;
			}

			[maxvertexcount(NUM_VERTEX)]
			void geo(point VertexOutput IN[1], inout TriangleStream<GeometryOutput> stream)
			{
				int idx = IN[0].ID;
				//Configuración de ejes:
				float3x3 rotation = AngleAxis3x3(radians(entityData[idx].rotation), float3(0, 0, -1));
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
				float3 midpoint0 = point0 + _TailPosition * (point3 - point0); //Punto cola pez arriba
				float3 midpoint1 = point1 + _TailPosition * (point2 - point1); //Punto cola pez abajo
				//Coleteo:
				float angle = sin(_Time.y * entityData[idx].speed) * _MaxTailAngle;
				float3x3 tailRot = AngleAxis3x3(radians(angle), up);
				point2 = mul(tailRot, point2 - midpoint1) + midpoint1;
				point3 = mul(tailRot, point3 - midpoint0) + midpoint0;
				//Triángulo cuerpo inferior izquierdo:
				stream.Append(GenerateVertex(point0, float2(0, 1)));
				stream.Append(GenerateVertex(point1, float2(0, 0)));
				stream.Append(GenerateVertex(midpoint1, float2(_TailPosition, 0)));
				//Triángulo cuerpo superior derecho:
				stream.Append(GenerateVertex(point0, float2(0, 1)));
				stream.Append(GenerateVertex(midpoint1, float2(_TailPosition, 0)));
				stream.Append(GenerateVertex(midpoint0, float2(_TailPosition, 1)));
				//Triángulo cola inferior izquierdo:
				stream.Append(GenerateVertex(midpoint0, float2(_TailPosition, 1)));
				stream.Append(GenerateVertex(midpoint1, float2(_TailPosition, 0)));
				stream.Append(GenerateVertex(point2, float2(1, 0)));
				//Triángulo cola superior derecho: 
				stream.Append(GenerateVertex(midpoint0, float2(_TailPosition, 1)));
				stream.Append(GenerateVertex(point2, float2(1, 0)));
				stream.Append(GenerateVertex(point3, float2(1, 1)));
			}

			//FRAGMENTS
			float4 frag (GeometryOutput IN, fixed facing : VFACE) : SV_Target
            {	
				float4 c = tex2Dlod(_FishTexture, float4(IN.uv, 0, 0));
				clip(c - 0.05);
				UNITY_APPLY_FOG(IN.fogCoord, c);
				return c;
            }
            ENDCG
        }
    }
}