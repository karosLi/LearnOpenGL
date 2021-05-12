#version 330 core

/**
混合
 
 discard: 可以丢弃片段，一般用于只有完全透明和完全不透明结合的场景。但是无法处理半透明的场景。
 
 颜色混合：
 OpenGL中的混合是通过下面这个方程来实现的：
 C¯result=C¯source∗Fsource+C¯destination∗Fdestination
 
 C¯source：源颜色向量。这是源自纹理的颜色向量。
 C¯destination：目标颜色向量。这是当前储存在颜色缓冲中的颜色向量。
 Fsource：源因子值。指定了alpha值对源颜色的影响。
 Fdestination：目标因子值。指定了alpha值对目标颜色的影响。
 
 
 glBlendFunc(GLenum sfactor, GLenum dfactor)函数接受两个参数，来设置源和目标因子。
 
 GL_ZERO    因子等于0
 GL_ONE    因子等于1
 GL_SRC_COLOR    因子等于源颜色向量C¯source
 GL_ONE_MINUS_SRC_COLOR    因子等于1−C¯source
 GL_DST_COLOR    因子等于目标颜色向量C¯destination
 GL_ONE_MINUS_DST_COLOR    因子等于1−C¯destination
 GL_SRC_ALPHA    因子等于C¯source的alpha分量
 GL_ONE_MINUS_SRC_ALPHA    因子等于1− C¯source的alpha分量
 GL_DST_ALPHA    因子等于C¯destination的alpha分量
 GL_ONE_MINUS_DST_ALPHA    因子等于1− C¯destination的alpha分量
 GL_CONSTANT_COLOR    因子等于常数颜色向量C¯constant
 GL_ONE_MINUS_CONSTANT_COLOR    因子等于1−C¯constant
 GL_CONSTANT_ALPHA    因子等于C¯constant的alpha分量
 GL_ONE_MINUS_CONSTANT_ALPHA    因子等于1− C¯constant的alpha分量
 
 
 OpenGL甚至给了我们更多的灵活性，允许我们改变方程中源和目标部分的运算符。当前源和目标是相加的，但如果愿意的话，我们也可以让它们相减。glBlendEquation(GLenum mode)允许我们设置运算符，它提供了三个选项：
 GL_FUNC_ADD：默认选项，将两个分量相加：C¯result=Src+Dst。
 GL_FUNC_SUBTRACT：将两个分量相减： C¯result=Src−Dst。
 GL_FUNC_REVERSE_SUBTRACT：将两个分量相减，但顺序相反：C¯result=Dst−Src。
 
 
 https://learnopengl-cn.github.io/04%20Advanced%20OpenGL/03%20Blending/
 */

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

void main()
{
    vec4 texColor = texture(texture1, TexCoords);
    if(texColor.a < 0.1)
        discard;
    FragColor = texColor;
}
