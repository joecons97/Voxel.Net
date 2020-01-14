#shader vertex
#version 330 core
layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;

uniform mat4 u_Projection;
uniform mat4 u_View;
uniform mat4 u_World;

out vec2 v_TexCoord;

void main()
{
    mat4 wvp = u_Projection * u_View * u_World;
    gl_Position = wvp * position;
    v_TexCoord = texCoord;
}

#shader fragment
#version 330 core

layout(location = 0) out vec4 color;

in vec2 v_TexCoord;

uniform sampler2D u_ColorMap;

void main()
{
    color = texture(u_ColorMap, v_TexCoord);
}