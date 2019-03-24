#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec4 Color;
layout(location = 2)in vec3 Offset;

// flat out vec4 fsin_Color; // takes color from "dominant" vertex
smooth out vec4 fsin_Color;
// noperspective out vec4 fsin_Color; // interpolates in screen space

void main()
{
    vec4 posOff = vec4(Position.x + Offset.x,Position.y+Offset.y, Position.z+Offset.z,1.0);
    gl_Position = Proj*View*World*posOff;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;
    fsin_Color = Color;
}
