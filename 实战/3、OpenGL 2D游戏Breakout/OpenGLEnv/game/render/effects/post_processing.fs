#version 330 core
/**
 这个着色器几乎直接构建自帧缓冲教程的片段着色器，并根据被激活的特效类型进行相应的后期处理。这一次，偏移矩阵(offset matrix)和卷积核作为uniform变量，由应用程序中的代码定义。好处是我们只需要设置这些内容一次，而不必在每个片段着色器执行时重新计算这些矩阵。例如，偏移矩阵的配置如下所示：

 GLfloat offset = 1.0f / 300.0f;
 GLfloat offsets[9][2] = {
     { -offset,  offset  },  // 左上
     {  0.0f,    offset  },  // 中上
     {  offset,  offset  },  // 右上
     { -offset,  0.0f    },  // 左中
     {  0.0f,    0.0f    },  // 正中
     {  offset,  0.0f    },  // 右中
     { -offset, -offset  },  // 左下
     {  0.0f,   -offset  },  // 中下
     {  offset, -offset  }   // 右下
 };
 glUniform2fv(glGetUniformLocation(shader.ID, "offsets"), 9, (GLfloat*)offsets);
 */

in  vec2  TexCoords;
out vec4  color;

uniform sampler2D scene;
uniform vec2      offsets[9];
uniform int       edge_kernel[9];
uniform float     blur_kernel[9];

uniform bool chaos;
uniform bool confuse;
uniform bool shake;

void main()
{
    // zero out memory since an out variable is initialized with undefined values by default
    color = vec4(0.0f);
    vec3 sample[9];
    // 如果使用卷积矩阵，则对纹理的偏移像素进行采样
    if(chaos || shake)
        for(int i = 0; i < 9; i++)
            sample[i] = vec3(texture(scene, TexCoords.st + offsets[i]));

    // 处理特效
    if(chaos)
    {
        for(int i = 0; i < 9; i++)
            color += vec4(sample[i] * edge_kernel[i], 0.0f);
        color.a = 1.0f;
    }
    else if(confuse)
    {
        color = vec4(1.0 - texture(scene, TexCoords).rgb, 1.0);
    }
    else if(shake)
    {
        for(int i = 0; i < 9; i++)
            color += vec4(sample[i] * blur_kernel[i], 0.0f);
        color.a = 1.0f;
    }
    else
    {
        color =  texture(scene, TexCoords);
    }
}
