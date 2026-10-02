Shader "Unlit/Outline"
{
    Properties
    {
        _Albedo("Albedo", Color) = (1,1,1,1)
        _Scale("Scale", Float) = 1.2
    }
    SubShader
    {
        Cull Front
        Blend SrcAlpha OneMinusSrcAlpha
	    ZWrite On

		Tags
		{
			"Queue" = "Transparent"
			"RenderType" = "Transparent"
			"LightMode" = "ForwardBase"
		}

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            float4 _Albedo;
            float _Scale;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex * _Scale);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_APPLY_FOG(i.fogCoord, _Albedo);
                return _Albedo;
            }
            ENDCG
        }
    }
}
