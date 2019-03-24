#version 330 core
out vec4 fsout_Color;

in vec3 TexCoords;

uniform samplerCube CubeSampler;

void main()
{    
    fsout_Color = texture(CubeSampler, TexCoords);
    // fsout_Color = vec4(1,0,0,1);
}