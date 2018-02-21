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
layout(location = 1)in vec3 Normals;
layout(location = 2)in vec2 UVs;

smooth out vec3 fsin_Normals;

void main()
{
    vec4 worldPos = vec4(Position, 1);
    mat4 viewMatrix = field_projView.View;
    mat4 projMatrix = field_projView.Proj;
    gl_Position = projMatrix*viewMatrix*worldPos;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;
    fsin_Normals = Normals;
}
