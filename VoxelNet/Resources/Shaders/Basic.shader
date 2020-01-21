#shader vertex
#version 330 core
layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec4 normal;
layout(location = 3) in vec2 texCoord2;
layout(location = 4) in vec4 vertcolor;

#include "Camera.ubo"

//Set world default value to identity matrix...
uniform mat4 u_World = 
mat4(
    1,0,0,0,
    0,1,0,0,
    0,0,1,0,
    0,0,0,1
);

out vec2 v_TexCoord2;
out vec2 v_TexCoord;
out vec4 v_Normal;
out vec4 v_Color;

void main()
{
    mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;
    gl_Position = wvp * position;

    v_Normal = normalize(normal * u_World);

    v_Color = vertcolor;

    v_TexCoord = texCoord;
    v_TexCoord2 = texCoord2;
}

#shader fragment
#version 330 core

layout(location = 0) out vec4 color;

uniform sampler2D u_ColorMap;

#include "Lighting.ubo"

in vec4 v_Color;
in vec2 v_TexCoord2;
in vec2 v_TexCoord;
in vec4 v_Normal;

void main()
{
    vec3 worldNormal = normalize(v_Normal.rgb);
    vec3 lightDir = vec3(0, .5, .5);

    float ndl = clamp(dot(worldNormal.rgb, -Lighting.SunDirection.rgb), 0.0, 1.0) * Lighting.SunStrength;
    vec4 pxLight = (ndl * Lighting.SunColour);// +Lighting.AmbientColour;

    vec4 texCol = texture(u_ColorMap, v_TexCoord);

    vec4 mask = texture(u_ColorMap, v_TexCoord2);
    if (mask.a != 0)
    {
        texCol = mask * v_Color;
    }

    color = texCol * pxLight;
}