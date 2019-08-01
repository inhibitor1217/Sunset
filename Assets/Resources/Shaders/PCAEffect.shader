Shader "Compute/PCAEffect"
{
    Properties
    {
        _MainTex ("Source", 2D) = "black" {}
        _PaletteTex  ("Palette", 2D) = "black" {}
        _MaskTex ("Mask", 2D) = "black" {}
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
            
            static float HORIZON = 1.5;
            static float FOV_Y = .25 * PI;
            static float YAW = .4 * PI;

            static float ALPHA = 1.0 / tan(YAW + FOV_Y);
            static float BETA  = cos(FOV_Y) / (sin(YAW + FOV_Y) + cos(YAW));

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
                float a = tex2D(_MaskTex, IN.texcoord).r;
                float perspective = ALPHA + BETA * (IN.texcoord.y) / (HORIZON - IN.texcoord.y);
                half4 color = a > 0.01
                    ? lerp(
                        tex2D(_PaletteTex, half2(IN.texcoord.x, .5 * IN.texcoord.y)),      // LOW 
                        tex2D(_PaletteTex, half2(IN.texcoord.x, .5 * IN.texcoord.y + .5)), // HIGH 
                        tex2D(_MainTex   , perspective * half2(
                            (IN.texcoord.x - .5), 
                            IN.texcoord.y
                        )).r // VALUE
                    )
                    : half4(0, 0, 0, 0);

                return color;
            }
        ENDCG
        }
    }
}