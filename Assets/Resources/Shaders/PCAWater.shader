Shader "Compute/PCAWater"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _LowTex  ("Low", 2D) = "white" {}
        _HighTex ("High", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
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
                clip(a - 0.01);

                half4 color = lerp(tex2D(_LowTex, IN.texcoord), tex2D(_HighTex, IN.texcoord), tex2D(_MainTex, IN.texcoord).r);
                color.a = a;

                return color;
            }
        ENDCG
        }
    }
}