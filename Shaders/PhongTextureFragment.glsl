#version 330 core

uniform sampler2D DiffuseTexture;
uniform sampler2D NormTexture;

struct Material
{
    vec4 Diffuse;
    vec4 Specular;
    vec4 Ambient;
    vec4 Coefficients;
};

layout(std140) uniform material
{
    Material field_material;
};

in vec3 fsin_NormalWorld;
in vec3 fsin_TangentWorld;
in vec3 fsin_FragWorld;
in vec3 fsin_LightWorld;
in vec3 fsin_CamPosWorld;
in vec2 fsin_UV;

out vec4 fsout_Color;

void main()
{
    vec4 textureColor = texture(DiffuseTexture,fsin_UV);
    vec3 normal_sample = normalize(2.0*texture(NormTexture,fsin_UV).xyz-1.0);

    vec3 Normal = normalize(fsin_NormalWorld);
    vec3 Tangent = normalize(fsin_TangentWorld);
    vec3 Bitangent = normalize(cross(Tangent, Normal));
    
    mat3 TBN = mat3(Tangent, Bitangent, Normal);

    //normal_sample = Normal;
    vec3 normalWS = normalize(TBN*normal_sample);

    vec3 L = normalize(fsin_LightWorld-fsin_FragWorld);
    float l_dot_n = dot(L,normalWS);
    vec4 diffuse = field_material.Diffuse*textureColor;

    vec3 R = reflect(-L,normalWS);
    vec3 V = normalize(fsin_CamPosWorld-fsin_FragWorld);
    float isDotFront = max(sign(dot(normalWS,L)),0.0);
    float spec = pow(isDotFront*dot(V,R),field_material.Coefficients.x);
    vec4 specular = field_material.Specular*spec;

    vec4 color_out = vec4(0.0,0.0,0.0,0.0);
    color_out += field_material.Ambient;
    color_out += diffuse;
    color_out += specular;
    fsout_Color = color_out;
    // fsout_Color = vec4(fsin_NormalWorld,1.0);
    // fsout_Color = vec4(fsin_TangentWorld,1.0);
    // fsout_Color = vec4(normalWS,1.0);
    // fsout_Color = vec4(normal_sample,1.0);
    // fsout_Color = vec4(fsin_LightWorld,1.0);
    // fsout_Color = vec4(fsin_UV,1.0,1.0);
    // fsout_Color = textureColor;
    // fsout_Color = vec4(L,1.0);
    // fsout_Color = vec4(l_dot_n,l_dot_n,l_dot_n,1.0);
}
