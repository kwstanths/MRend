Shader "Custom/TestShader1"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            /* Vertex - Geometry - fragment pass */
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma geometry geom
                #pragma fragment frag
                //#pragma geometry geom

                #include "UnityCG.cginc"

                /* Vertex shader input */
                struct appdata
                {
                    float4 vertex : POSITION;
                    float2 uv : TEXCOORD0;
                    float3 normal : NORMAL;
                };

        /* Vertex shader output - Geometry shader input*/
        struct v2g
        {
            float4 clip_position : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 object_position : TEXCOORD1;
            float3 normal : NORMAL;
        };

        /* Geometry shader output - fragment shader input */
        struct g2f
        {
            float4 clip_position: SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 col : COLOR;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;

        v2g vert(appdata v)
        {
            v2g o;
            o.clip_position = UnityObjectToClipPos(v.vertex);
            o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            o.object_position = v.vertex;
            return o;
        }

        [maxvertexcount(12)]
        void geom(triangle v2g input[3], inout TriangleStream<g2f> tristream) {
            g2f o;

            for (int i = 0; i < 3; i++) {
                o.clip_position = UnityObjectToClipPos(input[i].object_position + float3(0.2, 0, 0));
                o.uv = input[i].uv;
                o.col = fixed4(0, 0, 0, 1);
                tristream.Append(o);

                o.clip_position = UnityObjectToClipPos(input[i].object_position);
                o.uv = input[i].uv;
                o.col = fixed4(0, 0, 0, 1);
                tristream.Append(o);


                o.clip_position = UnityObjectToClipPos(input[i].object_position + float3(0, 0.2, 0));
                o.uv = input[i].uv;
                o.col = fixed4(0, 0, 0, 1);
                tristream.Append(o);

                tristream.RestartStrip();
            }
        }

        fixed4 frag(g2f i) : SV_Target
        {
            // sample the texture
            fixed4 col = tex2D(_MainTex, i.uv);
            return col;
        }
        ENDCG
    }

            //CGPROGRAM
            //// Physically based Standard lighting model, and enable shadows on all light types
            //#pragma surface surf Standard fullforwardshadows

            //// Use shader model 3.0 target, to get nicer looking lighting
            //#pragma target 3.0

            //sampler2D _MainTex;

            //struct Input
            //{
            //    float2 uv_MainTex;
            //};

            //half _Glossiness;
            //half _Metallic;
            //fixed4 _Color;

            //// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
            //// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
            //// #pragma instancing_options assumeuniformscaling
            //UNITY_INSTANCING_BUFFER_START(Props)
            //    // put more per-instance properties here
            //UNITY_INSTANCING_BUFFER_END(Props)

            //void surf (Input IN, inout SurfaceOutputStandard o)
            //{
            //    // Albedo comes from a texture tinted by color
            //    fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            //    o.Albedo = c.rgb;
            //    // Metallic and smoothness come from slider variables
            //    o.Metallic = _Metallic;
            //    o.Smoothness = _Glossiness;
            //    o.Alpha = c.a;
            //}
            //ENDCG
        }
            FallBack "Diffuse"
}
