#version 330 core

uniform sampler2D DiffuseTexture;
uniform sampler2D NormTexture;
uniform sampler2D ShadowMap;

layout(std140) uniform material
{
    vec4 Diffuse;
    vec4 Specular;
    vec4 Ambient;
    vec4 Coefficients;
};

in vec3 fsin_NormalWorld;
in vec3 fsin_TangentWorld;
in vec3 fsin_FragWorld;
in vec3 fsin_LightWorld;
in vec3 fsin_CamPosWorld;
in vec2 fsin_UV;
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
    vec4 textureColor = texture(DiffuseTexture,fsin_UV);
    vec3 normal_sample = normalize(2.0*texture(NormTexture,fsin_UV).xyz-1.0);

    vec3 Normal = normalize(fsin_NormalWorld);
    vec3 Tangent = normalize(fsin_TangentWorld);
    vec3 Bitangent = normalize(cross(Tangent, Normal));
    
    mat3 TBN = mat3(Tangent, Bitangent, Normal);

    // vec3 normalWS = normal_sample;
    vec3 normalWS = normalize(TBN*normal_sample);

    vec3 L = normalize(fsin_LightWorld-fsin_FragWorld);
    float l_dot_n = max(dot(L,normalWS),0.0);
    vec4 diffuse = Diffuse*textureColor;

    vec3 R = reflect(-L,normalWS);
    vec3 V = normalize(fsin_CamPosWorld-fsin_FragWorld);
    float isDotFront = max(sign(dot(normalWS,L)),0.0);
    float spec = max(pow(isDotFront*dot(V,R),Coefficients.x),0.0);
    vec4 specular = Specular*spec;

    float shadow = ShadowCalculation(fsin_LightFrag, l_dot_n);

    vec4 color_out = vec4(0.0,0.0,0.0,0.0);
    color_out += diffuse;
    color_out += specular;
    color_out *= (1.0 - shadow);
    color_out += Ambient*diffuse;

    float gamma = 2.2;
    fsout_Color = vec4(pow(color_out.rgb, vec3(1.0/gamma)),color_out.a);
    // fsout_Color = vec4(fsin_NormalWorld,1.0);
    // fsout_Color = vec4(Normal,1.0);
    // fsout_Color = vec4(fsin_TangentWorld,1.0);
    // fsout_Color = vec4(normalWS,1.0);
    // fsout_Color = vec4(normal_sample,1.0);
    // fsout_Color = vec4(fsin_LightWorld,1.0);
    // fsout_Color = vec4(fsin_UV,1.0,1.0);
    // fsout_Color = textureColor;
    // fsout_Color = vec4(L,1.0);
    // fsout_Color = vec4(l_dot_n,l_dot_n,l_dot_n,1.0);
}
