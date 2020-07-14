#shader vertex
#version 330 core

#include "Camera.ubo"

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec4 normal;
layout(location = 4) in vec4 vertcolor;

uniform mat4 u_World = 
mat4(
    1,0,0,0,
    0,1,0,0,
    0,0,1,0,
    0,0,0,1
);

out vec2 v_TexCoord;
out vec4 v_Normal;
out vec4 v_Color;

void main()
{
    v_Normal = normalize(normal * u_World);

    v_Color = vertcolor;

    v_TexCoord = texCoord;

    mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;
    gl_Position = wvp * position;
}

#shader fragment
#version 330 core

#include "Voxel.glsl"
#include "Lighting.ubo"

layout(location = 0) out vec4 color;

uniform sampler2D u_ColorMap;

in vec4 v_Color;
in vec2 v_TexCoord;
in vec4 v_Normal;

void main()
{
	vec4 texCol = texture(u_ColorMap, v_TexCoord);

	vec3 worldNormal = normalize(v_Normal.rgb);
	vec3 lightDir = vec3(0, .5, .5);

	float ndl = saturate(dot(worldNormal.rgb, -Lighting.SunDirection.rgb));
	vec4 pxLight = saturate((ndl * Lighting.SunStrength * Lighting.SunColour)) + Lighting.AmbientColour;

	if (texCol.a < 0.5)
		discard;

    color = texCol * pxLight;
}