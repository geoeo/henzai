#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(std140) uniform light
{
    vec3 LightPosition;
};

layout(std140) uniform normalMatrix
{
    mat4 NormalMatirx;
};

layout(location = 0)in vec3 Position;
layout(location = 1)in vec3 Normal;
layout(location = 2)in vec2 UV;
layout(location = 3)in vec3 Tangent;

smooth out vec3 fsin_NormalView;
//smooth out vec3 fsin_Tangent;
smooth out vec3 fsin_FragView;
smooth out vec3 fsin_LightView;
smooth out vec3 fsin_CamPosWorld;
smooth out vec2 fsin_UV;

void main()
{
    vec4 worldPos = World*vec4(Position, 1);

    mat4 viewMatrix = View;
    mat3 normalViewMatrix = mat3(viewMatrix);
    vec4 viewPos = viewMatrix*worldPos;

    mat4 projMatrix = Proj;

    gl_Position = projMatrix*viewPos;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;

    mat3 normalMatrix = mat3(NormalMatirx);
    fsin_NormalView = normalMatrix*Normal;
    fsin_FragView = worldPos.xyz;
    vec4 lightView = vec4(LightPosition,1.0);
    fsin_LightView = lightView.xyz;
    fsin_CamPosWorld = viewMatrix[3].xyz;
    fsin_UV = UV;
}
