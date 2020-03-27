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
    float t1 = (0.5f) * (-b + delta) / a;
    float t2 = (0.5f) * (-b - delta) / a;
    /* Get the closest point */
    float t = min(t1, t2);

    position_worldspace = A + B * t;
    normal_worldspace = normalize(position_worldspace - sphere_center);
}

void ImpostorCylinder(float3 fragment_position_worldspace, float3 cylinder_direction_worldspace, float cylinder_radius, 
    float cylinder_height, float4x4 cylinder_inverse_transform, inout float3 position_worldspace, inout float3 normal_worldspace) 
{
    float3 fragment_pos = fragment_position_worldspace;

    /* Calculate ray origin and direction in world space coordiantes */
    float3 ray_origin_world = _WorldSpaceCameraPos.xyz;
    float3 ray_direction_world = normalize(fragment_pos - _WorldSpaceCameraPos.xyz);

    /* Transform to cylinder space coordinates, cylinder base should be at XZ plane, cylinder direction towards Y axis */
    float3 ray_origin_cyl = mul(cylinder_inverse_transform, float4(ray_origin_world, 1));
    float3 ray_direction_cyl = mul(cylinder_inverse_transform, float4(ray_direction_world, 0));

    /* Perform ray casting in the above cylidner space, and calculate intersection with the infinite cylinder at XZ plane and cylinder_radius */
    float a = ray_direction_cyl.x * ray_direction_cyl.x + ray_direction_cyl.z * ray_direction_cyl.z;
    float b = 2 * (ray_origin_cyl.x * ray_direction_cyl.x + ray_origin_cyl.z * ray_direction_cyl.z);
    float c = ray_origin_cyl.x * ray_origin_cyl.x + ray_origin_cyl.z * ray_origin_cyl.z - cylinder_radius * cylinder_radius;

    /* If delta is negative, no intersection */
    float delta = b * b - 4 * a*c;
    clip(delta);

    /* Solve equation, find the two points */
    delta = sqrt(delta);
    float t1 = (0.5f) * (-b + delta) / a;
    float t2 = (0.5f) * (-b - delta) / a;
    /* Get the closest point */
    float t = min(t1, t2);

    /* Calculate cylinder space point, clip based on the height */
    float3 point_cyl = ray_origin_cyl + ray_direction_cyl * t;
    clip(point_cyl.y);
    clip(cylinder_height - point_cyl.y);

    /* Calculate world space position of intersection point */
    position_worldspace = ray_origin_world + t * ray_direction_world;

    /* 
        Calculate worldspace normal by extending the wordlspace cylinder origin, point_cyl.y towards the cylinder direction 
        to find the corresponding point on the axis of the cylinder, and then do the substraction with the intersection point
    */
    float3 cylinder_origin_worldspace = mul(UNITY_MATRIX_M, float4(0.0, 0.0, 0.0, 1.0)).xyz;
    float3 cyl_normal_center = cylinder_origin_worldspace + point_cyl.y * cylinder_direction_worldspace;

    normal_worldspace = normalize(position_worldspace - cyl_normal_center);
}

void ImpostorCylinder2(float3 fragment_position_worldspace, float3 cylinder_direction_worldspace, float cylinder_radius,
    float cylinder_height, inout float3 position_worldspace, inout float3 normal_worldspace)
{
    float3 fragment_pos = fragment_position_worldspace;

    /* Calculate ray origin and direction in world space coordiantes */
    float3 ray_origin_world = _WorldSpaceCameraPos.xyz;
    float3 ray_direction_world = normalize(fragment_pos - _WorldSpaceCameraPos.xyz);

    float3 pa = mul(UNITY_MATRIX_M, float4(0.0, 0.0, 0.0, 1.0)).xyz;
    float3 va = cylinder_direction_worldspace;
    float3 p = ray_origin_world;
    float3 v = ray_direction_world;

    /* Perform ray casting in the above cylidner space, and calculate intersection with the infinite cylinder at XZ plane and cylinder_radius */
    float3 Dp = p - pa;
    float3 dotva = float3(va.x * dot(v, va), va.y * dot(v, va), va.z * dot(v, va));
    float3 dotpa = float3(va.x * dot(Dp, va), va.y * dot(Dp, va), va.z * dot(Dp, va));

    float A = (v - dotva) * (v - dotva);
    float B = 2 * dot(v - dotva, Dp - dotpa);
    float C = (Dp - dotpa) * (Dp - dotpa) - cylinder_radius * cylinder_radius;

    /* If delta is negative, no intersection */
    float delta = B * B - 4 * A * C;
    clip(delta);

    /* Solve equation, find the two points */
    delta = sqrt(delta);
    float t1 = (0.5f) * (-B + delta) / A;
    float t2 = (0.5f) * (-B - delta) / A;
    /* Get the closest point */
    float t = min(t1, t2);

    t = t / 3;
    position_worldspace = float3(t, t, t);
    float projection = dot(position_worldspace - pa, cylinder_direction_worldspace);
    //clip(projection);
    //clip(cylinder_height - projection);

    float3 axis_point = pa + projection * cylinder_direction_worldspace;
    normal_worldspace = normalize(position_worldspace - axis_point);
}

#endif