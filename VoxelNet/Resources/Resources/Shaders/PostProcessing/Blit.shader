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

in vec2 v_TexCoord;

void main()
{
    vec4 col = texture(u_Src, v_TexCoord);
    color = col;
}