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

            float tex2lod(float2 tex)
            {
                return .5 * log2(tex2uv(tex).y);
            }

            float get_noise_value(float2 uv, float lod)
            {
                return tex2Dbias(_MainTex, float4(uv.x, uv.y, 0, lod)).a;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return fixed4(0, 0, 0, 0);

                // COORDINATES
                float2 uv   = tex2uv(IN.texcoord);
                uv = mul( half2x2(_Rotation.x, _Rotation.z, _Rotation.y, _Rotation.w), uv ) + half2( 0, _Speed * _Time.y );

                float r     = get_noise_value(uv, tex2lod(IN.texcoord));

                // LIGHT DIRECTIONS
                float2 v    = normalize(uv);
                float2 l    = normalize(_LightDirection.xy);

                // DIFFUSE (PALETTE)
                float4 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) );
                float4 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) );
                float4 diffuse  = lerp( low, high, r );
                
                // SPECULAR (ENVIRONMENT MAP)
                float4 envMap   = tex2D( _EnvTex, IN.texcoord + .4 * (1 - normalizedY(IN.texcoord)) * float2(r - .5, r - .5) );
                
                // FRESNEL
                float reflectance = lerp(
                    .2, .5,
                    pow(max(dot(v, l), 0), 100)
                );
                float4 color    = lerp( diffuse, envMap, reflectance );

                // FOG
                float4 fogColor = tex2D( _ImgTex, half2(IN.texcoord.x, _Horizon) );
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

            float tex2lod(float2 tex)
            {
                return .5 * log2(tex2uv(tex).y);
            }

            float get_noise_value(float2 uv, float lod)
            {
                return tex2Dbias(_MainTex, float4(uv.x, uv.y, 0, lod)).a;
            }

            float3 get_normal(float2 uv, float lod)
            {
                return 2 * tex2Dbias(_MainTex, float4(uv.x, uv.y, 0, lod)).rgb - float3(1, 1, 1);
            }

            float4 frag(v2f IN) : SV_Target
            {
                float alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return fixed4(0, 0, 0, 0);

                // COORDINATES
                float2 uv   = tex2uv(IN.texcoord);
                uv = mul( half2x2(_Rotation.x, _Rotation.z, _Rotation.y, _Rotation.w), uv ) + half2( 0, _Speed * _Time.y );

                // CALCULATE NORMAL
                float3 n = get_normal(uv, tex2lod(IN.texcoord));

                // LIGHT DIRECTIONS
                float3 l = _LightDirection.xyz;

                // ROTATE NORMAL
                n.xy = mul( float2x2(_Rotation.x, _Rotation.y, _Rotation.z, _Rotation.w), n.xy );

                // DIFFUSE (PALETTE)
                float3 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) ).rgb;
                float3 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) ).rgb;

                // SPECULAR (ENVIRONMENT MAP)
                float3 envMap   = high * tex2D( _EnvTex, IN.texcoord + .4 * (1 - normalizedY(IN.texcoord)) * n.xy ).rgb;
                
                // FRESNEL
                float4 color    = float4( low + envMap * max(dot(n, l), 0), 1 );

                // FOG
                float4 fogColor = tex2D( _ImgTex, float2(IN.texcoord.x, _Horizon) );
                color           = lerp( color, fogColor, smoothstep(_Horizon - .1, _Horizon, IN.texcoord.y) );

                // MASK BOUNDARY MIX
                color *= alpha;

                return color;
            }
        ENDCG
        }
    }
}