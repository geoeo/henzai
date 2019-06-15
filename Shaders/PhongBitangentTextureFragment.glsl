#version 330 core

uniform sampler2D DiffuseTexture;
uniform sampler2D NormTexture;
uniform sampler2D ShadowMap;

layout(std140) uniform light
{
    vec4 LightPosition;
    vec4 LightColor;
    vec4 LightAttenuation;
};

layout(std140) uniform spotlight
{
    vec4 SpotLightPosition;
    vec4 SpotLightColor;
    vec4 SpotLightDirection;// xyz for direction, w for linear attenuation
    vec4 SpotLightParameters; // cutoff,inner cutoff,epsilon,is Set
};

layout(std140) uniform material
{
    vec4 Diffuse;
    vec4 Specular;
    vec4 Ambient;
    vec4 Coefficients;
};

in vec3 fsin_NormalWorld;
in vec3 fsin_TangentWorld;
in vec3 fsin_BitangentWorld;
in vec3 fsin_FragWorld;
in vec3 fsin_LightWorld;
in vec3 fsin_CamPosWorld;
in vec2 fsin_UV;

out vec4 fsout_Color;

void main()
{
    vec4 lightColor = LightColor.a*vec4(LightColor.rgb,1.0);

    vec4 textureColor = texture(DiffuseTexture,fsin_UV);
    vec3 normalColor = texture(NormTexture,fsin_UV).xyz;
    vec3 normal_sample = normalize(2.0*normalColor-1.0);

    vec3 Normal = normalize(fsin_NormalWorld);
    vec3 Tangent = normalize(fsin_TangentWorld);
    vec3 Bitangent = normalize(fsin_BitangentWorld);

    
    mat3 TBN = mat3(Tangent, Bitangent, Normal);

    // vec3 normalWS = Normal;
    vec3 normalWS = normalize(TBN*normal_sample);

    vec3 L;
    float attenuation;
    if(LightPosition.w == 1.0){
        L = LightPosition.xyz-fsin_FragWorld;
        float distance = length(L);
        attenuation = 1.0 / (LightAttenuation.x + distance*LightAttenuation.y + distance*distance*LightAttenuation.z);
    }
    else {
        L = -LightPosition.xyz;
        attenuation = 1.0;
    }
    L = normalize(L);
    vec4 pl_color = vec4(0.0f);
    if(SpotLightParameters.w == 1.0f){
        vec3 lightDir = fsin_FragWorld-SpotLightPosition.xyz;
        float distance = length(lightDir);
        float theta = dot(normalize(lightDir),normalize(SpotLightDirection.xyz));
        float epsilon = SpotLightParameters.y - SpotLightParameters.x;
        float intensity = clamp((theta - SpotLightParameters.x) / epsilon, 0.0, 1.0);
        float pl_attenuation = 1.0 / (1.0 +SpotLightDirection.w *distance );
        pl_color = SpotLightColor*pl_attenuation*intensity;
    }


    float l_dot_n = max(dot(L,normalWS),0.0);
    vec4 diffuse = l_dot_n*Diffuse*textureColor;

    vec3 R = reflect(-L,normalWS);
    vec3 V = normalize(fsin_CamPosWorld-fsin_FragWorld);
    float isDotFront = max(sign(dot(normalWS,L)),0.0);
    float spec = max(pow(isDotFront*dot(V,R),Coefficients.x),0.0);
    vec4 specular = Specular*spec;

    vec4 color_out = vec4(0.0);
    color_out += Ambient;
    color_out += attenuation*diffuse;
    color_out += attenuation*specular;
    color_out += attenuation*lightColor;
    color_out += pl_color;

    float gamma = 2.2;
    fsout_Color = vec4(pow(color_out.rgb, vec3(1.0/gamma)),color_out.a);
    // fsout_Color = color_out;
    // fsout_Color = vec4(fsin_NormalWorld,1.0);
    // fsout_Color = vec4(fsin_TangentWorld,1.0);
    // fsout_Color = vec4(fsin_BitangentWorld,1.0);
    // fsout_Color = vec4(normalWS,1.0);
    // fsout_Color = vec4(normal_sample,1.0);
    // fsout_Color = vec4(normalColor,1.0);
    // fsout_Color = vec4(fsin_LightWorld,1.0);
    // fsout_Color = vec4(fsin_UV,1.0,1.0);
    // fsout_Color = textureColor;
    // fsout_Color = vec4(L,1.0);
    // fsout_Color = vec4(l_dot_n,l_dot_n,l_dot_n,1.0);
    // fsout_Color = vec4(attenuation,attenuation,attenuation,1.0);
    // fsout_Color = vec4(attenuation,attenuation,attenuation,1.0);
    // fsout_Color = vec4(LightColor.a,LightColor.a,LightColor.a,1.0);
}
