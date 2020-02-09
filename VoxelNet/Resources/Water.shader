#shader vertex
#version 330 core

#include "Camera.ubo"
#include "Time.ubo"

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec4 normal;
layout(location = 3) in vec2 texCoord2;
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

float N21(vec2 p)
{
    p = fract(p * vec2(123.34, 345.45));
    p += dot(p,p+34.345);
    return fract(p.x*p.y);
}

void main()
{
    v_Normal = normalize(normal * u_World);

    v_Color = vertcolor;

    v_TexCoord = texCoord;

    vec4 worldPos = u_World * position;

    float height = sin(Time.Time * N21(worldPos.xz)) * 0.05f;
    height -= 0.05f;

    vec4 finalPos = position + vec4(0,height,0,0);

    mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;
    gl_Position = (wvp * finalPos);
}

#shader fragment
#version 330 core

#type transparent
#culling none

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

	vec4 final = texCol * pxLight;

    color = vec4(final.r,final.g,final.b,texCol.a);
}