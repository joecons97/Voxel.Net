#shader vertex
#version 330 core
layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec4 normal;

uniform mat4 u_Projection;
uniform mat4 u_View;
uniform mat4 u_World;

out vec2 v_TexCoord;
out vec4 v_Normal;

void main()
{
    mat4 wvp = u_Projection * u_View * u_World;
    gl_Position = wvp * position;

    v_Normal = u_World * normal;
    v_TexCoord = texCoord;
}

#shader fragment
#version 330 core

layout(location = 0) out vec4 color;

uniform sampler2D u_ColorMap;

in vec2 v_TexCoord;
in vec4 v_Normal;

void main()
{
    vec4 worldNormal = normalize(v_Normal);
    vec3 lightDir = vec3(0, 0.5, -1);

    float ndl = 1.0;// clamp(dot(worldNormal.rgb, lightDir), 0.0, 1.0);
    color = texture(u_ColorMap, v_TexCoord) * ndl;
}