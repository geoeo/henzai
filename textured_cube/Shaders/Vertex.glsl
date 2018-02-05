#version 330 core

struct Camera_ProjView
{
    mat4 View;
    mat4 Proj;
};

layout(std140) uniform projView
{
    Camera_ProjView field_projView;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec2 UV;

smooth out vec4 fsin_Color;

void main()
{
    vec2 posOff = vec2(Position.x,Position.y);
    vec4 worldPos = vec4(Position, 1);
    mat4 viewMatrix = field_projView.View;
    mat4 projMatrix = field_projView.Proj;
    gl_Position = projMatrix*viewMatrix*worldPos;
    fsin_Color = vec4(1,0,0,1);
}
