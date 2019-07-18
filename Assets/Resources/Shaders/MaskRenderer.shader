Shader "Compute/MaskRenderer"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _PrevTex ("Previous Frame", 2D) = "white" {}
        _InputPosition ("Input Position", Vector) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags
        { 
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
       
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            name "Default"
        CGPROGRAM
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _PrevTex;

            float4 _InputPosition;

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

            half4 frag(v2f IN) : SV_Target
            {
                half4 color_current_frame = tex2D(_MainTex, IN.texcoord);

                half4 color = max(color_current_frame, tex2D(_PrevTex, IN.texcoord));

                return color;
            }
        ENDCG
        }
    }
}