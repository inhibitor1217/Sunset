Shader "Compute/EnvMap"
{
    Properties
    {
        _ImgTex ("Image", 2D) = "black" {}
        _MaskTex ("Mask", 2D) = "black" {}
        _BoundaryTex ("Boundary", 2D) = "white" {}

        _SkyColor ("Sky Color", Color) = (0.76, 0.80, 0.86, 1)
    }
    SubShader
    {
        Pass
        {
            name "Default"
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _ImgTex;
            sampler2D _MaskTex;
            sampler2D _BoundaryTex;

            half4 _SkyColor;

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
                float alpha = tex2D(_MaskTex, IN.texcoord).r;
                
                if (alpha < 0.01)
                    return half4(0, 0, 0, 0);
                
                float boundary_y = tex2D(_BoundaryTex, IN.texcoord).r;
                float reflected_y = 3 * boundary_y - 2 * IN.texcoord.y;
                float mask_reflected = tex2D(_MaskTex, half2(IN.texcoord.x, reflected_y)).r;
                float use_envmap = max(mask_reflected, smoothstep(0.8, 1.0, reflected_y));

                half4 color = lerp(tex2D(_ImgTex, half2(IN.texcoord.x, reflected_y)), _SkyColor, use_envmap);

                return color;
            }
        ENDCG
        }
    }
}