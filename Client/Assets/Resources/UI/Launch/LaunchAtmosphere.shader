Shader "AlicizaX/UI/Launch Atmosphere"
{
    Properties
    {
        [PerRendererData] _MainTex ("Background", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }
        Cull Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
            };

            sampler2D _MainTex;

            Varyings Vert(appdata_full vertex)
            {
                Varyings output;
                output.position = UnityObjectToClipPos(vertex.vertex);
                output.uv = vertex.texcoord.xy;
                output.color = vertex.color;
                return output;
            }

            float Hash(float2 cell)
            {
                return frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
            }

            float MistNoise(float2 uv)
            {
                float2 cell = floor(uv);
                float2 blend = smoothstep(0.0, 1.0, frac(uv));
                float lower = lerp(Hash(cell), Hash(cell + float2(1.0, 0.0)), blend.x);
                float upper = lerp(Hash(cell + float2(0.0, 1.0)), Hash(cell + 1.0), blend.x);
                return lerp(lower, upper, blend.y);
            }

            float DriftingSparks(float2 uv, float time)
            {
                float2 field = uv * float2(37.0, 21.0) + time * float2(-0.62, -0.30);
                float2 cell = floor(field);
                float seed = Hash(cell);
                float2 center = 0.18 + 0.64 * float2(seed, Hash(cell + 31.0));
                float distanceToCenter = length(frac(field) - center);
                float dotShape = 1.0 - smoothstep(0.008, 0.045 + seed * 0.025, distanceToCenter);
                float shimmer = 0.55 + 0.45 * sin(time * 1.7 + seed * 130.0);
                float mask = smoothstep(0.30, 0.66, uv.x) * smoothstep(0.10, 0.26, uv.y);
                return step(0.88, seed) * dotShape * shimmer * mask;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y;
                float zoom = 1.055 + 0.018 * sin(time * 0.16);
                float2 drift = float2(sin(time * 0.18), cos(time * 0.11)) * float2(0.011, 0.005);
                half4 plate = tex2D(_MainTex, (uv - 0.5) / zoom + 0.5 + drift);

                float2 fogUV = uv * float2(4.4, 10.0) + time * float2(-0.13, 0.039);
                float banks = MistNoise(fogUV) * 0.65 + MistNoise(fogUV * 1.9 + 8.0) * 0.35;
                float fogHeight = uv.y + sin(uv.x * 6.8 + time * 0.31) * 0.055;
                float fogMask = smoothstep(0.10, 0.31, fogHeight) * (1.0 - smoothstep(0.29, 0.53, fogHeight));
                float fog = banks * fogMask * smoothstep(0.24, 0.64, uv.x) * 0.29;
                half3 atmosphere = lerp(plate.rgb, half3(0.086, 0.175, 0.188), fog);
                float amberLight = smoothstep(0.06, 0.38, plate.r - plate.b) * smoothstep(0.51, 0.78, uv.x);
                atmosphere += half3(1.0, 0.41, 0.12) * amberLight * (0.048 + 0.033 * sin(time * 0.69));
                atmosphere += half3(0.75, 0.30, 0.075) * DriftingSparks(uv, time) * 0.36;

                float footerShade = smoothstep(0.0, 0.24, uv.y);
                atmosphere *= lerp(0.57, 1.0, footerShade);
                return half4(atmosphere * input.color.rgb, plate.a * input.color.a);
            }
            ENDCG
        }
    }
}
