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
layout(location = 1)in vec3 Normal;

smooth out vec3 fsin_NormalWorld;
smooth out vec3 fsin_FragWorld;
smooth out vec3 fsin_CamPosWorld;


void main()
{
    vec4 worldPos = World*vec4(Position, 1);
    gl_Position = Proj*View*worldPos;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;

    mat3 normalMatrix = mat3(World);
    fsin_NormalWorld = normalMatrix*Normal;
    fsin_FragWorld = worldPos.xyz;
    fsin_CamPosWorld = View[3].xyz;
}
