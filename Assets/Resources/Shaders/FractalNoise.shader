Shader "Compute/FractalNoise"
{
    Properties
    {
        _GlobalOffsetScale ("Global Offset, Scale", Vector) = (0, 0, 8, 8)
        // _GlobalRotation ("Global Rotation", Range(-360, 360)) = 0
        
        _Complexity ("Complexity", Int) = 3
        _SubOffsetScale("Sub Offset, Scale", Vector) = (0, 0, 2, 2)
        // _SubRotation("Sub Rotation", Range(-360, 360)) = 0
        _SubInfluence("Sub Influence", Range(0, 1)) = .5

        _Contrast ("Contrast", Range(0, 10)) = 1
        _Brightness ("Brightness", Range(-2, 2)) = 0

        _EvolutionSpeed ("Evolution Speed", Float) = 0
        // _Velocity ("Velocity (Global, Sub)", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Lighting Off
        Blend One Zero
        ColorMask R

        Pass
        {
            name "Default"
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile VALUE VALUE_LINEAR VALUE_SPLINE PERLIN PERLIN_LINEAR
            #pragma multi_compile BASIC TURBULENT ROCKY

            int _NoiseType;
            int _FractalType;

            float4 _Gradients[256];

            fixed4 _GlobalOffsetScale;
            // float _GlobalRotation;

            int _Complexity;
            fixed4 _SubOffsetScale;
            // float _SubRotation;
            float _SubInfluence;

            float _Contrast;
            float _Brightness;
            
            float _EvolutionSpeed;
            // float4 _Velocity;

            static float HASH_MAX = 255;
            static float HASH_ARRAY_SIZE = 256.0;
            static int HASH_MASK = 0xFF;
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

            #define MOD(x) (x >= 0 ? x & HASH_MASK : ~((~x) & HASH_MASK) & HASH_MASK)
        #if VALUE_SPLINE
            #define SPLINE(y0,y1,y2,y3,t) (y1 + .5 * t * ((y2-y0) + t * ((2*y0 - 5*y1 + 4*y2 - y3) + (-y0 + 3*y1 - 3*y2 + y3) * t)))
        #endif

            int hash3(int3 coords)
            {
                return HASH_ARRAY[MOD(HASH_ARRAY[MOD(HASH_ARRAY[MOD(coords.x)] + coords.y)] + coords.z)];
            }

            int hash3(int x, int y, int z)
            {
                return HASH_ARRAY[MOD(HASH_ARRAY[MOD(HASH_ARRAY[MOD(x)] + y)] + z)];
            }

            float value(int3 coords)
            {
                return hash3(coords) / HASH_MAX;
            }

            float value(int x, int y, int z)
            {
                return hash3(x, y, z) / HASH_MAX;
            }
        #if PERLIN || PERLIN_LINEAR
            float perlin(int3 grid, float3 offset)
            {
                return dot(_Gradients[hash3(grid)], offset);
            }

            float perlin(int x, int y, int z, float3 offset)
            {
                return dot(_Gradients[hash3(x, y, z)], offset);
            }
        #endif
            float interp(float3 coords, int level)
            {
                float3 mod = float3(
                    HASH_ARRAY_SIZE / (_GlobalOffsetScale.z * pow(_SubOffsetScale.z, level)),
                    HASH_ARRAY_SIZE / (_GlobalOffsetScale.w * pow(_SubOffsetScale.w, level)),
                    1
                );
                int3 coordsFloored = int3(
                    floor((HASH_ARRAY_SIZE * coords.x) / mod.x) * mod.x,
                    floor((HASH_ARRAY_SIZE * coords.y) / mod.y) * mod.y,
                    floor(coords.z)
                ); 
                float3 offset = (float3(HASH_ARRAY_SIZE, HASH_ARRAY_SIZE, 1) * coords - float3(coordsFloored)) / mod;

                // // FOR DEBUGGING (CHECKBOARD)
                // return .5 * (offset.x + offset.y);

        #if VALUE
                    return lerp(
                        value(coordsFloored),
                        value(coordsFloored + mod * int3(0, 0, 1)),
                        smoothstep(0, 1, offset.z)
                    );
        #endif
        #if VALUE_LINEAR
                    return lerp(
                        lerp(
                            lerp(value(coordsFloored + mod * int3(0, 0, 0)), value(coordsFloored + mod * int3(1, 0, 0)), offset.x),
                            lerp(value(coordsFloored + mod * int3(0, 1, 0)), value(coordsFloored + mod * int3(1, 1, 0)), offset.x),
                            offset.y
                        ),
                        lerp(
                            lerp(value(coordsFloored + mod * int3(0, 0, 1)), value(coordsFloored + mod * int3(1, 0, 1)), offset.x),
                            lerp(value(coordsFloored + mod * int3(0, 1, 1)), value(coordsFloored + mod * int3(1, 1, 1)), offset.x),
                            offset.y
                        ),
                        smoothstep(0, 1, offset.z)
                    );
        #endif
        #if VALUE_SPLINE
                    return lerp(
                        SPLINE(
                            SPLINE(value(coordsFloored + mod * int3(-1, -1, 0)), value(coordsFloored + mod * int3( 0, -1, 0)), value(coordsFloored + mod * int3( 1, -1, 0)), value(coordsFloored + mod * int3( 2, -1, 0)), offset.x),
                            SPLINE(value(coordsFloored + mod * int3(-1,  0, 0)), value(coordsFloored + mod * int3( 0,  0, 0)), value(coordsFloored + mod * int3( 1,  0, 0)), value(coordsFloored + mod * int3( 2,  0, 0)), offset.x),
                            SPLINE(value(coordsFloored + mod * int3(-1,  1, 0)), value(coordsFloored + mod * int3( 0,  1, 0)), value(coordsFloored + mod * int3( 1,  1, 0)), value(coordsFloored + mod * int3( 2,  1, 0)), offset.x),
                            SPLINE(value(coordsFloored + mod * int3(-1,  2, 0)), value(coordsFloored + mod * int3( 0,  2, 0)), value(coordsFloored + mod * int3( 1,  2, 0)), value(coordsFloored + mod * int3( 2,  2, 0)), offset.x),
                            offset.y
                        ),
                        SPLINE(
                            SPLINE(value(coordsFloored + mod * int3(-1, -1, 1)), value(coordsFloored + mod * int3( 0, -1, 1)), value(coordsFloored + mod * int3( 1, -1, 1)), value(coordsFloored + mod * int3( 2, -1, 1)), offset.x),
                            SPLINE(value(coordsFloored + mod * int3(-1,  0, 1)), value(coordsFloored + mod * int3( 0,  0, 1)), value(coordsFloored + mod * int3( 1,  0, 1)), value(coordsFloored + mod * int3( 2,  0, 1)), offset.x),
                            SPLINE(value(coordsFloored + mod * int3(-1,  1, 1)), value(coordsFloored + mod * int3( 0,  1, 1)), value(coordsFloored + mod * int3( 1,  1, 1)), value(coordsFloored + mod * int3( 2,  1, 1)), offset.x),
                            SPLINE(value(coordsFloored + mod * int3(-1,  2, 1)), value(coordsFloored + mod * int3( 0,  2, 1)), value(coordsFloored + mod * int3( 1,  2, 1)), value(coordsFloored + mod * int3( 2,  2, 1)), offset.x),
                            offset.y
                        ),
                        smoothstep(0, 1, offset.z)
                    );
        #endif
        #if PERLIN
                    return lerp(
                        perlin(coordsFloored, offset),
                        perlin(coordsFloored + mod * int3(0, 0, 1), float3(offset.x, offset.y, 1 - offset.z)),
                        smoothstep(0, 1, offset.z)
                    );
        #endif
        #if PERLIN_LINEAR
                    return lerp(
                        lerp(
                            lerp(
                                perlin(coordsFloored + mod * int3(0, 0, 0), offset), 
                                perlin(coordsFloored + mod * int3(1, 0, 0), offset - float3(1, 0, 0)), 
                                smoothstep(0, 1, offset.x)
                            ),
                            lerp(
                                perlin(coordsFloored + mod * int3(0, 1, 0), offset - float3(0, 1, 0)), 
                                perlin(coordsFloored + mod * int3(1, 1, 0), offset - float3(1, 1, 0)), 
                                smoothstep(0, 1, offset.x)
                            ),
                            smoothstep(0, 1, offset.y)
                        ),
                        lerp(
                            lerp(
                                perlin(coordsFloored + mod * int3(0, 0, 1), offset - float3(0, 0, 1)), 
                                perlin(coordsFloored + mod * int3(1, 0, 1), offset - float3(1, 0, 1)), 
                                smoothstep(0, 1, offset.x)
                            ),
                            lerp(
                                perlin(coordsFloored + mod * int3(0, 1, 1), offset - float3(0, 1, 1)), 
                                perlin(coordsFloored + mod * int3(1, 1, 1), offset - float3(1, 1, 1)), 
                                smoothstep(0, 1, offset.x)
                            ),
                            smoothstep(0, 1, offset.y)
                        ),
                        smoothstep(0, 1, offset.z)
                    );
        #endif
            }

            float octave(float3 texcoord, int level)
            {
                // float angle = radians(_GlobalRotation + _SubRotation * level);
                float3 coords;
                // coords.xy = mul(
                //     float2x2(
                //         cos(angle), -sin(angle), 
                //         sin(angle),  cos(angle)
                //     ),
                //     texcoord.xy
                // ) + _GlobalOffsetScale.xy + _SubOffsetScale.xy * level + _Time.y * (_Velocity.xy + _Velocity.zw * level);
                coords.xy = texcoord.xy;
                    // + _GlobalOffsetScale.xy + _SubOffsetScale.xy * level;
                    // + _Time.y * (_Velocity.xy + _Velocity.zw * level);
                coords.z = texcoord.z;

                float value = interp(coords, level);

        #if PERLIN || PERLIN_LINEAR
                value = value * .5 + .5;
        #endif
        
        #if ROCKY
                value = floor(8 * value) * .125;
        #endif

                return value;
            }

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
            };

            v2f vert(appdata_t v)
            {
                v2f OUT;

                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;

                return OUT;
            }

            float frag(v2f IN) : SV_Target
            {
                float3 coords;
                coords.xy = IN.texcoord;
                coords.z = _Time.y * _EvolutionSpeed;

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

        #if TURBULENT
                    value = 1 - 2 * abs(value - 0.5);
        #endif

                return value;
            }
        ENDCG
        }
    }

}