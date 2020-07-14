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

uniform float weight[5] = float[](0.227027, 0.1945946, 0.1216216, 0.054054, 0.016216);

uniform sampler2D u_Src;

in vec2 v_TexCoord;

void main()
{
    float radius = 1;
    vec2 tex_offset = 1.0 / textureSize(u_Src, 0); // gets size of single texel
    vec3 colBlurred = texture(u_Src, v_TexCoord).rgb * weight[0]; // current fragment's contribution
    for (int i = 0; i < 5; i++)
    {
        colBlurred += texture(u_Src, v_TexCoord + vec2(tex_offset.x * i * radius, 0.0)).rgb * weight[i];
        colBlurred += texture(u_Src, v_TexCoord - vec2(tex_offset.x * i * radius, 0.0)).rgb * weight[i];
        colBlurred += texture(u_Src, v_TexCoord + vec2(0.0, tex_offset.y * i * radius)).rgb * weight[i];
        colBlurred += texture(u_Src, v_TexCoord - vec2(0.0, tex_offset.y * i * radius)).rgb * weight[i];
    }

    color = vec4(colBlurred, 1);
}