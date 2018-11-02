Shader "MUPS/UI/Saturation-value gradient"
{
	Properties
	{
		_Hue ("Hue", range(0, 1)) = 0
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

            float _Hue;

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

			float3 hsv2rgb(float3 hsv)
			{
				float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				float3 p = abs(frac(hsv.xxx + K.xyz) * 6.0 - K.www);
				return hsv.z * lerp(K.xxx, saturate(p - K.xxx), hsv.y);
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			float3 frag (v2f i) : SV_Target
			{
				float3 col = hsv2rgb(float3(_Hue, i.uv.x, i.uv.y));
				return col;
			}
			ENDCG
		}
	}
}
