#ifndef __Lightning_cginc__
#define __Lightning_cginc__

float _Shininess;
float _SpecularIntensity;

struct DirectionalLight {
    float3 direction;
    float ambient_factor;
    float3 diffuse_color;
};

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


#endif
