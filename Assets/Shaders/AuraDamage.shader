Shader "Custom/AuraDamage"
{
    Properties {
        _AuraColor ("Aura Color", Color) = (1, 0.03, 0.44, 1)
        _Size ("Aura Size", Range(1.0, 1.5)) = 1.1
        _HitAmount ("Hit Amount", Range(0,1)) = 0
    }
    SubShader {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha One
        ZWrite Off

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 pos : SV_POSITION; };

            float4 _AuraColor;
            float _Size;
            float _HitAmount;

            // VERTEX: Infla el modelo hacia afuera
            v2f vert (appdata v) {
                v2f o;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
                v.vertex.xyz += v.normal * (_Size - 1.0) * _HitAmount; 
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // FRAGMENT: Color morado con transparencia
            fixed4 frag (v2f i) : SV_Target {
                return fixed4(_AuraColor.rgb, _HitAmount * 0.5);
            }
            ENDCG
        }
    }
}