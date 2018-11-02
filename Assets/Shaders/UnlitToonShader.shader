Shader "Toon/Simple toon (unlit)"
{
	Properties
	{
		_Tint ("Tint color", Color) = (1, 1, 1, 1)
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
    {
        Pass
        {
           CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag       
            #include "UnityCG.cginc"

            struct v2f {
                half3 worldNormal : TEXCOORD0;
                float4 pos : SV_POSITION;
                float4 wpos : TEXCOORD1;
            };

            v2f vert(float4 vertex : POSITION, float3 normal : NORMAL) {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.worldNormal = UnityObjectToWorldNormal(normal);
                o.wpos = mul(unity_ObjectToWorld, vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                fixed4 c = 0;

                float3 worldUp = UNITY_MATRIX_V[1]; //up vector of the camera in world space
                float3x3 worldToScreen;

                //z direction is the vector from camera to fragment
                worldToScreen[2] = UnityWorldSpaceViewDir(i.wpos);

                //create an orthogonal rotation matrix based around z being WorldSpaceViewDir
                worldToScreen[0] = cross(worldToScreen[2], worldUp);
                worldToScreen[1] = cross(worldToScreen[0], worldToScreen[2]); //ensure y vector is perpendicular to the other vectors

                float3 clipSpaceNormal = mul(worldToScreen, i.worldNormal);
                c.rgb = clipSpaceNormal;
                return c;
            }
            ENDCG
        }
    }
}
