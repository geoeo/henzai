#version 330 core

uniform sampler2D ShadowMapTexture;

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
in vec4 fsin_LightFrag;

out vec4 fsout_Color;

float ShadowCalculation(vec4 fragPosLightSpace, float l_dot_n)
{
    // perform perspective divide
    vec3 projCoords = fragPosLightSpace.xyz / fragPosLightSpace.w;
    projCoords = projCoords*0.5 +0.5;
    // we are sampling textures so we have to sample the center
    // get closest depth value from light's perspective (using [0,1] range fragPosLight as coords)
    float closestDepth = texture(ShadowMapTexture, vec2(projCoords.x , projCoords.y)).r; 
    // get depth of current fragment from light's perspective
    float currentDepth = projCoords.z;
    float bias = max(0.005 * (1.0 - l_dot_n), 0.005);  
    // check whether current frag pos is in shadow
    float shadow = currentDepth - bias > closestDepth  ? 1.0 : 0.0;  

    //return shadow;
    return closestDepth;
} 

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

    float shadow = ShadowCalculation(fsin_LightFrag, l_dot_n);

    vec4 color_out = Ambient;
    color_out += attenuation*diffuse;
    color_out += attenuation*specular;
    color_out += attenuation*lightColor;
    color_out *= (1.0 - shadow);

    float gamma = 2.2;
    fsout_Color = vec4(pow(color_out.rgb, vec3(1.0/gamma)),color_out.a);
    // fsout_Color= color_out;
    // fsout_Color= lightColor*color_out;
    //fsout_Color = vec4(LightColor.rgb,1.0);
    //fsout_Color = vec4(fsin_NormalWorld,1.0);
    // fsout_Color = vec4(fsin_FragWorld,1.0);
    //fsout_Color = vec4(fsin_LightWorld,1.0);
    fsout_Color = vec4(shadow,shadow,shadow,1.0);
}
