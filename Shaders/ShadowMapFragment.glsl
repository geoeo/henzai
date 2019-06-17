#version 330 core

void main()
{
    //Passthrough for depth
    gl_FragDepth = gl_FragCoord.z;
}
