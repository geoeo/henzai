#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(location = 0)in vec3 Position;

void main()
{
    gl_Position = World*vec4(Position, 1);
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;

}
