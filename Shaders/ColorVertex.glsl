#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(std140) uniform lightProjView
{
    mat4 LightProjView;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec4 Color;

// flat out vec4 fsin_Color; // takes color from "dominant" vertex
smooth out vec4 fsin_Color;
smooth out vec4 fsin_LightFrag;
// noperspective out vec4 fsin_Color; // interpolates in screen space

void main()
{
    vec4 worldPosition = World*vec4(Position, 1.0);
    gl_Position = Proj*View*worldPosition;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;
    fsin_Color = Color;
    fsin_LightFrag = LightProjView*worldPosition;
}
