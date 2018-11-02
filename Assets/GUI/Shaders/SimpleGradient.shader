Shader "MUPS/UI/RGB gradient"
{
	Properties
	{
		_Color1 ("Color 1", Color) = (1, 1, 1, 1)
        _Color2 ("Color 2", Color) = (0, 0, 0, 1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" "PreviewType"="Plane" "Queue"="Transparent" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

            float3 _Color1, _Color2;

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float3 frag (v2f i) : SV_Target
			{
				return lerp(_Color2, _Color1, i.uv.y);
			}
			ENDCG
		}
	}
}
