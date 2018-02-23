#version 330 core

struct Material
{
    vec4 diffuse;
    vec4 specular;
    vec4 ambient;
};

layout(std140) uniform material
{
    Material field_material;
};

in vec3 fsin_NormalView;
in vec3 fsin_FragView;
in vec3 fsin_LightView;

out vec4 fsout_Color;

void main()
{
    vec4 color_out = field_material.ambient;

    vec3 L = normalize(fsin_LightView-fsin_FragView);
    float l_dot_n = dot(L,fsin_NormalView);
    vec4 diffuse = l_dot_n*field_material.diffuse;

    vec3 R = reflect(-L,fsin_NormalView);
    vec3 V = normalize(fsin_LightView);
    float spec = pow(max(dot(V,R),0.0),32.0);
    vec4 specular = field_material.specular*spec;


    color_out += diffuse;
    color_out += specular;
    fsout_Color = color_out;
    //fsout_Color = vec4(fsin_NormalView,1.0);
}
