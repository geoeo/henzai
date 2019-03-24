#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec2 UV;
layout(location = 2)in vec3 Offset;

smooth out vec2 fsin_UV;

void main()
{
    vec4 position = vec4(Position.x+Offset.x,Position.y+Offset.y,Position.z+Offset.z, 1);
    gl_Position = Proj*View*World*position;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z - gl_Position.w;
    fsin_UV = UV;
}
