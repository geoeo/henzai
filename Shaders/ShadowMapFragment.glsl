#version 330 core

out vec4 fsout_Color;

void main()
{
    //Passthrough for depth
    gl_FragDepth = gl_FragCoord.z;
    fsout_Color = vec4(0,1,1,1);
}
