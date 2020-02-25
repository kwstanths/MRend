Shader "Custom/ImpostorTest"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _ImposterFrames("Frames",  float) = 8
        _ImposterSize("Radius", float) = 1
        _ImposterOffset("Offset", Vector) = (0,0,0,0)
        _ImposterFullSphere("Full Sphere", float) = 0
        _ImposterBorderClamp("Border Clamp", float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Off

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        uniform float _ScaleX;
        uniform float _ScaleY;
        half _ImposterFrames;
        half _ImposterSize;
        half3 _ImposterOffset;
        half _ImposterFullSphere;
        half _ImposterBorderClamp;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


            half3 SpriteProjection(half3 pivotToCameraRayLocal, half frames, half2 size, half2 coord)
        {
            half3 gridVec = pivotToCameraRayLocal;

            //octahedron vector, pivot to camera
            half3 y = normalize(gridVec);

            half3 x = normalize(cross(y, half3(0.0, 1.0, 0.0)));
            half3 z = normalize(cross(x, y));

            half2 uv = ((coord*frames) - 0.5) * 2.0; //-1 to 1 

            half3 newX = x * uv.x;
            half3 newZ = z * uv.y;

            half2 halfSize = size * 0.5;

            newX *= halfSize.x;
            newZ *= halfSize.y;

            half3 res = newX + newZ;

            return res;
        }

        void vert(inout appdata_full data) {
            
            //data.texcoord.xy = 2 * (float2(data.vertex.x, data.vertex.y) - 0.5);

            ////float4 view_position =
            ////    mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)) + float4(data.vertex.x, data.vertex.y, 0.0, 0.0) * float4(_ScaleX, _ScaleY, 1.0, 1.0);

            //float4 view_position =
            //    mul(UNITY_MATRIX_M, data.vertex);

            //float4x4 mat = transpose(UNITY_MATRIX_V);

            //data.vertex = mul(unity_WorldToObject, view_position);




            //incoming vertex, object space
            half4 vertex = data.vertex;

            //camera in object space
            half3 objectSpaceCameraPos = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
            half2 texcoord = data.texcoord;
            float4x4 objectToWorld = unity_ObjectToWorld;
            float4x4 worldToObject = unity_WorldToObject;

            half3 imposterPivotOffset = _ImposterOffset.xyz;

            float3 objectScale = float3(length(float3(objectToWorld[0].x, objectToWorld[1].x, objectToWorld[2].x)),
                length(float3(objectToWorld[0].y, objectToWorld[1].y, objectToWorld[2].y)),
                length(float3(objectToWorld[0].z, objectToWorld[1].z, objectToWorld[2].z)));

            //pivot to camera ray
            float3 pivotToCameraRay = normalize(objectSpaceCameraPos.xyz - imposterPivotOffset.xyz);

            //scale uv to single frame
            texcoord = half2(texcoord.x, texcoord.y)*(1.0 / _ImposterFrames.x);

            //radius * 2 * unity scaling
            half2 size = _ImposterSize.xx * 2.0; // * objectScale.xx; //unity_BillboardSize.xy                 

            half3 projected = SpriteProjection(pivotToCameraRay, _ImposterFrames, size, texcoord.xy);

            //this creates the proper offset for vertices to camera facing billboard
            half3 vertexOffset = projected + imposterPivotOffset;
            //subtract from camera pos 
            vertexOffset = normalize(objectSpaceCameraPos - vertexOffset);
            //then add the original projected world
            vertexOffset += projected;
            //remove position of vertex
            vertexOffset -= vertex.xyz;
            //add pivot
            vertexOffset += imposterPivotOffset;
            data.vertex.xyz += vertexOffset;

            data.texcoord.xy = 2 * (data.texcoord.xy - 0.5);
        }


        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float dotproduct = dot(IN.uv_MainTex, IN.uv_MainTex);
            clip(1 - dotproduct);

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
