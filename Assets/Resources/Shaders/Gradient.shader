Shader "Compute/Gradient"
{

    Properties
    {
        _MainTex ("Texture", 2D) = "black" {}
        _Amplitude ("Amplitude", Range(0, 5)) = 1
    }

    Subshader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        Pass
        {
            name "Default"
        CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            
            float4 _MainTex_TexelSize;
            float  _Amplitude;

            static float coeff_dx[9] = { -.25, 0, .25, -.50, 0, .50, -.25, 0, .25 };
            static float coeff_dy[9] = { -.25, -.50, -.25, 0, 0, 0, .25, .50, .25 };

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

                OUT.vertex   = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;

                return OUT;
            }

            float4 frag(v2f IN) : SV_Target
            {
                float2 offset = _MainTex_TexelSize.xy;

                float sum_dx = 0;
                float sum_dy = 0;

                float v = tex2D(_MainTex, IN.texcoord).r;

                for (int x = -1; x <= 1; x++)
                    for (int y = -1; y <= 1; y++)
                    {
                        sum_dx += coeff_dx[x + y * 3 + 4] * tex2D(_MainTex, IN.texcoord + offset * float2(x, y)).r;
                        sum_dy += coeff_dy[x + y * 3 + 4] * tex2D(_MainTex, IN.texcoord + offset * float2(x, y)).r;
                    }

                float3 n = cross(
                    normalize(float3(1, 0, _Amplitude * sum_dx)), 
                    normalize(float3(0, 1, _Amplitude * sum_dy))
                );
                float4 color = float4(.5 * n.x + .5, .5 * n.y + .5, .5 * n.z + .5, v);

                return color;
            }
        ENDCG
        }
    }
}