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
uniform vec2 u_Dimensions;
uniform float u_BorderSize;

in vec2 v_TexCoord;

float map(float value, float originalMin, float originalMax, float newMin, float newMax) {
    return (value - originalMin) / (originalMax - originalMin) * (newMax - newMin) + newMin;
}

// Helper function, because WET code is bad code
// Takes in the coordinate on the current axis and the borders 
float processAxis(float coord, float textureBorder, float windowBorder) {
    if (coord < windowBorder)
        return map(coord, 0, windowBorder, 0, textureBorder);
    if (coord < 1 - windowBorder)
        return map(coord, windowBorder, 1 - windowBorder, textureBorder, 1 - textureBorder);
    return map(coord, 1 - windowBorder, 1, 1 - textureBorder, 1);
}

void main()
{
    vec2 dimensions = vec2(u_BorderSize / u_Dimensions.x, u_BorderSize / u_Dimensions.y);
    vec2 clip = vec2(u_BorderSize) / textureSize(u_Texture, 0);

    vec2 newUV = vec2(
        processAxis(v_TexCoord.x, clip.x, dimensions.x),
        processAxis(v_TexCoord.y, clip.y, dimensions.y)
    );
    
	vec4 texCol = texture(u_Texture, newUV);

    color = texCol;
}