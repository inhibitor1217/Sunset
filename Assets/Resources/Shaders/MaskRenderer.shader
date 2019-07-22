Shader "Compute/MaskRenderer"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _PrevTex ("Previous Frame", 2D) = "black" {}
        _LabelTex ("Label Texture", 2D) = "black" {}
        _UseLabel ("Use Label", Int) = 0
        _UseEraser ("Use Eraser", Int) = 0
        _InputCoords ("Input Coordinates", Vector) = (0, 0, 0, 0)
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
            sampler2D _PrevTex;
            sampler2D _LabelTex;

            int _UseLabel;
            int _UseEraser;

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
                half4 color;
                half prev, cur;

                cur = _UseLabel ? all(tex2D(_LabelTex, IN.texcoord) == tex2D(_LabelTex, _InputCoords)) : tex2D(_MainTex, IN.texcoord).r;
                cur = _UseEraser ? 1 - cur : cur;

                prev = tex2D(_PrevTex, IN.texcoord).r;

                half value = _UseEraser ? min(cur, prev) : max(cur, prev);
                color = half4(value, value, value, value);

                return color;
            }
        ENDCG
        }
    }
}