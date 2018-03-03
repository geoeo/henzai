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


layout(location = 0)in vec3 Position;
layout(location = 1)in vec3 Normal;

smooth out vec3 fsin_NormalView;
smooth out vec3 fsin_FragView;
smooth out vec3 fsin_LightView;

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

    fsin_NormalView = normalViewMatrix*Normal;
    fsin_FragView = viewPos.xyz;
    vec4 lightView = viewMatrix*vec4(LightPosition,1.0);
    fsin_LightView = lightView.xyz;
}
