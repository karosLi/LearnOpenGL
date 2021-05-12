#version 330 core
layout (location = 0) in vec2 aPos;
layout (location = 1) in vec3 aColor;

/**
 而当我们将顶点属性定义为一个实例化数组时，顶点着色器就只需要对每个实例，而不是每个顶点，更新顶点属性的内容了。
 
 即每次切换绘制实例时，更新实例化数组当前实例索引对应的值
 */
layout (location = 2) in vec2 aOffset;

out vec3 fColor;

void main()
{
    fColor = aColor;
    
    gl_Position = vec4(aPos + aOffset, 0.0, 1.0);
    
    // 为了更有趣一点，我们也可以使用gl_InstanceID，从右上到左下逐渐缩小四边形。gl_InstanceID 是当前绘制的实例索引，从 0 开始
//    vec2 pos = aPos * (gl_InstanceID / 100.0);
//    gl_Position = vec4(pos + aOffset, 0.0, 1.0);
}
