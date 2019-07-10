Shader "Compute/FractalNoise"
{
    Properties
    {
        _NoiseType ("Noise Type", int) = 2
        _FractalType ("Fractal Type", int) = 0

        _GlobalOffsetScale ("Global Offset, Scale", Vector) = (0, 0, 8, 8)
        _GlobalRotation ("Global Rotation", Range(-360, 360)) = 0
        
        _Complexity ("Complexity", Int) = 6
        _SubOffsetScale("Sub Offset, Scale", Vector) = (0, 0, 2, 2)
        _SubRotation("Sub Rotation", Range(-360, 360)) = 0
        _SubInfluence("Sub Influence", Range(0, 1)) = .5

        _Contrast ("Contrast", Range(0, 10)) = 1
        _Brightness ("Brightness", Range(-2, 2)) = 0
    }

    SubShader
    {
        Lighting Off
        Blend One Zero

        Pass
        {
            name "Default"
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            int _NoiseType;
            int _FractalType;

            float4 _Gradients[256];

            fixed4 _GlobalOffsetScale;
            float _GlobalRotation;

            int _Complexity;
            fixed4 _SubOffsetScale;
            float _SubRotation;
            float _SubInfluence;

            float _Contrast;
            float _Brightness;

            static float HASH_MAX = 255;
            static int HASH_MASK = 0x7F;
            static int HASH_ARRAY[256] = {
                151,160,137, 91, 90, 15,131, 13,201, 95, 96, 53,194,233,  7,225,
                140, 36,103, 30, 69,142,  8, 99, 37,240, 21, 10, 23,190,  6,148,
                247,120,234, 75,  0, 26,197, 62, 94,252,219,203,117, 35, 11, 32,
                57,177, 33, 88,237,149, 56, 87,174, 20,125,136,171,168, 68,175,
                74,165, 71,134,139, 48, 27,166, 77,146,158,231, 83,111,229,122,
                60,211,133,230,220,105, 92, 41, 55, 46,245, 40,244,102,143, 54,
                65, 25, 63,161,  1,216, 80, 73,209, 76,132,187,208, 89, 18,169,
                200,196,135,130,116,188,159, 86,164,100,109,198,173,186,  3, 64,
                52,217,226,250,124,123,  5,202, 38,147,118,126,255, 82, 85,212,
                207,206, 59,227, 47, 16, 58, 17,182,189, 28, 42,223,183,170,213,
                119,248,152,  2, 44,154,163, 70,221,153,101,155,167, 43,172,  9,
                129, 22, 39,253, 19, 98,108,110, 79,113,224,232,178,185,112,104,
                218,246, 97,228,251, 34,242,193,238,210,144, 12,191,179,162,241,
                81, 51,145,235,249, 14,239,107, 49,192,214, 31,181,199,106,157,
                184, 84,204,176,115,121, 50, 45,127,  4,150,254,138,236,205, 93,
                222,114, 67, 29, 24, 72,243,141,128,195, 78, 66,215, 61,156,180
            };

            int mod(int idx) {
                return idx >= 0 ? idx & HASH_MASK : HASH_MASK - (~idx) & HASH_MASK;
            }

            float spline(float y0, float y1, float y2, float y3, float t)
            {
                float a =          2*y1;
                float b = -   y0        +   y2;
                float c =   2*y0 - 5*y1 + 4*y2 -   y3;
                float d = -   y0 + 3*y1 - 3*y2 +   y3;

                return .5 * (a + t * (b + t * (c + d * t)));
            }

            int hash3(int3 coords)
            {
                return HASH_ARRAY[mod(HASH_ARRAY[mod(HASH_ARRAY[mod(coords.x)] + mod(coords.y))] + mod(coords.z))];
            }

            float value(int3 coords)
            {
                return HASH_ARRAY[hash3(coords)] / HASH_MAX;
            }

            float perlin(int3 grid, float3 coords)
            {
                return dot(_Gradients[hash3(grid)], coords - grid) * .5 + .5;
            }

            float interp(float3 coords)
            {
                int3 coordsFloored = floor(coords);
                float3 offset = coords - coordsFloored;

                if (_NoiseType == 0)
                {
                    return value(coordsFloored);
                }
                else if (_NoiseType == 1)
                {
                    return lerp(
                        lerp(value(coordsFloored), value(coordsFloored + int3(1, 0, 0)), offset.x),
                        lerp(value(coordsFloored + int3(0, 1, 0)), value(coordsFloored + int3(1, 1, 0)), offset.x),
                        offset.y
                    );
                }
                else if (_NoiseType == 2)
                {
                    return spline(
                        spline(value(coordsFloored + int3(-1, -1, 0)), value(coordsFloored + int3(0, -1, 0)), value(coordsFloored + int3(1, -1, 0)), value(coordsFloored + int3(2, -1, 0)), offset.x),
                        spline(value(coordsFloored + int3(-1,  0, 0)), value(coordsFloored + int3(0,  0, 0)), value(coordsFloored + int3(1,  0, 0)), value(coordsFloored + int3(2,  0, 0)), offset.x),
                        spline(value(coordsFloored + int3(-1,  1, 0)), value(coordsFloored + int3(0,  1, 0)), value(coordsFloored + int3(1,  1, 0)), value(coordsFloored + int3(2,  1, 0)), offset.x),
                        spline(value(coordsFloored + int3(-1,  2, 0)), value(coordsFloored + int3(0,  2, 0)), value(coordsFloored + int3(1,  2, 0)), value(coordsFloored + int3(2,  2, 0)), offset.x),
                        offset.y
                    );
                }
                else if (_NoiseType == 3)
                {
                    return perlin(coordsFloored, coords);
                }
                else if (_NoiseType == 4)
                {
                    return lerp(
                        lerp(
                            lerp(perlin(coordsFloored + int3(0, 0, 0), coords), perlin(coordsFloored + int3(1, 0, 0), coords), smoothstep(0, 1, offset.x)),
                            lerp(perlin(coordsFloored + int3(0, 1, 0), coords), perlin(coordsFloored + int3(1, 1, 0), coords), smoothstep(0, 1, offset.x)),
                            smoothstep(0, 1, offset.y)
                        ),
                        lerp(
                            lerp(perlin(coordsFloored + int3(0, 0, 1), coords), perlin(coordsFloored + int3(1, 0, 1), coords), smoothstep(0, 1, offset.x)),
                            lerp(perlin(coordsFloored + int3(0, 1, 1), coords), perlin(coordsFloored + int3(1, 1, 1), coords), smoothstep(0, 1, offset.x)),
                            smoothstep(0, 1, offset.y)
                        ),
                        smoothstep(0, 1, offset.z)
                    );
                }

                return 0;
            }

            float2 rotate2(float2 vec, float angle)
            {
                return float2(
                    cos(angle) * vec.x - sin(angle) * vec.y,
                    sin(angle) * vec.x + cos(angle) * vec.y
                );
            }

            float octave(float3 texcoord, int level)
            {
                float3 coords;
                coords.xy = rotate2(
                    _GlobalOffsetScale.xy + _SubOffsetScale.xy * level
                    + _GlobalOffsetScale.zw * pow(_SubOffsetScale.zw, level) * texcoord.xy,
                    radians(_GlobalRotation + _SubRotation * level)
                );
                coords.z = texcoord.z;

                float value = interp(coords);

                switch (_FractalType)
                {
                    case 0:
                    case 1:
                        break;
                    case 2:
                        value = floor(8 * value) * .125;
                        break;
                }

                return value;
            }

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;

                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float3 coords;
                coords.xy = IN.texcoord;
                coords.z = 0;

                float value = octave(coords, _Complexity - 1);
                for (int level = _Complexity - 2; level >= 0; level--)
                {
                    value = lerp(
                        octave(coords, level),
                        value,
                        _SubInfluence
                    );
                }
                value = clamp(_Contrast * (value - .5) + (.5 + _Brightness), 0, 1);

                switch (_FractalType)
                {
                case 0:
                    break;
                case 1:
                    value = 2 * abs(value - 0.5);
                    break;
                case 2:
                    break;
                }

                half4 color = half4(value, value, value, 1);

                return color;
            }
        ENDCG
        }
    }

}