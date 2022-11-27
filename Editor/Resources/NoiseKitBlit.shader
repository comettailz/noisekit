Shader "Hidden/NoiseKitBlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
            Tags { "Queue" = "Overlay+1" }
            // No culling or depth
            Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local DEBUG

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            Texture2D _MainTex;
            SamplerState sampler_MainTex;

            float _Red;
            float _Green;
            float _Blue;
            float _Alpha;

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _MainTex.Sample(sampler_MainTex, i.uv);
                float toggle = 1.0 - step(1.0, (_Red + _Green + _Blue) / 3.0 ) * _Alpha;
                col.r = (col.r * _Red) * toggle  + col.a * _Alpha;
                col.g = (col.g * _Green) * toggle + col.a * _Alpha;
                col.b = (col.b * _Blue) * toggle + col.a * _Alpha;
                col.a = 1.0;
                return col;
            }
            ENDHLSL
        }
    }
}
