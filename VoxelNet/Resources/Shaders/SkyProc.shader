#shader vertex
#version 330 core

#include "Camera.ubo"

layout(location = 0) in vec4 position;

uniform mat4 u_World = 
mat4(
    1,0,0,0,
    0,1,0,0,
    0,0,1,0,
    0,0,0,1
);

out vec3 v_TexCoord;

void main()
{
    v_TexCoord = position.rgb;

    mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;
    gl_Position = wvp * position;
}

#shader fragment
#version 330 core

#include "Voxel.incl"
#include "Lighting.ubo"

layout(location = 0) out vec4 color;

uniform float U_SunSize;

in vec3 v_TexCoord;

void main()
{
    vec3 v = normalize(v_TexCoord);

    vec3 lightDir = -normalize(Lighting.SunDirection).rgb;

    vec4 sun = min(pow(max(0, dot(v, lightDir)), 30), 1) * (Lighting.SunStrength * 2e4) * Lighting.SunColour;

    color = vec4(.52,0.8,.92,1) + sun;
}