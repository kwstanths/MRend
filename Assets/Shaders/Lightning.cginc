#ifndef __Lightning_cginc__
#define __Lightning_cginc__

/* Phong shading parameters used in forward rendering */
static float _Shininess = 32;
static float _SpecularIntensity = 0.2f;

struct DirectionalLight {
    float3 direction;
    float ambient_factor;
    float3 diffuse_color;
};

struct PointLight {
    float3 position;
    float3 ambient_factor;
    float3 diffuse_color;
};

/* Calculate linear attenuation between the posision of the fragment and the light */
float Attenuation(float3 fragment_position, float3 light_position) {
    float3 vert = light_position - fragment_position;
    float distance_inv = 1.0 / length(vert);
    return lerp(0.0, 1.0, distance_inv);
}

/* Calculate directional light color contribution */
float3 DirectionalLightColor(DirectionalLight light, float3 fragment_normal, float3 view_direction, float3 fragment_color) {
    /* Ambient component */
    float3 light_ambient = light.ambient_factor * (fragment_color * light.diffuse_color);

    /* Diffuse component */
    float3 light_direction_inv = normalize(-light.direction);
    float light_diffuse_strength = dot(fragment_normal, light_direction_inv);
    float3 light_diffuse = max(light_diffuse_strength, 0.0f) * (fragment_color * light.diffuse_color);

    float3 light_specular = float3(0, 0, 0);
    if (light_diffuse_strength > 0) {
        /* Specular component */
        /* Find the reflected vector from the light towards the surface normal */
        float3 light_reflect_vector = reflect(light_direction_inv, fragment_normal);
        float light_specular_strength = pow(max(dot(view_direction, light_reflect_vector), 0.0), _Shininess);
        light_specular = light.diffuse_color * light_specular_strength * _SpecularIntensity;
    }

    return light_ambient + light_diffuse + light_specular;
}

float3 PointLightColor(PointLight light, float3 fragment_position, float3 fragment_normal, float3 view_direction, float3 fragment_color) {
    /* Ambient component */
    float3 light_ambient = light.ambient_factor * (fragment_color * light.diffuse_color);

    /* Calculate diffuse component */
    float3 light_direction_inv = normalize(light.position - fragment_position);
    float light_diffuse_strength = dot(fragment_normal, light_direction_inv);
    float3 light_diffuse = max(light_diffuse_strength, 0.0f) * (fragment_color * light.diffuse_color);

    float3 light_specular = float3(0, 0, 0);
    if (light_diffuse_strength > 0) {
        /* Specular component */
        /* Find the reflected vector from the light towards the surface normal */
        float3 light_reflect_vector = reflect(light_direction_inv, fragment_normal);
        float light_specular_strength = pow(max(dot(view_direction, light_reflect_vector), 0.0), _Shininess);
        light_specular = light.diffuse_color * light_specular_strength * _SpecularIntensity;
    }

    return light_ambient + Attenuation(fragment_position, light.position) * (light_diffuse + light_specular);
}

#endif
