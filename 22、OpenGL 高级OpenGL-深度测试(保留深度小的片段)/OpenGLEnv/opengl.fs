#version 330 core

/**
 深度缓冲：存储的是每个片段的 z 值；深度缓冲精度，一般默认是 24 位，z 值精度是非线性的，意思是深度越小，精度越高，深度越大，精度越小。
 深度测试：通过深度测试的片段会保留，否则被丢弃；默认测试函数是 z 值比深度缓冲存储的 z 值小时，就认为是测试通过。
 
 深度缓冲就像颜色缓冲(Color Buffer)（储存所有的片段颜色：视觉输出）一样，在每个片段中储存了信息，并且（通常）和颜色缓冲有着一样的宽度和高度。深度缓冲是由窗口系统自动创建的，它会以16、24或32位float的形式储存它的深度值。在大部分的系统中，深度缓冲的精度都是24位的。

 当深度测试(Depth Testing)被启用的时候，OpenGL会将一个片段的深度值与深度缓冲的内容进行对比。OpenGL会执行一个深度测试，如果这个测试通过了的话，深度缓冲将会更新为新的深度值。如果深度测试失败了，片段将会被丢弃。

 深度缓冲是在片段着色器运行之后（以及模板测试(Stencil Testing)运行之后，我们将在下一节中讨论）在屏幕空间中运行的。屏幕空间坐标与通过OpenGL的glViewport所定义的视口密切相关，并且可以直接使用GLSL内建变量gl_FragCoord从片段着色器中直接访问。gl_FragCoord的x和y分量代表了片段的屏幕空间坐标（其中(0, 0)位于左下角）。gl_FragCoord中也包含了一个z分量，它包含了片段真正的深度值。z值就是需要与深度缓冲内容所对比的那个值。

 https://learnopengl-cn.github.io/04%20Advanced%20OpenGL/01%20Depth%20testing/
 */

out vec4 FragColor;

in vec2 TexCoords;

uniform sampler2D texture1;

void main()
{
    FragColor = texture(texture1, TexCoords);
    
//    FragColor = vec4(vec3(gl_FragCoord.z), 1.0);
}
