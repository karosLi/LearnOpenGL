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
 
 
 如果你仔细看的话，你可能会注意到有些不对劲。最前面窗户的透明部分遮蔽了背后的窗户？这为什么会发生呢？
 发生这一现象的原因是，深度测试和混合一起使用的话会产生一些麻烦。当写入深度缓冲时，深度缓冲不会检查片段是否是透明的，所以透明的部分会和其它值一样写入到深度缓冲中。结果就是窗户的整个四边形不论透明度都会进行深度测试。即使透明的部分应该显示背后的窗户，深度测试仍然丢弃了它们。
 
 关于为什么会产生前面窗户的透明部分挡住后面的窗户，我的理解：
 1、由于窗户是比箱子先绘制，当我们先绘制前面的窗户时，因为窗户的z值比深度缓冲中的深度值（箱子的z值）小，那么此时的深度测试结果是通过的，所以窗户的片段（源片段）会覆盖箱子的片段（目标片段），但由于开启了混合功能，所以在覆盖箱子的片段之前，会先让窗户的片段（源片段）和箱子的片段（目标片段）进行混合计算，然后再覆盖。
 2、然后当我们绘制后面的窗户时（比前面的窗户晚绘制），因为后面的窗户的z值比深度缓冲中的深度值（前面箱子的z值）要大，那么此时深度测试结果是失败的，所以后面窗户与前面窗户重叠的片段会被丢弃掉，这就是为什么会产生前面窗户的透明部分挡住后面的窗户的原因。
 
 注意：深度测试不是实时运行的，而是一次绘制只运行一次（在开启的情况）
 
 那为什么先绘制后面的窗户再绘制前面的窗户（先远后近）就会没问题呢？
 1、因为近的窗户的z值比深度缓冲中的深度值（远的窗户的z值）要小，在当前绘制中的深度测试是通过的，又由于开启了混合功能，所以在覆盖远的窗户的片段之前，会先让近的窗户的片段（源片段）和远的窗户的片段（目标片段）进行混合计算，然后再覆盖，所以能从前面窗户看到后面的窗户了。
 
 深度测试和混合逻辑：
 if (当前绘制片段（源片段）的z值是否小于深度缓冲中的深度值) {// 深度测试通过
    if (是否开启了混合) {// 开启了混合
        让源片段和存储在颜色缓冲区中的目标片段进行混合计算
        结果颜色 = 源片段颜色 * 源片段alpha因子 + 目标片段颜色 * 目标片段alpha因子
    } else {// 没有开启混合
        让源片段直接覆盖存储在颜色缓冲区中的目标片段
        结果颜色 = 源片段颜色
    }
 } else {// 深度测试失败
    discard - 丢失片段
 }
 

 所以我们不能随意地决定如何渲染窗户，让深度缓冲解决所有的问题了。这也是混合变得有些麻烦的部分。要想保证窗户中能够显示它们背后的窗户，我们需要首先绘制背后的这部分窗户。这也就是说在绘制的时候，我们必须先手动将窗户按照最远到最近来排序，再按照顺序渲染。
 注意，对于草这种全透明的物体，我们可以选择丢弃透明的片段而不是混合它们，这样就解决了这些头疼的问题（没有深度问题）。
 
 不要打乱顺序
 要想让混合在多个物体上工作，我们需要最先绘制最远的物体，最后绘制最近的物体。普通不需要混合的物体仍然可以使用深度缓冲正常绘制，所以它们不需要排序。但我们仍要保证它们在绘制（排序的）透明物体之前已经绘制完毕了。当绘制一个有不透明和透明物体的场景的时候，大体的原则如下：

 先绘制所有不透明的物体。
 对所有透明的物体排序。
 按顺序绘制所有透明的物体。
 
 
 https://learnopengl-cn.github.io/04%20Advanced%20OpenGL/03%20Blending/
 */

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

void main()
{
    vec4 texColor = texture(texture1, TexCoords);
    FragColor = texColor;
}
