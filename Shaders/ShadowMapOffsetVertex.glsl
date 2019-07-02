#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec3 Offset;


void main()
{
    vec4 position = vec4(Position.x+Offset.x,Position.y+Offset.y,Position.z+Offset.z, 1);
    vec4 worldPos = World*position;

    gl_Position = Proj*View*worldPos;
    //gl_Position.z = 2.0*gl_Position.z -gl_Position.w;
}
