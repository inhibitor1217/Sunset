Shader "Compute/AlphaMask"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _AlphaTex ("Alpha", 2D) = "white" {}
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
            sampler2D _AlphaTex;

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
                half4 color;
                color.rgb = tex2D(_MainTex , IN.texcoord).rgb;
                color.a   = tex2D(_AlphaTex, IN.texcoord).r;

                return color;
            }
        ENDCG
        }
    }
}