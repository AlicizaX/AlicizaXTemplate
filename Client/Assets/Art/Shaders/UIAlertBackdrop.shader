Shader "AlicizaX/UI/Alert Backdrop"
{
    Properties
    {
        [PerRendererData] _MainTex ("Captured Frame", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                half alpha : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_full v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord.xy;
                #if UNITY_UV_STARTS_AT_TOP
                o.uv.y = 1 - o.uv.y;
                #endif
                o.alpha = v.color.a;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float mip = max(0, log2(_MainTex_TexelSize.w / 110.0));
                half3 scene = tex2Dlod(_MainTex, float4(i.uv, 0, mip)).rgb;
                half grey = dot(scene, half3(.2126, .7152, .0722));
                float2 edge = i.uv * 2 - 1;
                half vignette = 1 - .28 * saturate(dot(edge, edge));
                half3 color = lerp(grey.xxx, scene, .64) * .48 * vignette;
                return half4(color + half3(.008, .014, .018), i.alpha);
            }
            ENDCG
        }
    }
}
