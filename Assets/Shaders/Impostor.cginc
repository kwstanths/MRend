#ifndef __Impostor_cginc__
#define __Impostor_cginc__

void ImpostorSphere(float3 fragment_position_worldspace, float sphere_radius, inout float3 position_worldspace, inout float3 normal_worldspace) {

    float3 fragment_pos = fragment_position_worldspace;
    /* World space position of the center of the sphere, object space (0,0,0) */
    float3 sphere_center = mul(UNITY_MATRIX_M, float4(0.0, 0.0, 0.0, 1.0)).xyz;

    /* A is the origin of the ray */
    float3 A = _WorldSpaceCameraPos.xyz;
    /* B is the direction of the ray */
    float3 B = normalize(fragment_pos - _WorldSpaceCameraPos.xyz);
    /* C is the sphere center */
    float3 C = sphere_center;

    /* Solve the ray, sphere intersection problem */
    float a = dot(B, B);
    float b = 2.0f * dot(B, A - C);
    float c = dot(A - C, A - C) - (sphere_radius * sphere_radius);

    /* Calculate delta */
    float delta = (b * b) - (4.0f * a * c);
    /* Clip fragment if delta is negative */
    clip(delta);

    /* If not, calculate world space position of intersection point */
    delta = sqrt(delta);
    float t1 = (0.5f * a) * (-b + delta);
    float t2 = (0.5f * a) * (-b - delta);
    /* Get the closest point */
    float t = min(t1, t2);

    position_worldspace = A + B * t;
    normal_worldspace = normalize(position_worldspace - sphere_center);
}

#endif