#version 330 core
layout (location = 0) in vec3 aPos;

out vec3 TexCoords;

uniform mat4 projection;
uniform mat4 view;

void main()
{
    TexCoords = aPos;
    
    // 把天空盒子当做世界，所以可以认为天空盒子本身就是在世界坐标里了，那么这里就不需要模型变换
    vec4 pos = projection * view * vec4(aPos, 1.0);
    
    /**
     先绘制木盒子（0.5 x 0.5 x 0.5的立方体），在绘制天空盒子（1 x 1 x 1的立方体），而摄像机是在 z 轴向外位移 3 的位置上，
     所以在摄像机的视角下，天空盒子是包裹木盒子的，所以天空盒子的 z 值是更小的，那么就会产生天空盒子所有片段都会测试通过，从而遮挡住了木盒子。
     
     想要不遮挡木盒子，可以欺骗深度缓冲，我们绘制的天空盒子的深度值就是 1.0 (最大值)，
     [ 在坐标系统小节中我们说过，透视除法是在顶点着色器运行之后执行的，将gl_Position的xyz坐标除以w分量。我们又从深度测试小节中知道，相除结果的z分量等于顶点的深度值。使用这些信息，我们可以将输出位置的z分量等于它的w分量，让z分量永远等于1.0，这样子的话，当透视除法执行之后，z分量会变为w / w = 1.0。
     最终的标准化设备坐标将永远会有一个等于1.0的z值：最大的深度值。结果就是天空盒只会在没有可见物体的地方渲染了（只有这样才能通过深度测试，其它所有的东西都在天空盒前面）。]
     
     所以在有木盒子的片段的位置上，就会深度测试失败，从而让木盒子可以被看到。
     
     此外我们我们还要改变一下深度函数，将它从默认的GL_LESS改为GL_LEQUAL。因为深度缓冲区默认值是 1.0，为了能让天空盒子自己的片段不被丢弃，就要确保天空盒子的 z 值是要小于等于深度缓冲中的值（默认值是1.0），也是由于天空盒子的片段总是能测试通过，所以无论我们怎么移动摄像机，都能看到天空，也制造一种我们总在世界里的感觉。
     
     
     // 清除深度缓冲区默认值为1.0
     glClear(GL_COLOR, BUFFER_BIT|GL_DEPTH_BUFFER_BIT);
     // draw skybox as last
     glDepthFunc(GL_LEQUAL);  // change depth function so depth test passes when values are equal to depth buffer's content
     */
    gl_Position = pos.xyww;
}
