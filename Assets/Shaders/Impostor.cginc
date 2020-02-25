#ifndef __Impostor_cginc__
#define __Impostor_cginc__

void Impostor(float3 fragment_position_viewspace, float sphere_radius, inout float3 position_viewspace, inout float3 normal_viewspace) {
    
    float3 fragment_pos = fragment_position_viewspace;
    float3 sphere_center = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0)).xyz;

    float3 A = float3(0, 0, 0);
    float3 B = normalize(fragment_pos);
    float3 C = sphere_center;

    float a = dot(B, B);
    float b = 2.0f * dot(B, A - C);
    float c = dot(A - C, A - C) - (sphere_radius * sphere_radius);

    float delta = (b * b) - (4.0f * a * c);
    clip(delta);

    delta = sqrt(delta);
    float t1 = (0.5f * a) * (-b + delta);
    float t2 = (0.5f * a) * (-b - delta);
    float t = min(t1, t2);

    position_viewspace = B * t;
    normal_viewspace = normalize(position_viewspace - sphere_center);
}

#endif