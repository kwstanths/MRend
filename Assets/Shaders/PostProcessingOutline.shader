Shader "Hidden/Custom/Outline"
{
    HLSLINCLUDE
    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    int _Thickness;
    sampler2D _CameraGBufferTexture2;
    float4 _CameraGBufferTexture2_TexelSize;

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        float2 tex_size = _CameraGBufferTexture2_TexelSize.xy;
        float selected_value = tex2D(_CameraGBufferTexture2, i.texcoord).w;
        bool is_border = false;
        if (selected_value == 1)
        {
            [loop]
            for (int x = -_Thickness; x <= +_Thickness; x++)
            {
                [loop]
                for (int y = -_Thickness; y <= +_Thickness; y++)
                {
                    if (x == 0 && y == 0)
                    {
                        continue;
                    }
                    float2 offset = float2(x, y) * tex_size;
                    if (tex2D(_CameraGBufferTexture2, i.texcoord + offset).w == 0)
                    {
                        is_border = true;
                    }
                }
            }
        }
        
        if (is_border) return float4(1, 1, 1, 1);
        return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
    }

    ENDHLSL

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            HLSLPROGRAM

            #pragma vertex VertDefault
            #pragma fragment Frag

            ENDHLSL
        }
    }
}
