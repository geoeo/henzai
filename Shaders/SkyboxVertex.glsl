#version 330 core

layout(std140) uniform projViewWorld
{
    mat4 View;
    mat4 Proj;
    mat4 World;
};

layout(location = 0)in vec3 Position;

out vec3 TexCoords;

void main()
{
    TexCoords = Position;
    mat4 View_No_Translation = mat4(View[0],View[1],View[2],vec4(0,0,0,1)); // Remove translation
    vec4 pos = Proj * View_No_Translation * World * vec4(Position, 1.0);
    gl_Position = pos.xyww; // ensure depth is always 1 i.e. far
}  