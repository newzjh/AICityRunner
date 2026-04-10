Shader "AICityRunner/URP/WorldSpaceUV Rotate3Angles Transparent"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)

        _AngleX("Angle X (Deg)", Float) = 0
        _AngleY("Angle Y (Deg)", Float) = 0
        _AngleZ("Angle Z (Deg)", Float) = 0
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
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _AngleX;
                float _AngleY;
                float _AngleZ;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 RotateEulerXYZ(float3 v, float3 anglesRad)
            {
                float sx, cx, sy, cy, sz, cz;
                sincos(anglesRad.x, sx, cx);
                sincos(anglesRad.y, sy, cy);
                sincos(anglesRad.z, sz, cz);

                float3 vx = float3(v.x, cx * v.y - sx * v.z, sx * v.y + cx * v.z);
                float3 vy = float3(cy * vx.x + sy * vx.z, vx.y, -sy * vx.x + cy * vx.z);
                float3 vz = float3(cz * vy.x - sz * vy.y, sz * vy.x + cz * vy.y, vy.z);
                return vz;
            }

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float3 anglesRad = radians(float3(_AngleX, _AngleY, _AngleZ));
                float3 uv3 = float3(input.uv * 2 - 1, 0.0);
                float3 rotatedUV = RotateEulerXYZ(uv3, anglesRad);
                float2 uv = rotatedUV.xy * 0.5 + 0.5;
                uv = uv * _BaseMap_ST.xy + _BaseMap_ST.zw;

                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                return tex * half4(_BaseColor.rgb, _BaseColor.a);
            }
            ENDHLSL
        }
    }
}
