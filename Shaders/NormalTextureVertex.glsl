#version 330 core

// Name!
layout(std140) uniform projView
{
    mat4 View;
    mat4 Proj;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec3 Normal;
layout(location = 2)in vec2 UV;

smooth out vec3 fsin_Normal;

void main()
{
    vec4 worldPos = vec4(Position, 1);
    gl_Position = Proj*View*worldPos;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;
    fsin_Normal = Normal;
}
