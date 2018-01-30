#version 330 core

in vec2 Position;
in vec4 Color;

// flat out vec4 fsin_Color;
smooth out vec4 fsin_Color;
// noperspective out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}
