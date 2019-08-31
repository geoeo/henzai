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
layout(location = 2)in vec2 UV;
layout(location = 3)in vec3 Tangent;
layout(location = 4)in vec3 Bitangent;
layout(location = 5)in vec4 ViewCol1;
layout(location = 6)in vec4 ViewCol2;
layout(location = 7)in vec4 ViewCol3;
layout(location = 8)in vec4 ViewCol4;

smooth out vec3 fsin_NormalWorld;
smooth out vec3 fsin_TangentWorld;
smooth out vec3 fsin_BitangentWorld;
smooth out vec3 fsin_FragWorld;
smooth out vec3 fsin_CamPosWorld;
smooth out vec2 fsin_UV;
smooth out vec4 fsin_LightFrag;

void main()
{
    vec4 worldPos = World*vec4(Position, 1);
    mat4 viewInstance = mat4(ViewCol1,ViewCol2,ViewCol3,ViewCol4);
    
    gl_Position = Proj*viewInstance*worldPos;
    // composensate for D3D projection
    gl_Position.z = 2.0*gl_Position.z -gl_Position.w;

    mat3 normalMatrix = mat3(World);
    fsin_NormalWorld = normalMatrix*Normal;
    fsin_FragWorld = worldPos.xyz;
    fsin_CamPosWorld = viewInstance[3].xyz;
    fsin_TangentWorld = normalMatrix*Tangent;
    fsin_BitangentWorld = normalMatrix*Bitangent;
    fsin_UV = UV;
    fsin_LightFrag = LightProjView*worldPos;
}