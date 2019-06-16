#version 330 core

uniform sampler2D ShadowMap;

layout(std140) uniform material
{
    vec4 Diffuse;
    vec4 Specular;
    vec4 Ambient;
    vec4 Coefficients;
};

layout(std140) uniform light
{
    vec4 LightPosition;
    vec4 LightColor;
    vec4 LightAttenuation;
};

in vec3 fsin_NormalWorld;
in vec3 fsin_FragWorld;
in vec3 fsin_CamPosWorld;

out vec4 fsout_Color;

void main()
{
    vec4 lightColor = LightColor.a*vec4(LightColor.rgb,1.0);

    vec3 L = LightPosition.xyz-fsin_FragWorld;
    float distance = length(L);
    L = normalize(L);
    float attenuation = 1.0 / (LightAttenuation.x + distance*LightAttenuation.y + distance*distance*LightAttenuation.z);

    float l_dot_n = max(dot(L,normalize(fsin_NormalWorld)),0.0);
    vec4 diffuse = l_dot_n*Diffuse;

    vec3 R = reflect(-L,fsin_NormalWorld);
    vec3 V = normalize(fsin_CamPosWorld-fsin_FragWorld);
    float isDotFront = max(sign(dot(fsin_NormalWorld,L)),0.0);
    float spec = max(pow(isDotFront*dot(V,R),Coefficients.x),0.0);
    vec4 specular = Specular*spec;

    vec4 color_out = Ambient;
    color_out += attenuation*diffuse;
    color_out += attenuation*specular;
    color_out += attenuation*lightColor;

    float gamma = 2.2;
    fsout_Color = vec4(pow(color_out.rgb, vec3(1.0/gamma)),color_out.a);
    // fsout_Color= color_out;
    // fsout_Color= lightColor*color_out;
    //fsout_Color = vec4(LightColor.rgb,1.0);
    //fsout_Color = vec4(fsin_NormalWorld,1.0);
    // fsout_Color = vec4(fsin_FragWorld,1.0);
    //fsout_Color = vec4(fsin_LightWorld,1.0);
}
