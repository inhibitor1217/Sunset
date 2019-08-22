Shader "Compute/WaterEffect"
{
    Properties
    {
        _NoiseTex   ("Noise",       2D) = "white" {}
        _ImgTex     ("Image",       2D) = "white" {}
        _ImgBlurTex ("Image Blurred", 2D) = "white" {}
        _PaletteTex ("Palette",     2D) = "white" {}
        _EnvTex     ("Environment", 2D) = "white" {}

        _Horizon ("Horizon", Range(0, 1.5)) = .5
        _Perspective ("Perspective", Float) = 1
        _Rotation ("Rotation", Vector) = (1, 0, 0, 1)
        _VerticalBlurWidth ("Vertical Blur Width", Range(0, 0.3)) = 0.12
        _VerticalBlurStrength ("Vertical Blur Strength", Range(0, 2.5)) = 1
        _DistortionStrength ("Distortion Strength", Range(0, 2.5)) = 1
        _ToneStrength ("Tone Strength", Range(0, 1)) = .5
    }
    SubShader
    {
        Pass
        {
            name "Default"
            Cull Off ZWrite Off ZTest Always
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            #define PI 3.1415926536
            #define DEG2RAD (PI / 180.0)

            sampler2D _NoiseTex;
            sampler2D _ImgTex;
            sampler2D _ImgBlurTex;
            sampler2D _PaletteTex;
            sampler2D _EnvTex;
            
            float _Horizon;
            float _Perspective;
            float4 _Rotation;
            float _VerticalBlurWidth;
            float _VerticalBlurStrength;
            float _DistortionStrength;
            float _ToneStrength;

            static float3 _LightDirection = float3( 0, cos(DEG2RAD * 30), sin(DEG2RAD * 30) );

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
                return tex2Dbias(_NoiseTex, float4(uv.x, uv.y, 0, lod)).a;
            }

            float3 get_normal(float2 uv, float lod)
            {
                return 2 * tex2Dbias(_NoiseTex, float4(uv.x, uv.y, 0, lod)).rgb - float3(1, 1, 1);
            }

            float4 frag(v2f IN) : SV_Target
            {
                float alpha = tex2D(_EnvTex, IN.texcoord).a;

                if (alpha < 0.01)
                    return fixed4(0, 0, 0, 0);

                // COORDINATES
                float2 uv   = tex2uv(IN.texcoord);
                uv = mul( half2x2(_Rotation.x, _Rotation.z, _Rotation.y, _Rotation.w), uv );

                float3 n = get_normal(uv, tex2lod(IN.texcoord));

                // DIFFUSE (PALETTE)
                float3 low      = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y) ).rgb;
                float3 high     = tex2D( _PaletteTex, float2(IN.texcoord.x, .5 * IN.texcoord.y + .5) ).rgb;

                // ENV. MAP
                float y_r     = tex2D( _EnvTex, IN.texcoord ).r + tex2D( _EnvTex, IN.texcoord ).g * 0.00390625;
                float y_d     = .5 * ( y_r - IN.texcoord.y );
                float samples        = floor( lerp( 1, 16, smoothstep(_VerticalBlurWidth, 0, y_d) ) );
                float width          = lerp( 0, _VerticalBlurStrength * y_d, smoothstep(_VerticalBlurWidth, 0, y_d) );
                float3 envMap        = float3( 0, 0, 0 );
                float weight_sum     = 0;
                for (float i = 0; i <= samples; i++)
                {
                    envMap += tex2D( 
                        _ImgTex, 
                        float2(IN.texcoord.x, y_r + width * ( i/samples - .5 )) 
                        + _DistortionStrength * float2( 3 * y_d * n.x, 20 * y_d * y_d * n.y)
                    );
                }
                envMap /= (samples + 1);

                float3 c = lerp(
                    low + (high - low) * dot(n, _LightDirection),
                    lerp( envMap, low + (high - low) * envMap, _ToneStrength ),
                    smoothstep(0, 1, normalizedY(IN.texcoord))
                );
                float4 color = float4( c, 1 );

                // FOG
                float4 fogColor = tex2D( _ImgBlurTex, float2(IN.texcoord.x, _Horizon) );
                color           = lerp( color, fogColor, smoothstep(_Horizon - .1, _Horizon, IN.texcoord.y) );

                // MASK BOUNDARY MIX
                color *= alpha;

                return color;
            }
        ENDCG
        }
    }
}