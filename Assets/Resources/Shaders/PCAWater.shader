Shader "Compute/PCAWater"
{
    Properties
    {
        _MainTex ("Source", 2D) = "black" {}
        _LowTex  ("Low", 2D) = "black" {}
        _HighTex ("High", 2D) = "black" {}
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
            sampler2D _LowTex;
            sampler2D _HighTex;
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

                half4 color = lerp(tex2D(_LowTex, IN.texcoord), tex2D(_HighTex, IN.texcoord), tex2D(_MainTex, IN.texcoord).r);
                color *= a;

                return color;
            }
        ENDCG
        }
    }
}