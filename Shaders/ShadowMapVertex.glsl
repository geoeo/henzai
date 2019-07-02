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
    vec4 worldPos = World*vec4(Position, 1);

    gl_Position = Proj*View*worldPos;
    // composensate for D3D projection
    //gl_Position.z = 2.0*gl_Position.z -gl_Position.w;
}
