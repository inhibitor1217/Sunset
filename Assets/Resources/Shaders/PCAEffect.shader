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

            sampler2D _MainTex;
            sampler2D _PaletteTex;
            sampler2D _MaskTex;

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

                half4 color = lerp(tex2D(_PaletteTex, IN.texcoord * (1, 0.5)), tex2D(_PaletteTex, IN.texcoord * (1, 0.5) + (0, 0.5)), tex2D(_MainTex, IN.texcoord).r);
                color *= a;

                return color;
            }
        ENDCG
        }
    }
}