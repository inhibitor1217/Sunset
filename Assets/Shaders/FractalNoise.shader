Shader "Compute/FractalNoise"
{
    Properties
    {
        [Enum(Block, 0, Linear, 1, Spline, 2)] _NoiseType ("Noise Type", int) = 0

        _GlobalOffsetScale ("Global Offset, Scale", Vector) = (0, 0, 8, 8)
        _GlobalRotation ("Global Rotation", Range(-360, 360)) = 0
        
        _Complexity ("Complexity", Int) = 2
        _SubOffsetScale("Sub Offset, Scale", Vector) = (0, 0, 2, 2)
        _SubRotation("Sub Rotation", Range(-360, 360)) = 0
        _SubInfluence("Sub Influence", Range(0, 1)) = .7

        _Contrast ("Contrast", Range(0, 10)) = 1
        _Brightness ("Brightness", Range(-2, 2)) = 0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "PreviewType"="Plane"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _NoiseType;

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

            float hash(int2 coords)
            {
                return HASH_ARRAY[mod(HASH_ARRAY[mod(coords.x)] + mod(coords.y))] / HASH_MAX;
            }

            float hashInterp(float2 coords)
            {
                int2 coordsFloored = floor(coords);
                float2 offset = coords - coordsFloored;

                if (_NoiseType == 0)
                {
                    return hash(coordsFloored);
                }
                else if (_NoiseType == 1)
                {
                    return lerp(
                        lerp(hash(coordsFloored), hash(coordsFloored + int2(1, 0)), offset.x),
                        lerp(hash(coordsFloored + int2(0, 1)), hash(coordsFloored + int2(1, 1)), offset.x),
                        offset.y
                    );
                }
                else if (_NoiseType == 2)
                {
                    return spline(
                        spline(hash(coordsFloored + int2(-1, -1)), hash(coordsFloored + int2(0, -1)), hash(coordsFloored + int2(1, -1)), hash(coordsFloored + int2(2, -1)), offset.x),
                        spline(hash(coordsFloored + int2(-1,  0)), hash(coordsFloored + int2(0,  0)), hash(coordsFloored + int2(1,  0)), hash(coordsFloored + int2(2,  0)), offset.x),
                        spline(hash(coordsFloored + int2(-1,  1)), hash(coordsFloored + int2(0,  1)), hash(coordsFloored + int2(1,  1)), hash(coordsFloored + int2(2,  1)), offset.x),
                        spline(hash(coordsFloored + int2(-1,  2)), hash(coordsFloored + int2(0,  2)), hash(coordsFloored + int2(1,  2)), hash(coordsFloored + int2(2,  2)), offset.x),
                        offset.y
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

            float octave(float2 texcoord, int level)
            {
                return hashInterp(
                    rotate2(
                        _GlobalOffsetScale.xy + _SubOffsetScale.xy * level
                        + _GlobalOffsetScale.zw * pow(_SubOffsetScale.zw, level) * texcoord,
                        radians(_GlobalRotation + _SubRotation * level)
                    )
                );
            }

            v2f vert(appdata_t v)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;

                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float value = octave(IN.texcoord, _Complexity - 1);
                for (int level = _Complexity - 2; level >= 0; level--)
                {
                    value = lerp(
                        octave(IN.texcoord, level),
                        value,
                        _SubInfluence
                    );
                }
                value = clamp(_Contrast * (value - .5) + (.5 + _Brightness), 0, 1);

                half4 color = half4(value, value, value, 1);

                return color;
            }
        ENDCG
        }
    }

}