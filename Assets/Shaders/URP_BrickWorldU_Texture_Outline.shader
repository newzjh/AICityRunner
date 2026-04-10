Shader "AICityRunner/URP/BrickWorldU Texture Outline"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Tint", Color) = (1, 1, 1, 1)
        _Alpha("Alpha", Range(0, 1)) = 0.65

        _WorldUScale("World U Scale", Float) = 0.2
        _WorldUOffset("World U Offset", Float) = 0
        _LaneOffset("Lane Offset (World Z)", Float) = 5

        _Ambient("Ambient", Range(0, 2)) = 0.55
        _Diffuse("Diffuse", Range(0, 2)) = 1.0
        _SpecColor("Spec Color", Color) = (1, 1, 1, 1)
        _Smoothness("Smoothness", Range(0, 1)) = 0.75

        _RimColor("Rim Color", Color) = (1, 1, 1, 1)
        _RimIntensity("Rim Intensity", Range(0, 3)) = 0.9
        _RimPower("Rim Power", Range(0.5, 8)) = 3.0

        _StreakIntensity("Streak Intensity", Range(0, 2)) = 0.55
        _StreakTiling("Streak Tiling", Float) = 7.0
        _StreakWidth("Streak Width", Range(0.001, 0.2)) = 0.035
        _StreakSpeed("Streak Speed", Float) = 0.15

        _OutlineColor("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineAlpha("Outline Alpha", Range(0, 1)) = 0.9
        _OutlineWidth("Outline Width", Range(0, 0.05)) = 0.01
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        Pass
        {
            Name "Forward"
            Tags
            {
                "LightMode" = "UniversalForward"
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Alpha;

                float _WorldUScale;
                float _WorldUOffset;
                float _LaneOffset;

                float _Ambient;
                float _Diffuse;
                float4 _SpecColor;
                float _Smoothness;

                float4 _RimColor;
                float _RimIntensity;
                float _RimPower;

                float _StreakIntensity;
                float _StreakTiling;
                float _StreakWidth;
                float _StreakSpeed;

                float4 _OutlineColor;
                float _OutlineAlpha;
                float _OutlineWidth;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float4 color : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float3 positionOS : TEXCOORD5;
                float3 normalOS : TEXCOORD6;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionOS = input.positionOS.xyz;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.uv = input.uv;
                output.color = input.color;
                output.screenPos = ComputeScreenPos(output.positionCS);
                output.normalOS = input.normalOS;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 N = SafeNormalize(input.normalWS);
                float3 V = SafeNormalize(GetWorldSpaceViewDir(input.positionWS));

                float laneOffsetSafe = max(abs(_LaneOffset), 0.0001);
                float laneIndex = round((-input.positionWS.z) / laneOffsetSafe);

                float u = (input.positionWS.x + fmod(_Time.y,10) * 5) * _WorldUScale + _WorldUOffset + laneIndex * 0.33;
                u = u * _BaseMap_ST.x + _BaseMap_ST.z;
                float v = input.uv.y * _BaseMap_ST.y + _BaseMap_ST.w;
                v = clamp((v-0.5)*0.75+0.55,0,1);

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, float2(u, v));
                float3 baseRgb = tex.rgb * _BaseColor.rgb * input.color.rgb;

                Light mainLight = GetMainLight();
                float3 L = SafeNormalize(mainLight.direction);
                float ndl = saturate(dot(N, L));

                float3 ambient = SampleSH(N) * _Ambient;
                float3 lightCol = max(mainLight.color, 0.15.xxx);
                float3 diffuse = baseRgb * (ndl * _Diffuse) * lightCol;

                float3 H = SafeNormalize(L + V);
                float specExp = lerp(8.0, 128.0, saturate(_Smoothness));
                float spec = pow(saturate(dot(N, H)), specExp);
                float3 specular = spec * _SpecColor.rgb * lightCol;

                float rim = pow(1.0 - saturate(dot(N, V)), _RimPower) * _RimIntensity;
                float3 rimCol = rim * _RimColor.rgb;

                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                float diag = frac((screenUV.x + screenUV.y) * _StreakTiling + _Time.y * _StreakSpeed);
                float streak = smoothstep(1.0 - _StreakWidth, 1.0, diag) * _StreakIntensity;
                float facing = saturate(1.0 - abs(dot(N, V)));
                float3 streakCol = streak * facing;

                float3 rgb = ambient + diffuse + specular + rimCol + streakCol;
                float baseAlpha = saturate(_Alpha * _BaseColor.a * tex.a * input.color.a);

                float sx = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
                float sy = length(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21));
                float sz = length(float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22));
                float3 nOS = SafeNormalize(input.normalOS);
                float3 an = abs(nOS);

                float2 dAxis;
                float2 wAxis;
                if (an.y >= an.x && an.y >= an.z)
                {
                    dAxis = float2(0.5 - abs(input.positionOS.x), 0.5 - abs(input.positionOS.z));
                    wAxis = float2(sx, sz);
                }
                else if (an.x >= an.y && an.x >= an.z)
                {
                    dAxis = float2(0.5 - abs(input.positionOS.y), 0.5 - abs(input.positionOS.z));
                    wAxis = float2(sy, sz);
                }
                else
                {
                    dAxis = float2(0.5 - abs(input.positionOS.x), 0.5 - abs(input.positionOS.y));
                    wAxis = float2(sx, sy);
                }

                float2 localW = _OutlineWidth / max(wAxis, 0.000001.xx);
                float2 nd = dAxis / max(localW, 0.000001.xx);
                float outlineMask = step(min(nd.x, nd.y), 1.0);
                float outlineT = saturate(outlineMask * _OutlineAlpha * _OutlineColor.a);
                rgb = lerp(rgb, _OutlineColor.rgb, outlineT);

                float outlineAlpha = saturate(_Alpha * _BaseColor.a * input.color.a) * outlineT;
                float alpha = max(baseAlpha, outlineAlpha);
                return half4(rgb, alpha);
            }
            ENDHLSL
        }
    }
}
