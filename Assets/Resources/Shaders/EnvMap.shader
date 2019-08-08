Shader "Compute/EnvMap"
{
    Properties
    {
        _MainTex ("Image", 2D) = "black" {}
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

            sampler2D _MainTex;
            sampler2D _MaskTex;

            fixed4 _SkyColor;

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

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 mask = tex2D(_MaskTex, IN.texcoord);
                if (mask.r < 0.01)
                    return half4(0, 0, 0, 0);
                
                float bound          = (mask.g + mask.b * 0.00390625);
                float diff           = bound - IN.texcoord.y;
                float reflected_y    = bound + 2 * diff;

                float mask_reflected = tex2D( _MaskTex, half2(IN.texcoord.x, reflected_y) ).r;
                float use_envmap     = max( mask_reflected, smoothstep(0.9, 1.0, reflected_y) );

                fixed4 color         = lerp( tex2D( _MainTex, half2(IN.texcoord.x, reflected_y) ), _SkyColor, use_envmap );
                color.a = mask.r;

                return color;
            }
        ENDCG
        }
    }
}