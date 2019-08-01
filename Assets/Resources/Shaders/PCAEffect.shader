Shader "Compute/PCAEffect"
{
    Properties
    {
        _MainTex ("Source", 2D) = "black" {}
        _PaletteTex  ("Palette", 2D) = "black" {}
        _MaskTex ("Mask", 2D) = "black" {}

        _Horizon ("Horizon", Range(0, 1.5)) = .5
        _Fov_Y   ("Fov Y", Range(0, 1.57)) = .785
        _Yaw     ("Yaw", Range(-1.57, 1.57)) = 0 

        _FogColor ("Fog Color", Color) = (.76, .80, .89, 1)
    }
    SubShader
    {
        Pass
        {
            name "Default"
        CGPROGRAM
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma vertex vert
            #pragma fragment frag

            static float PI = 3.141593;

            sampler2D _MainTex;
            sampler2D _PaletteTex;
            sampler2D _MaskTex;

            float2 _MainTex_TexelSize;
            float2 _MaskTex_TexelSize;
            
            float _Horizon;
            float _Fov_Y;
            float _Yaw;

            half4 _FogColor;

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

            half4 frag(v2f IN) : SV_Target
            {
                float Alpha = 1.0 / tan(_Yaw + _Fov_Y); 
                float Beta  = cos(_Fov_Y) / (sin(_Yaw + _Fov_Y) + cos(_Yaw));

                float a = tex2D(_MaskTex, IN.texcoord).r;
                float Y = Alpha + Beta * (IN.texcoord.y) / abs(_Horizon - IN.texcoord.y);

                if (a < 0.01)
                    return half4(0, 0, 0, 0);

                half4 color = lerp(
                    tex2D(_PaletteTex, half2(IN.texcoord.x, .5 * IN.texcoord.y)),      // LOW 
                    tex2D(_PaletteTex, half2(IN.texcoord.x, .5 * IN.texcoord.y + .5)), // HIGH 
                    tex2D(_MainTex   , half2(
                        (IN.texcoord.x - .5) * (1 + abs(Y)), 
                        Y
                    )).r // VALUE
                );

                // FOG
                color = lerp(color, _FogColor, smoothstep(_Horizon - .1, _Horizon, IN.texcoord.y));

                // MASK BOUNDARY MIX
                color *= a;

                return color;
            }
        ENDCG
        }
    }
}