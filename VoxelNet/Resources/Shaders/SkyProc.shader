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
out vec3 v_WorldPosition;

void main()
{
    v_TexCoord = position.rgb;
    v_WorldPosition = (u_World * position).rgb;

    mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;
    gl_Position = wvp * position;
}

#shader fragment
#version 330 core

#include "Voxel.glsl"
#include "Sky/Atmos.glsl"
#include "Camera.ubo"
#include "Lighting.ubo"

layout(location = 0) out vec4 color;

uniform float U_SunSize;

in vec3 v_TexCoord;
in vec3 v_WorldPosition;

void main()
{
    vec3 lightDir = -normalize(Lighting.SunDirection).rgb;
    vec3 rayDir = normalize(v_WorldPosition);

    vec3 skyScatter = atmosphere(rayDir, vec3(0, 6372e3, 0), lightDir, 22.0, 6371e3, 6471e3, vec3(5.5e-6, 13.0e-6, 22.4e-6), 21e-6, 8e3, 1.2e3, 0.758);

    skyScatter = 1.0 - exp(-1.0 * skyScatter);

    color = vec4(skyScatter,1);
}