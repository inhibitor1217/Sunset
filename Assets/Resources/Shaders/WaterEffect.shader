Shader "Compute/WaterEffect"
{
    Properties
    {
        _MainTex    ("Source",      2D) = "black" {}
        _ImgTex     ("Image",       2D) = "black" {}
        _PaletteTex ("Palette",     2D) = "black" {}
        _EnvTex     ("Environment", 2D) = "white" {}

        _Horizon ("Horizon", Range(0, 1.5)) = .7
        _Fov_Y   ("Fov Y", Range(0, 1.57)) = .4
        _Yaw     ("Yaw", Range(-1.57, 1.57)) = 0 
    }
    SubShader
    {
        Pass
        {
            name "Calm"
            Cull Off ZWrite Off ZTest Always
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            static float PI = 3.141593;

            sampler2D _MainTex;
            sampler2D _ImgTex;
            sampler2D _PaletteTex;
            sampler2D _EnvTex;
            
            float _Horizon;
            float _Fov_Y;
            float _Yaw;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;

                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return half4(0, 0, 0, 0);

                float Alpha = 1.0 / tan(_Yaw + _Fov_Y); 
                float Beta  = cos(_Fov_Y) / (sin(_Yaw + _Fov_Y) + cos(_Yaw));

                float y_n   = IN.texcoord.y / _Horizon;
                float y_p   = Alpha + Beta / abs(1/y_n - 1);

                float r     = tex2D( _MainTex, float2(y_p * (IN.texcoord.x - .5), y_p) ).r;

                // DIFFUSE (PALETTE)
                fixed4 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) );
                fixed4 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) );
                fixed4 diffuse  = lerp( low, high, r );
                
                // SPECULAR (ENVIRONMENT MAP)
                fixed4 envMap   = tex2D( _EnvTex, IN.texcoord + .1 * (1 - pow(y_n, 3)) * float2(r, r) );
                
                // FRESNEL
                fixed4 color    = lerp( diffuse, envMap, .3 * .7 + pow(y_n, 3) );

                // FOG
                fixed4 fogColor = tex2D( _ImgTex, float2(IN.texcoord.x, _Horizon) );
                color          = lerp( color, fogColor, smoothstep(_Horizon - .1, _Horizon, IN.texcoord.y) );

                // MASK BOUNDARY MIX
                color *= alpha;

                return color;
            }
        ENDCG
        }

        Pass
        {
            name "River"
            Cull Off ZWrite Off ZTest Always
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            static float PI = 3.141593;

            sampler2D _MainTex;
            sampler2D _ImgTex;
            sampler2D _PaletteTex;
            sampler2D _EnvTex;
            
            float _Horizon;
            float _Fov_Y;
            float _Yaw;

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;

                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                // float alpha = tex2D(_EnvTex, IN.texcoord).a;

                // if (alpha < 0.01)
                //     return half4(0, 0, 0, 0);

                float Alpha = 1.0 / tan(_Yaw + _Fov_Y); 
                float Beta  = cos(_Fov_Y) / (sin(_Yaw + _Fov_Y) + cos(_Yaw));

                float y_n   = IN.texcoord.y / _Horizon;
                float y_p   = Alpha + Beta / abs(1/y_n - 1);

                fixed4 color;

                // // DIFFUSE (PALETTE)
                // fixed4 low      = tex2D( _PaletteTex, half2(IN.texcoord.x, .5 * IN.texcoord.y) );
                // fixed4 high     = tex2D( _PaletteTex, half2(IN.texcoord.x, .5 * IN.texcoord.y + .5) );
                
                // // SPECULAR (ENVIRONMENT MAP)
                // fixed4 envMap   = tex2D( _EnvTex, IN.texcoord + .1 * (1 - pow(y_n, 3)) * half2(r, r) );
                
                // // FRESNEL
                // fixed4 color    = lerp( diffuse, envMap, .3 * .7 + pow(y_n, 3) );

                // // FOG
                // fixed4 fogColor = tex2D( _ImgTex, half2(IN.texcoord.x, _Horizon) );
                // color          = lerp( color, fogColor, smoothstep(_Horizon - .1, _Horizon, IN.texcoord.y) );

                // // MASK BOUNDARY MIX
                // color *= alpha;

                return color;
            }
        ENDCG
        }
    }
}