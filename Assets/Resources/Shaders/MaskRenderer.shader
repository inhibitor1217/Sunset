Shader "Compute/MaskRenderer"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _PrevTex ("Previous Frame", 2D) = "black" {}
        _LabelTex ("Label Texture", 2D) = "black" {}
        _UseLabel ("Use Label", Int) = 0
        _InputCoords ("Input Coordinates", Vector) = (0, 0, 0, 0)
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
            sampler2D _LabelTex;

            int _UseLabel;
            float4 _InputCoords;

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
                half4 color, color_current_frame;
 
                switch (_UseLabel)
                {
                case 0:
                    color_current_frame = tex2D(_MainTex, IN.texcoord);
                    break;
                case 1:
                    float value = all(tex2D(_LabelTex, IN.texcoord) == tex2D(_LabelTex, _InputCoords)) ? 1 : 0;
                    color_current_frame = half4(value, value, value, value);
                    break;
                }

                color = max(color_current_frame, tex2D(_PrevTex, IN.texcoord));
                return color;
            }
        ENDCG
        }
    }
}