#version 330 core
out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

/// 离屏渲染 - 创建的帧缓冲区
void main()
{
    FragColor = texture(texture1, TexCoords);
}
