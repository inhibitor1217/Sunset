Shader "Compute/EnvMap"
{
    Properties
    {
        _ImgTex  ("Image", 2D) = "black" {}
        _MaskTex ("Mask",  2D) = "black" {}

        _SkyColor ("Sky Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Pass
        {
            name "Default"
            Cull Off ZWrite Off ZTest Always
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _ImgTex;
            sampler2D _MaskTex;

            float4 _SkyColor;

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

            float4 frag(v2f IN) : SV_Target
            {
                float4 mask = tex2D(_MaskTex, IN.texcoord);

                float bound          = (mask.g + mask.b * 0.00390625);
                float diff           = bound - IN.texcoord.y;

                float samples        = floor( lerp( 1, 30, smoothstep(0, 0.1, diff) ) );
                float width          = lerp( 0, 0.1, smoothstep(0, 0.1, diff) );

                float bound_blurred  = 0;
                float weight_sum     = 0;
                for (float i = 0; i <= samples; i++)
                {
                    float4 m = tex2D(_MaskTex, float2(IN.texcoord.x + width * ( i/samples - .5 ), IN.texcoord.y));
                    bound_blurred += (m.g + m.b * 0.00390625) * m.r;
                    weight_sum    += m.r;
                }
                bound_blurred /= weight_sum;

                float diff_blurred   = bound_blurred - IN.texcoord.y;
                float y_r            = bound_blurred + diff_blurred;

                float y_r1 = floor( y_r * 256 ) * 0.00390625;
                float y_r2 = ( y_r - y_r1 ) * 256;

                return float4(y_r1, y_r2, 0, mask.r);
            }
        ENDCG
        }
    }
}