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
out vec4 v_ClipSpace;
out float v_WaveHeight;
out vec4 v_Normal;
out vec4 v_Color;
out vec4 v_WorldPos;

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

    v_WorldPos = u_World * position;

    float height = sin(Time.Time * N21(v_WorldPos.xz)) * 0.05f;
    v_WaveHeight = height;
    height -= 0.05f;


    vec4 finalPos = position + vec4(0,height,0,0);

    mat4 wvp = Camera.ProjectionMat * Camera.ViewMat * u_World;

    vec4 outPos = (wvp * finalPos);
    v_ClipSpace = outPos;

    gl_Position = outPos;
}

#shader fragment
#version 330 core

#queue transparent
#blend none
#culling none

#include "Voxel.glsl"
#include "Lighting.ubo"
#include "Camera.ubo"

layout(location = 0) out vec4 color;

uniform sampler2D u_Src;
uniform sampler2D u_Depth;
uniform sampler2D u_ColorMap;

uniform vec4 u_ShallowColour;
uniform vec4 u_DeepColour;
uniform float u_ColourDepth;
uniform float u_waterDepth;

in vec4 v_Color;
in vec4 v_WaveHeight;
in vec4 v_ClipSpace;
in vec2 v_TexCoord;
in vec4 v_Normal;
in vec4 v_WorldPos;

float BlinnPhong(float smoothness, vec3 normal, vec3 lightDir, vec3 viewDir)
{
    if (smoothness > 0.99f)
        smoothness = 0.99f;

    float shininess = 2 / pow((1 - smoothness), 2) - 2;

    vec3 halfWay = normalize(lightDir + viewDir);
    float angle = max(dot(halfWay, normal), 0.0);
    return pow(angle, shininess);
}

float GetDepth(float exponent)
{
    float near = 0.1f;
    float far = 900.0f;

    vec2 texSize = textureSize(u_Depth, 0);
    vec2 uvs = gl_FragCoord.xy / texSize;
    float depth = texture(u_Depth, uvs).r;

    float waterDepth = depthToLinear(depth, near, far);

    float waterDist = depthToLinear(gl_FragCoord.z, near, far);

    float totalDepth = waterDepth - waterDist;

    return exp(-totalDepth * exponent);
}
//TODO DEPTH
void main()
{
    vec2 texSize = textureSize(u_Src, 0);
    vec2 uvs = gl_FragCoord.xy / texSize;
    uvs += vec2(v_WaveHeight.x, v_WaveHeight.y) * 0.08f;
    vec4 texCol = texture(u_Src, uvs);

    vec4 waveTex = texture(u_ColorMap, v_TexCoord);

	vec3 worldNormal = normalize(v_Normal.rgb);

	float ndl = saturate(dot(worldNormal.rgb, -Lighting.SunDirection.rgb));
    float gloss = 0;
    if (ndl > 0) 
    {
        vec3 viewDir = normalize(Camera.Position.rgb - v_WorldPos.rgb);
        gloss = BlinnPhong(1.7f * (1- waveTex.b), worldNormal, -Lighting.SunDirection.rgb, viewDir);
    }
	vec4 pxLight = max((ndl * Lighting.SunStrength * Lighting.SunColour), 0) + Lighting.AmbientColour;

    vec4 waterColour = mix(u_DeepColour, u_ShallowColour, GetDepth(u_ColourDepth)) * waveTex;

	vec4 final = mix(texCol, waterColour * pxLight, 1 - GetDepth(u_waterDepth));

   //vec4 bg = texture(u_Src, uvs);
    vec3 spec = gloss * Lighting.SunColour.rgb * 5;
    color = vec4(final.rgb + spec, 1);
}