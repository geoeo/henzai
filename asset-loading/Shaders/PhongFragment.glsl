#version 330 core

struct Material
{
    vec4 diffuse;
    vec4 specular;
    vec4 ambient;
};

struct Light
{
    vec3 position;
};

layout(std140) uniform material
{
    Material field_material;
};

layout(std140) uniform light
{
    Light field_light;
};

in vec3 fsin_Normal;

out vec4 fsout_Color;

void main()
{
    //fsout_Color = vec4(fsin_Normal,1.0);
    fsout_Color = field_material.diffuse;
}
