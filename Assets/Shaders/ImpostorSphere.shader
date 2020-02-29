﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ImpostorSphere"
{
    Properties{
        [Header(Transform)]
        _Radius("_Radius", Float) = 1.0
        _BoxCorrection("_BoxCorrection", Float) = 1.0

        [Header(Forward Rendering Material properties)]
        _Color("Color", Color) = (1,1,1,1)
        _Ambient("Ambient", Float) = 0.2
        _Shininess("Shininess", Range(0, 128)) = 32
        _SpecularIntensity("Specular intensity", Range(0, 1)) = 0.2

        [Header(Deferred Rendering Material properties)]
        _Metallic("Metallic", Range(0, 1)) = 1
        _Gloss("Gloss", Range(0, 1)) = 0.8
    }

    SubShader {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

        Pass {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
    
            #pragma vertex vert  
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lightning.cginc"
            #include "Impostor.cginc"
            #include "AutoLight.cginc"
    
            uniform float _Radius;
            uniform float _BoxCorrection;
            float4 _Color;
            float _Ambient;
            /* Provided by Unity */
            uniform float4 _LightColor0;
    
            struct appdata {
               float4 vertex : POSITION;
               float2 uv : TEXCOORD0;
            };
            struct v2f {
               float4 pos : SV_POSITION;
               float2 uv : TEXCOORD0;
               float4 view_pos : TEXCOORD1;
            };

            v2f vert(appdata input)
            {
               v2f output;
               output.view_pos = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + _BoxCorrection * float4(input.vertex.x, input.vertex.y, 0.0, 0.0) * 2.0f * float4(_Radius, _Radius, 1.0, 1.0);
               output.pos = mul(UNITY_MATRIX_P, output.view_pos);
    
               output.uv = 2 * (input.uv - 0.5);
    
               return output;
            }
    
            float4 frag(v2f input, out float outDepth : SV_Depth) : COLOR
            {
                float3 normal_world, position_world;
                Impostor(mul(UNITY_MATRIX_I_V, input.view_pos), _Radius, position_world, normal_world);

                float4 clip = mul(UNITY_MATRIX_VP, float4(position_world, 1.0f));
                float z_value = clip.z / clip.w;
                outDepth = z_value;

                float3 view_direction = normalize(position_world - _WorldSpaceCameraPos.xyz);

                DirectionalLight light;
                light.direction = -_WorldSpaceLightPos0.xyz;
                light.ambient_factor = _Ambient;
                light.diffuse_color = _LightColor0;

                float3 color = DirectionalLightColor(light, normal_world, view_direction, _Color);
                
                return half4(color, 1);
            }
            ENDCG
        }

        Pass {
            Tags { "LightMode" = "ForwardAdd" }

            Blend One One

            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag

            #include "Lightning.cginc"
            #include "Impostor.cginc"

            uniform float _Radius;
            uniform float _BoxCorrection;
            float4 _Color;
            float _Ambient;
            /* Provided by Unity */
            uniform float4 _LightColor0;

            struct appdata {
               float4 vertex : POSITION;
               float2 uv : TEXCOORD0;
            };
            struct v2f {
               float4 pos : SV_POSITION;
               float2 uv : TEXCOORD0;
               float4 view_pos : TEXCOORD1;
            };

            v2f vert(appdata input)
            {
               v2f output;
               output.view_pos = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + _BoxCorrection * float4(input.vertex.x, input.vertex.y, 0.0, 0.0) * 2.0f * float4(_Radius, _Radius, 1.0, 1.0);
               output.pos = mul(UNITY_MATRIX_P, output.view_pos);

               output.uv = 2 * (input.uv - 0.5);

               return output;
            }

            float4 frag(v2f input, out float outDepth : SV_Depth) : COLOR
            {
                float3 normal_world, position_world;
                Impostor(mul(UNITY_MATRIX_I_V, input.view_pos), _Radius, position_world, normal_world);

                float4 clip = mul(UNITY_MATRIX_VP, float4(position_world, 1.0f));
                float z_value = clip.z / clip.w;
                outDepth = z_value;

                float3 view_direction = normalize(position_world - _WorldSpaceCameraPos.xyz);

                PointLight light;
                light.position = _WorldSpaceLightPos0.xyz;
                light.ambient_factor = _Ambient;
                light.diffuse_color = _LightColor0;

                float3 color = PointLightColor(light, position_world, normal_world, view_direction, _Color);

                return half4(color, 1);
            }
            ENDCG
        }

        Pass {
            Tags { "LightMode" = "Deferred" }
            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Impostor.cginc"
            #include "UnityPBSLighting.cginc"

            uniform float _Radius;
            uniform float _BoxCorrection;
            float4 _Color;
            float _Metallic;
            float _Gloss;
            float _Ambient;

            struct appdata {
               float4 vertex : POSITION;
               float2 uv : TEXCOORD0;
            };
            struct v2f {
               float4 pos : SV_POSITION;
               float2 uv : TEXCOORD0;
               float4 view_pos : TEXCOORD1;
            };
            struct fragment_output
            {
                half4 diffuse : SV_Target0;
                half4 specular : SV_Target1;
                half4 normal_world : SV_Target2;
                half4 emission : SV_Target3;
            };

            v2f vert(appdata input)
            {
               v2f output;
               output.view_pos = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + _BoxCorrection * float4(input.vertex.x, input.vertex.y, 0.0, 0.0) * 2.0f * float4(_Radius, _Radius, 1.0, 1.0);
               output.pos = mul(UNITY_MATRIX_P, output.view_pos);

               output.uv = 2 * (input.uv - 0.5);

               return output;
            }

            fragment_output frag(v2f input, out float outDepth : SV_Depth) : COLOR
            {
                float3 normal_world, position_world;
                Impostor(mul(UNITY_MATRIX_I_V, input.view_pos), _Radius, position_world, normal_world);

                float4 clip = mul(UNITY_MATRIX_VP, float4(position_world, 1.0f));
                float z_value = clip.z / clip.w;
                outDepth = z_value;

                half3 specular;
                half specularMonochrome;
                half3 diffuseColor = DiffuseAndSpecularFromMetallic(_Color, _Metallic, specular, specularMonochrome);

                fragment_output o;
                o.diffuse.xyz = float4(_Color.xyz, 1);
                o.specular = half4(specular, _Gloss);
                o.normal_world.xyz = normal_world * 0.5f + 0.5f;
                o.normal_world.w = 1;
                o.emission = float4(0.2, 0.2, 0.2, 1) * _Color;
                return o;
            }
            ENDCG
        }
        
    }

    Fallback "Diffuse"
}
