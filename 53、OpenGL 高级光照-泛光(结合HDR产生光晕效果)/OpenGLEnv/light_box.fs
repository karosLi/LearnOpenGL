#version 330 core
/**
 绘制立方体灯
 */
layout (location = 0) out vec4 FragColor;// 颜色缓冲 0
layout (location = 1) out vec4 BrightColor;//颜色缓冲 1

in VS_OUT {
    vec3 FragPos;
    vec3 Normal;
    vec2 TexCoords;
} fs_in;

uniform vec3 lightColor;

void main()
{
    // 所有片段都输出到 颜色缓冲 0
    FragColor = vec4(lightColor, 1.0);
    // 获取物体的亮度
    float brightness = dot(FragColor.rgb, vec3(0.2126, 0.7152, 0.0722));
    
    // 所有片段都输出到 颜色缓冲 1
    if(brightness > 1.0)// 如果亮度大于 1，则输出当前片段颜色，否则用黑色填充
        BrightColor = vec4(FragColor.rgb, 1.0);
    else// 否则填充黑色
        BrightColor = vec4(0.0, 0.0, 0.0, 1.0);
}
