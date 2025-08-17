Shader "Custom/ColorSwap"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Tolerance("Tolerance", Range(0,0.01)) = 0.001

		_SkinColor("SkinColor", Color) = (1,1,1,1)
		_NewSkinColor("NewSkinColor", Color) = (1,1,1,1)
		_HairColor("HairColor", Color) = (1,1,1,1)
		_NewHairColor("NewHairColor", Color) = (1,1,1,1)
		_BodyColor("BodyColor", Color) = (1,1,1,1)
		_NewBodyColor("NewBodyColor", Color) = (1,1,1,1)
	}

	SubShader
	{
		Tags { "RenderType" = "Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

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

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _SkinColor;
			float4 _NewSkinColor;
			float4 _HairColor;
			float4 _NewHairColor;
			float4 _BodyColor;
			float4 _NewBodyColor;
			float _Tolerance;

			v2f vert(appdata v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);

				if(col.a == 0)
				{
					return half4(0,0,0,0);
				}

				if(length(col - _SkinColor) < _Tolerance)
				{
					return half4(_NewSkinColor.rgb, col.a);
				}

				if(length(col - _HairColor) < _Tolerance)
				{
					return half4(_NewHairColor.rgb, col.a);
				}

				if(length(col - _BodyColor) < _Tolerance)
				{
					return half4(_NewBodyColor.rgb, col.a);
				}

				return col;
			}

			ENDCG
		}
	}
}