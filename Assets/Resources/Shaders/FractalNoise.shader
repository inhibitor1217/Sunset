Shader "Compute/FractalNoise"
{
    Properties
    {
        _GlobalScale ("Global Scale", Vector) = (.125, .125, 8, 8)
        // _GlobalRotation ("Global Rotation", Range(-360, 360)) = 0
        
        // _Complexity ("Complexity", Int) = 3
        _SubScale("Sub Scale", Vector) = (.5, .5, 2, 2)
        // _SubRotation("Sub Rotation", Range(-360, 360)) = 0
        _SubInfluence("Sub Influence", Range(0, 1)) = .5

        _Contrast ("Contrast", Range(0, 10)) = 1
        _Brightness ("Brightness", Range(-2, 2)) = 0

        _EvolutionSpeed ("Evolution Speed", Float) = 0
        // _Velocity ("Velocity (Global, Sub)", Vector) = (0, 0, 0, 0)

        _Hash     ("Hash Array",     2D) = "black" {}
        _Gradient ("Gradient Array", 2D) = "black" {}
    }

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always
        Lighting Off
        Blend One Zero

        Pass
        {
            name "Default"
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile VALUE VALUE_LINEAR VALUE_SPLINE PERLIN PERLIN_LINEAR
            #pragma multi_compile BASIC TURBULENT ROCKY

            sampler2D _Hash;
            sampler2D _Gradient;

            float4 _GlobalScale;
            // float _GlobalRotation;

            float4 _SubScale;
            // float _SubRotation;
            float _SubInfluence;

            float _Contrast;
            float _Brightness;
            
            float _EvolutionSpeed;
            // float4 _Velocity;

            #define SIZE     256.0
            #define SIZE_INV 0.00390625

        #if VALUE_SPLINE
            #define SPLINE(y0,y1,y2,y3,t) (y1 + .5 * t * ((y2-y0) + t * ((2*y0 - 5*y1 + 4*y2 - y3) + (-y0 + 3*y1 - 3*y2 + y3) * t)))
        #endif

            float hash3(float x, float y, float z)
            {
                return tex2D( _Hash, float2(tex2D( _Hash, float2(tex2D( _Hash, float2(x, 0) ).r + y, 0) ).r + z, 0) ).r;
            }

        #if PERLIN || PERLIN_LINEAR
            float perlin(float x, float y, float z, float3 offset)
            {
                return dot( 2 * tex2D( _Gradient, float2(hash3(x, y, z), 0) ).rgb - float3(1, 1, 1), offset );
            }
        #endif
            float interp(float3 coords, float level)
            {
                float4 size = _GlobalScale * pow(_SubScale, level);
                float3 coordScaled  = float3(coords.x * size.z, coords.y * size.w, coords.z);
                float3 coordFloored = floor(coordScaled);
                float3 coordOffset  = coordScaled - coordFloored;

                float left   = size.x   * (coordFloored.x);
                float right  = size.x   * (coordFloored.x + 1);
                float bottom = size.y   * (coordFloored.y);
                float top    = size.y   * (coordFloored.y + 1);
                float rear   = SIZE_INV * (coordFloored.z);
                float head   = SIZE_INV * (coordFloored.z + 1);

        #if VALUE
                    return lerp(
                        hash3( left, bottom, rear ),
                        hash3( left, bottom, head ),
                        smoothstep(0, 1, coordOffset.z)
                    );
        #endif
        #if VALUE_LINEAR
                    return lerp(
                        lerp(
                            lerp(
                                hash3( left , bottom, rear ), 
                                hash3( right, bottom, rear ), 
                                coordOffset.x
                            ),
                            lerp(
                                hash3( left , top, rear ), 
                                hash3( right, top, rear ), 
                                coordOffset.x
                            ),
                            coordOffset.y
                        ),
                        lerp(
                            lerp(
                                hash3( left , bottom, head ), 
                                hash3( right, bottom, head ), 
                                coordOffset.x
                            ),
                            lerp(
                                hash3( left , top, head ), 
                                hash3( right, top, head ), 
                                coordOffset.x
                            ),
                            coordOffset.y
                        ),
                        smoothstep(0, 1, coordOffset.z)
                    );
        #endif
        #if VALUE_SPLINE
                    float x0 = size.x * (coordFloored.x - 1);
                    float x1 = size.x * (coordFloored.x    );
                    float x2 = size.x * (coordFloored.x + 1);
                    float x3 = size.x * (coordFloored.x + 2);
                    float y0 = size.y * (coordFloored.y - 1);
                    float y1 = size.y * (coordFloored.y    );
                    float y2 = size.y * (coordFloored.y + 1);
                    float y3 = size.y * (coordFloored.y + 2);
                    return lerp(
                        SPLINE(
                            SPLINE(hash3( x0, y0, rear ), hash3( x1, y0, rear ), hash3( x2, y0, rear ), hash3( x3, y0, rear ), coordOffset.x),
                            SPLINE(hash3( x0, y1, rear ), hash3( x1, y1, rear ), hash3( x2, y1, rear ), hash3( x3, y1, rear ), coordOffset.x),
                            SPLINE(hash3( x0, y2, rear ), hash3( x1, y2, rear ), hash3( x2, y2, rear ), hash3( x3, y2, rear ), coordOffset.x),
                            SPLINE(hash3( x0, y3, rear ), hash3( x1, y3, rear ), hash3( x2, y3, rear ), hash3( x3, y3, rear ), coordOffset.x),
                            coordOffset.y
                        ),
                        SPLINE(
                            SPLINE(hash3( x0, y0, head ), hash3( x1, y0, head ), hash3( x2, y0, head ), hash3( x3, y0, head ), coordOffset.x),
                            SPLINE(hash3( x0, y1, head ), hash3( x1, y1, head ), hash3( x2, y1, head ), hash3( x3, y1, head ), coordOffset.x),
                            SPLINE(hash3( x0, y2, head ), hash3( x1, y2, head ), hash3( x2, y2, head ), hash3( x3, y2, head ), coordOffset.x),
                            SPLINE(hash3( x0, y3, head ), hash3( x1, y3, head ), hash3( x2, y3, head ), hash3( x3, y3, head ), coordOffset.x),
                            coordOffset.y
                        ),
                        smoothstep(0, 1, coordOffset.z)
                    );
        #endif
        #if PERLIN
                    return lerp(
                        perlin( left, bottom, rear, coordOffset ),
                        perlin( left, bottom, head, coordOffset - float3(0, 0, 1) ),
                        smoothstep(0, 1, coordOffset.z)
                    );
        #endif
        #if PERLIN_LINEAR
                    return lerp(
                        lerp(
                            lerp(
                                perlin( left , bottom, rear, coordOffset ), 
                                perlin( right, bottom, rear, coordOffset - float3(1, 0, 0) ), 
                                smoothstep(0, 1, coordOffset.x)
                            ),
                            lerp(
                                perlin( left , top, rear, coordOffset - float3(0, 1, 0) ), 
                                perlin( right, top, rear, coordOffset - float3(1, 1, 0) ), 
                                smoothstep(0, 1, coordOffset.x)
                            ),
                            smoothstep(0, 1, coordOffset.y)
                        ),
                        lerp(
                            lerp(
                                perlin( left , bottom, head, coordOffset - float3(0, 0, 1) ), 
                                perlin( right, bottom, head, coordOffset - float3(1, 0, 1) ), 
                                smoothstep(0, 1, coordOffset.x)
                            ),
                            lerp(
                                perlin( left , top, head, coordOffset - float3(0, 1, 1) ), 
                                perlin( right, top, head, coordOffset - float3(1, 1, 1) ), 
                                smoothstep(0, 1, coordOffset.x)
                            ),
                            smoothstep(0, 1, coordOffset.y)
                        ),
                        smoothstep(0, 1, coordOffset.z)
                    );
        #endif
            }

            float octave(float3 texcoord, float level)
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

                float v0 = octave(coords, 0);
                float v1 = octave(coords, 1);
                float v2 = octave(coords, 2);
                float v3 = octave(coords, 3);

                float value = lerp( v0, lerp( v1, lerp( v2, v3, _SubInfluence ), _SubInfluence ), _SubInfluence );

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