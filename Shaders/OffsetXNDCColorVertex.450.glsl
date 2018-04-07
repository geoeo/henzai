#version 450
#extension GL_ARB_separate_shader_objects : enable
#extension GL_ARB_shading_language_420pack : enable

layout(std140) uniform projView
{
    mat4 View;
    mat4 Proj;
};

layout(location = 0)in vec2 Position;
layout(location = 1)in vec4 Color;
layout(location = 2)in float xOff;

// flat out vec4 fsin_Color; // takes color from "dominant" vertex
smooth out vec4 fsin_Color;
// noperspective out vec4 fsin_Color; // interpolates in screen space

void main()
{
    vec2 posOff = vec2(Position.x + xOff,Position.y);
    vec4 worldPos = vec4(posOff, 5, 1);
    gl_Position = Proj*View*worldPos;
    // Vulkan has inverted y NDC
    gl_Position.y = -gl_Position.y;
    fsin_Color = Color;
}
