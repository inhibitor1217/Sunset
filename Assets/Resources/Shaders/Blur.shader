Shader "Compute/Blur" {
	Properties{
		[HideInInspector]_MainTex ("Texture", 2D) = "white" {}
		_BlurSize("Blur Size", Range(0,0.5)) = 0
		[KeywordEnum(Low, Medium, High)] _Samples ("Sample amount", Float) = 0
		[Toggle(GAUSS)] _Gauss ("Gaussian Blur", float) = 0
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

			#pragma multi_compile _SAMPLES_LOW _SAMPLES_MEDIUM _SAMPLES_HIGH
			#pragma shader_feature GAUSS

			sampler2D _MainTex;
			float _BlurSize;
			float _StandardDeviation;

			#define PI 3.14159265359
			#define E 2.71828182846

		#if _SAMPLES_LOW
			#define SAMPLES 10
		#elif _SAMPLES_MEDIUM
			#define SAMPLES 30
		#else
			#define SAMPLES 100
		#endif

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v){
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET{
			#if GAUSS
				if(_StandardDeviation == 0)
				return tex2D(_MainTex, i.uv);
			#endif
				float4 col = 0;
			#if GAUSS
				float sum = 0;
			#else
				float sum = SAMPLES;
			#endif
				for(float index = 0; index < SAMPLES; index++){
					float offset = (index/(SAMPLES-1) - 0.5) * _BlurSize;
					float2 uv = i.uv + float2(0, offset);
				#if !GAUSS
					col += tex2D(_MainTex, uv);
				#else
					float stDevSquared = _StandardDeviation*_StandardDeviation;
					float gauss = (1 / sqrt(2*PI*stDevSquared)) * pow(E, -((offset*offset)/(2*stDevSquared)));
					sum += gauss;
					col += tex2D(_MainTex, uv) * gauss;
				#endif
				}
				col = col / sum;
				return col;
			}

			ENDCG
		}

		Pass{
            name "Horizontal"
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma multi_compile _SAMPLES_LOW _SAMPLES_MEDIUM _SAMPLES_HIGH
			#pragma shader_feature GAUSS

			#pragma vertex vert
			#pragma fragment frag

			sampler2D _MainTex;
			float _BlurSize;
			float _StandardDeviation;

			#define PI 3.14159265359
			#define E 2.71828182846

		#if _SAMPLES_LOW
			#define SAMPLES 10
		#elif _SAMPLES_MEDIUM
			#define SAMPLES 30
		#else
			#define SAMPLES 100
		#endif

			struct appdata{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f{
				float4 position : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v){
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_TARGET{
			#if GAUSS
				if(_StandardDeviation == 0)
				return tex2D(_MainTex, i.uv);
			#endif
				float invAspect = _ScreenParams.y / _ScreenParams.x;
				float4 col = 0;
			#if GAUSS
				float sum = 0;
			#else
				float sum = SAMPLES;
			#endif
				for(float index = 0; index < SAMPLES; index++){
					float offset = (index/(SAMPLES-1) - 0.5) * _BlurSize * invAspect;
					float2 uv = i.uv + float2(offset, 0);
				#if !GAUSS
					col += tex2D(_MainTex, uv);
				#else
					float stDevSquared = _StandardDeviation*_StandardDeviation;
					float gauss = (1 / sqrt(2*PI*stDevSquared)) * pow(E, -((offset*offset)/(2*stDevSquared)));
					sum += gauss;
					col += tex2D(_MainTex, uv) * gauss;
				#endif
				}
				col = col / sum;
				return col;
			}

			ENDCG
		}
	}
}