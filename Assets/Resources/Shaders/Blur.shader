// Reference: https://www.ronja-tutorials.com/2018/08/27/postprocessing-blur.html

Shader "Compute/Blur" {
	Properties{
		[HideInInspector]_MainTex ("Texture", 2D) = "white" {}
		_BlurSize("Blur Size", Range(0,0.5)) = 0
		[Toggle(GAUSS)] _Gauss ("Gaussian Blur", float) = 0
		[Toggle(R2G)] _R2G ("Blur from R to G", float) = 0
		[PowerSlider(3)]_StandardDeviation("Standard Deviation (Gauss only)", Range(0.00, 0.3)) = 0.02
	}

	SubShader{
		Cull Off
		ZWrite Off 
		ZTest Always

		Pass{
            name "Vertical"
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			#pragma shader_feature GAUSS
			#pragma shader_feature R2G

			sampler2D _MainTex;
			float _BlurSize;
			float _StandardDeviation;

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
			#if GAUSS
				if (_StandardDeviation == 0)
				{
					fixed4 color = tex2D(_MainTex, i.uv);
			#if R2G
					return fixed4(color.r, color.r, 0, 0);
			#else
					return color;
			#endif // R2G
				}
			#endif // GAUSS
			#if R2G
				float  col = 0;
			#else
				float4 col = 0;
			#endif // R2G
			#if GAUSS
				float sum = 0;
			#else
				float sum = SAMPLES;
			#endif // GAUSS
				for (float index = 0; index < SAMPLES; index++) 
				{
					float offset = (index/(SAMPLES-1) - 0.5) * _BlurSize;
					float2 uv = i.uv + float2(0, offset);
			#if !GAUSS
			#if R2G
					col += tex2D(_MainTex, uv).r;
			#else
					col += tex2D(_MainTex, uv);
			#endif // R2G
			#else
					float stDevSquared = _StandardDeviation*_StandardDeviation;
					float gauss = (1 / sqrt(2*PI*stDevSquared)) * pow(E, -((offset*offset)/(2*stDevSquared)));
					sum += gauss;
			#if R2G
					col += tex2D(_MainTex, uv).r * gauss;
			#else
					col += tex2D(_MainTex, uv) * gauss;
			#endif // R2G
			#endif // !GAUSS
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

			#pragma shader_feature GAUSS
			#pragma shader_feature R2G

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float _BlurSize;
			float _StandardDeviation;

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
			#if GAUSS
				if (_StandardDeviation == 0)
				{
					fixed4 color = tex2D(_MainTex, i.uv);
			#if R2G
					return fixed4(color.r, color.r, 0, 0);
			#else
					return color;
			#endif // R2G
				}
			#endif // GAUSS
			#if R2G
				float  col = 0;
			#else
				float4 col = 0;
			#endif // R2G
			#if GAUSS
				float sum = 0;
			#else
				float sum = SAMPLES;
			#endif // GAUSS
				for (float index = 0; index < SAMPLES; index++) {
					float offset = (index/(SAMPLES-1) - 0.5) * _BlurSize;
					float2 uv = i.uv + float2(offset, 0);
			#if !GAUSS
			#if R2G
					col += tex2D(_MainTex, uv).r;
			#else
					col += tex2D(_MainTex, uv);
			#endif // R2G
			#else
					float stDevSquared = _StandardDeviation*_StandardDeviation;
					float gauss = (1 / sqrt(2*PI*stDevSquared)) * pow(E, -((offset*offset)/(2*stDevSquared)));
					sum += gauss;
			#if R2G
					col += tex2D(_MainTex, uv).r * gauss;
			#else
					col += tex2D(_MainTex, uv) * gauss;
			#endif // R2G
			#endif // !GAUSS
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