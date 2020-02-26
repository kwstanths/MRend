// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/ImpostorSphere"
{
    Properties{
        [Header(Transform)]
        _Radius("_Radius", Float) = 1.0
        _BoxCorrection("_BoxCorrection", Float) = 1.0

        [Header(Material properties)]
        _Color("Color", Color) = (1,1,1,1)
        _Ambient("Ambient", Float) = 0.2
        _Shininess("Shininess", Range(0, 128)) = 32
        _SpecularIntensity("Specular intensity", Range(0, 1)) = 0.2
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
                float3 normal_view, position_view;
                Impostor(input.view_pos, _Radius, position_view, normal_view);
                
                float4 clip = mul(UNITY_MATRIX_P, float4(position_view, 1.0f));
                float z_value = clip.z / clip.w;

                float3 view_direction = normalize(position_view);

                DirectionalLight light;
                light.direction = mul(UNITY_MATRIX_V, -_WorldSpaceLightPos0).xyz;
                light.ambient_factor = _Ambient;
                light.diffuse_color = _LightColor0;

                float3 color = DirectionalLightColor(light, normal_view, view_direction, _Color);
                
                outDepth = z_value;
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
                float3 normal_view, position_view;
                Impostor(input.view_pos, _Radius, position_view, normal_view);

                float4 clip = mul(UNITY_MATRIX_P, float4(position_view, 1.0f));
                float z_value = clip.z / clip.w;

                float3 view_direction = normalize(position_view);

                PointLight light;
                light.position = mul(UNITY_MATRIX_V, _WorldSpaceLightPos0).xyz;
                light.ambient_factor = _Ambient;
                light.diffuse_color = _LightColor0;

                float3 color = PointLightColor(light, position_view, normal_view, view_direction, _Color);

                outDepth = z_value;
                return half4(color, 1);
            }
            ENDCG
        }
        
    }

    Fallback "Diffuse"
}
