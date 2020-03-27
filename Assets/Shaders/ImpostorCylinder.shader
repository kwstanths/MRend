Shader "Unlit/ImpostorCylinder"
{
    Properties
    {
        _Radius("_Radius", Float) = 0.2
        _Height("_Height", Float) = 1
    }
    SubShader
    {
        Tags { "Queue" = "Geometry" "RenderType" = "Opaque" }

        Pass
        {
            Tags { "LightMode" = "Deferred" }
            CGPROGRAM

            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Impostor.cginc"
            #include "UnityPBSLighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 world_pos : TEXCOORD0;
            };
            struct fragment_output
            {
                half4 diffuse : SV_Target0;
                half4 specular : SV_Target1;
                half4 normal_world : SV_Target2;
                half4 emission : SV_Target3;
            };

            float4x4 _InverseTransform;
            float _Radius;
            float _Height;

            float3 GetWorldSpaceCylinderDirection() {
                /* With no rotation, the cylinder direction is along the local z axis */
                return normalize(mul(UNITY_MATRIX_M, float4(0, 1, 0, 0)).xyz);
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world_pos = mul(UNITY_MATRIX_M, v.vertex);
                return o;
            }

            fragment_output frag(v2f input, out float outDepth : SV_Depth) : COLOR
            {
                float3 normal_world, position_world;
                ImpostorCylinder(input.world_pos.xyz, GetWorldSpaceCylinderDirection(), _Radius, _Height, _InverseTransform, position_world, normal_world);

                /* Calculate depth */
                float4 clip = mul(UNITY_MATRIX_VP, float4(position_world, 1.0f));
                float z_value = clip.z / clip.w;
                outDepth = z_value;

                half3 specular;
                half specularMonochrome;
                half3 diffuseColor = DiffuseAndSpecularFromMetallic(float3(1,0,0), 0, specular, specularMonochrome);

                fragment_output o;
                o.diffuse = float4(diffuseColor, 1);
                o.specular = half4(specular, 0);
                o.normal_world.xyz = normal_world * 0.5f + 0.5f;
                o.normal_world.w = 0;
                o.emission.xyz = 0.7 * diffuseColor;
                return o;
            }



            ENDCG
        }
    }
}
