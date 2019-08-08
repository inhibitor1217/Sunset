Shader "Compute/WaterEffect"
{
    Properties
    {
        _MainTex    ("Source",      2D) = "black" {}
        _ImgTex     ("Image",       2D) = "black" {}
        _PaletteTex ("Palette",     2D) = "black" {}
        _EnvTex     ("Environment", 2D) = "white" {}
        _FlowTex    ("Flow",        2D) = "black" {}

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
            name "Perspective"
            Cull Off ZWrite Off ZTest Always
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature USE_MIPMAP

            sampler2D _MainTex;
            sampler2D _FlowTex;

            float  _Horizon;
            float  _Perspective;
            
            float  _Speed;
            float4 _Rotation;

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
                float  y_n   = IN.texcoord.y / _Horizon;
                float  y_p   = _Perspective / (1 - y_n);

                float2 uv    = float2( y_p * (IN.texcoord.x - .5), y_p );
        #if USE_MIPMAP
                float  lod   = .5 * log2(y_p);
        #endif

                uv = mul( float2x2(_Rotation.x, _Rotation.z, _Rotation.y, _Rotation.w), uv ) + float2( 0, _Speed * _Time.y );

        #if USE_MIPMAP
                return tex2Dbias( _MainTex, float4(uv.x, uv.y, 0, lod) );
        #else
                return tex2D( _MainTex, uv );
        #endif
            }
        ENDCG
        }
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

            float _Horizon;
            float _Perspective;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                float alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return fixed4(0, 0, 0, 0);

                float y_n   = IN.texcoord.y / _Horizon;
                float y_p   = _Perspective / (1 - y_n);

                float  r    = tex2D( _MainTex, IN.texcoord ).r;

                float2 v    = normalize(float2( y_p * (IN.texcoord.x - .5), y_p));
                float2 l    = normalize(_LightDirection.xy);

                // DIFFUSE (PALETTE)
                fixed4 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) );
                fixed4 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) );
                fixed4 diffuse  = lerp( low, high, r );
                
                // SPECULAR (ENVIRONMENT MAP)
                fixed4 envMap   = tex2D( _EnvTex, IN.texcoord + .2 * (1 - y_n) * float2(r, r) );
                
                // FRESNEL
                float reflectance = lerp(
                    .2, .5,
                    pow(max(dot(v, l), 0), 100)
                );
                fixed4 color    = lerp( diffuse, envMap, reflectance );

                // FOG
                fixed4 fogColor = tex2D( _ImgTex, float2(IN.texcoord.x, _Horizon) );
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

            fixed4 frag(v2f IN) : SV_Target
            {
                float alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return fixed4(0, 0, 0, 0);

                float y_n   = IN.texcoord.y / _Horizon;
                float y_p   = _Perspective / (1 - y_n);

                float3 n = normalize(2 * tex2D(_MainTex, IN.texcoord).xyz - 1);
                float3 l = _LightDirection.xyz;

                // ROTATE NORMAL
                n.xy = mul( float2x2(_Rotation.x, _Rotation.y, _Rotation.z, _Rotation.w), n.xy );

                // DIFFUSE (PALETTE)
                fixed3 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) ).rgb;
                fixed3 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) ).rgb;

                // SPECULAR (ENVIRONMENT MAP)
                fixed3 envMap   = high * tex2D( _EnvTex, IN.texcoord + .4 * (1 - y_n) * n.xy ).rgb;
                
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