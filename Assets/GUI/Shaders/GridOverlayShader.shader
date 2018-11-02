Shader "MUPS/Unlit/Grid overlay"
{
	Properties
	{
    	_GridSize("Grid 1 size", float) = 0.5
		_GridColor("Grid 1 color", Color) = (1, 1, 1, 1)
		_GridThickness("Grid 1 line thickness", range(0,0.1)) = 0.02
		_Grid2Size("Grid 2 size", float) = 2.5
		_Grid2Color("Grid 2 color", Color) = (1, 1, 1, 1)
		_Grid2Thickness("Grid 2 line thickness", range(0,0.1)) = 0.02
      	_Alpha ("Alpha", Range(0,1)) = 1
		_Threshold ("Alpha threshold", Range(0, 1)) = 0.1
	}
	SubShader
	{
		Tags { "RenderType"="Overlay" }
		LOD 100
		ZWrite On

		Pass
		{
         	Blend SrcAlpha OneMinusSrcAlpha
         	Offset -20, -20

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
		
			#include "UnityCG.cginc"

			float _GridSize;
			float _Grid2Size;
			float4 _GridColor;
			float4 _Grid2Color;
			float _GridThickness;
			float _Grid2Thickness;
			float _Alpha;
			float _Threshold;

			struct appdata
			{
				float4 vertex : POSITION;
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
				o.uv = mul(unity_ObjectToWorld, v.vertex).xz;
				return o;
			}

			float DrawGrid(float2 uv, float size, float thickness)
			{
				float aaThresh = thickness;
				float aaMin = thickness*0.1;

				float2 gUV = uv / size + aaThresh;
				
				float2 fl = floor(gUV);
				gUV = frac(gUV);
				gUV -= aaThresh;
				gUV = smoothstep(aaThresh, aaMin, abs(gUV));
				float d = max(gUV.x, gUV.y);

				return d;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed g1 = DrawGrid(i.uv, _GridSize, _GridThickness);
				fixed g2 = DrawGrid(i.uv, _Grid2Size, _Grid2Thickness);
				fixed a = max(g1, g2) * _Alpha;
				clip(a - _Threshold);
				fixed4 rgb = clamp(_GridColor * g1 + _Grid2Color * g2, 0, 1);
				return float4(rgb.r, rgb.g, rgb.b, a);
			}
			ENDCG
		}
	}
}