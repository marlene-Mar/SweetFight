Shader "Custom/HitEffect"
{
    Properties
    {
        _HitColor  ("Hit Color",   Color) = (1, 0, 0, 1)
        _HitAmount ("Hit Amount",  Range(0,1)) = 0
        _Spread    ("Spread",      Range(0,1)) = 0.4
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f     { float4 pos    : SV_POSITION; float2 uv : TEXCOORD0; };

            fixed4 _HitColor;
            float  _HitAmount;
            float  _Spread;

            v2f vert(appdata v) {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv  = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target {
                // UV de -1 a 1
                float2 uv   = i.uv * 2.0 - 1.0;
                // Elipse (0.75 aplana verticalmente)
                float  dist = length(uv * float2(1.0, 0.75));
                // Máscara: 0 en centro, 1 en bordes
                float  mask = smoothstep(_Spread, 1.0, dist);

                return fixed4(_HitColor.rgb, mask * _HitAmount);
            }
            ENDCG
        }
    }
}