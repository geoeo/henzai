#version 330 core

uniform sampler2D QuadTexture;

in vec2 fsin_UV;
out vec4 fsout_Color;

void main()
{
    vec4 textureColour = texture(QuadTexture,fsin_UV);
    fsout_Color = textureColour;
}
