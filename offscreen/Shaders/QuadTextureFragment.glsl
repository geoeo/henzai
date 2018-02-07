#version 330 core

uniform sampler2D ColourTexture;

in vec2 fsin_UV;
out vec4 fsout_Color;

void main()
{
    // Compenstate for Render inverteing when writing FBO to texture
    vec2 UV_Compensated = vec2(fsin_UV.x,-fsin_UV.y);
    vec4 textureColour = texture(ColourTexture,UV_Compensated);
    fsout_Color = textureColour;
}
