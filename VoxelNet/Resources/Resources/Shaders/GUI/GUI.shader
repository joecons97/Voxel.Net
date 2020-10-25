#shader vertex
#version 330 core

layout(location = 0) in vec4 position;
layout(location = 1) in vec2 texCoord;

out vec2 v_TexCoord;

void main()
{
    v_TexCoord = texCoord;
    gl_Position = position;
}

#shader fragment
#version 330 core

#queue transparent
#blend oneminus

layout(location = 0) out vec4 color;

uniform sampler2D u_Texture;

in vec2 v_TexCoord;

void main()
{
	vec4 texCol = texture(u_Texture, v_TexCoord);

    color = texCol;
}