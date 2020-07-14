#shader vertex
#version 330 core

#include "Camera.ubo"
#include "Time.ubo"

layout(location = 0) in vec4 position;

out vec2 v_TexCoord;

void main()
{
    v_TexCoord = position.xy * 0.5 + 0.5;
    gl_Position = position;
}

#shader fragment
#version 330 core

//#type transparent
#culling none

#include "Voxel.glsl"
#include "Lighting.ubo"

layout(location = 0) out vec4 color;

uniform sampler2D u_Src;
uniform float u_Contrast = 1;

in vec2 v_TexCoord;

vec3 uncharted2_tonemap_partial(vec3 x)
{
    float A = 0.15;
    float B = 0.50;
    float C = 0.10;
    float D = 0.20;
    float E = 0.02;
    float F = 0.30;
    return ((x*(A*x+C*B)+D*E)/(x*(A*x+B)+D*F))-E/F;
}

vec3 uncharted2_filmic(vec3 v)
{
    float exposure_bias = 2.0;
    vec3 curr = uncharted2_tonemap_partial(v * exposure_bias);

    vec3 W = vec3(11.2f);
    vec3 white_scale = vec3(1.0f) / uncharted2_tonemap_partial(W);
    return curr * white_scale;
}

void main()
{
   color = texture(u_Src, v_TexCoord);
   color.rgb = uncharted2_tonemap_partial(color.rgb) * 5.5;

}