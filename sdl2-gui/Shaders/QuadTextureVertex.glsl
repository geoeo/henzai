#version 330 core

struct Transform_Pipeline
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(std140) uniform transformPipeline
{
    Transform_Pipeline field_pipeline;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec2 UV;

// flat out vec4 fsin_Color; // takes colour from "dominant" vertex
smooth out vec2 fsin_UV;
// noperspective out vec4 fsin_Color; // interpolates in screen space

void main()
{
    vec4 position = vec4(Position, 1);
    gl_Position = field_pipeline.Proj*field_pipeline.View*field_pipeline.World*position;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;
    fsin_UV = UV;
}
