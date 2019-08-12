Shader "Compute/WaterEffect"
{
    Properties
    {
        _MainTex    ("Source",      2D) = "white" {}
        _ImgTex     ("Image",       2D) = "white" {}
        _PaletteTex ("Palette",     2D) = "white" {}
        _EnvTex     ("Environment", 2D) = "white" {}
        _FlowTex    ("Flow",        2D) = "white" {}

        _Horizon ("Horizon", Range(0, 1.5)) = .5
        _Perspective ("Perspective", Float) = 1

        _Speed ("Speed", Float) = 0
        _Rotation ("Rotation", Vector) = (1, 0, 0, 1)

        _LightDirection ("Light Direction", Vector) = (0, 1, 0, 0)

        _Amplitude ("Amplitude", Range(0, 5)) = 1
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

            #define PI 3.1415926536

            sampler2D _MainTex;
            sampler2D _ImgTex;
            sampler2D _PaletteTex;
            sampler2D _EnvTex;

            float  _Horizon;
            float  _Perspective;

            float  _Speed;
            float4 _Rotation;

            float4 _LightDirection;

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

            float normalizedY(float2 tex)
            {
                return tex.y / _Horizon;
            }

            float2 tex2uv(float2 tex)
            {
                float y_p = _Perspective / (1 - normalizedY(tex));
                return float2( y_p * (tex.x - .5), y_p );
            }

            half get_noise_value(float2 uv)
            {
            #if USE_MIPMAP
                float lod = log2(length(uv));
                half4  _r = tex2D(_MainTex, uv);
                half  r   = _r.x * clamp( 1 - lod         , 0, 1 ) // COMPLEXITY LEVEL 0 
                          + _r.y * clamp( 1 - abs(lod - 1), 0, 1 ) // COMPLEXITY LEVEL 1
                          + _r.z * clamp( 1 - abs(lod - 2), 0, 1 ) // COMPLEXITY LEVEL 2 
                          + _r.w * clamp( 1 - abs(lod - 3), 0, 1 ) // COMPLEXITY LEVEL 3
                          + .5    * clamp( lod - 3, 0, 1 );
                return r;
            #else
                return tex2D(_MainTex, uv).w;
            #endif
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return fixed4(0, 0, 0, 0);

                // COORDINATES
                float2 uv   = tex2uv(IN.texcoord);
                uv = mul( half2x2(_Rotation.x, _Rotation.z, _Rotation.y, _Rotation.w), uv ) + half2( 0, _Speed * _Time.y );

                half  r     = get_noise_value(uv);

                // LIGHT DIRECTIONS
                half2 v     = normalize(uv);
                half2 l     = normalize(_LightDirection.xy);

                // DIFFUSE (PALETTE)
                fixed4 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) );
                fixed4 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) );
                fixed4 diffuse  = lerp( low, high, r );
                
                // SPECULAR (ENVIRONMENT MAP)
                fixed4 envMap   = tex2D( _EnvTex, IN.texcoord + .2 * (1 - normalizedY(IN.texcoord)) * float2(r, r) );
                
                // FRESNEL
                half reflectance = lerp(
                    .2, .5,
                    pow(max(dot(v, l), 0), 100)
                );
                fixed4 color    = lerp( diffuse, envMap, reflectance );

                // FOG
                fixed4 fogColor = tex2D( _ImgTex, half2(IN.texcoord.x, _Horizon) );
                color           = lerp( color, fogColor, smoothstep(_Horizon - .1, _Horizon, IN.texcoord.y) );

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

            #define PI 3.1415926536
            #define DEG2RAD (PI / 180.0)

            sampler2D _MainTex;
            sampler2D _ImgTex;
            sampler2D _PaletteTex;
            sampler2D _EnvTex;
            
            float _Horizon;
            float _Perspective;

            float  _Speed;
            float4 _Rotation;

            float4 _LightDirection;

            float4 _MainTex_TexelSize;
            float  _Amplitude;

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

            float normalizedY(float2 tex)
            {
                return tex.y / _Horizon;
            }

            float2 tex2uv(float2 tex)
            {
                float y_p = _Perspective / (1 - normalizedY(tex));
                return float2( y_p * (tex.x - .5), y_p );
            }

            half get_noise_value(float2 uv)
            {
            #if USE_MIPMAP
                float lod = log2(length(uv));
                half4  _r = tex2D(_MainTex, uv);
                half  r   = _r.x * clamp( 1 - lod         , 0, 1 ) // COMPLEXITY LEVEL 0 
                          + _r.y * clamp( 1 - abs(lod - 1), 0, 1 ) // COMPLEXITY LEVEL 1
                          + _r.z * clamp( 1 - abs(lod - 2), 0, 1 ) // COMPLEXITY LEVEL 2 
                          + _r.w * clamp( 1 - abs(lod - 3), 0, 1 ) // COMPLEXITY LEVEL 3
                          + .5    * clamp( lod - 3, 0, 1 );
                return r;
            #else
                return tex2D(_MainTex, uv).w;
            #endif
            }

            static float coeff_dx[9] = { -.25, 0, .25, -.50, 0, .50, -.25, 0, .25 };
            static float coeff_dy[9] = { -.25, -.50, -.25, 0, 0, 0, .25, .50, .25 };
            
            half3 get_normal(float2 uv)
            {
                float2 offset = _MainTex_TexelSize.xy;

                half sum_dx = 0;
                half sum_dy = 0;

                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        sum_dx += coeff_dx[x + y * 3 + 4] * get_noise_value(uv + offset * float2(x, y));
                        sum_dy += coeff_dy[x + y * 3 + 4] * get_noise_value(uv + offset * float2(x, y));
                    }

                return cross(normalize(half3(1, 0, _Amplitude * sum_dx)), normalize(half3(0, 1, _Amplitude * sum_dy)));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return fixed4(0, 0, 0, 0);

                // COORDINATES
                float2 uv   = tex2uv(IN.texcoord);
                uv = mul( half2x2(_Rotation.x, _Rotation.z, _Rotation.y, _Rotation.w), uv ) + half2( 0, _Speed * _Time.y );

                // CALCULATE NORMAL
                half3 n = get_normal(uv);

                // LIGHT DIRECTIONS
                half3 l = _LightDirection.xyz;

                // ROTATE NORMAL
                n.xy = mul( half2x2(_Rotation.x, _Rotation.y, _Rotation.z, _Rotation.w), n.xy );

                // DIFFUSE (PALETTE)
                fixed3 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) ).rgb;
                fixed3 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) ).rgb;

                // SPECULAR (ENVIRONMENT MAP)
                fixed3 envMap   = high * tex2D( _EnvTex, IN.texcoord + .4 * (1 - normalizedY(IN.texcoord)) * n.xy ).rgb;
                
                // FRESNEL
                fixed4 color    = fixed4( low + envMap * max(dot(n, l), 0), 1 );

                // FOG
                fixed4 fogColor = tex2D( _ImgTex, float2(IN.texcoord.x, _Horizon) );
                color           = lerp( color, fogColor, smoothstep(_Horizon - .1, _Horizon, IN.texcoord.y) );

                // MASK BOUNDARY MIX
                color *= alpha;

                return color;
            }
        ENDCG
        }
    }
}