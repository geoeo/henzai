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

in vec2 Position;
in vec4 Color;

// flat out vec4 fsin_Color;
smooth out vec4 fsin_Color;
// noperspective out vec4 fsin_Color;

void main()
{
    vec4 worldPos = vec4(Position, 5, 1);
    mat4 viewMatrix = field_projView.View;
    mat4 projMatrix = field_projView.Proj;
    gl_Position = projMatrix*viewMatrix*worldPos;
    fsin_Color = Color;
}
