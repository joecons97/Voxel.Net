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
out vec3 v_Position;

void main()
{
    v_TexCoord = position.rgb;
    v_WorldPosition = (u_World * position).rgb;
    v_Position = (position).rgb;

    mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;
    gl_Position = wvp * position;
}

#shader fragment
#version 330 core

#queue opaque

#include "Voxel.glsl"
#include "Sky/Scattering.glsl"
#include "Camera.ubo"
#include "Lighting.ubo"

layout(location = 0) out vec4 color;

uniform float U_SunSize;
uniform float U_SkyStrength;

in vec3 v_TexCoord;
in vec3 v_WorldPosition;
in vec3 v_Position;

float MieScatter(vec3 dir,float size, float hardness)
{
    vec3 delta = normalize(dir) - normalize(v_WorldPosition - Camera.Position.rgb);
    float dist = length(delta);
    float spot = 1.0 - smoothstep(0.0, size, dist);
    return 1.0 - pow(0.125, spot * hardness);
}

void main()
{
    vec3 lightDir = -normalize(Lighting.SunDirection).rgb;

    vec3 skyScatter = GetSkyScatter(vec3(0.529411765, 0.807843137, 0.921568627), v_Position, lightDir, -lightDir, 1) * U_SkyStrength;

    vec3 sun = vec3(MieScatter(-Lighting.SunDirection.rgb, U_SunSize, 50)) * (Lighting.SunColour.rgb);
    vec3 moon = vec3(MieScatter(Lighting.SunDirection.rgb, U_SunSize, 50)) / 2;

    color = vec4(skyScatter + sun + moon, 1);
}