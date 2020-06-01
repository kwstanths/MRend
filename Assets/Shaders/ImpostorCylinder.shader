Shader "Unlit/ImpostorCylinder"
{
    Properties
    {
        _Albedo("Albedo", Color) = (0.5, 0, 0, 0.015)
        _RadiusAndShading("_RadiusAndShading", Color) = (0, 0.7, 0, 0)
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" }

        Pass {
            /* Base forward rendering shader, executed when the rendering mode is set to forward, and with the directional light as input */
            Tags { "LightMode" = "ForwardBase" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM

            #pragma vertex vert  
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "Lightning.cginc"
            #include "Impostor.cginc"
            #include "AutoLight.cginc"

            /* Provided by Unity */
            uniform float4 _LightColor0;

            struct appdata {
                /* Instance ID */
                UNITY_VERTEX_INPUT_INSTANCE_ID
                /* Object space position */
                float4 vertex : POSITION;
            };
            struct v2f {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                /* clip space position */
                float4 vertex : SV_POSITION;
                /* world space position */
                float4 world_pos : TEXCOORD0;
                /* Single pass instanced rendering */
                UNITY_VERTEX_OUTPUT_STEREO
            };

            /* Unpack extra instance properties */
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Albedo)
                UNITY_DEFINE_INSTANCED_PROP(float4, _RadiusAndShading)
            UNITY_INSTANCING_BUFFER_END(Props)

            float3 GetWorldSpaceCylinderDirection() {
                /* In object space, the cylinder direction is along the local y axis */
                return normalize(mul(UNITY_MATRIX_M, float4(0, 1, 0, 0)).xyz);
            }

            float GetHeight() {
                return length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
            }

            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_OUTPUT(v2f, output);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.world_pos = mul(UNITY_MATRIX_M, input.vertex);
                return output;
            }

            float4 frag(v2f input, out float outDepth : SV_Depth) : COLOR
            {
                /* Set up instance id */
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float4 radius_and_shading = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusAndShading);
                float radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Albedo).a;
                float ambient_factor = radius_and_shading.g;

                /* Compute real fragment world position and normal */
                float3 normal_world, position_world;
                ImpostorCylinder2(input.world_pos.xyz, GetWorldSpaceCylinderDirection(), radius, GetHeight(), position_world, normal_world);

                /* Calculate depth */
                float4 clip = mul(UNITY_MATRIX_VP, float4(position_world, 1.0f));
                float z_value = clip.z / clip.w;
                outDepth = z_value;

                float3 view_direction = normalize(position_world - _WorldSpaceCameraPos.xyz);

                /* Phong shading */
                DirectionalLight light;
                light.direction = -_WorldSpaceLightPos0.xyz;
                light.ambient_factor = ambient_factor;
                light.diffuse_color = _LightColor0;

                float4 albedo = UNITY_ACCESS_INSTANCED_PROP(Props, _Albedo);

                float3 color = DirectionalLightColor(light, normal_world, view_direction, albedo.xyz);

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
            #pragma multi_compile_instancing

            #include "Lightning.cginc"
            #include "Impostor.cginc"
            #include "UnityCG.cginc"

            #define BOX_CORRECTION 1.5

            /* Provided by Unity */
            uniform float4 _LightColor0;

            struct appdata {
                /* Instance ID */
                UNITY_VERTEX_INPUT_INSTANCE_ID
                    /* Object space position */
                    float4 vertex : POSITION;
            };
            struct v2f {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                    /* clip space position */
                    float4 vertex : SV_POSITION;
                /* world space position */
                float4 world_pos : TEXCOORD0;
                /* Single pass instanced rendering */
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Albedo)
                UNITY_DEFINE_INSTANCED_PROP(float4, _RadiusAndShading)
                UNITY_INSTANCING_BUFFER_END(Props)

            float3 GetWorldSpaceCylinderDirection() {
                /* In object space, the cylinder direction is along the local y axis */
                return normalize(mul(UNITY_MATRIX_M, float4(0, 1, 0, 0)).xyz);
            }

            float GetHeight() {
                return length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
            }

            v2f vert(appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_OUTPUT(v2f, output);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.world_pos = mul(UNITY_MATRIX_M, input.vertex);
                return output;
            }

            float4 frag(v2f input, out float outDepth : SV_Depth) : COLOR
            {
                /* Set up instance id */
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 radius_and_shading = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusAndShading);
                float radius = UNITY_ACCESS_INSTANCED_PROP(Props, _Albedo).a;
                float ambient_factor = radius_and_shading.g;
                
                /* Compute real fragment world position and normal */
                float3 normal_world, position_world;
                ImpostorCylinder2(input.world_pos.xyz, GetWorldSpaceCylinderDirection(), radius, GetHeight(), position_world, normal_world);
                
                /* Calculate depth */
                float4 clip = mul(UNITY_MATRIX_VP, float4(position_world, 1.0f));
                float z_value = clip.z / clip.w;
                outDepth = z_value;

                float3 view_direction = normalize(position_world - _WorldSpaceCameraPos.xyz);

                PointLight light;
                light.position = _WorldSpaceLightPos0.xyz;
                light.ambient_factor = ambient_factor;
                light.diffuse_color = _LightColor0;

                float4 albedo = UNITY_ACCESS_INSTANCED_PROP(Props, _Albedo);

                float3 color = PointLightColor(light, position_world, normal_world, view_direction, albedo.xyz);

                return half4(color, 1);
            }
            ENDCG
        }

        Pass
        {
            Tags { "LightMode" = "Deferred" }
            CGPROGRAM

            #pragma target 3.0
            /* Exclude GPU that don't support Multi Target Rendering */
            #pragma exclude_renderers nomrt

            #pragma vertex vert
            #pragma fragment frag
            /* Add multi compiling support for instancing */
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "Impostor.cginc"
            #include "UnityPBSLighting.cginc"

            struct appdata
            {
                /* Instance ID */
                UNITY_VERTEX_INPUT_INSTANCE_ID
                /* Object space position */
                float4 vertex : POSITION;
            };

            struct v2f
            {
                UNITY_VERTEX_INPUT_INSTANCE_ID
                /* clip space position */
                float4 vertex : SV_POSITION;
                /* world space position */
                float4 world_pos : TEXCOORD0;
                /* Single pass instanced rendering */
                UNITY_VERTEX_OUTPUT_STEREO
            };
            struct fragment_output
            {
                half4 diffuse : SV_Target0;
                half4 specular : SV_Target1;
                half4 normal_world : SV_Target2;
                half4 emission : SV_Target3;
            };

            /* Unpack extra instance properties */
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _Albedo)
                UNITY_DEFINE_INSTANCED_PROP(float4, _RadiusAndShading)
                UNITY_INSTANCING_BUFFER_END(Props)

            float3 GetWorldSpaceCylinderDirection() {
                /* In object space, the cylinder direction is along the local y axis */
                return normalize(mul(UNITY_MATRIX_M, float4(0, 1, 0, 0)).xyz);
            }

            float GetHeight() {
                return length(float3(unity_ObjectToWorld[0].y, unity_ObjectToWorld[1].y, unity_ObjectToWorld[2].y));
            }

            v2f vert (appdata input)
            {
                v2f output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_OUTPUT(v2f, output);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.world_pos = mul(UNITY_MATRIX_M, input.vertex);
                return output;
            }

            fragment_output frag(v2f input, out float outDepth : SV_Depth) : COLOR
            {
                /* Set up instance id */
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                float4 radius_and_shading = UNITY_ACCESS_INSTANCED_PROP(Props, _RadiusAndShading);
                float4 albedo = UNITY_ACCESS_INSTANCED_PROP(Props, _Albedo);
                float radius = albedo.a;
                float is_highlighted = radius_and_shading.r;
                float ambient_factor = radius_and_shading.g;
                float metallic = radius_and_shading.b;
                float gloss = radius_and_shading.a;

                /* Compute real fragment world position and normal */
                float3 normal_world, position_world;
                //ImpostorCylinder(input.world_pos.xyz, GetWorldSpaceCylinderDirection(), _Radius, _Height, _InverseTransform, position_world, normal_world);
                //ImpostorCylinder(input.world_pos.xyz, GetWorldSpaceCylinderDirection(), _Radius, _Height, unity_WorldToObject, position_world, normal_world);
                ImpostorCylinder2(input.world_pos.xyz, GetWorldSpaceCylinderDirection(), radius, GetHeight(), position_world, normal_world);

                /* Calculate depth */
                float4 clip = mul(UNITY_MATRIX_VP, float4(position_world, 1.0f));
                float z_value = clip.z / clip.w;
                outDepth = z_value;

                /* Calculate diffuse and specular component from using Unity's Physically based rendering pipeline */
                half3 specular;
                half specularMonochrome;
                half3 diffuseColor = DiffuseAndSpecularFromMetallic(albedo.xyz, metallic, specular, specularMonochrome);

                /* Set output parameters */
                fragment_output o;
                o.diffuse = float4(diffuseColor, 1);
                o.specular = half4(specular, gloss);
                o.normal_world.xyz = normal_world * 0.5f + 0.5f;
                o.normal_world.w = is_highlighted;
                o.emission.xyz = ambient_factor * diffuseColor;
                return o;
            }



            ENDCG
        }
    }
}
