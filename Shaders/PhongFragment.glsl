#version 330 core

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
in vec3 fsin_LightWorld;
in vec3 fsin_CamPosWorld;

out vec4 fsout_Color;

void main()
{
    vec4 lightColor = LightColor.a*vec4(LightColor.rgb,1.0);

    vec3 L = normalize(fsin_LightWorld-fsin_FragWorld);
    float l_dot_n = max(dot(L,fsin_NormalWorld),0.0);
    vec4 diffuse = l_dot_n*Diffuse;

    vec3 R = reflect(-L,fsin_NormalWorld);
    vec3 V = normalize(fsin_CamPosWorld-fsin_FragWorld);
    float isDotFront = max(sign(dot(fsin_NormalWorld,L)),0.0);
    float spec = max(pow(isDotFront*dot(V,R),Coefficients.x),0.0);
    vec4 specular = Specular*spec;

    vec4 color_out = Ambient;
    color_out += diffuse;
    color_out += specular;
    fsout_Color = lightColor*color_out;
    //fsout_Color = vec4(LightColor.rgb,1.0);
    //fsout_Color = vec4(fsin_NormalWorld,1.0);
    // fsout_Color = vec4(fsin_FragWorld,1.0);
    //fsout_Color = vec4(fsin_LightWorld,1.0);
}
