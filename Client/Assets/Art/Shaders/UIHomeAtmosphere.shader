Shader "AlicizaX/UI/Home Atmosphere"
{
    Properties
    {
        [PerRendererData] _MainTex ("Atmosphere Plate", 2D) = "white" {}
        _FogColor ("Fog Color", Color) = (0.905, 0.918, 0.888, 1)
        _FogStrength ("Fog Strength", Range(0, 1)) = 0.19
        _FogScale ("Fog Scale", Float) = 1
        _FogSpeed ("Fog Speed", Float) = 1
        _SeaLine ("Sea Line", Range(0, 1)) = 0.31
        _SeaStrength ("Sea Strength", Range(0, 1)) = 0.55
        _SeaGlint ("Sea Glint", Range(0, 1)) = 0.16
        _FlareUv ("Flare Position (UV, origin bottom-left)", Vector) = (0.848, 0.732, 0, 0)
        _FlareColor ("Flare Color", Color) = (1, 0.74, 0.40, 1)
        _FlareStrength ("Flare Strength", Range(0, 2)) = 0.55
        _Breath ("Zoom Breath", Range(0, 0.1)) = 0.014
        _BreathSpeed ("Zoom Speed", Float) = 0.09
        _Grain ("Film Grain", Range(0, 0.2)) = 0.006
        _GrainSpeed ("Grain Speed", Float) = 6
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct Attributes
            {
                float4 position : POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                half4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            half4 _FogColor;
            float _FogStrength;
            float _FogScale;
            float _FogSpeed;
            float _SeaLine;
            float _SeaStrength;
            float _SeaGlint;
            float4 _FlareUv;
            half4 _FlareColor;
            float _FlareStrength;
            float _Breath;
            float _BreathSpeed;
            float _Grain;
            float _GrainSpeed;

            Varyings Vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.position = UnityObjectToClipPos(input.position);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float Hash12(float2 p)
            {
                float3 p3 = frac(float3(p.xyx) * 0.1031);
                p3 += dot(p3, p3.yzx + 33.33);
                return frac((p3.x + p3.y) * p3.z);
            }

            float ValueNoise(float2 p)
            {
                float2 cell = floor(p);
                float2 f = frac(p);
                float2 blend = f * f * (3.0 - 2.0 * f);
                float a = Hash12(cell);
                float b = Hash12(cell + float2(1.0, 0.0));
                float c = Hash12(cell + float2(0.0, 1.0));
                float d = Hash12(cell + float2(1.0, 1.0));
                return lerp(lerp(a, b, blend.x), lerp(c, d, blend.x), blend.y);
            }

            float Fbm(float2 p)
            {
                float sum = 0.0;
                float amp = 0.55;
                for (int i = 0; i < 4; i++)
                {
                    sum += ValueNoise(p) * amp;
                    p = p * 2.03 + 17.7;
                    amp *= 0.55;
                }
                return sum;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                float time = _Time.y;

                // ---- breathing camera ----
                float zoom = 1.0 + _Breath * sin(time * _BreathSpeed * 6.28318);
                float2 drift = float2(sin(time * 0.11), cos(time * 0.07)) * float2(0.0055, 0.0028);
                float2 suv = (uv - 0.5) / zoom + 0.5 + drift;

                // ---- sea swell: gentle horizontal wobble below the horizon ----
                float depth = saturate((_SeaLine - suv.y) / max(0.0001, _SeaLine));
                float swellMask = smoothstep(0.0, 0.16, depth) * _SeaStrength;
                float wobble = sin(suv.y * (52.0 - depth * 26.0) + time * 1.15) * 0.0016
                             + sin(suv.x * 7.0 + time * 0.62) * 0.0011
                             + Fbm(float2(suv.x * 6.0, suv.y * 20.0 - time * 0.30)) * 0.0022;
                suv.x += wobble * swellMask;
                suv.y += (Fbm(float2(suv.x * 3.0 + 11.0, suv.y * 14.0 - time * 0.22)) - 0.5) * 0.0035 * swellMask;
                suv = clamp(suv, 0.0, 1.0);

                half4 plate = tex2D(_MainTex, suv);
                half3 color = plate.rgb;

                // ---- drifting fog banks ----
                float2 fogUv = float2(uv.x * 3.4, uv.y * 9.0) * _FogScale;
                float fogTime = time * 0.045 * _FogSpeed;
                float low = Fbm(fogUv + float2(fogTime * -1.35, fogTime * 0.42));
                float high = Fbm(fogUv * 2.15 + float2(fogTime * 2.05, fogTime * -0.62) + 9.3);
                float bank = low * 0.62 + high * 0.38;

                float fogBand = smoothstep(0.06, 0.28, uv.y) * (1.0 - smoothstep(0.30, 0.62, uv.y));
                float sideFade = smoothstep(0.0, 0.30, uv.x) * 0.55 + 0.45;
                float fogAmount = saturate((bank - 0.36) * 2.15) * fogBand * sideFade * _FogStrength;
                color = lerp(color, _FogColor.rgb, fogAmount);

                // ---- drifting mist right on the water line ----
                float3 hazeBand = float3(uv.x * 4.6 + time * 0.018, uv.y * 26.0 - time * 0.05, 3.0);
                float hazeNoise = Fbm(hazeBand.xy);
                float horizonBand = exp(-pow(abs(uv.y - _SeaLine) * 16.0, 2.0));
                color = lerp(color, _FogColor.rgb, horizonBand * hazeNoise * 0.14);

                // ---- wave glints ----
                float glintNoise = Fbm(float2(uv.x * 9.0 + time * 0.03, uv.y * 34.0 - time * 0.42));
                float glintMask = smoothstep(0.62, 0.88, glintNoise) * smoothstep(0.04, 0.30, depth) * (1.0 - smoothstep(0.55, 0.95, depth));
                color += half3(0.95, 0.97, 0.93) * glintMask * _SeaGlint;

                // ---- flare flicker ----
                float aspectY = _ScreenParams.y / max(1.0, _ScreenParams.x);
                float2 flareDelta = float2(uv.x - _FlareUv.x, (uv.y - _FlareUv.y) * aspectY);
                float flareDist = length(flareDelta);
                float flicker = 0.72 + 0.28 * sin(time * 5.3) * sin(time * 2.11 + 1.3);
                float flare = exp(-pow(flareDist / 0.040, 2.0)) * flicker
                            + exp(-pow(flareDist / 0.009, 2.0)) * (0.8 + 0.2 * flicker);
                color += _FlareColor.rgb * flare * _FlareStrength;

                // ---- film grain ----
                float2 grainCell = uv * _ScreenParams.xy * 0.75 + frac(time * _GrainSpeed) * 91.7;
                float grain = Hash12(grainCell) - 0.5;
                color += grain * _Grain;

                float menuContrast = 1.0 - 0.22 * (1.0 - smoothstep(0.08, 0.53, uv.x));
                color = saturate(color * menuContrast);
                return half4(color * input.color.rgb, plate.a * input.color.a);
            }
            ENDCG
        }
    }

    Fallback "UI/Default"
}


