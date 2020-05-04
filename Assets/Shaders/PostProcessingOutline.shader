Shader "Hidden/Custom/Outline"
{
    HLSLINCLUDE
    #include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

    /* Currently drawn texture */
    TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
    /* Thickness parameter */
    int _Thickness;
    /* The Gbuffer render target with the highlighted information */
    sampler2D _CameraGBufferTexture2;
    /* The size inforamtion for the above texture */
    float4 _CameraGBufferTexture2_TexelSize;

    float4 GetHighlightColor(float texture_value) {
        if (texture_value > 0.7) return float4(1, 1, 1, 1);
        else if (texture_value > 0.4) return float4(0.8, 0, 0, 1);
        else return float4(0, 0.8, 0, 1);
    }

    float4 Frag(VaryingsDefault i) : SV_Target
    {
        /* Get size information */
        float2 tex_size = _CameraGBufferTexture2_TexelSize.xy;
        /* Get value for current fragment */
        float selected_value = tex2D(_CameraGBufferTexture2, i.texcoord).w;
        bool is_border = false;
        /* If this fragment is highlighted, it could belong to the border */
        if (selected_value > 0) {
            /* Sample a number of fragments around, and if any of them is not highlighted
                then this fragment belongs to the border */
            [loop]
            for (int x = -_Thickness; x <= +_Thickness; x++) {
                [loop]
                for (int y = -_Thickness; y <= +_Thickness; y++) {
                    if (x == 0 && y == 0) {
                        continue;
                    }
                    float2 offset = float2(x, y) * tex_size;
                    if (tex2D(_CameraGBufferTexture2, i.texcoord + offset).w != selected_value) {
                        is_border = true;
                    }
                }
            }
        }
        
        /* If it's border return color of outline */
        if (is_border) return GetHighlightColor(selected_value);

        /* Otherwise, return previous color */
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
