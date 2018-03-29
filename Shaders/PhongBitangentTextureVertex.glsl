#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(std140) uniform light
{
    vec4 LightPosition;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec3 Normal;
layout(location = 2)in vec2 UV;
layout(location = 3)in vec3 Tangent;
layout(location = 4)in vec3 Bitangent;

smooth out vec3 fsin_NormalWorld;
smooth out vec3 fsin_TangentWorld;
smooth out vec3 fsin_BitangentWorld;
smooth out vec3 fsin_FragWorld;
smooth out vec3 fsin_LightWorld;
smooth out vec3 fsin_CamPosWorld;
smooth out vec2 fsin_UV;

void main()
{
    vec4 worldPos = World*vec4(Position, 1);

    mat4 viewMatrix = View;
    vec4 viewPos = viewMatrix*worldPos;

    mat4 projMatrix = Proj;

    gl_Position = projMatrix*viewPos;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;

    mat3 normalMatrix = mat3(World);
    fsin_NormalWorld = normalMatrix*Normal;
    fsin_FragWorld = worldPos.xyz;
    fsin_LightWorld = LightPosition.xyz;
    fsin_CamPosWorld = viewMatrix[3].xyz;
    fsin_TangentWorld = normalMatrix*Tangent;
    fsin_BitangentWorld = normalMatrix*Bitangent;
    fsin_UV = UV;
}
