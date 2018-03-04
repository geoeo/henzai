#version 330 core

uniform sampler2D SphereTexture;

struct Material
{
    vec4 Diffuse;
    vec4 Specular;
    vec4 Ambient;
};

layout(std140) uniform material
{
    Material field_material;
};

in vec3 fsin_NormalView;
in vec3 fsin_FragView;
in vec3 fsin_LightView;
in vec2 fsin_UV;

out vec4 fsout_Color;

void main()
{
    vec4 textureColor = texture(SphereTexture,fsin_UV);

    vec3 L = normalize(fsin_LightView-fsin_FragView);
    float l_dot_n = dot(L,fsin_NormalView);
    vec4 diffuse = l_dot_n*field_material.Diffuse;

    vec3 R = reflect(-L,fsin_NormalView);
    vec3 V = normalize(fsin_LightView);
    float isDotFront = max(sign(dot(fsin_NormalView,L)),0.0);
    float spec = pow(isDotFront*dot(V,R),32.0);
    vec4 specular = field_material.Specular*spec;

    vec4 color_out = field_material.Ambient;
    color_out += diffuse;
    color_out += specular;
    fsout_Color = color_out;
    //fsout_Color = vec4(fsin_NormalView,1.0);
    //fsout_Color = vec4(fsin_LightView,1.0);
    // fsout_Color = vec4(fsin_UV,1.0,1.0);
    fsout_Color = textureColor;
}
