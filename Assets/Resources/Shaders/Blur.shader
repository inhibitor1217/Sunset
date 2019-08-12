// Reference: https://www.ronja-tutorials.com/2018/08/27/postprocessing-blur.html

Shader "Compute/Blur" {
	Properties
	{
		_MainTex ("Texture", 2D) = "white"
		_BlurSize ("Blur Size", Float) = .01
	}

	SubShader
	{
		Cull Off
		ZWrite Off 
		ZTest Always

		Pass{
            name "Vertical"
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature R2G

			sampler2D _MainTex;
			float _BlurSize;

			#define PI 3.14159265359
			#define E 2.71828182846

			#define SAMPLES 10

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
			#if R2G
				float  col = 0;
			#else
				float4 col = 0;
			#endif // R2G
				float sum = SAMPLES;
				for (float index = 0; index < SAMPLES; index++) 
				{
					float offset = (index/(SAMPLES-1) - 0.5) * _BlurSize;
					float2 uv = i.uv + float2(0, offset);
			#if R2G
					col += tex2D(_MainTex, uv).r;
			#else
					col += tex2D(_MainTex, uv);
			#endif // R2G
				}
				col = col / sum;
			#if R2G
				return fixed4(tex2D(_MainTex, i.uv).r, col, 0, 0);
			#else
				return col;
			#endif // R2G
			}

			ENDCG
		}

		Pass{
            name "Horizontal"
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma shader_feature R2G

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float _BlurSize;

			#define PI 3.14159265359
			#define E 2.71828182846

			#define SAMPLES 10

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET
			{
			#if R2G
				float  col = 0;
			#else
				float4 col = 0;
			#endif // R2G
				float sum = SAMPLES;
				for (float index = 0; index < SAMPLES; index++) {
					float offset = (index/(SAMPLES-1) - 0.5) * _BlurSize;
					float2 uv = i.uv + float2(offset, 0);
			#if R2G
					col += tex2D(_MainTex, uv).r;
			#else
					col += tex2D(_MainTex, uv);
			#endif // R2G
				}
				col = col / sum;
			#if R2G
				return fixed4(tex2D(_MainTex, i.uv).r, col, 0, 0);
			#else
				return col;
			#endif // R2G
			}

			ENDCG
		}
	}
}